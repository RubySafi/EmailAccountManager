using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Data.Sqlite;

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
            catch(Exception ex)
            {
                MessageBox.Show($"Failed to save DataBase {_dbPath}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Logger.LogError($"Failed to save DataBase {_dbPath}", ex);
            }

}

public static ObservableCollection<SiteInfo> LoadSites()
        {
            
            var result = new ObservableCollection<SiteInfo>();

            try
            {
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
            }
            catch(Exception ex)
            {
                MessageBox.Show($"Failed to load DataBase {_dbPath}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Logger.LogError($"Failed to load DataBase {_dbPath}", ex);
                return new ObservableCollection<SiteInfo>();
            }

            return result;
        }

    }

}
