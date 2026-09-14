using AgenciaViagens.Application.Services;
using AgenciaViagens.Infrastructure.Data;
using AgenciaViagens.Infrastructure.Identity;
using AgenciaViagens.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;


var builder = WebApplication.CreateBuilder(args);

// Base de dados
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity com cookies (não JWT — a Web usa sessões)
builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
{
    options.Password.RequiredLength = 8;
    options.Password.RequireDigit = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedEmail = false;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

// Configuração dos cookies
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Conta/Login";
    options.LogoutPath = "/Conta/Logout";
    options.AccessDeniedPath = "/Conta/AcessoNegado";
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;
});

// Serviços de aplicação
builder.Services.AddScoped<PacoteService>();
builder.Services.AddScoped<ReservaService>();
builder.Services.AddScoped<ItinerarioService>();
builder.Services.AddScoped<AvaliacaoService>();
builder.Services.AddScoped<PagamentoService>();
builder.Services.AddScoped<FaturaService>();
builder.Services.AddScoped<EstatisticasService>();
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<PdfService>();
builder.Services.AddScoped<XmlExportService>();

var app = builder.Build();

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

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();