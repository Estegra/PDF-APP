using System;
using System.Collections.Generic;
using System.Reflection.PortableExecutable;
using System.Text;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using PdfSharp.Drawing;
using System.IO;
using System.Diagnostics;

namespace PDF_APP
{
    internal class PdfMotoru
    {

        public void Birlestir(string[] kaynakDosyalar, string cikisYolu)
        {
            // Çıktı için boş bir PDF oluştur
            using (PdfDocument cikisDokumani = new PdfDocument())
            {
                foreach (string dosyaYolu in kaynakDosyalar)
                {
                    // Kaynak dosyayı "Import" modunda aç (Önemli!)
                    using (PdfDocument girisDokumani = PdfReader.Open(dosyaYolu, PdfDocumentOpenMode.Import))
                    {
                        // O dosyadaki tüm sayfaları tek tek kopyala
                        int sayfaSayisi = girisDokumani.PageCount;
                        for (int i = 0; i < sayfaSayisi; i++)
                        {
                            // Sayfayı yeni dökümana ekle
                            cikisDokumani.AddPage(girisDokumani.Pages[i]);
                        }
                    }
                }
                // Sonucu diske yaz
                cikisDokumani.Save(cikisYolu);
            }
        }

        public void ResimleriPdfYap(string[] kaynakDosyalar, string cikisYolu)
        {
            if (kaynakDosyalar.Length == 0)
                throw new ArgumentException("En az bir JPG veya PNG dosyası seçin.", nameof(kaynakDosyalar));

            using (PdfDocument cikisDokumani = new PdfDocument())
            {
                foreach (string dosyaYolu in kaynakDosyalar)
                {
                    string uzanti = Path.GetExtension(dosyaYolu);
                    if (!uzanti.Equals(".jpg", StringComparison.OrdinalIgnoreCase) &&
                        !uzanti.Equals(".jpeg", StringComparison.OrdinalIgnoreCase) &&
                        !uzanti.Equals(".png", StringComparison.OrdinalIgnoreCase))
                    {
                        throw new ArgumentException($"Desteklenmeyen resim biçimi: {uzanti}");
                    }

                    using (XImage resim = XImage.FromFile(dosyaYolu))
                    {
                        PdfPage sayfa = cikisDokumani.AddPage();
                        sayfa.Width = XUnit.FromPoint(resim.PointWidth);
                        sayfa.Height = XUnit.FromPoint(resim.PointHeight);

                        using (XGraphics grafik = XGraphics.FromPdfPage(sayfa))
                            grafik.DrawImage(resim, 0, 0, sayfa.Width.Point, sayfa.Height.Point);
                    }
                }

                cikisDokumani.Save(cikisYolu);
            }
        }

        public void DocxPdfYap(string kaynakDosya, string cikisYolu)
        {
            if (!string.Equals(Path.GetExtension(kaynakDosya), ".docx", StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("Kaynak dosya DOCX biçiminde olmalıdır.", nameof(kaynakDosya));

            string libreOffice = LibreOfficeYolunuBul();
            string geciciKlasor = Path.Combine(Path.GetTempPath(), "PDF_APP_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(geciciKlasor);

            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = libreOffice,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                startInfo.ArgumentList.Add("--headless");
                startInfo.ArgumentList.Add("--convert-to");
                startInfo.ArgumentList.Add("pdf");
                startInfo.ArgumentList.Add("--outdir");
                startInfo.ArgumentList.Add(geciciKlasor);
                startInfo.ArgumentList.Add(Path.GetFullPath(kaynakDosya));

                using Process process = Process.Start(startInfo)
                    ?? throw new InvalidOperationException("LibreOffice başlatılamadı.");
                process.WaitForExit();

                if (process.ExitCode != 0)
                    throw new InvalidOperationException($"LibreOffice dönüşüm hatası (kod {process.ExitCode}).");

                string geciciPdf = Path.Combine(
                    geciciKlasor,
                    Path.GetFileNameWithoutExtension(kaynakDosya) + ".pdf");

                if (!File.Exists(geciciPdf))
                    throw new InvalidOperationException("LibreOffice PDF dosyasını oluşturamadı.");

                File.Copy(geciciPdf, cikisYolu, true);
            }
            finally
            {
                Directory.Delete(geciciKlasor, true);
            }
        }

        private static string LibreOfficeYolunuBul()
        {
            string programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            string programFilesX86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
            var adaylar = new[]
            {
                Path.Combine(programFiles, "LibreOffice", "program", "soffice.exe"),
                Path.Combine(programFilesX86, "LibreOffice", "program", "soffice.exe")
            };

            foreach (string aday in adaylar)
            {
                if (File.Exists(aday))
                    return aday;
            }

            string? path = Environment.GetEnvironmentVariable("PATH");
            if (!string.IsNullOrWhiteSpace(path))
            {
                foreach (string klasor in path.Split(Path.PathSeparator))
                {
                    string aday = Path.Combine(klasor.Trim('"'), "soffice.exe");
                    if (File.Exists(aday))
                        return aday;
                }
            }

            throw new FileNotFoundException(
                "DOCX dosyalarını PDF'e çevirmek için LibreOffice gerekir. LibreOffice'i kurup tekrar deneyin.");
        }

        // 2. FONKSİYON: PARÇALAMA (SPLIT)
        public void Parcala(string kaynakDosya, string temelIsim)
        {
            // Kaynak dosyayı aç
            using (PdfDocument girisDokumani = PdfReader.Open(kaynakDosya, PdfDocumentOpenMode.Import))
            {
                // Düzeltme 1: string? yaparak null gelebileceğini kabul ediyoruz
                string? klasorYolu = Path.GetDirectoryName(temelIsim);

                // Düzeltme 2: Eğer klasör yolu null gelirse (örn: kök dizin), boş string ata.
                // Bu sayede Path.Combine hata vermez (CS8604 çözümü).
                if (string.IsNullOrEmpty(klasorYolu))
                {
                    klasorYolu = "";
                }

                string? dosyaAdi = Path.GetFileNameWithoutExtension(temelIsim);
                // Dosya adı da null gelirse varsayılan bir isim verelim
                if (string.IsNullOrEmpty(dosyaAdi))
                {
                    dosyaAdi = "Parca";
                }

                for (int i = 0; i < girisDokumani.PageCount; i++)
                {
                    using (PdfDocument tekSayfaDokuman = new PdfDocument())
                    {
                        tekSayfaDokuman.AddPage(girisDokumani.Pages[i]);

                        // Artık klasorYolu kesinlikle null değil, güvenle birleştirebiliriz.
                        string yeniYol = Path.Combine(klasorYolu, $"{dosyaAdi}_Sayfa{i + 1}.pdf");

                        tekSayfaDokuman.Save(yeniYol);
                    }
                }
            }
        }
    }


}
