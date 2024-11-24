# Use the .NET SDK image to build the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Publish
COPY . .
COPY --from=domain . ../Domain
COPY --from=application . ../Application
COPY --from=infrastructure . ../Infrastructure

RUN dotnet publish -c Release -o publish /p:UseAppHost=false

# Use the .NET runtime image to run the application
FROM mcr.microsoft.com/dotnet/runtime:8.0 AS final
WORKDIR /app

# Copy the published application from the build stage
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "OcrWorker.dll"]