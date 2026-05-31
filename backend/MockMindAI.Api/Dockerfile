FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY MockMindAI.Api.csproj .
RUN dotnet restore MockMindAI.Api.csproj

COPY . .
RUN dotnet publish MockMindAI.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .
EXPOSE 8080

ENTRYPOINT ["sh", "-c", "dotnet MockMindAI.Api.dll --urls http://0.0.0.0:${PORT:-8080}"]
