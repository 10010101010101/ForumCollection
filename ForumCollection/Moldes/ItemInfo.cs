using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForumCollection.Moldes
{
    public class ItemInfo
    {
        /// <summary>
        /// 帖子id
        /// </summary>
        public int TopicId { get; set; }

        /// <summary>
        /// 标题
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 链接
        /// </summary>
        public string URL { get; set; }

        /// <summary>
        /// 子链接信息
        /// </summary>
        public List<ItemInfo> ChildrenInfos { get; set; }

        /// <summary>
        /// 子链接列表
        /// </summary>
        public List<string> ChildrenLinks { get; set; }

        /// <summary>
        /// 父链接信息
        /// </summary>
        public ItemInfo ParentInfo { get; set; }

        /// <summary>
        /// 附件列表
        /// </summary>
        public List<UploadFileInfo> UploadFileList { get; set; }

        /// <summary>
        /// 任务盘地址
        /// </summary>
        public List<DiskAddressInfo> DiskAddressList { get; set; }

        /// <summary>
        /// 资管盘
        /// </summary>
        public List<DiskAddressInfo> DataDiskAddressList { get; set; }

        /// <summary>
        /// 是否有效
        /// </summary>
        public bool IsInvalid = true;

        /// <summary>
        /// 是否勾选激活
        /// </summary>
        public bool IsActive = true;

        /// <summary>
        /// 控件
        /// </summary>
        public UserControls.Item Item { get; set; }

        /// <summary>
        /// 浏览项
        /// </summary>
        public UserControls.BrowseItem BrowseItem { get; set; }

        /// <summary>
        /// 论坛信息
        /// </summary>
        public ForumInfo ForumInfos { get; set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        public ItemInfo()
        {
            this.ChildrenInfos = new List<ItemInfo>();
            this.DataDiskAddressList = new List<DiskAddressInfo>();
            this.DiskAddressList = new List<DiskAddressInfo>();
            this.UploadFileList = new List<UploadFileInfo>();
            this.ChildrenLinks = new List<string>();
        }


    }
}
