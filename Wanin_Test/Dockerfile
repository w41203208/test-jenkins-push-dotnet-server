#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 5000
EXPOSE 5001

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Wanin_Test/Wanin_Test.csproj", "Wanin_Test/"]
RUN dotnet restore "Wanin_Test/Wanin_Test.csproj"
COPY . .
WORKDIR "/src/Wanin_Test"
RUN dotnet build "Wanin_Test.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Wanin_Test.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Wanin_Test.dll"]