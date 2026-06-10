using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.CrossCuttingConcerns.Exceptions.Types
{
    public class ValidationException : Exception
    {
        public IEnumerable<ValidationExceptionModel> Errors88 { get; }

        public ValidationException() : base()
        {
            Errors88 = Array.Empty<ValidationExceptionModel>();
        }

        public ValidationException(string? message) : base(message)
        {
            Errors88 = Array.Empty<ValidationExceptionModel>();
        }

        public ValidationException(string? message, Exception? innerException) : base(message, innerException)
        {
            Errors88 = Array.Empty<ValidationExceptionModel>();
        }

        public ValidationException(IEnumerable<ValidationExceptionModel> errors) : base(BuildErrorMessage(errors))
        {
            Errors88 = errors;
        }

        private static string BuildErrorMessage(IEnumerable<ValidationExceptionModel> errors)
        {
            IEnumerable<string> arr = errors.Select(
                x => $"{Environment.NewLine} -- {x.Property}: {string.Join(Environment.NewLine, values: x.Errors99 ?? Array.Empty<string>())}"
            );
            return $"Validation failed: {string.Join(string.Empty, arr)}";
        }
    }

    public class ValidationExceptionModel
    {
        public string? Property { get; set; }
        public IEnumerable<string>? Errors99 { get; set; }
    }
}