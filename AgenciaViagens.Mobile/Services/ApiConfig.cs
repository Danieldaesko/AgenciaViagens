using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgenciaViagens.Mobile.Services;

public static class ApiConfig
{
    // No emulador Android, 10.0.2.2 aponta para o localhost da máquina anfitriã
#if ANDROID
    public const string BaseUrl = "https://10.0.2.2:44322/api/";
#else
    public const string BaseUrl = "https://localhost:44322/api/";
#endif
}
