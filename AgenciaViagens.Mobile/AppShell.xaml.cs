using AgenciaViagens.Mobile.Pages;

namespace AgenciaViagens.Mobile;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        Routing.RegisterRoute("detalhepacote", typeof(DetalhePacotePage));
    }
}