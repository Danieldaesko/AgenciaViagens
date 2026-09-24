using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using AgenciaViagens.Infrastructure.Identity;

namespace AgenciaViagens.Infrastructure.Data
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(IServiceProvider services)
        {
            var roleManager = services.GetRequiredService<RoleManager<ApplicationRole>>();
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

            // Criar os 3 roles do Termo
            foreach (var role in Roles.Todos)
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new ApplicationRole(role));
            }

            // Criar o utilizador Admin inicial
            const string adminEmail = "admin@agenciaviagens.pt";
            if (await userManager.FindByEmailAsync(adminEmail) is null)
            {
                var admin = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    Nome = "Administrador",
                    EmailConfirmed = true,
                    Ativo = true
                };

                var resultado = await userManager.CreateAsync(admin, "Admin@2026");
                if (resultado.Succeeded)
                    await userManager.AddToRoleAsync(admin, Roles.Admin);
            }

            const string staffEmail = "colaborador@agenciaviagens.pt";

            if (await userManager.FindByEmailAsync(staffEmail) is null)
            {
                var staff = new ApplicationUser
                {
                    UserName = staffEmail,
                    Email = staffEmail,
                    Nome = "Colaborador Teste",
                    EmailConfirmed = true,
                    Ativo = true
                };

                var resultado = await userManager.CreateAsync(staff, "Staff@2026");

                if (resultado.Succeeded)
                    await userManager.AddToRoleAsync(staff, Roles.Colaborador);
            }
        }
    }
}