using System.Collections.ObjectModel;
using System.Threading.Tasks;
using DynamicData;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.VisualElements;
using ReactiveUI;
using SkiaSharp;

namespace AvaloniaTests.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
#pragma warning disable CA1822 // Mark members as static
    public string Greeting => "Welcome to Avalonia!";
#pragma warning restore CA1822 // Mark members as static
    public ISeries[] Series { get; set; } =
    {
        new LineSeries<double>
        {
            Values = new double[] { 2, 1, 3, 5, 3, 4, 6 },
            Fill = null
        }
    };

    public LabelVisual Title { get; set; } =
        new LabelVisual
        {
            Text = "My chart title",
            TextSize = 25,
            Padding = new LiveChartsCore.Drawing.Padding(15),
            Paint = new SolidColorPaint(SKColors.DarkSlateGray)
        };

    public ObservableCollection<string> Items { get; } = ["One", "Two"];

    private string _selectedItem;
    public string SelectedItem
    {
        get => _selectedItem;
        set => this.RaiseAndSetIfChanged(ref _selectedItem, value);
    }

    public async void Test()
    {
        if (Items.Count > 0)
        {
            await Task.Delay(100);
            Items.Clear();
            return;
        }
        
    }

    public void Restore()
    {
        if (Items.Count > 0) return;
        Items.AddRange(["One", "Two"]);
    }
}