using EntityFramework;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("MY_DB")
                        ?? throw new Exception("Connection string not found");

builder.Services.AddDbContext<MasterContext>(options => options.UseSqlServer(connectionString));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();



app.MapGet("/api/devices", async (MasterContext context, CancellationToken cancellationToken) =>
{
    try
    {
        return Results.Ok(await context.Devices.ToListAsync(cancellationToken));
    }
    catch (Exception e)
    {
        Console.WriteLine(e);
        throw;
    }
});
    

app.Run();
