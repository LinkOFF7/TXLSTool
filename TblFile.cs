using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace TXLSTool
{
    internal class TblFile
    {
        public string Magic { get; set; }
        public uint Unknownx04 { get; set; }
        public int TextLength { get; set; } //Length of entire text in the file without any other values
        public int Count { get; set; }
        public uint Unknownx14 { get; set; } //0
        public ushort Unknownx18 { get; set; } //0

        public struct TextEntry
        {
            public short Index; //0,1,2,3,4 etc.
            public short Length; //delimited by 2
            public string Text; //no null-terminated string
        }

        public void Extract(string tlbFile)
        {
            BinaryReader reader = new BinaryReader(File.OpenRead(tlbFile));
            Magic = Encoding.UTF8.GetString(reader.ReadBytes(4));
            if (Magic != "TXLS")
                throw new FormatException("Incorrect magic! TXLS was expected.");
            Unknownx04 = reader.ReadUInt32();
            if(Unknownx04 != 1065688760)
                throw new FormatException("Incorrect value at 0x04!");
            TextLength = reader.ReadInt32();
            Count = reader.ReadInt32();
            Unknownx14 = reader.ReadUInt32();
            Unknownx18 = reader.ReadUInt16();

            TextEntry[] text = new TextEntry[Count];
            for(int i = 0; i < Count; i++)
            {
                TextEntry entry = new TextEntry();
                entry.Index = reader.ReadInt16();
                entry.Length = reader.ReadInt16();
                entry.Text = Encoding.Unicode.GetString(reader.ReadBytes(entry.Length * 2)).Replace("\r", "{CR}").Replace("\n", "{LF}");
                text[i] = entry;
            }
            File.WriteAllLines(tlbFile + ".txt", text.Select(t => t.Text).ToArray());
            File.WriteAllLines(Path.GetFileNameWithoutExtension(tlbFile) + "_indexmap.txt", text.Select(t => t.Index.ToString()).ToArray());
        }

        public void Create(string textFile)
        {
            string mapFile = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(textFile)) + "_indexmap.txt";
            if (!File.Exists(mapFile))
                throw new FileNotFoundException("Unable to load mapfile!");
            string[] text = File.ReadAllLines(textFile);
            short[] ids = File.ReadAllLines(mapFile).Select(i => short.Parse(i)).ToArray();
            if(text.Length != ids.Length)
            {
                Console.WriteLine("Text count and map count are not equal: {0}/{1}\nAborting...", text.Length, ids.Length);
                return;
            }

            BinaryWriter writer = new BinaryWriter(File.Create(Path.GetFileNameWithoutExtension(textFile) + "_new.tbl"));
            writer.Write(Encoding.UTF8.GetBytes("TXLS"));
            writer.Write(1065688760);
            writer.BaseStream.Position += 4;
            int sumlen = 0;//calculate new text len
            writer.Write(text.Length);
            writer.Write(0);
            writer.Write((short)0);

            for(int i = 0; i < text.Length; i++)
            {
                string line = text[i].Replace("{CR}", "\r").Replace("{LF}", "\n");
                TextEntry entry = new TextEntry();
                entry.Index = ids[i];
                entry.Length = (short)(Encoding.Unicode.GetByteCount(line) / 2);
                entry.Text = line;
                sumlen += entry.Length;
                writer.Write(entry.Index);
                writer.Write(entry.Length);
                writer.Write(Encoding.Unicode.GetBytes(entry.Text));
            }
            writer.BaseStream.Position = 0x8;
            writer.Write(sumlen);
            writer.Close();
        }
    }
}
