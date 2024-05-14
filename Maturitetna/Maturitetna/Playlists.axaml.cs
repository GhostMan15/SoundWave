using System;
using System.Collections.Generic;
using System.Data;
using Avalonia.Controls;
using DynamicData;
using MySqlConnector;

namespace Maturitetna;

public partial class Playlists : Window
{
    public Playlists()
    {
        InitializeComponent();
        DataContext = this;
    }
}
