using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ForumCollection.HelperClass
{
    public class CookieAwareWebClientHelper : WebClient
    {
        public CookieContainer cookie = new CookieContainer();

        protected override WebRequest GetWebRequest(Uri address)
        {
            WebRequest request = base.GetWebRequest(address);
            //request.ContentType= "text/plain; charset=utf-8";
            //request.ContentType = "text/plain; charset=utf-8";
            if (request is HttpWebRequest)
            {
                (request as HttpWebRequest).CookieContainer = cookie;
            }

            return request;
        }

    }
}
