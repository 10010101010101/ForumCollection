using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ForumCollection.HelperClass
{
    class IniFileHelper
    {
        /// <summary>
        /// 声明读ini文件所有key的API函数
        /// </summary>
        /// <param name="strTitle">段落，也就是title </param>
        /// <param name="strKey">关键字，也就是key</param>
        /// <param name="strDefaultValue">无法读取时候时候的缺省数值</param>
        /// <param name="bBuffer">读取的数值缓冲区数组</param>
        /// <param name="iBufferSize">读取的数值缓冲区数组大小</param>
        /// <param name="strFilePath">文件路径</param>
        /// <returns></returns>
        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(
           string strTitle,
           string strKey,
           string strDefaultValue,
           Byte[] bBuffer,
           int iBufferSize,
           string strFilePath
           );


        /// <summary>
        /// 声明读ini文件单个key的API函数
        /// </summary>
        /// <param name="strTitle">段落，也就是title</param>
        /// <param name="strKey">关键字，也就是key</param>
        /// <param name="strDefaultValue">无法读取时候时候的缺省数值</param>
        /// <param name="sbBuffer">读取的数值缓冲区 </param>
        /// <param name="iBufferSize">读取的数值缓冲区大小</param>
        /// <param name="strFilePath">文件路径</param>
        /// <returns></returns>
        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(
           string strTitle,
           string strKey,
           string strDefaultValue,
           StringBuilder sbBuffer,
           int iBufferSize,
           string strFilePath
           );

        /// <summary>
        /// 声明写ini文件单个key的API函数
        /// </summary>
        /// <param name="strTitle">段落，也就是title</param>
        /// <param name="strKey">关键字，也就是key</param>
        /// <param name="strValue">写入的数值</param>
        /// <param name="strFilePath">文件路径</param>
        /// <returns></returns>
        [DllImport("kernel32")]
        private static extern bool WritePrivateProfileString(
           string strTitle,
           string strKey,
           string strValue,
           string strFilePath
           );

        /// <summary>
        /// 读取ini文件所有title的名称
        /// </summary>
        /// <param name="strFileName"></param>
        /// <returns></returns>
        public static string[] ReadIniTitles(string strFileName)
        {
            return ReadIniKeys(null, strFileName);
        }

        /// <summary>
        /// 读取ini文件所有key的名称
        /// </summary>
        /// <param name="strTitle"></param>
        /// <param name="strFileName"></param>
        /// <returns></returns>
        public static string[] ReadIniKeys(string strTitle, string strFileName)
        {
            //获取到所有key并且存放到数组中
            Byte[] bBuffer = new Byte[1048575];
            GetPrivateProfileString(strTitle, null, null, bBuffer, 1048575, strFileName);//获取所有key
            string[] strBuffers = System.Text.Encoding.Default.GetString(bBuffer).Split('\0');//获取到的key进行分割存放数组，同时使用编码避免读取中文乱码

            //对数组的值进行有效性判断，取得第一个无效值的数组下标
            int iSize = strBuffers.Length;//定义有效数组的个数
            for (int i = 0; i < strBuffers.Length; i++)
            {
                if (strBuffers[i].Trim().Length == 0)
                {   //当读取到的数组为空时，则说明之后的数组值都无效，退出循环
                    iSize = i;
                    break;
                }
            }

            //新建一个数组，并且赋值后把值返回给函数
            string[] strKeys = new string[iSize];
            for (int i = 0; i < iSize; i++)
            {
                strKeys[i] = strBuffers[i];
            }
            return strKeys;
        }

        /// <summary>
        /// 读取ini文件单个key对应的value值
        /// </summary>
        /// <param name="strTitle"></param>
        /// <param name="strKey"></param>
        /// <param name="strDefaultValue"></param>
        /// <param name="strFileName"></param>
        /// <returns></returns>
        public static string ReadIniValue(string strTitle, string strKey, string strDefaultValue, string strFileName)
        {
            StringBuilder sbBuffer = new StringBuilder(1024);
            GetPrivateProfileString(strTitle, strKey, strDefaultValue, sbBuffer, 1024, strFileName);
            return sbBuffer.ToString();
        }

        /// <summary>
        /// 写入ini文件单个key对应的value值 
        /// </summary>
        /// <param name="strTitle"></param>
        /// <param name="strKey"></param>
        /// <param name="strValue"></param>
        /// <param name="strFileName"></param>
        public static void WriteIniValue(string strTitle, string strKey, string strValue, string strFileName)
        {
            WritePrivateProfileString(strTitle, strKey, strValue, strFileName);
        }

        /// <summary>
        /// 删除ini文件单个key以及对应的value值
        /// </summary>
        /// <param name="strTitle"></param>
        /// <param name="strKey"></param>
        /// <param name="strFileName"></param>
        public static void DelIniKey(string strTitle, string strKey, string strFileName)
        {
            WritePrivateProfileString(strTitle, strKey, null, strFileName);
        }

        /// <summary>
        /// 删除ini文件单个title以及对应的所有key、value
        /// </summary>
        /// <param name="strTitle"></param>
        /// <param name="strFileName"></param>
        public static void DelIniTitle(string strTitle, string strFileName)
        {
            WritePrivateProfileString(strTitle, null, null, strFileName);
        }
    }
}
