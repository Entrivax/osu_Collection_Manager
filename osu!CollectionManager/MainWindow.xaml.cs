using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
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

namespace osu_CollectionManager
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        IniFile config;
        CollectionFile collectionFile = null;

        ObservableCollection<Beatmap> beatmaps { get; set; }

        public MainWindow()
        {
            config = new IniFile();
            InitializeComponent();
            VersionLabel.Content = "Version " + Assembly.GetExecutingAssembly().GetName().Version;
            CheckVersion();
        }

        public void CheckVersion()
        {
            Thread thread = new Thread(new ThreadStart(delegate()
            {
                try
                {
                    WebClient client = new WebClient();
                    string[] lastver = client.DownloadString("http://entrivax.fr/osuCollecMana/version").Split('.');
                    Version currentversion = Assembly.GetExecutingAssembly().GetName().Version;
                    Console.WriteLine(currentversion.Major);
                    Console.WriteLine(currentversion.Minor);
                    Console.WriteLine(currentversion.Build);
                    Console.WriteLine(currentversion.Revision);
                    if (int.Parse(lastver[0]) > currentversion.Major)
                    {
                        StatusLabel.Dispatcher.Invoke(new Action(delegate()
                        {
                            StatusLabel.Content = "New update available. (" + lastver[0] + "." + lastver[1] + "." + lastver[2] + "." + lastver[3] + ")";
                            StatusLabel.Foreground = Brushes.Black;
                        }), System.Windows.Threading.DispatcherPriority.Normal);
                        return;
                    }
                    else if (int.Parse(lastver[0]) < currentversion.Major)
                    {
                        StatusLabel.Dispatcher.Invoke(new Action(delegate()
                        {
                            StatusLabel.Content = "No new update available.";
                            StatusLabel.Foreground = Brushes.Black;
                        }), System.Windows.Threading.DispatcherPriority.Normal);
                        return;
                    }

                    if (int.Parse(lastver[1]) > currentversion.Minor)
                    {
                        StatusLabel.Dispatcher.Invoke(new Action(delegate()
                        {
                            StatusLabel.Content = "New update available. (" + lastver[0] + "." + lastver[1] + "." + lastver[2] + "." + lastver[3] + ")";
                            StatusLabel.Foreground = Brushes.Black;
                        }), System.Windows.Threading.DispatcherPriority.Normal);
                        return;
                    }
                    else if (int.Parse(lastver[1]) < currentversion.Minor)
                    {
                        StatusLabel.Dispatcher.Invoke(new Action(delegate()
                        {
                            StatusLabel.Content = "No new update available.";
                            StatusLabel.Foreground = Brushes.Black;
                        }), System.Windows.Threading.DispatcherPriority.Normal);
                        return;
                    }

                    if (int.Parse(lastver[2]) > currentversion.Build)
                    {
                        StatusLabel.Dispatcher.Invoke(new Action(delegate()
                        {
                            StatusLabel.Content = "New update available. (" + lastver[0] + "." + lastver[1] + "." + lastver[2] + "." + lastver[3] + ")";
                            StatusLabel.Foreground = Brushes.Black;
                        }), System.Windows.Threading.DispatcherPriority.Normal);
                        return;
                    }
                    else if (int.Parse(lastver[2]) < currentversion.Build)
                    {
                        StatusLabel.Dispatcher.Invoke(new Action(delegate()
                        {
                            StatusLabel.Content = "No new update available.";
                            StatusLabel.Foreground = Brushes.Black;
                        }), System.Windows.Threading.DispatcherPriority.Normal);
                        return;
                    }

                    if (int.Parse(lastver[3]) > currentversion.Revision)
                    {
                        StatusLabel.Dispatcher.Invoke(new Action(delegate()
                        {
                            StatusLabel.Content = "New update available. (" + lastver[0] + "." + lastver[1] + "." + lastver[2] + "." + lastver[3] + ")";
                            StatusLabel.Foreground = Brushes.Black;
                        }), System.Windows.Threading.DispatcherPriority.Normal);
                        return;
                    }
                    else if (int.Parse(lastver[3]) < currentversion.Revision)
                    {
                        StatusLabel.Dispatcher.Invoke(new Action(delegate()
                        {
                            StatusLabel.Content = "No new update available.";
                            StatusLabel.Foreground = Brushes.Black;
                        }), System.Windows.Threading.DispatcherPriority.Normal);
                        return;
                    }
                }
                catch (Exception e)
                {
                    StatusLabel.Dispatcher.Invoke(new Action(delegate()
                    {
                        StatusLabel.Content = "Error trying to retrieve last version.";
                        StatusLabel.Foreground = Brushes.Black;
                    }), System.Windows.Threading.DispatcherPriority.Normal);
                }
                StatusLabel.Dispatcher.Invoke(new Action(delegate()
                {
                    StatusLabel.Content = "No new update available.";
                    StatusLabel.Foreground = Brushes.Black;
                }), System.Windows.Threading.DispatcherPriority.Normal);
            }));
            thread.IsBackground = false;
            thread.Start();
        }

        private void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            if (Check_TextBox_Files())
            {
                config.SetValue("General", "OsuFolder", OsuTextBox.Text);
                config.SetValue("General", "SongsFolder", SongsTextBox.Text);
                config.SetValue("General", "APIKey", APIKeyBox.Password.Trim());
                config.Export("config.ini", "=");
                StatusLabel.Content = "Changes have been applied.";
                StatusLabel.Foreground = Brushes.Green;
            }
        }

        private bool Check_TextBox_Files()
        {
            string error_msg = "";
            if (!Directory.Exists(OsuTextBox.Text) || !File.Exists(System.IO.Path.Combine(OsuTextBox.Text, "collection.db")))
                error_msg += "osu!";
            if (!Directory.Exists(SongsTextBox.Text))
                if (error_msg != "")
                    error_msg += " and songs folder are invalid.";
                else
                    error_msg += "Songs folder is invalid.";
            if (error_msg == "osu!")
                error_msg += " folder is invalid.";
            bool downloaded = false;
            string str = "";
            User user_test = null;
            while (!downloaded)
            {
                try
                {
                    WebClient client = new WebClient();
                    str = client.DownloadString("http://osu.ppy.sh/api/get_user?k=" + APIKeyBox.Password.Trim() + "&u=884950&type=id");
                    if (str != "")
                        downloaded = true;
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine(" Error! Retrying...");
                        downloaded = false;
                        continue;
                    }
                }
                catch (WebException e)
                {
                    if (e.Response is HttpWebResponse)
                    {
                        if ((e.Response as HttpWebResponse).StatusCode == HttpStatusCode.Unauthorized)
                            break;
                    }
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(" Connection error! Retrying in 5 seconds");
                    downloaded = false;
                    Thread.Sleep(5000);
                    continue;
                }
                catch (IOException e) { Console.ForegroundColor = ConsoleColor.Red; Console.WriteLine(" Connection interrupted! Retrying in 5 seconds"); downloaded = false; Thread.Sleep(5000); continue; }
                if (str != "")
                    user_test = JsonConvert.DeserializeObject<User>(str.Substring(1, str.Length - 2));
                if (user_test == null)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(" Error! Retrying...");
                    downloaded = false;
                    continue;
                }
            }
            if (!(user_test != null && user_test.user_id == 884950))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(" Error!");
                if (error_msg == "")
                    error_msg = "Error with API key.";
                else
                    error_msg = "And error with API key.";
            }
            if (error_msg != "")
            {
                StatusLabel.Content = error_msg;
                StatusLabel.Foreground = Brushes.Red;
                return false;
            }
            return true;
        }

        private bool Check_Config_Files()
        {
            string error_msg = "";
            string value = config.GetValue("General", "OsuFolder");
            if (value == null || !Directory.Exists(value) || !File.Exists(System.IO.Path.Combine(value, "collection.db")))
                error_msg += "osu!";
            value = config.GetValue("General", "SongsFolder");
            if (!Directory.Exists(value))
                if (error_msg != "")
                    error_msg += " and songs folder are invalid.";
                else
                    error_msg += "Songs folder is invalid.";
            if (error_msg == "osu!")
                error_msg += " folder is invalid.";
            bool downloaded = false;
            string str = "";
            User user_test = null;
            while (!downloaded)
            {
                try
                {
                    WebClient client = new WebClient();
                    str = client.DownloadString("http://osu.ppy.sh/api/get_user?k=" + config.GetValue("General", "APIKey") + "&u=884950&type=id");
                    if (str != "")
                        downloaded = true;
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine(" Error! Retrying...");
                        downloaded = false;
                        continue;
                    }
                }
                catch (WebException e)
                {
                    if (e.Response is HttpWebResponse)
                    {
                        if ((e.Response as HttpWebResponse).StatusCode == HttpStatusCode.Unauthorized)
                            break;
                    }
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(" Connection error! Retrying in 5 seconds");
                    downloaded = false;
                    Thread.Sleep(5000);
                    continue;
                }
                catch (IOException e) { Console.ForegroundColor = ConsoleColor.Red; Console.WriteLine(" Connection interrupted! Retrying in 5 seconds"); downloaded = false; Thread.Sleep(5000); continue; }
                if (str != "")
                    user_test = JsonConvert.DeserializeObject<User>(str.Substring(1, str.Length - 2));
                if (user_test == null)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(" Error! Retrying...");
                    downloaded = false;
                    continue;
                }
            }
            if (!(user_test != null && user_test.user_id == 884950))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(" Error!");
                if (error_msg == "")
                    error_msg = "Error with API key.";
                else
                    error_msg = "And error with API key.";
            }
            if (error_msg != "")
            {
                StatusLabel.Content = error_msg;
                StatusLabel.Foreground = Brushes.Red;
                return false;
            }
            return true;
        }

        private void Window_Initialized(object sender, EventArgs e)
        {
            if (File.Exists("config.ini"))
            {
                config.Load("config.ini", "=");
                string value = config.GetValue("General", "OsuFolder");
                if (value != null)
                    OsuTextBox.Text = value;
                value = config.GetValue("General", "SongsFolder");
                if (value != null)
                    SongsTextBox.Text = value;
                value = config.GetValue("General", "APIKey");
                if (value != null)
                    APIKeyBox.Password = value;
            }
            BeatmapList.DataContext = this;
            beatmaps = new ObservableCollection<Beatmap>();
            BeatmapList.ItemsSource = beatmaps;
        }

        private void BeginButton_Click(object sender, RoutedEventArgs e)
        {
            Beatmap.ClearBeatmapDictionary();
            if (Check_Config_Files())
            {
                GlobalGrid.IsEnabled = false;
                OsuDbFile dbfile = OsuDbFile.TryParse(System.IO.Path.Combine(config.GetValue("General", "OsuFolder"), "osu!.db"));
                if (dbfile == null)
                {
                    StatusLabel.Content = "osu!.db file is corrupted or not compatible.";
                    StatusLabel.Foreground = Brushes.Red;
                    return;
                }
                foreach (Beatmap beatmap in dbfile.beatmaps)
                {
                    beatmap.ispresent = true;
                    Beatmap.AddBeatmap(beatmap);
                }
                if (File.Exists("cache.json"))
                {
                    List<Beatmap> list = JsonConvert.DeserializeObject<List<Beatmap>>(File.ReadAllText("cache.json"));
                    foreach (Beatmap beatmap in list)
                    {
                        beatmap.ispresent = false;
                        Beatmap.AddBeatmap(beatmap);
                    }
                }
                collectionFile = CollectionFile.TryParse(System.IO.Path.Combine(config.GetValue("General", "OsuFolder"), "collection.db"));
                if (collectionFile == null)
                {
                    StatusLabel.Content = "collection.db file is corrupted or not compatible.";
                    StatusLabel.Foreground = Brushes.Red;
                    return;
                }
                RefreshCollectionList();
                ThreadStart ts = new ThreadStart(() =>
                {
                    //else
                    {

                        /*List<string> files = Directory.EnumerateFiles(config.GetValue("General", "SongsFolder"), "*.osu", SearchOption.AllDirectories).ToList<string>();
                        for (int i = 0; i < files.Count; i++)
                        {
                            IniFile beatmapFile = new IniFile();
                            StatusLabel.Dispatcher.Invoke(new Action(delegate()
                            {
                                StatusLabel.Content = "Loading file " + (i + 1) + "/" + files.Count + ".";
                                StatusLabel.Foreground = Brushes.Black;
                            }), System.Windows.Threading.DispatcherPriority.Normal);
                            beatmapFile.Load(files[i], ":");
                            Beatmap beatmap = new Beatmap();
                            beatmap.title = beatmapFile.GetValue("Metadata", "Title");
                            beatmap.artist = beatmapFile.GetValue("Metadata", "Artist");
                            beatmap.creator = beatmapFile.GetValue("Metadata", "Creator");
                            beatmap.version = beatmapFile.GetValue("Metadata", "Version");
                            string value = "";
                            if ((value = beatmapFile.GetValue("Difficulty", "OverallDifficulty")) != null)
                                beatmap.diff_approach = beatmap.diff_overall = float.Parse(value, NumberStyles.Float, CultureInfo.InvariantCulture);
                            if ((value = beatmapFile.GetValue("Difficulty", "ApproachRate")) != null)
                                beatmap.diff_approach = float.Parse(value, NumberStyles.Float, CultureInfo.InvariantCulture);
                            if ((value = beatmapFile.GetValue("Difficulty", "CircleSize")) != null)
                                beatmap.diff_size = float.Parse(value, NumberStyles.Float, CultureInfo.InvariantCulture);
                            if ((value = beatmapFile.GetValue("Difficulty", "HPDrainRate")) != null)
                                beatmap.diff_drain = float.Parse(value, NumberStyles.Float, CultureInfo.InvariantCulture);
                            beatmap.file_md5 = GetMD5HashFromFile(files[i]).ToLower();
                            Beatmap.AddBeatmap(beatmap);
                            beatmapFile.Destroy();
                        }*/
                        StatusLabel.Dispatcher.Invoke(new Action(delegate()
                        {
                            StatusLabel.Content = "Loading completed.";
                            StatusLabel.Foreground = Brushes.Green;
                        }), System.Windows.Threading.DispatcherPriority.Normal);
                    }
                    
                    List<string> missingmaps = new List<string>();
                    foreach(Collection collec in collectionFile.collections)
                    {
                        foreach(string md5 in collec.GetMapList())
                        {
                            if (!missingmaps.Contains(md5) && Beatmap.GetBeatmapFromMD5(md5).title == "")
                                missingmaps.Add(md5);
                        }
                    }
                    for (int i = 0; i < missingmaps.Count; i++)
                    {
                        bool downloaded = false;
                        string str = "";
                        while (!downloaded)
                        {
                            StatusLabel.Dispatcher.Invoke(new Action(delegate()
                            {
                                StatusLabel.Content = "Downloading map info ... (" + (i + 1) + "/" + missingmaps.Count + ")";
                                StatusLabel.Foreground = Brushes.Black;
                            }), System.Windows.Threading.DispatcherPriority.Normal);
                            try
                            {
                                WebClient client = new WebClient();
                                str = client.DownloadString("http://osu.ppy.sh/api/get_beatmaps?k=" + config.GetValue("General", "APIKey") + "&h=" + missingmaps[i]);
                                if (str != "")
                                    downloaded = true;
                                else
                                {
                                    Console.ForegroundColor = ConsoleColor.Red;
                                    Console.WriteLine(" Error! Retrying...");
                                    StatusLabel.Dispatcher.Invoke(new Action(delegate()
                                    {
                                        StatusLabel.Content = "Error! Retrying...";
                                        StatusLabel.Foreground = Brushes.Red;
                                    }), System.Windows.Threading.DispatcherPriority.Normal);
                                    downloaded = false;
                                    continue;
                                }
                            }
                            catch (WebException ex)
                            {
                                if (ex.Response is HttpWebResponse)
                                {
                                    if ((ex.Response as HttpWebResponse).StatusCode == HttpStatusCode.Unauthorized)
                                        break;
                                }
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine(" Connection error! Retrying in 5 seconds");
                                Console.Error.WriteLine(ex.StackTrace);
                                StatusLabel.Dispatcher.Invoke(new Action(delegate()
                                {
                                    StatusLabel.Content = "Connection error! Retrying in 5 seconds.";
                                    StatusLabel.Foreground = Brushes.Red;
                                }), System.Windows.Threading.DispatcherPriority.Normal);
                                downloaded = false;
                                Thread.Sleep(5000);
                                continue;
                            }
                            catch (IOException ex)
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine(" Connection interrupted! Retrying in 5 seconds");
                                Console.Error.WriteLine(ex.StackTrace);
                                StatusLabel.Dispatcher.Invoke(new Action(delegate()
                                {
                                    StatusLabel.Content = "Connection error! Retrying in 5 seconds.";
                                    StatusLabel.Foreground = Brushes.Red;
                                }), System.Windows.Threading.DispatcherPriority.Normal);
                                downloaded = false;
                                Thread.Sleep(5000);
                                continue;
                            }
                        }
                        if (str != "")
                        {
                            if (Beatmap.GetBeatmapDictionary().ContainsKey(missingmaps[i]))
                                Beatmap.GetBeatmapDictionary()[missingmaps[i]] = JsonConvert.DeserializeObject<Beatmap>(str.Substring(1, str.Length - 2));
                            else
                            {
                                Beatmap toadd = JsonConvert.DeserializeObject<Beatmap>(str.Substring(1, str.Length - 2));
                                if (toadd != null)
                                    Beatmap.AddBeatmap(toadd);
                            }
                        }
                        StatusLabel.Dispatcher.Invoke(new Action(delegate()
                        {
                            StatusLabel.Content = "Downloading completed. (" + (i + 1) + "/" + missingmaps.Count + ")";
                            StatusLabel.Foreground = Brushes.Black;
                        }), System.Windows.Threading.DispatcherPriority.Normal);
                        Thread.Sleep(1000);
                    }
                    StatusLabel.Dispatcher.Invoke(new Action(delegate()
                    {
                        StatusLabel.Content = "Operation completed.";
                        StatusLabel.Foreground = Brushes.Black;
                    }), System.Windows.Threading.DispatcherPriority.Normal);
                    WriteBeatmapCache();
                    GlobalGrid.Dispatcher.Invoke(new Action(delegate()
                    {
                        GlobalGrid.IsEnabled = true;
                    }));
                });
                Thread thread = new Thread(ts);
                thread.IsBackground = false;
                thread.Start();
            }
        }

        public void WriteBeatmapCache()
        {
            if (File.Exists("cache.json"))
                File.Delete("cache.json");
            List<Beatmap> toexport = new List<Beatmap>();
            foreach (Beatmap beatmap in Beatmap.GetBeatmapDictionary().Values.ToList())
            {
                if (beatmap != null && !beatmap.ispresent)
                    toexport.Add(beatmap);
            }
            File.WriteAllText("cache.json", JsonConvert.SerializeObject(toexport));
        }

        public void RefreshCollectionList()
        {
            CollectionList.Items.Clear();
            foreach (Collection collection in collectionFile.collections)
            {
                CollectionList.Items.Add(collection.GetName());
            }
        }

        private void CollectionList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RefreshBeatmapList();
        }

        private void RefreshBeatmapList()
        {
            beatmaps.Clear();
            if (CollectionList.SelectedItem == null)
                return;
            Collection collection = GetCollection((string)CollectionList.SelectedValue);
            if (collection == null)
                return;
            foreach (string md5 in collection.GetMapList())
            {
                Beatmap map = Beatmap.GetBeatmapFromMD5(md5);
                beatmaps.Add(map);
            }
        }

        private Collection GetCollection(string name)
        {
            foreach (Collection collec in collectionFile.collections)
            {
                if (collec.GetName() == name)
                {
                    return (collec);
                }
            }
            return null;
        }

        private void AddBeatmapItem_Click(object sender, RoutedEventArgs e)
        {
            bool valid = true;
            while (true)
            {
                BeatmapInput dialog = new BeatmapInput(valid);
                if (dialog.ShowDialog() == true)
                {
                    if (dialog.Answer.ToLower().StartsWith("http://osu.ppy.sh/b/") || dialog.Answer.ToLower().StartsWith("https://osu.ppy.sh/b/"))
                    {
                        Uri uri = new Uri(dialog.Answer);
                        if (uri.Segments.Length != 3)
                            continue;
                        int beatmapid = int.Parse(uri.Segments[2].Split('&')[0]);
                        bool downloaded = false;
                        string str = "";
                        while (!downloaded)
                        try
                        {
                            WebClient client = new WebClient();
                            str = client.DownloadString("http://osu.ppy.sh/api/get_beatmaps?k=" + config.GetValue("General", "APIKey") + "&b=" + beatmapid);
                            if (str != "")
                                downloaded = true;
                            else
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.Error.WriteLine(" Error! Retrying...");
                                downloaded = false;
                                continue;
                            }
                        }
                        catch (WebException ex)
                        {
                            if (ex.Response is HttpWebResponse)
                            {
                                if ((ex.Response as HttpWebResponse).StatusCode == HttpStatusCode.Unauthorized)
                                    break;
                            }
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine(" Connection error! Retrying in 5 seconds");
                            Console.Error.WriteLine(ex.StackTrace);
                            downloaded = false;
                            Thread.Sleep(5000);
                            continue;
                        }
                        catch (IOException ex)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine(" Connection interrupted! Retrying in 5 seconds");
                            Console.Error.WriteLine(ex.StackTrace);
                            downloaded = false;
                            Thread.Sleep(5000);
                            continue;
                        }
                        Beatmap[] beatmaps_downloaded = JsonConvert.DeserializeObject<Beatmap[]>(str);
                        foreach(Beatmap beatmap in beatmaps_downloaded)
                            Beatmap.AddBeatmap(beatmap);
                        WriteBeatmapCache();
                        if (beatmaps_downloaded.Length == 0)
                            continue;
                        valid = true;
                        BeatmapSelect beatmapselectdialog = new BeatmapSelect(beatmaps_downloaded);
                        beatmapselectdialog.ShowDialog();
                        Collection collection = GetCollection((string)CollectionList.SelectedValue);
                        foreach (Beatmap beatmap in beatmapselectdialog.Answer)
                        {
                            beatmap.ischecked = false;
                            collection.AddMap(beatmap);
                        }
                        RefreshBeatmapList();
                        break;
                    }
                    else if (dialog.Answer.ToLower().StartsWith("http://osu.ppy.sh/s/") || dialog.Answer.ToLower().StartsWith("https://osu.ppy.sh/s/"))
                    {
                        Uri uri = new Uri(dialog.Answer);
                        if (uri.Segments.Length != 3)
                            continue;
                        int beatmapid = int.Parse(uri.Segments[2].Split('&')[0]);
                        bool downloaded = false;
                        string str = "";
                        while (!downloaded)
                            try
                            {
                                WebClient client = new WebClient();
                                str = client.DownloadString("http://osu.ppy.sh/api/get_beatmaps?k=" + config.GetValue("General", "APIKey") + "&s=" + beatmapid);
                                if (str != "")
                                    downloaded = true;
                                else
                                {
                                    Console.ForegroundColor = ConsoleColor.Red;
                                    Console.Error.WriteLine(" Error! Retrying...");
                                    downloaded = false;
                                    continue;
                                }
                            }
                            catch (WebException ex)
                            {
                                if (ex.Response is HttpWebResponse)
                                {
                                    if ((ex.Response as HttpWebResponse).StatusCode == HttpStatusCode.Unauthorized)
                                        break;
                                }
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine(" Connection error! Retrying in 5 seconds");
                                Console.Error.WriteLine(ex.StackTrace);
                                downloaded = false;
                                Thread.Sleep(5000);
                                continue;
                            }
                            catch (IOException ex)
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine(" Connection interrupted! Retrying in 5 seconds");
                                Console.Error.WriteLine(ex.StackTrace);
                                downloaded = false;
                                Thread.Sleep(5000);
                                continue;
                            }
                        Beatmap[] beatmaps_downloaded = JsonConvert.DeserializeObject<Beatmap[]>(str);
                        foreach (Beatmap beatmap in beatmaps_downloaded)
                            Beatmap.AddBeatmap(beatmap);
                        WriteBeatmapCache();
                        if (beatmaps_downloaded.Length == 0)
                            continue;
                        valid = true;
                        BeatmapSelect beatmapselectdialog = new BeatmapSelect(beatmaps_downloaded);
                        beatmapselectdialog.ShowDialog();
                        Collection collection = GetCollection((string)CollectionList.SelectedValue);
                        foreach (Beatmap beatmap in beatmapselectdialog.Answer)
                        {
                            beatmap.ischecked = false;
                            collection.AddMap(beatmap);
                        }
                        RefreshBeatmapList();
                        break;
                    }
                    valid = false;
                }
                else
                    break;
            }
        }

        private void DeleteBeatmapItem_Click(object sender, RoutedEventArgs e)
        {
            if (BeatmapList.SelectedItem == null)
                return;
            MessageBoxResult result = MessageBox.Show(this, "Are you sure you want to delete this beatmap?", "Warning", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                string md5 = ((Beatmap)BeatmapList.SelectedItem).file_md5;
                Collection collection = GetCollection((string)CollectionList.SelectedValue);
                if (collection == null)
                    return;
                collection.RemoveMap(md5);
                RefreshBeatmapList();
            }
        }

        protected string GetMD5HashFromFile(string fileName)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(fileName))
                {
                    return BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", string.Empty);
                }
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            Thread thread = new Thread(new ThreadStart(delegate()
            {
                GlobalGrid.Dispatcher.Invoke(delegate() { GlobalGrid.IsEnabled = false; });
                try
                {
                    collectionFile.Export(collectionFile.path);
                    StatusLabel.Dispatcher.Invoke(delegate()
                    {
                        StatusLabel.Content = "Save complete!";
                        StatusLabel.Foreground = Brushes.Green;
                    }); 
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(ex.StackTrace);
                    StatusLabel.Dispatcher.Invoke(delegate()
                    {
                        StatusLabel.Content = "Error saving the file!";
                        StatusLabel.Foreground = Brushes.Red;
                    });
                }
                GlobalGrid.Dispatcher.Invoke(delegate() { GlobalGrid.IsEnabled = true; });
            }));
            thread.IsBackground = false;
            thread.Start();
        }

        private void SaveAs_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = "collection File|*.db";
            dialog.Title = "Save File";
            dialog.ShowDialog();
            if (dialog.FileName == "")
                return;
            Thread thread = new Thread(new ThreadStart(delegate()
            {
                GlobalGrid.Dispatcher.Invoke(delegate() { GlobalGrid.IsEnabled = false; });
                try
                {
                    collectionFile.Export(dialog.FileName);
                    StatusLabel.Dispatcher.Invoke(delegate()
                    {
                        StatusLabel.Content = "Save complete!";
                        StatusLabel.Foreground = Brushes.Green;
                    });
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(ex.StackTrace);
                    StatusLabel.Dispatcher.Invoke(delegate()
                    {
                        StatusLabel.Content = "Error saving the file!";
                        StatusLabel.Foreground = Brushes.Red;
                    });
                }
                GlobalGrid.Dispatcher.Invoke(delegate() { GlobalGrid.IsEnabled = true; });
            }));
            thread.IsBackground = false;
            thread.Start();
        }

        private void File_SubmenuOpened(object sender, RoutedEventArgs e)
        {
            SaveMenuItem.IsEnabled = !(collectionFile == null);
            SaveAsMenuItem.IsEnabled = !(collectionFile == null);
        }

        private void DeleteCollectionItem_Click(object sender, RoutedEventArgs e)
        {
            if (CollectionList.SelectedIndex == -1)
                return;
            MessageBoxResult result = MessageBox.Show(this, "Are you sure you want to delete this collection?", "Warning", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                Collection collection = GetCollection((string)CollectionList.SelectedItem);
                if (collection != null)
                    collectionFile.collections.Remove(collection);
                StatusLabel.Content = "Collection deleted!";
                StatusLabel.Foreground = Brushes.Green;
                RefreshCollectionList();
            }
        }

        private void AddCollectionItem_Click(object sender, RoutedEventArgs e)
        {
            InputDialog input = new InputDialog("Insert the name of the new collection:", "");
            if (input.ShowDialog() == true)
            {
                if (GetCollection(input.Answer) != null)
                {
                    StatusLabel.Content = "Collection with the same name already exists!";
                    StatusLabel.Foreground = Brushes.Red;
                    return;
                }
                collectionFile.collections.Add(new Collection(input.Answer));
                StatusLabel.Content = "Collection created!";
                StatusLabel.Foreground = Brushes.Green;
                RefreshCollectionList();
            }
        }

        private void RenameCollectionItem_Click(object sender, RoutedEventArgs e)
        {
            if (CollectionList.SelectedIndex == -1)
                return;
            InputDialog input = new InputDialog("Insert the new name of the collection:", (string)CollectionList.SelectedItem);
            if (input.ShowDialog() == true)
            {
                Collection collection = GetCollection((string)CollectionList.SelectedItem);
                if (collection == null)
                {
                    StatusLabel.Content = "No collection selected!";
                    StatusLabel.Foreground = Brushes.Red;
                    return;
                }
                if (GetCollection(input.Answer) != null)
                {
                    StatusLabel.Content = "Collection with the same name already exists!";
                    StatusLabel.Foreground = Brushes.Red;
                    return;
                }
                collection.SetName(input.Answer);
                StatusLabel.Content = "Collection name set!";
                StatusLabel.Foreground = Brushes.Green;
                RefreshCollectionList();
            }
        }

        private void CollectionList_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            AddCollectionItem.IsEnabled = collectionFile != null;
            DeleteCollectionItem.IsEnabled = CollectionList.SelectedIndex != -1;
            RenameCollectionItem.IsEnabled = CollectionList.SelectedIndex != -1;
        }

        private void DownloadButton_Click(object sender, RoutedEventArgs e)
        {
            Thread thread = new Thread(new ThreadStart(delegate()
            {
                GlobalGrid.Dispatcher.Invoke(delegate() { GlobalGrid.IsEnabled = false; });
                StatusLabel.Dispatcher.Invoke(delegate() { StatusLabel.Content = "Beggining downloading missing beatmaps..."; StatusLabel.Foreground = Brushes.Black; });
                List<Beatmap> missingbeatmaps = new List<Beatmap>();
                foreach (Beatmap beatmap in Beatmap.GetBeatmapDictionary().Values)
                {
                    if (beatmap.ispresent)
                        continue;
                    bool add = true;
                    foreach (Beatmap missingbeatmap in missingbeatmaps)
                        if (missingbeatmap.beatmapset_id == beatmap.beatmapset_id)
                        {
                            add = false;
                            break;
                        }
                    if (add)
                        missingbeatmaps.Add(beatmap);
                }
                if (!Directory.Exists("./downloaded"))
                    Directory.CreateDirectory("./downloaded");
                else
                {
                    foreach (string file in Directory.EnumerateFiles("./downloaded"))
                    {
                        File.Delete(file);
                    }
                }
                for (int i = 0; i < missingbeatmaps.Count; i++)
                {
                    if (File.Exists(config.GetValue("General", "SongsFolder") + "\\" + missingbeatmaps[i].beatmapset_id + " - " + missingbeatmaps[i].artist + " - " + missingbeatmaps[i].title + ".osz"))
                        continue;
                    StatusLabel.Dispatcher.Invoke(delegate() { StatusLabel.Content = "Downloading map... (" + (i + 1) + "/" + missingbeatmaps.Count + ")"; StatusLabel.Foreground = Brushes.Black; });
                    bool downloaded = false;
                    while (!downloaded)
                    {
                        try
                        {
                            WebClient client = new WebClient();
                            client.DownloadFile("http://bloodcat.com/osu/s/" + missingbeatmaps[i].beatmapset_id, "./downloaded/" + missingbeatmaps[i].beatmapset_id + " - " + missingbeatmaps[i].artist + " - " + missingbeatmaps[i].title + ".osz");
                            downloaded = true;
                        }
                        catch (Exception ex) { downloaded = false; Console.Error.WriteLine(ex.StackTrace); }
                    }
                }
                StatusLabel.Dispatcher.Invoke(delegate() { StatusLabel.Content = "Moving files..."; StatusLabel.Foreground = Brushes.Black; });
                foreach (string file in Directory.EnumerateFiles("./downloaded"))
                {
                    File.Move(file, config.GetValue("General", "SongsFolder") + "\\" + System.IO.Path.GetFileName(file));
                }
                StatusLabel.Dispatcher.Invoke(delegate() { StatusLabel.Content = "Operation completed."; StatusLabel.Foreground = Brushes.Green; });
                GlobalGrid.Dispatcher.Invoke(delegate() { GlobalGrid.IsEnabled = true; });
            }));
            thread.IsBackground = false;
            thread.Start();
        }

        private void BeatmapList_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            AddBeatmapItem.IsEnabled = CollectionList.SelectedIndex != -1;
            DeleteBeatmapItem.IsEnabled = CollectionList.SelectedIndex != -1 && BeatmapList.SelectedIndex != -1;
        }
    }
}
