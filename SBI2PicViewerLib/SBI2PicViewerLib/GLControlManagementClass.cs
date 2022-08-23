using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using OpenTK;
using Cornerstones.VectorMath;
using OpenTK.Graphics.OpenGL;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Cornerstones.Poly2DMath;

namespace SBI2PicViewerLib
{
    public class GLControlManagementClass
    {
        
        private OpenTK.GLControl _glScreen; //OpenGL Screen in Eigenregie/Eigenkontrolle
        private GLControlInterface _controlInt = null;
        private System.ComponentModel.IContainer _container = new System.ComponentModel.Container();

        private Color _contourDrawingColor = Color.Red;

        private System.Windows.Forms.Timer _timer;

        string _strData = "";

        public Color currentColor;
        public long imageCursorXPos;
        public long imageCursorYPos;
        public long imageControlXPos;
        public long imageControlYPos;

        /// <summary>
        /// Gives back the GLScreen - but needs a reference to OpenTK to work.
        /// </summary>
        public GLControl SCREEN
        {
            get
            {
                return _glScreen;
            }
        }

        public SimpleBigImage2.SBImage IMAGE
        {
            get
            {
                return _controlInt?.CONTAINER?.IMAGE;
            }
        }

        public Bitmap doScreenshot()
        {
            Bitmap bmp = new Bitmap(_glScreen.Width, _glScreen.Height);
            System.Drawing.Imaging.BitmapData data =
                bmp.LockBits(new Rectangle(0, 0, _glScreen.Width, _glScreen.Height), System.Drawing.Imaging.ImageLockMode.WriteOnly,
                             System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            ErrorCode ec = GL.GetError();
            GL.Flush();
            GL.ReadBuffer(ReadBufferMode.Back);
            GL.ReadPixels(0, 0, _glScreen.Width, _glScreen.Height, PixelFormat.Bgr,
                          PixelType.UnsignedByte, data.Scan0);
            ec = GL.GetError();
            bmp.UnlockBits(data);

            bmp.RotateFlip(RotateFlipType.RotateNoneFlipY);
            //_glScreen.DrawToBitmap(bmp, new Rectangle(0, 0, bmp.Width, bmp.Height));

            return bmp;
        }

        /// <summary>
        /// Gives back the GLScreen, but only as UserControl. Works in normal Windows Forms.
        /// </summary>
        public UserControl CONTROL
        {
            get
            {
                return _glScreen;
            }
        }

        /// <summary>
        /// Control Management Constructor for Windows Forms
        /// </summary>
        /// <param name="frm">Form</param>
        /// <param name="xPos">The X Position on the form</param>
        /// <param name="yPos">The Y Position on the form</param>
        /// <param name="Width">The Width</param>
        /// <param name="Height">The Height</param>
        public GLControlManagementClass(Form frm, int xPos, int yPos, int Width, int Height)
        {
            //muss davor stehen, initializeComponent macht visuelle Updates
            initializeOpenGL();
            //muss dahinter stehen, sonst existiert pbSpace nicht
            fitOpenGL(xPos, yPos, Width, Height); 

            frm.Controls.Add(_glScreen);
            frm.FormClosing += frm_FormClosing;
            
            _glScreen.BringToFront();

            initTimer();
        }

        /// <summary>
        /// Control Management Constructor for WPF
        /// </summary>
        /// <param name="winformhost">The WPF Windows Form Host</param>
        /// <param name="xPos">The X Position on the Form</param>
        /// <param name="yPos">The Y Position on the Form</param>
        /// <param name="Width">The Width</param>
        /// <param name="Height">The Height</param>
        public GLControlManagementClass(object winformhost, int xPos, int yPos, int Width, int Height)
        {
            if (winformhost.GetType() == typeof(System.Windows.Forms.Integration.WindowsFormsHost))
            {
                System.Windows.Forms.Integration.WindowsFormsHost host = (System.Windows.Forms.Integration.WindowsFormsHost)winformhost;

                initializeOpenGL(); 
                fitOpenGL(xPos, yPos, Width, Height); 

                host.Child = _glScreen;
                host.Unloaded += host_Unloaded;

                initTimer();
            }
            else
            {
                throw new NotSupportedException("Only System.Windows.Forms.Integration.WindowsFormsHost is supported!");
            }
        }








        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //Events







        /// <summary>
        /// An Event. Please dont trigger from outside.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void host_Unloaded(object sender, System.Windows.RoutedEventArgs e)
        {
            deinit();
        }
        
        /// <summary>
        /// An Event. Please dont trigger from outside.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void frm_FormClosing(object sender, FormClosingEventArgs e)
        {
            deinit();
        }

        /// <summary>
        /// Deinitializes the Object
        /// </summary>
        void deinit()
        {
            if (_glScreen != null)
            {
                _glScreen.Dispose();
                _glScreen = null;
            }
            if (_controlInt != null)
            {
                _controlInt.destroy();
                _controlInt = null;
            }
        }







        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //Timers






        /// <summary>
        /// Initializes the Timer, will be called internally
        /// </summary>
        private void initTimer()
        {
            _timer = new System.Windows.Forms.Timer(_container);
            _timer.Enabled = true;
            _timer.Interval = 10;
            _timer.Tick += new System.EventHandler(timerTick);
        }

        /// <summary>
        /// The Timer Tick, renders a frame
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timerTick(object sender, EventArgs e)
        {
            if (_controlInt != null)
            {
                _controlInt.start();
            }
        }

        /// <summary>
        /// fits the OpenGL Element to whatever size you want
        /// </summary>
        /// <param name="x">The X Position</param>
        /// <param name="y">The Y Position</param>
        /// <param name="w">The Width of the Element</param>
        /// <param name="h">The Height of the Element</param>
        private void fitOpenGL(int x, int y, int w, int h)
        {
            _glScreen.Location = new System.Drawing.Point(x,y);
            _glScreen.Size = new System.Drawing.Size(w,h);
        }

        /// <summary>
        /// Initializes the OpenGL Element; Please don't call from the outside
        /// </summary>
        /// <returns>A GLControl Element</returns>
        private GLControl initializeOpenGL()
        {
            _glScreen = new GLControl();
            _glScreen.BackColor = System.Drawing.Color.Black;

            _glScreen.TabIndex = 300; //irgendein Wert
            _glScreen.VSync = false;
            _glScreen.MouseMove += glScreen_MouseMove;
            _glScreen.MouseDown += glScreen_MouseDown;
            _glScreen.MouseUp += glScreen_MouseUp;
            _glScreen.MouseLeave += glScreen_MouseLeave;

            return _glScreen;
        }





        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //Events




        /// <summary>
        /// OnMouseDown event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void glScreen_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right && !SBI2PicViewerLib.ContourDrawingConfiguration.DELETE)
            {
                _controlInt.mouseDrawOnMouseDown(DRAW_COLOR);
            }
            if (SBI2PicViewerLib.ContourDrawingConfiguration.DELETE)
            {

            }
        }

        /// <summary>
        /// OnMouseUp event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void glScreen_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right && !SBI2PicViewerLib.ContourDrawingConfiguration.DELETE)
            {
                _controlInt.mouseDrawOnMouseUp(DRAW_COLOR, ContourDrawingConfiguration.CURRENT_LAYER);
            }
            if (ContourDrawingConfiguration.DELETE && e.Button == MouseButtons.Right)
            {
                float x, y;
                _controlInt.getMousePicPosition(out x, out y);
                List<Geom.DrawPolyline2D> polylines = _controlInt.getPolylinesAt(x, y, false);

                _controlInt.removePolylines(polylines);
                update();
            }
        }

        /// <summary>
        /// Happens when the mouse leaves the OpenGL Surface; Shuts down the mouse tracking
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void glScreen_MouseLeave(object sender, EventArgs e)
        {
            if (_controlInt != null)
            {
                _controlInt.setMousePosition(-1, -1);
            }
        }

        /// <summary>
        /// OnMouseMove Event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void glScreen_MouseMove(object sender, MouseEventArgs e)
        {
            if (_controlInt != null)
            {
                _controlInt.setMousePosition(e.X, e.Y);
                float x = e.X, y = e.Y;
                _controlInt.getMousePicPosition(out x, out y);
                //The mouse pointer should be updated on mouse move
                if (_controlInt.hasMousePointer())
                {
                    _controlInt.setMousePointerPosition((int)x, (int)y);
                    //Gives the command that something worth to draw has happened
                    _controlInt.update();
                }
                int r = 0;
                int g = 0;
                int b = 0;

                float posX = e.X;
                float posY = e.Y;

                int iMaxWidth = _glScreen.Width;
                int iMaxHeight = _glScreen.Height;

                if (!(posX > iMaxWidth - 2 || posY > iMaxHeight - 2) || posX < 2 || posY < 2)
                {
                    Bitmap bmp = new Bitmap(1, 1);
                    System.Drawing.Imaging.BitmapData data =
                        bmp.LockBits(new Rectangle(0,0,1,1), System.Drawing.Imaging.ImageLockMode.WriteOnly,
                                     System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                    ErrorCode ec = GL.GetError();
                    GL.Flush();
                    GL.ReadBuffer(ReadBufferMode.Back);
                    GL.ReadPixels((int)posX, iMaxHeight - (int)posY, 1, 1, PixelFormat.Bgr,
                                  PixelType.UnsignedByte, data.Scan0);
                    ec = GL.GetError();
                    bmp.UnlockBits(data);

                    Color c = bmp.GetPixel(0, 0);
                    bmp.Dispose();

                    r = c.R;
                    b = c.B;
                    g = c.G;

                    currentColor = Color.FromArgb(r, g, b);
                    imageCursorXPos = (long)(x + 0.5);
                    imageCursorYPos = (long)(y + 0.5);
                    imageControlXPos = (long)(posX + 0.5);
                    imageControlYPos = (long)(posY + 0.5);

                    if (e.Button == MouseButtons.Right && !SBI2PicViewerLib.ContourDrawingConfiguration.DELETE)
                    {
                        _controlInt.mouseDrawOnMove(_contourDrawingColor);
                    }
                }
            }
        }



        public void update()
        {
            if (_controlInt != null)
            {
                _controlInt.update();
            }
        }



        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //Contours and Colors

        public void setHUD(List<SBI2PicViewerLib.Renderer.GUIElementInterface> guiElements)
        {
            if (_controlInt != null)
            {
                _controlInt.setGUI(guiElements);
                _controlInt.update();
            }
        }

        /// <summary>
        /// Adds a contour to the "to Draw" List
        /// </summary>
        /// <param name="contours">The contours of the Element</param>
        /// <param name="color">The Color the contours have</param>
        public void addContours(Contours contours, Color color)
        {
            if (_controlInt != null)
            {
                _controlInt.addContours(contours, color);
            }
        }

        /// <summary>
        /// Sets the given Contours to be drawn as overlay
        /// </summary>
        /// <param name="contours">The contours to draw</param>
        /// <param name="defaultColor">The color</param>
        public void setContours(Contours contours, Color defaultColor)
        {
            if (_controlInt != null)
            {
                _controlInt.setContours(contours, defaultColor);
            }
        }

        /// <summary>
        /// Gets the Contour at Position x, y (Image Coordinates), due to the enormous ammount of Contours supported, only visible (rendered) contours are being processed (thus, small contours may get ignored by this function)
        /// </summary>
        /// <param name="x">X Position</param>
        /// <param name="y">Y Position</param>
        /// <returns></returns>
        public List<Contour> getContoursAt(float x, float y)
        {
            if (_controlInt != null)
            {
                return _controlInt.getContoursAt(x, y);
            }
            return new List<Contour>();
        }

        public List<Contour> getAllContours()
        {
            if (_controlInt != null)
            {
                return _controlInt.getAllContours();
            }
            return new List<Contour>();
        }

        /// <summary>
        /// The Color Contours are drawn (not shown later on, just DRAWN) in
        /// </summary>
        public Color DRAW_COLOR
        {
            get
            {
                return _contourDrawingColor;
            }
            set
            {
                _contourDrawingColor = value;
            }
        }

        /// <summary>
        /// Get all Elements drawn
        /// </summary>
        /// <param name="blnWithHoles"></param>
        /// <returns></returns>
        public List<OutputImageContainer> getDrawnElements(bool blnWithHoles = true)
        {
            if (_controlInt != null)
            {
                return _controlInt.getDrawnContours(blnWithHoles);
            }
            return new List<OutputImageContainer>();
        }

        public void addCurrentlyShownContours(Contours toAdd, int iLayer = 0)
        {
            if (_controlInt != null)
            {
                _controlInt.addCurrentlyShownContours(toAdd, iLayer);
            }
        }

        /// <summary>
        /// Resets all drawn Elements; That doesn't mean they get removed from the visible shapes, just internal from the "printout" list
        /// </summary>
        public void resetDrawnElements()
        {
            if (_controlInt != null)
            {
                _controlInt.resetDrawnElements();
            }
        }


        public void setControlable()
        {
            if (_controlInt != null)
            {
                _controlInt.setControlable();
            }
        }

        public void setUncontrolable()
        {
            if (_controlInt != null)
            {
                _controlInt.setUncontrolable();
            }
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //Loading file




        /// <summary>
        /// Loads a file
        /// </summary>
        /// <param name="strInput"></param>
        public void loadFile(string strInput)
        {
            if (System.IO.File.Exists(strInput) || System.IO.Directory.Exists(strInput))
            {
                StartNewWithImage(strInput);
                _strData = strInput;
            }
        }




        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //Mouse and Mouse functions




        /// <summary>
        /// Adds another "Mouse Pointer", which is nothing but a second or third polygon attached to the mouse movement. This is also the Element which draws when the right mouse is hold down.
        /// </summary>
        /// <param name="c">The "mouse pointer". A Polygon that moves with the mouse, and is used for drawing.</param>
        /// <param name="colr">The Color of the mouse pointer, can be anything</param>
        public void addMousePointer(Contour c, Color colr)
        {
            if (_controlInt != null)
            {
                _controlInt.addMousePointerElement(c, colr);
                _controlInt.update();
            }
        }

        /// <summary>
        /// Resets the Mouse Pointer to nothing
        /// </summary>
        public void resetMousePointer()
        {
            if (_controlInt != null)
            {
                _controlInt.resetMousePointerElement();
                _controlInt.update();
            }
        }

        /// <summary>
        /// Puts a "Mouse Pointer", which is nothing but a polygon attached to the mouse movement. This is also the Element which draws when the right mouse is hold down.
        /// </summary>
        /// <param name="c">The "mouse pointer". A Polygon that moves with the mouse, and is used for drawing.</param>
        /// <param name="colr">The Color of the mouse pointer, can be anything</param>
        public void setMousePointer(Contour c, Color colr)
        {
            if (_controlInt != null)
            {
                _controlInt.setMousePointerElement(c, colr);
                _controlInt.update();
            }
        }

        /// <summary>
        /// Puts the mousepointer where the mouse is
        /// </summary>
        public void putMousePointerUnderMouse()
        {
            if (_controlInt != null)
            {
                float x, y;
                _controlInt.getMousePicPosition(out x, out y);
                //The mouse pointer should be updated on mouse move
                if (_controlInt.hasMousePointer())
                {
                    _controlInt.setMousePointerPosition((int)x, (int)y);
                    //Gives the command that something worth to draw has happened
                    _controlInt.update();
                }
            }
        }

        /// <summary>
        /// Gets the mouse position on the SBI Image
        /// </summary>
        /// <param name="x">Output X Position</param>
        /// <param name="y">Output Y Position</param>
        public void getMousePicPosition(out float x, out float y)
        {
            if (_controlInt != null)
            {
                _controlInt.getMousePicPosition(out x, out y);
            }
            x = -1;
            y = -1;
        }


        

        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //New Image




        /// <summary>
        /// Restarts the whole System with a new image. Kind of a "hard reset", all Data not stored gets lost.
        /// </summary>
        /// <param name="strSource">The image to load</param>
        private void StartNewWithImage(string strSource)
        {
            if (!System.IO.File.Exists(strSource) && !System.IO.Directory.Exists(strSource))
            {
                return;
            }

            _controlInt = GLControlInterface.RenderEx(strSource, _glScreen);

            _controlInt.center();
        }


        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////



        /// <summary>
        /// changes the size of the OpenGL Management
        /// </summary>
        /// <param name="xPos">X Position of the viewer on the Form</param>
        /// <param name="yPos">Y Position of the viewer on the Form</param>
        /// <param name="Width">width</param>
        /// <param name="Height">height</param>
        public void changeSize(int xPos, int yPos, int Width, int Height)
        {
            fitOpenGL(xPos,yPos,Width,Height);
        }

        /// <summary>
        /// Disposes everything used. Please do not use the object after that.
        /// </summary>
        public void Dispose()
        {
            deinit();
        }


        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////


        public void setPosition(long x, long y)
        {
            _controlInt?.setPosition(x, y);
        }

        /// <summary>
        /// Sets the coordinates "seen by the camera", technically, moves the image and zooms out/in
        /// </summary>
        /// <param name="x">x position on the image that will be moved to the center of the camera</param>
        /// <param name="y">y position on the image that will be moved to the center of the camera</param>
        /// <param name="w">width of the sector (ratio always 1:1 with height, everything else will be ignored)</param>
        /// <param name="h">height of the sector</param>
        public void setCoordinates(float x, float y, float w, float h)
        {
            if (_controlInt != null)
            {
                x += w / 2;
                y += h / 2;

                _controlInt.goTo(x, y, w, h);
                _controlInt.update();
            }
        }



        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //Copy and Paste

        /// <summary>
        /// A Copy-Paste function; Uses data from the clipboard, and if formatted correctly, moves the Image with setCoordinates. If the data from the clipboard is not set correctly, the computer will crash and burn and the programmer will drown in tears.
        /// </summary>
        public void paste()
        {
            if (_controlInt == null)
            {
                return;
            }
            
            string input = Clipboard.GetText();
            string[] toCalc = input.Split(new char[] { '\t', ';' }, StringSplitOptions.RemoveEmptyEntries);
            float x, y, w, h;

            if (toCalc.Length == 4)
            {
                try
                {
                    y = Convert.ToSingle(toCalc[0]);
                    x = Convert.ToSingle(toCalc[1]);
                    w = Convert.ToSingle(toCalc[2]);
                    h = Convert.ToSingle(toCalc[3]);
                    _controlInt.goTo(x, y, w, h);
                    _controlInt.update();
                }
                finally { };
            }
        }

        /// <summary>
        /// A Copy-Paste function; Puts the current coordinates in the clipboard.
        /// </summary>
        public void copy()
        {
            if (_controlInt == null)
            {
                return;
            }

            float x, y, w, h;
            _controlInt.whereAreWe(out x, out y, out w, out h);
            Clipboard.SetText(y + ";" + x + ";" + w + ";" + h);
        }


        public double[] getScreenScaling()
        {
            if (_controlInt != null)
            {
                return _controlInt.getScreenScaling();
            }
            return null;
        }

        public double[] getImageScaling()
        {
            if (_controlInt != null)
            {
                return _controlInt.getImageScaling();
            }
            return null;
        }
    }
}
