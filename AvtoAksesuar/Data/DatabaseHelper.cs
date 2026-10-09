using Microsoft.Data.Sqlite;
using System;
using System.IO;

namespace PcKod.UI.Data
{
    public static class DatabaseHelper
    {
        private const string DbName = "PcKod.db";

        private static readonly string DatabaseFolder =
            Path.Combine(
                Environment.GetFolderPath(
                    Environment.SpecialFolder.LocalApplicationData),
                "PcKod");

        public static string DatabasePath =>
            Path.Combine(DatabaseFolder, DbName);

        public static string ConnectionString =>
            new SqliteConnectionStringBuilder
            {
                DataSource = DatabasePath,
                Mode = SqliteOpenMode.ReadWriteCreate
            }.ToString();

        public static void InitializeDatabase()
        {
            Directory.CreateDirectory(DatabaseFolder);

            MigrateOldDatabaseIfNeeded();

            using (var db = new SqliteConnection(ConnectionString))
            {
                db.Open();

                ExecuteNonQuery(@"
                    CREATE TABLE IF NOT EXISTS Ayarlar (
                        Id INTEGER PRIMARY KEY,
                        SifreHash TEXT,
                        PrinterAdi TEXT
                    );", db);

                AddColumnIfNeeded(
                    db, "Ayarlar", "SifreHash", "TEXT");

                AddColumnIfNeeded(
                    db, "Ayarlar", "PrinterAdi", "TEXT");

                ExecuteNonQuery(@"
                    CREATE TABLE IF NOT EXISTS Urunler (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Barkod TEXT UNIQUE,
                        UrunAdi TEXT,
                        AlisFiyati DECIMAL DEFAULT 0,
                        SatisFiyati DECIMAL DEFAULT 0,
                        BirimTipi INTEGER DEFAULT 0,
                        StokMiktari DOUBLE DEFAULT 0
                    );", db);

                AddColumnIfNeeded(
                    db, "Urunler", "Barkod", "TEXT");

                AddColumnIfNeeded(
                    db, "Urunler", "UrunAdi", "TEXT");

                AddColumnIfNeeded(
                    db, "Urunler", "AlisFiyati", "DECIMAL DEFAULT 0");

                AddColumnIfNeeded(
                    db, "Urunler", "SatisFiyati", "DECIMAL DEFAULT 0");

                AddColumnIfNeeded(
                    db, "Urunler", "BirimTipi", "INTEGER DEFAULT 0");

                AddColumnIfNeeded(
                    db, "Urunler", "StokMiktari", "DOUBLE DEFAULT 0");

                ExecuteNonQuery(@"
                    CREATE TABLE IF NOT EXISTS Firmalar (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        FirmaAdi TEXT NOT NULL,
                        ToplamBorc DECIMAL DEFAULT 0
                    );", db);

                AddColumnIfNeeded(
                    db, "Firmalar", "ToplamBorc", "DECIMAL DEFAULT 0");

                ExecuteNonQuery(@"
                    CREATE TABLE IF NOT EXISTS Satislar (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        UrunAdi TEXT,
                        Miktar DOUBLE,
                        ToplamTutar DECIMAL,
                        OdemeYontemi TEXT,
                        Tarih TEXT,
                        FirmaId INTEGER NULL,
                        SatisQrupId TEXT
                    );", db);

                AddColumnIfNeeded(
                    db, "Satislar", "FirmaId", "INTEGER NULL");

                AddColumnIfNeeded(
                    db, "Satislar", "SatisQrupId", "TEXT");
            }
        }

        private static void MigrateOldDatabaseIfNeeded()
        {
            if (File.Exists(DatabasePath))
                return;

            string oldDatabasePath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                DbName);

            if (!File.Exists(oldDatabasePath))
                return;

            File.Copy(
                oldDatabasePath,
                DatabasePath,
                overwrite: false);
        }

        private static void AddColumnIfNeeded(
            SqliteConnection db,
            string tableName,
            string columnName,
            string columnDefinition)
        {
            bool columnExists = false;

            using (var cmd = new SqliteCommand(
                $"PRAGMA table_info(\"{tableName}\");", db))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    string name = reader["name"]?.ToString() ?? "";

                    if (name.Equals(
                        columnName,
                        StringComparison.OrdinalIgnoreCase))
                    {
                        columnExists = true;
                        break;
                    }
                }
            }

            if (!columnExists)
            {
                ExecuteNonQuery(
                    $"ALTER TABLE \"{tableName}\" " +
                    $"ADD COLUMN \"{columnName}\" {columnDefinition};",
                    db);
            }
        }

        private static void ExecuteNonQuery(
            string sql,
            SqliteConnection db)
        {
            using (var cmd = new SqliteCommand(sql, db))
            {
                cmd.ExecuteNonQuery();
            }
        }
    }
}
