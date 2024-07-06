FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["OnlineShop.EntityHistory.Console.csproj", "/src/EntityHistory/OnlineShop.EntityHistory.Console/"]
RUN dotnet restore "/src/EntityHistory/OnlineShop.EntityHistory.Console/OnlineShop.EntityHistory.Console.csproj"
WORKDIR "/src/EntityHistory/OnlineShop.EntityHistory.Console"
COPY . .
RUN dotnet build "OnlineShop.EntityHistory.Console.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "OnlineShop.EntityHistory.Console.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OnlineShop.EntityHistory.Console.dll"]
