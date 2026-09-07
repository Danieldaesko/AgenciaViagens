using AgenciaViagens.Mobile.ViewModels;

namespace AgenciaViagens.Mobile.Pages;

public partial class ReservasPage : ContentPage
{
    private readonly ReservasViewModel _vm;

    public ReservasPage(ReservasViewModel vm)
    {
        InitializeComponent();
        BindingContext = _vm = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.CarregarCommand.ExecuteAsync(null);
    }
}