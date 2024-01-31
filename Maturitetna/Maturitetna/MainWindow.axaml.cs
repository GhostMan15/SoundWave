using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using MySqlConnector;
using Dapper;
using ClosedXML;
using NAudio.Wave;
using NAudio;

namespace Maturitetna;

public partial class MainWindow : Window
{
    private  bool SignedIn;
    public ObservableCollection<MusicItem> myUploads { get; } = new ObservableCollection<MusicItem>();
    private string  conn = "Server=localhost;Database=maturitetna;Uid=root;Pwd=root;";
    
    public MainWindow()
    {
        InitializeComponent();
        PobrisiUplode();
     //  Closed += TopLevel_OnClosed;
    }

    public class MusicItem
    {
        public string Naslov { get; set; }
        public string Dolzina { get; set; }
        public long UserId { get; set; }
        
        
    public MusicItem(){}
        public MusicItem( string naslov, string dolzina)
        {
            Naslov = naslov;
            Dolzina = dolzina;
        }
    }
 
    private void Button_OnClick(object? sender, RoutedEventArgs e)
    {
        var login = new Login(this);
        login.Show();
        SignedIn = true;
        PobrisiUplode();
    }

    
    private void Upload_OnClick(object? sender, RoutedEventArgs e)
    {
        Prikazi();
    }


    private async void MenuItem_OnClick(object? sender, RoutedEventArgs e)
    {
        
    }
    
    public class Audio
    {
        public static async Task<string> GetAudioFileLength(string filePath)
        {
            try
            {
                using (var audioFile = new AudioFileReader(filePath))
                {
                    TimeSpan duration = audioFile.TotalTime;
                    return duration.TotalSeconds.ToString();
                }
            }
            catch (Exception ex)
            {
                
                return "Unknown";
            }
        }
    }

    private async Task Prikazi()
    {
        var fileDialog = new OpenFileDialog();
        fileDialog.Title = "Izberite file";
        fileDialog.Filters.Add(new FileDialogFilter{Name = "file", Extensions = {"mp3","wav","ogg"}});

        var izbraniFile = await fileDialog.ShowAsync(this);
        if (izbraniFile != null && izbraniFile.Length > 0)
        {
            foreach (var file in izbraniFile)
            {
                var naslov = System.IO.Path.GetFileNameWithoutExtension(file);
                var dolzina = await Audio.GetAudioFileLength(file);
                var newMusic = new MusicItem(naslov,dolzina);
                myUploads.Add(newMusic);
                ShraniVDatabazo(newMusic);
            }
        }
    }

    public void NaloizIzDatabaze()
    {
      
        using (MySqlConnection connection = new MySqlConnection(conn) )
        {
            connection.Open();
            string sql = "SELECT * FROM  pesmi  ";
            var pesmi = connection.Query<MusicItem>(sql);
            foreach (var pesm in pesmi)
            {
                myUploads.Add(pesm);
            }
        }
    }

    private void ShraniVDatabazo(MusicItem musicItem)
    {
        using (MySqlConnection connection = new MySqlConnection(conn))
        {
            connection.Open();
            string sql = "INSERT INTO pesmi(user_fk_id,naslov_pesmi,dolzina_pesmi) VALUES(@user_fk_id,@naslov_pesmi,@dolzina_pesmi)";
            using (MySqlCommand command = new MySqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@user_fk_id", musicItem.UserId);
                command.Parameters.AddWithValue("@naslov_pesmi", musicItem.Naslov);
                command.Parameters.AddWithValue("@dolzina_pesmi", musicItem.Dolzina);
                command.ExecuteNonQuery();
                connection.Close();
            }
        }
    }
/* private async Task Spili(string? filePath) Za predvajanje glasbe
    {
        try
        {
            using (var audioFile = new AudioFileReader(filePath))
            using (var outputDevice = new WaveOutEvent())
            {
                outputDevice.Init(audioFile);
                outputDevice.Play();


                while (outputDevice.PlaybackState == PlaybackState.Playing)
                {
                    await Task.Delay(500);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error playing audio: {ex.Message}");
        }
    }

   private void PlayButton_Click(object? sender, EventArgs e)
    {
        var button = (Button)sender;
        var duration = (string)button.Tag;
        var filePath = (string)button.Tag;

         Spili(filePath);
    }
    */

    public void ShowProfile()
    {
        Profile.IsVisible = SignedIn;
        SigButton.IsVisible = !SignedIn;
        Logout.IsVisible = SignedIn;
    }

    public void PobrisiUplode()
    {
        myUploads.Clear();
        Uploads.ItemsSource = myUploads;
    }

    private void TopLevel_OnClosed(object? sender, EventArgs e)
    {
        PobrisiUplode();
    }

    private void Logout_OnClick(object? sender, RoutedEventArgs e)
    {
        
        SignedIn = false;
        ShowProfile();
        PobrisiUplode();
        
    }
    
}
