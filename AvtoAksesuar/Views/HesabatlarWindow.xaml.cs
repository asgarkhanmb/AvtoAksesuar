using ClosedXML.Excel;
using Microsoft.Data.Sqlite;
using PcKod.UI.Data;
using System.Globalization;

using System.Windows;

namespace PcKod.UI.Views
{
    public partial class HesabatlarWindow : Window
    {
        private List<SatisHesabatModel> satislar =
            new List<SatisHesabatModel>();

        private List<UrunHesabatModel> mehsullar =
            new List<UrunHesabatModel>();


        // ============================================================
        // KONSTRUKTOR
        // ============================================================

        public HesabatlarWindow()
        {
            InitializeComponent();

            dpBaslangic.SelectedDate =
                DateTime.Today;

            dpBitis.SelectedDate =
                DateTime.Today;

            cmbOdemeTipi.Items.Clear();

            cmbOdemeTipi.Items.Add(
                "Bütün ödənişlər");

            cmbOdemeTipi.Items.Add(
                "Nəğd");

            cmbOdemeTipi.Items.Add(
                "Kart");

            cmbOdemeTipi.Items.Add(
                "Xidmət");

            cmbOdemeTipi.SelectedIndex = 0;

        }


        // ============================================================
        // GERİ
        // ============================================================

        private void btnGeri_Click(
            object sender,
            RoutedEventArgs e)
        {
            Close();
        }


        // ============================================================
        // HESABATI GÖSTƏR
        // ============================================================

        private void btnHesabatGoster_Click(
            object sender,
            RoutedEventArgs e)
        {
            HesabatYukle();
        }


        // ============================================================
        // HESABATI YÜKLƏ
        // ============================================================

        private void HesabatYukle()
        {
            try
            {
                // --------------------------------------------------------
                // TARİX YOXLAMASI
                // --------------------------------------------------------

                if (dpBaslangic.SelectedDate == null ||
                    dpBitis.SelectedDate == null)
                {
                    MessageBox.Show(
                        "Zəhmət olmasa başlanğıc və bitiş tarixlərini seçin.",
                        "Xəbərdarlıq",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    return;
                }

                DateTime baslangic =
                    dpBaslangic.SelectedDate.Value.Date;

                DateTime bitis =
                    dpBitis.SelectedDate.Value.Date;

                if (baslangic > bitis)
                {
                    MessageBox.Show(
                        "Başlanğıc tarixi bitiş tarixindən böyük ola bilməz.",
                        "Xəbərdarlıq",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    return;
                }

                string odemeTipi =
                    cmbOdemeTipi.SelectedItem?.ToString()
                    ?? "Bütün ödənişlər";

                satislar.Clear();

                mehsullar.Clear();

                using (var db =
                    new SqliteConnection(
                        DatabaseHelper.ConnectionString))
                {
                    db.Open();

                    // ====================================================
                    // TARİX
                    // ====================================================

                    string baslangicTarix =
                        baslangic.ToString(
                            "yyyy-MM-dd",
                            CultureInfo.InvariantCulture);

                    string bitisTarix =
                        bitis
                            .AddDays(1)
                            .ToString(
                                "yyyy-MM-dd",
                                CultureInfo.InvariantCulture);

                    // ====================================================
                    // SATIŞ SQL
                    // ====================================================

                    string sql = @"
                        SELECT
                            Id,
                            UrunAdi,
                            Miktar,
                            ToplamTutar,
                            OdemeYontemi,
                            Tarih,
                            SatisQrupId
                        FROM Satislar
                        WHERE Tarih >= @baslangic
                          AND Tarih < @bitis
                    ";

                    // ====================================================
                    // ÖDƏNİŞ FİLTRİ
                    // ====================================================

                    if (odemeTipi == "Nəğd")
                    {
                        sql += @"
                            AND OdemeYontemi = 'Nəğd'
                        ";
                    }
                    else if (odemeTipi == "Kart")
                    {
                        sql += @"
                            AND OdemeYontemi = 'Kart'
                        ";
                    }
                    else if (odemeTipi == "Xidmət")
                    {
                        sql += @"
                            AND OdemeYontemi = 'Xidmət'
                        ";
                    }

                    sql += @"
                        ORDER BY Tarih DESC, Id DESC
                    ";

                    // ====================================================
                    // SATIŞLARI OXU
                    // ====================================================

                    using (var cmd =
                        new SqliteCommand(
                            sql,
                            db))
                    {
                        cmd.Parameters.AddWithValue(
                            "@baslangic",
                            baslangicTarix);

                        cmd.Parameters.AddWithValue(
                            "@bitis",
                            bitisTarix);

                        using (var reader =
                            cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                long id = 0;

                                if (reader["Id"] != DBNull.Value)
                                {
                                    long.TryParse(
                                        reader["Id"].ToString(),
                                        out id);
                                }

                                string urunAdi =
                                    reader["UrunAdi"]?.ToString()
                                    ?? "";

                                double miktar =
                                    ParseMiqdar(
                                        reader["Miktar"]);

                                decimal toplam =
                                    ParseDecimal(
                                        reader["ToplamTutar"]);

                                string odeme =
                                    reader["OdemeYontemi"]?.ToString()
                                    ?? "";

                                DateTime tarix =
                                    ParseTarix(
                                        reader["Tarih"]);

                                string satisQrupId =
                                    reader["SatisQrupId"]?.ToString()
                                    ?? "";

                                satislar.Add(
                                    new SatisHesabatModel
                                    {
                                        Id = id,

                                        UrunAdi =
                                            urunAdi,

                                        Miktar =
                                            miktar,

                                        Toplam =
                                            toplam,

                                        OdemeTipi =
                                            odeme,

                                        Tarix =
                                            tarix,

                                        SatisQrupId =
                                            satisQrupId,

                                        Musteri =
                                            "-"
                                    });
                            }
                        }
                    }


                    // ====================================================
                    // MƏHSULLAR ÜZRƏ NET HESABAT
                    // ====================================================
                    //
                    // Məsələn:
                    //
                    // Satış       +0.50
                    // Qaytarma    -0.50
                    //
                    // Net         0.00
                    //
                    // ====================================================

                    mehsullar =
                        satislar
                            .Where(x =>
                                x.OdemeTipi != "Xidmət" &&
                                !IsDuzelisSutri(x.UrunAdi))
                            .GroupBy(
                                x =>
                                    GetNormalUrunAdi(
                                        x.UrunAdi))
                            .Select(g =>
                                new UrunHesabatModel
                                {
                                    UrunAdi =
                                        g.Key,

                                    Miktar =
                                        g.Sum(
                                            x =>
                                                IsQaytarmaSutri(
                                                    x.UrunAdi)
                                                    ? -x.Miktar
                                                    : x.Miktar),

                                    Toplam =
                                        g.Sum(
                                            x =>
                                                x.Toplam)
                                })
                            .Where(x =>
                                Math.Abs(x.Miktar) > 0.000001 ||
                                x.Toplam != 0)
                            .OrderByDescending(
                                x => x.Toplam)
                            .ToList();
                }


                // ============================================================
                // ÜMUMİ GROSS SATIŞ
                // ============================================================

                decimal umumiSatis =
                    satislar
                        .Where(x =>
                            !IsQaytarmaSutri(x.UrunAdi) &&
                            x.Toplam > 0 &&
                            !IsDuzelisSutri(x.UrunAdi))
                        .Sum(x => x.Toplam);


                // ============================================================
                // ÜMUMİ QAYTARMA
                // ============================================================

                decimal umumiQaytarma =
                    satislar
                        .Where(x =>
                            IsQaytarmaSutri(x.UrunAdi))
                        .Sum(
                            x =>
                                Math.Abs(x.Toplam));


                // ============================================================
                // XALİS SATIŞ
                // ============================================================

                decimal xalisSatis =
                    umumiSatis -
                    umumiQaytarma;


                // ============================================================
                // NƏĞD GROSS
                // ============================================================

                decimal nagdGross =
                    satislar
                        .Where(x =>
                            x.OdemeTipi == "Nəğd" &&
                            !IsQaytarmaSutri(x.UrunAdi) &&
                            !IsDuzelisSutri(x.UrunAdi) &&
                            x.Toplam > 0)
                        .Sum(x => x.Toplam);


                // ============================================================
                // NƏĞD QAYTARMA
                // ============================================================

                decimal nagdQaytarma =
                    satislar
                        .Where(x =>
                            x.OdemeTipi == "Nəğd" &&
                            IsQaytarmaSutri(x.UrunAdi))
                        .Sum(
                            x =>
                                Math.Abs(x.Toplam));


                // ============================================================
                // NƏĞD NET
                // ============================================================

                decimal nagdSatis =
                    nagdGross -
                    nagdQaytarma;


                // ============================================================
                // KART GROSS
                // ============================================================

                decimal kartGross =
                    satislar
                        .Where(x =>
                            x.OdemeTipi == "Kart" &&
                            !IsQaytarmaSutri(x.UrunAdi) &&
                            !IsDuzelisSutri(x.UrunAdi) &&
                            x.Toplam > 0)
                        .Sum(x => x.Toplam);


                // ============================================================
                // KART QAYTARMA
                // ============================================================

                decimal kartQaytarma =
                    satislar
                        .Where(x =>
                            x.OdemeTipi == "Kart" &&
                            IsQaytarmaSutri(x.UrunAdi))
                        .Sum(
                            x =>
                                Math.Abs(x.Toplam));


                // ============================================================
                // KART NET
                // ============================================================

                decimal kartSatis =
                    kartGross -
                    kartQaytarma;


                // ============================================================
                // XİDMƏT
                // ============================================================

                decimal umumiXidmetSatisi =
                    satislar
                        .Where(x =>
                            x.OdemeTipi == "Xidmət" &&
                            !IsQaytarmaSutri(x.UrunAdi))
                        .Sum(x => x.Toplam);


                // ============================================================
                // SATIŞ SAYI
                // ============================================================

                int satisSayi =
                    satislar
                        .Count(
                            x =>
                                !IsQaytarmaSutri(x.UrunAdi) &&
                                !IsDuzelisSutri(x.UrunAdi) &&
                                x.OdemeTipi != "Xidmət" &&
                                x.Toplam > 0);


                // ============================================================
                // XALİS GƏLİR
                // ============================================================

                decimal xalisGelir =
                    HesablaXalisGelir();


                // ============================================================
                // STATİSTİKA
                // ============================================================

                txtUmumiSatis.Text =
                    $"{xalisSatis:N2} AZN";

                txtNagdSatis.Text =
                    $"{nagdSatis:N2} AZN";

                txtKartSatis.Text =
                    $"{kartSatis:N2} AZN";

                txtXalisGelir.Text =
                    $"{xalisGelir:N2} AZN";

                txtUmumiXidmetSatisi.Text =
                    $"{umumiXidmetSatisi:N2} AZN";

                txtSatisSayi.Text =
                    satisSayi.ToString();


                // ============================================================
                // SATIŞLAR DATAGRID
                // ============================================================

                dgSatislar.ItemsSource = null;

                dgSatislar.ItemsSource =
                    satislar;


                // ============================================================
                // MƏHSULLAR DATAGRID
                // ============================================================

                dgMehsullar.ItemsSource = null;

                dgMehsullar.ItemsSource =
                    mehsullar;


                // ============================================================
                // SATIŞ YOXDURSA
                // ============================================================

                if (satislar.Count == 0)
                {
                    MessageBox.Show(
                        "Seçilmiş tarix aralığında satış tapılmadı.\n\n" +
                        $"Tarix: {baslangic:dd.MM.yyyy} - " +
                        $"{bitis:dd.MM.yyyy}\n\n" +
                        $"Ödəniş: {odemeTipi}",
                        "Məlumat yoxdur",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Hesabat yüklənərkən xəta baş verdi:\n\n" +
                    ex.Message,
                    "Sistem xətası",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }


        // ============================================================
        // QAYTARMA SƏTRİDİR?
        // ============================================================

        private bool IsQaytarmaSutri(
            string urunAdi)
        {
            if (string.IsNullOrWhiteSpace(urunAdi))
                return false;

            return urunAdi.StartsWith(
                "GERİ QAYTARMA - ",
                StringComparison.OrdinalIgnoreCase);
        }


        // ============================================================
        // NORMAL MƏHSUL ADI
        // ============================================================

        private string GetNormalUrunAdi(
            string urunAdi)
        {
            if (string.IsNullOrWhiteSpace(urunAdi))
                return "";

            if (IsQaytarmaSutri(urunAdi))
            {
                return urunAdi.Substring(
                    "GERİ QAYTARMA - ".Length);
            }

            return urunAdi;
        }


        // ============================================================
        // DÜZƏLİŞ SƏTRİDİR?
        // ============================================================

        private bool IsDuzelisSutri(
            string urunAdi)
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


        // ============================================================
        // MİQDAR OXU
        // ============================================================

        private double ParseMiqdar(
            object value)
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
                value.ToString()?
                    .Trim()
                ?? "";

            text =
                text.Replace(",", ".");

            if (double.TryParse(
                text,
                NumberStyles.Any,
                CultureInfo.InvariantCulture,
                out double result))
            {
                return result;
            }

            return 0;
        }


        // ============================================================
        // DECIMAL OXU
        // ============================================================

        private decimal ParseDecimal(
            object value)
        {
            if (value == null ||
                value == DBNull.Value)
            {
                return 0m;
            }

            if (value is decimal dec)
                return dec;

            if (value is double d)
                return Convert.ToDecimal(
                    d,
                    CultureInfo.InvariantCulture);

            if (value is float f)
                return Convert.ToDecimal(
                    f,
                    CultureInfo.InvariantCulture);

            if (value is int i)
                return i;

            if (value is long l)
                return l;

            string text =
                value.ToString()?
                    .Trim()
                ?? "";

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
                new CultureInfo("az-Latn-AZ"),
                out result))
            {
                return result;
            }

            return 0m;
        }


        // ============================================================
        // TARİX OXU
        // ============================================================

        private DateTime ParseTarix(
            object value)
        {
            if (value == null ||
                value == DBNull.Value)
            {
                return DateTime.Today;
            }

            string text =
                value.ToString()?
                    .Trim()
                ?? "";

            if (DateTime.TryParseExact(
                text,
                "yyyy-MM-dd HH:mm:ss",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out DateTime result))
            {
                return result;
            }

            if (DateTime.TryParse(
                text,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out result))
            {
                return result;
            }

            if (DateTime.TryParse(
                text,
                out result))
            {
                return result;
            }

            return DateTime.Today;
        }


        // ============================================================
        // XALİS GƏLİR
        // ============================================================

        private decimal HesablaXalisGelir()
        {
            decimal netGelir = 0;

            try
            {
                if (satislar == null ||
                    satislar.Count == 0)
                {
                    return 0m;
                }

                using (var db =
                    new SqliteConnection(
                        DatabaseHelper.ConnectionString))
                {
                    db.Open();

                    foreach (var satis in satislar)
                    {
                        // ====================================================
                        // XİDMƏT
                        // ====================================================

                        if (satis.OdemeTipi == "Xidmət")
                        {
                            netGelir +=
                                satis.Toplam;

                            continue;
                        }


                        // ====================================================
                        // DÜZƏLİŞ
                        // ====================================================

                        if (IsDuzelisSutri(
                            satis.UrunAdi))
                        {
                            netGelir +=
                                satis.Toplam;

                            continue;
                        }


                        // ====================================================
                        // QAYTARMA
                        // ====================================================

                        bool qaytarma =
                            IsQaytarmaSutri(
                                satis.UrunAdi);

                        string normalUrunAdi =
                            GetNormalUrunAdi(
                                satis.UrunAdi);


                        // ====================================================
                        // ALIŞ QİYMƏTİ
                        // ====================================================

                        decimal alisFiyati =
                            0m;

                        bool qiymetTapildi =
                            false;

                        using (var cmd =
                            new SqliteCommand(
                                @"
                                SELECT AlisFiyati
                                FROM Urunler
                                WHERE LOWER(TRIM(UrunAdi))
                                    = LOWER(TRIM(@UrunAdi))
                                LIMIT 1
                                ",
                                db))
                        {
                            cmd.Parameters.AddWithValue(
                                "@UrunAdi",
                                normalUrunAdi);

                            object result =
                                cmd.ExecuteScalar();

                            if (result != null &&
                                result != DBNull.Value)
                            {
                                try
                                {
                                    alisFiyati =
                                        Convert.ToDecimal(
                                            result,
                                            CultureInfo.InvariantCulture);

                                    qiymetTapildi =
                                        true;
                                }
                                catch
                                {
                                    if (decimal.TryParse(
                                        result.ToString(),
                                        NumberStyles.Any,
                                        CultureInfo.InvariantCulture,
                                        out alisFiyati))
                                    {
                                        qiymetTapildi =
                                            true;
                                    }
                                }
                            }
                        }


                        if (!qiymetTapildi ||
                            alisFiyati < 0)
                        {
                            System.Diagnostics.Debug.WriteLine(
                                $"XƏBƏRDARLIQ: '{normalUrunAdi}' üçün " +
                                "alış qiyməti tapılmadı.");

                            continue;
                        }


                        // ====================================================
                        // NORMAL SATIŞ
                        // ====================================================

                        if (!qaytarma)
                        {
                            decimal alisMaliyyeti =
                                alisFiyati *
                                Convert.ToDecimal(
                                    satis.Miktar);

                            netGelir +=
                                satis.Toplam -
                                alisMaliyyeti;
                        }


                        // ====================================================
                        // GERİ QAYTARMA
                        //
                        // satis.Toplam artıq mənfidir.
                        //
                        // Məs:
                        // Satış:     +0.50
                        // Alış:       0.30
                        // Gəlir:      0.20
                        //
                        // Qaytarma:
                        // Toplam:    -0.50
                        // Alış geri: +0.30
                        //
                        // Nəticə:
                        // -0.50 + 0.30 = -0.20
                        //
                        // Beləliklə əvvəlki 0.20 gəlir də silinir.
                        // ====================================================

                        else
                        {
                            decimal alisMaliyyeti =
                                alisFiyati *
                                Convert.ToDecimal(
                                    satis.Miktar);

                            netGelir +=
                                satis.Toplam +
                                alisMaliyyeti;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"Xalis gəlir hesabında xəta: " +
                    $"{ex.Message}");
            }

            return netGelir;
        }


        // ============================================================
        // STATİSTİKALARI TƏMİZLƏ
        // ============================================================

        private void StatistikaniTemizle()
        {
            txtUmumiSatis.Text =
                "0.00 AZN";

            txtNagdSatis.Text =
                "0.00 AZN";

            txtKartSatis.Text =
                "0.00 AZN";

            txtXalisGelir.Text =
                "0.00 AZN";

            txtUmumiXidmetSatisi.Text =
                "0.00 AZN";

            txtSatisSayi.Text =
                "0";

            dgSatislar.ItemsSource =
                null;

            dgMehsullar.ItemsSource =
                null;
        }


        // ============================================================
        // EXCEL
        // ============================================================

        private void btnExcel_Click(
            object sender,
            RoutedEventArgs e)
        {
            try
            {
                if (satislar == null ||
                    satislar.Count == 0)
                {
                    MessageBox.Show(
                        "Excel faylı yaratmaq üçün hesabat məlumatı yoxdur.\n\n" +
                        "Əvvəlcə hesabatı göstərin.",
                        "Məlumat yoxdur",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    return;
                }

                var saveDialog =
                    new Microsoft.Win32.SaveFileDialog
                    {
                        Filter =
                            "Excel faylı (*.xlsx)|*.xlsx",

                        FileName =
                            $"Hesabat_{DateTime.Now:dd_MM_yyyy_HH_mm}.xlsx"
                    };

                if (saveDialog.ShowDialog() != true)
                {
                    return;
                }

                using (var workbook =
                    new XLWorkbook())
                {
                    // ====================================================
                    // SATIŞLAR
                    // ====================================================

                    var worksheet =
                        workbook.Worksheets.Add(
                            "Satışlar");

                    worksheet.Cell(1, 1).Value =
                        "SATIŞ HESABATI";

                    worksheet.Range("A1:F1")
                        .Merge();

                    worksheet.Cell(2, 1).Value =
                        "Başlanğıc tarixi";

                    worksheet.Cell(2, 2).Value =
                        dpBaslangic.SelectedDate?
                            .ToString("dd.MM.yyyy");

                    worksheet.Cell(3, 1).Value =
                        "Bitiş tarixi";

                    worksheet.Cell(3, 2).Value =
                        dpBitis.SelectedDate?
                            .ToString("dd.MM.yyyy");

                    worksheet.Cell(4, 1).Value =
                        "Ödəniş növü";

                    worksheet.Cell(4, 2).Value =
                        cmbOdemeTipi.SelectedItem?
                            .ToString();

                    worksheet.Cell(6, 1).Value =
                        "№";

                    worksheet.Cell(6, 2).Value =
                        "Tarix";

                    worksheet.Cell(6, 3).Value =
                        "Məhsul / Xidmət";

                    worksheet.Cell(6, 4).Value =
                        "Miqdar";

                    worksheet.Cell(6, 5).Value =
                        "Ödəniş növü";

                    worksheet.Cell(6, 6).Value =
                        "Məbləğ";

                    int row = 7;

                    int nomre = 1;

                    foreach (var satis in satislar)
                    {
                        worksheet.Cell(
                            row,
                            1).Value =
                            nomre;

                        worksheet.Cell(
                            row,
                            2).Value =
                            satis.Tarix.ToString(
                                "dd.MM.yyyy HH:mm");

                        worksheet.Cell(
                            row,
                            3).Value =
                            satis.UrunAdi;

                        worksheet.Cell(
                            row,
                            4).Value =
                            satis.Miktar;

                        worksheet.Cell(
                            row,
                            5).Value =
                            satis.OdemeTipi;

                        worksheet.Cell(
                            row,
                            6).Value =
                            satis.Toplam;

                        row++;

                        nomre++;
                    }


                    // ====================================================
                    // STATİSTİKA
                    // ====================================================

                    row += 2;

                    decimal umumiSatis =
                        satislar
                            .Where(x =>
                                !IsQaytarmaSutri(x.UrunAdi) &&
                                x.Toplam > 0 &&
                                !IsDuzelisSutri(x.UrunAdi))
                            .Sum(x => x.Toplam);

                    decimal umumiQaytarma =
                        satislar
                            .Where(x =>
                                IsQaytarmaSutri(x.UrunAdi))
                            .Sum(
                                x =>
                                    Math.Abs(x.Toplam));

                    decimal xalisSatis =
                        umumiSatis -
                        umumiQaytarma;


                    worksheet.Cell(row, 1).Value =
                        "Ümumi satış:";

                    worksheet.Cell(row, 2).Value =
                        umumiSatis;

                    row++;


                    worksheet.Cell(row, 1).Value =
                        "Qaytarmalar:";

                    worksheet.Cell(row, 2).Value =
                        umumiQaytarma;

                    row++;


                    worksheet.Cell(row, 1).Value =
                        "Xalis satış:";

                    worksheet.Cell(row, 2).Value =
                        xalisSatis;

                    row++;


                    decimal nagdGross =
                        satislar
                            .Where(x =>
                                x.OdemeTipi == "Nəğd" &&
                                !IsQaytarmaSutri(x.UrunAdi) &&
                                !IsDuzelisSutri(x.UrunAdi) &&
                                x.Toplam > 0)
                            .Sum(x => x.Toplam);

                    decimal nagdQaytarma =
                        satislar
                            .Where(x =>
                                x.OdemeTipi == "Nəğd" &&
                                IsQaytarmaSutri(x.UrunAdi))
                            .Sum(
                                x =>
                                    Math.Abs(x.Toplam));

                    decimal nagdSatis =
                        nagdGross -
                        nagdQaytarma;


                    worksheet.Cell(row, 1).Value =
                        "Nəğd satış:";

                    worksheet.Cell(row, 2).Value =
                        nagdSatis;

                    row++;


                    decimal kartGross =
                        satislar
                            .Where(x =>
                                x.OdemeTipi == "Kart" &&
                                !IsQaytarmaSutri(x.UrunAdi) &&
                                !IsDuzelisSutri(x.UrunAdi) &&
                                x.Toplam > 0)
                            .Sum(x => x.Toplam);

                    decimal kartQaytarma =
                        satislar
                            .Where(x =>
                                x.OdemeTipi == "Kart" &&
                                IsQaytarmaSutri(x.UrunAdi))
                            .Sum(
                                x =>
                                    Math.Abs(x.Toplam));

                    decimal kartSatis =
                        kartGross -
                        kartQaytarma;


                    worksheet.Cell(row, 1).Value =
                        "Kart satış:";

                    worksheet.Cell(row, 2).Value =
                        kartSatis;

                    row++;


                    decimal xidmetSatisi =
                        satislar
                            .Where(x =>
                                x.OdemeTipi == "Xidmət" &&
                                !IsQaytarmaSutri(x.UrunAdi))
                            .Sum(x => x.Toplam);


                    worksheet.Cell(row, 1).Value =
                        "Xidmət satışı:";

                    worksheet.Cell(row, 2).Value =
                        xidmetSatisi;

                    row++;


                    decimal xalisGelir =
                        HesablaXalisGelir();


                    worksheet.Cell(row, 1).Value =
                        "Xalis gəlir:";

                    worksheet.Cell(row, 2).Value =
                        xalisGelir;

                    row++;


                    int satisSayi =
                        satislar
                            .Count(
                                x =>
                                    !IsQaytarmaSutri(x.UrunAdi) &&
                                    !IsDuzelisSutri(x.UrunAdi) &&
                                    x.OdemeTipi != "Xidmət" &&
                                    x.Toplam > 0);


                    worksheet.Cell(row, 1).Value =
                        "Satış sətri:";

                    worksheet.Cell(row, 2).Value =
                        satisSayi;


                    // ====================================================
                    // EXCEL FORMAT
                    // ====================================================

                    worksheet.Columns()
                        .AdjustToContents();

                    worksheet.Column(3).Width =
                        35;

                    worksheet.Column(6)
                        .Style
                        .NumberFormat
                        .Format =
                        "#,##0.00";

                    worksheet.Range("A1:F1")
                        .Style
                        .Font
                        .Bold = true;

                    worksheet.Cell(1, 1)
                        .Style
                        .Font
                        .FontSize = 16;

                    worksheet.Range("A6:F6")
                        .Style
                        .Font
                        .Bold = true;


                    // ====================================================
                    // MƏHSULLAR
                    // ====================================================

                    var productSheet =
                        workbook.Worksheets.Add(
                            "Məhsullar");

                    productSheet.Cell(1, 1).Value =
                        "MƏHSUL ÜZRƏ NET SATIŞ HESABATI";

                    productSheet.Range("A1:C1")
                        .Merge();

                    productSheet.Cell(1, 1)
                        .Style
                        .Font
                        .Bold = true;

                    productSheet.Cell(1, 1)
                        .Style
                        .Font
                        .FontSize = 16;

                    productSheet.Cell(3, 1).Value =
                        "Məhsulun adı";

                    productSheet.Cell(3, 2).Value =
                        "Net miqdar";

                    productSheet.Cell(3, 3).Value =
                        "Net satış məbləği";

                    int productRow = 4;

                    foreach (var mehsul in mehsullar)
                    {
                        productSheet.Cell(
                            productRow,
                            1).Value =
                            mehsul.UrunAdi;

                        productSheet.Cell(
                            productRow,
                            2).Value =
                            mehsul.Miktar;

                        productSheet.Cell(
                            productRow,
                            3).Value =
                            mehsul.Toplam;

                        productRow++;
                    }

                    productSheet.Range("A3:C3")
                        .Style
                        .Font
                        .Bold = true;

                    productSheet.Columns()
                        .AdjustToContents();

                    productSheet.Column(1).Width =
                        35;

                    productSheet.Column(3)
                        .Style
                        .NumberFormat
                        .Format =
                        "#,##0.00";


                    // ====================================================
                    // XİDMƏTLƏR
                    // ====================================================

                    var serviceSheet =
                        workbook.Worksheets.Add(
                            "Xidmətlər");

                    serviceSheet.Cell(1, 1).Value =
                        "XİDMƏT SATIŞLARI";

                    serviceSheet.Range("A1:D1")
                        .Merge();

                    serviceSheet.Cell(1, 1)
                        .Style
                        .Font
                        .Bold = true;

                    serviceSheet.Cell(1, 1)
                        .Style
                        .Font
                        .FontSize = 16;

                    serviceSheet.Cell(3, 1).Value =
                        "№";

                    serviceSheet.Cell(3, 2).Value =
                        "Xidmətin adı";

                    serviceSheet.Cell(3, 3).Value =
                        "Tarix";

                    serviceSheet.Cell(3, 4).Value =
                        "Məbləğ";

                    int serviceRow = 4;

                    int serviceNumber = 1;

                    foreach (var xidmet in satislar
                        .Where(x =>
                            x.OdemeTipi == "Xidmət" &&
                            !IsQaytarmaSutri(x.UrunAdi)))
                    {
                        serviceSheet.Cell(
                            serviceRow,
                            1).Value =
                            serviceNumber;

                        serviceSheet.Cell(
                            serviceRow,
                            2).Value =
                            xidmet.UrunAdi;

                        serviceSheet.Cell(
                            serviceRow,
                            3).Value =
                            xidmet.Tarix.ToString(
                                "dd.MM.yyyy HH:mm");

                        serviceSheet.Cell(
                            serviceRow,
                            4).Value =
                            xidmet.Toplam;

                        serviceRow++;

                        serviceNumber++;
                    }

                    serviceSheet.Range("A3:D3")
                        .Style
                        .Font
                        .Bold = true;

                    serviceSheet.Columns()
                        .AdjustToContents();

                    serviceSheet.Column(2).Width =
                        35;

                    serviceSheet.Column(4)
                        .Style
                        .NumberFormat
                        .Format =
                        "#,##0.00";


                    // ====================================================
                    // YADDA SAXLA
                    // ====================================================

                    workbook.SaveAs(
                        saveDialog.FileName);
                }

                MessageBox.Show(
                    "Hesabat Excel faylına uğurla çıxarıldı.",
                    "Əməliyyat uğurlu oldu",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Excel faylı yaradılarkən xəta baş verdi:\n\n" +
                    ex.Message,
                    "Sistem xətası",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }


    // ================================================================
    // SATIŞ MODELİ
    // ================================================================

    public class SatisHesabatModel
    {
        public long Id { get; set; }

        public DateTime Tarix { get; set; }

        public string Musteri { get; set; } = "-";

        public string UrunAdi { get; set; } = "";

        public double Miktar { get; set; }

        public string OdemeTipi { get; set; } = "";

        public string SatisQrupId { get; set; } = "";

        public decimal Toplam { get; set; }
    }


    // ================================================================
    // MƏHSUL MODELİ
    // ================================================================

    public class UrunHesabatModel
    {
        public string UrunAdi { get; set; } = "";

        public double Miktar { get; set; }

        public decimal Toplam { get; set; }
    }
}