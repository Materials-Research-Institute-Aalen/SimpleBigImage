using Cornerstones.Poly2DMath;
using SBI2PicViewerLib.Geom;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SBI2PicViewerLib.Renderer
{
    class SBIRenderContainer
    {
        public static byte BACKGROUND_RED = 26;
        public static byte BACKGROUND_GREEN = 35;
        public static byte BACKGROUND_BLUE = 41;


        int _iTileSize;
        string _strImage;

        SimpleBigImage2.SBImage _image;
        TextureManagement _texmgmt;
        ContourManagement _cntmgmt;
        GUIManagement _guimgmt;
        List<DrawPolyline2D> _mousepointer;

        public SBIRenderContainer(string strImage)
        {
            _image = new SimpleBigImage2.SBImage(strImage);

            _iTileSize = _image.Tilesize;
            _strImage = strImage;

            _texmgmt = new TextureManagement(_image, _iTileSize);

            _mousepointer = new List<DrawPolyline2D>();
        }

        public SimpleBigImage2.SBImage IMAGE
        {
            get
            {
                return _image;
            }
        }

        public long WIDTH
        {
            get
            {
                return _image.Width;
            }
        }

        public long HEIGHT
        {
            get
            {
                return _image.Height;
            }
        }

        public TextureManagement TEXTUREMANAGEMENT
        {
            get
            {
                return _texmgmt;
            }
        }

        public ContourManagement CONTOURMANAGEMENT
        {
            get
            {
                return _cntmgmt;
            }
        }

        public GUIManagement GUIMANAGEMENT
        {
            get
            {
                return _guimgmt;
            }
        }

        public List<DrawPolyline2D> MOUSEPOINTER
        {
            get
            {
                return _mousepointer;
            }
        }

        public void changeContour(List<DrawPolyline2D> toRemove, List<DrawPolyline2D> toAdd)
        {
            if (toRemove != null)
            {
                _cntmgmt.changeContour(toRemove, toAdd);
            }
            else
            {
                _cntmgmt = null;
            }
        }

        public void changeContour(DrawPolyline2D toRemove, DrawPolyline2D toAdd)
        {
            if (toRemove != null)
            {
                _cntmgmt.changeContour(toRemove, toAdd);
            }
            else
            {
                _cntmgmt = null;
            }
        }

        public void changeContour(List<DrawPolyline2D> toRemove, Contours toAdd, Color clr)
        {
            if (_cntmgmt != null)
            {
                List<DrawPolyline2D> polyToAdd = new List<DrawPolyline2D>();
                foreach (Contour c in toAdd.LIST)
                {
                    polyToAdd.Add(new DrawPolyline2D(c, clr));
                }
                _cntmgmt.changeContour(toRemove, polyToAdd);
            }
        }

        public void smeltOntoContours(List<IntPoint> cont, Color defaultColor)
        {
            if (cont != null)
            {
                if (_cntmgmt != null)
                {
                    List<DrawPolyline2D> toAdd = new List<DrawPolyline2D>();
                    List<DrawPolyline2D> toRemove = //new List<DrawPolyline2D>();
                    
                    _cntmgmt.getCollidingPolylines(cont, out toAdd, defaultColor);

                    _cntmgmt.changeContour(toRemove, toAdd);
                }
            }
        }

        public List<DrawPolyline2D> addContours(Contours cont, Color defaultColor)
        {
            if (cont != null)
            {
                if (_cntmgmt == null)
                {
                    _cntmgmt = new ContourManagement(cont, defaultColor, _image, _image.Tilesize);
                }
                List<DrawPolyline2D> lst = new List<DrawPolyline2D>();
                foreach (Contour c in cont.LIST)
                {
                    lst.Add(new DrawPolyline2D(c, defaultColor));
                }
                _cntmgmt.attachPolylines(lst);
                return lst;
            }
            return new List<DrawPolyline2D>();
        }

        public void setContours(Contours cont, Color defaultColor)
        {
            if (cont != null)
            {
                _cntmgmt = new ContourManagement(cont, defaultColor, _image, _image.Tilesize);
                _cntmgmt.update();
            }
            else
            {
                _cntmgmt = null;
            }
        }

        public void setGUI(List<GUIElementInterface> guiElements)
        {
            if (guiElements != null)
            {
                _guimgmt = new GUIManagement(guiElements);
                //_guimgmt.setGUI(guiElements);
            }
            else
            {
                _guimgmt = null;
            }
        }

        public void Dispose()
        {
            _image.Dispose();
            _image = null;
        }
    }
}
