using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using System.Xml;
using HtmlAgilityPack;
using Microsoft.Win32;
using RaidBossChecker.Properties;

namespace RaidBossChecker
{
    public partial class MainWindow : Window
    {

        // TIME
        private DateTime mskDateTime;
        private DateTime respawnStart;
        private DateTime respawnEnd;
        private int[] secondsToUpdate = { 10, 15, 30 };
        private int[] showTime = { 10, 2000 }; // change number if need to test program: how long program see a boss in minutes
        private DispatcherTimer dispatcherTimer = new DispatcherTimer(); // timer object

        private List<string> rbNames = new List<string>(); // names that comet to this list after you selected boss(es) in check box(es)
        private List<RaidBoss> rbInfo = new List<RaidBoss>(); // all bosses list after time filter
        private string[] rbAllKeybossNames = { "Cabrio", "Kernon", "Hallate", "Golkonda" }; // array of key boss names. It used only for button "show all key bosses"
        private string[] rbAllNames = { "Cabrio", "Kernon", "Hallate", "Golkonda", "Chief Horus", "Commander Mos", "Barakiel", "Hero Hekaton", "Commander Tayr", "Queen Ant", "Orfen", "Core", "Antharas", "Baium", "Valakas", "Beleth" }; // array of all boss names. It used only for button "show all bosses"

        private List<ServerList> serverList = new List<ServerList>(); // server list object

        // LOAD SERVER NAMES START
        // Load server names for combo box where change server
        private void LoadServerList()
        {
            HtmlDocument serverListUrl = new HtmlWeb().Load("https://asterios.tm/index.php?cmd=rss");

            var serverListOptions = serverListUrl.DocumentNode.SelectNodes("//*[@id='serv']/option");

            foreach (var option in serverListOptions)
            {
                serverList.Add(new ServerList { ServerId = Convert.ToInt32(option.Attributes["value"].Value), ServerName = option.InnerText });
            }
            
        }
        // LOAD SERVER NAMES END



        // HTML READER START
        private HtmlDocument htmlDocument;
        private async Task ReaderHtml(string filter, int count, bool search)
        {
            string url = "https://asterios.tm/index.php?cmd=rss&serv=" + cboxServerList.SelectedValue + "&filter=" + filter + "&count=" + count;
            
            await Task.Factory.StartNew(() => 
            {
                htmlDocument = new HtmlWeb().Load(url); 
            });

            var bossesList = htmlDocument.DocumentNode.SelectNodes("//*[@id='page_contents']//table//tr[1]//td//a");
            foreach (var item in bossesList.Take(count))
            {
                string nameBoss = item.InnerText.Substring(item.InnerText.LastIndexOf(":"))
                    .Replace(": ", String.Empty)
                    .Replace("Boss ", String.Empty)
                    .Replace(" was killed", String.Empty)
                    .Replace("Убит босс ", String.Empty);
                DateTime dateKilledMsk = Convert.ToDateTime(item.InnerText.Remove(item.InnerText.LastIndexOf(":"))); // MSK time
                DateTime dateKilledUtc = dateKilledMsk.AddHours(-3);
                DateTime dateKilledLocal = dateKilledUtc.ToLocalTime();

                if (!search || (dateKilledUtc.AddMinutes(showTime[0]) >= DateTime.UtcNow & dateKilledUtc <= DateTime.UtcNow)) // если время респа + добавленное время больше текущего тогда показываем
                {

                    if (checkBoxMskTime.IsChecked == true) // MSK time check box
                    {
                        respawnStart = dateKilledMsk.AddHours(18); // respawn start MSK time
                        respawnEnd = dateKilledMsk.AddHours(30); // respawn end MSK time

                        rbInfo.Add(new RaidBoss() { Name = nameBoss, TimeKilled = dateKilledMsk, RespawnStart = respawnStart, RespawnEnd = respawnEnd }); // time killed MSK (+3 hours)
                    }

                    else // not MSK time check box
                    {
                        respawnStart = dateKilledLocal.AddHours(18); // respawn start time
                        respawnEnd = dateKilledLocal.AddHours(30); // respawn end time

                        rbInfo.Add(new RaidBoss() { Name = nameBoss, TimeKilled = dateKilledLocal, RespawnStart = respawnStart, RespawnEnd = respawnEnd }); // time killed computer time
                    }

                }
                if (checkBoxMskTime.IsChecked == true)
                {
                    lblLastTimeUpdated.Content = DateTime.UtcNow.AddHours(3).ToString("dd.MM.yyyy HH:mm:ss"); // Last time updated by MSK time

                }
                if (checkBoxMskTime.IsChecked == false)
                {
                    lblLastTimeUpdated.Content = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"); // last time updated by computer time
                }

            }
            
        }
        // HTML READER END

        // RSS READER FOR BOSSES START
        // RssReader:
        // filter [string] - filter name for the link. 'null' - to skip it, "keyboss" - key bosses.
        // count [int] - how many lines ask web site to get to method
        // Search [bool] - do we need to filter bosses base on time from now OR not. 'true' - yes, need to check, 'false' - skip this IF
        private XmlReader reader;
        private SyndicationFeed feedKey;
        private SyndicationFeed feedEpic;
        private SyndicationFeed feedAllBosses;
        private async Task ReaderRss(bool search)
        {
            string urlKey = "https://asterios.tm/index.php?cmd=rss&serv=" + cboxServerList.SelectedValue + "&filter=keyboss&out=xml";
            string urlEpic = "https://asterios.tm/index.php?cmd=rss&serv=" + cboxServerList.SelectedValue + "&filter=epic&out=xml";

            await Task.Factory.StartNew(() =>
            {
                using (reader = XmlReader.Create(urlKey)) 
                { 
                    feedKey = SyndicationFeed.Load(reader);
                }
                using (reader = XmlReader.Create(urlEpic))
                {
                    feedEpic = SyndicationFeed.Load(reader);
                }
                feedAllBosses = new SyndicationFeed(feedKey.Items.Union(feedEpic.Items));
            });

            //var feed = FeedReader.Read(@"https://asterios.tm/index.php?cmd=rss&serv=" + cboxServerList.SelectedValue  + "&filter=" + filter + "&out=xml"); // link cboxServerList.SelectedValue

            foreach (SyndicationItem item in feedAllBosses.Items)
            {
                String title = item.Title.Text
                    .Replace("Boss ", String.Empty)
                    .Replace(" was killed", String.Empty)
                    .Replace("Убит босс ", String.Empty); // Remove some words from rss

                //listBox.Items.Add(item.Title + " - " + item.PublishingDate); // ONLY FOR TEST

                if (!search || (item.PublishDate.UtcDateTime.AddMinutes(showTime[0]) >= DateTime.UtcNow & item.PublishDate.UtcDateTime <= DateTime.UtcNow)) // если время респа + добавленное время больше текущего тогда показываем
                {

                    if (checkBoxMskTime.IsChecked == true) // MSK time check box
                    {
                        mskDateTime = item.PublishDate.UtcDateTime.AddHours(3); // Convert to MSK time
                        respawnStart = item.PublishDate.UtcDateTime.AddHours(21); // respawn start MSK time
                        respawnEnd = item.PublishDate.UtcDateTime.AddHours(33); // respawn end MSK time

                        rbInfo.Add(new RaidBoss() { Name = title, TimeKilled = mskDateTime, RespawnStart = respawnStart, RespawnEnd = respawnEnd }); // time killed MSK (+3 hours)
                    }

                    else // not MSK time check box
                    {
                        respawnStart = item.PublishDate.UtcDateTime.ToLocalTime().AddHours(18); // respawn start time
                        respawnEnd = item.PublishDate.UtcDateTime.ToLocalTime().AddHours(30); // respawn end time

                        rbInfo.Add(new RaidBoss() { Name = title, TimeKilled = item.PublishDate.UtcDateTime.ToLocalTime(), RespawnStart = respawnStart, RespawnEnd = respawnEnd }); // time killed computer time
                    }

                }
                if (checkBoxMskTime.IsChecked == true)
                {
                    lblLastTimeUpdated.Content = DateTime.UtcNow.AddHours(3).ToString("dd.MM.yyyy HH:mm:ss"); // Last time updated by MSK time

                }
                if (checkBoxMskTime.IsChecked == false)
                {
                    lblLastTimeUpdated.Content = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"); // last time updated by computer time
                }

            }
        }
        // RSS READER FOR BOSSES END

        // TIMER START
        private void TimerStart()
        {
            dispatcherTimer.Tick += new EventHandler(TimerStart_Tick);
            dispatcherTimer.Interval = new TimeSpan(0, 0, secondsToUpdate[cboxSecondsToUpdate.SelectedIndex]);
            dispatcherTimer.Start();
        }

        private async void TimerStart_Tick(object sender, EventArgs e)
        {
            await CheckSpecificRaidBossAsync();
        }

        private void TimerStop()
        {
            dispatcherTimer.Tick -= TimerStart_Tick; // delete this event from timer tick
            dispatcherTimer.Stop(); // before was start FIX
        }

        private async Task CheckSpecificRaidBossAsync()
        {

            rbInfo.Clear();
            listView.Items.Clear();

            await ReaderHtml("keyboss", 5, true); // search only in first 10 keybosses and compair their time to our

            // filtring and sending rbInfo list information to ListView
            if (rbInfo.Count > 0)
            {
                rbInfo.Where(x => rbNames.Count(y => x.Name.Contains(y)) != 0)
                    .ToList<RaidBoss>()
                    .ForEach(x => listView.Items
                    .Add(new RaidBoss() { Name = x.Name, TimeKilled = x.TimeKilled, RespawnStart = x.RespawnStart, RespawnEnd = x.RespawnEnd })); // try to get it by youself
            }
            lblCount.Content = listView.Items.Count.ToString(); // Count how many bosses are founded
            
            if (listView.Items.Count > 0) // if items in the list is more then 0 (it means 1+) then:
            {
                Sound.StartPlaySoundLoop(); // start playing sound
                SearchAnimationStop();
                TimerStop(); // stop timer and any events (this method also) - stop searching
            }
        }
        // TIMER END


        // GET VERSION ASSEMBLY START
        private void GetVersion() // version only for 'about' page
        {
            string getVersion;
            getVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            TextRunVersion.Text = getVersion; // to text in about page
        }
        // GET VERSION ASSEMBLY END

        // GET COPYRIGHT YEAR START
        private void GetCopyrightYear()
        {
            int copyrightYear = Convert.ToInt32(DateTime.Today.Year);
            if (copyrightYear > Convert.ToInt32(TextRunCopyrightYear.Text)) // compare todays year with TextRunCopyrightYear which is 2 0 2 0
            {
                TextRunCopyrightYear.Text = "2020-" + copyrightYear; // write new years
            }
        }
        // GET COPYRIGHT YEAR END

        // BUTTON 'START' BASED ON CHECK BOXES START
        // If rbNames list has more then 0 (it means 1+) items => button 'Start' is enabled
        // If rbNames list has less then 1 (it means 0) items => button 'Start' is not enabled
        private void ButtonStartAfterCheckBox()
        {
            if (rbNames.Count < 1)
            {
                btnStart.IsEnabled = false;
            }
            if (rbNames.Count > 0)
            {
                btnStart.IsEnabled = true;
            }
        }
        // BUTTON 'START' BASED ON CHECK BOXES END

        // TRANSLATOR START
        private void SetLanguageDictionary()
        {
            try
            {
                // prefix to the relative Uri for resource (xaml file)
                string _prefix = String.Concat(typeof(App).Namespace, ";component/");

                // clear all ResourceDictionaries
                this.Resources.MergedDictionaries.Clear();

                // get correct file
                string filename = "";
                switch (Thread.CurrentThread.CurrentCulture.ToString().Substring(0, 2))
                {
                    case "ru":
                        filename = "..\\Languages\\Russian.xaml";
                        break;
                    case "uk":
                        filename = "..\\Languages\\Russian.xaml";
                        break;
                    case "be":
                        filename = "..\\Languages\\Russian.xaml";
                        break;
                    case "en":
                        filename = "..\\Languages\\English.xaml";
                        break;
                    default:
                        filename = "..\\Languages\\English.xaml";
                        break;
                }

                // add ResourceDictionary
                this.Resources.MergedDictionaries.Add
                (
                 new ResourceDictionary { Source = new Uri(String.Concat(_prefix + filename), UriKind.Relative) }
                );
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        // TRANSLATOR END

        public MainWindow()
        {
            InitializeComponent();
            SetLanguageDictionary();
            RegKey.Load();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Sound.LoadSoundList();
            cboxSecondsToUpdate.ItemsSource = secondsToUpdate;
            TopBarMessage.Opacity = 0;
            // buttons (start/stop)
            btnStop.IsEnabled = false; // button Stop is not enabled by default
            btnStart.IsEnabled = false; // button Start is not enabled by default
            SearchAnimationLoad(); // search animation get out
            // 'About' page
            GetVersion(); // version only for 'about' page
            GetCopyrightYear(); // copyright year only for 'about' page
        }

        // TOP BAR
        private void GridTopBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove(); // top bar is able to move when you did left click on it
        }

        // BUTTON 'EXIT'
        private void BtnExit_Click(object sender, RoutedEventArgs e)
        {
            
            Application.Current.Shutdown(); // close application
        }

        // BUTTON 'MINIMIZE'
        private void BtnMinimizeWindow_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        // BUTTON 'START'
        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            listView.Visibility = Visibility.Visible; // показывать ListView на переднем плане
            listViewAllRB.Visibility = Visibility.Hidden; // скрыть ListViewAllRB на задний план
            listView.Items.Clear(); // предварительно очистить лист от предыдущих записей
            // SOUND
            Sound.SaveSoundToDisk(); // save selected sound to temp folder
            // Server List
            cboxServerList.IsEnabled = false; // prohibit to change server
            // check boxes (raidbosses)
            checkBoxCabrio.IsEnabled = false; // prohibit to change Cabrio check box
            checkBoxKernon.IsEnabled = false; // prohibit to change Kernon check box
            checkBoxHallate.IsEnabled = false; // prohibit to change Hallate check box
            checkBoxGolkonda.IsEnabled = false; // prohibit to change Golkonda check box
            // buttons (start/stop)
            btnStart.IsEnabled = false; // prohibit to use button Start
            btnStop.IsEnabled = true; // allow to use button Stop 
            // buttons 
            btnGetAllBosses.IsEnabled = false; // prohibit to use button Show all bosses
            btnGetAllKeyBosses.IsEnabled = false;  // prohibit to use button Show all key bosses respawn
            BtnOpenSettings.IsEnabled = false; // prohibit to use button Settings
            BtnOpenAbout.IsEnabled = false; // prohibit to use button About
            BtnOpenGuide.IsEnabled = false; // prohibit to use button Help/Guide
            // chests
            BtnChestCabrio.Opacity = 0.5;
            BtnChestHallate.Opacity = 0.5;
            BtnChestKernon.Opacity = 0.5;
            BtnChestGolkonda.Opacity = 0.5;
            // Search animation
            SearchAnimationStart(); // put the animation on the center and make in visible

            // timer (all processes)
            TimerStart(); // start searching
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            // timer (all processes)
            TimerStop(); // stop searching

            // SOUND STOP
            Sound.StopSound(); // stop playing sound
            Sound.DeleteSoundFromDisk(); // delete the sound from temp folder
            // Server List
            cboxServerList.IsEnabled = true; // allow to change server
            // check boxes (raidbosses)
            checkBoxCabrio.IsEnabled = true; // allow to change Cabrio check box
            checkBoxKernon.IsEnabled = true; // allow to change Kernon check box
            checkBoxHallate.IsEnabled = true; // allow to change Hallate check box
            checkBoxGolkonda.IsEnabled = true; // allow to change Golkonda check box
            // buttons (start/stop)
            btnStart.IsEnabled = true; // allow to use button Start
            btnStop.IsEnabled = false; // prohibit to use button Stop
            // buttons
            btnGetAllBosses.IsEnabled = true; // allow to use button Show all bosses
            btnGetAllKeyBosses.IsEnabled = true; // allow to use button Show all key bosses respawn
            BtnOpenSettings.IsEnabled = true; // allow to use button Settings
            BtnOpenAbout.IsEnabled = true; // allow to use button About
            BtnOpenGuide.IsEnabled = true; // allow to use button Help/Guide
            // chests
            BtnChestCabrio.Opacity = 1;
            BtnChestHallate.Opacity = 1;
            BtnChestKernon.Opacity = 1;
            BtnChestGolkonda.Opacity = 1;
            //
            SearchAnimationStop(); // put the animation out of screen and make it invisible
        }

        private async void btnGetAllKeyBosses_Click(object sender, RoutedEventArgs e)
        {
            btnGetAllKeyBosses.IsEnabled = false;
            listView.Visibility = Visibility.Visible;
            listViewAllRB.Visibility = Visibility.Hidden;
            rbInfo.Clear();
            listView.Items.Clear();

            await ReaderHtml("keyboss", 50, false);

            rbInfo.Where(x => rbAllKeybossNames.Count(y => x.Name.Contains(y)) != 0)
                .GroupBy(x => x.Name).Select(g => g.First()) // this line is to remove duplicates from the list
                .ToList<RaidBoss>()
                .ForEach(x => listView.Items.Add(new RaidBoss() { Name = x.Name, TimeKilled = x.TimeKilled, RespawnStart = x.RespawnStart, RespawnEnd = x.RespawnEnd })); // try to get it by youself

            lblCount.Content = listView.Items.Count.ToString(); // how many RB are founded
            btnGetAllKeyBosses.IsEnabled = true;
        }

        private async void btnGetAllBosses_Click(object sender, RoutedEventArgs e)
        {
            btnGetAllBosses.IsEnabled = false;
            listView.Visibility = Visibility.Hidden;
            listViewAllRB.Visibility = Visibility.Visible;
            rbInfo.Clear();
            listViewAllRB.Items.Clear();

            await ReaderRss(false);

            rbInfo.Where(x => rbAllNames.Count(y => x.Name.Contains(y)) != 0)
                .GroupBy(x => x.Name).Select(g => g.First()) // this line is to remove duplicates from the list
                .ToList<RaidBoss>()
                .ForEach(x => listViewAllRB.Items.Add(new RaidBoss() { Name = x.Name, TimeKilled = x.TimeKilled, RespawnStart = x.RespawnStart, RespawnEnd = x.RespawnEnd })); // try to get it by youself

            lblCount.Content = listViewAllRB.Items.Count.ToString(); // how many RB are founded
            btnGetAllBosses.IsEnabled = true;
        }

        private void checkBoxKernon_Checked(object sender, RoutedEventArgs e)
        {
            rbNames.Add("Kernon");
            ButtonStartAfterCheckBox();
        }

        private void checkBoxKernon_Unchecked(object sender, RoutedEventArgs e)
        {
            rbNames.Remove("Kernon");
            ButtonStartAfterCheckBox();
        }

        private void checkBoxCabrio_Checked(object sender, RoutedEventArgs e)
        {
            rbNames.Add("Cabrio");
            ButtonStartAfterCheckBox();
        }

        private void checkBoxCabrio_Unchecked(object sender, RoutedEventArgs e)
        {
            rbNames.Remove("Cabrio");
            ButtonStartAfterCheckBox();
        }

        private void checkBoxHallate_Checked(object sender, RoutedEventArgs e)
        {
            rbNames.Add("Hallate");
            ButtonStartAfterCheckBox();
        }

        private void checkBoxHallate_Unchecked(object sender, RoutedEventArgs e)
        {
            rbNames.Remove("Hallate");
            ButtonStartAfterCheckBox();
        }

        private void checkBoxGolkonda_Checked(object sender, RoutedEventArgs e)
        {
            rbNames.Add("Golkonda");
            ButtonStartAfterCheckBox();
        }

        private void checkBoxGolkonda_Unchecked(object sender, RoutedEventArgs e)
        {
            rbNames.Remove("Golkonda");
            ButtonStartAfterCheckBox();
        }

        private void sliderVolume_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                sliderVolume.Value = Convert.ToDouble(RegKey.Get("Volume").ToString().Replace(",", "."));
            }
            catch
            {
                sliderVolume.Value = Convert.ToDouble(RegKey.Get("Volume").ToString().Replace(".", ","));
            }
            
            sliderVolume.Minimum = 0.1;
        }

        private void sliderVolume_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Sound.mediaPlayer.Volume = Convert.ToDouble(sliderVolume.Value);
            RegKey.Set("Volume", sliderVolume.Value);
        }

        private void btnSoundTest_Click(object sender, RoutedEventArgs e)
        {
            Sound.StopSound();
            Sound.SaveSoundToDisk();
            Sound.StartPlayTest();
        }

        private void cboxSound_Loaded(object sender, RoutedEventArgs e)
        {
            cboxSound.ItemsSource = Sound.soundNames;
            cboxSound.SelectedItem = RegKey.Get("Sound");
        }

        private void cboxSound_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Sound.StopSound();
            RegKey.Set("Sound", cboxSound.SelectedItem);
        } 

        private void cboxServerList_Loaded(object sender, RoutedEventArgs e)
        {
            LoadServerList();
            cboxServerList.ItemsSource = serverList;
            cboxServerList.DisplayMemberPath = "ServerName";
            cboxServerList.SelectedValuePath = "ServerId";
            cboxServerList.SelectedValue = RegKey.Get("Server");
        }

        private void cboxServerList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RegKey.Set("Server", cboxServerList.SelectedValue);
        }

        private void checkBoxMskTime_Checked(object sender, RoutedEventArgs e)
        {
            lblMyTime.FontWeight = FontWeights.Regular; 
            lblMskTime.FontWeight = FontWeights.Bold;
        }

        private void checkBoxMskTime_Unchecked(object sender, RoutedEventArgs e)
        {
            lblMyTime.FontWeight = FontWeights.Bold;
            lblMskTime.FontWeight = FontWeights.Regular;
        }

        private void BtnCloseSettings_Click(object sender, RoutedEventArgs e)
        {
            Sound.StopSound(); // stop music
            Sound.DeleteSounds(); // delete all sounds
        }

        private void BtnCloseSettingsSecond_Click(object sender, RoutedEventArgs e) // the same as 'BtnClose_Click'
        {
            Sound.StopSound(); // stop music
            Sound.DeleteSounds(); // delete all sounds
        }

        private void cboxSecondsToUpdate_Loaded(object sender, RoutedEventArgs e)
        {
            cboxSecondsToUpdate.SelectedIndex = Convert.ToInt32(RegKey.Get("SecondsToUpdate"));
        }

        private void cboxSecondsToUpdate_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RegKey.Set("SecondsToUpdate", cboxSecondsToUpdate.SelectedIndex);
        }

        // TOP BAR MESSAGE

        private void TopBarMessageShow(string message)
        {
            Storyboard sb = Resources["SbTopBarMessage"] as Storyboard;
            TextRunTopBarMessage.Text = message;
            sb.Begin(TopBarMessage);
        }
        private void TopBarMessageShowAndCopy(string message)
        {
            Storyboard sb = Resources["SbTopBarMessage"] as Storyboard;
            string lang_topBarMessage_IsCopied = this.Resources["lang_TopBarMessage_IsCopied"].ToString();
            TextRunTopBarMessage.Text = message + " " + lang_topBarMessage_IsCopied;
            Clipboard.SetText(message);
            sb.Begin(TopBarMessage);
        }

        // CHEST COPY
        private void BtnChestCabrio_Click(object sender, RoutedEventArgs e)
        {
            TopBarMessageShowAndCopy("/target Coffer of the Dead");
        }

        private void BtnChestKernon_Click(object sender, RoutedEventArgs e)
        {
            TopBarMessageShowAndCopy("/target Hallate's chest");
        }

        private void BtnChestHallate_Click(object sender, RoutedEventArgs e)
        {
            TopBarMessageShowAndCopy("/target Chest of Kernon");
        }

        private void BtnChestGolkonda_Click(object sender, RoutedEventArgs e)
        {
            TopBarMessageShowAndCopy("/target Chest of Golkonda﻿");
        }

        // GUIDE PAGE
        public void GuidePage()
        {
            GridGuide.Margin = new Thickness(0, 0, 0, 0);
        }

        private void GridGuide_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            GridGuide.Margin = new Thickness(670, 0, -670, 0);
        }

        private void BtnOpenGuide_Click(object sender, RoutedEventArgs e)
        {
            GuidePage();
        }

        // SEARCH ANIMATION
        private void SearchAnimationLoad()
        {
            GridSearchAnimation.Visibility = Visibility.Hidden;
            GridSearchAnimation.Margin = new Thickness(350, 350, -350, -350);
            SearchAnimation_LineOne.Value = 100;
            SearchAnimation_LineTwo.Value = 100;
            SearchAnimation_LineThree.Value = 100;
        }

        private void SearchAnimationStart()
        {            
            GridSearchAnimation.Visibility = Visibility.Visible;
            GridSearchAnimation.Margin = new Thickness(0, 0, 0, 50);
        }

        private void SearchAnimationStop()
        {
            GridSearchAnimation.Visibility = Visibility.Hidden;
            GridSearchAnimation.Margin = new Thickness(350, 350, -350, -350);
        }

        // CONTACT BUTTONS
        private async void BtnContactEmail_Click(object sender, RoutedEventArgs e)
        {
            TopBarMessageShowAndCopy("ibkucher@gmail.com");
            await Task.Factory.StartNew(() =>
            {
                System.Diagnostics.Process.Start("mailto:ibkucher@gmail.com");
            });
        }

        private async void BtnContactGithub_Click(object sender, RoutedEventArgs e)
        {
            TopBarMessageShowAndCopy("github.com/ibkucher");
            await Task.Factory.StartNew(() =>
            {
                System.Diagnostics.Process.Start("https://github.com/ibkucher");
            }); 
        }

        private async void BtnContactTelegram_Click(object sender, RoutedEventArgs e)
        {
            TopBarMessageShowAndCopy("t.me/ibkucher");
            await Task.Factory.StartNew(() =>
            {
                System.Diagnostics.Process.Start("https://t.me/ibkucher");
            });
        }

        // COPY RAID BOSS INFORMATION
        private void CopyRaidBossInformation(object ListViewSenderSelectedItem, bool WithRespawnTime)
        {
            var item = (RaidBoss)ListViewSenderSelectedItem;
            if (item != null)
            {
                string lang_killedIn = this.Resources["lang_Code_KilledIn"].ToString();
                string lang_respawnFrom = this.Resources["lang_Code_RespawnFrom"].ToString();
                string lang_to = this.Resources["lang_Code_To"].ToString();

                string message;

                if (WithRespawnTime) 
                {
                    message = item.Name
                    + " "
                    + lang_killedIn
                    + " "
                    + item.TimeKilled.ToString("dd.MM.yyyy HH:mm:ss")
                    + ". "
                    + lang_respawnFrom
                    + " "
                    + item.RespawnStart.ToString("dd.MM HH:mm")
                    + " "
                    + lang_to
                    + " "
                    + item.RespawnEnd.ToString("dd.MM HH:mm");
                }
                else
                {
                    message = item.Name
                    + " "
                    + lang_killedIn
                    + " "
                    + item.TimeKilled.ToString("dd.MM.yyyy HH:mm:ss");
                }

                Clipboard.SetText(message);

                string lang_topBarMessage_InfoAbout = this.Resources["lang_TopBarMessage_InfoAbout"].ToString();
                string lang_topBarMessage_Copied = this.Resources["lang_TopBarMessage_Copied"].ToString();
                TopBarMessageShow(lang_topBarMessage_InfoAbout + " " + item.Name + " " + lang_topBarMessage_Copied);

                this.listView.SelectedIndex = -1;
                this.listViewAllRB.SelectedIndex = -1;
            }
        }

        private void listView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CopyRaidBossInformation(((ListView)sender).SelectedItem, true);
        }

        private void listViewAllRB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CopyRaidBossInformation(((ListView)sender).SelectedItem, false);
        }
    }
}
