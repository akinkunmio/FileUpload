using System.Collections.Generic;

namespace FileUploadApi.Services
{
    public class ValidationResult<T>
    {
        public ValidationResult()
        {
            ValidRows = new List<T>();
            Failures = new List<T>();
        }
        public IList<T> ValidRows { get; set; }

        public IList<T> Failures { get; set; }
    }
}