using System;
using System.Windows;
using PcKod.UI.Data;

namespace PcKod.UI
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            try
            {
                base.OnStartup(e);

                DatabaseHelper.InitializeDatabase();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Proqram başladılarkən xəta baş verdi.\n\n" +
                    ex.Message + "\n\n" +
                    ex.ToString(),
                    "PcKod POS - Xəta",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                Shutdown();
            }
        }
    }
}