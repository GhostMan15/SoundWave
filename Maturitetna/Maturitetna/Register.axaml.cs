using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Tmds.DBus.Protocol;
using MySqlConnector;
using MySql;
using System.Data.SqlClient;
using ThingLing.Controls;



namespace Maturitetna;

public partial class Register : Window
{
    private string conn = "Server=localhost;Database=maturitetna;Uid=root;Pwd=root;";
   
    public Register()
    {
        InitializeComponent();
    }

    private void Register_OnClick(object? sender, RoutedEventArgs e)
    {
        string username = Username.Text;
        string password = Password.Text;
        string reenter = Reenter.Text;
        if (string.IsNullOrEmpty(username) ||   string.IsNullOrEmpty(password) || string.IsNullOrEmpty(reenter))
        {
            Console.WriteLine("Prosim vpišite podatke");
            return;
        }

        try
        {
            using (MySqlConnection connection = new MySqlConnection(conn))
            {
                connection.Open();
                var sql = "INSERT INTO user(username,password) VALUES (@username, @password)";
                using (MySqlCommand command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@username", username);
                    command.Parameters.AddWithValue("@password", password);
                    
                    command.ExecuteNonQuery();
                    this.Close();   
                    
                }
               
            }
        }
        catch (Exception exception)
        {
            if (reenter != password)
            {
                Password.Text = "";
                Reenter.Text = "";
            }
            else
            {
                Console.WriteLine(exception);
                throw;   
            }
           
        }
    }
}