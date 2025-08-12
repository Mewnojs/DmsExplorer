using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using System.Collections.Generic;

namespace DmsExplorer.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();
    }

    public void TreeDataGrid_DoubleTapped(object sender, TappedEventArgs args) 
    {
        //DataContext.
    }
}
