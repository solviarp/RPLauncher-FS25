using System.Windows;
using RPLauncher.Core.Configuration;
using RPLauncher.Core.Logging;

namespace RPLauncher.App;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var configService = new ConfigService();
        Logger.Initialize(configService.LogsDirectory);
        Logger.Info("RP Launcher démarré.");

        DispatcherUnhandledException += (_, args) =>
        {
            Logger.Error("Exception non gérée", args.Exception);
            MessageBox.Show(
                $"Une erreur inattendue est survenue :\n{args.Exception.Message}\n\nConsultez l'onglet Logs pour plus de détails.",
                "RP Launcher",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            args.Handled = true;
        };
    }
}
