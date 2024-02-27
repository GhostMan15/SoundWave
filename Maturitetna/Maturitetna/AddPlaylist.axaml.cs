
using System;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;

using MySqlConnector;
namespace Maturitetna;

public partial class AddPlaylist : Window
{
    private string  conn = "Server=localhost;Database=maturitetna;Uid=root;Pwd=root;";
    private readonly MainWindow _mainWindow;
    private readonly PlayListItem _playListItem;
    private readonly PlayList _playList;
    private ListBox _playListListBox;
    private int privacy = 1;
    public AddPlaylist(MainWindow mainWindow, PlayListItem playListItem)
    {
        InitializeComponent();
        _mainWindow = mainWindow;
        _playListItem = playListItem;
        DataContext = _mainWindow;
        _playListListBox = MainWindow.FindListBoxByName("playListListBox", _mainWindow.Uploads);
       
    }
  
    public void DodajPlaylisto()
    {
       
        var addplaylist = addPlaylist.Text; 
        var datum_ustvarjanja = DateTime.Now.ToString();
        int fk_user = MainWindow.userId;
        
        using MySqlConnection connection = new MySqlConnection(conn);
        connection.Open();
        string sql = "INSERT INTO playlist(playlist_ime,privacy,playlist_fk_user,datum_ustvarjanja) VALUES(@playlist_ime,@privacy,@playlist_fk_user,@datum_ustvarjanja);";
        using MySqlCommand command = new MySqlCommand(sql,connection);
        command.Parameters.AddWithValue("@playlist_ime", addplaylist);
        command.Parameters.AddWithValue("@privacy", privacy);
        command.Parameters.AddWithValue("@playlist_fk_user", fk_user);
        command.Parameters.AddWithValue("@datum_ustvarjanja", datum_ustvarjanja);
        command.ExecuteNonQuery();
        this.Close();
        IzpisiPlayliste();
     
    }

  public void IzpisiPlayliste()
    {
        _mainWindow.myPlaylist.Clear();
        _mainWindow.AllPlaylists.Clear();
        
            using (MySqlConnection connection = new MySqlConnection(conn))
            {
                connection.Open();
                string sql =
                    "SELECT  playlist_ime, privacy, playlist.playlist_fk_user, datum_ustvarjanja FROM playlist JOIN user ON playlist.playlist_fk_user = user.user_id WHERE user.user_id = @userId;";
                using (MySqlCommand command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("userId", MainWindow.userId);
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            //_playListItem.PlaylistId = reader.GetInt32("playlist_id");
                            string imePlaylista = reader.GetString("playlist_ime");
                            int privacy = reader.GetInt32("privacy");
                            int userId = reader.GetInt32("playlist_fk_user");
                            string ustvarjeno = reader.GetString("datum_ustvarjanja");
                            PlayList playlist = new PlayList
                            {
                                ImePlaylista = imePlaylista,
                                Privacy = privacy,
                                UserId = userId,
                                Ustvarjeno = ustvarjeno
                            };
                            _mainWindow.myPlaylist.Add(playlist);
                            _mainWindow.AllPlaylists.Add(playlist);
                            _mainWindow.PlaylistBox.ItemsSource = _mainWindow.myPlaylist;
                            
                        }
                    }
                }
            }
    }

    public void PobrisiPlaylist()
    {
        _mainWindow.myPlaylist.Clear();
        _mainWindow.PlaylistBox.ItemsSource = _mainWindow.myPlaylist;
    }
 
  
    private void ToggleButton_OnChecked(object? sender, RoutedEventArgs e)
    {
        if (sender is ToggleButton toggleButton && toggleButton.IsChecked == true)
        {
            privacy = 2;
        }
    }

    private void Adding_OnClick(object? sender, RoutedEventArgs e)
    {
      DodajPlaylisto();
    }
}

public class PlayList
{
    public string ImePlaylista { get; set; }
    public int Privacy { get;set; }
    public int UserId { get; set; }
    public string? Ustvarjeno { get; set; }
 
    public PlayList(){}
    
}