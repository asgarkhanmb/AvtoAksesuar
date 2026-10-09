using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Data.Sqlite;
using PcKod.UI.Models;
using PcKod.UI.Data;

namespace PcKod.UI.Views
{
    public partial class UrunListesiWindow : Window
    {
        private string ConnectionString =>
    DatabaseHelper.ConnectionString;

        // =========================================================
        // CONSTRUCTOR
        // =========================================================

        public UrunListesiWindow()
        {
            InitializeComponent();

            cmbBirim.SelectedIndex = 0;

            UrunleriYukle();
        }


        // =========================================================
        // GERİ
        // =========================================================

        private void btnGeri_Click(
            object sender,
            RoutedEventArgs e)
        {
            this.Close();
        }


        // =========================================================
        // MƏHSULLARI YÜKLƏ
        // =========================================================

        private void UrunleriYukle(
            string arama = "")
        {
            var urunListesi =
                new List<Məhsul>();

            try
            {
                using (var db =
                    new SqliteConnection(
                        ConnectionString))
                {
                    db.Open();


                    string sql = @"
                        SELECT
                            Id,
                            Barkod,
                            UrunAdi,
                            AlisFiyati,
                            SatisFiyati,
                            BirimTipi,
                            StokMiktari
                        FROM Urunler
                    ";


                    if (!string.IsNullOrWhiteSpace(arama))
                    {
                        sql += @"
                            WHERE UrunAdi LIKE @p
                            OR Barkod LIKE @p
                        ";
                    }


                    sql +=
                        " ORDER BY UrunAdi";


                    using (var cmd =
                        new SqliteCommand(
                            sql,
                            db))
                    {
                        if (!string.IsNullOrWhiteSpace(arama))
                        {
                            cmd.Parameters.AddWithValue(
                                "@p",
                                "%" + arama + "%");
                        }


                        using (var reader =
                            cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var urun =
                                    new Məhsul();


                                // =================================================
                                // ID
                                // =================================================

                                urun.Id =
                                    reader.IsDBNull(0)
                                        ? 0
                                        : reader.GetInt32(0);


                                // =================================================
                                // BARKOD
                                // =================================================

                                urun.Barkod =
                                    reader.IsDBNull(1)
                                        ? ""
                                        : reader.GetString(1);


                                // =================================================
                                // MƏHSUL ADI
                                // =================================================

                                urun.MəhsulAdi =
                                    reader.IsDBNull(2)
                                        ? ""
                                        : reader.GetString(2);


                                // =================================================
                                // ALIŞ QİYMƏTİ
                                // =================================================

                                urun.AlisQiymeti =
                                    reader.IsDBNull(3)
                                        ? 0
                                        : reader.GetDecimal(3);


                                // =================================================
                                // SATIŞ QİYMƏTİ
                                // =================================================

                                urun.SatisQiymeti =
                                    reader.IsDBNull(4)
                                        ? 0
                                        : reader.GetDecimal(4);


                                // =================================================
                                // ÖLÇÜ VAHİDİ
                                // =================================================

                                urun.VahidNovu =
                                    reader.IsDBNull(5)
                                        ? 0
                                        : reader.GetInt32(5);


                                // =================================================
                                // STOK SAYI
                                // =================================================

                                urun.StokMiqdar =
                                    reader.IsDBNull(6)
                                        ? 0
                                        : reader.GetDecimal(6);


                                urunListesi.Add(
                                    urun);
                            }
                        }
                    }
                }


                dgUrunler.ItemsSource =
                    urunListesi;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Məhsullar yüklənərkən xəta baş verdi:\n\n" +
                    ex.Message,
                    "Xəta",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }


        // =========================================================
        // AXTARIŞ
        // =========================================================

        private void txtHizliAra_TextChanged(
            object sender,
            TextChangedEventArgs e)
        {
            UrunleriYukle(
                txtHizliAra.Text.Trim());
        }


        // =========================================================
        // MƏHSUL SEÇİLDİ
        // =========================================================

        private void dgUrunler_SelectionChanged(
            object sender,
            SelectionChangedEventArgs e)
        {
            if (dgUrunler.SelectedItem is Məhsul s)
            {
                txtBarkod.Text =
                    s.Barkod;


                txtUrunAdi.Text =
                    s.MəhsulAdi;


                // =================================================
                // ALIŞ QİYMƏTİ
                // =================================================

                txtAlisFiyati.Text =
                    s.AlisQiymeti.ToString(
                        "N2",
                        CultureInfo.CurrentCulture);


                // =================================================
                // SATIŞ QİYMƏTİ
                // =================================================

                txtSatisFiyati.Text =
                    s.SatisQiymeti.ToString(
                        "N2",
                        CultureInfo.CurrentCulture);


                // =================================================
                // STOK SAYI
                // =================================================

                txtStokMiktari.Text =
                    s.StokMiqdar.ToString(
                        "0.##",
                        CultureInfo.InvariantCulture);


                // =================================================
                // ÖLÇÜ VAHİDİ
                // =================================================

                cmbBirim.SelectedIndex =
                    s.VahidNovu;
            }
        }


        // =========================================================
        // YADDA SAXLA / YENİLƏ
        // =========================================================

        private void btnKaydet_Click(
            object sender,
            RoutedEventArgs e)
        {
            // =====================================================
            // BARKOD
            // =====================================================

            if (string.IsNullOrWhiteSpace(
                txtBarkod.Text))
            {
                MessageBox.Show(
                    "Barkod nömrəsini daxil edin.",
                    "Xəbərdarlıq",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                txtBarkod.Focus();

                return;
            }


            // =====================================================
            // MƏHSUL ADI
            // =====================================================

            if (string.IsNullOrWhiteSpace(
                txtUrunAdi.Text))
            {
                MessageBox.Show(
                    "Məhsulun adını daxil edin.",
                    "Xəbərdarlıq",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                txtUrunAdi.Focus();

                return;
            }


            // =====================================================
            // ALIŞ QİYMƏTİ
            // =====================================================

            if (!QiymetOxu(
                txtAlisFiyati.Text,
                out decimal alisFiyati))
            {
                MessageBox.Show(
                    "Alış qiymətini düzgün daxil edin.",
                    "Xəbərdarlıq",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                txtAlisFiyati.Focus();

                return;
            }


            // =====================================================
            // SATIŞ QİYMƏTİ
            // =====================================================

            if (!QiymetOxu(
                txtSatisFiyati.Text,
                out decimal satisFiyati))
            {
                MessageBox.Show(
                    "Satış qiymətini düzgün daxil edin.",
                    "Xəbərdarlıq",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                txtSatisFiyati.Focus();

                return;
            }


            // =====================================================
            // STOK SAYI
            // =====================================================

            decimal stokMiktari = 0;


            if (!string.IsNullOrWhiteSpace(
                txtStokMiktari.Text))
            {
                if (!StokOxu(
                    txtStokMiktari.Text,
                    out stokMiktari))
                {
                    MessageBox.Show(
                        "Məhsul sayını düzgün daxil edin.",
                        "Xəbərdarlıq",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    txtStokMiktari.Focus();

                    return;
                }
            }


            if (stokMiktari < 0)
            {
                MessageBox.Show(
                    "Məhsul sayı mənfi ola bilməz.",
                    "Xəbərdarlıq",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                txtStokMiktari.Focus();

                return;
            }


            int birimTipi =
                cmbBirim.SelectedIndex;


            try
            {
                using (var db =
                    new SqliteConnection(
                        ConnectionString))
                {
                    db.Open();


                    // =================================================
                    // MƏHSULU YADDA SAXLA
                    // =================================================

                    string sql = @"
                        INSERT OR REPLACE INTO Urunler
                        (
                            Barkod,
                            UrunAdi,
                            AlisFiyati,
                            SatisFiyati,
                            BirimTipi,
                            StokMiktari
                        )
                        VALUES
                        (
                            @Barkod,
                            @UrunAdi,
                            @AlisFiyati,
                            @SatisFiyati,
                            @BirimTipi,
                            @StokMiktari
                        )
                    ";


                    using (var cmd =
                        new SqliteCommand(
                            sql,
                            db))
                    {
                        cmd.Parameters.AddWithValue(
                            "@Barkod",
                            txtBarkod.Text.Trim());


                        cmd.Parameters.AddWithValue(
                            "@UrunAdi",
                            txtUrunAdi.Text.Trim());


                        cmd.Parameters.AddWithValue(
                            "@AlisFiyati",
                            alisFiyati);


                        cmd.Parameters.AddWithValue(
                            "@SatisFiyati",
                            satisFiyati);


                        cmd.Parameters.AddWithValue(
                            "@BirimTipi",
                            birimTipi);


                        cmd.Parameters.AddWithValue(
                            "@StokMiktari",
                            stokMiktari);


                        cmd.ExecuteNonQuery();
                    }
                }


                // Siyahılama yenilə
                UrunleriYukle();


                // Formu təmizlə
                Temizle();


                MessageBox.Show(
                    "Məhsul uğurla yadda saxlanıldı.",
                    "Uğurlu",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Məhsul yadda saxlanılarkən xəta baş verdi:\n\n" +
                    ex.Message,
                    "Xəta",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }


        // =========================================================
        // QİYMƏT OXU
        // =========================================================

        private bool QiymetOxu(
            string text,
            out decimal value)
        {
            value = 0;


            if (string.IsNullOrWhiteSpace(text))
                return false;


            // ₼ işarəsini sil
            text =
                text.Replace("₼", "");


            // AZN sözünü sil
            text =
                text.Replace("AZN", "");


            text =
                text.Trim();


            // Azərbaycan formatı:
            // 12,50
            //
            // İngilis formatı:
            // 12.50

            text =
                text.Replace(",", ".");


            return decimal.TryParse(
                text,
                NumberStyles.Any,
                CultureInfo.InvariantCulture,
                out value);
        }


        // =========================================================
        // STOK SAYI OXU
        // =========================================================

        private bool StokOxu(
            string text,
            out decimal value)
        {
            value = 0;


            if (string.IsNullOrWhiteSpace(text))
                return false;


            text =
                text.Trim();


            // Vergülü nöqtəyə çevir
            text =
                text.Replace(",", ".");


            return decimal.TryParse(
                text,
                NumberStyles.Any,
                CultureInfo.InvariantCulture,
                out value);
        }


        // =========================================================
        // MƏHSULU SİL - İLK DÜYMƏ
        // =========================================================

        private void btnSilIlk_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (dgUrunler.SelectedItem == null)
            {
                MessageBox.Show(
                    "Əvvəlcə silmək istədiyiniz məhsulu seçin.",
                    "Xəbərdarlıq",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }


            pnlSilOnay.Visibility =
                Visibility.Visible;


            txtSilSifre.Clear();

            txtSilSifre.Focus();
        }


        // =========================================================
        // SİLMƏNİ TƏSDİQLƏ
        // =========================================================

        private void btnSilOnay_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (dgUrunler.SelectedItem is not Məhsul s)
            {
                MessageBox.Show(
                    "Məhsul seçilməyib.",
                    "Xəbərdarlıq",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }


            // =====================================================
            // GİRİŞ KODUNU DATABASE-DƏN OXU
            // =====================================================

            string sistemKodu =
                GetSystemPassword();


            // =====================================================
            // SİLMƏ KODUNU YOXLAYIR
            // =====================================================

            if (txtSilSifre.Password != sistemKodu)
            {
                MessageBox.Show(
                    "Şifrə yanlışdır.",
                    "Xəta",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                txtSilSifre.Clear();

                txtSilSifre.Focus();

                return;
            }


            try
            {
                using (var db =
                    new SqliteConnection(
                        ConnectionString))
                {
                    db.Open();


                    string sql = @"
                        DELETE FROM Urunler
                        WHERE Id = @Id
                    ";


                    using (var cmd =
                        new SqliteCommand(
                            sql,
                            db))
                    {
                        cmd.Parameters.AddWithValue(
                            "@Id",
                            s.Id);

                        cmd.ExecuteNonQuery();
                    }
                }


                UrunleriYukle();


                btnSilIptal_Click(
                    null,
                    null);


                MessageBox.Show(
                    "Məhsul uğurla silindi.",
                    "Uğurlu",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Məhsul silinərkən xəta baş verdi:\n\n" +
                    ex.Message,
                    "Xəta",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }


        // =========================================================
        // SİSTEM GİRİŞ KODUNU OXU
        // =========================================================

        private string GetSystemPassword()
        {
            try
            {
                using (var db =
                    new SqliteConnection(
                        ConnectionString))
                {
                    db.Open();


                    var cmd =
                        new SqliteCommand(
                            @"SELECT SifreHash
                              FROM Ayarlar
                              WHERE Id = 1",
                            db);


                    var result =
                        cmd.ExecuteScalar();


                    // =================================================
                    // ƏGƏR HƏLƏ KOD TƏYİN EDİLMƏYİB
                    // =================================================

                    if (result == null ||
                        result == DBNull.Value ||
                        string.IsNullOrWhiteSpace(
                            result.ToString()))
                    {
                        return "1234";
                    }


                    return result.ToString();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Giriş kodu oxunarkən xəta baş verdi:\n\n" +
                    ex.Message,
                    "Sistem xətası",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);


                // Database oxunmasa məhsul silməyə icazə vermirik.
                return string.Empty;
            }
        }


        // =========================================================
        // SİLMƏDƏN İMTİNA
        // =========================================================

        private void btnSilIptal_Click(
            object sender,
            RoutedEventArgs e)
        {
            pnlSilOnay.Visibility =
                Visibility.Collapsed;


            txtSilSifre.Clear();
        }


        // =========================================================
        // FORMU TƏMİZLƏ
        // =========================================================

        private void Temizle()
        {
            txtBarkod.Clear();

            txtUrunAdi.Clear();

            txtAlisFiyati.Clear();

            txtSatisFiyati.Clear();

            txtStokMiktari.Clear();

            cmbBirim.SelectedIndex = 0;

            dgUrunler.SelectedItem = null;

            pnlSilOnay.Visibility =
                Visibility.Collapsed;

            txtSilSifre.Clear();

            txtBarkod.Focus();
        }


        // =========================================================
        // STOK SAHƏSİ - YALNIZ RƏQƏM
        // =========================================================

        private void txtStokMiktari_PreviewTextInput(
            object sender,
            TextCompositionEventArgs e)
        {
            foreach (char c in e.Text)
            {
                if (!char.IsDigit(c) &&
                    c != '.' &&
                    c != ',')
                {
                    e.Handled = true;

                    return;
                }
            }


            e.Handled = false;
        }
    }
}