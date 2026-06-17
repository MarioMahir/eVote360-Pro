using Microsoft.EntityFrameworkCore;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// ==========================================
// 1. REGISTRO DE SERVICIOS (builder.Services)
// ==========================================

builder.Services.AddControllersWithViews();

// Configuración de la Base de Datos (SQL Server)
builder.Services.AddDbContext<eVote360.Infrastructure.Data.AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// BUSCADOR AUTOMÁTICO DE SERVICIOS: 
// Esto busca ICiudadanoService y su clase en todo el proyecto sin importar el namespace.
var coreAssembly = Assembly.Load("eVote360.Core");
var infraAssembly = Assembly.Load("eVote360.Infrastructure");

if (coreAssembly != null && infraAssembly != null)
{
    var iCiudadano = coreAssembly.GetTypes().FirstOrDefault(t => t.Name == "ICiudadanoService");
    var tCiudadano = infraAssembly.GetTypes().FirstOrDefault(t => t.Name == "CiudadanoService");

    if (iCiudadano != null && tCiudadano != null)
    {
        builder.Services.AddScoped(iCiudadano, tCiudadano);
    }
}

var app = builder.Build();

// ==========================================
// 2. CONFIGURACIÓN DEL PIPELINE (Middleware)
// ==========================================

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();