using ForumCollection.HelperClass;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ForumCollection.Common
{
    class Constant
    {
        private static CookieAwareWebClientHelper _Client = null;
        private static CookieContainer _CookieContainer = null;

        /// <summary>
        /// 保存登录的cookie
        /// </summary>
        public static string LoginCookie { get; set; }

        public static CookieContainer GetCookieContainer()
        {
            if (_CookieContainer != null)
            {
                return _CookieContainer;
            }

            if (!string.IsNullOrWhiteSpace(Constant.LoginCookie))
            {
                CookieContainer cookieContainer = new CookieContainer();

                string[] cookstr = LoginCookie.Split(';');

                foreach (string str in cookstr)
                {
                    List<string> cookieNameValue = str.Split('=').ToList();
                    string key = cookieNameValue.First();
                    cookieNameValue.RemoveAt(0);
                    string value = string.Join("=", cookieNameValue);

                    Cookie ck = new Cookie(key.Trim().ToString(), value.Trim().ToString());
                    ck.Domain = "forum.nderp.99.com";
                    cookieContainer.Add(ck);
                }

                return cookieContainer;
            }

            return null;
        }

        public static CookieAwareWebClientHelper Client
        {
            get
            {
                if (_Client == null)
                {
                    _Client = new CookieAwareWebClientHelper();
                    _Client.BaseAddress = @"http://forum.nderp.99.com/Forum/";

                    if (string.IsNullOrWhiteSpace(LoginCookie))
                    {
                        throw new Exception("未登录论坛或没有论坛权限！");
                    }
                    else
                    {
                        _Client.cookie = GetCookieContainer();
                    }
                }

                return _Client;
            }
        }

        /// <summary>
        /// 用来验证论坛链接
        /// </summary>
        public static string ContainsForumHref = "http://forum.nderp.99.com/Forum/TopicList";

        /// <summary>
        /// 用来验证附件链接
        /// </summary>
        public static string ContaninsUploadFileHref = "A1_frmFileDownLoad.aspx?";

        public static object NavigationObj = new object();

        /// <summary>
        /// 项目文件夹目录名称
        /// </summary>
        public static string ProjectFolderName = "【排期】{0}";

        /// <summary>
        /// 文件名特殊字符
        /// </summary>
        public static string FileNameSpecial = @"\:" + @"|\;" + @"|\/" + @"|\\" + @"|\|" + @"|\," + @"|\*" + @"|\?" + @"|\""" + @"|\<" + @"|\>";

        /// <summary>
        /// 判断是否有效的论坛链接
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static bool IsForumURL(string url)
        {
            if (!string.IsNullOrWhiteSpace(url) && url.Trim().StartsWith(Constant.ContainsForumHref))
            {
                return true;
            }

            //不是有效链接
            return false;
        }

        /// <summary>
        /// 判断是否是有效的附件链接
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static bool IsUploadFileURL(string url)
        {
            if (!string.IsNullOrWhiteSpace(url) && !url.Trim().StartsWith(Constant.ContaninsUploadFileHref))
            {
                //不是有效链接
                return false;
            }

            return true;
        }

        /// <summary>
        /// 帮助目录
        /// </summary>
        public static string HelpDirectory
        {
            get
            {
                string outputDir = Path.Combine(System.IO.Directory.GetCurrentDirectory(), "help");
                if (!Directory.Exists(outputDir))
                {
                    DirectoryHelper.CreateDirectory(outputDir);
                }

                return outputDir;
            }
        }

        /// <summary>
        /// 输出目录
        /// </summary>
        public static string OutputDirectory
        {
            get
            {
                string outputDir = Path.Combine(System.IO.Directory.GetCurrentDirectory(), "输出目录\\论坛工具");
                if (!Directory.Exists(outputDir))
                {
                    DirectoryHelper.CreateDirectory(outputDir);
                }

                return outputDir;
            }
        }

        /// <summary>
        /// ini目录
        /// </summary>
        private static string IniDirectory
        {
            get
            {
                string iniDir = Path.Combine(System.IO.Directory.GetCurrentDirectory(), "ini");
                if (!Directory.Exists(iniDir))
                {
                    DirectoryHelper.CreateDirectory(iniDir);
                }

                return iniDir;
            }
        }

        public static string HistoricalURL
        {
            get
            {
                string iniDir = Path.Combine(System.IO.Directory.GetCurrentDirectory(), "ini");
                if (!Directory.Exists(iniDir))
                {
                    DirectoryHelper.CreateDirectory(iniDir);
                }

                return Path.Combine(iniDir, "historicalurl.ini");
            }
        }
    }
}
