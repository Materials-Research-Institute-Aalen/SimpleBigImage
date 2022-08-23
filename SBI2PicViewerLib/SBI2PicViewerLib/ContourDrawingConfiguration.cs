using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SBI2PicViewerLib
{
    /// <summary>
    /// A Class that stores the configuration on "how to draw new polygons"
    /// </summary>
    public class ContourDrawingConfiguration
    {
        public static bool DELETE = false;

        public static bool ORIGINAL_PLUS = true;
        public static Color ELEMENT_COLOR = Color.White;
        public static Color HOLE_COLOR = Color.Red;
        public static Color BACKGROUND_COLOR = Color.Black;

        public static int CURRENT_LAYER = 0;
        public static Color FINESTRUCTURE_COLOR = Color.Red;

        public static bool drawOnMouseHold = true;

        /// <summary>
        /// Opens a ColorPicker for the user
        /// </summary>
        /// <returns></returns>
        public static Color getHandPickedColor()
        {
            ColorDialog dlg = new ColorDialog();

            dlg.ShowDialog();

            return dlg.Color;
        }
    }
}
