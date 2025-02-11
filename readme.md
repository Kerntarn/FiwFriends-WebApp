# Todo Before Running
### Install Necessary Tool
- `dotnet tool install --global dotnet-ef`
- `dotnet restore`

### Create DB and Add Migrations
- `docker-compose up -d`
- `dotnet ef migrations add InitialCreate`   to create migration for db
- `dotnet ef database update`                to apply migration to db

### Build & Run Project
- `dotnet build`
- `dotnet watch run`

### Drop db [Optional when There's conflict among migrations]
- `dotnet ef database drop --force`
- `dotnet ef migrations remove`
