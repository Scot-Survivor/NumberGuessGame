FROM mcr.microsoft.com/dotnet/runtime:5.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["NGGClient/NGGClient.csproj", "NGGClient/"]
RUN dotnet restore "NGGClient/NGGClient.csproj"
COPY . .
WORKDIR "/src/NGGClient"
RUN dotnet build "NGGClient.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "NGGClient.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "NGGClient.dll"]
