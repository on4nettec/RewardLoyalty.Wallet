# ساخت از ریشهٔ solution: از پوشه Services/wallet اجرا کنید:
# docker build -f Wallet.Core.Api/Dockerfile .

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Wallet.Core.Api/Wallet.Core.Api.csproj", "Wallet.Core.Api/"]
COPY ["Wallet.Core.Data/Wallet.Core.Data.csproj", "Wallet.Core.Data/"]
COPY ["Wallet.Core.Model/Wallet.Core.Model.csproj", "Wallet.Core.Model/"]
RUN dotnet restore "Wallet.Core.Api/Wallet.Core.Api.csproj"
COPY . .
WORKDIR "/src/Wallet.Core.Api"
RUN dotnet build "Wallet.Core.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Wallet.Core.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Wallet.Core.Api.dll"]
