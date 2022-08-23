using System;
using System.Collections.Generic;
using System.Text;

using System.IO;

namespace SBI2PicViewerLib
{
    class FileReadWrite
    {
        public static void write(string s, string file)
        {
            using (StreamWriter outfile = new StreamWriter(file))
            {
                outfile.Write(s);
            }
        }

        public static string read(string file)
        {
            if (System.IO.File.Exists(file))
            {
                return System.IO.File.ReadAllText(file);
            }
            return "";
        }
    }
}
