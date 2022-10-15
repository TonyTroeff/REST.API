# REST.API

In this repoitory you can find the final version of the REST.API template discussed at the seminar ["RESTful Web API development with ASP.NET Core"](https://softuni.bg/trainings/3947/restful-web-api-development-with-asp-net-core).

## Setup

The easiest way to setup this project is to use Docker. There is a "docker_componse.yml" file at the root level where all required dependencies are enlisted and pre-configured (as this is the recommended approach, all parameters in the "appsettings.json" and "appsettings.Development.json" files are pre-defined).

All you have to do is execute a single command at the root level:

> docker compose up -d

Having a [Postman](https://www.postman.com) account would come you in handy as there is a global [workspace](https://www.postman.com/galactic-eclipse-520741/workspace/rest-api) where you shall find all requests you may ask for.


## Database management

> dotnet ef migrations add <migration_name> -s .\API\API.csproj -p .\Data.PostgreSql\Data.PostgreSql.csproj -- <connection_string>
> dotnet ef database update -s .\API\API.csproj -p .\Data.PostgreSql\Data.PostgreSql.csproj -- <connection_string>