FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /source

# Copy everything from the Ayurveda-AI-Backend.WebAPI folder
COPY Ayurveda-AI-Backend.WebAPI/. .

# Restore and publish
RUN dotnet restore
RUN dotnet publish -c Release -o /app

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app .

ENV ASPNETCORE_URLS=http://0.0.0.0:${PORT:-5000}

ENTRYPOINT ["dotnet", "Ayurveda-AI-Backend.WebAPI.dll"]