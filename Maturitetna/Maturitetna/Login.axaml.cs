using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace Maturitetna;

public partial class Login : Window
{
    public Login()
    {
        InitializeComponent();
    }

    private void Button_OnClick(object? sender, RoutedEventArgs e)
    {
        var register = new Register();
        register.Show();
        this.Close();
    }

   
}