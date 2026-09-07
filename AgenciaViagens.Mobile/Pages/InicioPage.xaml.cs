using AgenciaViagens.Mobile.ViewModels;

namespace AgenciaViagens.Mobile.Pages;

public partial class InicioPage : ContentPage
{
    private readonly InicioViewModel _vm;

    public InicioPage(InicioViewModel vm)
    {
        InitializeComponent();
        BindingContext = _vm = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (_vm.Destaques.Count == 0)
            await _vm.CarregarCommand.ExecuteAsync(null);
    }
}