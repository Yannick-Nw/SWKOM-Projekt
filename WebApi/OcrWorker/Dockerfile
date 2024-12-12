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

# Tesseract und Dependencies installieren
RUN apt-get update && apt-get install -y --no-install-recommends \
    libleptonica-dev \
    libtesseract-dev \
    ghostscript \
    tesseract-ocr \
    tesseract-ocr-eng \
    && rm -rf /var/lib/apt/lists/*

WORKDIR /app/x64

RUN ln -s /usr/lib/x86_64-linux-gnu/libdl.so.2 /usr/lib/x86_64-linux-gnu/libdl.so && \
    ln -s /usr/lib/x86_64-linux-gnu/liblept.so.5 /app/x64/libleptonica-1.82.0.so && \
    ln -s /usr/lib/x86_64-linux-gnu/libtesseract.so.5 /app/x64/libtesseract50.so

WORKDIR /app

# Copy the published application from the build stage
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "OcrWorker.dll"]