
using System;
using System.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using System.Collections.ObjectModel;
using MySqlConnector;
namespace Maturitetna;

public partial class AddPlaylist : Window
{
    private string  conn = "Server=localhost;Database=maturitetna;Uid=root;Pwd=root;";
    private readonly MainWindow _mainWindow;
    private readonly PlayListItem _playListItem;
    private readonly PlayList _playList;
    private readonly ListBox _playListListBox;
    

    private int privacy = 1;
    public AddPlaylist(MainWindow mainWindow, PlayListItem playListItem)
    {
        InitializeComponent();
        _mainWindow = mainWindow;
        _playListItem = playListItem;
        DataContext = _mainWindow;
        _playListListBox = MainWindow.FindListBoxByName("playListListBox", _mainWindow.Uploads);
        if (_playListListBox == null)
        {
            _playListListBox = new ListBox();
            _playListListBox.Name = "playListListBox";
            
        }
       
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
        IzpisiPlaylistePublic();
     
    }
    
  public void IzpisiPlayliste()
    {
        _mainWindow.myPlaylist.Clear();
        _mainWindow.AllPlaylists.Clear();
        try
        {
            using (MySqlConnection connection = new MySqlConnection(conn))
            {
                connection.Open();
                const string sql =
                    "SELECT playlist_id, playlist_ime, privacy, playlist.playlist_fk_user, datum_ustvarjanja FROM playlist " +
                    "JOIN user ON playlist.playlist_fk_user = user.user_id " +
                    "WHERE user.user_id = @userId ";
                using (MySqlCommand command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@userId", MainWindow.userId);
                    //command.Parameters.AddWithValue("@playlist_id", PlayListItem.playlisId);
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        { 
                            string imePlaylista = reader.GetString("playlist_ime");
                            int playlist_id = reader.GetInt32("playlist_id");
                            int privacy = reader.GetInt32("privacy");
                            int userId = reader.GetInt32("playlist_fk_user");
                            string ustvarjeno = reader.GetString("datum_ustvarjanja");
                            PlayList playlist = new PlayList
                            {
                                ImePlaylista = imePlaylista,
                                PlayListId = playlist_id,
                                Privacy = privacy,
                                UserId = userId,
                                Ustvarjeno = ustvarjeno
                            };
                            _mainWindow.myPlaylist.Add(playlist);
                            _mainWindow.AllPlaylists.Add(playlist);
                            _mainWindow.PlaylistBox.ItemsSource = _mainWindow.myPlaylist;
                            _playListListBox.ItemsSource = _mainWindow.AllPlaylists;
                           
                        }
                    }
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"Ne izpise playliste {e}");
            throw;
        }
            
    }
     public  void IzpisiPlaylistePublic()
    {
        _mainWindow.PublicPlayLists.Clear();
        
            using (MySqlConnection connection = new MySqlConnection(conn))
            {
                connection.Open();
                string sql =
                    "SELECT playlist_id, playlist_ime, privacy, playlist.playlist_fk_user, datum_ustvarjanja FROM playlist WHERE  privacy =2;";
                using (MySqlCommand command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("userId", MainWindow.userId);
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string imePlaylista = reader.GetString("playlist_ime");
                            int playlist_id = reader.GetInt32("playlist_id");
                            int privacy = reader.GetInt32("privacy");
                            int userId = reader.GetInt32("playlist_fk_user");
                            string ustvarjeno = reader.GetString("datum_ustvarjanja");
                            PlayList playlist = new PlayList
                            {
                                ImePlaylista = imePlaylista,
                                PlayListId = playlist_id,
                                Privacy = privacy,
                                UserId = userId,
                                Ustvarjeno = ustvarjeno
                            };
                            _mainWindow.PublicPlayLists.Add(playlist);
                            _mainWindow.Public.ItemsSource = _mainWindow.PublicPlayLists;
                           
                            
                           
                           
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
    public int PlayListId { get; set; }
    public int Privacy { get; set; }
    public int UserId { get; set; }
    public string Ustvarjeno { get; set; }
    
    public PlayList(){}
    
}
