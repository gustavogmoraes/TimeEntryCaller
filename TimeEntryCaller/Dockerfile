FROM mcr.microsoft.com/dotnet/runtime:5.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["TimeEntryCaller/TimeEntryCaller.csproj", "TimeEntryCaller/"]
RUN dotnet restore "TimeEntryCaller/TimeEntryCaller.csproj"
COPY . .
WORKDIR "/src/TimeEntryCaller"
RUN dotnet build "TimeEntryCaller.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TimeEntryCaller.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TimeEntryCaller.dll"]
