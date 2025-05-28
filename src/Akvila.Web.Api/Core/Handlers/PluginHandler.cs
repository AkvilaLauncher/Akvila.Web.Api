using System.Diagnostics.Tracing;
using System.IO.Compression;
using System.Net;
using Akvila.Web.Api.Domains.Plugins;
using Akvila.Web.Api.Dto.Messages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Akvila.Web.Api.Core.Handlers;

public abstract class PluginHandler : IPluginHandler {
    public static Task<IResult> RemovePlugin(string name, string version) {
        var pluginPath = new DirectoryInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "plugins"));

        var file = pluginPath.GetFiles($"{name}.dll", SearchOption.AllDirectories)
            .FirstOrDefault(c => c.Directory!.Name == version);

        if (file?.Exists == true) {
            try {
                file.Delete();
            }
            catch (Exception exception) {
                Console.WriteLine(exception);
                return Task.FromResult(Results.BadRequest(ResponseMessage.Create(
                    $"There was an error during uninstallation. The plugin was not deleted.", HttpStatusCode.BadRequest)));
            }
        }

        return Task.FromResult(Results.Ok(ResponseMessage.Create("The plugin has been successfully removed", HttpStatusCode.OK)));
    }

    public static Task<IResult> GetInstalledPlugins() {
        var pluginsDirectory = new DirectoryInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "plugins"));

        var plugins = pluginsDirectory.GetFiles("*.dll", SearchOption.AllDirectories);

        var pluginsDto = plugins.Select(c => new PluginVersionReadDto {
            Name = c.Name.Replace(Path.GetExtension(c.Name), string.Empty),
            Version = c.Directory!.Name
        });

        return Task.FromResult(Results.Ok(ResponseMessage.Create(pluginsDto, string.Empty, HttpStatusCode.OK)));
    }

    public static async Task<IResult> InstallPlugin(HttpContext context) {
        var pluginFormData = new {
            Url = context.Request.Form["pluginUrl"]
        };

        if (string.IsNullOrEmpty(pluginFormData.Url)) {
            return Results.BadRequest(ResponseMessage.Create("Plugin address is not specified", HttpStatusCode.BadRequest));
        }

        var pluginsDirectory = new DirectoryInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "plugins"));

        if (!pluginsDirectory.Exists) {
            pluginsDirectory.Create();
        }

        using (var httpClient = new HttpClient()) {
            var response = await httpClient.GetAsync(pluginFormData.Url);

            var contentDisposition = response.Content.Headers.ContentDisposition;
            string? fileName = contentDisposition?.FileName?.Trim('\"');

            if (string.IsNullOrEmpty(fileName)) {
                return Results.BadRequest(ResponseMessage.Create("The plugin name had an invalid format",
                    HttpStatusCode.BadRequest));
            }

            var pluginPath = Path.Combine(pluginsDirectory.FullName, fileName);

            using (var contentStream = await response.Content.ReadAsStreamAsync())
                using (Stream fileStream = new FileStream(pluginPath, FileMode.Create,
                           FileAccess.Write, FileShare.None, 8192, true)) {
                    await contentStream.CopyToAsync(fileStream);
                }

            ExtractPlugin(pluginsDirectory.FullName, pluginPath);
        }

        return Results.Ok(ResponseMessage.Create("The plugin was successfully installed", HttpStatusCode.OK));
    }

    private static void ExtractPlugin(string pluginsDirectory, string zipPath) {
        var pluginNameAndVersion = Path.GetFileNameWithoutExtension(zipPath).Substring("plugin-".Length);

        var versionStartIndex = pluginNameAndVersion.IndexOf("-v", StringComparison.Ordinal) + 2;
        var pluginName = pluginNameAndVersion.Substring(0, versionStartIndex - 2);
        var pluginVersion = pluginNameAndVersion.Substring(versionStartIndex);

        var extractPath = Path.Combine(pluginsDirectory, pluginName, $"v{pluginVersion}");

        if (!Directory.Exists(extractPath)) {
            Directory.CreateDirectory(extractPath);
        }

        ZipFile.ExtractToDirectory(zipPath, extractPath, true);
        File.Delete(zipPath);
    }
}