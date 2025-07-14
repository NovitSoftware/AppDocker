using AcademiaNovit;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

builder.Host.UseSerilog((context, loggerConfiguration) => loggerConfiguration
    .ReadFrom.Configuration(context.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console());

builder.Configuration.AddEnvironmentVariables();

// El connection string se puede obtener de 2 formas (en orden de prioridad):
// 1. Docker secret file (SEGURO - no visible en env vars)
// 2. appsettings.json (fallback para desarrollo local)

string connectionString;
var secretFilePath = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING_FILE");

if (!string.IsNullOrEmpty(secretFilePath) && File.Exists(secretFilePath))
{
    // Leer desde Docker secret
    connectionString = File.ReadAllText(secretFilePath).Trim();
    Console.WriteLine($"Connection string loaded from Docker secret: {secretFilePath}");
}
else
{
    // Fallback a appsettings.json (desarrollo local)
    connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
    Console.WriteLine("Connection string loaded from appsettings.json (local development)");
}

builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.Migrate();
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
