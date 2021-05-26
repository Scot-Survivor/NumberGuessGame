FROM mcr.microsoft.com/dotnet/runtime:5.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["NGGServer/NGGServer.csproj", "NGGServer/"]
RUN dotnet restore "NGGServer/NGGServer.csproj"
COPY . .
WORKDIR "/src/NGGServer"
RUN dotnet build "NGGServer.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "NGGServer.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "NGGServer.dll"]
