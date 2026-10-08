using Microsoft.Data.Sqlite;
using PcKod.UI.Data;
using PcKod.UI.Models;
using PcKod.UI.Views;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace PcKod.UI.Views
{
    public partial class GunlukSatislarWindow : Window
    {
        private readonly List<SatisModel> satislar =
            new List<SatisModel>();

        private readonly List<SatisModel> butunSatisSetirleri =
            new List<SatisModel>();

        private readonly CultureInfo azCulture =
            new CultureInfo("az-Latn-AZ");


        // =========================================================
        // CONSTRUCTOR
        // =========================================================

        public GunlukSatislarWindow()
        {
            InitializeComponent();

            EnsureSalesColumns();

            txtGun.Text =
                "Bugünkü satışlar: " +
                DateTime.Now.ToString("dd.MM.yyyy");

            GunlukSatislariGetir();
        }


        // =========================================================
        // DECIMAL OXU
        // =========================================================

        private decimal ParseDecimal(object value)
        {
            if (value == null ||
                value == DBNull.Value)
            {
                return 0m;
            }

            if (value is decimal decimalValue)
                return decimalValue;

            if (value is double doubleValue)
            {
                return Convert.ToDecimal(
                    doubleValue,
                    CultureInfo.InvariantCulture);
            }

            if (value is float floatValue)
            {
                return Convert.ToDecimal(
                    floatValue,
                    CultureInfo.InvariantCulture);
            }

            if (value is int intValue)
                return intValue;

            if (value is long longValue)
                return longValue;

            string text =
                value.ToString()?.Trim() ?? "";

            if (string.IsNullOrWhiteSpace(text))
                return 0m;

            if (decimal.TryParse(
                text,
                NumberStyles.Any,
                CultureInfo.InvariantCulture,
                out decimal result))
            {
                return result;
            }

            if (decimal.TryParse(
                text,
                NumberStyles.Any,
                azCulture,
                out result))
            {
                return result;
            }

            text = text
                .Replace("AZN", "")
                .Replace("azn", "")
                .Replace("Azn", "")
                .Replace(" ", "")
                .Replace(",", ".");

            if (decimal.TryParse(
                text,
                NumberStyles.Any,
                CultureInfo.InvariantCulture,
                out result))
            {
                return result;
            }

            return 0m;
        }


        // =========================================================
        // MİQDAR OXU
        // =========================================================

        private double ParseMiqdar(object value)
        {
            if (value == null ||
                value == DBNull.Value)
            {
                return 0;
            }

            if (value is double d)
                return d;

            if (value is decimal dec)
                return (double)dec;

            if (value is float f)
                return f;

            if (value is int i)
                return i;

            if (value is long l)
                return l;

            string text =
                value.ToString()?.Trim() ?? "";

            if (double.TryParse(
                text,
                NumberStyles.Any,
                CultureInfo.InvariantCulture,
                out double result))
            {
                return result;
            }

            if (double.TryParse(
                text,
                NumberStyles.Any,
                azCulture,
                out result))
            {
                return result;
            }

            return 0;
        }


        // =========================================================
        // SATIŞ CƏDVƏLİNİ YOXLAMA
        // =========================================================

        private void EnsureSalesColumns()
        {
            try
            {
                using (var db =
                    new SqliteConnection(
                        DatabaseHelper.ConnectionString))
                {
                    db.Open();

                    bool birimFiyatVar = false;
                    bool birimTipiVar = false;
                    bool satisQrupIdVar = false;

                    using (var cmd =
                        new SqliteCommand(
                            "PRAGMA table_info(Satislar);",
                            db))
                    {
                        using (var reader =
                            cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string columnName =
                                    reader["name"]?.ToString()
                                    ?? "";

                                if (columnName.Equals(
                                    "BirimFiyat",
                                    StringComparison.OrdinalIgnoreCase))
                                {
                                    birimFiyatVar = true;
                                }

                                if (columnName.Equals(
                                    "BirimTipi",
                                    StringComparison.OrdinalIgnoreCase))
                                {
                                    birimTipiVar = true;
                                }

                                if (columnName.Equals(
                                    "SatisQrupId",
                                    StringComparison.OrdinalIgnoreCase))
                                {
                                    satisQrupIdVar = true;
                                }
                            }
                        }
                    }


                    // =================================================
                    // BİRİM QİYMƏTİ
                    // =================================================

                    if (!birimFiyatVar)
                    {
                        using (var alter =
                            new SqliteCommand(
                                @"ALTER TABLE Satislar
                                  ADD COLUMN BirimFiyat REAL
                                  DEFAULT 0;",
                                db))
                        {
                            alter.ExecuteNonQuery();
                        }
                    }


                    // =================================================
                    // BİRİM TİPİ
                    // =================================================

                    if (!birimTipiVar)
                    {
                        using (var alter =
                            new SqliteCommand(
                                @"ALTER TABLE Satislar
                                  ADD COLUMN BirimTipi INTEGER
                                  DEFAULT 0;",
                                db))
                        {
                            alter.ExecuteNonQuery();
                        }
                    }


                    // =================================================
                    // SATIŞ QRUP ID
                    // =================================================

                    if (!satisQrupIdVar)
                    {
                        using (var alter =
                            new SqliteCommand(
                                @"ALTER TABLE Satislar
                                  ADD COLUMN SatisQrupId TEXT;",
                                db))
                        {
                            alter.ExecuteNonQuery();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Satış cədvəli yoxlanılarkən xəta baş verdi:\n\n" +
                    ex.Message,
                    "Database xətası",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }


        // =========================================================
        // GÜNLÜK SATIŞLARI GƏTİR
        // =========================================================

        private void GunlukSatislariGetir()
        {
            satislar.Clear();
            butunSatisSetirleri.Clear();

            decimal gunlukCem = 0m;

            string bugun =
                DateTime.Now.ToString("yyyy-MM-dd");

            try
            {
                using (var db =
                    new SqliteConnection(
                        DatabaseHelper.ConnectionString))
                {
                    db.Open();

                    string sql = @"
                        SELECT
                            Id,
                            UrunAdi,
                            Miktar,
                            BirimFiyat,
                            BirimTipi,
                            ToplamTutar,
                            OdemeYontemi,
                            Tarih,
                            SatisQrupId
                        FROM Satislar
                        WHERE Tarih LIKE @tarix
                        ORDER BY Id DESC";

                    using (var cmd =
                        new SqliteCommand(sql, db))
                    {
                        cmd.Parameters.AddWithValue(
                            "@tarix",
                            bugun + "%");

                        using (var reader =
                            cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                // =================================================
                                // ID
                                // =================================================

                                int id = 0;

                                if (!reader.IsDBNull(0))
                                {
                                    int.TryParse(
                                        reader[0].ToString(),
                                        out id);
                                }


                                // =================================================
                                // MƏHSUL
                                // =================================================

                                string urunAdi =
                                    reader.IsDBNull(1)
                                        ? ""
                                        : reader[1]?.ToString()
                                          ?? "";


                                // =================================================
                                // MİQDAR
                                // =================================================

                                double miktar =
                                    ParseMiqdar(reader[2]);


                                // =================================================
                                // BİRİM QİYMƏTİ
                                // =================================================

                                decimal birimFiyat =
                                    ParseDecimal(reader[3]);


                                // =================================================
                                // BİRİM TİPİ
                                // =================================================

                                int birimTipi = 0;

                                if (!reader.IsDBNull(4))
                                {
                                    int.TryParse(
                                        reader[4].ToString(),
                                        out birimTipi);
                                }


                                // =================================================
                                // TOPLAM
                                // =================================================

                                decimal databaseMebleg =
                                    ParseDecimal(reader[5]);


                                // =================================================
                                // ÖDƏNİŞ
                                // =================================================

                                string odemeYontemi =
                                    reader.IsDBNull(6)
                                        ? ""
                                        : reader[6]?.ToString()
                                          ?? "";


                                // =================================================
                                // TARİX
                                // =================================================

                                string tarix = "";

                                if (!reader.IsDBNull(7))
                                {
                                    string tarixText =
                                        reader[7]?.ToString()
                                        ?? "";

                                    if (DateTime.TryParse(
                                        tarixText,
                                        out DateTime dt))
                                    {
                                        tarix =
                                            dt.ToString(
                                                "dd.MM.yyyy HH:mm");
                                    }
                                    else
                                    {
                                        tarix =
                                            tarixText;
                                    }
                                }


                                // =================================================
                                // QRUP ID
                                // =================================================

                                string satisQrupId = "";

                                if (!reader.IsDBNull(8))
                                {
                                    satisQrupId =
                                        reader[8]?.ToString()
                                        ?? "";
                                }

                                /*
                                 * Köhnə satışlarda SatisQrupId olmaya bilər.
                                 *
                                 * Belə halda həmin sətrin öz Id-sini
                                 * qrup kimi istifadə edirik.
                                 *
                                 * Beləliklə köhnə satışlar da ayrıca çek
                                 * kimi görünəcək.
                                 */

                                if (string.IsNullOrWhiteSpace(
                                    satisQrupId))
                                {
                                    satisQrupId =
                                        "OLD-" + id;
                                }


                                // =================================================
                                // FƏRQ SƏTRİ
                                // =================================================

                                bool differenceRow =
                                    IsDifferenceRow(urunAdi);


                                // =================================================
                                // MƏBLƏĞ
                                // =================================================

                                decimal mebleg;

                                if (differenceRow)
                                {
                                    mebleg =
                                        databaseMebleg;
                                }
                                else
                                {
                                    if (birimFiyat > 0)
                                    {
                                        mebleg =
                                            Math.Round(
                                                birimFiyat *
                                                (decimal)miktar,
                                                2,
                                                MidpointRounding.AwayFromZero);
                                    }
                                    else if (
                                        miktar > 0 &&
                                        databaseMebleg != 0)
                                    {
                                        birimFiyat =
                                            databaseMebleg /
                                            (decimal)miktar;

                                        mebleg =
                                            Math.Round(
                                                birimFiyat *
                                                (decimal)miktar,
                                                2,
                                                MidpointRounding.AwayFromZero);
                                    }
                                    else
                                    {
                                        mebleg = 0m;
                                    }
                                }


                                // =================================================
                                // MODEL
                                // =================================================

                                var model =
                                    new SatisModel
                                    {
                                        Id =
                                            id,

                                        UrunAdi =
                                            urunAdi,

                                        Miktar =
                                            miktar,

                                        BirimFiyat =
                                            birimFiyat,

                                        BirimTipi =
                                            birimTipi,

                                        BirimMetni =
                                            GetBirimMetni(
                                                birimTipi),

                                        ToplamTutar =
                                            mebleg,

                                        OdemeYontemi =
                                            odemeYontemi,

                                        Tarix =
                                            tarix,

                                        SatisQrupId =
                                            satisQrupId,

                                        IsDifference =
                                            differenceRow
                                    };


                                // Bütün real sətirləri saxlayırıq.
                                butunSatisSetirleri.Add(model);
                            }
                        }
                    }
                }


                // =========================================================
                // SATIŞLARI QRUPLAŞDIR
                // =========================================================

                var qruplar =
                    butunSatisSetirleri
                        .GroupBy(x => x.SatisQrupId)
                        .OrderByDescending(
                            g => g.Max(x => x.Id));


                foreach (var qrup in qruplar)
                {
                    var mehsulSetirleri =
                        qrup
                            .Where(x => !x.IsDifference)
                            .ToList();

                    var ferqSetirleri =
                        qrup
                            .Where(x => x.IsDifference)
                            .ToList();


                    // =====================================================
                    // ÇEKİN ÜMUMİ MƏBLƏĞİ
                    // =====================================================

                    decimal cekCemi =
                        qrup.Sum(x => x.ToplamTutar);


                    // =====================================================
                    // MƏHSUL ADLARI
                    // =====================================================

                    string mehsulAdi;

                    if (mehsulSetirleri.Count == 0)
                    {
                        mehsulAdi =
                            "Endirim / Fərq";
                    }
                    else if (mehsulSetirleri.Count == 1)
                    {
                        mehsulAdi =
                            mehsulSetirleri[0].UrunAdi;
                    }
                    else
                    {
                        mehsulAdi =
                            string.Join(
                                ", ",
                                mehsulSetirleri
                                    .Select(x => x.UrunAdi));
                    }


                    // =====================================================
                    // MİQDAR
                    // =====================================================

                    double toplamMiqdar =
                        mehsulSetirleri.Sum(
                            x => x.Miktar);


                    // =====================================================
                    // İLK SATIŞ
                    // =====================================================

                    SatisModel ilk =
                        qrup
                            .OrderBy(x => x.Id)
                            .First();


                    // =====================================================
                    // QRUP MODELİ
                    // =====================================================

                    var qrupModel =
                        new SatisModel
                        {
                            Id =
                                ilk.Id,

                            UrunAdi =
                                mehsulAdi,

                            Miktar =
                                toplamMiqdar,

                            BirimFiyat =
                                0m,

                            BirimTipi =
                                0,

                            BirimMetni =
                                mehsulSetirleri.Count == 1
                                    ? mehsulSetirleri[0].BirimMetni
                                    : "çek",

                            ToplamTutar =
                                Math.Round(
                                    cekCemi,
                                    2,
                                    MidpointRounding.AwayFromZero),

                            OdemeYontemi =
                                ilk.OdemeYontemi,

                            Tarix =
                                ilk.Tarix,

                            SatisQrupId =
                                qrup.Key,

                            IsGroup =
                                true,

                            MehsulSayi =
                                mehsulSetirleri.Count
                        };


                    satislar.Add(
                        qrupModel);


                    // =====================================================
                    // GÜNLÜK CƏM
                    // =====================================================

                    /*
                     * Burada artıq fərq sətrini çıxmırıq.
                     *
                     * Məsələn:
                     *
                     * Məhsullar = 28 AZN
                     * Endirim   = -2 AZN
                     *
                     * Gündəlik satış = 26 AZN
                     */

                    gunlukCem +=
                        cekCemi;
                }


                // =========================================================
                // DATAGRID
                // =========================================================

                dgGunlukSatislar.ItemsSource = null;

                dgGunlukSatislar.ItemsSource =
                    satislar;


                // =========================================================
                // GÜNLÜK ÜMUMİ
                // =========================================================

                txtGunlukCem.Text =
                    gunlukCem.ToString(
                        "N2",
                        azCulture) +
                    " AZN";
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Gündəlik satışlar yüklənərkən xəta baş verdi:\n\n" +
                    ex.Message,
                    "Sistem xətası",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }


        // =========================================================
        // VAHİD
        // =========================================================

        private string GetBirimMetni(int birimTipi)
        {
            switch (birimTipi)
            {
                case 0:
                    return "ədəd";

                case 1:
                    return "kq";

                default:
                    return "ədəd";
            }
        }


        // =========================================================
        // FƏRQ SƏTRİDİRMİ?
        // =========================================================

        private bool IsDifferenceRow(string urunAdi)
        {
            if (string.IsNullOrWhiteSpace(urunAdi))
                return false;

            return
                urunAdi.Equals(
                    "Kassa endirimi / Yuvarlaqlaşdırma",
                    StringComparison.OrdinalIgnoreCase)
                ||
                urunAdi.Equals(
                    "Əlavə ödəniş / Fərq",
                    StringComparison.OrdinalIgnoreCase);
        }


        // =========================================================
        // VAHİD QİYMƏT DƏYİŞDİRİLƏNDƏ
        // =========================================================

        private void dgGunlukSatislar_CellEditEnding(
            object sender,
            DataGridCellEditEndingEventArgs e)
        {
            if (e.EditAction !=
                DataGridEditAction.Commit)
            {
                return;
            }

            if (e.Row.Item is not SatisModel satis)
                return;

            /*
             * Artıq Gündəlik Satışlar 1 çek = 1 sətir göstərir.
             *
             * Ona görə qruplaşdırılmış sətirdə vahid qiyməti
             * dəyişdirmək məntiqli deyil.
             *
             * Bu səbəbdən qrup sətrində editə icazə verilmir.
             */

            return;
        }


        // =========================================================
        // SEÇİLMİŞ ÇEKİN BÜTÜN MƏHSULLARINI GƏTİR
        // =========================================================

        private List<SatisModel> GetSaleGroup(
            string satisQrupId)
        {
            if (string.IsNullOrWhiteSpace(
                satisQrupId))
            {
                return new List<SatisModel>();
            }

            return
                butunSatisSetirleri
                    .Where(x =>
                        x.SatisQrupId ==
                        satisQrupId)
                    .ToList();
        }


        // =========================================================
        // ÇEKİ AÇ
        // =========================================================

        private void CekiAc(
            SatisModel satis)
        {
            if (satis == null)
                return;


            // =====================================================
            // QRUPUN BÜTÜN SƏTİRLƏRİ
            // =====================================================

            var qrup =
                GetSaleGroup(
                    satis.SatisQrupId);


            if (qrup.Count == 0)
            {
                MessageBox.Show(
                    "Bu satışın məhsulları tapılmadı.",
                    "Məlumat",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                return;
            }


            // =====================================================
            // FƏRQ SƏTRLƏRİNİ AYIR
            // =====================================================

            var mehsulSetirleri =
                qrup
                    .Where(x => !x.IsDifference)
                    .ToList();


            if (mehsulSetirleri.Count == 0)
            {
                MessageBox.Show(
                    "Bu qrupda məhsul satışı yoxdur.",
                    "Məlumat",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                return;
            }


            // =====================================================
            // ÇEK MƏBLƏĞİ
            // =====================================================

            decimal cekMeblegi =
                Math.Round(
                    qrup.Sum(
                        x => x.ToplamTutar),
                    2,
                    MidpointRounding.AwayFromZero);


            // =====================================================
            // BÜTÜN MƏHSULLARI SƏBƏT MODELİNƏ ÇEVİR
            // =====================================================

            var mehsullar =
                new List<SəbətMəhsul>();


            foreach (var satisSetiri in mehsulSetirleri)
            {
                var mehsul =
                    new SəbətMəhsul
                    {
                        Id =
                            satisSetiri.Id,

                        Barkod =
                            "SATIS-" +
                            satisSetiri.Id,

                        UrunAdi =
                            satisSetiri.UrunAdi ?? "",

                        Miktar =
                            satisSetiri.Miktar,

                        BirimFiyat =
                            satisSetiri.BirimFiyat,

                        BirimTipi =
                            satisSetiri.BirimTipi
                    };

                mehsullar.Add(
                    mehsul);
            }


            // =====================================================
            // ÇEK
            // =====================================================

            var cek =
                new CekWindow(
                    mehsullar,
                    cekMeblegi,
                    satis.OdemeYontemi ?? "",
                    satis.Id);

            cek.Owner = this;

            cek.ShowDialog();
        }


        // =========================================================
        // SƏTRDƏN ÇEKİ AÇ
        // =========================================================

        private void btnCek_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (sender is not Button button)
                return;

            if (button.DataContext
                is not SatisModel satis)
            {
                return;
            }

            CekiAc(satis);
        }


        // =========================================================
        // SEÇİLMİŞ ÇEKİ ÇAP ET
        // =========================================================

        private void btnCekCapEt_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (dgGunlukSatislar.SelectedItem
                is not SatisModel satis)
            {
                MessageBox.Show(
                    "Zəhmət olmasa əvvəlcə satış seçin.",
                    "Məlumat",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                return;
            }

            CekiAc(satis);
        }


        // =========================================================
        // YENİLƏ
        // =========================================================

        private void btnYenile_Click(
            object sender,
            RoutedEventArgs e)
        {
            GunlukSatislariGetir();
        }


        // =========================================================
        // SEÇİM DƏYİŞDİ
        // =========================================================

        private void dgGunlukSatislar_SelectionChanged(
            object sender,
            SelectionChangedEventArgs e)
        {
            // Xüsusi əməliyyat tələb olunmur.
        }
    }


    // =============================================================
    // SATIŞ MODELİ
    // =============================================================

    public class SatisModel
    {
        public int Id { get; set; }

        public string UrunAdi { get; set; } = "";

        public double Miktar { get; set; }

        public decimal BirimFiyat { get; set; }

        public int BirimTipi { get; set; }

        public string BirimMetni { get; set; } = "ədəd";

        public decimal ToplamTutar { get; set; }

        public string OdemeYontemi { get; set; } = "";

        public string Tarix { get; set; } = "";

        // =========================================================
        // YENİ
        // =========================================================

        public string SatisQrupId { get; set; } = "";

        public bool IsDifference { get; set; }

        public bool IsGroup { get; set; }

        public int MehsulSayi { get; set; }
    }
}