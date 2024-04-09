using System;
using System.Collections.Generic;
using System.Data;
using Avalonia.Controls;
using DynamicData;
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
    private const string conn = "Server=localhost;Database=maturitetna;Uid=root;Pwd=root;";
    public PlayListItem() {}
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
    public int Pesmica { get; set; }
    public string Dolzinapesmi
    {
        get { return dolzinaPesmi; }
        set { dolzinaPesmi = value; }
    }
    public int PesmId { get; set; }
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
    public int UporabnikID { get; set; }
    public string UporabniskoIme { get; set; }
    public int UserCollab { get; set; }
    public int PlaylistCollab { get; set; }
    public string DatumDostopa { get; set; }
   // protected DateTime Dodano { get; set; }
    private readonly MainWindow _mainWindow;
    private string DodaoAgo { get { return CalculateDodano(dodano); } set { } }
    public PlayListItem(MainWindow mainWindow, MainWindow.MusicItem musicItem)
    {
        _mainWindow = mainWindow;
        _musicItem = musicItem;
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
    public PlayListItem(int uporabnikId, string uporabniskoIme)
    {
        UporabnikID = uporabnikId;
        UporabniskoIme = uporabniskoIme;
    }
    // Za collebanje
    public PlayListItem(int userId, int playlisId, string datum_dostopa)
    {
        UserCollab = userId;
        PlaylistCollab = playlisId;
        DatumDostopa = datum_dostopa;
    }
    private string CalculateDodano( DateTime dodano)
    {
        TimeSpan neki = DateTime.Now - dodano;
        return neki.ToString();
    }


  DateTime dodano = DateTime.Today;

    public void DodajvPlaylisto(int playlist_id)
    {
            using (MySqlConnection connection = new MySqlConnection(conn))
            {
                connection.Open();
                string sql = "INSERT INTO inplaylist(user_id,pesmi_id,dodano,playlist_id) VALUES (@user_id,@pesmi_id,@dodano,@playlist_id);";
                    using (MySqlCommand command = new MySqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@user_id", MainWindow.userId);
                        command.Parameters.AddWithValue("@pesmi_id", _mainWindow.PESMI);
                        command.Parameters.AddWithValue("@dodano", dodano);
                        command.Parameters.AddWithValue("@playlist_id", playlist_id);
                        command.ExecuteNonQuery();
                    }
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
                          "WHERE playlist.playlist_id = @playlist_id  ";

            using (MySqlCommand command = new MySqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@playlist_id", playlist_id);
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
                            PesmId= pesmiId,
                            dodano = DodaoAgo,
                            UserId = userId,
                            PlaylistId = playlistId, 
                            Username = username,
                            Naslovpesmi = naslovPesmi,
                            Dolzinapesmi = dolzinaPesmi
                        );
                        _mainWindow.myPlayListsSongs.Add(playListItem);
                    }

                }
            }
        }
        _mainWindow.PlayListSongs.ItemsSource = _mainWindow.myPlayListsSongs;
    }
    //==============================================================================================================================
    //Dodaj uporabnika / Collabing
   /* public void Collabing()
    {
        using (MySqlConnection connection = new MySqlConnection(conn))
        {
            connection.Open();
            string sql = "SELECT pesmi_id FROM inplaylist WHERE playlist_id = @playlist_id";
            using (MySqlCommand command = new MySqlCommand(sql,connection))
            {
                //command.Parameters.AddWithValue("@pesmi_id",pesmica);
                command.Parameters.AddWithValue("@playlist_id",PlaylistId);
                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    { 
                        pesmica = reader.GetInt32("pesmi_id");
                        Console.WriteLine(pesmica);
                        for ( i = pesmica; i <= pesmica; i++)
                        {
                            //pesmice.Add(i);
                            Console.WriteLine(i);
                        }

                    }
                }
            }
        }
    }*/

    //private int i;
    //private int pesmica;
    public void DodajUporabnika(int user_id)
    {
        try
        {
                using (MySqlConnection connection = new MySqlConnection(conn))
                {
                    connection.Open();
                    string sql = "INSERT INTO collaborate VALUES (NULL,@user_id,@playlist_id)";
                    using (MySqlCommand command = new MySqlCommand(sql,connection))
                    {
                        command.Parameters.AddWithValue("user_id", user_id);
                        command.Parameters.AddWithValue("playlist_id",PlaylistId);
                        command.ExecuteNonQuery();
                        Console.WriteLine($"{user_id},{PlaylistId}");
                    }
                }
        }
        catch (Exception e)
        {
            Console.WriteLine($"ne deluje\n{e}");
            throw;
        }
       
    }

    public void NaloziUporabnike()
    {
        _mainWindow.DodajUporabnika.Clear();
        using (MySqlConnection connection = new MySqlConnection(conn))
        {
            connection.Open();
            string sql = "SELECT user_id, username From user";
            using (MySqlCommand command = new MySqlCommand(sql, connection))
            {
                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int uporabnikID = reader.GetInt32("user_id");
                        string uporabniskoIme = reader.GetString("username");
                        PlayListItem dodaj = new PlayListItem(
                            UporabnikID = uporabnikID,
                            UporabniskoIme = uporabniskoIme
                        );
                        _mainWindow.DodajUporabnika.Add(dodaj);
                    }
                    _mainWindow.Dodajuporabnika.ItemsSource = _mainWindow.DodajUporabnika;
                }
            }
        }
    }

    public void NaloziCollabanje()
    {
        _mainWindow.Collebanje.Clear();
        using (MySqlConnection connection = new MySqlConnection(conn))
        {
            connection.Open();
            string sql = "SELECT user_id, playlist_id, datum_dostopa FROM collaborate WHERE user_id=@user_id ORDER BY datum_dostopa DESC ";
            using (MySqlCommand command = new MySqlCommand(sql,connection))
            {
                command.Parameters.AddWithValue("user_id", MainWindow.userId);
                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int user_id = reader.GetInt32("user_id");
                        int playlist_id = reader.GetInt32("playlist_id");
                        string datum_dostopa = reader.GetString("datum_dostopa");
                        PlayListItem collab = new PlayListItem(
                            UserCollab = user_id,
                            PlaylistCollab = playlist_id,
                            DatumDostopa = datum_dostopa
                        );
                        Console.WriteLine($"{user_id},{playlist_id}");
                        _mainWindow.Collebanje.Add(collab);
                    }
                    _mainWindow.collabiList.ItemsSource = _mainWindow.Collebanje;
                    
                }   
            }
        }
    }
    
}