using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using MySql;
using MySqlConnector;
using System.Data.SqlClient;
using ThingLing.Controls;
using MySqlConnection1 = MySql.Data.MySqlClient.MySqlConnection; 
using MySqlCommand1 = MySql.Data.MySqlClient.MySqlCommand; 
using MySqlDataReader1 = MySql.Data.MySqlClient.MySqlDataReader;


namespace Maturitetna;


public partial class Login : Window
{
    private string  conn = "Server=localhost;Database=maturitetna;Uid=root;Pwd=root;";
    public Login()
    {
        InitializeComponent();
    }

    private void Button_OnClick(object? sender, RoutedEventArgs e)
    {
        var register = new Register();
        register.Show();
        this.Close();
    }


    private void SignIn_OnClick(object? sender, RoutedEventArgs e)
    {
        string username = Username.Text;
        string password = Password.Text;
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            MessageBox.ShowAsync("nuh uh");
            return;
        }
        using (MySqlConnection connection = new MySqlConnection(conn))
        {
            connection.Open();
            try
            {
            
                string sql = "SELECT * FROM user  WHERE username = @username AND password = @password";
                using (MySqlCommand command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@username", username);
                    command.Parameters.AddWithValue("@password", password);
                    
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            MessageBox.ShowAsync("Dobro došli");
                            this.Close();
                        }
                        else
                        {
                            MessageBox.ShowAsync("Nepravilno ime ali geslo \n Poskusite še enkrat");
                        }
                        
                    }
                    
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                throw;
            }
        }
      
    }
}