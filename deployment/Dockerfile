FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
ARG APP_VERSION
RUN test -n "${APP_VERSION}" && dotnet publish GramParserWorkbench/GramParserWorkbench.csproj \
    --configuration Release \
    --output /app/publish \
    /p:UseAppHost=false \
    "/p:Version=${APP_VERSION}" \
    "/p:InformationalVersion=${APP_VERSION}" \
    /p:IncludeSourceRevisionInInformationalVersion=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "GramParserWorkbench.dll"]
