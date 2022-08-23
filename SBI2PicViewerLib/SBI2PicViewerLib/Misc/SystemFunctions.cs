using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.IO;

namespace SBI2PicViewerLib.Misc
{
    class SystemFunctions
    {
        public static float getFreeMemoryInMB()
        {
            PerformanceCounter freeMem = new PerformanceCounter("Memory", "Available Bytes");
            return freeMem.NextValue();
        }

        public static bool fileLoadAble(string strFileName)
        {
            FileInfo f = new FileInfo(strFileName);
            float s1 = f.Length;
            if ((s1 / 1000) > (getFreeMemoryInMB() / 2))
            {
                return false;
            }
            return true;
        }
    }
}
