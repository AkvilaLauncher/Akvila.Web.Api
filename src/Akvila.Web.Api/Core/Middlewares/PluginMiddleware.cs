using System.Diagnostics;
using System.Net;
using System.Reactive.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Loader;
using Akvila.Web.Api.Core.Services;
using Akvila.Web.Api.Dto.Messages;
using Akvila.Web.Api.EndpointSDK;
using FluentValidation.Results;
using AkvilaCore.Interfaces;
using Spectre.Console;

namespace Akvila.Web.Api.Core.Middlewares;

public class PluginMiddleware {
    private readonly RequestDelegate _next;
    private static AccessTokenService _accessTokenService;
    private static IAkvilaManager akvilaManager;

    public PluginMiddleware(RequestDelegate next, AccessTokenService accessTokenService, IAkvilaManager akvilaManager) {
        _next = next;
        _accessTokenService = accessTokenService;
        PluginMiddleware.akvilaManager = akvilaManager;
    }

    public async Task Invoke(HttpContext context) {
        try {
            var reference = await Process(context);

            if (reference is null) {
                return;
            }

            if (!context.Response.HasStarted) {
                await _next(context);
            }

            for (int i = 0; i < 10 && reference.IsAlive; i++) {
                GC.Collect();
                GC.WaitForPendingFinalizers();

                // Debug.WriteLine($"Clean GC: {i}/10");
            }
        } catch (Exception exeption) {
            Console.WriteLine(exeption);
            akvilaManager.BugTracker.CaptureException(exeption);
            await context.Response.WriteAsJsonAsync(ResponseMessage.Create([new ValidationFailure
            {
                ErrorMessage = exeption.Message,
            }], "The server accepted the request, but was unable to process it", HttpStatusCode.UnprocessableContent));
        }

        // Debug.WriteLine($"Unload successful: {!reference.IsAlive}");
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static async Task<WeakReference?> Process(HttpContext context) {
        var loadContext = new AssemblyLoadContext("AkvilaAssemblyResolver", true);

        if (string.IsNullOrEmpty(context.Request.Path.Value) || !context.Request.Path.Value.Contains("plugins"))
            return new WeakReference(loadContext);

        var directoryInfo = new DirectoryInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "plugins"));

        if (!directoryInfo.Exists) {
            directoryInfo.Create();
        }

        var plugins = directoryInfo.GetFiles("*.dll", SearchOption.AllDirectories);


        try {
            foreach (var plugin in plugins) {
                var assembly = loadContext.LoadFromAssemblyPath(plugin.FullName);

                foreach (var type in assembly.GetTypes()) {
                    if (!typeof(IPluginEndpoint).IsAssignableFrom(type)) continue;

                    var pathInfo = type.GetCustomAttribute<PathAttribute>();

                    if (pathInfo is not { Method: not null, Path: not null }
                        || !pathInfo.Method.Equals(context.Request.Method, StringComparison.OrdinalIgnoreCase)
                        || !pathInfo.Path.Equals(context.Request.Path, StringComparison.OrdinalIgnoreCase)) continue;

                    if (pathInfo.NeedAuth) {
                        var accessToken = context.Request.Headers.Authorization
                            .FirstOrDefault()
                            ?.Split("Bearer ")
                            .Last();

                        if (string.IsNullOrEmpty(accessToken) || !_accessTokenService.ValidateToken(accessToken)) {
                            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                            return null;
                        }
                    }

                    var endpoint = Activator.CreateInstance(type) as IPluginEndpoint;
                    await endpoint?.Execute(context, akvilaManager)!;
                }
            }
        }
        catch (Exception exception) {
            Console.WriteLine(exception);
        }
        finally {
            loadContext.Unload();
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        return new WeakReference(loadContext);
    }
}