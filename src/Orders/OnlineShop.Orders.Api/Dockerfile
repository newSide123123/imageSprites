FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["OnlineShop.Orders.Api.csproj", "/src/Orders/OnlineShop.Orders.Api/"]
RUN dotnet restore "/src/Orders/OnlineShop.Orders.Api/OnlineShop.Orders.Api.csproj"
WORKDIR "/src/Orders/OnlineShop.Orders.Api"
COPY . .
RUN dotnet build "OnlineShop.Orders.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "OnlineShop.Orders.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OnlineShop.Orders.Api.dll"]
