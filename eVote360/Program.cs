using eVote360.Core.Interfaces.Repositories;
using eVote360.Core.Interfaces.Services;
using eVote360.Infrastructure.Data;
using eVote360.Infrastructure.Repositories;
using eVote360.Infrastructure.Services;
using eVote360.Shared.Email;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// MVC
builder.Services.AddControllersWithViews();

// Persistencia
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

// Capa Shared: correo (SMTP real o archivos HTML en desarrollo)
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection(EmailSettings.SectionName));
builder.Services.AddScoped<IEmailService>(sp => new SmtpEmailService(
    sp.GetRequiredService<IOptions<EmailSettings>>(),
    sp.GetRequiredService<ILogger<SmtpEmailService>>(),
    builder.Environment.ContentRootPath));

// OCR (modelo en /tessdata, copiado a la salida en la compilación)
builder.Services.AddScoped<IOcrService>(sp => new OcrService(
    Path.Combine(AppContext.BaseDirectory, "tessdata"),
    sp.GetRequiredService<ILogger<OcrService>>()));

// Servicios de aplicación
builder.Services.AddScoped<IEleccionService, EleccionService>();
builder.Services.AddScoped<IVotacionService, VotacionService>();
builder.Services.AddScoped<ICiudadanoService, CiudadanoService>();
builder.Services.AddScoped<IPartidoPoliticoService, PartidoPoliticoService>();
builder.Services.AddScoped<IImageService, ImageService>();
builder.Services.AddScoped<IPuestoElectivoService, PuestoElectivoService>();
builder.Services.AddScoped<IUsuarioService, UsuarioService>();
builder.Services.AddScoped<IDirigentePoliticoService, DirigentePoliticoService>();
builder.Services.AddScoped<ICandidatoService, CandidatoService>();
builder.Services.AddScoped<IAlianzaPoliticaService, AlianzaPoliticaService>();
builder.Services.AddScoped<IAsignacionCandidatoPuestoService, AsignacionCandidatoPuestoService>();
builder.Services.AddScoped<IDirigenteService, DirigenteService>();
builder.Services.AddScoped<IAuthService, AuthService>();

// Autenticación por cookie para administradores y dirigentes
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
    });

// Sesión para el proceso del elector (documento validado, OCR, código y selecciones)
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Migraciones pendientes y administrador inicial
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await context.Database.MigrateAsync();
    await DbSeeder.SeedAdministradorAsync(context);

    // Datos de demostración (opcional): "SeedDemoData": true en appsettings o variable de entorno
    if (app.Configuration.GetValue<bool>("SeedDemoData"))
        await DemoDataSeeder.SeedAsync(context);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
