using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace AgenciaViagens.Domain.Entities;

public class TipoAlojamento
{
    public string Codigo { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public decimal SuplementoPorPessoa { get; set; }

    public static readonly List<TipoAlojamento> Todos = new()
    {
        new TipoAlojamento
        {
            Codigo = "standard",
            Nome = "Standard",
            Descricao = "Quarto duplo em hotel de 3 estrelas, pequeno-almoço incluído.",
            SuplementoPorPessoa = 0
        },
        new TipoAlojamento
        {
            Codigo = "superior",
            Nome = "Superior",
            Descricao = "Hotel de 4 estrelas no centro, quarto com vista e pequeno-almoço.",
            SuplementoPorPessoa = 80
        },
        new TipoAlojamento
        {
            Codigo = "suite",
            Nome = "Suite",
            Descricao = "Hotel de 5 estrelas, suite com sala separada e meia pensão.",
            SuplementoPorPessoa = 200
        }
    };

    public static TipoAlojamento? PorCodigo(string? codigo) =>
        Todos.FirstOrDefault(a => a.Codigo == codigo);
}