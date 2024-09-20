using Avalonia.Controls;
using AvaloniaTests.ViewModels;

namespace AvaloniaTests.Views;

public partial class MainWindow : Window
{
    public required MainWindowViewModel ViewModel { get; init; }

    public MainWindow()
    {
        InitializeComponent();
    }
}