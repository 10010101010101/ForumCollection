using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForumCollection.HelperClass
{
    public class EscapeCharacterHelper
    {
        public static string XamlTransformation(string xmal)
        {
            return xmal.Replace("&lt;","<").Replace("&gt;",">").Replace("&amp;","&").Replace("&apos;", "'").Replace("&quot;", "\"").Replace("&nbsp;", ";");
        }

        public static string XamlTransforEmpty(string xmal)
        {
            return xmal.Replace("&lt;", "").Replace("&gt;", "").Replace("&amp;", "").Replace("&apos;", "").Replace("&quot;", "").Replace("&nbsp;","").Replace(";","");
        }
    }
}
