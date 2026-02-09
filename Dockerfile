# ---------- BUILD STAGE ----------
    FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
    WORKDIR /src
    
    COPY src/ ./src/
    
    RUN dotnet restore src/Ayurveda-AI-Backend.WebAPI/Ayurveda-AI-Backend.WebAPI.csproj
    
    RUN dotnet publish src/Ayurveda-AI-Backend.WebAPI/Ayurveda-AI-Backend.WebAPI.csproj \
        -c Release \
        -o /app/publish
    
    # ---------- RUNTIME STAGE ----------
    FROM mcr.microsoft.com/dotnet/aspnet:9.0
    WORKDIR /app
    
    COPY --from=build /app/publish .
    
    ENV ASPNETCORE_URLS=http://+:8080
    EXPOSE 8080
    
    ENTRYPOINT ["dotnet", "Ayurveda-AI-Backend.WebAPI.dll"]
    