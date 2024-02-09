using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Maturitetna;

public partial class Playlist : Window
{
    
    private static MainWindow _mainWindow;
    public Playlist()
    {
        InitializeComponent();
        DataContext = this;
    }

    public Playlist(MainWindow mainWindow) : this()
    {
        _mainWindow = mainWindow;
    }
}

public class PlayListItem : MainWindow.MusicItem
{
    public PlayListItem(){}
    public string PlaylistIme { get; set; }
    public int PesmId { get; set; }
    public bool Privacy { get; set; }
    public PlayListItem(string playlistIme,int pesmId,bool privacy, string naslov, string dolzina, string destinacija, int userId) : base(naslov, dolzina, destinacija,
        userId)
    {
        PlaylistIme = playlistIme;
        PesmId = pesmId;
        Privacy = privacy;
    }
}