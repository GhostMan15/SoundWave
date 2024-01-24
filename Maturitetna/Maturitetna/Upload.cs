namespace Maturitetna;

public class Upload
{
    private bool SignedIn = false;
    private ObservableCollection<MusicItem> myUploads = new ObservableCollection<MusicItem>();
    private string  conn = "Server=localhost;Database=maturitetna;Uid=root;Pwd=root;"; private async Task Prikazi() {
        var fileDialog = new OpenFileDialog();
        fileDialog.Title = "Izberite file";
        fileDialog.Filters.Add(new FileDialogFilter{Name = "file", Extensions = {"mp3","wav","ogg"}});

        var izbraniFile = await fileDialog.ShowAsync(this);
        if (izbraniFile != null && izbraniFile.Length > 0)
        {
            foreach (var file in izbraniFile)
            {
                var naslov = System.IO.Path.GetFileNameWithoutExtension(file); 
                var dolzina = "Neznana";
                var newMusic = new MusicItem( 0, naslov,dolzina);
                myUploads.Add(newMusic);
                ShraniVDatabazo(newMusic);
            }
        }
    }

    private void NaloizIzDatabaze()
    {
      
        using (MySqlConnection connection = new MySqlConnection(conn) )
        {
            connection.Open();
            string sql = "SELECT pesmi_id ,naslov_pesmi, dolzina_pesmi FROM  pesmi";
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
            string sql = "INSERT INTO pesmi(pesmi_id,naslov_pesmi,dolzina_pesmi) VALUES(@pesmi_id, @naslov_pesmi,@dolzina_pesmi)";
            using (MySqlCommand command = new MySqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@pesmi_id", musicItem.Pesmi_id);
                command.Parameters.AddWithValue("@naslov_pesmi", musicItem.Naslov);
                command.Parameters.AddWithValue("@dolzina_pesmi", musicItem.Dolzina);
                command.ExecuteNonQuery();
            }
        }
    }

}