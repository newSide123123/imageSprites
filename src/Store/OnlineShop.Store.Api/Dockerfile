FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["OnlineShop.Store.Api.csproj", "/src/Store/OnlineShop.Store.Api/"]
RUN dotnet restore "/src/Store/OnlineShop.Store.Api/OnlineShop.Store.Api.csproj"
WORKDIR "/src/Store/OnlineShop.Store.Api"
COPY . .
RUN dotnet build "OnlineShop.Store.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "OnlineShop.Store.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OnlineShop.Store.Api.dll"]
