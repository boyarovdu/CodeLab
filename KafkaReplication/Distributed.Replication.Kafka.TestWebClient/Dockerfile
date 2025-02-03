FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Distributed.Replication.Kafka.TestWebClient/Distributed.Replication.Kafka.TestWebClient.csproj", "Distributed.Replication.Kafka.TestWebClient/"]
RUN dotnet restore "Distributed.Replication.Kafka.TestWebClient/Distributed.Replication.Kafka.TestWebClient.csproj"
COPY . .
WORKDIR "/src/Distributed.Replication.Kafka.TestWebClient"
RUN dotnet build "Distributed.Replication.Kafka.TestWebClient.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Distributed.Replication.Kafka.TestWebClient.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
#ENTRYPOINT ["dotnet", "Distributed.Replication.Kafka.TestWebClient.dll"]
