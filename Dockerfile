FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["src/Core/Marketplace.Domain/Marketplace.Domain.csproj", "src/Core/Marketplace.Domain/"]
COPY ["src/Core/Marketplace.Application/Marketplace.Application.csproj", "src/Core/Marketplace.Application/"]
COPY ["src/Infrastructure/Marketplace.Infrastructure/Marketplace.Infrastructure.csproj", "src/Infrastructure/Marketplace.Infrastructure/"]
COPY ["src/Infrastructure/Marketplace.Identity/Marketplace.Identity.csproj", "src/Infrastructure/Marketplace.Identity/"]
COPY ["src/Infrastructure/Marketplace.Scraping/Marketplace.Scraping.csproj", "src/Infrastructure/Marketplace.Scraping/"]
COPY ["src/SharedKernel/Marketplace.Shared/Marketplace.Shared.csproj", "src/SharedKernel/Marketplace.Shared/"]
COPY ["src/Presentation/Marketplace.API/Marketplace.API.csproj", "src/Presentation/Marketplace.API/"]

RUN dotnet restore "src/Presentation/Marketplace.API/Marketplace.API.csproj"

COPY . .
WORKDIR "/src/src/Presentation/Marketplace.API"
RUN dotnet build "Marketplace.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Marketplace.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Marketplace.API.dll"]
