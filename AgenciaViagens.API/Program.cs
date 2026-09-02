using AgenciaViagens.Application.Services;
using AgenciaViagens.Infrastructure.Data;
using AgenciaViagens.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;


var builder = WebApplication.CreateBuilder(args);

// 1. Configurar DbContext para ligar ao SQL Server
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 1b. Configurar ASP.NET Core Identity com Roles
builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
{
    options.Password.RequiredLength = 8;
    options.Password.RequireDigit = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedEmail = false;   // pôr a true depois do EmailService
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

// 2. Registrar os Serviços de Aplicação (Injeção de Dependência)
builder.Services.AddScoped<ReservaService>();
builder.Services.AddScoped<PacoteService>();
builder.Services.AddScoped<AvaliacaoService>();
builder.Services.AddScoped<ItinerarioService>();

// 3. Adicionar CORS (Essencial para permitir pedidos do .NET MAUI e Web)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// 4. Adicionar Controllers com tratamento de referências circulares
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

builder.Services.AddEndpointsApiExplorer();

// 5. Configurar Swagger com mapeamento seguro de Schemas e Tags
builder.Services.AddSwaggerGen(c =>
{
    c.CustomSchemaIds(type => type.Name);

    c.TagActionsBy(api =>
    {
        if (api.GroupName != null)
            return new[] { api.GroupName };

        if (api.ActionDescriptor.RouteValues.TryGetValue("controller", out var controllerName) && !string.IsNullOrEmpty(controllerName))
            return new[] { controllerName };

        return new[] { "Outros" };
    });
});

var app = builder.Build();

// 5b. Seed dos roles e utilizadores iniciais
using (var scope = app.Services.CreateScope())
{
    try
    {
        await DbSeeder.SeedAsync(scope.ServiceProvider);
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Erro ao popular a base de dados com roles e utilizadores iniciais.");
    }
}

// 6. Configurar a pipeline HTTP e ativar o Swagger
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "AgenciaViagens API v1");
    c.RoutePrefix = "swagger";
});

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseAuthentication();   // ← tem de vir ANTES de UseAuthorization
app.UseAuthorization();

app.MapControllers();

app.Run();