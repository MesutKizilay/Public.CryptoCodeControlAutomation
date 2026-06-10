using Azure.Core;
using Core.CrossCuttingConcerns.Exceptions.Handlers;
using Core.CrossCuttingConcerns.Logging;
using Core.CrossCuttingConcerns.Logging.SeriLog;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
using System.Reflection;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace Core.CrossCuttingConcerns.Exceptions
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly HttpExceptionHandler _httpExceptionHandler;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly LoggerServiceBase _loggerServiceBase;

        public ExceptionMiddleware(RequestDelegate next, IHttpContextAccessor httpContextAccessor, LoggerServiceBase loggerServiceBase)
        {
            _next = next;
            _httpExceptionHandler = new HttpExceptionHandler();
            _httpContextAccessor = httpContextAccessor;
            _loggerServiceBase = loggerServiceBase;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception exception)
            {
                await LogException(context, exception);
                await HandleExceptionAsync(context.Response, exception);
            }
        }

        private Task HandleExceptionAsync(HttpResponse response, Exception exception)
        {
            response.ContentType = "application/json";
            _httpExceptionHandler.Response = response;
            return _httpExceptionHandler.HandleExceptionAsync(exception);
        }

        private Task LogException(HttpContext context, Exception exception)
        {
            //List<LogParameter> logParameters = [new LogParameter { Type = context.GetType().Name, Value = exception.ToString() }];

            //LogDetail logDetail2 =
            //    new()
            //    {
            //        MethodName = _next.Method.Name,
            //        Parameters = logParameters,
            //        User = _contextAccessor.HttpContext?.User.Identity?.Name ?? "?"
            //    };

            LogDetailWithException logDetail = new LogDetailWithException()
            {
                //MethodName = _next.Method.Name,
                MethodName = context.Request.Path,
                ExceptionMessage = exception.Message,

                //Value = context,
                //LogParameters = parameters,
                User = _httpContextAccessor.HttpContext.User.Identity?.Name ?? "",
            };

            _loggerServiceBase.Error(JsonSerializer.Serialize(logDetail,new JsonSerializerOptions() { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping }));
            return Task.CompletedTask;
        }
    }
}