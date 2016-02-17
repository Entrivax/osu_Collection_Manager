using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Shapes;

namespace osu_CollectionManager
{
    /// <summary>
    /// Logique d'interaction pour BeatmapInput.xaml
    /// </summary>
    public partial class BeatmapSelect : Window
    {
        ObservableCollection<Beatmap> list { get; set; }
        public BeatmapSelect(Beatmap[] beatmaplist)
        {
            InitializeComponent();
            BeatmapList.DataContext = this;
            list = new ObservableCollection<Beatmap>();
            foreach (Beatmap beatmap in beatmaplist)
                list.Add(beatmap);
            BeatmapList.ItemsSource = list;
            this.SizeToContent = SizeToContent.WidthAndHeight;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        public Beatmap[] Answer
        {
            get
            {
                List<Beatmap> beatmaps = new List<Beatmap>();
                foreach(Beatmap beatmap in BeatmapList.Items)
                {
                    if (beatmap.ischecked)
                        beatmaps.Add(beatmap);
                }
                return beatmaps.ToArray();
            }
        }
    }
}
