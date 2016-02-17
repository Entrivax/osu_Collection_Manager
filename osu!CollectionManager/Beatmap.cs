using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace osu_CollectionManager
{
    public class Beatmap : INotifyPropertyChanged
    {
		private static Dictionary<string, Beatmap> Beatmaps = new Dictionary<string,Beatmap>();

        private int Approved;
        private string Approved_date;
        private string Last_update;
        private string Artist;
        private int Beatmap_id;
        private int Beatmapset_id;
        private double Bpm;
        private string Creator;
        private float Difficultyrating;
        private float Diff_size;
        private float Diff_overall;
        private float Diff_approach;
        private float Diff_drain;
        private float Hit_length;
        private string Source;
        private int Genre_id;
        private int Language_id;
        private string Title;
        private float Total_length;
        private string Version;
        private string File_md5;
        private int Mode;
        private string Tags;
        private int Favourite_count;
        private int Playcount;
        private int Passcount;
        private int Max_combo;
        private bool IsPresent;

        private bool IsChecked;

        public int approved { get { return Approved; } set { Approved = value; OnPropertyChanged("approved"); } }
        public string approved_date { get { return Approved_date; } set { Approved_date = value; OnPropertyChanged("approved_date"); } }
        public string last_update { get { return Last_update; } set { Last_update = value; OnPropertyChanged("last_update"); } }
        public string artist { get { return Artist; } set { Artist = value; OnPropertyChanged("artist"); } }
        public int beatmap_id { get { return Beatmap_id; } set { Beatmap_id = value; OnPropertyChanged("beatmap_id"); } }
        public int beatmapset_id { get { return Beatmapset_id; } set { Beatmapset_id = value; OnPropertyChanged("beatmapset_id"); } }
        public double bpm { get { return Bpm; } set { Bpm = value; OnPropertyChanged("bpm"); } }
        public string creator { get { return Creator; } set { Creator = value; OnPropertyChanged("creator"); } }
        public float difficultyrating { get { return Difficultyrating; } set { Difficultyrating = value; OnPropertyChanged("difficultyrating"); } }
        public float diff_size { get { return Diff_size; } set { Diff_size = value; OnPropertyChanged("diff_size"); } }
        public float diff_overall { get { return Diff_overall; } set { Diff_overall = value; OnPropertyChanged("diff_overall"); } }
        public float diff_approach { get { return Diff_approach; } set { Diff_approach = value; OnPropertyChanged("diff_approach"); } }
        public float diff_drain { get { return Diff_drain; } set { Diff_drain = value; OnPropertyChanged("diff_drain"); } }
        public float hit_length { get { return Hit_length; } set { Hit_length = value; OnPropertyChanged("hit_length"); } }
        public string source { get { return Source; } set { Source = value; OnPropertyChanged("source"); } }
        public int genre_id { get { return Genre_id; } set { Genre_id = value; OnPropertyChanged("genre_id"); } }
        public int language_id { get { return Language_id; } set { Language_id = value; OnPropertyChanged("language_id"); } }
        public string title { get { return Title; } set { Title = value; OnPropertyChanged("title"); } }
        public float total_length { get { return Total_length; } set { Total_length = value; OnPropertyChanged("total_length"); } }
        public string version { get { return Version; } set { Version = value; OnPropertyChanged("version"); } }
        public string file_md5 { get { return File_md5; } set { File_md5 = value; OnPropertyChanged("file_md5"); } }
        public int mode { get { return Mode; } set { Mode = value; OnPropertyChanged("mode"); } }
        public string tags { get { return Tags; } set { Tags = value; OnPropertyChanged("tags"); } }
        public int favourite_count { get { return Favourite_count; } set { Favourite_count = value; OnPropertyChanged("favourite_count"); } }
        public int playcount { get { return Playcount; } set { Playcount = value; OnPropertyChanged("playcount"); } }
        public int passcount { get { return Passcount; } set { Passcount = value; OnPropertyChanged("passcount"); } }
        public int? max_combo { get { return Max_combo; } set { if (value == null) Max_combo = 0; else Max_combo = (int)value; OnPropertyChanged("max_combo"); } }

        [JsonIgnore]
        public bool ischecked { get { return IsChecked; } set { IsChecked = value; OnPropertyChanged("ischecked"); } }
        [JsonIgnore]
        public bool ispresent { get { return IsPresent; } set { IsPresent = value; OnPropertyChanged("ispresent"); OnPropertyChanged("Fcolor"); } }
        [JsonIgnore]
        public Brush Fcolor { get { return ispresent ? Brushes.Black : Brushes.Red; } }

		public Beatmap()
		{
			approved_date = last_update = artist = creator = source = title = version = file_md5 = tags = "";
			approved = beatmap_id = beatmapset_id = genre_id = language_id = mode = favourite_count = playcount = passcount = 0;
            max_combo = 0;
            bpm = difficultyrating = diff_size = diff_overall = diff_approach = diff_drain = hit_length = total_length = 0f;
            ispresent = false;
		}

		public static void AddBeatmap(Beatmap map)
		{
            if (Beatmaps.ContainsKey(map.file_md5))
                return;
			Beatmaps.Add(map.file_md5, map);
		}

        public static void ClearBeatmapDictionary()
        {
            Beatmaps.Clear();
        }

        public static void SetBeatmapDictionary(Dictionary<string, Beatmap> dictionary)
        {
            Beatmaps = dictionary;
        }

		public static Dictionary<string, Beatmap> GetBeatmapDictionary()
		{
			return Beatmaps;
		}

		public static Beatmap GetBeatmapWithID(int beatmap_id)
		{
			foreach (Beatmap map in Beatmaps.Values)
                if (map.beatmap_id == beatmap_id)
					return map;
			return null;
		}

        public static List<Beatmap> GetBeatmapSetWithID(int beatmapset_id)
        {
            List<Beatmap> beatmaps = new List<Beatmap>();
            foreach (Beatmap map in Beatmaps.Values)
                if (map.beatmapset_id == beatmapset_id)
                    beatmaps.Add(map);
            return beatmaps;
        }

		public static Beatmap GetBeatmapFromMD5(string md5)
		{
			if (Beatmaps.ContainsKey(md5))
				return Beatmaps[md5];
			Beatmap map = new Beatmap();
			map.file_md5 = md5;
			return map;
		}

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }
    }
}
