FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER root
WORKDIR /app
EXPOSE 8080
EXPOSE 8081
RUN apt-get update && apt-get install -y git
USER $APP_UID

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/Akvila.Web.Api/Akvila.Web.Api.csproj", "src/Akvila.Web.Api/"]
COPY ["src/Akvila.Web.Api.Domains/Akvila.Web.Api.Domains.csproj", "src/Akvila.Web.Api.Domains/"]
COPY ["src/Akvila.Web.Api.Dto/Akvila.Web.Api.Dto.csproj", "src/Akvila.Web.Api.Dto/"]
COPY ["src/Akvila.Web.Api.EndpointSDK/Akvila.Web.Api.EndpointSDK.csproj", "src/Akvila.Web.Api.EndpointSDK/"]
COPY ["src/Akvila.Core/src/CmlLib.Core.Installer.Forge/CmlLib.Core.Installer.Forge/CmlLib.Core.Installer.Forge.csproj", "src/Akvila.Core/src/CmlLib.Core.Installer.Forge/CmlLib.Core.Installer.Forge/"]
RUN dotnet restore "src/Akvila.Web.Api/Akvila.Web.Api.csproj"
COPY . .
WORKDIR "/src/src/Akvila.Web.Api"
RUN dotnet build "Akvila.Web.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Akvila.Web.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Akvila.Web.Api.dll"]
