using BusinessLogicLayer.Profiles;
using DataAccessLayer.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddOpenApi();

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
builder.Services.AddScoped<DataAccessLayer.Interfaces.IPuestosRepository,
                           DataAccessLayer.Repositories.PuestoRepository>();

builder.Services.AddScoped<BusinessLogicLayer.Interfaces.IPuestosManager, 
                           BusinessLogicLayer.Managers.PuestoManager>();

builder.Services.AddScoped<ApplicationLayer.Interfaces.IPuestoService, 
                           ApplicationLayer.Services.PuestoService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();