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
            : base(message ?? "Validation failed for the given row")
        {
            ArgumentGuard.NotNull(validationResult, nameof(validationResult));

            ValidationError = validationResult;
        }

        public ValidationError ValidationError { get; set; }
    }

    //[Serializable]
    //public class ModelValidationResult
    //{
    //    public string Message { get; set; }

    //    public IList<ValidationError> Errors { get; set; }
    //}

    [Serializable]
    public class ValidationError
    {
        public string PropertyName { get; set; }

        public string ErrorMessage { get; set; }
    }

    [Serializable]
    public class MappingError
    {
        public int ColumnIndex { get; set; }

        public string ErrorMessage { get; set; }

        public string UnmappedRow { get; set; }
    }

}
