# Build Stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /FiwFriends
EXPOSE 80
EXPOSE 443

# Publish Stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

# Copy the project file and restore dependencies
COPY ["FiwFriends.csproj", "./"]
RUN dotnet restore "FiwFriends.csproj"

# Copy only the source code (without bin/ or obj/ directories)
COPY . ./

# Publish the application
RUN dotnet publish "FiwFriends.csproj" -c Release -o /FiwFriends/publish

# Final Stage
FROM base AS final
COPY --from=build /FiwFriends/publish /FiwFriends

ENTRYPOINT ["dotnet", "FiwFriends.dll"]