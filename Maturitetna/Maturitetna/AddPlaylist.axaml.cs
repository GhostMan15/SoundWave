
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
    private int privacy = 1;
    public AddPlaylist(MainWindow mainWindow)
    {
        InitializeComponent();
        _mainWindow = mainWindow;
    }
  
    
    private void DodajPlaylisto()
    {
       
        var addplaylist = addPlaylist.Text; 
        var datum_ustvarjanja = DateTime.Now.ToString();
        int fk_user = MainWindow.userId;
        
            using MySqlConnection connection = new MySqlConnection(conn);
            connection.Open();
            string sql = "INSERT INTO playlist(playlist_ime,privacy,playlist_fk_user,datum_ustvarjanja) VALUES(@playlist_ime,@privacy,@playlist_fk_user,@datum_ustvarjanja);SELECT  LAST_INSERT_ID();";
            using MySqlCommand command = new MySqlCommand(sql,connection);
            command.Parameters.AddWithValue("@playlist_ime", addplaylist);
            command.Parameters.AddWithValue("@privacy", privacy);
            command.Parameters.AddWithValue("@playlist_fk_user", fk_user);
            command.Parameters.AddWithValue("@datum_ustvarjanja", datum_ustvarjanja);
            command.ExecuteNonQuery();

            int playlistId = Convert.ToInt32(command.ExecuteScalar());
                PlayList playlist = new PlayList
                {
                    ImePlaylista = addplaylist,
                    Privacy = privacy,
                    UserId = fk_user,
                    Ustvarjeno = datum_ustvarjanja
                };
                _mainWindow.myPlaylist.Add(playlist);
                foreach (var item in _mainWindow.myPlaylist)
                {
                    Console.WriteLine($"ImePlaylista: {item.ImePlaylista}, Privacy: {item.Privacy}, UserId: {item.UserId}, Ustvarjeno: {item.Ustvarjeno}");
                }
            this.Close();
        
     
    }

    public void IzpisiPlayliste()
    {
        // _mainWindow.myPlaylist.Clear();
        using (MySqlConnection connection = new MySqlConnection(conn))
        {
            connection.Open();
            string sql =
                "SELECT playlist_ime, privacy, playlist.playlist_fk_user, datum_ustvarjanja FROM playlist JOIN user ON playlist.playlist_fk_user = user.user_id WHERE user.user_id = @userId;";
            using (MySqlCommand command = new MySqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("userId", MainWindow.userId);
                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
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
                    }
                }
            }
        }
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
    public string? ImePlaylista { get; set; }
    public int Privacy {get; set; }
    public int UserId { get; set; }
    public string? Ustvarjeno { get; set; }
}