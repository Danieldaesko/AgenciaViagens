using AgenciaViagens.Mobile.ViewModels;

namespace AgenciaViagens.Mobile.Pages;

public partial class PacotesPage : ContentPage
{
    private readonly PacotesViewModel _vm;

    public PacotesPage(PacotesViewModel vm)
    {
        InitializeComponent();
        BindingContext = _vm = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (_vm.Pacotes.Count == 0)
            await _vm.CarregarCommand.ExecuteAsync(null);
    }
}