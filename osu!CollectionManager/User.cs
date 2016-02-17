using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu_CollectionManager
{

    public class Event
    {
        public string display_html { get; set; }
        public int beatmap_id { get; set; }
        public int beatmapset_id { get; set; }
        public DateTime date { get; set; }
        public int epicfactor { get; set; }
    }

    public class User
    {
        public int user_id { get; set; }
        public string username { get; set; }
        public int count300 { get; set; }
        public int count100 { get; set; }
        public int count50 { get; set; }
        public int playcount { get; set; }
        public long ranked_score { get; set; }
        public long total_score { get; set; }
        public int pp_rank { get; set; }
        public float level { get; set; }
        public float pp_raw { get; set; }
        public float accuracy { get; set; }
        public int count_rank_ss { get; set; }
        public int count_rank_s { get; set; }
        public int count_rank_a { get; set; }
        public string country { get; set; }
        public int pp_country_rank { get; set; }
        public List<Event> events { get; set; }
    }

}
