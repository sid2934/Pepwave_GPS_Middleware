#Prior to running create an appsettings.json `docker build` first run `dotnet publish -c Release`

FROM mcr.microsoft.com/dotnet/sdk:5.0

EXPOSE 8888/tcp

COPY ./bin/Release/net5.0/publish/ App/
WORKDIR /App
ENV DOTNET_EnableDiagnostics=0
ENTRYPOINT ["dotnet", "Pepwave_Gps_Middleware.dll"]
