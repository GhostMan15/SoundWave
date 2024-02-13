using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using MySqlConnector;

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
    private const string conn = "Server=localhost;Database=maturitetna;Uid=root;Pwd=root;";
    public PlayListItem()
    {
    }

    public int PesmId { get; set; }

    public PlayListItem(int pesmId, string imePlaylist, int privacy, int userId, string ustvarjeno)
        : base(imePlaylist, privacy, userId, ustvarjeno)
    {

        PesmId = pesmId;
    }

    public void DodajvPlaylisto()
    {
        using (MySqlConnection connection = new MySqlConnection(conn))
        {
            connection.Open();
            using (MySqlCommand command = new MySqlCommand(conn))
            {
                
            }
        }
    }

}