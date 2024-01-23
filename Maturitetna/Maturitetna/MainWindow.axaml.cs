using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Maturitetna;

public partial class MainWindow : Window
{
    private bool SignedIn = false;
    public MainWindow()
    {
        InitializeComponent();
        ShowProfile();
    }


    private void Button_OnClick(object? sender, RoutedEventArgs e)
    {
        var login = new Login();
        login.Show();
        SignedIn = true;
        ShowProfile();
    }

    private void ShowProfile()
    {
        Profile.IsVisible = SignedIn;
        SigButton.IsVisible = !SignedIn;
    }
}