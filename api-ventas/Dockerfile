# API de Ventas — imagen de dos etapas (compilación y ejecución)
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY WeCorp.Ventas.Api/WeCorp.Ventas.Api.csproj WeCorp.Ventas.Api/
RUN dotnet restore WeCorp.Ventas.Api/WeCorp.Ventas.Api.csproj
COPY WeCorp.Ventas.Api/ WeCorp.Ventas.Api/
RUN dotnet publish WeCorp.Ventas.Api/WeCorp.Ventas.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "WeCorp.Ventas.Api.dll"]
