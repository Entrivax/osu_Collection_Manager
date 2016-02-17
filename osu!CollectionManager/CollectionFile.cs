using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace osu_CollectionManager
{
	class CollectionFile
	{
		public List<Collection> collections;
		public int version;
        public string path;

		public static CollectionFile TryParse(string file)
        {
            CollectionFile collectionFile = new CollectionFile();
            BinaryReader br = null;
            try
            {
                collectionFile.path = file;
                br = new BinaryReader(File.OpenRead(collectionFile.path));
                int collectionCount = 0;
                collectionFile.version = br.ReadInt32();
                collectionCount = br.ReadInt32();
                for (int i = 0; i < collectionCount; i++)
                {
                    byte b1 = br.ReadByte();
                    if (b1 == 0x0b)
                    {
                        Collection collection = new Collection(br.ReadString());
                        int collectionLength = br.ReadInt32();
                        for (int j = 0; j < collectionLength; j++)
                        {
                            byte b2 = br.ReadByte();
                            if (b2 == 0x0b)
                            {
                                collection.AddMap(Beatmap.GetBeatmapFromMD5(br.ReadString()));
                            }
                            else if (b1 == 0x00)
                                continue;
                            else
                            {
                                br.Close();
                                return null;
                            }
                        }
                        collectionFile.collections.Add(collection);
                    }
                    else if (b1 == 0x00)
                        continue;
                    else
                    {
                        br.Close();
                        return null;
                    }

                }
            }
            catch (Exception ex) { Console.Error.WriteLine(ex.StackTrace); collectionFile = null; }
            finally
            {
                if (br != null)
                {
                    br.Close();
                }
            }
            return collectionFile;
		}

		public void Export(string file)
		{
			BinaryWriter bw = new BinaryWriter(File.OpenWrite(file));
            bw.Write(version);
            bw.Write(collections.Count);
            foreach (Collection collec in collections)
            {
                bw.Write((byte)0x0B);
                bw.Write(collec.GetName());
                bw.Write(collec.GetMapList().Count);
                foreach (string map in collec.GetMapList())
                {
                    bw.Write((byte)0x0B);
                    bw.Write(map);
                }
            }
            bw.Flush();
            bw.Close();
		}

		public CollectionFile()
		{
			version = 0;
			collections = new List<Collection>();
		}

		public CollectionFile(string file)
		{
			version = 0;
			int collectionCount = 0;
			uint offset = 0;
			byte[] filecontent = File.ReadAllBytes(file);
			if (filecontent.Length < 8)
				return ;
			for (int i = 0; i < 4; i++)
				version |= filecontent[i] << (4 - i);
			for (int i = 0; i < 4; i++)
				collectionCount |= filecontent[i + 4] << (4 - i);
			offset = 8;
			for (int i = 0; i < collectionCount; i++)
			{
				offset++;
				byte collectionNameLength = filecontent[offset];
				string collectionName = "";
				for (int j = 0; j < collectionNameLength; j++)
					collectionName += (char)filecontent[++offset];
				Collection collection = new Collection(collectionName);
				int collectionSize = 0;
				for (int j = 0; j < 4; j++)
					collectionSize |= filecontent[++offset] << (4 - i);
				for (int j = 0; j < collectionSize; j++)
				{
					offset += 2;
					string md5 = "";
					for (int k = 0; k < 32; k++)
					{
						md5 += filecontent[++offset];
					}
					collection.AddMap(Beatmap.GetBeatmapFromMD5(md5));
				}
                collections.Add(collection);
			}
		}
	}
}
