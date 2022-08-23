using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SBI_SlidingWindow
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 6)
            {
                if (args.Length == 2)
                {
                    if (args[0] == "getWidth")
                    {
                        string strImage = args[1];
                        strImage = strImage.Replace("\r\n", "");
                        SimpleBigImage2.SBImage img = new SimpleBigImage2.SBImage(strImage);
                        Console.WriteLine(img.Width);
                    }
                    else
                    {
                        string strImage = args[1];
                        strImage = strImage.Replace("\r\n", "");
                        SimpleBigImage2.SBImage img = new SimpleBigImage2.SBImage(strImage);
                        Console.WriteLine(img.Height);
                    }
                    return;
                }
                Console.WriteLine("Wrong amount of arguments");
                return;
            }


            string strX = args[0];
            string strY = args[1];
            string strW = args[2];
            string strH = args[3];

            string strInput = args[4];
            string strOutput = args[5];

            strInput = strInput.Replace("\r\n", "");

            try
            {
                long iX = Convert.ToInt64(strX);
                long iY = Convert.ToInt64(strY);
                long iW = Convert.ToInt64(strW);
                long iH = Convert.ToInt64(strH);

                SimpleBigImage2.SBImage img = new SimpleBigImage2.SBImage(strInput);
                Image output = img.getImagePart(iX, iY, iW, iH);
                output.Save(strOutput);
            }
            catch(Exception ex)
            {
                Console.WriteLine("invalid size or input, wrong hole " + ex.ToString());
                return;
            }
        }
    }
}
