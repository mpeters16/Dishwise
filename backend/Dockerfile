# Stage 1: Build the application
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy project files for restore
COPY Dishwise.Api/Dishwise.Api.csproj Dishwise.Api/
COPY Dishwise.Application/Dishwise.Application.csproj Dishwise.Application/
COPY Dishwise.Domain/Dishwise.Domain.csproj Dishwise.Domain/
COPY Dishwise.Infrastructure/Dishwise.Infrastructure.csproj Dishwise.Infrastructure/

# Restore dependencies
RUN dotnet restore Dishwise.Api/Dishwise.Api.csproj

# Copy all source code
COPY . .

# Publish
RUN dotnet publish Dishwise.Api/Dishwise.Api.csproj -c Release -o /app/publish

# Stage 2: Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 10000
ENV ASPNETCORE_URLS=http://+:10000
ENV DOTNET_RUNNING_IN_CONTAINER=true

CMD ["dotnet", "Dishwise.Api.dll"]
