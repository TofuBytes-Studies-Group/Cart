# Stage 1: Build the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy solution and projects files
COPY *.sln ./

COPY Cart.API/Cart.API.csproj Cart.API/
COPY Cart.Domain/Cart.Domain.csproj Cart.Domain/
COPY Cart.Infrastructure/Cart.Infrastructure.csproj Cart.Infrastructure/
COPY Domain.UnitTests/Domain.UnitTests.csproj Domain.UnitTests/
COPY API.Tests/API.Tests.csproj API.Tests/
COPY Infrastructure.Tests/Infrastructure.Tests.csproj Infrastructure.Tests/

# Restore dependencies
RUN dotnet restore 

# Copy the remaining files and build the application
COPY . ./
RUN dotnet publish Cart.API/Cart.API.csproj -c Release -o /app/out

# Use the official .NET runtime image to run the application
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

COPY --from=build /app/out .

EXPOSE 80

# Set the entry point for the container
ENTRYPOINT ["dotnet", "Cart.API.dll"]