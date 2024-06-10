using BlogCore.AccesoDatos.Data.Inicializador;
using BlogCore.AccesoDatos.Data.Repository;
using BlogCore.AccesoDatos.Data.Repository.IRepository;
using BlogCore.Data;
using BlogCore.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("ConexionSQL") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultUI();
    
builder.Services.AddControllersWithViews();


//Agregar contenedor de trabajo al contenedor IoC de inyecci�n de dependencias
builder.Services.AddScoped<IContenedorTrabajo, ContenedorTrabajo>();

//Siembra de datos
builder.Services.AddScoped<IInicializadorBD, InicializadorBD>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();

SiembraDatos();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{area=Cliente}/{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();

//Funcionalidad metodo siembra de datos
void SiembraDatos()
{
    using(var scope = app.Services.CreateScope())
    {
        var inicializadorBD = scope.ServiceProvider.GetRequiredService<IInicializadorBD>();
        inicializadorBD.Inicializar();
    }
}
