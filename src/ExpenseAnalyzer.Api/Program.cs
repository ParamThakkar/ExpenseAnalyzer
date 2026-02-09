using Asp.Versioning;
using Asp.Versioning.ApiExplorer;

using ExpenseAnalyzer.Api.Endpoints;
using ExpenseAnalyzer.Infra;
using ExpenseAnalyzer.Infra.Repos;

using Microsoft.OpenApi.Models;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddInfra(builder.Configuration);

// Configure API Versioning
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = ApiVersionReader.Combine(
        new UrlSegmentApiVersionReader(),
        new HeaderApiVersionReader("X-Api-Version"),
        new QueryStringApiVersionReader("api-version"));
})
.AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

// Configure Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "ExpenseAnalyzer API",
        Version = "v1",
        Description = "API for managing expenses, accounts, categories, and tags"
    });

    options.SwaggerDoc("v2", new OpenApiInfo
    {
        Title = "ExpenseAnalyzer API",
        Version = "v2",
        Description = "API for managing expenses, accounts, categories, and tags (Version 2)"
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint(
            $"/swagger/v1/swagger.json",
            $"ExpenseAnalyzer API v1");

        options.SwaggerEndpoint(
            $"/swagger/v2/swagger.json",
            $"ExpenseAnalyzer API v2");

    });
}

app.UseHttpsRedirection();

// Create API version sets
var apiVersionSet = app.NewApiVersionSet()
    .HasApiVersion(new ApiVersion(1, 0))
    .HasApiVersion(new ApiVersion(2, 0))
    .ReportApiVersions()
    .Build();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

// Version 1.0 endpoint
app.MapGet("/api/v{version:apiVersion}/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return Results.Ok(forecast);
})
.WithApiVersionSet(apiVersionSet)
.MapToApiVersion(new ApiVersion(2, 0))
.WithName("GetWeatherForecastV2")
.WithTags("WeatherForecast");

app.MapGet("/api/v{version:apiVersion}/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return Results.Ok(forecast);
})
.WithApiVersionSet(apiVersionSet)
.MapToApiVersion(new ApiVersion(1, 0))
.WithName("GetWeatherForecastV1")
.WithTags("WeatherForecast");

app.MapAccountEndpoints(apiVersionSet);
app.MapIncomeEndpoints(apiVersionSet);

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

record WeatherForecastV2(DateOnly Date, int TemperatureC, string? Summary, int Humidity, int WindSpeed)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
