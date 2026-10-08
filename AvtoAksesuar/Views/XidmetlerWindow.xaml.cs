using Microsoft.Data.Sqlite;
using PcKod.UI.Data;
using System;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows;

namespace PcKod.UI.Views
{
    public partial class XidmetlerWindow : Window, INotifyPropertyChanged
    {
        public XidmetlerWindow()
        {
            InitializeComponent();

            txtXidmetAdi.Focus();
        }


        // ============================================================
        // SATIŞA VUR
        // ============================================================

        private void btnSatisaVur_Click(
            object sender,
            RoutedEventArgs e)
        {
            string xidmetAdi =
                txtXidmetAdi.Text.Trim();


            string meblegMetni =
                txtMebleg.Text
                    .Replace(".", ",")
                    .Replace("AZN", "")
                    .Replace("azn", "")
                    .Trim();


            // ========================================================
            // XİDMƏT ADI YOXLAMASI
            // ========================================================

            if (string.IsNullOrWhiteSpace(xidmetAdi))
            {
                MessageBox.Show(
                    "Xidmətin adını daxil edin.",
                    "Məlumat çatışmır",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                txtXidmetAdi.Focus();

                return;
            }


            // ========================================================
            // MƏBLƏĞ YOXLAMASI
            // ========================================================

            if (!decimal.TryParse(
                meblegMetni,
                NumberStyles.Number,
                new CultureInfo("az-Latn-AZ"),
                out decimal mebleg))
            {
                MessageBox.Show(
                    "Məbləği düzgün daxil edin.",
                    "Yanlış məbləğ",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                txtMebleg.Focus();

                return;
            }


            if (mebleg <= 0)
            {
                MessageBox.Show(
                    "Məbləğ 0-dan böyük olmalıdır.",
                    "Yanlış məbləğ",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                txtMebleg.Focus();

                return;
            }


            // ========================================================
            // XİDMƏTİN BAZADA SAXLANACAĞI AD
            // ========================================================
            // Geri qaytarma sistemi bu prefiksə baxaraq
            // xidmətləri məhsullardan ayıracaq.

            string bazadakiXidmetAdi =
                "XİDMƏT - " + xidmetAdi;


            // ========================================================
            // SATIŞI BAZAYA YAZ
            // ========================================================

            try
            {
                using (var db =
                    new SqliteConnection(
                        DatabaseHelper.ConnectionString))
                {
                    db.Open();


                    using (var cmd =
                        new SqliteCommand(
                            @"INSERT INTO Satislar
                              (
                                  UrunAdi,
                                  Miktar,
                                  ToplamTutar,
                                  OdemeYontemi,
                                  Tarih
                              )
                              VALUES
                              (
                                  @ad,
                                  @miqdar,
                                  @mebleg,
                                  @odeme,
                                  @tarix
                              )",
                            db))
                    {
                        cmd.Parameters.AddWithValue(
                            "@ad",
                            bazadakiXidmetAdi);


                        cmd.Parameters.AddWithValue(
                            "@miqdar",
                            1);


                        cmd.Parameters.AddWithValue(
                            "@mebleg",
                            mebleg);


                        cmd.Parameters.AddWithValue(
                            "@odeme",
                            "Xidmət");


                        cmd.Parameters.AddWithValue(
                            "@tarix",
                            DateTime.Now.ToString(
                                "yyyy-MM-dd"));


                        cmd.ExecuteNonQuery();
                    }
                }


                // ====================================================
                // UĞURLU MESAJ
                // ====================================================

                MessageBox.Show(
                    $"Xidmət: {xidmetAdi}\n" +
                    $"Məbləğ: {mebleg:N2} AZN\n\n" +
                    "Satışa uğurla vuruldu.",
                    "Xidmət satışı",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);


                // ====================================================
                // SAHƏLƏRİ TƏMİZLƏ
                // ====================================================

                txtXidmetAdi.Clear();

                txtMebleg.Clear();

                txtXidmetAdi.Focus();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Xidmət satışının əlavə edilməsi zamanı xəta baş verdi:\n\n" +
                    ex.Message,
                    "Sistem xətası",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}