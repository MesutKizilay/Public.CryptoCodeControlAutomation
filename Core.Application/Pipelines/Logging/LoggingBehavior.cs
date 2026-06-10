using Core.CrossCuttingConcerns.Logging;
using Core.CrossCuttingConcerns.Logging.SeriLog;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace Core.Application.Pipelines.Logging
{
    public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IBaseRequest, ILoggableRequest
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly LoggerServiceBase _loggerServiceBase;

        public LoggingBehavior(IHttpContextAccessor httpContextAccessor, LoggerServiceBase loggerServiceBase)
        {
            _httpContextAccessor = httpContextAccessor;
            _loggerServiceBase = loggerServiceBase;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            var result = await next();

            //List<LogParameter> parameters = new List<LogParameter>()
            //{
            //    new LogParameter(){Type = request.GetType().Name, Value = request}
            //};

            LogDetail logDetail = new LogDetail()
            {
                MethodName = /*next.Method.Name*/request.GetType().Name,
                LogMessage = request.LogMessage,

                Value = request,
                //LogParameters = parameters,
                User = _httpContextAccessor.HttpContext.User.Identity?.Name ?? "",
            };

            _loggerServiceBase.Info(JsonSerializer.Serialize(logDetail, new JsonSerializerOptions() { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping }));

            return result;
        }
    }
}