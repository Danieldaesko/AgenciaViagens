using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgenciaViagens.Infrastructure.Identity
{
    public static class Roles
    {
        public const string Admin = "Admin";
        public const string Colaborador = "Colaborador";
        public const string Cliente = "Cliente";

        public static readonly string[] Todos = { Admin, Colaborador, Cliente };
    }
}