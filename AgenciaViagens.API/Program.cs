using AgenciaViagens.Application.Services;
using AgenciaViagens.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1. Configurar DbContext para ligar ao SQL Server
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. Registrar os Serviços de Aplicação (Injeção de Dependência)
builder.Services.AddScoped<ReservaService>();
builder.Services.AddScoped<PacoteService>();
builder.Services.AddScoped<AvaliacaoService>();
builder.Services.AddScoped<ItinerarioService>();

// 3. Adicionar CORS (Essencial para permitir pedidos do .NET MAUI e Blazor/Web)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// 4. Adicionar Controllers e Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// 5. Configurar a pipeline HTTP e ativar o Swagger
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "AgenciaViagens API v1");
    c.RoutePrefix = "swagger"; // Garante que abre em /swagger
});

app.UseHttpsRedirection();

// Ativar a política de CORS antes do Authorization
app.UseCors("AllowAll");

app.UseAuthorization();
app.MapControllers();

app.Run();