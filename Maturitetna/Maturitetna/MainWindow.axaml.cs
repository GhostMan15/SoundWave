using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Threading.Tasks;
using Avalonia;
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
    public ObservableCollection<MusicItem> myUploads { get; }= new ObservableCollection<MusicItem>();
    private string  conn = "Server=localhost;Database=maturitetna;Uid=root;Pwd=root;";
    private static Login? _login;
    public  static int userId;
    private string uploadFolder = "C:\\Users\\faruk\\Documents\\GitHub\\Muska";
    public MainWindow()
    {
        InitializeComponent();
        _login = new Login(this);
    }

    public MainWindow(Login login) : this()
    {
         //PobrisiUplode();
        _login = login;
        DataContext = _login;
        
    }

    public class MusicItem
    {
        public string Naslov { get; set; }
        public string Dolzina { get; set; }
        public string Destinacija { get; }
        
        public int UserId
        {
            get { return userId;  }
            set { userId = value; }
        }
        
        public MusicItem(){}
        public MusicItem( string naslov, string dolzina, string destinacija, int userId)
        {
            Naslov = naslov;
            Dolzina = dolzina;
            Destinacija = destinacija;
            UserId = userId; 
        }
    }
 
    private void Button_OnClick(object? sender, RoutedEventArgs e)
    {
        var login = new Login(this);
        login.Show();
        SignedIn = true;
       // PobrisiUplode();
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
            fileDialog.Filters.Add(new FileDialogFilter { Name = "file", Extensions = { "mp3", "wav", "ogg" } });

            var izbraniFile = await fileDialog.ShowAsync(this);
            if (izbraniFile != null && izbraniFile.Length > 0)
            {
                foreach (var file in izbraniFile)
                {
                    //Dodajanje file v databazo in v file kjer ga hrani
                    var naslov = System.IO.Path.GetFileNameWithoutExtension(file);
                    var dolzina = await Audio.GetAudioFileLength(file);
                    var userID = MainWindow.userId;
                    //=================================================================================
                    var fileName = Guid.NewGuid().ToString() + System.IO.Path.GetExtension(file);
                    var destinacija = System.IO.Path.Combine(uploadFolder, fileName);
                    System.IO.File.Copy(file, destinacija, true);
                    //================================================================================
                    var newMusic = new MusicItem(naslov, dolzina, destinacija,userID);
                    myUploads.Add(newMusic);
                    ShraniVDatabazo(newMusic);
                }
            }
        }
   

    public async Task NaloizIzDatabaze()
    {
      
        using (MySqlConnection connection = new MySqlConnection(conn) )
        {
            connection.Open();
            int userID = MainWindow.userId;
            string neki = userID.ToString();
            string sql = "SELECT naslov_pesmi,dolzina_pesmi,file_ext,pesmi.user_id FROM pesmi JOIN  user ON pesmi.user_id = user.user_id WHERE  user.user_id = @user_id ";
                using (MySqlCommand command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@user_id", neki);
                    await using (MySqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        while (reader.Read())
                        {
                            var musicItem = new MusicItem(
                                reader.GetString("naslov_pesmi"),
                                reader.GetString("dolzina_pesmi"), 
                                reader.GetString("file_ext"), 
                                    reader.GetInt32("user_id")
                                );
                                
                            myUploads.Add(musicItem);
                        }
                    }
                }
           
        }
    }

  
   private void ShraniVDatabazo(MusicItem musicItem)
   {
       try
       {
           if (userId == null)
           {
               // Handle the case where a user is not logged in (informative error message)
               Console.WriteLine("Please log in to save uploads.");
               return;
           }
           using (MySqlConnection connection = new MySqlConnection(conn))
           {
               connection.Open();
               string sql = "INSERT INTO pesmi(naslov_pesmi,dolzina_pesmi,file_ext,user_id) VALUES(@naslov_pesmi,@dolzina_pesmi,@file_ext,@user_id)";
               using (MySqlCommand command = new MySqlCommand(sql, connection))
               {
                   command.Parameters.AddWithValue("@naslov_pesmi", musicItem.Naslov);
                   command.Parameters.AddWithValue("@dolzina_pesmi", musicItem.Dolzina);
                   command.Parameters.AddWithValue("@file_ext", musicItem.Destinacija );
                   command.Parameters.AddWithValue("user_id", musicItem.UserId);
                    command.ExecuteNonQuery();

               }
           }
       }
       catch (Exception e)
       {
           Console.WriteLine(e);
           throw;
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
