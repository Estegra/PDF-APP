using System.Configuration;
using System.Data;
using System.Windows;

namespace PDF_APP
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            // Encoding düzeltmesi (Türkçe karakter ve PDF okuma için)
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            base.OnStartup(e);
        }
    }

}
