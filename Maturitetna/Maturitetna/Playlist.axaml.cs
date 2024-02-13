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

public class PlayListItem : PlayList
{
    public PlayListItem(){}
  
    public int PesmId { get; set; }

    public PlayListItem( int pesmId, string imePlaylist, int privacy,int userId, string ustvarjeno)
        : base(imePlaylist,privacy,userId,ustvarjeno)
    {
      
        PesmId = pesmId;
    }
}