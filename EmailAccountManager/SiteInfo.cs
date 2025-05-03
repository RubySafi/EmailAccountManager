using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmailAccountManager
{
    internal class SiteInfo
    {
        public string SiteName { get; set; }
        public string Comment { get; set; }
        public DateTime Timestamp { get; set; }
        public SecurityLevel SecurityLevel { get; set; } // or enum
        public List<MailElm> EmailList { get; set; } = new();

        public SiteInfo(string SiteName) { 
            this.SiteName = SiteName;
        
        
        }
    }
}
