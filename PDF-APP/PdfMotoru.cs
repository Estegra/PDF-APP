using System;
using System.Collections.Generic;
using System.Reflection.PortableExecutable;
using System.Text;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using System.IO;

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
