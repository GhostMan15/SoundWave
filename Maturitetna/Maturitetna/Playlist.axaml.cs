using System;
using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using MySqlConnector;

namespace Maturitetna;

public partial class Playlist : Window
{
    public Playlist()
    {
        InitializeComponent();
        DataContext = this;
    }
    
}

public class PlayListItem : PlayList
{
    private const string conn = "Server=localhost;Database=maturitetna;Uid=root;Pwd=root;";
    public PlayListItem()
    {
    }

    public static int PesmId;
    protected string Dodano { get; set; }
    private static MainWindow _mainWindow;
    private string DodaoAgo
    {
        get { return CalculateDodano(dodano); }
    }
    public PlayListItem(int pesmId, string imePlaylist, int privacy, int userId, string ustvarjeno, string dodano,MainWindow mainWindow)
        : base(imePlaylist, privacy, userId, ustvarjeno)
    {
        PesmId = pesmId;
        Dodano = dodano;
        _mainWindow = mainWindow;
       
    }
    private string CalculateDodano( DateTime dodano)
    {
        TimeSpan neki = DateTime.Now - dodano;
        return neki.ToString();
    }
    DateTime dodano = DateTime.Now;

  
    public void DodajvPlaylisto()
    {
        try
        {
            using (MySqlConnection connection = new MySqlConnection(conn))
            {
                connection.Open();
                string sql = "INSERT INTO inplaylist(user_id,pesmi_id,dodano) VALUES (@user_id,@pesmi_id,@dodano);";
                using (MySqlCommand command = new MySqlCommand(sql,connection))
                {
                    command.Parameters.AddWithValue("@user_id", UserId);
                    command.Parameters.AddWithValue("@pesmi_id", PesmId);
                    command.Parameters.AddWithValue("@dodano", dodano);
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

    public void NaloziPlaylisto()
    {
        using (MySqlConnection connection = new MySqlConnection(conn))
        {
            connection.Open();
        }
    }

}