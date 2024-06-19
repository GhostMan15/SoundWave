using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Confluent.Kafka;
using MySqlConnector;
using NAudio.Wave;
using VisualExtensions = Avalonia.VisualTree.VisualExtensions;
using System.Net;


namespace Maturitetna;
public partial class  MainWindow:Window,INotifyPropertyChanged
{
    private  bool SignedIn;
    public ObservableCollection<MusicItem> myUploads { get; }= new ObservableCollection<MusicItem>();
    public ObservableCollection<PlayList> myPlaylist { get; set; } = new ObservableCollection<PlayList>();
    public ObservableCollection<PlayList> AllPlaylists { get; set; } = new ObservableCollection<PlayList>();
    public ObservableCollection<PlayListItem> myPlayListsSongs { get; } = new ObservableCollection<PlayListItem>();
    public ObservableCollection<PlayList> PublicPlayLists { get; } = new ObservableCollection<PlayList>();
    public ObservableCollection<PlayListItem> DodajUporabnika { get; } = new ObservableCollection<PlayListItem>();
    public ObservableCollection<PlayListItem> Collebanje { get; } = new ObservableCollection<PlayListItem>();
    public ObservableCollection<PlayList> Reacently { get; } = new ObservableCollection<PlayList>();
    
    private string uploadFolder = "C:\\Users\\faruk\\Documents\\GitHub\\Maturitetna\\Muska";
    private static  Login _login;
    public  static int  userId;
    private readonly AddPlaylist _addPlaylist;
    private readonly PlayListItem _playlist;
    private readonly PlayList _onlyplaylist;

    private readonly string _conn;
    //private  ButtonTag _buttonTag;
    public MusicItem _musicItem;
    //private PlayList _song;

    public string Username
    {
        get { return PlayListItem.username; }
        set { PlayListItem.username = value; }
    }
    
    public MainWindow()
    {
        InitializeComponent();
        _login = new Login(this, _addPlaylist, _playlist);
        _musicItem = new MusicItem();
        _onlyplaylist = new PlayList();
        _addPlaylist = new AddPlaylist(this, _playlist);
        _playlist = new PlayListItem(this, _musicItem);
        var reader = new AppSettingsReader("appsettings.json");
        _conn = reader.GetStringValue("ConnectionStrings:MyConnectionString");
        DataContext = this;
        _addPlaylist.IzpisiPlaylistePublic();
    }
    

    public MainWindow(Login login, AddPlaylist addplaylist, PlayListItem playlist, PlayList onlyplaylist, MusicItem musicItem) : this()
    {
        _login = login;
        _addPlaylist = addplaylist;
        _onlyplaylist = onlyplaylist;
        _playlist = playlist;
        _musicItem = musicItem;
        DataContext = this;
    }
//======================================================================================================================
// My Uploads
  
  //=======================================================================================================================
 //Login/Upload buttons

    private void Button_OnClick(object? sender, RoutedEventArgs e)
    {
        var login = new Login(this, _addPlaylist, _playlist);
        login.Show();
        SignedIn = true;
        uploadButton.IsVisible = true;
        
    }

    
    [Obsolete("Opening file explorer")]
    private void Upload_OnClick(object? sender, RoutedEventArgs e)
    {
        Prikazi();
    }
//======================================================================================================================    
//Za pridobivanje in shrabmo glasbe
    public class Audio
    {
        public static Task<string> GetAudioFileLength(string filePath)
        {
            try
            {
                using (var audioFile = new AudioFileReader(filePath))
                {
                    TimeSpan duration = audioFile.TotalTime;
                    string formattedDuration = $"{(int)duration.TotalMinutes}:{duration.Seconds:D2}";
                    return Task.FromResult(formattedDuration);
                }
            }
            catch (Exception ex)
            {

                Console.WriteLine($"Ne gre žau {ex}");
                throw;
            }
        }
    }

   
        [Obsolete("Prikaz za upload file-ov")] 
        private async Task Prikazi()
        {
            try
            {
                var fileDialog = new OpenFileDialog();
                fileDialog.Title = "Izberite file";
                fileDialog.Filters.Add(new FileDialogFilter { Name = "file", Extensions = { "mp3", "wav", "ogg","MP3" } });

                var izbraniFile = await fileDialog.ShowAsync(this);
                if (izbraniFile != null && izbraniFile.Length > 0)
                {
                    foreach (var file in izbraniFile)
                    {
                        //Dodajanje file v databazo in v file kjer ga hrani
                        var naslov = Path.GetFileNameWithoutExtension(file);
                        var dolzina = await Audio.GetAudioFileLength(file);
                        var userID = MainWindow.userId;
                        //=================================================================================
                        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file);
                        var destinacija = Path.Combine(uploadFolder, fileName);
                        File.Copy(file, destinacija, true);
                        //================================================================================
                        var newMusic = new MusicItem(naslov, dolzina, destinacija,userID);
                        myUploads.Add(newMusic);
                        ShraniVDatabazo(newMusic);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Ne uploda {e}");
                throw;
            }
            
        }
    public void NaloizIzDatabaze()
    {
        using (MySqlConnection connection = new MySqlConnection(_conn))
        {
            connection.Open();
            int userID = MainWindow.userId;
            string sql =
                "SELECT pesmi_id,naslov_pesmi,dolzina_pesmi,file_ext,pesmi.user_id FROM pesmi JOIN user ON pesmi.user_id=user.user_id WHERE user.user_id=@user_id ";
                using (MySqlCommand command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@user_id", userID);
                     using (MySqlDataReader reader =  command.ExecuteReader())
                    {
                        
                        while (reader.Read())
                        {
                            var musicItem = new MusicItem(
                                reader.GetInt32("pesmi_id"),
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
         
           using (MySqlConnection connection = new MySqlConnection(_conn))
           {
               connection.Open();
               string sql = "INSERT INTO pesmi(naslov_pesmi,dolzina_pesmi,file_ext,user_id) VALUES(@naslov_pesmi,@dolzina_pesmi,@file_ext,@user_id)";
               using (MySqlCommand command = new MySqlCommand(sql, connection))
               {
                   command.Parameters.AddWithValue("@naslov_pesmi", musicItem.Naslov);
                   command.Parameters.AddWithValue("@dolzina_pesmi", musicItem.Dolzina);
                   command.Parameters.AddWithValue("@file_ext", musicItem.Destinacija);
                   command.Parameters.AddWithValue("user_id", musicItem.UserId);
                   command.ExecuteNonQuery();
               }

               using ( MySqlConnection konekcija = new MySqlConnection(_conn))
               {    konekcija.Open();
                   string query = "SELECT pesmi_id FROM pesmi WHERE user_id=@user_id AND naslov_pesmi = @naslov_pesmi AND dolzina_pesmi = @dolzina_pesmi AND file_ext = @file_ext  ";
                   using (MySqlCommand komanda = new MySqlCommand(query,konekcija))
                   {
                       komanda.Parameters.AddWithValue("@naslov_pesmi", musicItem.Naslov);
                       komanda.Parameters.AddWithValue("@dolzina_pesmi", musicItem.Dolzina);
                       komanda.Parameters.AddWithValue("@file_ext", musicItem.Destinacija);
                       komanda.Parameters.AddWithValue("@user_id", musicItem.UserId);
                       try
                       {
                           using (MySqlDataReader reader = komanda.ExecuteReader())
                           {
                               
                               while (reader.Read())
                               {
                                   musicItem.PesmiID = reader.GetInt32("pesmi_id");
                                   
                                   if (reader != null)
                                   {
                                       Console.WriteLine(musicItem.PesmiID);
                                   }
                                   else
                                   {
                                       Console.WriteLine("ne prebran id");
                                   }
                               }
                           }
                       }
                       catch (Exception e)
                       {
                           Console.WriteLine($"idiiii {e.Message} ");
                           throw;
                       }
                     
                     
                   }
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
    

    private void Logout_OnClick(object? sender, RoutedEventArgs e)
    {
        SignedIn = false;
        ShowProfile();
        PobrisiUplode();
        _addPlaylist.PobrisiPlaylist();
        Reacently.Clear();
        Collebanje.Clear();
        CreatePlaylistButton.IsVisible = false;
        uploadButton.IsVisible = false;
        
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
    private void UpdatePlayProgressAndLength(double currentProgress, double totalLength)
    {
        PlayProgress = currentProgress;
        PlayDolzina = totalLength;
    }
    public new event PropertyChangedEventHandler? PropertyChanged;
    
    protected virtual void RaisePropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));   
    }
//==============================================================================================================================================
//Naprej, nazaj, start, stop, player volume up and down
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

    private async Task PlayNext()
    {
        if (playlist_id != null)
        {
           await PlayPlaylistNext();
        }
        else
        {
            if (_trenutniTrack < myUploads.Count - 1)
            {
                if (outputDevice != null && outputDevice.PlaybackState == PlaybackState.Playing)
                {
                    outputDevice.Stop();
                    outputDevice.Dispose();
                }
                _trenutniTrack++;
                await PlaySelectedTrack();
            
            }
        }
       
    }

    private async Task PlayPlaylistNext()
    {
        if (_trenutniTrack < myPlayListsSongs.Count - 1)
        {
            if (outputDevice != null && outputDevice.PlaybackState == PlaybackState.Playing)
            {
                outputDevice.Stop();
                outputDevice.Dispose();
            }
            _trenutniTrack++;
            await PlaySelectedTrack();
            
        }
    }
    
    private async Task PlayPrevious()
    {
        if (_trenutniTrack > 0)
        {
            if (outputDevice.PlaybackState == PlaybackState.Playing)
            {
              outputDevice.Stop();
              outputDevice.Dispose();
            }
            _trenutniTrack--;
           await PlaySelectedTrack();
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
    private void Stop_OnClick(object? sender, RoutedEventArgs e)
    {
        outputDevice.Stop(); 
        outputDevice.Dispose();
        IsProgressBarEnabled = false;
    }
    private async Task PlaySelectedTrack()
    {
        
            if (_trenutniTrack >= 0 && _trenutniTrack < myUploads.Count)
            {
                var filePath = Path.Combine(uploadFolder, myUploads[_trenutniTrack].Destinacija);
                if (File.Exists(filePath))
                {
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
            else
            {
                throw new Exception("ne dela");
            }
            await Task.Delay(1000);
        }
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
    private bool _isProgressBarEnabled = true;
    public bool IsProgressBarEnabled
    {
        get { return _isProgressBarEnabled; }
        set
        {
            _isProgressBarEnabled = value;
            RaisePropertyChanged(nameof(IsProgressBarEnabled));
        }
    }
  
    private void VolumeSlider_OnValueChanged(object? sender, RangeBaseValueChangedEventArgs e)
    {
        VolumeLevel = (float)VolumeSlider.Value;
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
    private async Task PlayAudio(string filePath)
    {
        try
        {
           
          await  using (var audioFile= new AudioFileReader(filePath))
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
                  UpdatePlayProgressAndLength(PlayProgress,PlayDolzina);
                  await Task.Delay(200);
              }
          }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            throw;
        }
    }
    
 
    private void Play_OnClick(object? sender, RoutedEventArgs e)
    {
        
        
        var button = sender as Button;
        var musicItem = button.Tag as MusicItem;
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
                Console.WriteLine($"File ne obstaja{filePath}");
            }
        }
    }

    
    
//==================================================================================================================================
//Kreiraj playlisto
  
    private void CreatePlaylistButton_OnClick(object? sender, RoutedEventArgs e)
    {
        var addPlaylist = new AddPlaylist(this, _playlist);
        addPlaylist.Show();
    }
//=================================================================================================================================
//V playlistu
    private void OpenPlaylist_OnClick(object? sender, RoutedEventArgs e)
    {
        BorderUploads.IsVisible = false;
        mewo.IsVisible = false;
        played.IsVisible = false;
        playlist.IsVisible = true;
        myPlayListsSongs.Clear();
        PlayListSongs.ItemsSource = myPlayListsSongs;
        if (sender is Button button && button.Tag is PlayList playList)
        {
            int? playlist_id = playList.PlayListId; //Treba je pridobiti id playliste ki je povezana (taggana) na  button
            _playlist.NaloziPlaylisto(playlist_id);
            _addPlaylist.UpdateTime(playlist_id);
            _addPlaylist.IzpisiPlayliste();
            Console.WriteLine($"{playlist_id},{_playlist.UserId}");
        }
        else
        {
            Console.WriteLine("ne dewa :(");
        }
    }
    private void OpenCollabList_OnClick(object? sender, RoutedEventArgs e)
    {
        BorderUploads.IsVisible = false;
        mewo.IsVisible = false;
        played.IsVisible = false;
        playlist.IsVisible = true;
        myPlayListsSongs.Clear();
        PlayListSongs.ItemsSource = myPlayListsSongs;
        if (sender is Button button && button.Tag is PlayListItem playList)
        {
            int playlist_id = playList.PlaylistCollab; //Treba je pridobiti id playliste ki je povezana (taggana) na  button
            _playlist.NaloziPlaylisto(playlist_id);
            _addPlaylist.UpdateTimeC(playlist_id);
            Console.WriteLine($"{playlist_id}");
        }
        else
        {
            Console.WriteLine("ne dewa :(");
        }
    }
    private void Nazaj_OnClick(object? sender, RoutedEventArgs e)
    {
        BorderUploads.IsVisible = true;
        mewo.IsVisible = true;
        played.IsVisible = true;
        playlist.IsVisible = false;
        _addPlaylist.PrikaziReacent();
    }
//==================================================================================================================================
// Dodaj pesm v playlist
   // private MusicItem _selectedMusicItem;

   private int playlist_id;

    private void AddToSelectedPlaylist_OnClick(object? sender, RoutedEventArgs e)
    {
           if (sender is Button button &&  button.DataContext is PlayList buttonTag)
           {
               playlist_id = buttonTag.PlayListId;
               Console.WriteLine(playlist_id);
               _playlist.DodajvPlaylisto (playlist_id);
           }
           else
           {
               Console.WriteLine("Button ne deluje pravilno");
           }
    }

    public int PESMI;
    private void Playlisti_OnExpanded(object? sender, RoutedEventArgs e)
    {
        if (sender is Expander button && button.Tag is MusicItem musicItem)
        {
            PESMI = musicItem.PesmiID;
            Console.WriteLine("dela");
            Console.WriteLine(PESMI);
        }
    }
    //================================================================================================================================
    //Dostopanje do childa
        public static ListBox FindListBoxByName(string name, ListBox parentListBox)
        {
            if (parentListBox.Name == name)
            {
                return parentListBox;
            }
            //Da gre skozi iteme znotri Listboxa Uploads
            foreach (var item in parentListBox.Items)
            {
                if (item is ListBoxItem listBoxItem)
                {
                    // Da najde listbox playListListBox
                    var parent = VisualExtensions.GetVisualParent(listBoxItem);
                    while (parent != null && !(parent is ListBox))
                    {
                        parent = VisualExtensions.GetVisualParent(parent);
                    }
                    if (parent is ListBox listBox && listBox.Name == name)
                    {
                        return listBox;
                    }
                }
            }
            return null;
        }
 //=================================================================================================================
 //Dodajanje novega uporabnika v playlisto
        private void Expander_OnExpanded(object? sender, RoutedEventArgs e) 
        {
            _playlist.NaloziUporabnike();
        }
        
        private void AddUser_OnClick(object? sender, RoutedEventArgs e)
        {
            if(sender is Button button && button.Tag is PlayListItem playListItem )
            {
                int userID = playListItem.UporabnikID;
                Console.WriteLine(userID);
                _playlist.DodajUporabnika(userID); //Uporabi oz. uncommenti ko pogruntas query za collabanje
            }
            else
            {
                Console.WriteLine("nedeluje");
            }
        }
//=================================================================================================================
//Downloadanje songov
    private async Task DownloadFile(IEnumerable<string> fileNames)
    {
        SaveFileDialog saveFileDialog = new SaveFileDialog();
        saveFileDialog.Title = "Shrani pesem";
        foreach (var fileName in fileNames)
        {
            var filePath = Path.Combine(uploadFolder, fileName);
       
            if (File.Exists(filePath))
            {
                saveFileDialog.InitialFileName = Path.GetFileName(filePath);
                var saveFilePath = await saveFileDialog.ShowAsync(this);
                if (!string.IsNullOrEmpty(saveFilePath))
                {
                    File.Copy(filePath, saveFilePath, true);
                }
            }
            else
            {
                Console.WriteLine($"File does not exist: {filePath}");
            }
        }
        
    }
  
    private void Download_OnClick(object? sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is MusicItem musicItem)
        {
            _musicItem = musicItem;
             var selectedFiles = new List<string> { _musicItem.Destinacija };
             DownloadFile(selectedFiles);
        }
    }
 
}




