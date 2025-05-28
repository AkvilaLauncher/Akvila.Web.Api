using System.Net;
using Akvila.Web.Api.Dto.Messages;

namespace Akvila.Web.Api.Core.Middlewares;

public class BadRequestExceptionMiddleware(RequestDelegate next) {
    public async Task Invoke(HttpContext context) {
        try {
            await next(context);
        } catch (BadHttpRequestException badHttpRequestException) when (badHttpRequestException.Message.StartsWith(
                                                                           "Implicit body inferred for parameter")) {
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;

            await context.Response.WriteAsJsonAsync(ResponseMessage.Create("The request body cannot be empty",
                HttpStatusCode.BadRequest));
        } catch (BadHttpRequestException exception) {
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;

            await context.Response.WriteAsJsonAsync(ResponseMessage.Create(exception.Message,
                HttpStatusCode.BadRequest));
        } catch (IOException ioException) {
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            Console.WriteLine(ioException);
            await context.Response.WriteAsJsonAsync(ResponseMessage.Create(
                "An error occurred while working with the file system. Try restarting the service to restore operation",
                HttpStatusCode.InternalServerError));
        }
    }
}