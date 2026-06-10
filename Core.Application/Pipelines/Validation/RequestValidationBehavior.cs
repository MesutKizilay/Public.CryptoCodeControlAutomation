using Core.CrossCuttingConcerns.Exceptions.Types;
using FluentValidation;
using MediatR;
using ValidationException = Core.CrossCuttingConcerns.Exceptions.Types.ValidationException;

namespace Core.Application.Pipelines.Validation
{
    public class RequestValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IBaseRequest
    {
        private readonly IEnumerable<IValidator<TRequest>> _validators;

        public RequestValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
        {
            _validators = validators;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            ValidationContext<object> context = new(request);

            IEnumerable<ValidationExceptionModel> errors = _validators.Select(validator => validator.Validate(context))
                                                                      .SelectMany(result => result.Errors)
                                                                      .Where(failure => failure != null)
                                                                      .GroupBy(
                                                                          keySelector: (p) => {
                                                                              //if (p.FormattedMessagePlaceholderValues != null &&
                                                                              //    p.FormattedMessagePlaceholderValues.TryGetValue("PropertyName", out var val) &&
                                                                              //    val is not null)
                                                                              //    return val.ToString()!;

                                                                              return p.PropertyName;
                                                                          },
                                                                          resultSelector: (propertyName, errors) =>
                                                                              new ValidationExceptionModel
                                                                              {
                                                                                  Property = propertyName,
                                                                                  Errors99 = errors.Select(e => e.ErrorMessage)
                                                                              }
                                                                      ).ToList();

            if (errors.Any())
                throw new ValidationException(errors);

            TResponse response = await next();
            return response;
        }
    }
}