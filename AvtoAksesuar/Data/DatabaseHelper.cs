using Microsoft.Data.Sqlite;
using System;
using System.IO;

namespace PcKod.UI.Data
{
    /// <summary>
    /// SQLite verilənlər bazasını idarə edir.
    /// </summary>
    public static class DatabaseHelper
    {
        private const string DbName = "PcKod.db";

        // =========================================================
        // DATABASE PATH
        // =========================================================

        // EXE proqramının yerləşdiyi qovluq
        public static string DatabasePath =>
            Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                DbName);

        public static string ConnectionString =>
            $"Data Source={DatabasePath}";

        // =========================================================
        // DATABASE INITIALIZE
        // =========================================================

        public static void InitializeDatabase()
        {
            using (var db =
                   new SqliteConnection(ConnectionString))
            {
                db.Open();

                // =====================================================
                // AYARLAR
                // =====================================================

                ExecuteNonQuery(
                    @"CREATE TABLE IF NOT EXISTS Ayarlar (
                        Id INTEGER PRIMARY KEY,
                        SifreHash TEXT,
                        PrinterAdi TEXT
                    )",
                    db);

                // -----------------------------------------------------
                // ƏGƏR AYARLAR CƏDVƏLİ ƏVVƏLDƏN YARADILIBSA
                // PrinterAdi SÜTUNUNU YOXLA
                // -----------------------------------------------------

                AddPrinterColumnIfNeeded(db);

                // =====================================================
                // MƏHSULLAR
                // =====================================================

                string urunSql = @"
                    CREATE TABLE IF NOT EXISTS Urunler (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Barkod TEXT UNIQUE,
                        UrunAdi TEXT,
                        AlisFiyati DECIMAL,
                        SatisFiyati DECIMAL,
                        BirimTipi INTEGER,
                        StokMiktari DOUBLE DEFAULT 0
                    )";

                ExecuteNonQuery(
                    urunSql,
                    db);

                // =====================================================
                // FİRMALAR
                // =====================================================

                string firmaSql = @"
                    CREATE TABLE IF NOT EXISTS Firmalar (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        FirmaAdi TEXT NOT NULL,
                        ToplamBorc DECIMAL DEFAULT 0
                    )";

                ExecuteNonQuery(
                    firmaSql,
                    db);

                // =====================================================
                // SATIŞLAR
                // =====================================================

                string satisSql = @"
                    CREATE TABLE IF NOT EXISTS Satislar (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        UrunAdi TEXT,
                        Miktar DOUBLE,
                        ToplamTutar DECIMAL,
                        OdemeYontemi TEXT,
                        Tarih TEXT,
                        FirmaId INTEGER NULL
                    )";

                ExecuteNonQuery(
                    satisSql,
                    db);
            }
        }

        // =========================================================
        // PRINTER SÜTUNUNU YOXLA
        // =========================================================

        private static void AddPrinterColumnIfNeeded(
            SqliteConnection db)
        {
            bool printerColumnExists = false;

            using (var cmd = new SqliteCommand(
                "PRAGMA table_info(Ayarlar)",
                db))
            {
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string columnName =
                            reader["name"]?.ToString() ?? "";

                        if (columnName.Equals(
                            "PrinterAdi",
                            StringComparison.OrdinalIgnoreCase))
                        {
                            printerColumnExists = true;
                            break;
                        }
                    }
                }
            }

            // PrinterAdi yoxdursa əlavə et
            if (!printerColumnExists)
            {
                ExecuteNonQuery(
                    "ALTER TABLE Ayarlar ADD COLUMN PrinterAdi TEXT",
                    db);
            }
        }

        // =========================================================
        // SQL EXECUTE
        // =========================================================

        private static void ExecuteNonQuery(
            string sql,
            SqliteConnection db)
        {
            using (var cmd =
                   new SqliteCommand(sql, db))
            {
                cmd.ExecuteNonQuery();
            }
        }
    }
}