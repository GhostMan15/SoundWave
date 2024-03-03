using System;
using System.Collections.Generic;
using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using MySqlConnector;

namespace Maturitetna;

public partial class Playlists : Window
{
    public Playlists()
    {
        InitializeComponent();
        DataContext = this;
        
    }
   
}

public class PlayListItem  
{
   
      /*  private readonly PlayList _playlist;

        public PlayListItem(MainWindow mainWindow, PlayList playlist)
        {
            _playlist = playlist;
            // Other constructor logic
        }

        public void UpdatePlaylist(PlayListItem playlist)
        {
            // Update the playlist information
            PlaylistId = playlist?.PlaylistId ?? 0;
            // Other playlist updates as needed
        }*/

        // Other class members
    
    private const string conn = "Server=localhost;Database=maturitetna;Uid=root;Pwd=root;";
    public PlayListItem() {}
    public  int pesmId;
    public static string username;
    public  string naslovPesmi;
    public  string dolzinaPesmi;
    public  int playlisId;
    private MainWindow.MusicItem _musicItem;
    public string Naslovpesmi
    {
        get { return naslovPesmi; }
        set { naslovPesmi = value; }
    }
    public string Dolzinapesmi
    {
        get { return dolzinaPesmi; }
        set { dolzinaPesmi = value; }
    }
    public int PesmId
    {
        get { return pesmId;}
        set { pesmId = value; }
    }
    public string Username
    {
        get { return username; }
        set { username = value; }
    }
    public int PlaylistId
    {
        get { return playlisId;}
        set { playlisId = value; }
    }
    public  int UserId { get; set; }
   // protected DateTime Dodano { get; set; }
    private readonly MainWindow _mainWindow;
    private string DodaoAgo
    {
        get
        {
            return CalculateDodano(dodano); 
        }
        set { }
    }

    public PlayListItem(MainWindow.MusicItem musicItem)
    {
        _musicItem = musicItem;
    }
    public PlayListItem(MainWindow mainWindow)
    {
        _mainWindow = mainWindow;
        
    }
    public PlayListItem(int pesmId ,string dodaoAgo, int playlisId, int userId, string username, string naslovPesmi, string dolzinaPesmi)
    {
        PesmId = pesmId;
        PlaylistId = playlisId;
        UserId = userId;
        DodaoAgo = dodaoAgo;
        Username = username;
        Naslovpesmi = naslovPesmi;
        Dolzinapesmi = dolzinaPesmi;

    }
    private string CalculateDodano( DateTime dodano)
    {
        TimeSpan neki = DateTime.Now - dodano;
        return neki.ToString();
    }


  DateTime dodano = DateTime.Now;

    public void DodajvPlaylisto(int playlist_id)
    {
       
        try
        {
            using (MySqlConnection connection = new MySqlConnection(conn))
            {
                connection.Open();
                string sql = "INSERT INTO inplaylist(user_id,pesmi_id,dodano,playlist_id) VALUES (@user_id,@pesmi_id,@dodano,@playlist_id);";
                using (MySqlCommand command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@user_id", MainWindow.userId);
                    command.Parameters.AddWithValue("@pesmi_id", _musicItem.PesmiID);
                    command.Parameters.AddWithValue("@dodano", dodano);
                    command.Parameters.AddWithValue("@playlist_id", playlist_id);
                    command.ExecuteNonQuery();
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"Problemi slef {e}");
            throw;
        }

        
    }

    
    public void NaloziPlaylisto(int playlist_id)
    {
        _mainWindow.myPlayListsSongs.Clear();
        using (MySqlConnection connection = new MySqlConnection(conn))
        {
            connection.Open();
            string sql =  "SELECT  pesmi.pesmi_id, pesmi.naslov_pesmi,pesmi.dolzina_pesmi, user.user_id, user.username, dodano, playlist.playlist_id FROM inplaylist " +
                          "JOIN pesmi ON pesmi.pesmi_id = inplaylist.pesmi_id " +
                          "JOIN playlist ON playlist.playlist_id = inplaylist.playlist_id " +
                          "JOIN user ON user.user_id = inplaylist.user_id " +
                          "WHERE playlist.playlist_id = @playlist_id AND user.user_id = @user_id ";

            using (MySqlCommand command = new MySqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@playlist_id", playlist_id);
                command.Parameters.AddWithValue("@user_id", MainWindow.userId);
                command.Parameters.AddWithValue("@username", username);
                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int userId = reader.GetInt32("user_id");
                        int pesmiId = reader.GetInt32("pesmi_id"); 
                        int playlistId = reader.GetInt32("playlist_id");
                        string dodano = reader.GetString("dodano");
                        string username = reader.GetString("username");
                        string naslovPesmi = reader.GetString("naslov_pesmi");
                        string dolzinaPesmi = reader.GetString("dolzina_pesmi");
                        PlayListItem playListItem = new PlayListItem(
                            _musicItem.PesmiID = pesmiId,
                            dodano = DodaoAgo,
                            UserId = userId,
                            PlaylistId = playlistId, 
                            Username = username,
                            Naslovpesmi = naslovPesmi,
                            Dolzinapesmi = dolzinaPesmi
                            
                            //inplaylistId
                            
                        );
                        _mainWindow.myPlayListsSongs.Add(playListItem);
                        _mainWindow.PlayListSongs.ItemsSource = _mainWindow.myPlayListsSongs;
                    }

                }
            }
        }
        
        
    }
    //==============================================================================================================================
    //Dodaj uporabnika
    public void DodajUporabnika()
    {
        _mainWindow.DodajUporabnika.Clear();
        using (MySqlConnection connection = new MySqlConnection(conn))
        {
            connection.Open();
            string sql = "SELECT  username From user";
            using (MySqlCommand command = new MySqlCommand(sql, connection))
            {
                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string username = reader.GetString("username");
                        _mainWindow.DodajUporabnika.Add(username);
                        _mainWindow.Dodajuporabnika.ItemsSource = _mainWindow.DodajUporabnika;
                    }
                   
                }
            }
        }
    }
}