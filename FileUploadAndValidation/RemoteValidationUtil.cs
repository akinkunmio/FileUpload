using System;
using System.Collections.Generic;
using FileUploadAndValidation.Helpers;
using FileUploadApi.Services;

namespace FileUploadAndValidation
{
    public static class RemoteValidationUtil
    {
        public static ValidationResult<T> HandleFailureResponse<T>(string responseCode) where T : ValidatedRow
        {
            string errorMessage = "";
            CompletionStateStatus status = CompletionStateStatus.Failed;

            switch(responseCode) {
                case "400":
                case "401":
                case "404":
                    errorMessage = $"{responseCode}: Unexpected error occurred and we are working to restore. Please try again later"; 
                    status = CompletionStateStatus.Aborted;
                    break;
                case "403":
                    errorMessage = "Forbidden: You do not have the permission to perform this request. Please contact your administrator";
                    status = CompletionStateStatus.Failed;
                    break;
                case "500":
                case "502":
                case "503":
                    status = CompletionStateStatus.Aborted;
                    errorMessage = $"{responseCode}: Unexpected error occurred and we are working to restore. Please try again later"; 
                    break;
                default: 
                    status = CompletionStateStatus.Aborted;
                    errorMessage = $"{responseCode}: Unexpected error occurred and we are working to restore. Please try again later"; 
                    break;
            }

            return new ValidationResult<T>
            {
                CompletionStatus = new CompletionState {
                    Status = status,
                    ErrorMessage = errorMessage
                },
                ValidRows = new List<T>(),
                Failures = new List<T>()
            };
        }

        public static string GetStatusFromRemoteResponseCode(CompletionStateStatus status)
        {
            if (status == CompletionStateStatus.Aborted)
                return GenericConstants.PendingValidation;

            if (status == CompletionStateStatus.Queued)
                return "Validation In Progress";

            if (status == CompletionStateStatus.Failed)
                return "Failed";

            return "Failed";
        }
    }
}
