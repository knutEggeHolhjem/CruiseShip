# -------- Stage 1: Build and publish with AOT --------
    FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
    WORKDIR /src
    
    # Copy everything and restore
    COPY . ./
    RUN dotnet restore
    
    # Publish with trimming, and invariant globalization
    RUN dotnet publish -c Release \
        -r linux-x64 \
        /p:InvariantGlobalization=true \
        -o /app/out
    
    # -------- Stage 2: Runtime (chiseled) --------
    FROM mcr.microsoft.com/dotnet/runtime:8.0 AS final
    WORKDIR /app
    
    # Declare volume mount point
    VOLUME ["/app/data"]

    # Copy the native binary
    COPY --from=build /app/out ./
    
    # Use native executable as entrypoint (no dotnet needed!)
    ENTRYPOINT ["/app/SensorPOC"]
    