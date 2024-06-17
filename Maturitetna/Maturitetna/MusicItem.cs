namespace Maturitetna;

public class MusicItem
{
   
    public int PesmiID { get; set; }
    public string Naslov { get; set; }
    public string Dolzina { get; set; }
    public string Destinacija { get; }
    public int PlaylistId { get; set; }
    public string ImePlaylista { get; set; }

    public int UserId
    {
        get { return MainWindow.userId; }
        set { MainWindow.userId = value; }
    }

    public MusicItem()
    {
    }
    
public MusicItem(int pesmiId, string naslov, string dolzina, string destinacija, int userId) : this(naslov,
            dolzina, destinacija, userId)
        {
            PesmiID = pesmiId;
        }

        public MusicItem(string imePlaylista, int playlistId)
        {
            ImePlaylista = imePlaylista;
            PlaylistId = playlistId;
        }
        public MusicItem( string naslov, string dolzina, string destinacija, int userId)
        {
        
            Naslov = naslov;
            Dolzina = dolzina;
            Destinacija = destinacija;
            UserId = userId;
            
        }
    
}