using Microsoft.Win32;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;

namespace PDF_APP
{
    public partial class MainWindow : FluentWindow
    {
        private readonly PdfMotoru _motor = new();
        private ToolMode _mode = ToolMode.Convert;

        public MainWindow()
        {
            SystemThemeWatcher.Watch(this);
            InitializeComponent();
        }

        private void ConvertButton_Click(object sender, RoutedEventArgs e) => SetMode(ToolMode.Convert);
        private void MergeButton_Click(object sender, RoutedEventArgs e) => SetMode(ToolMode.Merge);
        private void SplitButton_Click(object sender, RoutedEventArgs e) => SetMode(ToolMode.Split);

        private void SetMode(ToolMode mode)
        {
            _mode = mode;
            ConvertButton.Background = mode == ToolMode.Convert ? ActiveButtonBrush() : Brushes.Transparent;
            MergeButton.Background = mode == ToolMode.Merge ? ActiveButtonBrush() : Brushes.Transparent;
            SplitButton.Background = mode == ToolMode.Split ? ActiveButtonBrush() : Brushes.Transparent;

            switch (mode)
            {
                case ToolMode.Convert:
                    PageTitle.Text = "Dosyaları dönüştür";
                    PageDescription.Text = "JPG, PNG veya DOCX dosyalarını PDF'e çevirin.";
                    DropTitle.Text = "Dosyaları buraya sürükleyin";
                    DropDescription.Text = "JPG, PNG veya DOCX";
                    break;
                case ToolMode.Merge:
                    PageTitle.Text = "PDF dosyalarını birleştir";
                    PageDescription.Text = "PDF'leri seçtiğiniz sırayla tek bir dosyada birleştirin.";
                    DropTitle.Text = "PDF dosyalarını buraya sürükleyin";
                    DropDescription.Text = "Birden fazla PDF seçebilirsiniz";
                    break;
                case ToolMode.Split:
                    PageTitle.Text = "PDF dosyasını parçala";
                    PageDescription.Text = "Her sayfayı ayrı bir PDF dosyası olarak kaydedin.";
                    DropTitle.Text = "Bir PDF dosyasını buraya sürükleyin";
                    DropDescription.Text = "Her sayfa ayrı dosya olarak kaydedilir";
                    break;
            }
        }

        private static Brush ActiveButtonBrush() =>
            new SolidColorBrush(Color.FromArgb(34, 0, 120, 212));

        private void SelectFiles_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Multiselect = _mode != ToolMode.Split,
                Filter = _mode switch
                {
                    ToolMode.Convert => "Desteklenen dosyalar (*.jpg;*.jpeg;*.png;*.docx)|*.jpg;*.jpeg;*.png;*.docx|Tüm dosyalar (*.*)|*.*",
                    _ => "PDF dosyaları (*.pdf)|*.pdf"
                }
            };

            if (dialog.ShowDialog() == true)
                ProcessFiles(dialog.FileNames);
        }

        private void DropZone_Drop(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop) ||
                e.Data.GetData(DataFormats.FileDrop) is not string[] files ||
                files.Length == 0)
            {
                return;
            }

            ProcessFiles(files);
        }

        private void ProcessFiles(string[] files)
        {
            try
            {
                switch (_mode)
                {
                    case ToolMode.Convert:
                        ConvertFiles(files);
                        break;
                    case ToolMode.Merge:
                        MergeFiles(files);
                        break;
                    case ToolMode.Split:
                        SplitFile(files[0]);
                        break;
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(
                    "İşlem tamamlanamadı: " + ex.Message,
                    "Hata",
                    System.Windows.MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void ConvertFiles(string[] files)
        {
            var extensions = files
                .Select(file => Path.GetExtension(file)?.ToLowerInvariant() ?? string.Empty)
                .ToArray();
            var imageExtensions = new[] { ".jpg", ".jpeg", ".png" };

            if (extensions.All(imageExtensions.Contains))
            {
                var dialog = CreatePdfSaveDialog("Images.pdf");
                if (dialog.ShowDialog() == true)
                {
                    _motor.ResimleriPdfYap(files, dialog.FileName);
                    PlaySuccessAnimation();
                }
                return;
            }

            if (files.Length == 1 && extensions[0] == ".docx")
            {
                var dialog = CreatePdfSaveDialog(Path.GetFileNameWithoutExtension(files[0]) + ".pdf");
                if (dialog.ShowDialog() == true)
                {
                    _motor.DocxPdfYap(files[0], dialog.FileName);
                    PlaySuccessAnimation();
                }
                return;
            }

            throw new InvalidOperationException("JPG, PNG dosyaları birlikte veya tek bir DOCX dosyası seçin.");
        }

        private void MergeFiles(string[] files)
        {
            if (files.Any(file => !string.Equals(Path.GetExtension(file), ".pdf", StringComparison.OrdinalIgnoreCase)))
                throw new InvalidOperationException("Birleştirmek için yalnızca PDF dosyaları seçin.");

            var dialog = CreatePdfSaveDialog("Birlestirilmis.pdf");
            if (dialog.ShowDialog() == true)
            {
                _motor.Birlestir(files, dialog.FileName);
                PlaySuccessAnimation();
            }
        }

        private void SplitFile(string file)
        {
            if (!string.Equals(Path.GetExtension(file), ".pdf", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Parçalamak için bir PDF dosyası seçin.");

            var dialog = new SaveFileDialog
            {
                Title = "Kaydedilecek yeri ve dosya ön ekini seçin",
                Filter = "PDF dosyası (*.pdf)|*.pdf",
                FileName = "Parca_.pdf"
            };

            if (dialog.ShowDialog() == true)
            {
                _motor.Parcala(file, dialog.FileName);
                PlaySuccessAnimation();
            }
        }

        private static SaveFileDialog CreatePdfSaveDialog(string fileName) => new()
        {
            Filter = "PDF dosyası (*.pdf)|*.pdf",
            FileName = fileName,
            DefaultExt = ".pdf",
            AddExtension = true
        };

        private void PlaySuccessAnimation()
        {
            var animation = new DoubleAnimation
            {
                From = 0.0,
                To = 0.8,
                Duration = new Duration(TimeSpan.FromSeconds(0.8)),
                AutoReverse = true
            };

            SuccessOverlay.BeginAnimation(OpacityProperty, animation);
        }

        private enum ToolMode
        {
            Convert,
            Merge,
            Split
        }
    }
}
