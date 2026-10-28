# Stage 1: Build the application
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy project files for restore
COPY RecipeManager.Api/RecipeManager.Api.csproj RecipeManager.Api/
COPY RecipeManager.Application/RecipeManager.Application.csproj RecipeManager.Application/
COPY RecipeManager.Domain/RecipeManager.Domain.csproj RecipeManager.Domain/
COPY RecipeManager.Infrastructure/RecipeManager.Infrastructure.csproj RecipeManager.Infrastructure/

# Restore dependencies
RUN dotnet restore RecipeManager.Api/RecipeManager.Api.csproj

# Copy all source code
COPY . .

# Publish
RUN dotnet publish RecipeManager.Api/RecipeManager.Api.csproj -c Release -o /app/publish

# Stage 2: Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 10000
ENV ASPNETCORE_URLS=http://+:10000
ENV DOTNET_RUNNING_IN_CONTAINER=true

CMD ["dotnet", "RecipeManager.Api.dll"]
