FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy the solution and restore first
COPY ["tsg-zelis-okta-MohamedFarrag.sln", "./"]
COPY ["GraphQL/GraphQL.csproj", "GraphQL/"]
RUN dotnet restore "GraphQL/GraphQL.csproj"

# Copy everything else
COPY . .
WORKDIR "/src/GraphQL"
RUN dotnet build -c Release -o /app/build
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "GraphQL.dll"]
