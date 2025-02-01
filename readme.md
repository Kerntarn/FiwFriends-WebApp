## Run App (Required .NET 9)
- dotnet tool install --global dotnet-aspnet-codegenerator
- dotnet tool install --global dotnet-ef
- dotnet restore
- dotnet build
- dotnet watch run

## Create db
- docker-compose up -d
- dotnet ef migrations add InitialCreate   to create migration for db
- dotnet ef migrations update              to apply migration to db

## Drop db
- dotnet ef database drop --force
- dotnet ef migrations remove
