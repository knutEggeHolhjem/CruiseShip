FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY . ./
RUN dotnet restore

RUN dotnet publish -c Release -o /app/out

FROM mcr.microsoft.com/dotnet/runtime:8.0 AS final
WORKDIR /app

VOLUME ["/app/data"]

COPY --from=build /app/out ./

ENTRYPOINT ["/app/SensorPOC"]
