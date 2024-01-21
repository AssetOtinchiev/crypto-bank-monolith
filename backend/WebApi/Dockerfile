FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base

WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build

WORKDIR /src
COPY ./WebApi/WebApi.csproj ./WebApi/
RUN dotnet restore ./WebApi/WebApi.csproj

COPY . .
RUN dotnet build ./WebApi/WebApi.csproj -c Release -o /app/build

RUN dotnet publish ./WebApi/WebApi.csproj -c Release -o /app/publish

FROM base AS final

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "WebApi.dll"]
