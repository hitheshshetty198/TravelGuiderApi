FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["TravelGuiderAPI.csproj", "./"]
RUN dotnet restore "TravelBackend.csproj"
COPY . .
RUN dotnet publish "TravelGuiderAPI.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "TravelGuiderAPI.dll"]
