FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copiar csproj para aprovechar cache de capas en restore
COPY ERPNet.Domain/ERPNet.Domain.csproj             ERPNet.Domain/
COPY ERPNet.Application/ERPNet.Application.csproj   ERPNet.Application/
COPY ERPNet.Infrastructure/ERPNet.Infrastructure.csproj ERPNet.Infrastructure/
COPY ERPNet.Api/ERPNet.Api.csproj                   ERPNet.Api/
RUN dotnet restore ERPNet.Api/ERPNet.Api.csproj

# Copiar código fuente y publicar
COPY ERPNet.Domain/       ERPNet.Domain/
COPY ERPNet.Application/  ERPNet.Application/
COPY ERPNet.Infrastructure/ ERPNet.Infrastructure/
COPY ERPNet.Api/          ERPNet.Api/

# SkipApiDescription=true: omite Microsoft.Extensions.ApiDescription.Server
# (ejecuta la app para extraer el esquema OpenAPI — no funciona en build Docker)
RUN dotnet publish ERPNet.Api/ERPNet.Api.csproj \
    -c Release -o /app/publish \
    -p:SkipApiDescription=true

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "ERPNet.Api.dll"]
