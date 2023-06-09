# AspNetMicroservices-StepByStep

[[_TOC_]]

RUN 
```docker-compose -f .\docker-compose.yml -f .\docker-compose.override.yml up -d```

IN CASE OF CHANGES
```docker-compose -f .\docker-compose.yml -f .\docker-compose.override.yml up --build -d```


![Visual Studio - Docker Compose Configuration](./img/visualstudio-dockerconfiguration.png)

## TROUBLESHOOTING

### Docker Desktop out of memory issue

Docker Desktop needs at least 4 GB to run all microservices correctly.

Configure the resource in the app's Settings menu, or if you're using WSL2, add a .wslconfig file in C:\Users\<UserName>\.

```
# Settings apply across all Linux distros running on WSL 2
[wsl2]

# Limits VM memory to use no more than 4 GB, this can be set as whole numbers using GB or MB
memory=4GB 
```

## SOME CHANGES

### StackExchange.Redis.RedisServerException: ERR unknown command 'EVAL'

Using StackExchange.Redis as proposed in the course, I had error on UpdateBasket method:

```
StackExchange.Redis.RedisServerException: ERR unknown command 'EVAL'
   at Microsoft.Extensions.Caching.StackExchangeRedis.RedisCache.SetAsync(String key, Byte[] value, DistributedCacheEntryOptions options, CancellationToken token)
   at Basket.API.Repositories.BasketRepository.UpdateBasket(ShoppingCart basket) in C:\Users\caio.cruz\Pessoal\Projetos\AspnetMicroservices\src\Services\Basket\Basket.API\Repositories\BasketRepository.cs:line 30
```

To solve this, as a workaround, just to proceed with the course, I used another library to Redis connection: ServiceStack.Redis

### Environmet variables declaration on docker-compose.override.yml doesn't work properly

Instead of:
```
catalog.api:
    container_name: catalog.api
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - "DatabaseSettings:ConnectionString=mongodb://catalogdb:27017"
    depends_on:
      - catalogdb
    ports:
      - "8000:80"
```

Try: 
```
catalog.api:
    container_name: catalog.api
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - DatabaseSettings__ConnectionString=mongodb://catalogdb:27017
    depends_on:
      - catalogdb
    ports:
      - "8000:80"
```

By using __ (double underscore) in the environment variable names, Docker Compose will properly override the corresponding values in the appsettings.json file.

## NEXT STEPS

### Add OpenTelemetry (and do some refactorings)

Reference:
https://github.com/mansoorafzal/AspnetMicroservices/blob/main/src/BuildingBlocks/Common.Policy/PollyPolicy.cs

### Add basic Blazor SPA Web Application

Reference:
https://github.com/thanhxuanhd/AspnetMicroservices/tree/main/aspnetrun-microservices/WebApps/AspnetRunBasicBlazor

## Complementary material
- https://devblogs.microsoft.com/dotnet/monitoring-and-observability-in-cloud-native-asp-net-core-apps/
- https://devblogs.microsoft.com/dotnet/improvements-in-net-core-3-0-for-troubleshooting-and-monitoring-distributed-apps/
- https://github.com/aspnetrun/run-devops
- https://github.com/aspnetrun/run-aspnet-identityserver4
