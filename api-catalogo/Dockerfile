# API de Catálogo e Identidad — imagen de ejecución
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY WeCorp.Catalogo.Api/WeCorp.Catalogo.Api.csproj WeCorp.Catalogo.Api/
RUN dotnet restore WeCorp.Catalogo.Api/WeCorp.Catalogo.Api.csproj
COPY WeCorp.Catalogo.Api/ WeCorp.Catalogo.Api/
RUN dotnet publish WeCorp.Catalogo.Api/WeCorp.Catalogo.Api.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app .
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "WeCorp.Catalogo.Api.dll"]
