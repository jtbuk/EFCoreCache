using Jtbuk.EFCoreCache;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<WeatherContext>(o => o.UseInMemoryDatabase("InMemory"));
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSingleton<ICacheService, DistributedCacheService>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApiDocument(o =>
{
    o.Title = "Cache Example";            
});

var app = builder.Build();

app.UseOpenApi();

app.UseSwaggerUi3(o =>
{
    o.DocExpansion = "list";
});

var cacheKey = "api:weather:";

app.MapGet("api/weather/{id}", async (WeatherContext context, ICacheService cacheService, [FromRoute] int id) =>
{
    await context.WeatherRecords
        .Select(w => new GetWeatherDto(w.Id, w.Temperature, w.TimeStamp))
        .TryCacheSingleOrDefaultAsync(cacheService, $"{cacheKey}:{id}");
});

app.MapPost("api/weather", async (WeatherContext context, [FromBody] CreateWeatherDto dto) =>
{
    var record = new WeatherRecord
    {
        Temperature = dto.Temperature,
        TimeStamp = DateTime.UtcNow
    };

    context.Add(record);
    await context.SaveChangesAsync();

    return record.Id;
});

app.MapPut("api/weather/{id}", async (WeatherContext context, ICacheService cacheService, [FromRoute] int id, [FromBody] UpdateWeatherDto dto) =>
{
    var record = await context.WeatherRecords        
        .SingleOrDefaultAsync(w => w.Id == id);

    if (record == null) throw new Exception("Not found");
    
    record.Temperature = dto.Temperature;
    record.TimeStamp = DateTime.UtcNow;

    await context.SaveChangesAsync();
    await cacheService.Remove($"{cacheKey}:{id}");
});

app.Run();
