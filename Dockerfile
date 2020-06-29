FROM mcr.microsoft.com/dotnet/core/aspnet:2.2

MAINTAINER Akinkunmi_Okunola <akinkunmi.okunola@interswitchgroup.com>

# Set OS time zone
ENV TZ Africa/Lagos

ENV ASPNETCORE_ENVIRONMENT Production

WORKDIR /FileUploadApi

COPY FileUploadApi/out .

ENV ASPNETCORE_URLS http://*:5000

EXPOSE 5000

ENTRYPOINT ["dotnet", "FileUploadApi.dll"]


