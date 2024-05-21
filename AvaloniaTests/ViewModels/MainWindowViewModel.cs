using System.Collections.ObjectModel;
using System.Threading.Tasks;
using DynamicData;

namespace AvaloniaTests.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    public ObservableCollection<string> Items { get; } = ["One", "Two"];

    public async void Test()
    {
        if (Items.Count <= 0) return;
        await Task.Delay(100);
        // This one throws deep down if selected
        Items.Clear();
    }

    public void Restore()
    {
        if (Items.Count > 0) return;
        Items.AddRange(["One", "Two"]);
    }
}