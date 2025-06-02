using Microsoft.EntityFrameworkCore;
using DeviceAPI;
using FluentValidation;
using DeviceAPI.DTO;
using DeviceAPI.Validators;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("MyDatabase")
                       ?? throw new Exception("Connection string not found");

builder.Services.AddDbContext<MasterContext>(options => options.UseSqlServer(connectionString));
builder.Services.AddScoped<IValidator<DeviceDTO>, DeviceDTOValidator>();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();