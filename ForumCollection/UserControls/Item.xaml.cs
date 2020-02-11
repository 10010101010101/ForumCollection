using ForumCollection.Common;
using ForumCollection.HelperClass;
using ForumCollection.Moldes;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace ForumCollection.UserControls
{
    /// <summary>
    /// Item.xaml 的交互逻辑
    /// </summary>
    public partial class Item : UserControl
    {
        public ItemInfo Info { get; set; }

        public bool IsTreeOpen { get; set; }

        public double TreeHeight { get; set; }

        public bool IsMouseRight { get; set; }

        public delegate void ShowListHandler();
        public delegate void HiddenListHandler();

        public event ShowListHandler ShowList;
        public event HiddenListHandler HiddenList;

        public Item(ItemInfo itemInfo)
        {
            InitializeComponent();

            this.Hyperlink.NavigateUri = new Uri(itemInfo.URL);
            this.Hyperlink.Foreground = new SolidColorBrush(Colors.Blue);
            this.Hypername.Text = itemInfo.Title.Split('»').Last();

            Info = itemInfo;

            SetDiskInfo();
            SetDataDiskInfo();
            SetFileInfo();
            SetLinkInfo();

            if (itemInfo.DiskAddressList.Count > 0)
            {
                SetUserNameScriptInfo(itemInfo.DiskAddressList.Last().UserName);
            }
            
        }

        public void SetDiskInfo()
        {
            if (Info.DiskAddressList.Count > 0)
            {
                this.Item_Disk.Text = this.Item_Disk.Text.Replace(RegexHelper.RemoveNotNumber(this.Item_Disk.Text), Info.DiskAddressList.Count.ToString());
                this.Item_Disk.TextDecorations = TextDecorations.Underline;
                this.Item_Disk.Foreground = new SolidColorBrush(Colors.Blue);
            }
        }

        public void SetDataDiskInfo()
        {
            if (Info.DataDiskAddressList.Count > 0)
            {
                this.Item_DataDisk.Text = this.Item_DataDisk.Text.Replace(RegexHelper.RemoveNotNumber(this.Item_DataDisk.Text), Info.DataDiskAddressList.Count.ToString());
                this.Item_DataDisk.TextDecorations = TextDecorations.Underline;
                this.Item_DataDisk.Foreground = new SolidColorBrush(Colors.Blue);
            }
        }

        public void SetFileInfo()
        {

            if (Info.UploadFileList.Count > 0)
            {
                this.Item_File.Text = this.Item_File.Text.Replace(RegexHelper.RemoveNotNumber(this.Item_File.Text), Info.UploadFileList.Count.ToString());
                this.Item_File.TextDecorations = TextDecorations.Underline;
                this.Item_File.Foreground = new SolidColorBrush(Colors.Blue);
            }
        }

        public void SetLinkInfo()
        {
            if (Info.ChildrenLinks.Count > 0)
            {
                this.Item_Link.Text = this.Item_Link.Text.Replace(RegexHelper.RemoveNotNumber(this.Item_Link.Text), Info.ChildrenLinks.Count.ToString());
            }
       
        }

        public void SetUserNameScriptInfo(string name)
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                this.Item_UserName_Script.Text = string.Format("脚本:\"{0}\"", name);
                this.Item_UserName_Script.Visibility = Visibility.Visible;
            }

        }

        public void SetUserNameDataInfo(string name)
        {
            this.Item_UserName_Data.Text = string.Format("资管:\"{0}\"", name);
            this.Item_UserName_Data.Visibility = Visibility.Visible;
        }

        private void Hyperlink_MouseLeave(object sender, MouseEventArgs e)
        {
            Hyperlink.Foreground = new SolidColorBrush(Colors.Blue);
        }

        private void Hyperlink_MouseMove(object sender, MouseEventArgs e)
        {
            Hyperlink.Foreground = new SolidColorBrush(Colors.Red);
        }

        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            Hyperlink link = sender as Hyperlink;
            Process.Start(new ProcessStartInfo(link.NavigateUri.AbsoluteUri));
        }

        private void Frame_MouseEnter(object sender, MouseEventArgs e)
        {
            Frame.Background = new SolidColorBrush(Colors.LightGray);
        }

        private void Frame_MouseLeave(object sender, MouseEventArgs e)
        {
            Frame.Background = new SolidColorBrush(Colors.AliceBlue);
        }

        private void Info_MouseMove(object sender, MouseEventArgs e)
        {
            TextBlock textBlock = sender as TextBlock;

            if (textBlock.TextDecorations.Equals(TextDecorations.Underline))
            {
                textBlock.Foreground = new SolidColorBrush(Colors.Red);
                this.Cursor = Cursors.Hand;
            }
        }

        private void Info_MouseLeave(object sender, MouseEventArgs e)
        {
            TextBlock textBlock = sender as TextBlock;

            if (textBlock.TextDecorations.Equals(TextDecorations.Underline))
            {
                textBlock.Foreground = new SolidColorBrush(Colors.Blue);
                this.Cursor = Cursors.Arrow;
            }
        }

        public void ExportDiskFile(object sender, RoutedEventArgs e)
        {
            //通过主界面按钮全部导出
            if (e.Source.GetType() != typeof(TextBlock))
            {
                if (this.Info.IsInvalid)
                {
                    if (this.Info.IsActive)
                    {
                        _ExportDiskFile(sender, e);
                    }

                    if (this.Info.ChildrenInfos.Count > 0)
                    {
                        foreach (ItemInfo itemInfo in this.Info.ChildrenInfos)
                        {
                            if (itemInfo.IsInvalid && itemInfo.IsActive)
                            {
                                itemInfo.Item.ExportDiskFile(sender, e);
                            }
                        }
                    }
                }
            }
            else
            {
                Thread _thread = new Thread(new ThreadStart(new Action(() =>
                {
                    try
                    {
                        HiddenList();

                        _ExportDiskFile(sender, e);

                        ShowList();
                    }
                    catch (Exception ex)
                    {
                        ShowList();

                        MessageBox.Show(App.GetExceptionMsg(ex, string.Empty), "报错");
                    }
                })));

                _thread.IsBackground = true;
                _thread.Start();
            }
        }

        private void _ExportDiskFile(object sender, RoutedEventArgs e)
        {
            if (Info.DiskAddressList.Count > 0 && this.Info.IsActive == true)
            {
                ItemInfo ItemInfo = Info.ForumInfos.ForumAlreadyInfos.Where(i => i.Value.URL.Equals(Info.ForumInfos.MasterURL)).First().Value;

                string saveDir = System.IO.Path.Combine(Constant.OutputDirectory, new Regex(Constant.FileNameSpecial).Replace(string.Format(Constant.ProjectFolderName, ItemInfo.Title.Split('»').Last()), "") + "\\R盘文件");

                if (!Directory.Exists(saveDir))
                {
                    DirectoryHelper.CreateDirectory(saveDir);
                }

                var AddressList = Info.DiskAddressList.Where(i => i.Floor == Info.DiskAddressList.Last().Floor);
                
                foreach(DiskAddressInfo diskAddressInfo in AddressList)
                {
                    if (Directory.Exists(diskAddressInfo.Address))
                    {
                        string[] Files = System.IO.Directory.GetFiles(diskAddressInfo.Address).Where(i => System.IO.Path.GetExtension(i).Contains(".zip")).ToArray();

                        if (Files.Count() > 0)
                        {
                            string zip = Files.First();

                            if (System.IO.File.Exists(zip))
                            {
                                File.Copy(zip, System.IO.Path.Combine(saveDir, System.IO.Path.GetFileName(zip)), true);
                            }
                        }
                        else
                        {
                            //简体魔域 资管包是文件夹格式的
                            string newName = Path.GetFileName(Path.GetDirectoryName(diskAddressInfo.Address));
                            string newSaveDir = Path.Combine(saveDir, newName);

                            DirectoryHelper.CopyDirectorys(diskAddressInfo.Address, newSaveDir);
                        }

                    }
                }

                if (e.Source.GetType() == typeof(TextBlock))
                {
                    Process.Start(saveDir);
                }
            }
        }

        public void ExportDataDiskFile(object sender, RoutedEventArgs e)
        {
            //通过主界面按钮全部导出
            if (e.Source.GetType() != typeof(TextBlock))
            {
                if (this.Info.IsInvalid)
                {
                    if (this.Info.IsActive)
                    {
                        _ExportDataDiskFile(sender, e);
                    }
                    
                    if (this.Info.ChildrenInfos.Count > 0)
                    {
                        foreach (ItemInfo itemInfo in this.Info.ChildrenInfos)
                        {
                            if (itemInfo.IsInvalid && itemInfo.IsActive)
                            {
                                itemInfo.Item.ExportDataDiskFile(sender, e);
                            }
                        }
                    }
                }
            }
            else
            {
                Thread _thread = new Thread(new ThreadStart(new Action(() =>
                {
                    try
                    {
                        HiddenList();

                        _ExportDataDiskFile(sender, e);

                        ShowList();
                    }
                    catch(Exception ex)
                    {
                        ShowList();

                        MessageBox.Show(App.GetExceptionMsg(ex, string.Empty), "报错");
                    }
                })));

                _thread.IsBackground = true;
                _thread.Start();
            }
        }

        private void _ExportDataDiskFile(object sender, RoutedEventArgs e)
        {
            if (Info.DataDiskAddressList.Count > 0 && this.Info.IsActive == true)
            {
                ItemInfo ItemInfo = Info.ForumInfos.ForumAlreadyInfos.Where(i => i.Value.URL.Equals(Info.ForumInfos.MasterURL)).First().Value;

                string saveDir = System.IO.Path.Combine(Constant.OutputDirectory, new Regex(Constant.FileNameSpecial).Replace(string.Format(Constant.ProjectFolderName, ItemInfo.Title.Split('»').Last()), "") + "\\W盘文件");

                if (!Directory.Exists(saveDir))
                {
                    DirectoryHelper.CreateDirectory(saveDir);
                }

                var AddressList = Info.DataDiskAddressList.Where(i => i.Floor == Info.DataDiskAddressList.Last().Floor);

                foreach(DiskAddressInfo diskAddressInfo in AddressList)
                {
                    //资管w盘都是rar格式的压缩包
                    if (File.Exists(diskAddressInfo.Address))
                    {
                        File.Copy(diskAddressInfo.Address, Path.GetFileName(diskAddressInfo.Address), true);
                    }
                }

                if (e.Source.GetType() == typeof(TextBlock))
                {
                    if (Directory.Exists(saveDir))
                    {
                        Process.Start(saveDir);
                    }
                }
            }
        }

        public void ExportUploadFile(object sender, RoutedEventArgs e)
        {
            //通过主界面按钮全部导出
            if (e.Source.GetType() != typeof(TextBlock))
            {
                if (this.Info.IsInvalid)
                {
                    if (this.Info.IsActive)
                    {
                        _ExportUploadFile(sender, e);
                    }

                    if (this.Info.ChildrenInfos.Count > 0)
                    {
                        foreach (ItemInfo itemInfo in this.Info.ChildrenInfos)
                        {
                            if (itemInfo.IsInvalid && itemInfo.IsActive)
                            {
                                itemInfo.Item.ExportUploadFile(sender, e);
                            }
                        }
                    }

                }
            }
            else
            {
                Thread _thread = new Thread(new ThreadStart(new Action(() =>
                {
                    try
                    {
                        HiddenList();

                        _ExportUploadFile(sender, e);

                        ShowList();
                    }
                    catch (Exception ex)
                    {
                        ShowList();

                        MessageBox.Show(App.GetExceptionMsg(ex, string.Empty), "报错");
                    }
                })));

                _thread.IsBackground = true;
                _thread.Start();
            }
        }

        private void _ExportUploadFile(object sender, RoutedEventArgs e)
        {
            if (Info.UploadFileList.Count > 0 && this.Info.IsActive)
            {
                ItemInfo ItemInfo = Info.ForumInfos.ForumAlreadyInfos.Where(i => i.Value.URL.Equals(Info.ForumInfos.MasterURL)).First().Value;

                string saveDir = System.IO.Path.Combine(Constant.OutputDirectory, new Regex(Constant.FileNameSpecial).Replace(string.Format(Constant.ProjectFolderName, ItemInfo.Title.Split('»').Last()), "") + "\\论坛附件");

                if (!Directory.Exists(saveDir))
                {
                    DirectoryHelper.CreateDirectory(saveDir);
                }

                foreach (UploadFileInfo file in Info.UploadFileList)
                {
                    Constant.Client.DownloadFile(file.FileHref, System.IO.Path.Combine(saveDir, file.FileName));
                }

                if (e.Source.GetType() == typeof(TextBlock))
                {
                    Process.Start(saveDir);
                }
            }
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (this.ItemList != null && this.ItemList.Children != null)
            {
                CheckBox self = sender as CheckBox;

                this.Info.IsActive = self.IsChecked == true;

                if (!this.IsMouseRight)
                {
                    if (this.ItemList.Children.Count > 0 && this.Info.IsInvalid)
                    {
                        foreach (UserControls.Item item in ItemList.Children)
                        {
                            if (item.Info.IsInvalid)
                            {
                                item.CheckBox.IsChecked = self.IsChecked;
                            }
                        }
                    }
                }
                else
                {
                    this.IsMouseRight = false;
                }
            }
        }

        public void CheckInvalid()
        {
            if (!this.Info.IsInvalid)
            {
                this.Frame.Background = new SolidColorBrush(Colors.Transparent);
                this.CheckBox.IsChecked = false;
                this.IsEnabled = false;
                this.Hypername.TextDecorations = TextDecorations.Strikethrough;
                this.Hypername.Foreground = new SolidColorBrush(Colors.Gray);
                this.Item_Disk.Foreground = new SolidColorBrush(Colors.Gray);
                this.Item_DataDisk.Foreground = new SolidColorBrush(Colors.Gray);
                this.Item_File.Foreground = new SolidColorBrush(Colors.Gray);
                this.Item_Link.Foreground = new SolidColorBrush(Colors.Gray);
                this.Item_UserName_Script.Foreground = new SolidColorBrush(Colors.Gray);
                this.Item_UserName_Data.Foreground = new SolidColorBrush(Colors.Gray);
                this.Info.IsInvalid = false;

                if (this.ItemList != null && this.ItemList.Children != null && this.ItemList.Children.Count > 0)
                {
                    foreach (UserControls.Item item in ItemList.Children)
                    {
                        item.Info.IsInvalid = false;
                        item.CheckInvalid();
                    }
                }
            }
            else
            {
                if (this.ItemList != null && this.ItemList.Children != null && this.ItemList.Children.Count > 0)
                {
                    foreach (UserControls.Item item in ItemList.Children)
                    {
                        item.CheckInvalid();
                    }
                }
            }
        }

        private void Btn_Tree_Click(object sender, RoutedEventArgs e)
        {
            if (this.ItemList.Children.Count > 0)
            {
                if (this.IsTreeOpen)
                {
                    this.Btn_Tree.Visibility = Visibility.Visible;
                    this.IsTreeOpen = false;
                    this.Btn_Tree.Content = "+";

                    this.ItemList.Height = 0;
                }
                else
                {
                    this.Btn_Tree.Visibility = Visibility.Visible;
                    this.IsTreeOpen = true;
                    this.Btn_Tree.Content = "-";

                    this.ItemList.Height = this.TreeHeight;
                }
            }
        }

        public void AddItem(UserControls.Item item)
        {
            this.ItemList.Children.Add(item);

            if (this.ItemList.Children.Count > 0)
            {
                this.Btn_Tree.Visibility = Visibility.Visible;
                this.IsTreeOpen = true;
                this.Btn_Tree.Content = "-";

                this.TreeHeight = this.ItemList.Height;
            }
        }

        private void CheckBox_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton.Equals(MouseButton.Right))
            {
                this.IsMouseRight = true;
                this.CheckBox.IsChecked = !this.CheckBox.IsChecked;
            }
        }
    }
}
