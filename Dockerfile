# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY WhoAmI/WhoAmI.csproj WhoAmI/
RUN dotnet restore "WhoAmI/WhoAmI.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/WhoAmI"
RUN dotnet build "WhoAmI.csproj" -c Release -o /app/build
RUN dotnet publish "WhoAmI.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
EXPOSE 8080

# Copy published app
COPY --from=build /app/publish .

# Set environment variable for ASP.NET Core
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "WhoAmI.dll"]
