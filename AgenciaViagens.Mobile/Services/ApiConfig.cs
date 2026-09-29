namespace AgenciaViagens.Mobile.Services;

public static class ApiConfig
{
    // No emulador Android, 10.0.2.2 aponta para o localhost da máquina anfitriã
#if ANDROID
    public const string BaseUrl = "https://10.0.2.2:44341/api/";

    // As imagens vão por HTTP porque o componente Image do MAUI usa um
    // HttpClient interno próprio, que não passa pelo nosso handler e por
    // isso rejeita o certificado de desenvolvimento.
    public const string MediaBaseUrl = "http://10.0.2.2:5000/";
#else
    public const string BaseUrl = "https://localhost:44341/api/";
    public const string MediaBaseUrl = "http://localhost:5000/";
#endif
}