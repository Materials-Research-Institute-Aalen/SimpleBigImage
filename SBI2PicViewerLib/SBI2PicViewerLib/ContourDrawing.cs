using Cornerstones.Poly2DMath;
using SBI2PicViewerLib.Geom;
using SimpleImageProcessing;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SBI2PicViewerLib
{
    public class OutputImageContainer
    {
        public Image img;
        public List<Image> imageList;
        public IntRect rect;

        public OutputImageContainer()
        {
            imageList = new List<Image>();
        }

        public void Dispose()
        {
            img?.Dispose();
            foreach (Image elm in imageList)
            {
                elm.Dispose();
                img = null;
            }
        }
    }
    /// <summary>
    /// A Container for drawing contours, mostly to manage contour layers
    /// </summary>
    class ContourDrawingContainer
    {
        //Not Necessairy
        //public List<DrawPolyline2D> _currentlyDrawingContour;
        //Necessairy
        public List<Contours> _allContours;

        /// <summary>
        /// Contour Drawing
        /// </summary>
        public ContourDrawingContainer()
        {
            _allContours = new List<Contours>();
        }

        /// <summary>
        /// Returns the index of a contour at a given point; Making it easier to expand already existing elements
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public int getContourIndexAtImagePoint(float x, float y)
        {
            for (int i = 0; i < _allContours.Count; i++)
            {
                Contours cs = _allContours[i];
                foreach (Contour c in cs.LIST)
                {
                    if (c.isPointInside(x, y))
                    {
                        return i;
                    }
                }
            }
            return 0;
        }

        /// <summary>
        /// Returns an image containing the "sublayer"
        /// </summary>
        /// <param name="area"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        public Image getSublayer(SimpleBigImage2.SBImage img, IntRect rectangle, double scaling, bool blnWithHoles, Contour main)
        {
            Image result = new Bitmap((int)(rectangle.right - rectangle.left), (int)(rectangle.bottom - rectangle.top));
                /*
                              img.getImagePartResized(rectangle.left,
                              rectangle.top,
                              rectangle.right - rectangle.left,
                              rectangle.bottom - rectangle.top,
                              1 / scaling);
            */
            Graphics g = Graphics.FromImage(result);
            SolidBrush brush = new SolidBrush(ContourDrawingConfiguration.BACKGROUND_COLOR);
            g.FillRegion(brush, new Region(new Rectangle(0, 0, result.Width, result.Height)));

            g.Flush();

            double dblXPos = rectangle.left;
            double dblYPos = rectangle.top;
            double dblWidth = rectangle.right - rectangle.left;
            double dblHeight = rectangle.bottom - rectangle.top;

            for (int i = 0; i < _allContours.Count; i++)
            {
                Contours cs = _allContours[i];
                foreach (Contour cont in cs.LIST)
                {
                    if (cont.isROIOverlapping(main.GET_RECT))
                    //if (cont.collidesWith(main))
                    {
                        if (!(cont.IS_HOLE && blnWithHoles))
                        {
                            cont.drawOntoImage(result, true, ContourDrawingConfiguration.ELEMENT_COLOR, -(int)dblXPos, -(int)dblYPos, scaling, scaling);
                        }
                        if (cont.IS_HOLE && blnWithHoles)
                        {
                            cont.drawOntoImage(result, true, ContourDrawingConfiguration.HOLE_COLOR, -(int)dblXPos, -(int)dblYPos, scaling, scaling);
                        }
                        //return result;
                    }
                }
            }
            return result;
        }


        public List<OutputImageContainer> getCurrentLayerImages(List<ContourDrawingContainer> subLayers, int iMaxSize, SimpleBigImage2.SBImage img, bool blnWithHoles = true)
        {
            List<OutputImageContainer> output = new List<OutputImageContainer>();
            foreach (Contours c in _allContours)
            {
                try
                {
                    OutputImageContainer container = new OutputImageContainer();
                    container.rect = c.getRect();
                    IntRect rectangle = c.getRect();

                    double dblXPos = rectangle.left;
                    double dblYPos = rectangle.top;
                    double dblWidth = rectangle.right - rectangle.left;
                    double dblHeight = rectangle.bottom - rectangle.top;

                    double xScaling = dblWidth > iMaxSize ? iMaxSize / dblWidth : 1;
                    double yScaling = dblHeight > iMaxSize ? iMaxSize / dblHeight : 1;

                    double scaling = yScaling < xScaling ? yScaling : xScaling;

                    if (scaling < 1.0)
                    {
                        dblWidth *= scaling;
                        dblHeight *= scaling;
                    }

                    if (dblWidth < 1 || dblHeight < 1)
                    {
                        break;
                    }

                    if (rectangle.left < 0)
                    {
                        rectangle.left = 0;
                    }
                    if (rectangle.top < 0)
                    {
                        rectangle.top = 0;
                    }

                    if (rectangle.right > img.Width)
                    {
                        rectangle.right = img.Width - 1;
                    }
                    if (rectangle.bottom > img.Height)
                    {
                        rectangle.top = img.Height - 1;
                    }

                    if (rectangle.right - rectangle.left < 1)
                    {
                        output.Add(new OutputImageContainer());
                        continue;
                    }
                    if (rectangle.bottom - rectangle.top < 1)
                    {
                        output.Add(new OutputImageContainer());
                        continue;
                    }


                    using (Image part = img.getImagePartResized(rectangle.left,
                                                  rectangle.top,
                                                  rectangle.right - rectangle.left,
                                                  rectangle.bottom - rectangle.top,
                                                  1 / scaling))
                    {
                        using (Image backGroundImage = ImageCopier.CLONE(part))
                        {
                            Graphics g = Graphics.FromImage(backGroundImage);
                            SolidBrush brush = new SolidBrush(ContourDrawingConfiguration.BACKGROUND_COLOR);
                            g.FillRegion(brush, new Region(new Rectangle(0, 0, backGroundImage.Width, backGroundImage.Height)));
                            g.Flush();

                            Contour mainContour = null;
                            foreach (Contour cont in c.LIST)
                            {
                                if (!(cont.IS_HOLE && blnWithHoles))
                                {
                                    mainContour = cont;
                                    cont.drawOntoImage(backGroundImage, true, ContourDrawingConfiguration.ELEMENT_COLOR, -(int)dblXPos, -(int)dblYPos, scaling, scaling);
                                }
                            }
                            foreach (Contour cont in c.LIST)
                            {
                                if (cont.IS_HOLE && blnWithHoles)
                                {
                                    cont.drawOntoImage(backGroundImage, true, ContourDrawingConfiguration.HOLE_COLOR, -(int)dblXPos, -(int)dblYPos, scaling, scaling);
                                }
                            }

                            if (!ContourDrawingConfiguration.ORIGINAL_PLUS)
                            {
                                using (Image result = BasicWDIFunctions.binaryAnd(part, backGroundImage))
                                {
                                    //output.Add(result);
                                    container.img = SimpleImageProcessing.ImageCopier.CLONE(result);
                                }
                            }
                            else
                            {
                                container.img = ImageCopier.CLONE(part);
                                container.imageList.Add(ImageCopier.CLONE(backGroundImage));
                                //output.Add(part);
                                //output.Add(backGroundImage);
                            }
                            for (int i = 1; i < subLayers.Count; i++)
                            {
                                ContourDrawingContainer layer = subLayers[i];
                                using (Image subImage = layer.getSublayer(img, rectangle, scaling, blnWithHoles, mainContour))
                                {
                                    container.imageList.Add(ImageCopier.CLONE(subImage));
                                }
                                //output.Add(layer.getSublayer(img, rectangle, scaling, blnWithHoles, mainContour));
                            }

                            output.Add(container);
                        }
                    }
                }
                catch (Exception ex)
                {

                }
            }
            return output;
        }
    }

    /// <summary>
    /// This class manages manual drawing of contours
    /// </summary>
    class ContourDrawing
    {
        GLControlInterface _control;
        ContourDrawSupportClass _mouseDrawSupport;
        List<DrawPolyline2D> _currentlyDrawingContour;

        ContourDrawingContainer _container;
        List<ContourDrawingContainer> _layers;
        List<Contours> _toRemove;

        public static long iMaxSize = 2048;

        /// <summary>
        /// The Constructor
        /// </summary>
        /// <param name="control">The GLControlInterface to draw on</param>
        public ContourDrawing(GLControlInterface control)
        {
            _mouseDrawSupport = new ContourDrawSupportClass();
            _control = control;
            _container = new ContourDrawingContainer();
            _layers = new List<ContourDrawingContainer>();
            _layers.Add(_container);
            _toRemove = new List<Contours>();
        }




        ////////////////////////////////////////////////////////////////////////////////////




        /// <summary>
        /// Actually more of an event than a function. Please do not trigger from outside
        /// </summary>
        /// <param name="colr">The Color</param>
        public void mouseDrawOnMouseDown(Color colr)
        {
            float x, y;
            _control.getMousePicPosition(out x, out y);
            Contours cont = _mouseDrawSupport.getContoursToAddOnMouseDown(_control.CONTAINER, x, y);
            List<DrawPolyline2D> lst = _control.addContours(cont, colr);
            _currentlyDrawingContour = new List<DrawPolyline2D>();
            _currentlyDrawingContour.AddRange(lst);
        }

        /// <summary>
        /// Only needed if some rectangles get loaded via CSV
        /// </summary>
        /// <param name="toAdd"></param>
        /// <param name="iLayer"></param>
        public void addCurrentlyShownContours(Contours toAdd, int iLayer = 0)
        {
            _layers[iLayer]._allContours.Add(toAdd);
        }

        /// <summary>
        /// Actually more of an event than a function. Please do not trigger from outside
        /// </summary>
        /// <param name="colr">The color after everything is put together</param>
        /// <param name="iLayer">The layer the final result should be put in</param>
        public void mouseDrawOnMouseUp(Color colr, int iLayer)
        {
            float x, y;
            _control.getMousePicPosition(out x, out y);


            List<Contour> result = new List<Contour>();
            for (int i = 0; i < _currentlyDrawingContour.Count; i++)
            {
                result.Add(_currentlyDrawingContour[i].CONTOUR);
            }

            Contours toDraw = new Contours(result);

            if (_control.CONTAINER != null)
            {
                Contours actual = toDraw.smelt();
                _control.CONTAINER.changeContour(_currentlyDrawingContour, actual, colr);
                _mouseDrawSupport.onMouseUp(actual);

                if (iLayer >= 0)
                {
                    //If there is a lower layer, we can attach another layer
                    while (_layers.Count <= iLayer)
                    {
                        _layers.Add(new ContourDrawingContainer());
                    }
                    if (_layers.Count > iLayer)
                    {
                        _layers[iLayer]._allContours.Add(actual);
                    }
                    _toRemove = new List<Contours>();
                }
                else
                {
                    _toRemove.Add(actual);
                }
            }
            _currentlyDrawingContour = new List<DrawPolyline2D>();
        }

        /// <summary>
        /// Actually more of an event than a function. Please do not trigger from outside
        /// </summary>
        /// <param name="colr"></param>
        public void mouseDrawOnMove(Color colr)
        {
            float x, y;
            _control.getMousePicPosition(out x, out y);

            if (!ContourDrawingConfiguration.drawOnMouseHold)
            {
                _currentlyDrawingContour = new List<DrawPolyline2D>();
            }
            else
            {
                Contours cont = _mouseDrawSupport.getContoursToAddOnMove(_control.CONTAINER, x, y);
                List<DrawPolyline2D> lst = _control.addContours(cont, colr);
                _currentlyDrawingContour.AddRange(lst);
            }
            
        }


        //////////////////////////////////////////////////////////////////////////


        /// <summary>
        /// Adding the Contours to the Interface, and returning the Polyline2D
        /// </summary>
        /// <param name="control">The Control</param>
        /// <param name="contours">The Contours</param>
        /// <param name="defaultColor">The default color</param>
        /// <returns>The resulting Polylines on the Control Interface</returns>
        public List<DrawPolyline2D> addContours(GLControlInterface control, Contours contours, Color defaultColor)
        {
            return control.addContours(contours, defaultColor);
        }

        public void subtractDrawnPolylines()
        {
            //ToDo: Add "subtract polygon from polygon" here!
            /*
            foreach (ContourDrawingContainer cont in _layers)
            {
                cont._allContours -= _toRemove;
            }
            */




            _toRemove = new List<Contours>();
        }

        public void removePolylines(List<DrawPolyline2D> input)
        {
            if (_layers != null)
            {
                List<Contour> contourList = new List<Contour>();

                foreach (DrawPolyline2D line in input)
                {
                    contourList.Add(line.CONTOUR);
                }
                


                //_layers
                foreach (ContourDrawingContainer cont in _layers)
                {
                    List<Contours> lstContours = new List<Contours>();

                    foreach (Contours conts in cont._allContours)
                    {
                        foreach (Contour c in conts.LIST)
                        {
                            foreach (Contour showing in contourList)
                            {
                                if (showing == c)
                                {
                                    lstContours.Add(conts);
                                }
                            }
                        }
                    }

                    foreach (Contours cntrs in lstContours)
                    {
                        cont._allContours.Remove(cntrs);
                    }
                }
            }
        }

        /// <summary>
        /// Resets drawn Elements
        /// </summary>
        public void resetDrawnElements()
        {
            _container._allContours = new List<Contours>();
        }

        /// <summary>
        /// Returns an Imagelist of already drawn contours (and resets the internal list)
        /// </summary>
        /// <param name="img">The SBI Image the shapes are drawn on</param>
        /// <param name="blnWithHoles">Do you want to cut out holes, or not?</param>
        /// <returns>A List of Images</returns>
        public List<OutputImageContainer> getDrawnContours(SimpleBigImage2.SBImage img, bool blnWithHoles = true)
        {
            return _container.getCurrentLayerImages(_layers, 2048, img, blnWithHoles);
        }
    }
}
