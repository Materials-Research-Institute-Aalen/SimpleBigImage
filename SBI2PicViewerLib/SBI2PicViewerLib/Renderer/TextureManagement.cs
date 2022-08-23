using Cornerstones.Poly2DMath;
using SBI2PicViewerLib.Geom;
using SimpleBigImage2;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SBI2PicViewerLib.Renderer
{
    class TextureManagement
    {
        private List<long[]> _currentlyRendering;

        private TextureElement[] _renderList;
        private int _iTileSize;
        private SBImage _img;

        private int _iXStart;
        private int _iYStart;
        private int _iZStart;

        private int _iXEnd;
        private int _iYEnd;
        private int _iZEnd;


        public TextureManagement(SBImage img, int iTileSize)
        {
            _iTileSize = iTileSize;
            _img = img;
        }

        public float getCurrentTileSize(float fTileSize, int iDepth)
        {
            float fSize = (float)(fTileSize * Math.Pow(2, (iDepth)));
            return fSize;
        }

        private RectangleF getRect(int tileXPos, int tileYPos, int iDepth)
        {
            float fSize = getCurrentTileSize(_iTileSize, iDepth);

            return new RectangleF(tileXPos * fSize, tileYPos * fSize, fSize, fSize);
        }

        private bool isInZRange(TextureElement input)
        {
            return (input.DEPTH <= _iZEnd && input.DEPTH >= _iZStart);
        }

        private void getElementsToRender(Camera cam)
        {
            float xc = 0;
            float yc = 0;
            float xc2 = 0;
            float yc2 = 0;
            cam.getBounds(out xc, out xc2, out yc, out yc2);
            float zoom = cam.getZoom();

            usedRectangle(xc, xc2, yc, yc2, zoom);

            List<TextureElement> elements = new List<TextureElement>();
            if (_renderList != null)
            {
                elements.AddRange(_renderList);
            }

            List<TextureElement> toDelete = new List<TextureElement>();
            List<TextureElement> toAdd = new List<TextureElement>();

            foreach (TextureElement texElm in elements)
            {
                if (!cam.doesOverlap(texElm.ROI) || !isInZRange(texElm))
                {
                    toDelete.Add(texElm);
                }
            }

            //Delete what's not needed
            int iDeleteCount = 0;
            foreach (TextureElement elm in toDelete)
            {
                elements.Remove(elm);
                elm.deactivateTexture();
                iDeleteCount++;
            }

            int iAddCount = 0;
            _currentlyRendering = new List<long[]>();
            for (int x = _iXStart; x <= _iXEnd; x++)
            {
                for (int y = _iYStart; y <= _iYEnd; y++)
                {
                    for (int z = _iZStart; z <= _iZEnd; z++)
                    {
                        if (cam.doesOverlap(getRect(x, y, z)))
                        {
                            float fTileSize = _iTileSize * (float)Math.Pow(2, z);
                            int tileX = x * (int)fTileSize;
                            int tileY = y * (int)fTileSize;

                            _currentlyRendering.Add(new long[]{x,y,z});

                            bool blnDraw = true;

                            foreach (TextureElement elm in elements)
                            {
                                if (elm.isAt(tileX, tileY, z))
                                {
                                    blnDraw = false;
                                }
                            }

                            if (blnDraw)
                            {
                                iAddCount++;
                                
                                Image tileImg = _img.getTile(x, y, z).getImage(0);
                                Bitmap tileBitmap = new Bitmap(tileImg);
                                toAdd.Add(new TextureElement(tileBitmap, tileX, tileY, fTileSize, z));
                                tileBitmap.Dispose();
                                tileBitmap = null;
                                tileImg.Dispose();
                                tileImg = null;
                            }
                        }
                    }
                }
            }

            elements.AddRange(toAdd);

            _renderList = elements.ToArray();

            List<DrawPolyline2D> contourList = new List<DrawPolyline2D>();

        }

        private IntRect? collide(IntRect a, IntRect b)
        {
            long left = a.left > b.left ? a.left : b.left;
            long top = a.top > b.top ? a.top : b.top;
            long right = a.right < b.right ? a.right : b.right;
            long bottom = a.bottom < b.bottom ? a.bottom : b.bottom;


            if (right - left <= 0)
            {
                return null;
            }
            if (bottom - top <= 0)
            {
                return null;
            }

            return new IntRect(left, top, right, bottom);
        }
        /*
        private void getContoursToRender(Camera cam)
        {
            float xc = 0;
            float yc = 0;
            float xc2 = 0;
            float yc2 = 0;
            cam.getBounds(out xc, out xc2, out yc, out yc2);
            float zoom = cam.getZoom();

            List<DrawPolyline2D> drawList = new List<DrawPolyline2D>();

            if (_contours.LIST.Count == 0)
            {
                _polygonRenderer = null;
                return;
            }

            IntRect camRect = new IntRect((long)Math.Floor(xc), (long)Math.Floor(yc), (long)Math.Ceiling(xc2), (long)Math.Ceiling(yc2));

            foreach (Contour s in _contours.LIST)
            {
                if (collide(camRect, s.GET_RECT) != null)
                {
                    float width = s.GET_RECT.right - s.GET_RECT.right;
                    float height = s.GET_RECT.bottom - s.GET_RECT.top;

                    float size = width > height ? width : height;

                    size *= zoom;

                    if (size > 5 && s.AREA != 0)
                    {
                        DrawPolyline2D toDraw = new DrawPolyline2D(s, Color.Blue);
                        drawList.Add(toDraw);
                    }
                }
            }

            _polygonRenderer = drawList.ToArray();
        }
        */
        private void usedRectangle(float startX, float endX, float startY, float endY, float zoom)
        {
            int iCurrentDepth = (int)Math.Round((1.0f / (Math.Sqrt(zoom))));
            if (iCurrentDepth < 0)
            {
                iCurrentDepth = 0;
            }
            if (iCurrentDepth >= _img.Depth)
            {
                iCurrentDepth = _img.Depth - 1;
            }

            float fActualTileSize = getCurrentTileSize(_iTileSize, iCurrentDepth);

            int iStartX = (int)(startX / fActualTileSize);
            int iEndX = (int)(endX / fActualTileSize);
            int iStartY = (int)(startY / fActualTileSize);
            int iEndY = (int)(endY / fActualTileSize);
            usedRectangle(iStartX, iEndX, iStartY, iEndY, iCurrentDepth);
        }

        private void usedRectangle(int iStartX, int iEndX, int iStartY, int iEndY, int iCurrentDepth)
        {
            if (iCurrentDepth >= _img.Depth)
            {
                iCurrentDepth = _img.Depth - 1;
            }
            if (iCurrentDepth < 0)
            {
                iCurrentDepth = 0;
            }

            int iCurrXSize = _img.getXSize(iCurrentDepth);
            int iCurrYSize = _img.getYSize(iCurrentDepth);

            int iWidth = iEndX - iStartX;
            int iHeight = iEndY - iStartY;

            _iXStart = iStartX - iWidth / 3;
            _iXStart = (_iXStart < 1 ? 0 : _iXStart);
            _iXEnd = iEndX + iWidth / 3;
            _iXEnd = (_iXEnd >= iCurrXSize ? iCurrXSize - 1 : _iXEnd);

            _iYStart = iStartY - iWidth / 3;
            _iYStart = (_iYStart < 1 ? 0 : _iYStart);
            _iYEnd = iEndY + iWidth / 3;
            _iYEnd = (_iYEnd >= iCurrYSize ? iCurrYSize - 1 : _iYEnd);

            _iZStart = iCurrentDepth - 0;
            _iZStart = (_iZStart < 1 ? 0 : _iZStart);
            _iZEnd = iCurrentDepth + 0;
            _iZEnd = (_iZEnd >= _img.Depth ? _img.Depth - 1 : _iZEnd);
            _iZStart = (_iZStart >= _img.Depth ? _img.Depth - 1 : _iZStart);
        }

        public void render(Camera cam)
        {
            getElementsToRender(cam);
            if (_renderList != null)
            {
                foreach (TextureElement elm in _renderList)
                {
                    elm.render();
                }
            }
        }

        public List<long[]> CURRENTLY_RENDERING
        {
            get
            {
                return _currentlyRendering;
            }
        }
    }
}
