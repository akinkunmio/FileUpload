using Autofac;
using FileUploadAndValidation.DTOs;
using FileUploadAndValidation.FileDataExtractor;
using FileUploadApi.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace FileUploadAndValidation.Bootstrap
{
    public class AutofacModule : Module
    {
        protected override void Load(ContainerBuilder containerBuilder)
        {
            var assembly = GetType().Assembly;

            containerBuilder.RegisterType<FirsWhtDataExtractor>()
                .As<IDataExtractor<FirsWhtTransferModel>>()
                .InstancePerLifetimeScope();

            containerBuilder.RegisterType<FirsWhtFileUploadService>()
               .As<IFileUploadService<FirsWhtUploadResult>>()
               .InstancePerLifetimeScope();

        }
    }
}
