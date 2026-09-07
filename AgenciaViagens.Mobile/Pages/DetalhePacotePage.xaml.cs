using AgenciaViagens.Mobile.ViewModels;

namespace AgenciaViagens.Mobile.Pages;

public partial class DetalhePacotePage : ContentPage
{
    public DetalhePacotePage(DetalhePacoteViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}