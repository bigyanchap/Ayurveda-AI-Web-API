FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /source

# Copy the .csproj from the subdirectory
COPY Ayurveda-AI-Backend.WebAPI/*.csproj ./Ayurveda-AI-Backend.WebAPI/
WORKDIR /source/Ayurveda-AI-Backend.WebAPI
RUN dotnet restore

# Copy everything and build
COPY Ayurveda-AI-Backend.WebAPI/. .
RUN dotnet publish -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app .

ENV ASPNETCORE_URLS=http://0.0.0.0:${PORT:-5000}

ENTRYPOINT ["dotnet", "Ayurveda-AI-Backend.WebAPI.dll"]