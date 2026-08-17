using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Pos.Data;
using Pos.Helpers;
using Pos.Repositories;
using Pos.Repositories.Interfaces;
using Pos.Services;
using Pos.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Configurar DbContext
builder.Services.AddDbContext<SeguridadContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Seguridad")));

// Configurar Autenticación con Cookies
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
        options.SlidingExpiration = true;
        options.Cookie.Name = "POSAuthCookie";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    });

// Registrar servicios
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<ILoginService, LoginService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<DbInitializer>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<TokenHelper>();

// Configurar Email Settings - CORREGIDO
var emailSettings = builder.Configuration.GetSection("EmailSettings").Get<EmailSettings>();
if (emailSettings != null)
{
    builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
}

// Configurar Session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();
app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

// Inicializar base de datos (con manejo de errores)
using (var scope = app.Services.CreateScope())
{
    try
    {
        var dbInitializer = scope.ServiceProvider.GetRequiredService<DbInitializer>();
        await dbInitializer.InitializeAsync();
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError($"Error al inicializar la base de datos: {ex.Message}");
    }
}

app.Run();