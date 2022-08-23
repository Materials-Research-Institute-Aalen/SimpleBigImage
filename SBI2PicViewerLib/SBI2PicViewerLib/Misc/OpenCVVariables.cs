using System;
using System.Collections.Generic;
using System.Text;

namespace SBI2PicViewerLib
{
    class OpenCVVariables
    {
        public static int Schrittweite = 1024;
        public static string strFolder = @"C:\TextureFolder\";

        public static void initFolder()
        {
            if (!System.IO.Directory.Exists(strFolder))
            {
                System.IO.Directory.CreateDirectory(strFolder);
            }
        }
    }

}

