using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using MySqlConnector;

namespace Maturitetna;

public partial class Register : Window
{
    private readonly string _conn;
    public Register()
    {
        InitializeComponent();
        var reader = new AppSettingsReader("appsettings.json");
        _conn = reader.GetStringValue("ConnectionStrings:MyConnectionString");
    }

    private void Register_OnClick(object? sender, RoutedEventArgs e)
    {
        string username = Username.Text;
        string password = Password.Text;
        string reenter = Reenter.Text;
        if (string.IsNullOrEmpty(username) ||   string.IsNullOrEmpty(password) || string.IsNullOrEmpty(reenter))
        {
            Username.Text = "";
            Password.Text = "";
            Reenter.Text = ""; 
            error.IsVisible = true;
            return;
        } 

        try
        {
            using (MySqlConnection connection = new MySqlConnection(_conn))
            {
                connection.Open();
                const string sql = "INSERT INTO user(username,password) VALUES (@username, @password)";
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
