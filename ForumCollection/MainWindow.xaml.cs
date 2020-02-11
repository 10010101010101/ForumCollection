using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using System.Windows.Threading;
using ForumApi;
using ForumCollection.Common;
using ForumCollection.HelperClass;
using ForumCollection.Moldes;
using mshtml;

namespace ForumCollection
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public ForumInfo ForumInfos = new ForumInfo();

        /// <summary>
        /// 记录历史浏览记录
        /// </summary>
        public List<ItemInfo> HistoricalURL = new List<ItemInfo>();

        public double DropItemListWidth { get; set; }
        public double DropItemListMargin = 60;

        public MainWindow()
        {
            InitializeComponent();

            InitializeNavigation();

            //打开R盘
            //OpenTaskDisk();

            //打开W盘
            //OpenDataDisk();

            //尝试登录论坛
            webBrowser.Navigate("http://forum.nderp.99.com/Forum/Default.aspx");
            Thread.Sleep(100);
        }

        private void InitializeNavigation()
        {
            //ie11
            WebBrowserHelper.SetWebBrowserFeatures(11);

            NavigationComboBox.Items.Clear();

            foreach (string key in IniFileHelper.ReadIniKeys("URL", Constant.HistoricalURL))
            {
                string value = IniFileHelper.ReadIniValue("URL", key, "", Constant.HistoricalURL);

                AddBrowseItem(new ItemInfo { Title = value, URL = key});

                if (NavigationComboBox.Items.Count >= 10)
                {
                    break;
                }
            }
        }

        private ItemInfo GetItemInfo(string url, ForumClient forumClient)
        {
            var items = forumClient.GetBordInfo(url);

            ItemInfo itemInfo = new ItemInfo();

            itemInfo.Title = items.title;
            itemInfo.URL = url;
            itemInfo.TopicId = items.topicId;
            itemInfo.ForumInfos = ForumInfos;

            #region "采集子帖"
            Regex reg = new Regex(@"(?is)<a[^>]*?href=(['""]?)(?<url>[^'""\s>]+)\1[^>]*>(?<text>(?:(?!</?a\b).)*)</a>");
            MatchCollection matchs = reg.Matches(EscapeCharacterHelper.XamlTransformation(EscapeCharacterHelper.XamlTransformation(items.innerHtml)));

            foreach (Match match in matchs)
            {
                string link = EscapeCharacterHelper.XamlTransforEmpty(match.Groups["url"].Value).Trim();

                if (Constant.IsForumURL(link) && !itemInfo.ChildrenLinks.Contains(link))
                {
                    itemInfo.ChildrenLinks.Add(link);
                }
            }

            foreach (var reply in items.replys)
            {
                MatchCollection _matchs = reg.Matches(EscapeCharacterHelper.XamlTransformation(EscapeCharacterHelper.XamlTransformation(reply.innerHtml)));

                foreach (Match _match in _matchs)
                {
                    string link = EscapeCharacterHelper.XamlTransforEmpty(_match.Groups["url"].Value).Trim();

                    if (Constant.IsForumURL(link) && !itemInfo.ChildrenLinks.Contains(link))
                    {
                        if (reply.innerHtml.Contains("---"))
                        {
                            //抠出的子帖
                            itemInfo.ChildrenLinks.Remove(link);
                        }
                        else
                        {
                            itemInfo.ChildrenLinks.Add(link);
                        }

                    }
                }
            }
            #endregion

            foreach (var fileInfo in items.files)
            {
                itemInfo.UploadFileList.Add(new UploadFileInfo { FileHref = fileInfo.downloadUrl, FileName = fileInfo.fileName, Floor = 0, UserName = items.userName });
            }
            
            foreach(var reply in items.replys)
            {
                foreach(var fileInfo in reply.files)
                {
                    itemInfo.UploadFileList.Add(new UploadFileInfo { FileHref = fileInfo.downloadUrl, FileName = fileInfo.fileName, Floor = itemInfo.UploadFileList.Count, UserName = reply.userName });
                }

                foreach (Match R in new Regex("(R:|r:)(.*?<)").Matches(reply.innerHtml))
                {
                    string address = R.Value.Trim().Replace("<","");

                    itemInfo.DiskAddressList.Add(new DiskAddressInfo { Address = address, Floor = itemInfo.DiskAddressList.Count, UserName = reply.userName});
                }

                foreach (Match W in new Regex("(w:|W:)(.*?<)").Matches(reply.innerHtml))
                {
                    string address = "w:" + W.Value.Trim();

                    itemInfo.DataDiskAddressList.Add(new DiskAddressInfo { Address = address, Floor = itemInfo.DiskAddressList.Count, UserName = reply.userName });
                }
            }

            return itemInfo;
        }

        ///加载
        private void Load(string url, ForumClient forumClient, ItemInfo ParentInfo = null)
        {
            if (!Constant.IsForumURL(url))
            {
                //不是有效链接
                return;
            }

            ItemInfo MasterItem = GetItemInfo(url, forumClient);

            MasterItem.ParentInfo = ParentInfo;

            if (ForumInfos.ForumAlreadyInfos.Count == 0)
            {
                AddBrowseItem(MasterItem);
            }


            if (!ForumInfos.ForumAlreadyInfos.Keys.Contains(MasterItem.TopicId))
            {
                AddItem(MasterItem);
                ForumInfos.ForumAlreadyInfos.Add(MasterItem.TopicId, MasterItem);
            }
          
            foreach (string childrenLink in MasterItem.ChildrenLinks)
            {
                Load(childrenLink, forumClient, MasterItem);
            }


        }


        /// <summary>
        /// 浏览
        /// </summary>
        /// <param name="url"></param>
        private void Forum(string url)
        {
            if (!Constant.IsForumURL(url))
            {
                //不是有效链接
                return;
            }

            ClearList();
            HiddenList();

            Thread _thread = new Thread(new ThreadStart(new Action(async () =>
            {
                try
                {

                    CookieContainer cookieContainer = Constant.GetCookieContainer();
                    
                    if (cookieContainer != null)
                    {
                        ForumClient forumClient = new ForumClient(cookieContainer);

                        Load(url, forumClient);

                    }

                    OnLoadCompleted();
                    ShowList();
            }
                catch (Exception ex)
            {
                ShowList();
                throw ex;
            }

        })))
            {
                IsBackground = true
            };
            _thread.Start();
            
        }


        /// <summary>
        /// 全部加载完成后
        /// </summary>
        private void OnLoadCompleted()
        {
            this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (ThreadStart)delegate ()
            {
                foreach (UserControls.Item item in ItemList.Children)
                {
                    item.CheckInvalid();
                }

                if (string.IsNullOrWhiteSpace(ForumInfos.MasterURL))
                {
                    ForumInfos.MasterURL = NavigationComboBox.Text.Trim();
                }
            });

            ShowList();
        }

        /// <summary>
        /// 添加单据信息
        /// </summary>
        /// <param name="title"></param>
        /// <param name="url"></param>
        /// <returns></returns>
        private void AddItem(ItemInfo itemInfo)
        {
            this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (ThreadStart)delegate ()
            {
                UserControls.Item item = new UserControls.Item(itemInfo);

                item.ShowList += new UserControls.Item.ShowListHandler(ShowList);
                item.HiddenList += new UserControls.Item.HiddenListHandler(HiddenList);

                if (itemInfo.ParentInfo != null)
                {
                    itemInfo.ParentInfo.Item.AddItem(item);
                }
                else
                {
                    ItemList.Children.Add(item);
                }

                itemInfo.Item = item;
              });

        }

        private void ClearList()
        {
            ItemList.Children.Clear();
            ForumInfos = new ForumInfo();

            ForumWindow.IsEnabled = false;
        }

        public void ShowList()
        {
            this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (ThreadStart)delegate () {
                ForumWindow.IsEnabled = true;
            });
        }

        public void HiddenList()
        {
            this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (ThreadStart)delegate () {
                ForumWindow.IsEnabled = false;
            });
        }

        #region "浏览下拉框"

        private string DisplayText = "";
        private void NavigationComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox comboBox = sender as ComboBox;

            if (comboBox.SelectedItem is UserControls.BrowseItem item)
            {
                DisplayText = item.ItemURL.Text;
                NavigationComboBox.Text = DisplayText;

                //限制只有99论坛
                if (Constant.IsForumURL(item.ItemURL.Text))
                {
                    ClearList();

                    string URL = item.ItemURL.Text.Trim().ToString();

                    ForumInfos.MasterURL = URL;
                    Forum(URL);
                }
            }
        }

        private void NavigationComboBox_DropDownClosed(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(DisplayText))
            {
                NavigationComboBox.Text = DisplayText;

                NavigationComboBox.IsEditable = false;
                NavigationComboBox.IsEditable = true;

                DisplayText = "";
            }
        }

        private void NavigationComboBox_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.WidthChanged)
            {
                if (e.NewSize.Width - DropItemListMargin > 0)
                {
                    DropItemListWidth = (e.NewSize.Width) - 20;

                    foreach (UserControls.BrowseItem item in NavigationComboBox.Items)
                    {
                        item.Item.Width = DropItemListWidth;
                        item.ItemTitle.Width = (DropItemListWidth - DropItemListMargin) / 2;
                        item.ItemURL.Width = (DropItemListWidth - DropItemListMargin) / 2;
                    }

                }
            }
        }

        /// <summary>
        /// 开始浏览
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NavigationComboBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key.Equals(Key.Enter))
            {
                ComboBox comboBox = (ComboBox)sender;
                string URL = comboBox.Text.Trim().ToString();

                //限制只有99论坛
                if (Constant.IsForumURL(URL))
                {
                    ClearList();

                    ForumInfos.MasterURL = URL;

                    Forum(URL);
                }
            }
        }

        private void RecordHistoricalURL(string title, string url)
        {
            IniFileHelper.WriteIniValue("URL", url, title, Constant.HistoricalURL);
        }

        private void RemoveHistoricalURL(string url)
        {
            IniFileHelper.DelIniKey("URL", url, Constant.HistoricalURL);
        }

        private void AddBrowseItem(ItemInfo info)
        {
            this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (ThreadStart)delegate ()
            {
                if (HistoricalURL.Where(i => i.URL.Equals(info.URL)).Count() == 0)
                {
                    UserControls.BrowseItem browseItem = new UserControls.BrowseItem(info, DropItemListWidth, DropItemListMargin);

                    NavigationComboBox.Items.Insert(0, browseItem);
                    HistoricalURL.Add(info);

                    info.BrowseItem = browseItem;

                    browseItem.BrowseItemRmoveBtn.Click += new RoutedEventHandler(DelBrowseItem);
                    browseItem.BrowseItemRmoveBtn.Tag = browseItem;

                    RecordHistoricalURL(info.Title, info.URL);

                    if (NavigationComboBox.Items.Count > 10)
                    {
                        UserControls.BrowseItem HistoryItem = NavigationComboBox.Items.GetItemAt(NavigationComboBox.Items.Count - 1) as UserControls.BrowseItem;

                        RemoveHistoricalURL(HistoryItem.Info.URL);
                        NavigationComboBox.Items.RemoveAt(NavigationComboBox.Items.Count - 1);
                    }
                }
            });
        }

        private void DelBrowseItem(object sender, RoutedEventArgs e)
        {
            UserControls.BrowseItem Item = ((Button)sender).Tag as UserControls.BrowseItem;

            RemoveHistoricalURL(Item.Info.URL);
            NavigationComboBox.Items.Remove(Item);
            HistoricalURL.Remove(Item.Info);
        }

        #endregion

        /// <summary>
        /// 打开输出目录
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OpenExportPath(object sender, RoutedEventArgs e)
        {
            Process.Start(Constant.OutputDirectory);
        }

        /// <summary>
        /// 打开R盘
        /// </summary>
        private void OpenTaskDisk()
        {
            if (!System.IO.Directory.Exists("R:\\"))
            {
                string bat = System.IO.Path.Combine(Constant.HelpDirectory, "taskdist.bat");
                if (System.IO.File.Exists(bat))
                {
                    Process process = new Process();
                    process.StartInfo.FileName = bat;
                    process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;

                    process.Start();
                }
            }
        }

        /// <summary>
        /// 打开W盘
        /// </summary>
        private void OpenDataDisk()
        {
            if (!System.IO.Directory.Exists("W:\\"))
            {
                string bat = System.IO.Path.Combine(Constant.HelpDirectory, "otherDist.bat");

                if (System.IO.File.Exists(bat))
                {
                    Process process = new Process();
                    process.StartInfo.FileName = bat;
                    process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;

                    process.Start();
                }
            }
        }

        #region "登录界面"
        //登录界面加载
        private void WebBrowser_LoadCompleted(object sender, NavigationEventArgs e)
        {
            WebBrowser browser = (WebBrowser)sender;
            HTMLDocument document = (HTMLDocument)browser.Document;

            if (document.url.Contains("http://forum.nderp.99.com/Forum/Default.aspx"))
            {
                Constant.LoginCookie = document.cookie;

                HiddenLoginWindow();
                browser.Dispose();
            }
            else
            {
                ShowLoginWindow();
            }
        }

        private void ShowLoginWindow()
        {
            ForumWindow.Visibility = Visibility.Hidden;

            webBrowser.Visibility = Visibility.Visible;
            webBrowser.Width = ForumWindow.Width;
            webBrowser.Height = ForumWindow.Height;
        }

        private void HiddenLoginWindow()
        {
            webBrowser.Visibility = Visibility.Hidden;
            webBrowser.Width = 0;
            webBrowser.Height = 0;

            ForumWindow.Visibility = Visibility.Visible;
        }
        #endregion

        #region "功能按钮"
        private void Btn_ExportUploadFiles_Click(object sender, RoutedEventArgs e)
        {
            //if (ItemList.Children.Count > 0)
            //{
            //    ItemInfo ItemInfo = ForumInfos.ForumAlreadyInfos.Where(i => i.Value.URL.Equals(ForumInfos.MasterURL)).First().Value;

            //    string saveDir = System.IO.Path.Combine(Constant.OutputDirectory, new Regex(Constant.FileNameSpecial).Replace(string.Format(Constant.ProjectFolderName, ItemInfo.Title.Split('»').Last()), "") + "\\论坛附件");

            //    if (!System.IO.Directory.Exists(saveDir))
            //    {
            //        DirectoryHelper.CreateDirectory(saveDir);
            //    }

            //    bool IsExport = false;

            //    foreach (UserControls.Item item in ItemList.Children.OfType<UserControls.Item>())
            //    {
            //        if (item.Info.IsInvalid)
            //        {
            //            item.Info.Item.ExportUploadFile(sender, e);

            //            IsExport = true;
            //        }
            //    }


            //    if (IsExport)
            //    {
            //        Process.Start(saveDir);
            //    }
            //}

           
        }

        private void Btn_ExportDataDiskFiles_Click(object sender, RoutedEventArgs e)
        {
            if (!System.IO.Directory.Exists("W:\\"))
            {
                MessageBox.Show("W盘未找到。","提示");
                return;
            }

            HiddenList();

            Thread _thread = new Thread(new ThreadStart(new Action(() =>
            {
                try
                {
                    if (ForumInfos.ForumAlreadyInfos.Count > 0 && !string.IsNullOrWhiteSpace(ForumInfos.MasterURL))
                    {
                        ItemInfo ItemInfo = ForumInfos.ForumAlreadyInfos.Where(i => i.Value.URL.Equals(ForumInfos.MasterURL)).First().Value;

                        string saveDir = System.IO.Path.Combine(Constant.OutputDirectory, new Regex(Constant.FileNameSpecial).Replace(string.Format(Constant.ProjectFolderName, ItemInfo.Title.Split('»').Last()), "") + "\\W盘文件");

                        if (!System.IO.Directory.Exists(saveDir))
                        {
                            DirectoryHelper.CreateDirectory(saveDir);
                        }
                        else
                        {
                            DirectoryHelper.DeleteDirectory(saveDir, true);
                            DirectoryHelper.CreateDirectory(saveDir);
                        }

                        IEnumerable<KeyValuePair<int, ItemInfo>> Items = ForumInfos.ForumAlreadyInfos.Where(i => i.Value.DataDiskAddressList.Count > 0);

                        bool IsExport = false;

                        foreach (var item in Items)
                        {
                            if (item.Value.IsInvalid && item.Value.IsActive)
                            {
                                item.Value.Item.ExportDataDiskFile(sender, e);

                                IsExport = true;
                            }
                        }

                        if (IsExport)
                        {
                            Process.Start(saveDir);
                        }

                        ShowList();
                    }
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

        private void Btn_ExportDiskFiles_Click(object sender, RoutedEventArgs e)
        {
            if (!System.IO.Directory.Exists("R:\\"))
            {
                MessageBox.Show("R盘未找到。", "提示");
                return;
            }

            HiddenList();

            Thread _thread = new Thread(new ThreadStart(new Action(() =>
            {
                try
                {
                    if (ForumInfos.ForumAlreadyInfos.Count > 0 && !string.IsNullOrWhiteSpace(ForumInfos.MasterURL))
                    {
                        ItemInfo ItemInfo = ForumInfos.ForumAlreadyInfos.Where(i => i.Value.URL.Equals(ForumInfos.MasterURL)).First().Value;

                        string saveDir = System.IO.Path.Combine(Constant.OutputDirectory, new Regex(Constant.FileNameSpecial).Replace(string.Format(Constant.ProjectFolderName, ItemInfo.Title.Split('»').Last()), "") + "\\R盘文件");

                        if (!System.IO.Directory.Exists(saveDir))
                        {
                            DirectoryHelper.CreateDirectory(saveDir);
                        }
                        else
                        {
                            DirectoryHelper.DeleteDirectory(saveDir,true);
                            DirectoryHelper.CreateDirectory(saveDir);
                        }

                        IEnumerable<KeyValuePair<int, ItemInfo>> Items = ForumInfos.ForumAlreadyInfos.Where(i => i.Value.DiskAddressList.Count > 0);

                        bool IsExport = false;

                        foreach (var item in Items)
                        {
                            if (item.Value.IsInvalid && item.Value.IsActive)
                            {
                                item.Value.Item.ExportDiskFile(sender, e);

                                IsExport = true;
                            }
                        }

                        if (IsExport)
                        {
                            Process.Start(saveDir);
                        }
                    }

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

        private void Btn_BillsList_Click(object sender, RoutedEventArgs e)
        {
            HiddenList();

            Thread _thread = new Thread(new ThreadStart(new Action(() =>
            {
                try
                {
                    if (ForumInfos.ForumAlreadyInfos.Count > 0)
                    {
                        ItemInfo Item = ForumInfos.ForumAlreadyInfos.Where(i => i.Value.URL.Equals(ForumInfos.MasterURL)).First().Value;
                        string saveFileName = new Regex(Constant.FileNameSpecial).Replace(string.Format(Constant.ProjectFolderName, Item.Title.Split('»').Last()), "");
                        string saveDir = System.IO.Path.Combine(Constant.OutputDirectory, saveFileName);

                        BillsList();
                    }

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

        private void Btn_SchedulingList_Click(object sender, RoutedEventArgs e)
        {
            HiddenList();

            Thread _thread = new Thread(new ThreadStart(new Action(() =>
            {
                try
                {
                    if (ForumInfos.ForumAlreadyInfos.Count > 0)
                    {
                        ItemInfo Item = ForumInfos.ForumAlreadyInfos.Where(i => i.Value.URL.Equals(ForumInfos.MasterURL)).First().Value;
                        string saveFileName = new Regex(Constant.FileNameSpecial).Replace(string.Format(Constant.ProjectFolderName, Item.Title.Split('»').Last()), "");
                        string saveDir = System.IO.Path.Combine(Constant.OutputDirectory, saveFileName);

                        SchedulingList();
                    }

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

        #endregion

        private void SchedulingList()
        {
            if (ForumInfos.ForumAlreadyInfos.Count > 0 && !string.IsNullOrWhiteSpace(ForumInfos.MasterURL))
            {
                ItemInfo Item = ForumInfos.ForumAlreadyInfos.Where(i => i.Value.URL.Equals(ForumInfos.MasterURL)).First().Value;

                Microsoft.Office.Interop.Excel.Application excel = new Microsoft.Office.Interop.Excel.Application();
                excel.Visible = false;
                var book = excel.Workbooks.Add();
                var sheet = book.Sheets.Add() as Microsoft.Office.Interop.Excel.Worksheet;

                sheet.Columns.ColumnWidth = 100;
                sheet.Rows.RowHeight = 28;

                var firstrange = sheet.Range["A1"];
                firstrange.Value2 = Item.Title.Split('»').Last();
                sheet.Hyperlinks.Add(firstrange, Item.URL);
                firstrange.Interior.Color = System.Drawing.Color.Orange;

                int index = 1;
                foreach (ItemInfo childrenItem in Item.ChildrenInfos)
                {
                    if (childrenItem.IsInvalid && childrenItem.IsActive)
                    {
                        index++;
                        // add link
                        var range = sheet.Range["A" + index];
                        range.Value2 = childrenItem.Title.Split('»').Last();
                        sheet.Hyperlinks.Add(range, childrenItem.URL);
                    }
                }

                string saveFileName = new Regex(Constant.FileNameSpecial).Replace(string.Format(Constant.ProjectFolderName, Item.Title.Split('»').Last()), "");

                string saveDir = System.IO.Path.Combine(Constant.OutputDirectory, saveFileName);

                if (!System.IO.Directory.Exists(saveDir))
                {
                    DirectoryHelper.CreateDirectory(saveDir);
                }

                //保存文档  
                string savePath = System.IO.Path.Combine(saveDir, saveFileName + ".xlsx");

                try
                {
                    if (System.IO.File.Exists(savePath))
                    {
                        System.IO.File.Delete(savePath);
                    }

                    Thread.Sleep(1000);

                    book.Close(true, savePath);
                    excel.Quit();
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(book);
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(excel);

                    Process.Start(saveDir);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "排期列表导出失败");
                    return;
                }
            }

        }

        private void BillsList()
        {
            if (ForumInfos.ForumAlreadyInfos.Count > 0 && !string.IsNullOrWhiteSpace(ForumInfos.MasterURL))
            {
                ItemInfo ItemInfo = ForumInfos.ForumAlreadyInfos.Where(i => i.Value.URL.Equals(ForumInfos.MasterURL)).First().Value;

                HelperClass.ExcelHelper excel = new ExcelHelper();
                excel.Open();
                excel.m_objSheet.get_Range("A1", "A65535").ColumnWidth = 80;
                excel.m_objSheet.get_Range("B1", "B65535").ColumnWidth = 160;
                excel.m_objSheet.get_Range("A1", excel.m_objOpt).Value = "任务名";
                excel.m_objSheet.get_Range("B1", excel.m_objOpt).Value = "R盘地址";
                excel.m_objSheet.get_Range("A1", excel.m_objOpt).Interior.Color = ColorTranslator.FromHtml("#FF808080");
                excel.m_objSheet.get_Range("B1", excel.m_objOpt).Interior.Color = ColorTranslator.FromHtml("#FF808080");

                int index = 1;

                IEnumerable<KeyValuePair<int, ItemInfo>> Items = ForumInfos.ForumAlreadyInfos.Where(i => !i.Value.URL.Equals(ItemInfo.URL) && i.Value.ParentInfo.URL.Equals(ItemInfo.URL));

                foreach (var Item in Items)
                {
                    if (Item.Value.IsInvalid && Item.Value.IsActive && Item.Value.DiskAddressList.Count > 0)
                    {
                        index++;
                        excel.m_objSheet.get_Range("A" + index, excel.m_objOpt).Value = Item.Value.Title.Split('»').Last();
                        excel.m_objSheet.get_Range("B" + index, excel.m_objOpt).Value = System.IO.Path.GetDirectoryName(System.IO.Path.GetDirectoryName(Item.Value.DiskAddressList.Last().Address));

                    }

                }


                string saveFileName = new Regex(Constant.FileNameSpecial).Replace(string.Format(Constant.ProjectFolderName, ItemInfo.Title.Split('»').Last()), "");

                string saveDir = System.IO.Path.Combine(Constant.OutputDirectory, saveFileName);

                if (!System.IO.Directory.Exists(saveDir))
                {
                    DirectoryHelper.CreateDirectory(saveDir);
                }

                string savePath = System.IO.Path.Combine(saveDir, "单据列表.xlsx");

                try
                {
                    if (System.IO.File.Exists(savePath))
                    {
                        System.IO.File.Delete(savePath);
                    }

                    excel.SaveFile(savePath);
                    excel.Dispose();
                    excel = null;

                    Process.Start(saveDir);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "排期列表导出失败");
                    return;
                }
            }
        }

    }
}
