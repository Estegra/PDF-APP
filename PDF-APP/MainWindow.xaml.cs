using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;
using System.Windows.Media.Animation;

namespace PDF_APP

{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : FluentWindow
    {
        private PdfMotoru _motor = new PdfMotoru();

        public MainWindow()
        {
            SystemThemeWatcher.Watch(this);

            InitializeComponent();
        }

        
        private void MergeZone_Drop(object sender, DragEventArgs e)
        {

            //Bırakılan verinin dosya olup olmadığını kontrol et
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] dosyalar = (string[])e.Data.GetData(DataFormats.FileDrop);

                SaveFileDialog saveDialog = new SaveFileDialog();
                saveDialog.Filter = "PDF Dosyası (*.pdf)|*.pdf";
                saveDialog.FileName = "Birlestirilmis.pdf";

                if (saveDialog.ShowDialog() == true)
                {
                    try
                    {
                        // MOTORU ÇALIŞTIR
                        _motor.Birlestir(dosyalar, saveDialog.FileName);
                        PlaySuccessAnimation(this.MergeSuccessOverlay);
                    }
                    catch (Exception ex)
                    {
                        System.Windows.MessageBox.Show("Hata oluştu: " + ex.Message, "Hata", System.Windows.MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void SplitZone_Drop(object sender, DragEventArgs e)
        {

            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] dosyalar = (string[])e.Data.GetData(DataFormats.FileDrop);
                string islenecekDosya = dosyalar[0];

                SaveFileDialog saveDialog = new SaveFileDialog();
                saveDialog.Title = "Kaydedilecek yeri ve dosya ön ekini seçin";
                saveDialog.Filter = "PDF Dosyası (*.pdf)|*.pdf";
                saveDialog.FileName = "Parca_";

                if (saveDialog.ShowDialog() == true)
                {
                    try
                    {
                        // MOTORU ÇALIŞTIR
                        _motor.Parcala(islenecekDosya, saveDialog.FileName);
                        PlaySuccessAnimation(this.SplitSuccessOverlay);
                    }
                    catch (Exception ex)
                    {
                        System.Windows.MessageBox.Show("Hata oluştu: " + ex.Message, "Hata", System.Windows.MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }
        // BAŞARI ANİMASYONUNU OYNATAN YARDIMCI FONKSİYON
        private void PlaySuccessAnimation(Border overlayTarget)
        {
            // Opaklığı (Görünürlüğü) değiştiren bir animasyon tanımlıyoruz
            DoubleAnimation flashAnimation = new DoubleAnimation();
            flashAnimation.From = 0.0;          // Tamamen şeffaf başla
            flashAnimation.To = 0.8;            
            flashAnimation.Duration = new Duration(TimeSpan.FromSeconds(0.8)); 
            flashAnimation.AutoReverse = true;  // İşlem bitince otomatik geri dön (sön)

            // Animasyonu ilgili katmanın Opacity özelliğine uygula ve başlat
            overlayTarget.BeginAnimation(Border.OpacityProperty, flashAnimation);
        }
    }
}
