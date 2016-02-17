using System;
using System.Collections.Generic;

namespace osu_CollectionManager
{
    class Collection
    {
		private List<string> maps;
		private string name;

        public Collection(string name)
		{
			maps = new List<string>();
			this.name = name;
		}

		public string GetName()
		{
			return (name);
		}

        public void SetName(string name)
        {
            this.name = name;
        }

		public void AddMap(Beatmap map)
		{
            if (!maps.Contains(map.file_md5))
			    maps.Add(map.file_md5);
		}

		public List<string> GetMapList()
		{
			return (maps);
		}

        public void RemoveMap(string md5)
        {
            if (maps.Contains(md5))
                maps.Remove(md5);
        }
	}
}
