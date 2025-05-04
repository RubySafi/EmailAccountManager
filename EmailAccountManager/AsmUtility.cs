using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace EmailAccountManager
{
    public static class AsmUtility
    {
        public static string GetAssemblyDirectoryName()
        {
            string location = Assembly.GetEntryAssembly().Location;
            return Path.GetDirectoryName(location);
        }
        public static string GetAssemblyFileNameWithoutExtension()
        {
            string location = Assembly.GetEntryAssembly().Location;
            return Path.GetFileNameWithoutExtension(location);
        }
    }
}
