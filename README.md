# RSSFeedify - version 1.2.0
Simple RSS proxy server with a REST API for storing, uploading and updating RSS feeds with easy-to-use CLI client.

## Setup - API

Clone the repository.

``` 
git clone https://github.com/kerrambit/RSSFeedify.git && cd RSSFeedify/RSSFeedify
```

- Server is almost ready for the production, you only need to set environment variables (via *.env* file) before running docker compose file.
- You can use *.env.production* file and rename it to *.env*. Inside the file, set RSSFEEDIFY_JWT_KEY to your secret key (use long strings, something like *ThisIsSecretJWTKeyThisIsSecretJWTKeyThisIsSecretJWTKeyThisIsSecretJWTKeyThisIsSecretJWTKeyThisIsSecretJWTKeyThisIsSecretJWTKeyThisIsSecretJWTKey*), you can also set other variables if you wish.
- Finally, run the docker compose file.

```
 docker-compose up
```
- And you are done, you can inspect that containers are running by the following command.

```
docker ps
```

## Setup - CLI client
- **RSSFeedify version 1.2.0** offers also CLI client. The easist way to run it is via Visual Studio. Open the whole solution and run (preferably in *Release* mode) the RSSFeedifyCLIClient project.

## Setup - For developers
- The following information can be handy for developers. You might have noticed also another two environment files, one in *RSSFeedify/RSSFeedify/RSSFeedify* (we will refer to it simply as *RSSFeedify* project) and in *RSSFeedify/RSSFeedify/RSSFeedifyCLIClient* (*RSSFeedifyCLIClient* project). The first mentioned is used when you run API via Visual Studio (no docker compose file), basically it holds very similiar data as the *.env* file for production. The last *.env* can be used to set URL inside the CLI client (development API runs under HTTPS - different URL needed than with docker compose file).
- Without docker compose, you have to set up containers yourself, too.

```
docker run --detach -p 5555:5432 --name rssfeedify-db -e POSTGRES_PASSWORD=password -e POSTGRES_USER=user -e POSTGRES_DB=database postgres:latest
```

```
docker run --name rssfeedify-blacklist-db -p 6379:6379 -d redis
```

- You can verify Redis connection with the following command (it should print out "PONG").

```
docker run -it --rm redis redis-cli -h host.docker.internal -p 6378 ping
```
 
- Make sure that **Host** in DefaulConnection entry in *appsettings.json* is the same address as the one you get by the following command:

```
docker inspect -f '{{range .NetworkSettings.Networks}}{{.IPAddress}}{{end}}' rssfeedify-db
```

- The same applies also for entry called "RedisConnection":

```
docker inspect -f '{{range .NetworkSettings.Networks}}{{.IPAddress}}{{end}}' rssfeedify-blacklist-db
```
- **! Note: the following paragraphs about migration should not be needed anymore. We keep it here for backward compatibility. !**
- Now, we need to apply generated migration to the database. For that, change temporarily (you can copy the current state into the clipboard) the DefaultConnection entry in *appsettings.json* to this:
```
"Host=localhost; Port=5555; Database=database; Username=user; Password=password;"
```

- Open Package Manager Console and run the following command:
```
Update-Database
```

- After the migration is applied, change back the DefaultConnection entry in *appsettings.json*. Finally, you should be able to build the solution and run the application.

## Setup - Additional notes

If you want to inspect the data in the PostgreSQL database, you can follow this link: https://medium.com/@marvinjungre/get-postgresql-and-pgadmin-4-up-and-running-with-docker-4a8d81048aea.
For Redis, you can use application such as RedisInsight: https://redis.io/insight/.

## Version history

- **1.0.0** - First official release of the RSSFeedify API. This version introduced integration with PostgreSQL for managing RSS feeds, a polling service to regularly update items/news, and a set of basic API endpoints for creating, editing, and deleting RSS feeds. The polling service automatically fetched new updates at defined intervals. Additionally, CLI tools were provided for seamless interaction with the API.

- **1.1.0** - This version brought authentication and authorization support via JWT. Three new endpoints were added to allow user registration, login, and logout, significantly improving security. The entire API underwent thorough testing, and multiple bugs were resolved, ensuring smooth operation. Also, all API responses were standardized with unified warning and error messages. One major improvement was the introduction of a Docker Compose file, making deployment much simpler for end users—requiring just a single command to run the entire API setup.

- **1.2.0** - Implemented logging enhancements across various components, enabling better monitoring and troubleshooting. Fixed multiple bugs, including a critical race condition issue during the creation of RSS feeds, ensuring more reliable operation under concurrent scenarios. Integrated a continuous integration and deployment (CI/CD) pipeline, automating the build and deployment process. Added basic unit tests to improve test coverage and ensure code stability. Introduced a new RSSFeedifyCommon project to centralize common functionalities. Updated to CommandParsonaut version 1.3.0, improving command parsing capabilities. Various other improvements and optimizations across the project.
