# ---- build ----
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore src/Sirh.Api/Sirh.Api.csproj
RUN dotnet publish src/Sirh.Api/Sirh.Api.csproj -c Release -o /app/publish --no-restore /p:UseAppHost=false

# ---- exécution (utilisateur non-root) ----
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
USER $APP_UID
EXPOSE 8080
ENTRYPOINT ["dotnet", "Sirh.Api.dll"]
