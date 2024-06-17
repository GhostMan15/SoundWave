
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
    private readonly string _conn;
    private readonly MainWindow _mainWindow;
    private readonly PlayListItem _playListItem;
    private readonly PlayList _playList;
    private readonly ListBox _playListListBox;
    private MusicItem _musicItem;
    private DateTime dodano = DateTime.Now;
    private int privacy = 1;
    public AddPlaylist(MainWindow mainWindow, PlayListItem playListItem)
    {
        InitializeComponent();
        _mainWindow = mainWindow;
        _playListItem = playListItem;
        DataContext = _mainWindow;
        var reader = new AppSettingsReader("appsettings.json");
        _conn = reader.GetStringValue("ConnectionStrings:MyConnectionString");
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
        using MySqlConnection connection = new MySqlConnection(_conn);
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
            using (MySqlConnection connection = new MySqlConnection(_conn))
            {
                connection.Open();
                const string sql =
                    "SELECT playlist_id, playlist_ime, privacy, playlist.playlist_fk_user, datum_ustvarjanja,datum_dostopa FROM playlist " +
                    "JOIN user ON playlist.playlist_fk_user = user.user_id " +
                    "WHERE user.user_id = @userId ORDER BY datum_dostopa DESC";
                using (MySqlCommand command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@userId", MainWindow.userId);
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        { 
                            string imePlaylista = reader.GetString("playlist_ime");
                            int playlist_id = reader.GetInt32("playlist_id");
                            int privacy = reader.GetInt32("privacy");
                            int userId = reader.GetInt32("playlist_fk_user");
                            string ustvarjeno = reader.GetString("datum_ustvarjanja");
                            string? datum_dostopa = reader.IsDBNull(reader.GetOrdinal("datum_dostopa")) ? null : reader.GetString("datum_dostopa");
                            PlayList playlist = new PlayList
                            {
                                ImePlaylista = imePlaylista,
                                PlayListId = playlist_id,
                                Privacy = privacy,
                                UserId = userId,
                                Ustvarjeno = ustvarjeno,
                                DatumDostopa = datum_dostopa
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
        
            using (MySqlConnection connection = new MySqlConnection(_conn))
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
                                Ustvarjeno = ustvarjeno,
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

    public void PrikaziReacent()
    {
        _mainWindow.Reacently.Clear();
        using (MySqlConnection connection = new MySqlConnection(_conn))
        {
            connection.Open();
            /*string sql = "SELECT c.playlist_id, p.playlist_id, p.playlist_ime, p.playlist_fk_user " +
                         "FROM playlist p LEFT JOIN collaborate c ON c.playlist_id = p.playlist_id " +
                         "UNION SELECT c.playlist_id, p.playlist_id, p.playlist_ime,p.playlist_fk_user " +
                         "FROM collaborate c RIGHT JOIN playlist p ON c.playlist_id = p.playlist_id  WHERE p.playlist_fk_user = @user_id OR c.user_id = @user_id;"; */

            string sql = "SELECT * FROM  (SELECT c.playlist_id AS collab_playlist_id, p.playlist_id AS playlist_id, p.playlist_ime, p.playlist_fk_user, NULL AS datum_dostopa   " +
                         "FROM playlist p LEFT JOIN collaborate c ON c.playlist_id = p.playlist_id " +
                         "UNION SELECT c.playlist_id , p.playlist_id, p.playlist_ime, p.playlist_fk_user, p.datum_dostopa FROM collaborate c RIGHT JOIN playlist p ON c.playlist_id = p.playlist_id " +
                         "WHERE p.playlist_fk_user = @playlist_fk_user OR c.user_id = @user_id) AS combined_data ORDER BY CASE " +
                         "WHEN datum_dostopa IS NOT NULL THEN datum_dostopa ELSE datum_dostopa END DESC LIMIT 0, 5";
            using (MySqlCommand command = new MySqlCommand(sql,connection))
            {
                command.Parameters.AddWithValue("playlist_fk_user", MainWindow.userId);
                command.Parameters.AddWithValue("user_id", MainWindow.userId);
                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string ime_playlista = reader.GetString("playlist_ime");
                        int playlist_id = reader.GetInt32("playlist_id");
                        int playlist_fk_user = reader.GetInt32("playlist_fk_user");
                        string? datum = reader.IsDBNull(reader.GetOrdinal("datum_dostopa")) ? null : reader.GetString("datum_dostopa");
                        string? datumC = datum;
                        // se dokoncat (mnde sm)
                        var collabi = new PlayList(ime_playlista,playlist_id,playlist_fk_user,datum,datumC);
                        _mainWindow.Reacently.Add(collabi);
                    }
                }
            }
        }

        _mainWindow.RecentlyBox.ItemsSource = _mainWindow.Reacently;
        
    }

    public void UpdateTime(int? playlistid)
    {
        using (MySqlConnection connection = new MySqlConnection(_conn))
        {
            connection.Open();
            string sql = "UPDATE playlist SET datum_dostopa = NOW() WHERE playlist_id = @playlist_id AND playlist_fk_user = @user_id";
            using (MySqlCommand command = new MySqlCommand(sql,connection))
            {
                command.Parameters.AddWithValue("playlist_id",playlistid);
                command.Parameters.AddWithValue("@user_id", MainWindow.userId);
                command.ExecuteNonQuery();
            }
        }
    }
    public void UpdateTimeC(int playlistid)
    {
        using (MySqlConnection connection = new MySqlConnection(_conn))
        {
            connection.Open();
            string sql = "UPDATE collaborate SET datum_dostopa = NOW() WHERE playlist_id = @playlist_id AND user_id = @user_id";
            using (MySqlCommand command = new MySqlCommand(sql,connection))
            {
                command.Parameters.AddWithValue("playlist_id",playlistid);
                command.Parameters.AddWithValue("@user_id", MainWindow.userId);
                command.ExecuteNonQuery();
            }
        }
    }
}

    

