using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace EmailAccountManager
{
    public class SiteInfo
    {
        public string SiteName { get; set; }
        public SecurityLevel SecurityLevel { get; set; }
        public List<MailElm> EmailList { get; set; } = new();
        public string Comment { get; set; }
        public DateTime Timestamp { get; set; }

        public string AllEmails => string.Join("\n", EmailList?.Select(e => e.Address) ?? new[] { "None" });


        public SiteInfo() { }

        public SiteInfo(SiteInfo site)
        {
            SiteName = site.SiteName;
            SecurityLevel = site.SecurityLevel;
            EmailList = new List<MailElm>(site.EmailList);
            Timestamp = site.Timestamp;
            Comment = site.Comment;
        }

    }
}
