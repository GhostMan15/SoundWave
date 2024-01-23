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
        string email = Email.Text;
        string password = Password.Text;
        string reenter = Reenter.Text;
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(email))
        {
           MessageBox.ShowAsync("Prosim vpišite podatke");
            return;
            
        }

        try
        {
           
            using (MySqlConnection connection = new MySqlConnection(conn))
            {
                connection.Open();
                var sql = "INSERT INTO user(username,email,password) VALUES (@username, @email, @password)";
                using (MySqlCommand command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@username", username);
                    command.Parameters.AddWithValue("@email", email);
                    command.Parameters.AddWithValue("@password", password);

                    // Execute the command
                    command.ExecuteNonQuery();
                    this.Close();   
                }
               
            }
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception);
            throw;
        }
    }
    private void ShowMessage(string message)
    {
        MessageText.Text = message;
        MessageWindow.IsVisible = true;

        // Optionally, hide the message after a certain delay
        Task.Run(async () =>
        {
            await Task.Delay(3000); // 3 seconds delay
            MessageWindow.IsVisible = false;
        });
    }

 
}