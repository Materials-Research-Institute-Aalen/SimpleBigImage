using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace SimpleImageProcessing
{
    /// <summary>
    /// Copies an Image (with ROI) onto another image (with ROI) while resizing it, not using EmguCV
    /// </summary>
    public class ImageCopier
    {
        Image _data;

        /////////////////////////
        /// <summary>
        /// Clones an image part, resizes it
        /// </summary>
        /// <param name="input">The Input Image</param>
        /// <param name="xSrc">The source image part x pos</param>
        /// <param name="ySrc">The source image part y pos</param>
        /// <param name="wSrc">The source image part width</param>
        /// <param name="hSrc">The source image part height</param>
        /// <param name="xTrg">The target image xPos (normally 0)</param>
        /// <param name="yTrg">The target image yPos (normally 0)</param>
        /// <param name="wTrg">The target image target width</param>
        /// <param name="hTrg">The target image target height</param>
        /// <returns></returns>
        private Image createNewImageFromCoords(Image input, int xSrc, int ySrc, int wSrc, int hSrc, int xTrg, int yTrg, int wTrg, int hTrg)
        {
            if (_data == null)
            {
                return null;
            }

            // An empty bitmap which will hold the cropped image
            if (input == null)
            {
                input = new Bitmap(wTrg, hTrg);
            }
            using (Graphics g = Graphics.FromImage(input))
            {
                Rectangle destRect = new Rectangle(xTrg, yTrg, wTrg, hTrg);
                Rectangle srcRect = new Rectangle(xSrc, ySrc, wSrc, hSrc);
                g.DrawImage(_data, destRect, srcRect, GraphicsUnit.Pixel);
            }
            return input;
        }
        /////////////////////////

        /// <summary>
        /// The constructor of the class; With the Image to process as input.
        /// </summary>
        /// <param name="input">This image gets processed by all class functions</param>
        public ImageCopier(Image input)
        {
            _data = input;
        }

        /// <summary>
        /// Creates a new image as a clone from the class's image, and crops the clone.
        /// </summary>
        /// <param name="xSrc"></param>
        /// <param name="ySrc"></param>
        /// <param name="wSrc"></param>
        /// <param name="hSrc"></param>
        /// <returns></returns>
        public Image cropImage(int xSrc, int ySrc, int wSrc, int hSrc)
        {
            return createNewImageFromCoords(null, xSrc, ySrc, wSrc, hSrc, 0, 0, wSrc, hSrc);
        }

        /// <summary>
        /// Creates a new image as a clone from the class's image, and 
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        public Image resize(int width, int height)
        {
            if (_data != null)
            {
                return createNewImageFromCoords(null, 0, 0, _data.Width, _data.Height, 0, 0, width, height);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Creates a new image as a clone from the class's image, and 
        /// </summary>
        /// <param name="result"></param>
        /// <param name="src"></param>
        /// <param name="dst"></param>
        /// <returns></returns>
        public Image copyImageOntoImage(Image result, Rectangle src, Rectangle dst)
        {
            return createNewImageFromCoords(result, src.X, src.Y, src.Width, src.Height, dst.X, dst.Y, dst.Width, dst.Height);
        }

        /// <summary>
        /// Creates a new image as a clone from the class's image, and 
        /// </summary>
        /// <returns></returns>
        public Image clone()
        {
            if (_data == null)
            {
                return null;
            }

            Bitmap bmp = null;
            while (bmp == null)
            {
                try
                {
                    bmp = new Bitmap(_data.Width, _data.Height);
                }
                catch { };
            }
            using (Graphics g = Graphics.FromImage(bmp))
            {
                try
                {
                    Rectangle dstRectangle = new Rectangle(0, 0, _data.Width, _data.Height);
                    Rectangle srcRectangle = new Rectangle(0, 0, _data.Width, _data.Height);
                    g.DrawImage(_data, dstRectangle, srcRectangle, GraphicsUnit.Pixel);
                }
                catch
                {
                    return null;
                }
            }
            return bmp;
        }

        ////////////////////////////

        /// <summary>
        /// Creates a new image as a clone from the class's image, and 
        /// </summary>
        /// <param name="input"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        public static Image CROP(Image input, int x, int y, int width, int height)
        {
            ImageCopier copy = new ImageCopier(input);
            return copy.cropImage(x, y, width, height);
        }

        /// <summary>
        /// Creates a new image as a clone from the class's image, and 
        /// </summary>
        /// <param name="input"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        public static Image RESIZE(Image input, int width, int height)
        {
            ImageCopier copy = new ImageCopier(input);
            return copy.resize(width, height);
        }

        /// <summary>
        /// Creates a new image as a clone from the class's image, and 
        /// </summary>
        /// <param name="input"></param>
        /// <param name="target"></param>
        /// <param name="src"></param>
        /// <param name="dst"></param>
        /// <returns></returns>
        public static Image COPY(Image input, Image target, Rectangle src, Rectangle dst)
        {
            ImageCopier copy = new ImageCopier(input);
            return copy.copyImageOntoImage(target, src, dst);
        }

        /// <summary>
        /// Creates a new image as a clone from the class's image, and 
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static Image CLONE(Image input)
        {
            ImageCopier copy = new ImageCopier(input);
            return copy.clone();
        }
    }
}
