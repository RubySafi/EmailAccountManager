using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmailAccountManager
{
    public enum SecurityLevel
    {
        None,
        General,//個人情報は含むが金融にかかわらない
        Finance,//金融情報にかかわる
    }
}
