using System;
using System.Data;
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
    private const string conn = "Server=localhost;Database=maturitetna;Uid=root;Pwd=root;";
    private  int userId;
    private readonly MainWindow _mainWindow;
    
    public Login(MainWindow mainWindow)
    {
        InitializeComponent();
        _mainWindow = mainWindow;
        _mainWindow.Uploads.ItemsSource = _mainWindow.myUploads;
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

                string sql = "SELECT user_id FROM user  WHERE username = @username AND password = @password";
              
                using (MySqlCommand command = new MySqlCommand(sql, connection))
                {
                    
                    command.Parameters.AddWithValue("@username", username);
                    command.Parameters.AddWithValue("@password", password);

                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            userId = reader.GetInt32("user_id");
                            MainWindow.userId = reader.GetInt32("user_id");
                            this.Close();
                            _mainWindow.ShowProfile();
                            _mainWindow.NaloizIzDatabaze();
                        }
                        else
                        {
                            Username.Text = "";
                            Password.Text = "";
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

    public int GetUserId()
    {
        return userId;
    }
}