FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src

# Install the dotnet-ef tool and update the PATH environment variable
RUN dotnet tool install --global dotnet-ef
ENV PATH="${PATH}:/root/.dotnet/tools"

COPY ["OnlineShop.Users.Api.csproj", "/src/Users/OnlineShop.Users.Api/"]
RUN dotnet restore "/src/Users/OnlineShop.Users.Api/OnlineShop.Users.Api.csproj"
WORKDIR "/src/Users/OnlineShop.Users.Api"
COPY . .
RUN dotnet build "OnlineShop.Users.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "OnlineShop.Users.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OnlineShop.Users.Api.dll"]
