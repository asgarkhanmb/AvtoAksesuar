using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace PcKod.UI.Views
{
    public partial class GeriQaytarmaWindow : Window
    {
        private readonly List<SatisQaytarmaModel> satislar =
            new List<SatisQaytarmaModel>();

        private SatisQaytarmaModel secilenSatis;

        private readonly CultureInfo azCulture =
            new CultureInfo("az-Latn-AZ");

        public GeriQaytarmaWindow()
        {
            InitializeComponent();

            dpTarix.SelectedDate = DateTime.Today;

            SatislariYukle();
        }

        // ============================================================
        // SATIŞLARI YÜKLƏ
        // ============================================================

        private void SatislariYukle()
        {
            try
            {
                satislar.Clear();

                DateTime tarix =
                    dpTarix.SelectedDate?.Date ?? DateTime.Today;

                string tarixText = tarix.ToString(
                    "yyyy-MM-dd",
                    CultureInfo.InvariantCulture);

                using (var db = new SqliteConnection(
                    PcKod.UI.Data.DatabaseHelper.ConnectionString))
                {
                    db.Open();

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
                        WHERE Tarih LIKE @tarix
                          AND UrunAdi NOT LIKE 'GERİ QAYTARMA - %'
                          AND UrunAdi != 'Kassa endirimi / Yuvarlaqlaşdırma'
                          AND UrunAdi != 'Əlavə ödəniş / Fərq'
                          AND Miktar > 0
                          AND ToplamTutar > 0
                        ORDER BY Id DESC";

                    using (var cmd = new SqliteCommand(sql, db))
                    {
                        cmd.Parameters.AddWithValue(
                            "@tarix",
                            tarixText + "%");

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                long id = 0;

                                long.TryParse(
                                    reader["Id"]?.ToString(),
                                    out id);

                                string urunAdi =
                                    reader["UrunAdi"]?.ToString() ?? "";

                                double originalMiqdar =
                                    ParseMiqdar(reader["Miktar"]);

                                decimal originalMebleg =
                                    ParseDecimal(reader["ToplamTutar"]);

                                string odeme =
                                    reader["OdemeYontemi"]?.ToString() ?? "";

                                string tarixTextDb =
                                    reader["Tarih"]?.ToString() ?? "";

                                DateTime tarixDate;

                                if (!DateTime.TryParseExact(
                                    tarixTextDb,
                                    "yyyy-MM-dd HH:mm:ss",
                                    CultureInfo.InvariantCulture,
                                    DateTimeStyles.None,
                                    out tarixDate))
                                {
                                    if (!DateTime.TryParse(
                                        tarixTextDb,
                                        CultureInfo.InvariantCulture,
                                        DateTimeStyles.None,
                                        out tarixDate))
                                    {
                                        tarixDate = tarix;
                                    }
                                }

                                string satisQrupId =
                                    reader["SatisQrupId"]?.ToString() ?? "";

                                if (string.IsNullOrWhiteSpace(satisQrupId))
                                {
                                    satisQrupId = "OLD-" + id;
                                }

                                long cekNomresi = GetCekNomresi(
                                    db,
                                    satisQrupId,
                                    id);

                                double qaytarilanMiqdar =
                                    GetArtıqQaytarilanMiqdar(
                                        db,
                                        id,
                                        urunAdi,
                                        satisQrupId);

                                double qalanMiqdar =
                                    originalMiqdar - qaytarilanMiqdar;

                                if (qalanMiqdar < 0)
                                    qalanMiqdar = 0;

                                if (qalanMiqdar <= 0)
                                    continue;

                                // 1 ƏDƏDİN QİYMƏTİ İLK SATIŞDAN HESABLANIR.
                                decimal vahidQiymet = originalMiqdar > 0
                                    ? originalMebleg / (decimal)originalMiqdar
                                    : 0m;

                                // CƏDVƏLDƏ QALAN MİQDARA UYĞUN MƏBLƏĞ.
                                // Məsələn: 2 ədəd x 2 AZN = 4 AZN.
                                // 1 ədəd qaytarıldıqdan sonra 1 ədəd x 2 AZN = 2 AZN.
                                decimal qalanMebleg = Math.Round(
                                    vahidQiymet * (decimal)qalanMiqdar,
                                    2,
                                    MidpointRounding.AwayFromZero);

                                satislar.Add(new SatisQaytarmaModel
                                {
                                    Id = id,
                                    CekNomresi = cekNomresi,
                                    SatisQrupId = satisQrupId,
                                    UrunAdi = urunAdi,

                                    // QALAN QAYTARILA BİLƏN MİQDAR
                                    Miktar = qalanMiqdar,

                                    // İLK SATIŞDAKI MİQDAR
                                    OriginalMiktar = originalMiqdar,

                                    ArtıqQaytarilanMiqdar =
                                        qaytarilanMiqdar,

                                    // QALAN MİQDARA UYĞUN MƏBLƏĞ
                                    ToplamTutar = qalanMebleg,

                                    // İLK SATIŞIN DƏYİŞMƏYƏN MƏBLƏĞİ
                                    OriginalToplamTutar = originalMebleg,

                                    OdemeYontemi = odeme,
                                    Tarih = tarixDate
                                });
                            }
                        }
                    }
                }

                secilenSatis = null;

                dgSatishlar.SelectedItem = null;
                dgSatishlar.ItemsSource = null;
                dgSatishlar.ItemsSource = satislar;

                txtSecilenUrun.Text = "-";
                txtSecilenCek.Text = "Çek №: -";
                txtQaytarmaMiqdar.Text = "1";

                txtAxtarisNeticesi.Text =
                    $"{satislar.Count} məhsul satışı";

                if (satislar.Count == 0)
                {
                    txtAxtarisNeticesi.Text =
                        "Bu tarixdə qaytarılacaq satış yoxdur.";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Satışlar yüklənərkən xəta baş verdi:\n\n" +
                    ex.Message,
                    "Sistem xətası",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        // ============================================================
        // ARTİQ QAYTARILAN MİQDAR
        // ============================================================

        private double GetArtıqQaytarilanMiqdar(
            SqliteConnection db,
            long originalSaleId,
            string urunAdi,
            string satisQrupId)
        {
            try
            {
                string returnPrefix =
                    "RETURN-FOR-" + originalSaleId + "-";

                string sql = @"
                    SELECT COALESCE(SUM(Miktar), 0)
                    FROM Satislar
                    WHERE UrunAdi = @returnName
                      AND SatisQrupId LIKE @returnPrefix
                      AND ToplamTutar < 0";

                using (var cmd = new SqliteCommand(sql, db))
                {
                    cmd.Parameters.AddWithValue(
                        "@returnName",
                        "GERİ QAYTARMA - " + urunAdi);

                    cmd.Parameters.AddWithValue(
                        "@returnPrefix",
                        returnPrefix + "%");

                    return ParseMiqdar(cmd.ExecuteScalar());
                }
            }
            catch
            {
                return 0;
            }
        }

        // ============================================================
        // ÇEK NÖMRƏSİ
        // ============================================================

        private long GetCekNomresi(
            SqliteConnection db,
            string satisQrupId,
            long fallbackId)
        {
            if (string.IsNullOrWhiteSpace(satisQrupId))
                return fallbackId;

            try
            {
                string sql = @"
                    SELECT MAX(Id)
                    FROM Satislar
                    WHERE SatisQrupId = @q
                      AND UrunAdi NOT LIKE 'GERİ QAYTARMA - %'";

                using (var cmd = new SqliteCommand(sql, db))
                {
                    cmd.Parameters.AddWithValue("@q", satisQrupId);

                    object result = cmd.ExecuteScalar();

                    if (result != null && result != DBNull.Value)
                    {
                        return Convert.ToInt64(result);
                    }
                }
            }
            catch
            {
            }

            return fallbackId;
        }

        // ============================================================
        // DECIMAL OXU
        // ============================================================

        private decimal ParseDecimal(object value)
        {
            if (value == null || value == DBNull.Value)
                return 0m;

            if (value is decimal decimalValue)
                return decimalValue;

            if (value is double doubleValue)
                return Convert.ToDecimal(
                    doubleValue,
                    CultureInfo.InvariantCulture);

            if (value is float floatValue)
                return Convert.ToDecimal(
                    floatValue,
                    CultureInfo.InvariantCulture);

            if (value is int intValue)
                return intValue;

            if (value is long longValue)
                return longValue;

            string text = value.ToString()?.Trim() ?? "";

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

            return 0m;
        }

        // ============================================================
        // MİQDAR OXU
        // ============================================================

        private double ParseMiqdar(object value)
        {
            if (value == null || value == DBNull.Value)
                return 0;

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

            string text = value.ToString()?.Trim() ?? "";
            text = text.Replace(",", ".");

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

        // ============================================================
        // TARİX DƏYİŞƏNDƏ
        // ============================================================

        private void dpTarix_SelectedDateChanged(
            object sender,
            SelectionChangedEventArgs e)
        {
            if (!IsInitialized)
                return;

            if (txtCekNomresiAxtar != null)
                txtCekNomresiAxtar.Text = "";

            if (txtMehsulAxtar != null)
                txtMehsulAxtar.Text = "";

            SatislariYukle();
        }

        // ============================================================
        // AXTAR
        // ============================================================

        private void btnAxtar_Click(
            object sender,
            RoutedEventArgs e)
        {
            Axtar();
        }

        private void txtAxtaris_KeyDown(
            object sender,
            KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Axtar();
                e.Handled = true;
            }
        }

        private void Axtar()
        {
            string cekText =
                txtCekNomresiAxtar.Text?.Trim() ?? "";

            string mehsulText =
                txtMehsulAxtar.Text?.Trim() ?? "";

            IEnumerable<SatisQaytarmaModel> netice = satislar;

            if (!string.IsNullOrWhiteSpace(cekText))
            {
                if (!long.TryParse(cekText, out long cekNomresi))
                {
                    MessageBox.Show(
                        "Çek nömrəsini düzgün daxil edin.",
                        "Yanlış çek nömrəsi",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    txtCekNomresiAxtar.Focus();
                    return;
                }

                netice = netice.Where(
                    x => x.CekNomresi == cekNomresi);
            }

            if (!string.IsNullOrWhiteSpace(mehsulText))
            {
                netice = netice.Where(
                    x => x.UrunAdi.Contains(
                        mehsulText,
                        StringComparison.OrdinalIgnoreCase));
            }

            var siyahi = netice.ToList();

            secilenSatis = null;
            dgSatishlar.SelectedItem = null;

            txtSecilenUrun.Text = "-";
            txtSecilenCek.Text = "Çek №: -";
            txtQaytarmaMiqdar.Text = "1";

            dgSatishlar.ItemsSource = null;
            dgSatishlar.ItemsSource = siyahi;

            txtAxtarisNeticesi.Text = siyahi.Count == 0
                ? "Nəticə tapılmadı."
                : $"{siyahi.Count} nəticə tapıldı.";
        }

        // ============================================================
        // AXTARIŞI TƏMİZLƏ
        // ============================================================

        private void btnAxtarTemizle_Click(
            object sender,
            RoutedEventArgs e)
        {
            txtCekNomresiAxtar.Text = "";
            txtMehsulAxtar.Text = "";

            secilenSatis = null;

            dgSatishlar.SelectedItem = null;
            dgSatishlar.ItemsSource = null;
            dgSatishlar.ItemsSource = satislar;

            txtAxtarisNeticesi.Text =
                $"{satislar.Count} məhsul satışı";

            txtSecilenUrun.Text = "-";
            txtSecilenCek.Text = "Çek №: -";
            txtQaytarmaMiqdar.Text = "1";
        }

        // ============================================================
        // DATAGRID SEÇİMİ
        // ============================================================

        private void dgSatishlar_SelectionChanged(
            object sender,
            SelectionChangedEventArgs e)
        {
            var selected =
                dgSatishlar.SelectedItem as SatisQaytarmaModel;

            if (selected == null)
                return;

            secilenSatis = selected;

            txtSecilenUrun.Text = selected.UrunAdi;
            txtSecilenCek.Text = "Çek №: " + selected.CekNomresi;
            txtQaytarmaMiqdar.Text = "1";
        }

        // ============================================================
        // DATAGRID SƏTRİNƏ KLİKLƏ SEÇ
        // ============================================================

        private void dgSatishlar_PreviewMouseLeftButtonDown(
            object sender,
            MouseButtonEventArgs e)
        {
            DependencyObject element =
                e.OriginalSource as DependencyObject;

            while (element != null)
            {
                if (element is DataGridRow row)
                {
                    var item = row.Item as SatisQaytarmaModel;

                    if (item != null)
                    {
                        dgSatishlar.SelectedItem = item;
                        row.IsSelected = true;
                        dgSatishlar.ScrollIntoView(item);

                        secilenSatis = item;

                        txtSecilenUrun.Text = item.UrunAdi;
                        txtSecilenCek.Text =
                            "Çek №: " + item.CekNomresi;

                        txtQaytarmaMiqdar.Text = "1";
                    }

                    break;
                }

                element = VisualTreeHelper.GetParent(element);
            }
        }

        // ============================================================
        // GERİ QAYTAR
        // ============================================================

        private void btnGeriQaytar_Click(
            object sender,
            RoutedEventArgs e)
        {
            var selected =
                dgSatishlar.SelectedItem as SatisQaytarmaModel;

            if (selected != null)
                secilenSatis = selected;

            if (secilenSatis == null)
            {
                MessageBox.Show(
                    "Zəhmət olmasa geri qaytarılacaq məhsulu seçin.",
                    "Məhsul seçilməyib",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            string miqdarText =
                txtQaytarmaMiqdar.Text?.Trim() ?? "";

            miqdarText = miqdarText.Replace(",", ".");

            if (!double.TryParse(
                miqdarText,
                NumberStyles.Any,
                CultureInfo.InvariantCulture,
                out double qaytarmaMiqdar))
            {
                MessageBox.Show(
                    "Geri qaytarılacaq miqdarı düzgün daxil edin.",
                    "Yanlış miqdar",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                txtQaytarmaMiqdar.Focus();
                return;
            }

            if (qaytarmaMiqdar <= 0)
            {
                MessageBox.Show(
                    "Miqdar 0-dan böyük olmalıdır.",
                    "Yanlış miqdar",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            if (qaytarmaMiqdar >
                secilenSatis.Miktar + 0.000001)
            {
                MessageBox.Show(
                    $"Maksimum {secilenSatis.Miktar:N2} miqdar geri qaytara bilərsiniz.",
                    "Yanlış miqdar",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            // ORİJİNAL SATIŞ MƏLUMATLARINI BAZADAN OXU
            decimal originalMebleg = 0m;
            double originalMiqdar = 0;

            try
            {
                using (var qiymetDb = new SqliteConnection(
                    PcKod.UI.Data.DatabaseHelper.ConnectionString))
                {
                    qiymetDb.Open();

                    const string sql = @"
                        SELECT Miktar, ToplamTutar
                        FROM Satislar
                        WHERE Id = @id
                          AND Miktar > 0
                          AND ToplamTutar > 0
                        LIMIT 1";

                    using (var cmd = new SqliteCommand(
                        sql,
                        qiymetDb))
                    {
                        cmd.Parameters.AddWithValue(
                            "@id",
                            secilenSatis.Id);

                        using (var reader = cmd.ExecuteReader())
                        {
                            if (!reader.Read())
                            {
                                MessageBox.Show(
                                    "Orijinal satış məlumatı tapılmadı.",
                                    "Xəta",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Warning);

                                return;
                            }

                            originalMiqdar =
                                ParseMiqdar(reader["Miktar"]);

                            originalMebleg =
                                ParseDecimal(reader["ToplamTutar"]);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Satış məbləği oxunarkən xəta baş verdi:\n\n" +
                    ex.Message,
                    "Xəta",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                return;
            }

            if (originalMiqdar <= 0 || originalMebleg <= 0)
            {
                MessageBox.Show(
                    "Orijinal satışın miqdarı və ya məbləği düzgün deyil.",
                    "Xəta",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            // VAHİD QİYMƏT İLK SATIŞ MƏBLƏĞİNDƏN HESABLANIR.
            decimal vahidQiymet = Math.Round(
                originalMebleg / (decimal)originalMiqdar,
                2,
                MidpointRounding.AwayFromZero);

            // QAYTARILAN MİQDARIN MƏBLƏĞİ
            decimal qaytarmaMeblegi = Math.Round(
                vahidQiymet * (decimal)qaytarmaMiqdar,
                2,
                MidpointRounding.AwayFromZero);

            var result = MessageBox.Show(
                $"Çek №: {secilenSatis.CekNomresi}\n" +
                $"Məhsul: {secilenSatis.UrunAdi}\n" +
                $"Miqdar: {qaytarmaMiqdar:N2}\n" +
                $"1 ədədin qiyməti: {vahidQiymet.ToString("N2", azCulture)} AZN\n" +
                $"Qaytarılan məbləğ: {qaytarmaMeblegi.ToString("N2", azCulture)} AZN\n\n" +
                "Məhsul anbara geri əlavə ediləcək.\n\n" +
                "Əməliyyatı təsdiqləyirsiniz?",
                "Geri qaytarma təsdiqi",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
                return;

            using (var db = new SqliteConnection(
                PcKod.UI.Data.DatabaseHelper.ConnectionString))
            {
                db.Open();

                using (var transaction = db.BeginTransaction())
                {
                    try
                    {
                        string barkod = null;

                        using (var findCmd = new SqliteCommand(
                            @"SELECT Barkod
                              FROM Urunler
                              WHERE LOWER(TRIM(UrunAdi)) =
                                    LOWER(TRIM(@urunAdi))
                              LIMIT 1",
                            db,
                            transaction))
                        {
                            findCmd.Parameters.AddWithValue(
                                "@urunAdi",
                                secilenSatis.UrunAdi);

                            object resultBarkod =
                                findCmd.ExecuteScalar();

                            if (resultBarkod != null &&
                                resultBarkod != DBNull.Value)
                            {
                                barkod = resultBarkod.ToString();
                            }
                        }

                        if (string.IsNullOrWhiteSpace(barkod))
                        {
                            transaction.Rollback();

                            MessageBox.Show(
                                "Bu məhsul anbar məhsulları arasında tapılmadı.\n\n" +
                                "Məhsul adı:\n" +
                                secilenSatis.UrunAdi +
                                "\n\nMəhsulun anbar qeydiyyatı olmadığı üçün " +
                                "geri qaytarma həyata keçirilmədi.",
                                "Məhsul tapılmadı",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);

                            return;
                        }

                        // QAYTARMA MİQDARINI BAZADAKI QEYDLƏRLƏ YENİDƏN YOXLAYIRIQ.
                        double artiqQaytarilib =
                            GetArtıqQaytarilanMiqdar(
                                db,
                                secilenSatis.Id,
                                secilenSatis.UrunAdi,
                                secilenSatis.SatisQrupId);

                        double maksimumQaytarma =
                            originalMiqdar - artiqQaytarilib;

                        if (maksimumQaytarma < 0)
                            maksimumQaytarma = 0;

                        if (qaytarmaMiqdar >
                            maksimumQaytarma + 0.000001)
                        {
                            transaction.Rollback();

                            MessageBox.Show(
                                $"Bu satışdan maksimum {maksimumQaytarma:N2} miqdar qaytarmaq olar.",
                                "Qaytarma limiti",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);

                            SatislariYukle();
                            return;
                        }

                        // ANBARA GERİ ƏLAVƏ ET
                        using (var stockCmd = new SqliteCommand(
                            @"UPDATE Urunler
                              SET StokMiktari = StokMiktari + @m
                              WHERE Barkod = @b",
                            db,
                            transaction))
                        {
                            stockCmd.Parameters.AddWithValue(
                                "@m",
                                qaytarmaMiqdar);

                            stockCmd.Parameters.AddWithValue(
                                "@b",
                                barkod);

                            int affected =
                                stockCmd.ExecuteNonQuery();

                            if (affected == 0)
                            {
                                throw new Exception(
                                    "Məhsulun anbar qalığı yenilənmədi.");
                            }
                        }

                        // GERİ QAYTARMA SƏTRİ
                        string returnGroupId =
                            "RETURN-FOR-" +
                            secilenSatis.Id +
                            "-" +
                            secilenSatis.SatisQrupId;

                        using (var returnCmd = new SqliteCommand(
                            @"INSERT INTO Satislar
                              (
                                  UrunAdi,
                                  Miktar,
                                  ToplamTutar,
                                  OdemeYontemi,
                                  Tarih,
                                  SatisQrupId
                              )
                              VALUES
                              (
                                  @a,
                                  @m,
                                  @t,
                                  @y,
                                  @d,
                                  @q
                              )",
                            db,
                            transaction))
                        {
                            returnCmd.Parameters.AddWithValue(
                                "@a",
                                "GERİ QAYTARMA - " +
                                secilenSatis.UrunAdi);

                            returnCmd.Parameters.AddWithValue(
                                "@m",
                                qaytarmaMiqdar);

                            returnCmd.Parameters.AddWithValue(
                                "@t",
                                -qaytarmaMeblegi);

                            returnCmd.Parameters.AddWithValue(
                                "@y",
                                secilenSatis.OdemeYontemi);

                            returnCmd.Parameters.AddWithValue(
                                "@d",
                                DateTime.Now.ToString(
                                    "yyyy-MM-dd HH:mm:ss",
                                    CultureInfo.InvariantCulture));

                            returnCmd.Parameters.AddWithValue(
                                "@q",
                                returnGroupId);

                            returnCmd.ExecuteNonQuery();
                        }

                        transaction.Commit();

                        MessageBox.Show(
                            "Məhsul uğurla geri qaytarıldı.\n\n" +
                            $"Çek №: {secilenSatis.CekNomresi}\n" +
                            $"Məhsul: {secilenSatis.UrunAdi}\n" +
                            $"Miqdar: {qaytarmaMiqdar:N2}\n" +
                            $"1 ədədin qiyməti: {vahidQiymet.ToString("N2", azCulture)} AZN\n" +
                            $"Qaytarılan məbləğ: {qaytarmaMeblegi.ToString("N2", azCulture)} AZN\n\n" +
                            "Orijinal satış saxlanıldı.\n" +
                            "Məhsul anbara əlavə edildi.",
                            "Geri qaytarma tamamlandı",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);

                        SatislariYukle();

                        secilenSatis = null;
                        dgSatishlar.SelectedItem = null;

                        txtSecilenUrun.Text = "-";
                        txtSecilenCek.Text = "Çek №: -";
                        txtQaytarmaMiqdar.Text = "1";

                        txtCekNomresiAxtar.Text = "";
                        txtMehsulAxtar.Text = "";
                    }
                    catch (Exception ex)
                    {
                        try
                        {
                            transaction.Rollback();
                        }
                        catch
                        {
                        }

                        MessageBox.Show(
                            "Geri qaytarma zamanı xəta baş verdi:\n\n" +
                            ex.Message,
                            "Sistem xətası",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
                    }
                }
            }
        }
    }

    // ================================================================
    // MODEL
    // ================================================================

    public class SatisQaytarmaModel
    {
        public long Id { get; set; }

        public long CekNomresi { get; set; }

        public string SatisQrupId { get; set; } = "";

        public string UrunAdi { get; set; } = "";

        // QALAN QAYTARILA BİLƏN MİQDAR
        public double Miktar { get; set; }

        // ORİJİNAL SATIŞ MİQDARI
        public double OriginalMiktar { get; set; }

        // ƏVVƏL QAYTARILMIŞ MİQDAR
        public double ArtıqQaytarilanMiqdar { get; set; }

        // QALAN MİQDARA UYĞUN ÜMUMİ MƏBLƏĞ
        public decimal ToplamTutar { get; set; }

        // ORİJİNAL SATIŞIN ÜMUMİ MƏBLƏĞİ
        public decimal OriginalToplamTutar { get; set; }

        public string OdemeYontemi { get; set; } = "";

        public DateTime Tarih { get; set; }

        public string TarixMetni
        {
            get
            {
                return Tarih.ToString("dd.MM.yyyy HH:mm");
            }
        }
    }
}
