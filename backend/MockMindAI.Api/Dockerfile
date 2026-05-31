FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY backend/MockMindAI.Api/MockMindAI.Api.csproj backend/MockMindAI.Api/
RUN dotnet restore backend/MockMindAI.Api/MockMindAI.Api.csproj

COPY backend/MockMindAI.Api/ backend/MockMindAI.Api/
RUN dotnet publish backend/MockMindAI.Api/MockMindAI.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .
EXPOSE 8080

ENTRYPOINT ["sh", "-c", "dotnet MockMindAI.Api.dll --urls http://0.0.0.0:${PORT:-8080}"]
