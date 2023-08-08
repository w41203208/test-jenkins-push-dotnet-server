#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0.404 AS build
WORKDIR /src
COPY ["GameTestServer/GameTestServer.csproj", "GameTestServer/"]
RUN dotnet restore "GameTestServer/GameTestServer.csproj"
COPY . .
WORKDIR "/src/GameTestServer"
RUN dotnet build "GameTestServer.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "GameTestServer.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY ./testcert.pfx /app/testcert.pfx
COPY ./cert.pem /app/cert.pem
COPY ./openssl.cnf /etc/ssl/openssl.cnf
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "GameTestServer.dll"]