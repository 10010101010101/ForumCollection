using ForumCollection.Moldes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ForumCollection.UserControls
{
    /// <summary>
    /// BrowseItem.xaml 的交互逻辑
    /// </summary>
    public partial class BrowseItem : UserControl
    {
        public ItemInfo Info { get; set; }

        public BrowseItem(ItemInfo urlInfo, double width, double margin)
        {
            InitializeComponent();

            Info = urlInfo;

            this.ItemTitle.Text = Info.Title;
            this.ItemURL.Text = Info.URL;

            if (width > 0)
            {
                this.Item.Width = width;
                this.ItemTitle.Width = (width - margin) / 2;
                this.ItemURL.Width = (width - margin) / 2;
            }
        }

        public void Refresh()
        {
            this.ItemTitle.Text = Info.Title;
            this.ItemURL.Text = Info.URL;
        }

    }
}
