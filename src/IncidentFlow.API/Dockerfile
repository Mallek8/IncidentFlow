FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY IncidentFlow.sln ./
COPY src/IncidentFlow.Domain/IncidentFlow.Domain.csproj src/IncidentFlow.Domain/
COPY src/IncidentFlow.Application/IncidentFlow.Application.csproj src/IncidentFlow.Application/
COPY src/IncidentFlow.Infrastructure/IncidentFlow.Infrastructure.csproj src/IncidentFlow.Infrastructure/
COPY src/IncidentFlow.API/IncidentFlow.API.csproj src/IncidentFlow.API/
RUN dotnet restore src/IncidentFlow.API/IncidentFlow.API.csproj
COPY src/ ./src/
RUN dotnet publish src/IncidentFlow.API/IncidentFlow.API.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "IncidentFlow.API.dll"]
