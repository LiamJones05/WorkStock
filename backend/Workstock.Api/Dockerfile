FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY Workstock.Api/Workstock.Api.csproj Workstock.Api/
RUN dotnet restore Workstock.Api/Workstock.Api.csproj
COPY . .
RUN dotnet publish Workstock.Api/Workstock.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
RUN mkdir -p /data/uploads && chown -R $APP_UID /data
USER $APP_UID
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "Workstock.Api.dll"]
