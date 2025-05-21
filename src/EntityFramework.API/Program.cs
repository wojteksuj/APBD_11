using System.Text.Json;
using EntityFramework;
using Microsoft.EntityFrameworkCore;
using EntityFramework.DTO;
using EntityFramework.Validators;
using FluentValidation;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("MY_DB")
                        ?? throw new Exception("Connection string not found");

builder.Services.AddDbContext<MasterContext>(options => options.UseSqlServer(connectionString));
builder.Services.AddScoped<IValidator<DeviceDTO>, DeviceDTOValidator>();

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
        var devices = await context.Devices
            .Select(d => new { d.Id, d.Name })
            .ToListAsync(cancellationToken);

        return Results.Ok(devices);
    }
    catch (Exception ex)
    {
        return Results.Problem("Failed to retrieve devices.");
    }
});

app.MapGet("/api/devices/{id}", async (int id, MasterContext context, CancellationToken cancellationToken) =>
{
    try
    {
        var device = await context.Devices
            .Include(d => d.DeviceType)
            .Include(d => d.DeviceEmployees.OrderByDescending(de => de.IssueDate))
                .ThenInclude(de => de.Employee)
                    .ThenInclude(e => e.Person)
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);

        if (device == null)
            return Results.NotFound();

        var lastUser = device.DeviceEmployees.FirstOrDefault(de => de.ReturnDate == null);

        return Results.Ok(new
        {
            device.Name,
            DeviceTypeName = device.DeviceType?.Name,
            device.IsEnabled,
            AdditionalProperties = JsonSerializer.Deserialize<object>(device.AdditionalProperties),
            Employee = lastUser != null ? new
            {
                lastUser.Employee.Id,
                Name = $"{lastUser.Employee.Person.FirstName} {lastUser.Employee.Person.LastName}"
            } : null
        });
    }
    catch (Exception ex)
    {
        return Results.Problem("Failed to retrieve device.");
    }
});

app.MapPost("/api/devices", async (DeviceDTO dto, IValidator<DeviceDTO> validator, MasterContext context, CancellationToken cancellationToken) =>
{
    var result = await validator.ValidateAsync(dto, cancellationToken);
    if (!result.IsValid)
    {
        var errors = result.Errors.Select(e => new
        {
            e.PropertyName,
            e.ErrorMessage
        });

        return Results.BadRequest(new { Errors = errors });
    }

    var deviceType = await context.DeviceTypes
        .FirstOrDefaultAsync(dt => dt.Name == dto.DeviceTypeName, cancellationToken);

    if (deviceType == null)
    {
        return Results.BadRequest($"Device type '{dto.DeviceTypeName}' not found.");
    }

    var device = new Device
    {
        Name = dto.Name,
        IsEnabled = dto.IsEnabled.Value,
        AdditionalProperties = JsonSerializer.Serialize(dto.AdditionalProperties),
        DeviceTypeId = deviceType.Id
    };

    context.Devices.Add(device);
    await context.SaveChangesAsync(cancellationToken);

    return Results.Created($"/api/devices/{device.Id}", new { device.Id });
});

app.MapPut("/api/devices/{id}", async (int id, DeviceDTO dto, MasterContext context, CancellationToken cancellationToken) =>
{
    try
    {
        var device = await context.Devices.FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
        if (device == null)
            return Results.NotFound();

        var deviceType = await context.DeviceTypes
            .FirstOrDefaultAsync(dt => dt.Name == dto.DeviceTypeName, cancellationToken);

        if (deviceType == null)
            return Results.BadRequest($"Device type '{dto.DeviceTypeName}' not found.");

        device.Name = dto.Name;
        device.IsEnabled = dto.IsEnabled.Value;
        device.AdditionalProperties = JsonSerializer.Serialize(dto.AdditionalProperties);
        device.DeviceTypeId = deviceType.Id;

        await context.SaveChangesAsync(cancellationToken);
        return Results.Ok(new { device.Id });
    }
    catch (Exception ex)
    {
        return Results.Problem("Failed to update device.");
    }
});

app.MapDelete("/api/devices/{id}", async (int id, MasterContext context, CancellationToken cancellationToken) =>
{
    try
    {
        var device = await context.Devices.FindAsync(new object[] { id }, cancellationToken);
        if (device == null)
            return Results.NotFound();

        context.Devices.Remove(device);
        await context.SaveChangesAsync(cancellationToken);
        return Results.NoContent();
    }
    catch (Exception ex)
    {
        return Results.Problem("Failed to delete device.");
    }
});


app.MapGet("/api/employees", async (MasterContext context, CancellationToken cancellationToken) =>
{
    try
    {
        var employees = await context.Employees
            .Include(e => e.Person)
            .Select(e => new
            {
                e.Id,
                FullName = $"{e.Person.FirstName} {e.Person.LastName}"
            })
            .ToListAsync(cancellationToken);

        return Results.Ok(employees);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[GET /api/employees] {ex.Message}");
        return Results.Problem("Failed to retrieve employees.");
    }
});

app.MapGet("/api/employees/{id}", async (int id, MasterContext context, CancellationToken cancellationToken) =>
{
    try
    {
        var employee = await context.Employees
            .Include(e => e.Person)
            .Include(e => e.Position)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

        if (employee == null)
            return Results.NotFound();

        return Results.Ok(new
        {
            Person = new
            {
                employee.Person.FirstName,
                employee.Person.MiddleName,
                employee.Person.LastName,
                employee.Person.Email,
                employee.Person.PhoneNumber,
                employee.Person.PassportNumber
            },
            employee.Salary,
            Position = new { employee.Position.Id, employee.Position.Name },
            employee.HireDate
        });
    }
    catch (Exception ex)
    {
        return Results.Problem("Failed to retrieve employee.");
    }
});

    

app.Run();
