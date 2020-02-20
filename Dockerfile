FROM mcr.microsoft.com/dotnet/core/aspnet:2.2

MAINTAINER Akinkunmi_Okunola <akinkunmi.okunola@interswitchgroup.com>

ENV ASPNETCORE_ENVIRONMENT Production

WORKDIR /FileUploadAndValidation/FileUploadApi

COPY FileUploadApi/out .

ENV ASPNETCORE_URLS http://*:5000

EXPOSE 5000

ENTRYPOINT ["dotnet", "FileUploadApi.dll"]

