# RSSFeedify
Simple RSS proxy server with a REST API for storing, uploading and updating RSS feeds.

## Setup

After you download/clone the repository with the solution, you need to run the following steps to build and run the API.

- First of all, we need to run the database:

```
docker run --detach -p 5555:5432 --name rssfeedify-db -e POSTGRES_PASSWORD=password -e POSTGRES_USER=user -e POSTGRES_DB=database postgres:latest
```
 
- Make sure that **Host** in DefaulConnection entry in *appsettings.json* is the same adress as the one you get by the following command:

```
docker inspect -f '{{range .NetworkSettings.Networks}}{{.IPAddress}}{{end}}' rssfeedify-db
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

If you want to inspect the data in the database, you can follow this link: https://medium.com/@marvinjungre/get-postgresql-and-pgadmin-4-up-and-running-with-docker-4a8d81048aea.
