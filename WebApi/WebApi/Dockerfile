# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Publish
COPY ./Application ./Application 
COPY ./Domain ./Domain
COPY ./Infrastructure ./Infrastructure
COPY ./WebApi ./WebApi
RUN dotnet publish WebApi -c Release -o publish /p:UseAppHost=false

# This stage is used to run the service project
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

COPY --from=build app/publish .

EXPOSE 8080
ENTRYPOINT ["dotnet", "WebApi.dll"]