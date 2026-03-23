FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copiar csproj para aprovechar cache de capas en restore
COPY ERPNet.ApiClient/ERPNet.ApiClient.csproj \
     ERPNet.ApiClient/
COPY ERPNet.Web.Blazor/ERPNet.Web.Blazor.Client/ERPNet.Web.Blazor.Client.csproj \
     ERPNet.Web.Blazor/ERPNet.Web.Blazor.Client/
COPY ERPNet.Web.Blazor/ERPNet.Web.Blazor/ERPNet.Web.Blazor.csproj \
     ERPNet.Web.Blazor/ERPNet.Web.Blazor/
RUN dotnet restore ERPNet.Web.Blazor/ERPNet.Web.Blazor/ERPNet.Web.Blazor.csproj \
    -p:SkipNSwag=true

# Copiar código fuente y publicar
# SkipNSwag=true: omite la generación de NSwag/FilterExtensions.
# Los archivos Generated/ se copian desde el contexto de build (generados en CI).
COPY ERPNet.ApiClient/    ERPNet.ApiClient/
COPY ERPNet.Web.Blazor/   ERPNet.Web.Blazor/
RUN dotnet publish ERPNet.Web.Blazor/ERPNet.Web.Blazor/ERPNet.Web.Blazor.csproj \
    -c Release -o /app/publish \
    -p:SkipNSwag=true

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "ERPNet.Web.Blazor.dll"]
