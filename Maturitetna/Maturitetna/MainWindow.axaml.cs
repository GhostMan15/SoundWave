using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using Avalonia.VisualTree;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using MySqlConnector;
using NAudio.Wave;
using VisualExtensions = Avalonia.VisualTree.VisualExtensions;
namespace Maturitetna;
public partial class  MainWindow:Window,INotifyPropertyChanged
{
    private  bool SignedIn;
    public ObservableCollection<MusicItem> myUploads { get; }= new ObservableCollection<MusicItem>();
    public ObservableCollection<PlayList> myPlaylist { get; set; } = new ObservableCollection<PlayList>();
    public ObservableCollection<PlayList> AllPlaylists { get; set; } = new ObservableCollection<PlayList>();
    public ObservableCollection<PlayListItem> myPlayListsSongs { get; } = new ObservableCollection<PlayListItem>();
    public ObservableCollection<PlayList> PublicPlayLists { get; } = new ObservableCollection<PlayList>();
    public ObservableCollection<string> DodajUporabnika { get; } = new ObservableCollection<string>();
    private string  conn = "Server=localhost;Database=maturitetna;Uid=root;Pwd=root;";
    private string uploadFolder = "C:\\Users\\faruk\\Documents\\GitHub\\Maturitetna\\Muska";
    private static  Login _login;
    public static int userId;
    private readonly AddPlaylist _addPlaylist;
    private readonly PlayListItem _playlist;
    private readonly PlayList _onlyplaylist;
    private  MusicItem _musicItem;
    //private PlayList _song;


    public string Username
    {
        get { return PlayListItem.username; }
        set { PlayListItem.username = value; }
    }
    
    public MainWindow()
    {
        InitializeComponent();
        _login = new Login(this, _addPlaylist);
        _musicItem = new MusicItem();
        _onlyplaylist = new PlayList();
        _addPlaylist = new AddPlaylist(this, _playlist );
        _playlist = new PlayListItem(this, _musicItem);
         DataContext = this;
         _addPlaylist.IzpisiPlaylistePublic();

    }
    

    public MainWindow(Login login, AddPlaylist addplaylist, PlayListItem playlist, PlayList onlyplaylist, MainWindow.MusicItem musicItem) : this()
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
    public class MusicItem 
    {
        public int PesmiID { get; set; }
        public string Naslov { get; set; }
        public string Dolzina { get; set; }
        public string Destinacija { get; }
      
        public int UserId
        {
            get { return userId;  }
            set { userId = value; }
        }

      
        public MusicItem(){}

        public MusicItem(int pesmiId, string naslov, string dolzina, string destinacija, int userId) : this(naslov,
            dolzina, destinacija, userId)
        {
            PesmiID = pesmiId;
        }
        
        public MusicItem( string naslov, string dolzina, string destinacija, int userId)
        {
        
            Naslov = naslov;
            Dolzina = dolzina;
            Destinacija = destinacija;
            UserId = userId;
            
        }
    }
  //=======================================================================================================================
  // Za Pridobijanje ta pravega playlista in tapravega songa
  
  
 /* private PlayListItem _selectedMusicItem;
  public PlayListItem SelectedMusicItem
  {
      get { return _selectedMusicItem; }
      set
      {
          _selectedMusicItem = value;
          // Update the playlist of the selected item
          _selectedMusicItem?.UpdatePlaylist(SelectedPlaylist);
          OnPropertyChanged(nameof(SelectedMusicItem));
      }
  }

  private PlayListItem _selectedPlaylist;
  public PlayListItem SelectedPlaylist
  {
      get { return _selectedPlaylist; }
      set
      {
          _selectedPlaylist = value;
          // Update the playlist of the selected item
          SelectedMusicItem?.UpdatePlaylist(_selectedPlaylist);
          OnPropertyChanged(nameof(SelectedPlaylist));
      }
  }

 
  public void OnPropertyChanged(string propertyName)
  {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
  }*/
   //=======================================================================================================================
 //Login/Upload buttons

    private void Button_OnClick(object? sender, RoutedEventArgs e)
    {
        var login = new Login(this, _addPlaylist);
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
            var fileDialog = new OpenFileDialog();
            fileDialog.Title = "Izberite file";
            fileDialog.Filters.Add(new FileDialogFilter { Name = "file", Extensions = { "mp3", "wav", "ogg" } });

            var izbraniFile = await fileDialog.ShowAsync(this);
            if (izbraniFile != null && izbraniFile.Length > 0)
            {
                List<int> dodanSongId = new List<int>();
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
                    dodanSongId.Add(newMusic.PesmiID);
                }
            }
        }
   

    public void NaloizIzDatabaze()
    {
        using (MySqlConnection connection = new MySqlConnection(conn))
        {
            connection.Open();
            int userID = MainWindow.userId;
            string sql =
                "SELECT pesmi_id,naslov_pesmi,dolzina_pesmi,file_ext,pesmi.user_id FROM pesmi JOIN user ON pesmi.user_id=user.user_id WHERE user.user_id=@user_id ";
                using (MySqlCommand command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@user_id", userID);
                    //command.Parameters.AddWithValue("@pesmi_id",PlayListItem.pesmId);
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
         
           using (MySqlConnection connection = new MySqlConnection(conn))
           {
               connection.Open();
               string sql = "INSERT INTO pesmi(naslov_pesmi,dolzina_pesmi,file_ext,user_id) VALUES(@naslov_pesmi,@dolzina_pesmi,@file_ext,@user_id)";
               using (MySqlCommand command = new MySqlCommand(sql, connection))
               {
                   command.Parameters.AddWithValue("@naslov_pesmi", musicItem.Naslov);
                   command.Parameters.AddWithValue("@dolzina_pesmi", musicItem.Dolzina);
                   command.Parameters.AddWithValue("@file_ext", musicItem.Destinacija);
                   command.Parameters.AddWithValue("user_id", musicItem.UserId);
                   //command.ExecuteNonQuery();
                   /*using (MySqlDataReader reader = command.ExecuteReader())
                   {
                       while (reader.Read())
                       {
                           musicItem.PesmiID = reader.GetInt32("pesmi_id");
                           Console.WriteLine(musicItem.PesmiID);
                       }
                   }*/

                   command.ExecuteNonQuery();
               }

               using ( MySqlConnection konekcija = new MySqlConnection(conn))
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
        var musicItem = button?.DataContext as MusicItem;
        var fileName = musicItem?.Destinacija;
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
        Serach.IsVisible = false;
        mewo.IsVisible = false;
        played.IsVisible = false;
        playlist.IsVisible = true;
        myPlayListsSongs.Clear();
        PlayListSongs.ItemsSource = myPlayListsSongs;
        if (Tag is PlayList)
        {
            int playlist_id = (Tag as PlayList).PlayListId; //Treba je pridobiti id playliste ki je povezana (taggana) na  button
            _playlist.NaloziPlaylisto(playlist_id);
        }
        else
        {
            Console.WriteLine("ne dewa :(");
        }
        

    }
    private void Nazaj_OnClick(object? sender, RoutedEventArgs e)
    {
        BorderUploads.IsVisible = true;
        Serach.IsVisible = true;
        mewo.IsVisible = true;
        played.IsVisible = true;
        playlist.IsVisible = false;
    }
//==================================================================================================================================
// Dodaj pesm v playlist

   // private MusicItem _selectedMusicItem;
    private void AddToSelectedPlaylist_OnClick(object? sender, RoutedEventArgs e)
    {
       /* if (_musicItem.PesmiID ==null)
        {
            Console.WriteLine("shit je null ");
            return;
        }
        else
        {
            Console.WriteLine(_musicItem.PesmiID);
        }*/
           /* _playlist.PesmId = SelectedMusicItem.PesmId;
            _playlist.PlaylistId = SelectedPlaylist.PlaylistId;*/
           if (sender is Button button && button.Tag is PlayList playList)
           {
               //button.Click += SongButton_Click; Ne dela (Performanje clicka)
               int playlist_id = playList.PlayListId;
               Console.WriteLine(playlist_id);
              
               SongButton_Click(sender, e);

               if (button.DataContext is MusicItem musicItem)
               {
                   //Izbrani_song(musicItem);
                   _playlist.PesmId = musicItem.PesmiID;
                   Console.WriteLine($"{musicItem.Destinacija}, {musicItem.Dolzina}, {musicItem.Naslov}, {musicItem.PesmiID}, {musicItem.UserId}");
                   _playlist.DodajvPlaylisto(new List<int>(musicItem.PesmiID), playlist_id);  
               }
               else
               {
                   Console.WriteLine("Ni songa");
               }
           }
           else
           {
               Console.WriteLine("Ni buttona.");
           }
    }
    private void SongButton_Click(object sender, RoutedEventArgs e)
    {
       
        HandleButtonClick(sender as Button, e);
    }

    public void Izbrani_song(MusicItem musicItem)
    {
        _musicItem = musicItem;
        var pesmi_id = _musicItem.PesmiID;
        _playlist.PesmId = pesmi_id;
        Console.WriteLine($"{musicItem.Destinacija}, {musicItem.Dolzina}, {musicItem.Naslov}, {musicItem.PesmiID}, {musicItem.UserId}");      
        
    }
    private void HandleButtonClick(Button button, RoutedEventArgs e)
    {
        if (button == null)
        {
            Console.WriteLine("button is null");
            return;
        }
      

        if (button.Tag is MusicItem musicItem)
        {
           Izbrani_song(musicItem);
        }
        else
        {
            Console.WriteLine("button je null");
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

        private Button FindButtonInListBox(string name, ListBox listBox)
        {
            foreach (var item in listBox.Items)
            {
                if (item is ListBoxItem listBoxItem)
                {
                    var button = FindButtonInVisualTree(name, listBoxItem);
                    if (button != null)
                    {
                        return button;
                    }
                }
            }
            return null;
        }

        private Button FindButtonInVisualTree(string name, ListBoxItem listBoxItem)
        {
            var button = listBoxItem.FindControl<Button>(name);
            if (button != null)
            {
                return button;
            }

            // If the button was not found, search recursively in the visual tree
            foreach (var child in listBoxItem.GetVisualChildren())
            {
                if (child is ListBoxItem childListBoxItem)
                {
                    button = FindButtonInVisualTree(name, childListBoxItem);
                    if (button != null)
                    {
                        return button;
                    }
                }
            }

            return null;
        }
 //=================================================================================================================
 //Dodajanje novega uporabnika v playlisto
        private void Expander_OnExpanded(object? sender, RoutedEventArgs e)
        {
            _playlist.DodajUporabnika();
        }

     
}




