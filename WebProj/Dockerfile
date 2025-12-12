# Multi-stage Dockerfile for building and running the WebProj .NET app
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution and project files for restore
COPY ./WebProj.sln ./
COPY ./WebProj/WebProj.csproj ./WebProj/
COPY ./Application/Application.csproj ./Application/
COPY ./Domain/Domain.csproj ./Domain/
COPY ./Infrastructure/Infrastructure.csproj ./Infrastructure/

# Restore solution (restores all projects)
RUN dotnet restore "WebProj.sln"

# Copy remaining source
COPY . .

# Build and publish the WebProj project
RUN dotnet publish ./WebProj/WebProj.csproj -c Release -o /app/publish

# runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
ENV ASPNETCORE_URLS="http://+:80"
EXPOSE 80
COPY --from=build /app/publish ./
ENTRYPOINT ["dotnet", "WebProj.dll"]
