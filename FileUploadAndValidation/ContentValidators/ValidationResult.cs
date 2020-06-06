using System.Collections.Generic;
using System.Linq;
using FileUploadAndValidation;

namespace FileUploadApi.Services
{
    public class ValidationResult<T> where T : ValidatedRow
    {
        public ValidationResult()
        {
            ValidRows = new List<T>();
            Failures = new List<T>();
        }
        public List<T> ValidRows { get; set; }

        public List<T> Failures { get; set; }
        public ValidationResult<T> MergeResults(ValidationResult<T> otherResult)
        {
            //local validations has all rows: failed and valid
            //remote validations has valid rows from previous step
            //merge result should update the status on local validation

            //update the valid records with data from remote
            foreach (var row in otherResult.ValidRows)
            {
                var originalRow = this.ValidRows.FirstOrDefault(r => r.Row == row.Row);
                if (originalRow != null)
                {
                    //set the Extra data from remote
                }
            }

            //update the status of earlier valid records. they are failed by remote validation
            foreach (var row in otherResult.Failures)
            {
                var originalRow = this.ValidRows.FirstOrDefault(r => r.Row == row.Row);
                if (originalRow != null){
                    originalRow.IsValid = false;
                    originalRow.ErrorMessages = row.ErrorMessages;
                }
            }

            return this;
        }
    }
}