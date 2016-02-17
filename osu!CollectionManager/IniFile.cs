using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu_CollectionManager
{
    class IniFile
    {
        private Dictionary<string, Dictionary<string, string>> sections;
        private string content;
        public void Load(string file, string separator)
        {
            sections = new Dictionary<string, Dictionary<string, string>>();
            content = File.ReadAllText(file);
            List<string> lines = content.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries).ToList<string>();
            string currentsection = "";
            for (int i = 0; i < lines.Count; i++)
            {
                if (lines[i].StartsWith("[") && lines[i].EndsWith("]"))
                {
                    currentsection = lines[i].Substring(1, lines[i].Length - 2);
                    if (!sections.ContainsKey(currentsection))
                        sections.Add(currentsection, new Dictionary<string, string>());
                    continue;
                }
                if (currentsection != "" && sections.ContainsKey(currentsection) && lines[i].Contains(separator))
                {
                    string[] parts = lines[i].Split(new string[] { separator }, 2, StringSplitOptions.None);
                    if (!sections[currentsection].ContainsKey(parts[0]))
                        sections[currentsection].Add(parts[0].Trim(), parts[1].Trim());
                    else
                        sections[currentsection][parts[0].Trim()] = parts[1].Trim();
                }
            }
        }

        public void Export(string file, string separator)
        {
            if (File.Exists(file))
                File.Delete(file);
            StreamWriter sw = new StreamWriter(File.OpenWrite(file));
            foreach (string section in sections.Keys)
            {
                sw.WriteLine("[" + section + "]");
                sw.Flush();
                foreach(KeyValuePair<string, string> keyvalue in sections[section])
                {
                    sw.WriteLine(keyvalue.Key + separator + keyvalue.Value);
                    sw.Flush();
                }
            }
            sw.Close();
        }

        public IniFile()
        {
            sections = new Dictionary<string, Dictionary<string, string>>();
            content = "";
        }

        public void Destroy()
        {
            content = null;
            foreach(KeyValuePair<string, Dictionary<string, string>> section in sections)
                section.Value.Clear();
            sections.Clear();
            sections = null;
        }

        public string GetFileContent()
        {
            return (content);
        }

        public void SetValue(string section, string key, string value)
        {
            if (sections.ContainsKey(section))
                if (sections[section].ContainsKey(key))
                    sections[section][key] = value;
                else
                    sections[section].Add(key, value);
            else
            {
                sections.Add(section, new Dictionary<string, string>());
                sections[section].Add(key, value);
            }
        }

        public string GetValue(string section, string key)
        {
            if (sections.ContainsKey(section))
                if (sections[section].ContainsKey(key))
                    return (sections[section][key]);
            return (null);
        }
    }
}
