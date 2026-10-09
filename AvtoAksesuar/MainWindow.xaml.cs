using Microsoft.Data.Sqlite;
using PcKod.UI.Data;
using PcKod.UI.Data.Helpers;
using PcKod.UI.Models;
using PcKod.UI.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace PcKod.UI
{
    public partial class MainWindow : Window
    {
        // ============================================================
        // SƏBƏT
        // ============================================================

        public ObservableCollection<SəbətMəhsul> Sepet { get; set; }
            = new ObservableCollection<SəbətMəhsul>();


        // ============================================================
        // ƏL İLƏ DƏYİŞDİRİLMİŞ ÜMUMİ MƏBLƏĞLƏR
        // ============================================================

        private readonly Dictionary<SəbətMəhsul, decimal> manualTotalAmounts
            = new Dictionary<SəbətMəhsul, decimal>();


        // ============================================================
        // GİRİŞ STATUSU
        // ============================================================

        private bool isLoggedIn = false;


        // ============================================================
        // TEMA
        // ============================================================

        private bool isDarkTheme = false;


        // ============================================================
        // CONSTRUCTOR
        // ============================================================

        public MainWindow()
        {
            InitializeComponent();

            dgSepet.ItemsSource = Sepet;

            EnsureSalesGroupColumn();

            ApplyTheme(false);

            txtBarkodOkuyucu.Focus();

            CalculateTotal();
        }


        // ============================================================
        // TEMA TƏTBİQ ET
        // ============================================================

        private void ApplyTheme(bool dark)
        {
            isDarkTheme = dark;

            if (dark)
            {
                // ====================================================
                // GECƏ TEMASI
                // ====================================================

                Resources["WindowBackgroundBrush"] =
                    new SolidColorBrush(
                        Color.FromRgb(12, 15, 20));

                Resources["PanelBackgroundBrush"] =
                    new SolidColorBrush(
                        Color.FromRgb(23, 28, 36));

                Resources["TextBrush"] =
                    new SolidColorBrush(
                        Color.FromRgb(245, 248, 252));

                Resources["SecondaryTextBrush"] =
                    new SolidColorBrush(
                        Color.FromRgb(185, 198, 212));

                Resources["BorderBrushTheme"] =
                    new SolidColorBrush(
                        Color.FromRgb(65, 78, 94));

                Resources["HeaderBackgroundBrush"] =
                    new SolidColorBrush(
                        Color.FromRgb(35, 46, 60));

                Resources["HeaderTextBrush"] =
                    new SolidColorBrush(
                        Color.FromRgb(125, 195, 245));

                Resources["GridLineBrush"] =
                    new SolidColorBrush(
                        Color.FromRgb(52, 62, 75));

                Resources["AlternateRowBrush"] =
                    new SolidColorBrush(
                        Color.FromRgb(28, 34, 43));

                Resources["HoverRowBrush"] =
                    new SolidColorBrush(
                        Color.FromRgb(40, 52, 67));

                Resources["SelectedRowBrush"] =
                    new SolidColorBrush(
                        Color.FromRgb(45, 78, 110));

                Resources["SelectedTextBrush"] =
                    new SolidColorBrush(
                        Color.FromRgb(255, 255, 255));

                Resources["InputBackgroundBrush"] =
                    new SolidColorBrush(
                        Color.FromRgb(30, 37, 47));

                Resources["InputBorderBrush"] =
                    new SolidColorBrush(
                        Color.FromRgb(75, 135, 180));

                // ====================================================
                // ÜMUMİ MƏBLƏĞ - GECƏDƏ PARLAQ YAŞIL
                // ====================================================

                Resources["TotalTextBrush"] =
                    new SolidColorBrush(
                        Color.FromRgb(80, 205, 255));

                Resources["TotalAmountBrush"] =
                    new SolidColorBrush(
                        Color.FromRgb(55, 220, 125));


                // ====================================================
                // DÜYMƏ RƏNGLƏRİ - GECƏ
                // ====================================================

                Resources["SalesButtonBrush"] =
                    new SolidColorBrush(
                        Color.FromRgb(83, 132, 180));

                Resources["LoginButtonBrush"] =
                    new SolidColorBrush(
                        Color.FromRgb(88, 102, 120));

                Resources["ThemeButtonBrush"] =
                    new SolidColorBrush(
                        Color.FromRgb(65, 78, 92));

                Resources["CashButtonBrush"] =
                    new SolidColorBrush(
                        Color.FromRgb(45, 165, 105));

                Resources["CardButtonBrush"] =
                    new SolidColorBrush(
                        Color.FromRgb(55, 130, 190));

                Resources["ReturnButtonBrush"] =
                    new SolidColorBrush(
                        Color.FromRgb(205, 130, 55));

                Resources["ReportButtonBrush"] =
                    new SolidColorBrush(
                        Color.FromRgb(125, 95, 175));

                Resources["ServiceButtonBrush"] =
                    new SolidColorBrush(
                        Color.FromRgb(55, 150, 140));

                Resources["ProductButtonBrush"] =
                    new SolidColorBrush(
                        Color.FromRgb(85, 105, 125));

                Resources["StockButtonBrush"] =
                    new SolidColorBrush(
                        Color.FromRgb(90, 100, 155));

                Resources["WholesaleButtonBrush"] =
                    new SolidColorBrush(
                        Color.FromRgb(200, 115, 60));

                Resources["ClearButtonBrush"] =
                    new SolidColorBrush(
                        Color.FromRgb(205, 65, 75));

                Resources["DeleteButtonBrush"] =
                    new SolidColorBrush(
                        Color.FromRgb(200, 65, 75));

                Resources["ManualButtonBrush"] =
                    new SolidColorBrush(
                        Color.FromRgb(45, 165, 105));


                btnTema.Content = "☀️";
                btnTema.ToolTip = "Gündüz rejiminə keç";
            }
            else
            {
                // ====================================================
                // GÜNDÜZ TEMASI
                // ====================================================

                Resources["WindowBackgroundBrush"] =
                    new SolidColorBrush(
                        Color.FromRgb(244, 247, 251));

                Resources["PanelBackgroundBrush"] =
                    new SolidColorBrush(
                        Color.FromRgb(255, 255, 255));

                Resources["TextBrush"] =
                    new SolidColorBrush(
                        Color.FromRgb(38, 50, 56));

                Resources["SecondaryTextBrush"] =
                    new SolidColorBrush(
                        Color.FromRgb(120, 144, 156));

                Resources["BorderBrushTheme"] =
                    new SolidColorBrush(
                        Color.FromRgb(213, 222, 232));

                Resources["HeaderBackgroundBrush"] =
                    new SolidColorBrush(
                        Color.FromRgb(232, 241, 250));

                Resources["HeaderTextBrush"] =
                    new SolidColorBrush(
                        Color.FromRgb(49, 90, 125));

                Resources["GridLineBrush"] =
                    new SolidColorBrush(
                        Color.FromRgb(228, 234, 240));

                Resources["AlternateRowBrush"] =
                    new SolidColorBrush(
                        Color.FromRgb(248, 250, 253));

                Resources["HoverRowBrush"] =
                    new SolidColorBrush(
                        Color.FromRgb(238, 246, 252));

                Resources["SelectedRowBrush"] =
                    new SolidColorBrush(
                        Color.FromRgb(221, 238, 255));

                Resources["SelectedTextBrush"] =
                    new SolidColorBrush(
                        Color.FromRgb(31, 79, 115));

                Resources["InputBackgroundBrush"] =
                    new SolidColorBrush(
                        Color.FromRgb(255, 255, 255));

                Resources["InputBorderBrush"] =
                    new SolidColorBrush(
                        Color.FromRgb(156, 194, 223));

                // ====================================================
                // ÜMUMİ MƏBLƏĞ - GÜNDÜZ MAVİ
                // ====================================================

                Resources["TotalTextBrush"] =
                    new SolidColorBrush(
                        Color.FromRgb(47, 111, 159));

                Resources["TotalAmountBrush"] =
                    new SolidColorBrush(
                        Color.FromRgb(47, 111, 159));


                // ====================================================
                // DÜYMƏ RƏNGLƏRİ - GÜNDÜZ
                // ====================================================

                Resources["SalesButtonBrush"] =
                    new SolidColorBrush(
                        Color.FromRgb(91, 141, 184));

                Resources["LoginButtonBrush"] =
                    new SolidColorBrush(
                        Color.FromRgb(108, 122, 137));

                Resources["ThemeButtonBrush"] =
                    new SolidColorBrush(
                        Color.FromRgb(69, 90, 100));

                Resources["CashButtonBrush"] =
                    new SolidColorBrush(
                        Color.FromRgb(76, 175, 122));

                Resources["CardButtonBrush"] =
                    new SolidColorBrush(
                        Color.FromRgb(79, 141, 187));

                Resources["ReturnButtonBrush"] =
                    new SolidColorBrush(
                        Color.FromRgb(216, 154, 85));

                Resources["ReportButtonBrush"] =
                    new SolidColorBrush(
                        Color.FromRgb(125, 107, 157));

                Resources["ServiceButtonBrush"] =
                    new SolidColorBrush(
                        Color.FromRgb(78, 154, 145));

                Resources["ProductButtonBrush"] =
                    new SolidColorBrush(
                        Color.FromRgb(102, 120, 138));

                Resources["StockButtonBrush"] =
                    new SolidColorBrush(
                        Color.FromRgb(122, 130, 168));

                Resources["WholesaleButtonBrush"] =
                    new SolidColorBrush(
                        Color.FromRgb(201, 130, 78));

                Resources["ClearButtonBrush"] =
                    new SolidColorBrush(
                        Color.FromRgb(212, 106, 106));

                Resources["DeleteButtonBrush"] =
                    new SolidColorBrush(
                        Color.FromRgb(229, 115, 115));

                Resources["ManualButtonBrush"] =
                    new SolidColorBrush(
                        Color.FromRgb(76, 175, 122));


                btnTema.Content = "🌙";
                btnTema.ToolTip = "Gecə rejiminə keç";
            }

            txtBarkodOkuyucu.Focus();
        }


        // ============================================================
        // SATIŞ QRUP SÜTUNUNU YARAT
        // ============================================================

        private void EnsureSalesGroupColumn()
        {
            try
            {
                using (var db =
                    new SqliteConnection(
                        DatabaseHelper.ConnectionString))
                {
                    db.Open();

                    bool columnExists = false;

                    using (var cmd =
                        new SqliteCommand(
                            "PRAGMA table_info(Satislar);",
                            db))
                    {
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string columnName =
                                    reader["name"]?.ToString() ?? "";

                                if (columnName.Equals(
                                    "SatisQrupId",
                                    StringComparison.OrdinalIgnoreCase))
                                {
                                    columnExists = true;
                                    break;
                                }
                            }
                        }
                    }

                    if (!columnExists)
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
                    "Satış qrup sütunu yaradılarkən xəta baş verdi:\n\n" +
                    ex.Message,
                    "Database xətası",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }


        // ============================================================
        // BARKOD OXUYUCU
        // ============================================================

        private void txtBarkodOkuyucu_KeyDown(
            object sender,
            KeyEventArgs e)
        {
            if (e.Key != Key.Enter)
                return;

            string input =
                txtBarkodOkuyucu.Text.Trim();

            if (string.IsNullOrEmpty(input))
                return;

            ProcessScannedInput(input);

            txtBarkodOkuyucu.Clear();

            txtBarkodOkuyucu.Focus();
        }


        // ============================================================
        // BARKODU EMAL ET
        // ============================================================

        private void ProcessScannedInput(string rawInput)
        {
            var (urunKodu, miktar) =
                BarcodeParser.Parse(rawInput);

            if (miktar <= 0)
            {
                MessageBox.Show(
                    "Məhsul miqdarı düzgün deyil.",
                    "Yanlış miqdar",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            using (var db =
                new SqliteConnection(
                    DatabaseHelper.ConnectionString))
            {
                db.Open();

                var cmd =
                    new SqliteCommand(
                        "SELECT * FROM Urunler " +
                        "WHERE Barkod = @b OR Barkod = @kisa",
                        db);

                cmd.Parameters.AddWithValue(
                    "@b",
                    rawInput);

                cmd.Parameters.AddWithValue(
                    "@kisa",
                    urunKodu);

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        double stokMiqdari =
                            Convert.ToDouble(
                                reader["StokMiktari"],
                                CultureInfo.InvariantCulture);

                        if (stokMiqdari <= 0)
                        {
                            MessageBox.Show(
                                $"\"{reader.GetString(2)}\" məhsulunun stokunda məhsul yoxdur.\n\n" +
                                "Bu məhsul satıla bilməz.",
                                "Stok yoxdur",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);

                            return;
                        }

                        if (miktar > stokMiqdari)
                        {
                            MessageBox.Show(
                                $"\"{reader.GetString(2)}\" məhsulundan stokda yalnız " +
                                $"{stokMiqdari.ToString(
                                    "0.##",
                                    new CultureInfo("az-Latn-AZ"))} " +
                                $"ədəd var.\n\n" +
                                $"Tələb olunan miqdar: {miktar}",
                                "Kifayət qədər stok yoxdur",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);

                            return;
                        }

                        var product =
                            new SəbətMəhsul
                            {
                                Barkod =
                                    reader.GetString(1),

                                UrunAdi =
                                    reader.GetString(2),

                                BirimFiyat =
                                    reader.GetDecimal(4),

                                Miktar =
                                    miktar,

                                BirimTipi =
                                    reader.GetInt32(5)
                            };

                        var existing =
                            Sepet.FirstOrDefault(
                                s => s.Barkod == product.Barkod);

                        if (existing != null &&
                            product.BirimTipi == 0)
                        {
                            double yeniMiqdar =
                                existing.Miktar +
                                product.Miktar;

                            if (yeniMiqdar > stokMiqdari)
                            {
                                MessageBox.Show(
                                    $"\"{product.UrunAdi}\" məhsulundan stokda yalnız " +
                                    $"{stokMiqdari.ToString(
                                        "0.##",
                                        new CultureInfo("az-Latn-AZ"))} " +
                                    $"ədəd var.\n\n" +
                                    $"Səbətdə artıq {existing.Miktar} ədəd var.\n" +
                                    "Daha çox əlavə etmək mümkün deyil.",
                                    "Stok kifayət etmir",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Warning);

                                return;
                            }
                        }

                        UpdateCart(product);
                    }
                    else
                    {
                        MessageBox.Show(
                            $"'{urunKodu}' barkoduna uyğun məhsul tapılmadı.",
                            "Sistem xəbərdarlığı",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);
                    }
                }
            }
        }
        // ============================================================
        // MƏHSUL AXTARIŞI
        // ============================================================

        private void txtMehsulAxtar_TextChanged(
            object sender,
            TextChangedEventArgs e)
        {
            // Mətn dəyişdikdə əlavə əməliyyat aparılmır.
            // Axtarış Enter düyməsi ilə başladılır.
        }


        // ============================================================
        // MƏHSULU ADINA VƏ YA BARKODUNA GÖRƏ TAP
        // ============================================================

        private void txtMehsulAxtar_KeyDown(
            object sender,
            KeyEventArgs e)
        {
            if (e.Key != Key.Enter)
                return;

            string searchText =
                txtMehsulAxtar.Text.Trim();

            if (string.IsNullOrWhiteSpace(searchText))
                return;

            try
            {
                string barkod = null;

                using (var db =
                    new SqliteConnection(
                        DatabaseHelper.ConnectionString))
                {
                    db.Open();

                    using (var cmd =
                        new SqliteCommand(
                            @"SELECT Barkod
                      FROM Urunler
                      WHERE Barkod = @search
                         OR UrunAdi LIKE @name
                      ORDER BY
                          CASE
                              WHEN Barkod = @search THEN 0
                              ELSE 1
                          END
                      LIMIT 1",
                            db))
                    {
                        cmd.Parameters.AddWithValue(
                            "@search",
                            searchText);

                        cmd.Parameters.AddWithValue(
                            "@name",
                            "%" + searchText + "%");

                        object result =
                            cmd.ExecuteScalar();

                        if (result != null &&
                            result != DBNull.Value)
                        {
                            barkod = result.ToString();
                        }
                    }
                }

                if (string.IsNullOrWhiteSpace(barkod))
                {
                    MessageBox.Show(
                        "Axtarışa uyğun məhsul tapılmadı.",
                        "Məhsul tapılmadı",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);

                    txtMehsulAxtar.SelectAll();
                    txtMehsulAxtar.Focus();

                    e.Handled = true;
                    return;
                }

                // Mövcud barkod və stok yoxlama mexanizmindən istifadə et.
                ProcessScannedInput(barkod);

                txtMehsulAxtar.Clear();

                txtBarkodOkuyucu.Focus();

                e.Handled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Məhsul axtarışı zamanı xəta baş verdi:\n\n" +
                    ex.Message,
                    "Sistem xətası",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                txtMehsulAxtar.Focus();

                e.Handled = true;
            }
        }


        // ============================================================
        // SƏBƏTƏ ƏLAVƏ ET
        // ============================================================

        private void UpdateCart(SəbətMəhsul item)
        {
            var existing =
                Sepet.FirstOrDefault(
                    s => s.Barkod == item.Barkod);

            if (existing != null &&
                item.BirimTipi == 0)
            {
                existing.Miktar += item.Miktar;

                manualTotalAmounts.Remove(existing);

                dgSepet.Items.Refresh();
            }
            else
            {
                Sepet.Add(item);
            }

            CalculateTotal();
        }


        // ============================================================
        // MƏHSULUN REAL ÜMUMİ MƏBLƏĞİ
        // ============================================================

        private decimal GetEffectiveTotal(
            SəbətMəhsul item)
        {
            if (manualTotalAmounts.TryGetValue(
                item,
                out decimal manualTotal))
            {
                return manualTotal;
            }

            return item.ToplamTutar;
        }


        // ============================================================
        // MƏHSULUN ALIŞ QİYMƏTİNİ TAP
        // ============================================================

        private decimal GetPurchasePrice(string barkod)
        {
            if (string.IsNullOrWhiteSpace(barkod) ||
                barkod == "MANUEL")
            {
                return 0m;
            }

            using (var db =
                new SqliteConnection(
                    DatabaseHelper.ConnectionString))
            {
                db.Open();

                var cmd =
                    new SqliteCommand(
                        @"SELECT AlisFiyati
                          FROM Urunler
                          WHERE Barkod = @b
                          LIMIT 1",
                        db);

                cmd.Parameters.AddWithValue(
                    "@b",
                    barkod);

                object result =
                    cmd.ExecuteScalar();

                if (result == null ||
                    result == DBNull.Value)
                {
                    return 0m;
                }

                return Convert.ToDecimal(
                    result,
                    CultureInfo.InvariantCulture);
            }
        }


        // ============================================================
        // MƏHSULUN MİNİMUM SATIŞ MƏBLƏĞİ
        // ============================================================

        private decimal GetMinimumSaleTotal(
            SəbətMəhsul item)
        {
            if (item == null)
                return 0m;

            if (item.Barkod == "MANUEL")
                return 0m;

            decimal alisQiymeti =
                GetPurchasePrice(item.Barkod);

            decimal miqdar =
                Convert.ToDecimal(item.Miktar);

            return Math.Round(
                alisQiymeti * miqdar,
                2,
                MidpointRounding.AwayFromZero);
        }


        // ============================================================
        // SƏBƏTDƏN SİL
        // ============================================================

        private void btnSebetdenSil_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (sender is Button button &&
                button.DataContext is SəbətMəhsul urun)
            {
                var result =
                    MessageBox.Show(
                        $"\"{urun.UrunAdi}\" məhsulunu səbətdən silmək istəyirsiniz?",
                        "Məhsulu sil",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);

                if (result != MessageBoxResult.Yes)
                    return;

                manualTotalAmounts.Remove(urun);

                Sepet.Remove(urun);

                CalculateTotal();

                txtBarkodOkuyucu.Focus();
            }
        }


        // ============================================================
        // ÜMUMİ MƏBLƏĞİ HESABLA
        // ============================================================

        private void CalculateTotal()
        {
            decimal total =
                Sepet.Sum(
                    s => GetEffectiveTotal(s));

            txtGenelToplam.Text =
                $"{total.ToString(
                    "N2",
                    new CultureInfo("az-Latn-AZ"))} AZN";
        }


        // ============================================================
        // SƏBƏT CELL EDIT
        // ============================================================

        private void dgSepet_CellEditEnding(
            object sender,
            DataGridCellEditEndingEventArgs e)
        {
            if (e.EditAction != DataGridEditAction.Commit)
                return;

            Dispatcher.BeginInvoke(
                new Action(() =>
                {
                    try
                    {
                        if (e.Column.Header?.ToString() == "Miqdar" &&
                            e.Row.Item is SəbətMəhsul editedItem)
                        {
                            manualTotalAmounts.Remove(editedItem);
                        }

                        CalculateTotal();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(
                            "Məhsul məlumatı yenilənərkən xəta baş verdi:\n\n" +
                            ex.Message,
                            "Sistem xətası",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
                    }
                }),
                DispatcherPriority.Background);
        }



        // ============================================================
        // SƏBƏT ÜMUMİ MƏBLƏĞİ EDITOR AÇILANDA
        // ============================================================

        private void txtCartTotalEditor_Loaded(
            object sender,
            RoutedEventArgs e)
        {
            if (sender is not TextBox textBox)
                return;

            if (textBox.DataContext is not SəbətMəhsul item)
                return;

            textBox.Text =
                GetEffectiveTotal(item).ToString(
                    "N2",
                    new CultureInfo("az-Latn-AZ"));

            textBox.SelectAll();
        }


        // ============================================================
        // SƏBƏT ÜMUMİ MƏBLƏĞİ - DƏYİŞİKLİYİ YADDA SAXLA
        // ============================================================

        private bool SaveCartTotal(
            TextBox textBox)
        {
            if (textBox.DataContext is not SəbətMəhsul item)
                return false;

            string value =
                textBox.Text
                    .Replace("AZN", "")
                    .Replace("Azn", "")
                    .Replace("azn", "")
                    .Trim();

            value = value.Replace(".", ",");

            if (!decimal.TryParse(
                value,
                NumberStyles.Number,
                new CultureInfo("az-Latn-AZ"),
                out decimal newTotal))
            {
                textBox.Text =
                    GetEffectiveTotal(item).ToString(
                        "N2",
                        new CultureInfo("az-Latn-AZ"));

                return false;
            }

            if (newTotal < 0)
            {
                MessageBox.Show(
                    "Ümumi məbləğ mənfi ola bilməz.",
                    "Yanlış məbləğ",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                textBox.Text =
                    GetEffectiveTotal(item).ToString(
                        "N2",
                        new CultureInfo("az-Latn-AZ"));

                return false;
            }

            if (item.Barkod != "MANUEL")
            {
                decimal minimumTotal =
                    GetMinimumSaleTotal(item);

                if (newTotal < minimumTotal)
                {
                    decimal alisQiymeti =
                        GetPurchasePrice(item.Barkod);

                    MessageBox.Show(
                        $"Bu məhsulu alış qiymətindən aşağı satmaq olmaz.\n\n" +
                        $"Məhsul: {item.UrunAdi}\n" +
                        $"Alış qiyməti: {alisQiymeti.ToString(
                            "N2",
                            new CultureInfo("az-Latn-AZ"))} AZN\n" +
                        $"Miqdar: {item.Miktar:0.##}\n" +
                        $"Minimum satış məbləği: {minimumTotal.ToString(
                            "N2",
                            new CultureInfo("az-Latn-AZ"))} AZN\n" +
                        $"Daxil edilən məbləğ: {newTotal.ToString(
                            "N2",
                            new CultureInfo("az-Latn-AZ"))} AZN",
                        "Alış qiymətindən aşağı satış",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    textBox.Text =
                        GetEffectiveTotal(item).ToString(
                            "N2",
                            new CultureInfo("az-Latn-AZ"));

                    return false;
                }
            }

            manualTotalAmounts[item] =
                Math.Round(
                    newTotal,
                    2,
                    MidpointRounding.AwayFromZero);

            CalculateTotal();

            return true;
        }


        // ============================================================
        // SƏBƏT ÜMUMİ MƏBLƏĞİ - ENTER
        // ============================================================

        private void txtCartTotalEditor_KeyDown(
            object sender,
            KeyEventArgs e)
        {
            if (e.Key != Key.Enter)
                return;

            if (sender is not TextBox textBox)
                return;

            SaveCartTotal(textBox);

            dgSepet.CommitEdit(
                DataGridEditingUnit.Cell,
                true);

            dgSepet.CommitEdit(
                DataGridEditingUnit.Row,
                true);

            txtBarkodOkuyucu.Focus();

            e.Handled = true;
        }


        // ============================================================
        // SƏBƏT ÜMUMİ MƏBLƏĞİ - FOCUSDAN ÇIXANDA
        // ============================================================

        private void txtCartTotalEditor_LostFocus(
            object sender,
            RoutedEventArgs e)
        {
            if (sender is not TextBox textBox)
                return;

            SaveCartTotal(textBox);
        }


        // ============================================================
        // BÜTÜN SƏBƏT ÜZRƏ MİNİMUM SATIŞ QİYMƏTİ
        // ============================================================

        private bool CheckMinimumSalePrice()
        {
            try
            {
                decimal minimumTotal = 0m;

                foreach (var item in Sepet)
                {
                    if (item.Barkod == "MANUEL")
                        continue;

                    minimumTotal +=
                        GetMinimumSaleTotal(item);
                }

                string inputTotal =
                    txtGenelToplam.Text
                        .Replace("AZN", "")
                        .Replace("Azn", "")
                        .Replace("azn", "")
                        .Trim();

                inputTotal =
                    inputTotal.Replace(".", ",");

                if (!decimal.TryParse(
                    inputTotal,
                    NumberStyles.Number,
                    new CultureInfo("az-Latn-AZ"),
                    out decimal finalTotal))
                {
                    finalTotal =
                        Sepet.Sum(
                            s => GetEffectiveTotal(s));
                }

                if (finalTotal < minimumTotal)
                {
                    MessageBox.Show(
                        $"Satış alış qiymətindən aşağı ola bilməz.\n\n" +
                        $"Minimum satış məbləği: {minimumTotal.ToString(
                            "N2",
                            new CultureInfo("az-Latn-AZ"))} AZN\n" +
                        $"Daxil edilən satış məbləği: {finalTotal.ToString(
                            "N2",
                            new CultureInfo("az-Latn-AZ"))} AZN\n\n" +
                        $"Minimum {minimumTotal.ToString(
                            "N2",
                            new CultureInfo("az-Latn-AZ"))} AZN məbləğində satış etməlisiniz.",
                        "Alış qiymətindən aşağı satış",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    CalculateTotal();

                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Minimum satış qiyməti yoxlanılarkən xəta baş verdi:\n\n" +
                    ex.Message,
                    "Sistem xətası",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                return false;
            }
        }


        // ============================================================
        // SATIŞI TAMAMLA + ÇEKİ AÇ
        // ============================================================

        private void SatisiTamamla(
            string paymentMethod)
        {
            if (Sepet.Count == 0)
            {
                MessageBox.Show(
                    "Səbət boşdur.",
                    "Xəbərdarlıq",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                txtBarkodOkuyucu.Focus();

                return;
            }

            Keyboard.ClearFocus();

            if (!CheckStockBeforeSale())
            {
                txtBarkodOkuyucu.Focus();

                return;
            }

            if (!CheckMinimumSalePrice())
            {
                txtBarkodOkuyucu.Focus();

                return;
            }

            decimal calculatedTotal =
                Sepet.Sum(
                    s => GetEffectiveTotal(s));

            string inputTotal =
                txtGenelToplam.Text
                    .Replace("Azn", "")
                    .Replace("AZN", "")
                    .Replace("azn", "")
                    .Trim();

            inputTotal =
                inputTotal.Replace(".", ",");

            decimal finalTotal;

            if (!decimal.TryParse(
                inputTotal,
                NumberStyles.Number,
                new CultureInfo("az-Latn-AZ"),
                out finalTotal))
            {
                finalTotal =
                    calculatedTotal;
            }

            if (finalTotal < 0)
            {
                MessageBox.Show(
                    "Satış məbləği mənfi ola bilməz.",
                    "Yanlış məbləğ",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                CalculateTotal();

                txtBarkodOkuyucu.Focus();

                return;
            }

            decimal minimumSaleTotal = 0m;

            foreach (var item in Sepet)
            {
                minimumSaleTotal +=
                    GetMinimumSaleTotal(item);
            }

            if (finalTotal < minimumSaleTotal)
            {
                MessageBox.Show(
                    $"Satış alış qiymətindən aşağı ola bilməz.\n\n" +
                    $"Minimum satış: {minimumSaleTotal.ToString(
                        "N2",
                        new CultureInfo("az-Latn-AZ"))} AZN\n" +
                    $"Satış məbləği: {finalTotal.ToString(
                        "N2",
                        new CultureInfo("az-Latn-AZ"))} AZN",
                    "Satışa icazə verilmir",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                CalculateTotal();

                txtBarkodOkuyucu.Focus();

                return;
            }

            decimal difference =
                finalTotal -
                calculatedTotal;

            var realSaleAmounts =
                new List<decimal>();

            if (calculatedTotal > 0 &&
                difference != 0)
            {
                decimal qalanMebleg =
                    finalTotal;

                for (int index = 0;
                     index < Sepet.Count;
                     index++)
                {
                    var s =
                        Sepet[index];

                    decimal normalMebleg =
                        GetEffectiveTotal(s);

                    decimal realMebleg;

                    if (index == Sepet.Count - 1)
                    {
                        realMebleg =
                            qalanMebleg;
                    }
                    else
                    {
                        decimal faiz =
                            calculatedTotal == 0
                                ? 0
                                : normalMebleg /
                                  calculatedTotal;

                        realMebleg =
                            Math.Round(
                                finalTotal * faiz,
                                2,
                                MidpointRounding.AwayFromZero);

                        qalanMebleg -=
                            realMebleg;
                    }

                    if (realMebleg < 0)
                        realMebleg = 0;

                    realSaleAmounts.Add(
                        realMebleg);
                }
            }
            else
            {
                foreach (var s in Sepet)
                {
                    realSaleAmounts.Add(
                        GetEffectiveTotal(s));
                }
            }

            string satisQrupId =
                Guid.NewGuid().ToString("N");

            int lastSaleId = 0;

            using (var db =
                new SqliteConnection(
                    DatabaseHelper.ConnectionString))
            {
                db.Open();

                using (var tr =
                    db.BeginTransaction())
                {
                    try
                    {
                        for (int i = 0;
                             i < Sepet.Count;
                             i++)
                        {
                            var s =
                                Sepet[i];

                            decimal realSaleAmount =
                                realSaleAmounts[i];

                            lastSaleId =
                                InsertSaleRecord(
                                    satisQrupId,
                                    s.UrunAdi,
                                    s.Miktar,
                                    realSaleAmount,
                                    paymentMethod,
                                    db,
                                    tr);

                            if (s.Barkod != "MANUEL")
                            {
                                UpdateStockRecord(
                                    s.Barkod,
                                    s.Miktar,
                                    db,
                                    tr);
                            }
                        }

                        tr.Commit();

                        var cekMehsullari =
                            new List<SəbətMəhsul>();

                        for (int i = 0;
                             i < Sepet.Count;
                             i++)
                        {
                            var s =
                                Sepet[i];

                            decimal realSaleAmount =
                                realSaleAmounts[i];

                            decimal realBirimFiyat =
                                0m;

                            if (s.Miktar > 0)
                            {
                                realBirimFiyat =
                                    Math.Round(
                                        realSaleAmount /
                                        Convert.ToDecimal(
                                            s.Miktar),
                                        2,
                                        MidpointRounding.AwayFromZero);
                            }

                            cekMehsullari.Add(
                                new SəbətMəhsul
                                {
                                    Id =
                                        s.Id,

                                    Barkod =
                                        s.Barkod,

                                    UrunAdi =
                                        s.UrunAdi,

                                    BirimFiyat =
                                        realBirimFiyat,

                                    Miktar =
                                        s.Miktar,

                                    BirimTipi =
                                        s.BirimTipi
                                });
                        }

                        string odemeMetni =
                            paymentMethod == "Nəğd"
                                ? "Nəğd"
                                : "Kart";

                        MessageBox.Show(
                            $"Satış uğurla tamamlandı.\n\n" +
                            $"Məbləğ: {finalTotal.ToString(
                                "N2",
                                new CultureInfo("az-Latn-AZ"))} AZN\n" +
                            $"Ödəniş: {odemeMetni}",
                            "Satış tamamlandı",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);

                        var cekWindow =
                            new CekWindow(
                                cekMehsullari,
                                finalTotal,
                                paymentMethod,
                                lastSaleId);

                        cekWindow.Owner = this;

                        cekWindow.ShowDialog();

                        Sepet.Clear();

                        manualTotalAmounts.Clear();

                        CalculateTotal();

                        txtBarkodOkuyucu.Focus();
                    }
                    catch (Exception ex)
                    {
                        try
                        {
                            tr.Rollback();
                        }
                        catch
                        {
                        }

                        MessageBox.Show(
                            $"Verilənlər bazasında kritik xəta baş verdi:\n{ex.Message}",
                            "Sistem xətası",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
                    }
                }
            }
        }


        // ============================================================
        // SATIŞI DATABASE-YƏ YAZ
        // ============================================================

        private int InsertSaleRecord(
            string satisQrupId,
            string urunAdi,
            double miktar,
            decimal tutar,
            string odemeYontemi,
            SqliteConnection db,
            SqliteTransaction tr)
        {
            var cmd =
                new SqliteCommand(
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
                      );

                      SELECT last_insert_rowid();",
                    db,
                    tr);

            cmd.Parameters.AddWithValue(
                "@a",
                urunAdi);

            cmd.Parameters.AddWithValue(
                "@m",
                miktar);

            cmd.Parameters.AddWithValue(
                "@t",
                tutar);

            cmd.Parameters.AddWithValue(
                "@y",
                odemeYontemi);

            cmd.Parameters.AddWithValue(
                "@d",
                DateTime.Now.ToString(
                    "yyyy-MM-dd HH:mm:ss"));

            cmd.Parameters.AddWithValue(
                "@q",
                satisQrupId);

            object result =
                cmd.ExecuteScalar();

            return Convert.ToInt32(result);
        }


        // ============================================================
        // STOKDAN ÇIX
        // ============================================================

        private void UpdateStockRecord(
            string barkod,
            double miktar,
            SqliteConnection db,
            SqliteTransaction tr)
        {
            var cmd =
                new SqliteCommand(
                    @"UPDATE Urunler
                      SET StokMiktari =
                          StokMiktari - @m
                      WHERE Barkod = @b
                        AND StokMiktari >= @m",
                    db,
                    tr);

            cmd.Parameters.AddWithValue(
                "@m",
                miktar);

            cmd.Parameters.AddWithValue(
                "@b",
                barkod);

            int affectedRows =
                cmd.ExecuteNonQuery();

            if (affectedRows == 0)
            {
                throw new Exception(
                    $"\"{barkod}\" məhsulunun stoku satış üçün kifayət etmir.");
            }
        }


        // ============================================================
        // SATIŞDAN ƏVVƏL STOKU YOXLAMA
        // ============================================================

        private bool CheckStockBeforeSale()
        {
            try
            {
                using (var db =
                    new SqliteConnection(
                        DatabaseHelper.ConnectionString))
                {
                    db.Open();

                    foreach (var s in Sepet)
                    {
                        if (s.Barkod == "MANUEL")
                            continue;

                        var cmd =
                            new SqliteCommand(
                                @"SELECT UrunAdi, StokMiktari
                                  FROM Urunler
                                  WHERE Barkod = @b",
                                db);

                        cmd.Parameters.AddWithValue(
                            "@b",
                            s.Barkod);

                        using (var reader =
                            cmd.ExecuteReader())
                        {
                            if (!reader.Read())
                            {
                                MessageBox.Show(
                                    $"\"{s.UrunAdi}\" məhsulu artıq verilənlər bazasında tapılmadı.",
                                    "Məhsul tapılmadı",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Warning);

                                return false;
                            }

                            string urunAdi =
                                reader["UrunAdi"]?.ToString()
                                ?? s.UrunAdi;

                            double stokMiqdari =
                                Convert.ToDouble(
                                    reader["StokMiktari"],
                                    CultureInfo.InvariantCulture);

                            if (stokMiqdari <= 0)
                            {
                                MessageBox.Show(
                                    $"\"{urunAdi}\" məhsulunun stoku bitib.\n\n" +
                                    "Satış həyata keçirilə bilməz.",
                                    "Stok yoxdur",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Warning);

                                return false;
                            }

                            if (s.Miktar > stokMiqdari)
                            {
                                MessageBox.Show(
                                    $"\"{urunAdi}\" məhsulundan kifayət qədər stok yoxdur.\n\n" +
                                    $"Stokda: {stokMiqdari.ToString(
                                        "0.##",
                                        new CultureInfo("az-Latn-AZ"))}\n" +
                                    $"Satış miqdarı: {s.Miktar}",
                                    "Stok kifayət etmir",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Warning);

                                return false;
                            }
                        }
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Stok yoxlanılarkən xəta baş verdi:\n{ex.Message}",
                    "Sistem xətası",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                return false;
            }
        }


        // ============================================================
        // ÜMUMİ MƏBLƏĞ - SAĞ TƏRƏF
        // ============================================================

        private void txtGenelToplam_GotFocus(
            object sender,
            RoutedEventArgs e)
        {
            txtGenelToplam.Text =
                txtGenelToplam.Text
                    .Replace("Azn", "")
                    .Replace("AZN", "")
                    .Replace("azn", "")
                    .Trim();

            txtGenelToplam.SelectAll();
        }


        private void txtGenelToplam_LostFocus(
            object sender,
            RoutedEventArgs e)
        {
            string value =
                txtGenelToplam.Text
                    .Replace("Azn", "")
                    .Replace("AZN", "")
                    .Replace("azn", "")
                    .Trim();

            value =
                value.Replace(".", ",");

            if (decimal.TryParse(
                value,
                NumberStyles.Number,
                new CultureInfo("az-Latn-AZ"),
                out decimal val))
            {
                if (val < 0)
                {
                    CalculateTotal();

                    return;
                }

                decimal minimumTotal = 0m;

                foreach (var item in Sepet)
                {
                    minimumTotal +=
                        GetMinimumSaleTotal(item);
                }

                if (val < minimumTotal)
                {
                    MessageBox.Show(
                        $"Satış alış qiymətindən aşağı ola bilməz.\n\n" +
                        $"Minimum satış məbləği: {minimumTotal.ToString(
                            "N2",
                            new CultureInfo("az-Latn-AZ"))} AZN\n" +
                        $"Daxil edilən məbləğ: {val.ToString(
                            "N2",
                            new CultureInfo("az-Latn-AZ"))} AZN",
                        "Alış qiymətindən aşağı satış",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    CalculateTotal();

                    return;
                }

                txtGenelToplam.Text =
                    $"{val.ToString(
                        "N2",
                        new CultureInfo("az-Latn-AZ"))} AZN";
            }
            else
            {
                CalculateTotal();
            }
        }


        // ============================================================
        // NAĞD
        // ============================================================

        private void btnNakit_Click(
            object sender,
            RoutedEventArgs e)
        {
            SatisiTamamla("Nəğd");
        }


        // ============================================================
        // KART
        // ============================================================

        private void btnKart_Click(
            object sender,
            RoutedEventArgs e)
        {
            SatisiTamamla("Kart");
        }


        // ============================================================
        // ANBAR
        // ============================================================

        private void btnStokSayfasi_Click(
            object sender,
            RoutedEventArgs e)
        {
            new StokSayfasi().ShowDialog();

            txtBarkodOkuyucu.Focus();
        }


        // ============================================================
        // MƏHSULLAR
        // ============================================================

        private void btnUrunListesi_Click(
            object sender,
            RoutedEventArgs e)
        {
            new UrunListesiWindow().ShowDialog();

            txtBarkodOkuyucu.Focus();
        }


        // ============================================================
        // XİDMƏTLƏR
        // ============================================================

        private void btnXidmetler_Click(
            object sender,
            RoutedEventArgs e)
        {
            var window =
                new XidmetlerWindow();

            window.Owner = this;

            window.ShowDialog();

            txtBarkodOkuyucu.Focus();
        }



        // ============================================================
        // GERİ QAYTARMA
        // ============================================================

        private void btnGeriQaytarma_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (!isLoggedIn)
            {
                MessageBox.Show(
                    "Bu bölməyə daxil olmaq üçün əvvəlcə DAXİL OL düyməsindən giriş etməlisiniz.",
                    "Giriş tələb olunur",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                txtBarkodOkuyucu.Focus();

                return;
            }

            var window =
                new GeriQaytarmaWindow();

            window.Owner = this;

            window.ShowDialog();

            txtBarkodOkuyucu.Focus();
        }


        // ============================================================
        // HESABATLAR
        // ============================================================

        private void btnHesabatlar_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (!isLoggedIn)
            {
                MessageBox.Show(
                    "Bu bölməyə daxil olmaq üçün əvvəlcə DAXİL OL düyməsindən giriş etməlisiniz.",
                    "Giriş tələb olunur",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                txtBarkodOkuyucu.Focus();

                return;
            }

            var hesabatlarWindow =
                new HesabatlarWindow();

            hesabatlarWindow.Owner = this;

            hesabatlarWindow.ShowDialog();

            txtBarkodOkuyucu.Focus();
        }


        // ============================================================
        // SATIŞLAR
        // ============================================================

        private void btnGunlukSatislar_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (!isLoggedIn)
            {
                MessageBox.Show(
                    "Bu bölməyə daxil olmaq üçün əvvəlcə DAXİL OL düyməsindən giriş etməlisiniz.",
                    "Giriş tələb olunur",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                txtBarkodOkuyucu.Focus();

                return;
            }

            var window =
                new GunlukSatislarWindow();

            window.Owner = this;

            window.ShowDialog();

            txtBarkodOkuyucu.Focus();
        }


        // ============================================================
        // LOGIN
        // ============================================================

        private void btnLogin_Click(
            object sender,
            RoutedEventArgs e)
        {
            string password =
                Microsoft.VisualBasic.Interaction.InputBox(
                    "Giriş kodunu daxil edin:",
                    "🔐 DAXİL OL",
                    "");

            if (string.IsNullOrWhiteSpace(password))
            {
                txtBarkodOkuyucu.Focus();

                return;
            }

            if (!ValidatePassword(password))
            {
                isLoggedIn = false;

                MessageBox.Show(
                    "Giriş kodu yanlışdır.",
                    "DAXİL OL",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                txtBarkodOkuyucu.Focus();

                return;
            }

            isLoggedIn = true;

            MessageBox.Show(
                "Giriş uğurludur.\n\n" +
                "Satışlar, Geri Qaytarma və Hesabatlar bölmələrinə giriş aktiv edildi.",
                "DAXİL OL",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            var changeResult =
                MessageBox.Show(
                    "Giriş kodunu dəyişmək istəyirsiniz?",
                    "Təhlükəsizlik",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

            if (changeResult == MessageBoxResult.Yes)
            {
                ChangePassword();
            }

            txtBarkodOkuyucu.Focus();
        }


        // ============================================================
        // KODU DƏYİŞDİR
        // ============================================================

        private void ChangePassword()
        {
            string newPassword =
                Microsoft.VisualBasic.Interaction.InputBox(
                    "Yeni 4 rəqəmli giriş kodunu daxil edin:",
                    "🔑 KODU DƏYİŞDİR",
                    "");

            if (string.IsNullOrWhiteSpace(newPassword))
                return;

            if (newPassword.Length != 4 ||
                !newPassword.All(char.IsDigit))
            {
                MessageBox.Show(
                    "Giriş kodu tam olaraq 4 rəqəmdən ibarət olmalıdır.",
                    "Yanlış kod",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            string confirmPassword =
                Microsoft.VisualBasic.Interaction.InputBox(
                    "Yeni giriş kodunu yenidən daxil edin:",
                    "🔑 KODU TƏSDİQLƏ",
                    "");

            if (string.IsNullOrWhiteSpace(confirmPassword))
                return;

            if (newPassword != confirmPassword)
            {
                MessageBox.Show(
                    "Daxil edilən kodlar eyni deyil.",
                    "Kod təsdiqlənmədi",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            try
            {
                using (var db =
                    new SqliteConnection(
                        DatabaseHelper.ConnectionString))
                {
                    db.Open();

                    var cmd =
                        new SqliteCommand(
                            @"INSERT OR REPLACE INTO Ayarlar
                              (
                                  Id,
                                  SifreHash
                              )
                              VALUES
                              (
                                  1,
                                  @sifre
                              )",
                            db);

                    cmd.Parameters.AddWithValue(
                        "@sifre",
                        newPassword);

                    cmd.ExecuteNonQuery();
                }

                MessageBox.Show(
                    "Giriş kodu uğurla dəyişdirildi.",
                    "Əməliyyat uğurlu oldu",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Giriş kodu dəyişdirilərkən xəta baş verdi:\n{ex.Message}",
                    "Sistem xətası",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }


        // ============================================================
        // SƏBƏTİ TƏMİZLƏ
        // ============================================================

        private void btnSepetiTemizle_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (Sepet.Count == 0)
            {
                txtBarkodOkuyucu.Focus();

                return;
            }

            var result =
                MessageBox.Show(
                    "Səbətdəki bütün məhsulları silmək istəyirsiniz?",
                    "Səbəti təmizlə",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
                return;

            Sepet.Clear();

            manualTotalAmounts.Clear();

            CalculateTotal();

            txtBarkodOkuyucu.Focus();
        }


        // ============================================================
        // ƏL İLƏ MƏHSUL ƏLAVƏ ET
        // ============================================================

        private void btnManuelEkle_Click(
            object sender,
            RoutedEventArgs e)
        {
            string urunAdi =
                txtManuelUrunAdi.Text.Trim();

            string fiyatMetni =
                txtManuelFiyat.Text
                    .Replace(".", ",")
                    .Trim();

            if (string.IsNullOrEmpty(urunAdi) ||
                !decimal.TryParse(
                    fiyatMetni,
                    NumberStyles.Number,
                    new CultureInfo("az-Latn-AZ"),
                    out decimal price))
            {
                MessageBox.Show(
                    "Məhsul adı və qiymət məlumatı mütləq daxil edilməlidir.",
                    "Natamam məlumat",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            if (price <= 0)
            {
                MessageBox.Show(
                    "Məhsul qiyməti 0-dan böyük olmalıdır.",
                    "Yanlış qiymət",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            Sepet.Add(
                new SəbətMəhsul
                {
                    Barkod =
                        "MANUEL",

                    UrunAdi =
                        urunAdi,

                    BirimFiyat =
                        price,

                    Miktar =
                        1,

                    BirimTipi =
                        0
                });

            txtManuelUrunAdi.Clear();

            txtManuelFiyat.Clear();

            CalculateTotal();

            txtBarkodOkuyucu.Focus();
        }


        // ============================================================
        // ŞİFRƏ YOXLAMASI
        // ============================================================

        private bool ValidatePassword(
            string inputPassword)
        {
            try
            {
                using (var db =
                    new SqliteConnection(
                        DatabaseHelper.ConnectionString))
                {
                    db.Open();

                    var cmd =
                        new SqliteCommand(
                            "SELECT SifreHash " +
                            "FROM Ayarlar " +
                            "WHERE Id = 1",
                            db);

                    var result =
                        cmd.ExecuteScalar();

                    if (result == null ||
                        result == DBNull.Value ||
                        string.IsNullOrWhiteSpace(
                            result.ToString()))
                    {
                        return inputPassword == "1234";
                    }

                    return result.ToString() ==
                           inputPassword;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Giriş kodu yoxlanılarkən xəta baş verdi:\n{ex.Message}",
                    "Sistem xətası",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                return false;
            }
        }


        // ============================================================
        // TEMA DÜYMƏSİ
        // ============================================================

        private void btnTema_Click(
            object sender,
            RoutedEventArgs e)
        {
            ApplyTheme(!isDarkTheme);
        }


        // ============================================================
        // BOŞ EVENTLƏR
        // ============================================================

        private void txtGenelToplam_TextChanged(
            object sender,
            TextChangedEventArgs e)
        {
        }


        private void dgSepet_SelectionChanged(
            object sender,
            SelectionChangedEventArgs e)
        {
        }


        private void dgSepet_SelectionChanged_1(
            object sender,
            SelectionChangedEventArgs e)
        {
        }


        private void txtBarkodOkuyucu_TextChanged(
            object sender,
            TextChangedEventArgs e)
        {
        }
    }
}