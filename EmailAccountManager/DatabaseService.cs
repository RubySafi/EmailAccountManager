using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;


namespace EmailAccountManager
{
    public class DatabaseService
    {
        private string _dbPath;

        public DatabaseService(string dbPath)
        {
            _dbPath = dbPath;
        }

        public ObservableCollection<SiteInfo> LoadSiteInfos()
        {
            var siteList = new ObservableCollection<SiteInfo>();

            using var connection = new SqliteConnection($"Data Source={_dbPath}");
            connection.Open();

            var siteCmd = connection.CreateCommand();
            siteCmd.CommandText = "SELECT Id, SiteName, SecurityLevel, Comment, Timestamp FROM SiteInfo";

            using var siteReader = siteCmd.ExecuteReader();
            while (siteReader.Read())
            {
                int siteId = siteReader.GetInt32(0);
                string siteName = siteReader.GetString(1);
                SecurityLevel securityLevel = (SecurityLevel)siteReader.GetInt32(2);
                string comment = siteReader.IsDBNull(3) ? "" : siteReader.GetString(3);
                DateTime timestamp = DateTime.Parse(siteReader.GetString(4), CultureInfo.InvariantCulture);

                var site = new SiteInfo
                {
                    SiteName = siteName,
                    SecurityLevel = securityLevel,
                    Comment = comment,
                    Timestamp = timestamp,
                    EmailList = new List<MailElm>()
                };

                var mailCmd = connection.CreateCommand();
                mailCmd.CommandText = "SELECT Address, Comment, Timestamp FROM MailElm WHERE SiteInfoId = $id";
                mailCmd.Parameters.AddWithValue("$id", siteId);

                using var mailReader = mailCmd.ExecuteReader();
                while (mailReader.Read())
                {
                    string address = mailReader.GetString(0);
                    string mailComment = mailReader.IsDBNull(1) ? "" : mailReader.GetString(1);
                    DateTime mailTimestamp = DateTime.Parse(mailReader.GetString(2), CultureInfo.InvariantCulture);

                    site.EmailList.Add(new MailElm
                    {
                        Address = address,
                        Comment = mailComment,
                        Timestamp = mailTimestamp
                    });
                }

                siteList.Add(site);
            }

            return siteList;
        }
        
    }
        
}
