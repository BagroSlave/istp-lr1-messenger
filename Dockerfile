# Збірка
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY nuget.config ./
COPY app/ ./app/
RUN dotnet restore app/app.csproj
RUN dotnet publish app/app.csproj -c Release -o /out /p:UseAppHost=false

# Запуск
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /out ./
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "app.dll"]
