using FilleUploadCore.UploadManagers;
using System.Collections.Generic;

namespace FileUploadAndValidation
{
    internal class UploadServiceFactory
    {
        private Dictionary<string, UploadServiceBase> services = new Dictionary<string, UploadServiceBase>();

        internal UploadServiceBase FindOrDefault(string fileContentType)
        {
            if (services.TryGetValue(fileContentType, out UploadServiceBase service))
                return service;

            return null;
        }

        internal void Register(string name, UploadServiceBase service)
        {
            services.Add(name, service);
        }
    }
}
