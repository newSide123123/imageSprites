FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["src/Email/OnlineShop.Email.Console/OnlineShop.Email.Console.csproj", "src/Email/OnlineShop.Email.Console/"]
RUN dotnet restore "src/Email/OnlineShop.Email.Console/OnlineShop.Email.Console.csproj"
COPY . .
WORKDIR "/src/src/Email/OnlineShop.Email.Console"
RUN dotnet build "OnlineShop.Email.Console.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "OnlineShop.Email.Console.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OnlineShop.Email.Console.dll"]
