# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution and csproj files
COPY FinalProjectMohamF.sln ./
COPY GraphQL/GraphQL.csproj GraphQL/
COPY ServerApp/ServerApp.csproj ServerApp/
COPY Data/Data.csproj Data/

# Restore dependencies
RUN dotnet restore FinalProjectMohamF.sln

# Copy everything else
COPY . .

# Build and publish ServerApp
WORKDIR /src/ServerApp
RUN dotnet publish -c Release -o /app

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["dotnet", "ServerApp.dll"]
