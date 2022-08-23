using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading;

using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using Cornerstones.VectorMath;
using Cornerstones.Poly2DMath;
using SBI2PicViewerLib.Geom;


namespace SBI2PicViewerLib
{
    class GLControlInterface
    {
        #region --- Fields ---
        private bool _blnControlable = true;
        private Geom.Camera _cam;
        GLControl GUI;
        ContourDrawing _contDrawer;

        public static int iPieceCount = 0;

        private static bool blnStarted = false;

        public Thread renderer;

        private bool _blnChanges = true;

        int Width, Height;
        double _xPos = 0; double _yPos = 0;
        bool exit = false;

        static bool blnRenderMutex = false;

        Renderer.SBIRenderContainer _container;
        TextWriter _text;

        int _MouseX;
        int _MouseY;

        MouseState _current, _previous;
        PointF _PointPictureSpace;

        VectorRoom _pictureRoom;

        #endregion

        #region static variables

        static GLControlInterface controlInterface;

        #endregion

        #region --- Constructor ---

        public GLControlInterface(GLControl control)
        {
            Width = control.Width;
            Height = control.Height;

            _cam = new Geom.Camera(0, 0, Width, Height);
            GUI = control;
            exit = false;

            renderer = new Thread(renderThreaded);
            _contDrawer = new ContourDrawing(this);

            control.Resize += control_Resize;

            OnLoad();
        }

        public void destroy()
        {
            renderer.Abort();

            GUI.Resize -= control_Resize;

            if (_container != null)
            {
                _container.Dispose();
                _container = null;
            }

            _cam = null;
        }

        void control_Resize(object sender, EventArgs e)
        {
            GL.Viewport(0, 0, GUI.Width, GUI.Height);

            _cam.resize(GUI.Width, GUI.Height);
            update();
            OnUpdateFrame();
        }

        public void update()
        {
            _blnChanges = true;
        }

        #endregion

        #region OnLoad

        protected void OnLoad()
        {
            GL.ClearDepth(2.0);
            GL.ClearColor(Color.Black);
            GL.Enable(EnableCap.Texture2D);
        }

        #endregion

        #region OnResize

        /// <summary>
        /// Called when the user resizes the window.
        /// </summary>
        /// <param name="e">Contains the new width/height of the window.</param>
        /// <remarks>
        /// You want the OpenGL viewport to match the window. This is the place to do it!
        /// </remarks>
        protected void OnResize()
        {
            GL.Viewport(0, 0, Width, Height);
            double aspect_ratio = Width / (double)Height;

            _text.resize(Width, Height);
            _cam.resize(Width, Height); //Resize funktioniert nicht? -> update...
        }
        #endregion

        #region internals

        private float mouseXPos()
        {
            return _MouseX;
        }

        private float mouseYPos()
        {
            return _MouseY;
        }

        #endregion

        #region render

        /// <summary>
        /// Prepares the next frame for rendering.
        /// </summary>
        /// <remarks>
        /// Place your control logic here. This is the place to respond to user input,
        /// update object positions etc.
        /// </remarks>
        protected void OnUpdateFrame()
        {
            float speed = 5f;

            float xMousePos = mouseXPos();//this.X;
            float yMousePos = mouseYPos();//this.Y;
            float[] Pos;
            if (_blnControlable)
            {
                if (xMousePos < 0)
                {
                    return;
                }
                if (yMousePos < 0)
                {
                    return;
                }
                if (xMousePos > GUI.Width)
                {
                    return;
                }
                if (yMousePos > GUI.Height)
                {
                    return;
                }

                if (KeyPresses.isKeyDown(KeyPresses.VK_LSHIFT) || KeyPresses.isKeyDown(KeyPresses.VK_RSHIFT))
                {
                    speed *= 4;
                }

                if (KeyPresses.isKeyDown(KeyPresses.VK_LCONTROL) || KeyPresses.isKeyDown(KeyPresses.VK_RCONTROL))
                {
                    Pos = _cam.fromOwnPointToReference(new float[] { Width / 2, Height / 2 });
                    _PointPictureSpace = new PointF(Pos[0], Pos[1]);
                    if (KeyPresses.isKeyDown(KeyPresses.VK_UP))
                    {
                        _cam.zoom(1);
                    }
                    if (KeyPresses.isKeyDown(KeyPresses.VK_DOWN))
                    {
                        _cam.zoom(-1);
                    }
                    _cam.putOwnPointOverReferencePoint(Width / 2, Height / 2, _PointPictureSpace.X, _PointPictureSpace.Y);
                    _blnChanges = true;
                }
                else
                {
                    if (KeyPresses.isKeyDown(KeyPresses.VK_LEFT))
                    {
                        _cam.addPosition(-speed / _cam.getZoom(), 0);
                        _blnChanges = true;
                    }
                    if (KeyPresses.isKeyDown(KeyPresses.VK_RIGHT))
                    {
                        _cam.addPosition(speed / _cam.getZoom(), 0);
                        _blnChanges = true;
                    }
                    if (KeyPresses.isKeyDown(KeyPresses.VK_UP))
                    {
                        _cam.addPosition(0, -speed / _cam.getZoom());
                        _blnChanges = true;
                    }
                    if (KeyPresses.isKeyDown(KeyPresses.VK_DOWN))
                    {
                        _cam.addPosition(0, speed / _cam.getZoom());
                        _blnChanges = true;
                    }
                }
                if (KeyPresses.isKeyDown(KeyPresses.VK_SPACE))
                {
                    center();
                    _blnChanges = true;
                }


                

                _current = OpenTK.Input.Mouse.GetState();
                Pos = new float[] { 0, 0 };
                if (_current != _previous)
                {
                    int zdelta = _current.Wheel - _previous.Wheel;
                    Pos = _cam.fromOwnPointToReference(new float[] { xMousePos, yMousePos });

                    if (zdelta != 0)
                    {
                        _PointPictureSpace = new PointF(Pos[0], Pos[1]);
                        _cam.zoom(zdelta);
                        _cam.putOwnPointOverReferencePoint(xMousePos, yMousePos, _PointPictureSpace.X, _PointPictureSpace.Y);
                        _blnChanges = true;
                    }

                    if (_current.IsButtonDown(MouseButton.Left))
                    {
                        if (_previous.IsButtonDown(MouseButton.Left))
                        {
                            _cam.putOwnPointOverReferencePoint(xMousePos, yMousePos, _PointPictureSpace.X, _PointPictureSpace.Y);
                            _blnChanges = true;
                        }
                        else
                        {
                            _PointPictureSpace = new PointF(Pos[0], Pos[1]);
                        }
                    }
                }
            }
            double dblX = 0;
            double dblY = 0;
            //Stage.getStagePos(ref dblX, ref dblY);

            if (Math.Abs(_xPos - dblX) > 20 || Math.Abs(_yPos - dblY) > 20)
            {
                _xPos = dblX;
                _yPos = dblY;
                _blnChanges = true;
            }

            _previous = _current;

        }

        /// <summary>
        /// Place your rendering code here.
        /// </summary>
        protected void OnRenderFrame()
        {
            if (_blnChanges)
            {
                _cam.render(_container.GUIMANAGEMENT, _container.TEXTUREMANAGEMENT, _container.CONTOURMANAGEMENT, _container.MOUSEPOINTER);
                GUI.SwapBuffers();
                _blnChanges = false;
            }
        }

        /// <summary>
        /// Invalidates the GUI, thus triggering a new render loop.
        /// </summary>
        public void render()
        {
            try
            {
                GUI.Invalidate();
            }
            catch { };
        }

        /// <summary>
        /// Clears the mouse pointer
        /// </summary>
        public void resetMousePointerElement()
        {
            _container.MOUSEPOINTER.Clear();
        }

        public void setPosition(long xPos, long yPos)
        {
            _cam.putOwnPointOverReferencePoint(Width / 2, Height / 2, xPos, yPos);
        }

        /// <summary>
        /// Sets the mouse pointer contour
        /// </summary>
        /// <param name="contour"></param>
        /// <param name="colr"></param>
        public void setMousePointerElement(Contour contour, Color colr)
        {
            _container.MOUSEPOINTER.Clear();
            _container.MOUSEPOINTER.Add(new Geom.DrawPolyline2D(contour, colr));
        }

        /// <summary>
        /// Adds a mouse pointer element
        /// </summary>
        /// <param name="contour"></param>
        /// <param name="colr"></param>
        public void addMousePointerElement(Contour contour, Color colr)
        {
            _container.MOUSEPOINTER.Add(new Geom.DrawPolyline2D(contour, colr));
        }

        /// <summary>
        /// Draws "onMouseDown" - Kind of an Event. Should only be triggered by an Event.
        /// </summary>
        /// <param name="colr"></param>
        public void mouseDrawOnMouseDown(Color colr)
        {
            _contDrawer.mouseDrawOnMouseDown(colr);
        }

        /// <summary>
        /// Draws "onMouseUp" - Kind of an Event. Should only be triggered by an Event.
        /// </summary>
        /// <param name="colr"></param>
        public void mouseDrawOnMouseUp(Color colr, int iLayer)
        {
            _contDrawer.mouseDrawOnMouseUp(colr, iLayer);
        }

        /// <summary>
        /// Draws "onMouseMove" - Kind of an Event. Should only be triggered by an Event.
        /// </summary>
        /// <param name="colr"></param>
        public void mouseDrawOnMove(Color colr)
        {
            _contDrawer.mouseDrawOnMove(colr);
        }

        public void addCurrentlyShownContours(Contours toAdd, int iLayer = 0)
        {
            _contDrawer.addCurrentlyShownContours(toAdd, iLayer);
        }
        /// <summary>
        /// Resets all drawn Elements.
        /// </summary>
        public void resetDrawnElements()
        {
            _contDrawer.resetDrawnElements();
        }

        /// <summary>
        /// returns all drawn contours
        /// </summary>
        /// <param name="blnWithHoles"></param>
        /// <returns></returns>
        public List<OutputImageContainer> getDrawnContours(bool blnWithHoles = true)
        {
            return _contDrawer.getDrawnContours(_container.IMAGE, blnWithHoles);
        }

        public void setGUI(List<SBI2PicViewerLib.Renderer.GUIElementInterface> guiElements)
        {
            _container.setGUI(guiElements);
        }

        public void setControlable()
        {
            _blnControlable = true;
        }

        public void setUncontrolable()
        {
            _blnControlable = false;
        }
       
        /// <summary>
        /// adds Contours to current Drawing
        /// </summary>
        /// <param name="contours"></param>
        /// <param name="defaultColor"></param>
        /// <returns></returns>
        public List<DrawPolyline2D> addContours(Contours contours, Color defaultColor)
        {
            if (_container != null)
            {
                return _container.addContours(contours, defaultColor);
            }
            return null;
        }

        public void smeltOntoContours(List<IntPoint> polygon, Color defaultColor)
        {
            if (_container != null)
            {
                _container.smeltOntoContours(polygon, defaultColor);
            }
        }

        public void setContours(Contours contours, Color defaultColor)
        {
            if (_container != null)
            {
                _container.setContours(contours, defaultColor);
            }
        }

        public bool hasMousePointer()
        {
            return _container.MOUSEPOINTER.Count != 0;
        }

        public List<DrawPolyline2D> getPolylinesAt(float x, float y, bool blnUseGUIReference = true)
        {
            if (blnUseGUIReference)
            {
                float[] output = _cam.fromOwnPointToReference(new float[] { x, y });

                x = output[0];
                y = output[1];
            }

            if (_container != null)
            {
                if (_container.CONTOURMANAGEMENT != null)
                {
                    return _container.CONTOURMANAGEMENT.getPolylinesAt((long)x, (long)y);
                }
            }
            return new List<DrawPolyline2D>();
        }

        public List<Contour> getAllContours()
        {
            List<Contour> result = new List<Contour>();
            if (_container.CONTOURMANAGEMENT != null)
            {
                foreach (var element in _container.CONTOURMANAGEMENT.getAllPolylines())
                {
                    result.Add(element.CONTOUR);
                }

                return result;
            }
            return new List<Contour>();
        }

        public List<Contour> getContoursAt(float x, float y)
        {
            float[] output = _cam.fromOwnPointToReference(new float[] { x, y });

            x = output[0];
            y = output[1];

            if (_container != null)
            {
                if (_container.CONTOURMANAGEMENT != null)
                {
                    return _container.CONTOURMANAGEMENT.getContoursAt((long)x, (long)y);
                }
            }
            return new List<Contour>();
        }

        public double[] getImageScaling()
        {
            if (_container != null)
            {
                return new double[] {_container.IMAGE.XSCALING, _container.IMAGE.YSCALING};
            }
            return null;
        }

        public double[] getScreenScaling()
        {
            if (_container != null)
            {
                return new double[] { _container.IMAGE.XSCALING * _cam.getScale(), _container.IMAGE.YSCALING * _cam.getScale() };
            }
            return null;
        }

        public void start()
        {
            blnStarted = true;
        }

        public void stop()
        {
            blnStarted = false;
        }

        private bool checkForChange()
        {
            return _blnChanges;
        }

        public void renderThreaded()
        {
            while (true)
            {
                if (blnStarted)
                {
                    render();
                }
                Thread.Sleep(10);
            }
        }

        #endregion

        #region exit
        public void Exit()
        {
            exit = true;
        }
        #endregion
        
        #region run
        public void Run(double fps, double unknown)
        {
            while (!exit)
            {
                GUI.Invalidate();
                GUI.Update();
                Thread.Sleep((int)(1000.0 / fps));
            }
        }
        #endregion

        #region main

        public static GLControlInterface RenderEx(string texPieces, GLControl control)
        {
            if (controlInterface != null)
            {
                control.Paint -= control_Paint;
                if (controlInterface._container != null)
                {
                    controlInterface._container.Dispose();
                    controlInterface._container = null;
                }
                controlInterface.destroy();
                controlInterface = null;
                GC.Collect();
            }
            try
            {
                controlInterface = new GLControlInterface(control);
            }
            catch (Exception ex)
            {
                throw new Exception("There was an error: " + ex.ToString()); 
            }
            controlInterface.renderer.Start();
            controlInterface._container = new Renderer.SBIRenderContainer(texPieces);
            //controlInterface._mouseDrawSupport = new ContourDrawSupportClass();

            controlInterface._pictureRoom = new VectorRoom();
            controlInterface._text = new TextWriter(new Size(100, 70));
            controlInterface._text.AddLine("-", new PointF(10, 10), Brushes.Red);
            controlInterface._text.AddLine("-", new PointF(10, 20), Brushes.Red);
            controlInterface._text.AddLine("-", new PointF(10, 30), Brushes.Red);
            controlInterface._text.AddLine("-", new PointF(10, 40), Brushes.Red);

            controlInterface.OnResize();

            control.Paint += control_Paint;

            return controlInterface;
        }

        static void control_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
        {
            if (!blnRenderMutex)
            {
                blnRenderMutex = true;
                controlInterface.OnUpdateFrame();
                controlInterface.OnRenderFrame();
                blnRenderMutex = false;
            }
        }
        #endregion

        #region variables

        public void setMousePosition(int x, int y)
        {
            _MouseX = x;
            _MouseY = y;
        }

        public void setMousePointerPosition(int x, int y)
        {
            if (_container != null)
            {
                foreach (Geom.DrawPolyline2D polyline in _container.MOUSEPOINTER)
                {
                    polyline.setPos(0, 1, x, y);
                }
            }

        }

        public bool getMousePicPosition(out float x, out float y)
        {
            float mx = mouseXPos();
            float my = mouseYPos();

            if (_cam == null)
            {
                x = 0;
                y = 0;
                return false;
            }

            float[] output = _cam.fromOwnPointToReference(new float[] { mx, my });

            x = output[0];
            y = output[1];

            return true;
        }

        public void center()
        {
            float picXSize;
            float picYSize;

            picXSize = (float)_container.WIDTH;
            picYSize = (float)_container.HEIGHT;
            
            float scalingW = picXSize / (float)GUI.Width;
            float scalingH = picYSize / (float)GUI.Height;

            float xPos = picXSize / 2;
            float yPos = picYSize / 2;
            _cam.setPos(0, scalingW > scalingH ? scalingW : scalingH, xPos, yPos);
            _cam.putOwnPointOverReferencePoint(GUI.Width / 2, GUI.Height / 2, xPos, yPos);

            _blnChanges = true;
        }

        public void goTo(float x, float y, float w, float h)
        {
            float scalingW = w / (float)GUI.Width;
            float scalingH = h / (float)GUI.Height;

            _cam.setPos(0, scalingW < scalingH ? scalingW : scalingH, x, y);

            //_cam.putOwnPointOverReferencePoint(GUI.Width / 2, GUI.Height / 2, x, y);
        }

        public void whereAreWe(out float x, out float y, out float w, out float h)
        {
            float scalingW = _cam.getScale();
            float scalingH = _cam.getScale();

            w = scalingW * (float)GUI.Width;
            h = scalingH * (float)GUI.Width;

            x = _cam.getXPos();// + w / 2;
            y = _cam.getYPos();// + h / 2;
        }

        public void removePolylines(List<DrawPolyline2D> input)
        {
            //ToDo: Testen
            _container.CONTOURMANAGEMENT.removePolylines(input);
            _contDrawer.removePolylines(input);
        }

        public Renderer.SBIRenderContainer CONTAINER
        {
            get
            {
                return _container;
            }
        }
        
        public float[] fromOwnPointToReference(float[] points)
        {
            return _cam.fromOwnPointToReference(points);
        }

        public float[] fromReferencePointToOwnPoint(float[] points)
        {
            return _cam.fromReferencePointToOwnPoint(points);
        }

        public float[] fromDisplayToRealWorldCoords(float[] points)
        {
            return null;
        }

        public float[] fromRealWorldToDisplayCoords(float[] points)
        {
            return null;
        }

        #endregion
    }
}
