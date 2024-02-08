using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using MySqlConnector;
using NAudio.Wave;




namespace Maturitetna;

public partial class MainWindow : Window, INotifyPropertyChanged
{
    private  bool SignedIn;
    public ObservableCollection<MusicItem> myUploads { get; }= new ObservableCollection<MusicItem>();
    private string  conn = "Server=localhost;Database=maturitetna;Uid=root;Pwd=root;";
    private static Login? _login;
    public  static int userId;
    private string uploadFolder = "/home/Faruk/Documents/GitHub/Maturitetna/Muska";
    public MainWindow()
    {
        InitializeComponent();
        _login = new Login(this);
    }

    public MainWindow(Login login) : this()
    {
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


    private void MenuItem_OnClick(object? sender, RoutedEventArgs e)
    {
        
    }
//Za pridobivanje in shrabmo glasbe
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
            string sql = "SELECT naslov_pesmi,dolzina_pesmi,file_ext,pesmi.user_id FROM pesmi JOIN  user ON pesmi.user_id = user.user_id WHERE  user.user_id = @user_id ";
                using (MySqlCommand command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@user_id", userID);
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
               Console.WriteLine("Prvo login");
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

//=============================================================================================================================
    public void ShowProfile()
    {
        Profile.IsVisible = SignedIn;
        SigButton.IsVisible = !SignedIn;
        Logout.IsVisible = SignedIn;
    }

    private void PobrisiUplode()
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
    //==============================================================================================================================================   
    //Za prikazovanje na playerju
    private double _playProgress;
    public double PlayProgress
    {
        get { return _playProgress; }
        set
        {
            _playProgress = value; 
            RaisePropertyChanged(nameof(PlayProgress));
        }
    }
    private double _playDolzina;
    public double PlayDolzina
    {
        get { return _playDolzina; }
        set
        {
            _playDolzina = value;
            RaisePropertyChanged(nameof(PlayDolzina));
        }
    }
    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void RaisePropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));   
    }
//==============================================================================================================================================
//Naprej nazaj start stop player
    private TimeSpan _trenutniCas;

    public TimeSpan TrenutniCas
    {
        get { return _trenutniCas; }
        set
        {
            _trenutniCas = value;
            RaisePropertyChanged(nameof(TrenutniCas));
        }
    }

    private int _trenutniTrack = -1;

    private void PlayNext()
    {
        if (_trenutniTrack < myUploads.Count - 1)
        {
            if (outputDevice!=null && outputDevice.PlaybackState == PlaybackState.Playing)
            {
                outputDevice.Stop();
                outputDevice.Dispose();
            }
            _trenutniTrack++;
            PlaySelectedTrack();
        }
    }

    private void PlayPrevious()
    {
        if (_trenutniTrack > 0)
        {
            if (outputDevice.PlaybackState == PlaybackState.Playing)
            {
                outputDevice.Stop();
                outputDevice.Dispose();
            }
            _trenutniTrack--;
            PlaySelectedTrack();
        }
    }

    private async Task PlaySelectedTrack()
    {
        if (_trenutniTrack >= 0 && _trenutniTrack < myUploads.Count)
        {
            var filePath = Path.Combine(uploadFolder, myUploads[_trenutniTrack].Destinacija);
            if (File.Exists(filePath))
            {
                if (outputDevice != null && outputDevice.PlaybackState == PlaybackState.Playing)
                {
                    outputDevice.Stop();
                    outputDevice.Dispose();
                }
                await  PlayAudio(filePath);
            }
           
            else
            {
                Console.WriteLine("Ne spila");
            }
        }
    }

    private WaveOutEvent outputDevice;
    private async Task UpdateTime()
    {
        while (true)
        {
            if (outputDevice != null && outputDevice.PlaybackState == PlaybackState.Playing)
            {
                TrenutniCas = TimeSpan.FromSeconds(PlayProgress);
            }

            await Task.Delay(1000);
        }
    }
    private void Next_OnClick(object? sender, RoutedEventArgs e)
    {
        PlayNext();
    }
    private void Previous_OnClick(object? sender, RoutedEventArgs e)
    {
       PlayPrevious();
    }
//=========================================================================================================================================================
//Vloume Up and Down
    private float _volumeLevel = 0.5f;

    public float VolumeLevel
    {
        get { return _volumeLevel; }
        set
        {
            _volumeLevel = value;
            RaisePropertyChanged(nameof(VolumeLevel));
            UpdateVolume();
        }
    }

    private void UpdateVolume()
    {
        if (outputDevice != null)
        {
            outputDevice.Volume = VolumeLevel;
        }
    }
//=========================================================================================================================================================    
// Za predavjanje glasbe 
    public async Task PlayAudio(string filePath)
    {
        try
        {
            using (var audioFile = new AudioFileReader(filePath))
            {
                    outputDevice = new WaveOutEvent();
                    outputDevice.Volume = VolumeLevel;
                    outputDevice.Init(audioFile);
                    outputDevice.Play();

                    PlayDolzina = audioFile.TotalTime.TotalSeconds;
                    _ = UpdateTime();
                
                    while (outputDevice.PlaybackState == PlaybackState.Playing)
                    {
                        PlayProgress = audioFile.CurrentTime.TotalSeconds;
                        await Task.Delay(200); 
                    }
                
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"Problem sef: {e}");
            throw;
        }
    }
 
    private void Play_OnClick(object? sender, RoutedEventArgs e)
    {
        var button = sender as Button;
        var musicItem = button.DataContext as MusicItem;
        var fileName = musicItem.Destinacija;
        if (!string.IsNullOrEmpty(fileName))
        {
            var filePath = Path.Combine(uploadFolder, fileName);
            if (File.Exists(filePath))
            {
                PlayAudio(filePath);
            }
            else
            {
                Console.WriteLine("File ne obstaja");
            }
        }
    }



}
