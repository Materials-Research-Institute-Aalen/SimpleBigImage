using SBI2PicViewerLib.Geom;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SBI2PicViewerLib.Renderer
{
    /// <summary>
    /// This class 
    /// </summary>
    class ContourTileListElement
    {
        public List<DrawPolyline2D> POLYLINES = new List<DrawPolyline2D>();
        private long _TileXPos;
        private long _TileYPos;
        private long _iDepth;
        private RectangleF _drawRect;

        public ContourTileListElement(long tileXPos, long tileYPos, long iDepth, double xPos, double yPos, double fSize)
        {
            _TileXPos = tileXPos;
            _TileYPos = tileYPos;
            _iDepth = iDepth;
            _drawRect = new RectangleF((float)xPos, (float)yPos, (float)fSize, (float)fSize);
        }

        public void setPolygons(List<DrawPolyline2D> input)
        {
            POLYLINES = new List<DrawPolyline2D>();
            foreach (DrawPolyline2D elm in input)
            {
                if (elm != null)
                {
                    POLYLINES.Add(elm);
                }
            }
        }
        
        public void render(Camera cam)
        {
            foreach (DrawPolyline2D poly in POLYLINES)
            {
                poly.render(cam.getZoom());
            }
        }

        public RectangleF ROI
        {
            get
            {
                return _drawRect;
            }
        }

        public bool toRender(List<long[]> currently_rendering)
        {
            foreach (long[] element in currently_rendering)
            {
                if (element[0] == _TileXPos && element[1] == _TileYPos && element[2] == _iDepth)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
