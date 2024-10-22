namespace Maturitetna;

public class PlayList
{
    public string ImePlaylista { get; set; }
    public int PlayListId { get; set; }
    public int Privacy { get; set; }
    public int UserId { get; set; }
    public string Ustvarjeno { get; set; }
    public string DatumDostopa { get; set; }

    public PlayList() { }


    public int? CollabID { get; set; }
    public string? DatumDostopaC { get; set; }

    public PlayList(string imePlaylista, int playListId, int userId, string datumDostopa, string datumDostopaC)
    {
        ImePlaylista = imePlaylista;
        PlayListId = playListId;
        UserId = userId;
        DatumDostopa = datumDostopa;
        DatumDostopaC = datumDostopaC;
    }
}
