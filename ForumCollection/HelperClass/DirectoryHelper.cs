using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace ForumCollection.HelperClass
{
    public class DirectoryHelper
    {
        /// <summary>
        /// 删除一个文件夹包括里面的文件
        /// </summary>
        /// <param name="dir">文件夹地址</param>
        /// <param name="deleteSelf">是否删除自身</param>
        public static void DeleteDirectory(DirectoryInfo dirInfo, bool deleteSelf = false)
        {
            if (dirInfo.Exists)
            {
                foreach (FileInfo file in dirInfo.GetFiles())
                {
                    try
                    {
                        file.Delete();
                    }
                    catch
                    {
                        //CheckUpMessageHelper.AddLogText("删除文件失败：" + ex.Message, LogType.Error);
                    }
                }
                foreach (DirectoryInfo dir in dirInfo.GetDirectories())
                {
                    try
                    {
                        DeleteDirectory(dir, deleteSelf);
                    }
                    catch
                    {
                        //CheckUpMessageHelper.AddLogText("删除文件夹失败：" + ex.Message, LogType.Error);
                    }

                }
                if (deleteSelf)
                {
                    try
                    {
                        dirInfo.Delete();
                    }
                    catch
                    {
                        //CheckUpMessageHelper.AddLogText("删除文件夹失败：" + ex.Message, LogType.Error);
                    }
                }
            }
        }

        public static void DeleteDirectory(string dirString, bool deleteSelf = false)
        {
            DeleteDirectory(new DirectoryInfo(dirString), deleteSelf);
        }

        /// <summary>
        /// 创建文件夹，包含父级文件夹
        /// </summary>
        /// <param name="dirInfo"></param>
        public static void CreateDirectory(DirectoryInfo dirInfo)
        {
            if (dirInfo.Parent != null && !dirInfo.Parent.Exists)
            {
                CreateDirectory(dirInfo.Parent);
            }
            if (!dirInfo.Exists)
            {
                if (dirInfo.FullName.Length >= 240)
                {

                }
                dirInfo.Create();
            }
        }

        public static void CreateDirectory(string dirString)
        {
            CreateDirectory(new DirectoryInfo(dirString));
        }

        /// <summary>
        /// 获取目录下所有路径
        /// </summary>
        /// <returns></returns>
        public static Dictionary<string, string> GetDirAllFileFullName(DirectoryInfo curDir, Dictionary<string, string> Dic, string strDirName = "")
        {
            foreach (FileInfo file in curDir.GetFiles())
            {
                Dic.Add(Path.Combine(strDirName, file.Name), file.FullName);
            }

            foreach (DirectoryInfo dir in curDir.GetDirectories())
            {
                GetDirAllFileFullName(dir, Dic, strDirName + dir.Name + "\\");
            }

            return Dic;
        }

        public static void CopyDirectorys(string sourceDir, string destDirName, string sRepalce = "")
        {
            if (!Directory.Exists(destDirName))
            {
                if (!string.IsNullOrWhiteSpace(sRepalce))
                {
                    destDirName = destDirName.Replace(sRepalce,"");
                }

                CreateDirectory(destDirName);
            }

            foreach(string filePath in Directory.GetFiles(sourceDir))
            {
                string _filePath = filePath;
                //if (!string.IsNullOrWhiteSpace(sRepalce))
                //{
                //    _filePath = _filePath.Replace(sRepalce, "");
                //}

                Copy(filePath,Path.Combine(destDirName,Path.GetFileName(_filePath)),true);
            }

            foreach(string dirPath in Directory.GetDirectories(sourceDir))
            {
                string _dirPath = dirPath;
                if (!string.IsNullOrWhiteSpace(sRepalce))
                {
                    _dirPath = _dirPath.Replace(sRepalce, "");
                }

                CopyDirectorys(dirPath,Path.Combine(destDirName,Path.GetFileName(_dirPath)));
            }
        }

        /// <summary>
        /// 递归查找文件
        /// </summary>
        /// <returns></returns>
        public static string GetDirofFilePath(string dir, string name)
        {
            string path = Path.Combine(dir,name);

            if (Directory.Exists(dir))
            {
                if (File.Exists(path))
                {
                    return path;
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(Path.GetDirectoryName(dir)))
                    {
                        path = GetDirofFilePath(Path.GetDirectoryName(dir), name);
                    }
                }
            }

            return path;
        }

        /// <summary>
        /// 长文件名的文件拷贝
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <param name="overwrite"></param>
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool CopyFile(string source, string target, bool overwrite);
        public static void Copy(string source, string target, bool overwrite)
        {
            string formattedName_source = @"\\?\" + source;
            string formattedName_target = @"\\?\" + target;
            // CopyFile 第三个参数是 FALSE 的时候自动覆盖 所以写成 !overwrite
            // 参见 http://msdn.microsoft.com/en-us/library/aa363851(v=vs.85).aspx
            bool v = CopyFile(formattedName_source, formattedName_target, !overwrite);
        }
    }
}
