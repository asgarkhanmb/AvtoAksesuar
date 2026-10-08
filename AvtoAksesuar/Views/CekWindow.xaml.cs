using Microsoft.Data.Sqlite;
using PcKod.UI.Data;
using PcKod.UI.Models;
using System.Printing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Xps;

namespace PcKod.UI.Views
{
    public partial class CekWindow : Window
    {
        private readonly List<SəbətMəhsul> mehsullar;
        private readonly decimal toplamTutar;
        private readonly string odemeYontemi;
        private readonly DateTime tarix;
        private readonly int cekNo;

        public CekWindow(
            List<SəbətMəhsul> mehsullar,
            decimal toplamTutar,
            string odemeYontemi,
            int cekNo)
        {
            InitializeComponent();

            this.mehsullar = mehsullar;
            this.toplamTutar = toplamTutar;
            this.odemeYontemi = odemeYontemi;
            this.tarix = DateTime.Now;
            this.cekNo = cekNo;

            CekiDoldur();
        }

        // =========================================================
        // ÇEKİ DOLDUR
        // =========================================================

        private void CekiDoldur()
        {
            txtMagazaAdi.Text = "AVTO AKSESUAR";

            txtUnvan.Text =
                "Ünvan: Bakı şəhəri,Yasamal rayonu\n" +
                "2 nömrəli Tibb Kollecinin qarşısı";

            txtTelefon.Text =
                "Tel: 050 443 47 11  |  050 722 70 76";

            txtCekNo.Text =
                "Çek №: " + cekNo.ToString("D6");

            txtTarix.Text =
                "Tarix: " + tarix.ToString("dd.MM.yyyy HH:mm");

            string odeme = odemeYontemi ?? "";

            txtOdeme.Text = odeme.ToUpper();
            txtOdemeAlt.Text = odeme;

            icMhsullar.ItemsSource = mehsullar;

            txtAraCem.Text =
                toplamTutar.ToString("N2") + " AZN";

            grdFerq.Visibility =
                Visibility.Collapsed;

            txtCem.Text =
                toplamTutar.ToString("N2") + " AZN";

            txtAltMelumat.Text =
                "AVTO AKSESUAR";
        }

        // =========================================================
        // YADDA SAXLANMIŞ PRİNTERİ GƏTİR
        // =========================================================

        private string GetSavedPrinter()
        {
            try
            {
                using var db =
                    new SqliteConnection(
                        DatabaseHelper.ConnectionString);

                db.Open();

                using var cmd = db.CreateCommand();

                cmd.CommandText = @"
                    SELECT PrinterAdi
                    FROM Ayarlar
                    WHERE Id = 1
                    LIMIT 1";

                object result = cmd.ExecuteScalar();

                if (result == null ||
                    result == DBNull.Value)
                {
                    return "";
                }

                return result.ToString() ?? "";
            }
            catch
            {
                return "";
            }
        }

        // =========================================================
        // PRİNTERİ YADDA SAXLA
        // =========================================================

        private void SavePrinter(string printerName)
        {
            if (string.IsNullOrWhiteSpace(printerName))
                return;

            try
            {
                using var db =
                    new SqliteConnection(
                        DatabaseHelper.ConnectionString);

                db.Open();

                using var cmd = db.CreateCommand();

                cmd.CommandText = @"
                    INSERT INTO Ayarlar (Id, PrinterAdi)
                    VALUES (1, @printer)

                    ON CONFLICT(Id)
                    DO UPDATE SET PrinterAdi = excluded.PrinterAdi;";

                cmd.Parameters.AddWithValue(
                    "@printer",
                    printerName);

                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Printer yadda saxlanılarkən xəta baş verdi:\n\n" +
                    ex.Message,
                    "Sistem xətası",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        // =========================================================
        // PRİNTERİ TAP
        // =========================================================

        private PrintQueue? FindPrinter(
            LocalPrintServer printServer,
            string printerName)
        {
            try
            {
                foreach (PrintQueue printer
                         in printServer.GetPrintQueues())
                {
                    if (printer.Name.Equals(
                        printerName,
                        StringComparison.OrdinalIgnoreCase))
                    {
                        return printer;
                    }
                }
            }
            catch
            {
                // Printer tapılmadı
            }

            return null;
        }

        // =========================================================
        // ÇAP ET
        // =========================================================

        private void btnCapEt_Click(
            object sender,
            RoutedEventArgs e)
        {
            try
            {
                string savedPrinter =
                    GetSavedPrinter();

                LocalPrintServer printServer =
                    new LocalPrintServer();

                PrintQueue? printer = null;

                // =================================================
                // 1. YADDA SAXLANMIŞ PRİNTERİ AXTAR
                // =================================================

                if (!string.IsNullOrWhiteSpace(savedPrinter))
                {
                    printer = FindPrinter(
                        printServer,
                        savedPrinter);
                }

                // =================================================
                // 2. PRİNTER TAPILMAYIBSA BİR DƏFƏ SEÇ
                // =================================================

                if (printer == null)
                {
                    PrintDialog printDialog =
                        new PrintDialog();

                    bool? netice =
                        printDialog.ShowDialog();

                    if (netice != true)
                        return;

                    printer =
                        printDialog.PrintQueue;

                    if (printer == null)
                    {
                        MessageBox.Show(
                            "Printer seçilmədi.",
                            "Xəbərdarlıq",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);

                        return;
                    }

                    // Printeri yadda saxla
                    SavePrinter(printer.Name);
                }

                // =================================================
                // 3. ÇEKİ ŞƏKİL KİMİ HAZIRLA VƏ ÇAP ET
                // =================================================

                stkCek.Measure(
                    new Size(
                        stkCek.ActualWidth,
                        double.PositiveInfinity));

                stkCek.Arrange(
                    new Rect(
                        new Size(
                            stkCek.ActualWidth,
                            stkCek.DesiredSize.Height)));

                double dpi = 300;
                double scale = dpi / 96.0;

                RenderTargetBitmap bitmap =
                    new RenderTargetBitmap(
                        (int)(stkCek.ActualWidth * scale),
                        (int)(stkCek.ActualHeight * scale),
                        dpi,
                        dpi,
                        PixelFormats.Pbgra32);

                bitmap.Render(stkCek);

                System.Windows.Controls.Image cekImage =
                    new System.Windows.Controls.Image
                    {
                        Source = bitmap,
                        Width = stkCek.ActualWidth,
                        Height = stkCek.ActualHeight
                    };

                FixedDocument fixedDoc =
                    new FixedDocument();

                FixedPage fixedPage =
                    new FixedPage
                    {
                        Width = stkCek.ActualWidth,
                        Height = stkCek.ActualHeight
                    };

                fixedPage.Children.Add(cekImage);

                PageContent pageContent =
                    new PageContent();

                ((IAddChild)pageContent).AddChild(fixedPage);

                fixedDoc.Pages.Add(pageContent);

                XpsDocumentWriter writer =
                    PrintQueue.CreateXpsDocumentWriter(printer);

                writer.Write(fixedDoc);

                // =================================================
                // 4. ÇEK PƏNCƏRƏSİNİ AVTOMATİK BAĞLA
                // =================================================

                DialogResult = true;

                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Çap zamanı xəta baş verdi:\n\n" +
                    ex.Message,
                    "Çap xətası",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        // =========================================================
        // PƏNCƏRƏ BAĞLANIR
        // =========================================================

        protected override void OnClosing(
            System.ComponentModel.CancelEventArgs e)
        {
            base.OnClosing(e);
        }
    }
}