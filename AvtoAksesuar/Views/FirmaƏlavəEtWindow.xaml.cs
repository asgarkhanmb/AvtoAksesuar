using System.Windows;
using Microsoft.Data.Sqlite;

namespace PcKod.UI.Views
{
    public partial class FirmaEkleWindow : Window
    {
        public FirmaEkleWindow()
        {
            InitializeComponent();
        }


        // =========================================================
        // GERİ QAYIT
        // =========================================================

        private void btnGeri_Click(
            object sender,
            RoutedEventArgs e)
        {
            this.Close();
        }


        // =========================================================
        // FİRMANI YADDA SAXLA
        // =========================================================

        private void btnFirmaKaydet_Click(
            object sender,
            RoutedEventArgs e)
        {
            // Firma adı boşdursa
            if (string.IsNullOrWhiteSpace(txtFirmaAdi.Text))
            {
                MessageBox.Show(
                    "Zəhmət olmasa firma adını daxil edin.",
                    "Xəbərdarlıq",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }


            try
            {
                using (var db =
                    new SqliteConnection(
                        "Data Source=PcKod.db"))
                {
                    db.Open();


                    var cmd = new SqliteCommand(
                        @"INSERT INTO Firmalar
                          (FirmaAdi, ToplamBorc)
                          VALUES
                          (@ad, 0)",
                        db);


                    cmd.Parameters.AddWithValue(
                        "@ad",
                        txtFirmaAdi.Text.Trim());


                    cmd.ExecuteNonQuery();
                }


                // Uğurlu əlavə mesajı
                MessageBox.Show(
                    "Firma uğurla əlavə edildi.",
                    "Əməliyyat uğurlu oldu",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);


                // Pəncərəni bağla
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Qeydiyyat zamanı xəta baş verdi:\n"
                    + ex.Message,
                    "Sistem xətası",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}
