using System.Net;

using Api;
using Api.Models;
using Api.Utils;

using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddLogging();
builder.Services.AddHealthChecks();


builder.Services.AddCors(options => options.AddPolicy("AllowFrontend",
        policy => policy.WithOrigins("http://localhost:5173")
                        .AllowAnyHeader()
                        .AllowAnyMethod()));

var app = builder.Build();


app.UseCors("AllowFrontend");
app.UseHttpsRedirection();
app.UseHealthChecks("/health");

app.MapPost("/calculate/{algorithm}",
    (string algorithm,
    IFormFile file,
    [FromForm] int initialEnergy,
    [FromForm] int capacity,
    [FromForm] int power,
    [FromForm] int intervals) =>
{
    var lines = ExcelUtils.Read(file.OpenReadStream());

    if (lines == null || lines.Count == 0)
    {
        return Results.BadRequest("No data provided.");
    }

    var entryData = new EntryData(
        InitialEnergy: initialEnergy,
        Capacity: capacity,
        MaximumPower: power,
        Intervals: intervals,
        Lines: lines);

    (TransactionResult?, TransactionResult?) result = algorithm switch
    {
        "interval" => (Algorithm.CalculateOptimalInterval(entryData), null),
        "oneCycle" => (Algorithm.CalculateOneCycle(entryData), null),
        "twoCycles" => Algorithm.CalculateTwoCycles(entryData),
        _ => (null, null)
    };

    var r = new Response
    (
        FirstCycle: result.Item1,
        SecondCycle: result.Item2,
        Lines: lines
    );

    return r.FirstCycle == null && r.SecondCycle == null
        ? Results.BadRequest("Invalid algorithm specified.")
        : Results.Ok(r);
})
.DisableAntiforgery();

app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (Exception ex)
    {
        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Unhandled exception");

        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
        context.Response.ContentType = "application/problem+json";

        var problem = new ProblemDetails
        {
            Status = context.Response.StatusCode,
            Title = "An unexpected error occurred",
            Detail = ex.Message,
            Instance = context.Request.Path
        };

        await context.Response.WriteAsJsonAsync(problem);
    }
});

app.Run();
