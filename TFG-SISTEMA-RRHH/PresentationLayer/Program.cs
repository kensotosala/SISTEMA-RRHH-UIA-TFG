using BusinessLogicLayer.Profiles;
using DataAccessLayer.Data;
using Microsoft.EntityFrameworkCore;
using PresentationLayer.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddSwaggerGen();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                               ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<SistemaRhContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

builder.Services.AddLogging();

builder.Services.AddAutoMapper(
    cfg =>
    {
        cfg.AddProfile(new MappingProfile());
    });

// Registrar dependencias
builder.Services.Dependencies();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseExceptionHandler("/error");
app.UseAuthorization();
app.MapControllers();

app.Run();