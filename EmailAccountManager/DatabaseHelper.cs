using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Data.Sqlite;
using System.IO;

namespace EmailAccountManager
{
    public static class DatabaseHelper
    {
        private static string _dbPath = "administrator.db";

        public static void SetDatabasePath(string dbPath)
        {
            _dbPath = dbPath;
        }


        public static void InitializeDatabase()
        {
            using var connection = new SqliteConnection($"Data Source={_dbPath}");
            connection.Open();

            var createSiteTableCmd = connection.CreateCommand();
            createSiteTableCmd.CommandText =
            @"
            CREATE TABLE IF NOT EXISTS SiteInfo (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                SiteName TEXT NOT NULL,
                SecurityLevel INTEGER NOT NULL,
                Comment TEXT,
                Timestamp TEXT NOT NULL
            );
        ";
            createSiteTableCmd.ExecuteNonQuery();

            var createEmailTableCmd = connection.CreateCommand();
            createEmailTableCmd.CommandText =
            @"
            CREATE TABLE IF NOT EXISTS MailElm (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                SiteId INTEGER NOT NULL,
                Address TEXT NOT NULL,
                Comment TEXT,
                Timestamp TEXT NOT NULL,
                FOREIGN KEY (SiteId) REFERENCES SiteInfo(Id) ON DELETE CASCADE
            );
        ";
            createEmailTableCmd.ExecuteNonQuery();
        }

        public static void SaveSites(IEnumerable<SiteInfo> sites)
        {

            try
            {
                using var connection = new SqliteConnection($"Data Source={_dbPath}");
                connection.Open();

                using var transaction = connection.BeginTransaction();

                var clearMailCmd = connection.CreateCommand();
                clearMailCmd.CommandText = "DELETE FROM MailElm;";
                clearMailCmd.ExecuteNonQuery();

                var clearSiteCmd = connection.CreateCommand();
                clearSiteCmd.CommandText = "DELETE FROM SiteInfo;";
                clearSiteCmd.ExecuteNonQuery();

                foreach (var site in sites)
                {
                    var insertSiteCmd = connection.CreateCommand();
                    insertSiteCmd.CommandText =
                    @"
                    INSERT INTO SiteInfo (SiteName, SecurityLevel, Comment, Timestamp)
                    VALUES ($name, $level, $comment, $timestamp);
                    SELECT last_insert_rowid();
        ";
                    insertSiteCmd.Parameters.AddWithValue("$name", site.SiteName);
                    insertSiteCmd.Parameters.AddWithValue("$level", (int)site.SecurityLevel);
                    insertSiteCmd.Parameters.AddWithValue("$comment", site.Comment ?? "");
                    insertSiteCmd.Parameters.AddWithValue("$timestamp", site.Timestamp.ToString("o"));

                    var siteId = (long)insertSiteCmd.ExecuteScalar();

                    foreach (var email in site.EmailList)
                    {
                        var insertMailCmd = connection.CreateCommand();
                        insertMailCmd.CommandText =
                        @"
                        INSERT INTO MailElm (SiteId, Address, Comment, Timestamp)
                        VALUES ($siteId, $address, $comment, $timestamp);
            ";
                        insertMailCmd.Parameters.AddWithValue("$siteId", siteId);
                        insertMailCmd.Parameters.AddWithValue("$address", email.Address);
                        insertMailCmd.Parameters.AddWithValue("$comment", email.Comment ?? "");
                        insertMailCmd.Parameters.AddWithValue("$timestamp", email.Timestamp.ToString("o"));

                        insertMailCmd.ExecuteNonQuery();
                    }
                }

                transaction.Commit();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save DataBase {_dbPath}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Logger.LogError($"Failed to save DataBase {_dbPath}", ex);
            }

        }

        public static ObservableCollection<SiteInfo> _LoadSites()
        {

            var result = new ObservableCollection<SiteInfo>();

            using var connection = new SqliteConnection($"Data Source={_dbPath}");
            connection.Open();

            var getSitesCmd = connection.CreateCommand();
            getSitesCmd.CommandText = "SELECT Id, SiteName, SecurityLevel, Comment, Timestamp FROM SiteInfo;";

            using var reader = getSitesCmd.ExecuteReader();
            var siteMap = new Dictionary<long, SiteInfo>();

            while (reader.Read())
            {
                var id = reader.GetInt64(0);
                var site = new SiteInfo
                {
                    SiteName = reader.GetString(1),
                    SecurityLevel = (SecurityLevel)reader.GetInt32(2),
                    Comment = reader.GetString(3),
                    Timestamp = DateTime.Parse(reader.GetString(4)),
                    EmailList = new List<MailElm>()
                };
                siteMap[id] = site;
                result.Add(site);
            }

            reader.Close();

            var getEmailsCmd = connection.CreateCommand();
            getEmailsCmd.CommandText = "SELECT SiteId, Address, Comment, Timestamp FROM MailElm;";
            using var emailReader = getEmailsCmd.ExecuteReader();

            while (emailReader.Read())
            {
                var siteId = emailReader.GetInt64(0);
                if (!siteMap.ContainsKey(siteId)) continue;

                siteMap[siteId].EmailList.Add(new MailElm
                {
                    Address = emailReader.GetString(1),
                    Comment = emailReader.GetString(2),
                    Timestamp = DateTime.Parse(emailReader.GetString(3))
                });
            }

            return result;
        }

        public static ObservableCollection<SiteInfo> LoadSites()
        {

            var result = new ObservableCollection<SiteInfo>();

            try
            {
                result = _LoadSites();
            }
            catch (SqliteException ex)
            {
                Logger.LogError($"Failed to load the database: {_dbPath}", ex);

                var sb = new StringBuilder();
                bool takeBackup = true;
                sb.AppendLine($"Failed to load the database: {_dbPath}");
                if (File.Exists(_dbPath))
                {
                    sb.AppendLine("SQLite error: the database may be inaccessible or corrupted.");
                    sb.AppendLine("Would you like to initialize the database?");
                    sb.AppendLine("The original file will be backed up automatically.");
                }
                else
                {
                    sb.AppendLine("Reason: the database does not exist.");
                    sb.AppendLine("Would you like to initialize the database?");
                    takeBackup = false;
                }

                var errMsgboxResult = MessageBox.Show(sb.ToString(), "Database Error", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (errMsgboxResult == MessageBoxResult.Yes)
                {
                    if (takeBackup)
                    {
                        BackupCorruptedDatabase();
                    }
                    InitializeDatabase();
                    return new ObservableCollection<SiteInfo>();
                }
                else
                {
                    MessageBox.Show("The application will now close. Please restart it later.", "Application Exit");
                    Application.Current.Shutdown();
                    return null;
                }

            }
            catch (IOException ex)
            {
                Logger.LogError($"Failed to load the database: {_dbPath}", ex);

                var sb = new StringBuilder();
                bool takeBackup = true;
                sb.AppendLine($"Failed to load the database: {_dbPath}");
                if (File.Exists(_dbPath))
                {
                    sb.AppendLine("I/O error: the database may be locked by another process.");
                    sb.AppendLine("Would you like to initialize the database?");
                    sb.AppendLine("The original file will be backed up automatically.");
                }
                else
                {
                    sb.AppendLine("Reason: the database does not exist.");
                    sb.AppendLine("Would you like to initialize the database?");
                    takeBackup = false;
                }

                var errMsgboxResult = MessageBox.Show(sb.ToString(), "Database Error", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (errMsgboxResult == MessageBoxResult.Yes)
                {
                    if (takeBackup)
                    {
                        BackupCorruptedDatabase();
                    }
                    InitializeDatabase();
                    return new ObservableCollection<SiteInfo>();
                }
                else
                {
                    MessageBox.Show("The application will now close. Please restart it later.", "Application Exit");
                    Application.Current.Shutdown();
                    return null;
                }

            }
            catch (Exception ex)
            {
                Logger.LogError($"Failed to load the database: {_dbPath}", ex);

                var sb = new StringBuilder();
                bool takeBackup = true;
                sb.AppendLine($"Failed to load the database: {_dbPath}");
                if (File.Exists(_dbPath))
                {
                    sb.AppendLine("An unknown error occurred.");
                    sb.AppendLine("Would you like to initialize the database?");
                    sb.AppendLine("The original file will be backed up automatically.");
                }
                else
                {
                    sb.AppendLine("Reason: the database does not exist.");
                    sb.AppendLine("Would you like to initialize the database?");
                    takeBackup = false;
                }

                var errMsgboxResult = MessageBox.Show(sb.ToString(), "Database Error", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (errMsgboxResult == MessageBoxResult.Yes)
                {
                    if (takeBackup)
                    {
                        BackupCorruptedDatabase();
                    }
                    InitializeDatabase();
                    return new ObservableCollection<SiteInfo>();
                }
                else
                {
                    MessageBox.Show("The application will now close. Please restart it later.", "Application Exit");
                    Application.Current.Shutdown();
                    return null;
                }

            }
            return result;
        }

        private static void BackupCorruptedDatabase()
        {
            try
            {
                if (File.Exists(_dbPath))
                {
                    string backupPathInitial = _dbPath + ".backup_" + DateTime.Now.ToString("yyyyMMdd_HHmmss");
                    string backupPath = backupPathInitial;
                    int i = 1;
                    while(File.Exists(backupPath))
                    {
                        backupPath = backupPathInitial + $"_{i}";
                        i++;
                        if (i > 100) { break; }
                    }
                    File.Move(_dbPath, backupPath);
                    Logger.LogInfo($"Backed up the corrupted database to: {backupPath}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to create a backup.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Logger.LogError("Failed to create a backup.", ex);
            }
        }

    }

}
