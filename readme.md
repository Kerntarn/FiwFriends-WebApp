dotnet tool install --global dotnet-aspnet-codegenerator
dotnet tool install --global dotnet-ef

dotnet dev-certs https --trust

dotnet restore

dotnet build

dotnet watch run