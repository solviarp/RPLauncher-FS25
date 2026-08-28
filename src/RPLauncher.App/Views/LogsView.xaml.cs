using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using RPLauncher.Core.Logging;

namespace RPLauncher.App.Views;

public partial class LogsView : UserControl
{
    public LogsView()
    {
        InitializeComponent();
        Render();
        Logger.EntryLogged += _ => Dispatcher.Invoke(Render);
    }

    private void Render()
    {
        LogItems.Items.Clear();
        foreach (var entry in Logger.GetRecent().Reverse())
        {
            var color = entry.Level switch
            {
                LogLevel.Error => (Brush)FindResource("AccentRed"),
                LogLevel.Warning => (Brush)FindResource("AccentAmber"),
                _ => (Brush)FindResource("TextSecondary")
            };

            LogItems.Items.Add(new TextBlock
            {
                Text = $"[{entry.Timestamp:HH:mm:ss}] {entry.Message}",
                Foreground = color,
                Margin = new Thickness(0, 0, 0, 4),
                TextWrapping = TextWrapping.Wrap
            });
        }
    }

    private void OpenFolder_Click(object sender, RoutedEventArgs e)
    {
        if (Logger.CurrentLogFile is null) return;
        var directory = System.IO.Path.GetDirectoryName(Logger.CurrentLogFile);
        if (directory is not null)
        {
            Process.Start(new ProcessStartInfo { FileName = directory, UseShellExecute = true });
        }
    }
}
