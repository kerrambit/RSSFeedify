# RSSFeedify
Simple RSS proxy server with a REST API for storing, uploading and updating RSS feeds.

## Setup

After you download/clone the repository with the solution, you need to run the following steps to build and run the API.

- First of all, we need to run the PostgreSQL database:

```
docker run --detach -p 5555:5432 --name rssfeedify-db -e POSTGRES_PASSWORD=password -e POSTGRES_USER=user -e POSTGRES_DB=database postgres:latest
```

- Redis database is also needed:

```
docker run --name rssfeedify-blacklist-db -p 6379:6379 -d redis
```

- You can verify Redis connection with (it should print out "PONG"):

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

- Now, we need to apply generated migration to the database. For that, change temporarily (you can copy the current state into the clipboard) the DefaultConnection entry in *appsettings.json* to this:
```
"Host=localhost; Port=5555; Database=database; Username=user; Password=password;"
```

- Open Package Manager Console and run the following command:
```
Update-Database
```

- After the migration is applied, change back the DefaultConnection entry in *appsettings.json*. Finally, you should be able to build the solution and run the application.

## Additional notes

If you want to inspect the data in the PostgreSQL database, you can follow this link: https://medium.com/@marvinjungre/get-postgresql-and-pgadmin-4-up-and-running-with-docker-4a8d81048aea.
For Redis, you can use application such as RedisInsight: https://redis.io/insight/.
