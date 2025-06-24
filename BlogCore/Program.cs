using BlogCore.AccesoDatos.Data.Inicializador;
using BlogCore.AccesoDatos.Data.Repository;
using BlogCore.AccesoDatos.Data.Repository.IRepository;
using BlogCore.Data;
using BlogCore.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// 🔌 DB Context
var connectionString = builder.Configuration.GetConnectionString("ConexionSQL") ?? throw new InvalidOperationException("Falta cadena de conexión");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// 🛡️ Identity Core SIN UI
builder.Services.AddIdentityCore<ApplicationUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddSignInManager()
.AddUserManager<UserManager<ApplicationUser>>()
.AddRoleManager<RoleManager<IdentityRole>>();

// 🧠 Repositorio y Siembra
builder.Services.AddScoped<IContenedorTrabajo, ContenedorTrabajo>();
builder.Services.AddScoped<IInicializadorBD, InicializadorBD>();

// 🍪 Política de cookies
builder.Services.Configure<CookiePolicyOptions>(options =>
{
    options.MinimumSameSitePolicy = SameSiteMode.Lax;
});

// 🔐 Keycloak OIDC
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = "oidc";
})
.AddCookie()
.AddOpenIdConnect("oidc", options =>
{
    var kc = builder.Configuration.GetSection("Keycloak");
    options.Authority = kc["Authority"];

    //Obtencion de endpoints
    options.MetadataAddress = $"{kc["Authority"]}/.well-known/openid-configuration";
    
    options.ClientId = kc["ClientId"];
    options.ClientSecret = kc["ClientSecret"];
    options.RequireHttpsMetadata = false;
    options.ResponseType = "code";
    options.SaveTokens = true;
    options.GetClaimsFromUserInfoEndpoint = true;

    //Volver al home despues de logout
    options.SignedOutRedirectUri = "/";

    options.Scope.Clear();
    options.Scope.Add("openid");
    options.Scope.Add("profile");
    options.Scope.Add("email");

    options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
    {
        NameClaimType = "preferred_username",
        RoleClaimType = ClaimTypes.Role
    };

    options.Events = new OpenIdConnectEvents
    {
        OnTokenValidated = async context =>
        {
            var identity = context.Principal.Identity as ClaimsIdentity;
            var realmAccess = identity.FindFirst("realm_access")?.Value;

            if (!string.IsNullOrEmpty(realmAccess))
            {
                var roles = JObject.Parse(realmAccess)["roles"]?.ToObject<List<string>>() ?? new List<string>();
                foreach (var role in roles)
                {
                    identity.AddClaim(new Claim(ClaimTypes.Role, role));
                    Console.WriteLine($"✅ Rol añadido como ClaimTypes.Role: {role}");
                }
            }

            var principal = new ClaimsPrincipal(identity);
            var props = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(1)
            };
            await context.HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                props);

            context.Principal = principal;
        }
    };
});

builder.Services.AddControllersWithViews();

var app = builder.Build();

// 🔧 Middleware
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseStaticFiles();
SiembraDatos();

app.UseRouting();
app.UseCookiePolicy();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{area=Cliente}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default_no_area",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

void SiembraDatos()
{
    using var scope = app.Services.CreateScope();
    var inicializadorBD = scope.ServiceProvider.GetRequiredService<IInicializadorBD>();
    inicializadorBD.Inicializar();
}
