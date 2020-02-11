using ForumCollection.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForumCollection.Moldes
{
    public class ForumInfo
    {

        /// <summary>
        /// 原始父链接
        /// </summary>
        public string MasterURL { get; set; }

        /// <summary>
        /// 保存已读信息
        /// </summary>
        public Dictionary<int, ItemInfo> ForumAlreadyInfos = new Dictionary<int, ItemInfo>();

        public ForumInfo()
        {
            
        }
    }
}
