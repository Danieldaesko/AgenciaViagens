using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AgenciaViagens.Mobile.ViewModels;

public partial class BaseViewModel : ObservableObject
{
    [ObservableProperty]
    private bool ocupado;

    [ObservableProperty]
    private string? mensagemErro;

    public bool NaoOcupado => !Ocupado;

    partial void OnOcupadoChanged(bool value) => OnPropertyChanged(nameof(NaoOcupado));
}