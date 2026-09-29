using System;
using System.Globalization;
using Microsoft.Maui.Controls;
using AgenciaViagens.Mobile.Services;

namespace AgenciaViagens.Mobile.Converters
{
    /// <summary>
    /// Converte o ImagemUrl de um pacote num ImageSource utilizável no MAUI.
    /// Aceita URLs absolutos (http/https) e caminhos relativos vindos da API.
    /// Devolve null quando não há imagem, deixando visível a cor de fundo.
    /// </summary>
    public class UrlImagemConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var caminho = value as string;

            if (string.IsNullOrWhiteSpace(caminho))
                return null;

            caminho = caminho.Trim();

            // Já é um URL completo (ex. imagem externa)
            if (caminho.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                caminho.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                return ImageSource.FromUri(new Uri(caminho));
            }

            // Caminho relativo guardado na base de dados (ex. /imagens/pacotes/roma.jpg)
            var relativo = caminho.TrimStart('~', '/');

            return ImageSource.FromUri(new Uri(ApiConfig.MediaBaseUrl + relativo));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}