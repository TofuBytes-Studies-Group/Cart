# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app
COPY *.sln ./
COPY Cart.API/*.csproj Cart.API/
COPY Cart.Domain/*.csproj Cart.Domain/
COPY Cart.Infrastructure/*.csproj Cart.Infrastructure/
COPY API.Tests/*.csproj API.Tests/
COPY Domain.UnitTests/*.csproj Domain.UnitTests/

RUN dotnet restore 

COPY . ./

RUN dotnet publish Cart.API/Cart.API.csproj -c Release -o /app/out

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

COPY --from=build /app/out .

EXPOSE 80

ENTRYPOINT ["dotnet", "Cart.API.dll"]