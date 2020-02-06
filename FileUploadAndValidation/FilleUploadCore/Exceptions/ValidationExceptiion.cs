using FilleUploadCore.Helpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace FilleUploadCore.Exceptions
{

    [Serializable]
    public class ValidationException : AppException
    {
        public ValidationException(ValidationError validationResult, string message = null)
            : base(message ?? "Validation failed")
        {
            ArgumentGuard.NotNull(validationResult, nameof(validationResult));

            ValidationError = validationResult;
        }

        public ValidationException(string message)
            : base(message ?? "Validation failed")
        {
        }

        public ValidationError ValidationError { get; set; }
    }

    [Serializable]
    public class ValidationError
    {
        public string PropertyName { get; set; }

        public string ErrorMessage { get; set; }
    }
    
}
