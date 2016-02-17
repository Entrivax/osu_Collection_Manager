using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu_CollectionManager
{
    class OsuDbFile
    {
        int version;
        public List<Beatmap> beatmaps;

        /// <summary>
        /// Try to parse osu.db file
        /// </summary>
        /// <param name="file">Path to the file</param>
        /// <returns>returns OsuDbFile object; returns null if error</returns>
        public static OsuDbFile TryParse(string file)
        {
            OsuDbFile dbfile = null;
            BinaryReader br = null;
            try
            {
                dbfile = new OsuDbFile();
                dbfile.beatmaps = new List<Beatmap>();
                byte[] bytes = File.ReadAllBytes(file);
                br = new BinaryReader(new MemoryStream(bytes));
                dbfile.version = br.ReadInt32(); // osu! version (e.g. 20150203)
                br.ReadInt32(); // Folder Count
                br.ReadBoolean(); // AccountUnlocked (only false when the account is locked or banned in any way)
                br.ReadInt64(); // Date the account will be unlocked
                byte b1 = br.ReadByte(); // byte before string
                if (b1 == 0x0b) // String valid
                    br.ReadString(); // Player name
                else if (b1 != 0x00) // 0x00 : String info not present ; Anything else : corrupted
                {
                    br.Close();
                    return null;
                }
                int beatmapCount = br.ReadInt32(); // Number of beatmaps
                for(int i = 0; i < beatmapCount; i++) // For each beatmap
                {
                    Beatmap beatmap = new Beatmap();
                    b1 = br.ReadByte(); // byte before string
                    if (b1 == 0x0b) // String valid
                        beatmap.artist = br.ReadString(); // Artist name
                    else if (b1 != 0x00) // 0x00 : String info not present ; Anything else : corrupted
                    {
                        br.Close();
                        return null;
                    }

                    b1 = br.ReadByte(); // byte before string
                    if (b1 == 0x0b) // String valid
                        br.ReadString(); // Artist name, in Unicode
                    else if (b1 != 0x00) // 0x00 : String info not present ; Anything else : corrupted
                    {
                        br.Close();
                        return null;
                    }

                    b1 = br.ReadByte(); // byte before string
                    if (b1 == 0x0b) // String valid
                        beatmap.title = br.ReadString(); // Song title
                    else if (b1 != 0x00) // 0x00 : String info not present ; Anything else : corrupted
                    {
                        br.Close();
                        return null;
                    }

                    b1 = br.ReadByte(); // byte before string
                    if (b1 == 0x0b) // String valid
                        br.ReadString(); // Song title, in Unicode
                    else if (b1 != 0x00) // 0x00 : String info not present ; Anything else : corrupted
                    {
                        br.Close();
                        return null;
                    }

                    b1 = br.ReadByte(); // byte before string
                    if (b1 == 0x0b) // String valid
                        beatmap.creator = br.ReadString(); // Creator name
                    else if (b1 != 0x00) // 0x00 : String info not present ; Anything else : corrupted
                    {
                        br.Close();
                        return null;
                    }

                    b1 = br.ReadByte(); // byte before string
                    if (b1 == 0x0b) // String valid
                        beatmap.version = br.ReadString(); // Difficulty (e.g. Hard, Insane, etc.)
                    else if (b1 != 0x00) // 0x00 : String info not present ; Anything else : corrupted
                    {
                        br.Close();
                        return null;
                    }

                    b1 = br.ReadByte(); // byte before string
                    if (b1 == 0x0b) // String valid
                        br.ReadString(); // Audio file name
                    else if (b1 != 0x00) // 0x00 : String info not present ; Anything else : corrupted
                    {
                        br.Close();
                        return null;
                    }

                    b1 = br.ReadByte(); // byte before string
                    if (b1 == 0x0b) // String valid
                        beatmap.file_md5 = br.ReadString(); // MD5 hash of the beatmap
                    else if (b1 != 0x00) // 0x00 : String info not present ; Anything else : corrupted
                    {
                        br.Close();
                        return null;
                    }

                    b1 = br.ReadByte(); // byte before string
                    if (b1 == 0x0b) // String valid
                        br.ReadString(); // Name of the .osu file corresponding to this beatmap
                    else if (b1 != 0x00) // 0x00 : String info not present ; Anything else : corrupted
                    {
                        br.Close();
                        return null;
                    }

                    br.ReadByte(); // Ranked status (4 = ranked, 5 = approved, 2 = pending/graveyard)
                    br.ReadInt16(); // Number of hitcircles
                    br.ReadInt16(); // Number of sliders (note: this will be present in every mode)
                    br.ReadInt16(); // Number of spinners (note: this will be present in every mode)
                    br.ReadInt64(); // Last modification time, Windows ticks.
                    if (dbfile.version < 20140609) // Approach rate. Byte if the version is less than 20140609, Single otherwise.
                        beatmap.diff_approach = br.ReadByte();
                    else
                        beatmap.diff_approach = br.ReadSingle();
                    if (dbfile.version < 20140609) // Circle size. Byte if the version is less than 20140609, Single otherwise.
                        beatmap.diff_size = br.ReadByte();
                    else
                        beatmap.diff_size = br.ReadSingle();
                    if (dbfile.version < 20140609) // HP drain. Byte if the version is less than 20140609, Single otherwise.
                        beatmap.diff_drain = br.ReadByte();
                    else
                        beatmap.diff_drain = br.ReadSingle();
                    if (dbfile.version < 20140609) // Overall difficulty. Byte if the version is less than 20140609, Single otherwise.
                        beatmap.diff_overall = br.ReadByte();
                    else
                        beatmap.diff_overall = br.ReadSingle();
                    br.ReadDouble(); // Slider velocity
                    if (dbfile.version >= 20140609)
                    {
                        for (int k = 0; k < 4; k++)
                        {
                            int count = br.ReadInt32(); // An Int indicating the number of following Int-Double pairs, then the aforementioned pairs. Purpose unknown. Only present if version is greater than or equal to 20140609.
                            br.ReadBytes(14 * count);
                        }
                    }
                    beatmap.hit_length = br.ReadInt32(); // Drain time, in seconds
                    beatmap.total_length = br.ReadInt32() / 1000f; // Total time, in milliseconds / 1000
                    br.ReadInt32(); // Time when the audio preview when hovering over a beatmap in beatmap select starts, in milliseconds.
                    int timingpointcount = br.ReadInt32(); // An Int indicating the number of following Timing points, then the aforementioned Timing points.
                    for (int k = 0; k < timingpointcount; k++) // Consists of a Double, signifying the BPM, another Double, signifying the offset into the song, in milliseconds, and a Boolean; if false, then this timing point is inherited. See Osu (file format) for more information regarding timing points.
                    {
                        if (k == 0)
                            beatmap.bpm = br.ReadDouble(); // BPM
                        else
                            br.ReadDouble(); // BPM
                        br.ReadDouble(); // Offset, in milliseconds
                        br.ReadBoolean(); // False : Is inherited
                    }
                    beatmap.beatmap_id = br.ReadInt32(); // Beatmap ID
                    beatmap.beatmapset_id = br.ReadInt32(); // Beatmap set ID
                    br.ReadInt32(); // Thread ID
                    br.ReadBytes(4); // Unknown
                    br.ReadInt16(); // Local beatmap offset
                    br.ReadSingle(); // Stack leniency
                    beatmap.mode = br.ReadByte(); // Osu gameplay mode. 0x00 = osu!Standard, 0x01 = Taiko, 0x02 = CTB, 0x03 = Mania

                    b1 = br.ReadByte(); // byte before string
                    if (b1 == 0x0b) // String valid
                        br.ReadString(); // Song source
                    else if (b1 != 0x00) // 0x00 : String info not present ; Anything else : corrupted
                    {
                        br.Close();
                        return null;
                    }

                    b1 = br.ReadByte(); // byte before string
                    if (b1 == 0x0b) // String valid
                        br.ReadString(); // Song tags
                    else if (b1 != 0x00) // 0x00 : String info not present ; Anything else : corrupted
                    {
                        br.Close();
                        return null;
                    }

                    br.ReadInt16(); // Online offset

                    b1 = br.ReadByte(); // byte before string
                    if (b1 == 0x0b) // String valid
                        br.ReadString(); // Font used for the title of the song
                    else if (b1 != 0x00) // 0x00 : String info not present ; Anything else : corrupted
                    {
                        br.Close();
                        return null;
                    }

                    br.ReadBoolean(); // Is beatmap unplayed
                    br.ReadInt64(); // Last time when beatmap was played
                    br.ReadBoolean(); // Is the beatmap osz2

                    b1 = br.ReadByte(); // byte before string
                    if (b1 == 0x0b) // String valid
                        br.ReadString(); // Folder name of the beatmap, relative to Songs folder
                    else if (b1 != 0x00) // 0x00 : String info not present ; Anything else : corrupted
                    {
                        br.Close();
                        return null;
                    }

                    br.ReadInt64(); // Last time when beatmap was checked against osu! repository
                    br.ReadBoolean(); // Ignore beatmap sounds
                    br.ReadBoolean(); // Ignore beatmap skin
                    br.ReadBoolean(); // Disable storyboard
                    br.ReadBoolean(); // Disable video
                    br.ReadBoolean(); // Visual override
                    if (dbfile.version < 20140609)
                        br.ReadInt16(); // Unknown. Only present if version is less than 20140609.
                    br.ReadInt32(); // Last modification time (?)
                    br.ReadByte(); // Mania scroll speed
                    dbfile.beatmaps.Add(beatmap);
                }
                br.ReadInt32(); // Unknown Int, always seems to be 4
            }
            catch (Exception ex) { Console.Error.WriteLine(ex.StackTrace); dbfile = null; }
            finally
            {
                if (br != null)
                {
                    br.Close();
                }
            }

            return dbfile;
        }
    }
}
