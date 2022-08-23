using Cornerstones.Poly2DMath;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SimpleImageProcessing
{
    public class BasicWDIFunctions
    {
        /// <summary>
        /// Makes the Image an one dimensional array
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        public static byte[] makeImage1DByteArray(Image image, out int stride)
        {
            Image toWorkWith = ImageCopier.CLONE(image);

            System.Drawing.Bitmap bmp = new Bitmap(toWorkWith);
            Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
            System.Drawing.Imaging.BitmapData bmpData = bmp.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadOnly, image.PixelFormat);
            IntPtr ptr = bmpData.Scan0;
            int bytes = Math.Abs(bmpData.Stride) * bmp.Height;
            stride = bmpData.Stride;
            byte[] rgbValues = new byte[bytes];
            Marshal.Copy(ptr, rgbValues, 0, bytes);
            bmp.UnlockBits(bmpData);

            bmp.Dispose();
            toWorkWith.Dispose();

            return rgbValues;
        }

        /// <summary>
        /// Makes an image 
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        public static byte[,,] makeImage3DByteArray(Image image)
        {
            int iWidth = image.Width;
            int iHeight = image.Height;

            int stride;
            byte[] data = makeImage1DByteArray(image, out stride);

            int iPixelByteSize = data.Length / iWidth / iHeight;

            byte[, ,] output = new byte[iWidth, iHeight, iPixelByteSize];


            for (int x = 0; x < iWidth; x++)
            {
                for (int y = 0; y < iHeight; y++)
                {
                    for (int p = 0; p < iPixelByteSize; p++)
                    {
                        int iPos = p + x * iPixelByteSize + y * stride;// * iPixelByteSize;
                        output[x, y, p] = data[iPos];
                    }
                }
            }

            return output;
        }

        /// <summary>
        /// Gets the stride
        /// </summary>
        /// <param name="width"></param>
        /// <param name="pxFormat"></param>
        /// <returns></returns>
        public static int GetStride(int width, PixelFormat pxFormat)
        {
            //float bitsPerPixel = System.Drawing.Image.GetPixelFormatSize(format);
            int bitsPerPixel = ((int)pxFormat >> 8) & 0xFF;
            //Number of bits used to store the image data per line (only the valid data)
            int validBitsPerLine = width * bitsPerPixel;
            //4 bytes for every int32 (32 bits)
            int stride = ((validBitsPerLine + 31) / 32) * 4;
            return stride;
        }

        /// <summary>
        /// Creates an Image from a 1D Array
        /// </summary>
        /// <param name="array"></param>
        /// <param name="format"></param>
        /// <param name="iWidth"></param>
        /// <param name="iHeight"></param>
        /// <returns></returns>
        public static Image makeImage(byte[] array, PixelFormat format, int iWidth, int iHeight)
        {
            int iStride = GetStride(iWidth, format);
            int bytes = iStride * iHeight; //28620 vs 28800, 159 vs 160 bzw. auf int32 "runden"
            if (bytes != array.Length)
            {
                throw new Exception("Data length wrong! Is the PixelFormat correct?");
            }
            GCHandle handle = GCHandle.Alloc(array, GCHandleType.Pinned);
            Bitmap bmp = new Bitmap(iWidth, iHeight, iStride, format, handle.AddrOfPinnedObject());

            Image output = ImageCopier.CLONE(bmp);

            bmp.Dispose();
            handle.Free();

            return output;
        }

        public static Bitmap convertIndexedToUseable(Bitmap original, System.Drawing.Imaging.PixelFormat format = System.Drawing.Imaging.PixelFormat.Format32bppArgb)
        {
            if (original == null)
            {
                return null;
            }

            Bitmap tempBitmap2 = new Bitmap(original.Width, original.Height, format);
            using (Graphics g = Graphics.FromImage(tempBitmap2))
            {
                g.DrawImage(original, 0, 0, original.Width, original.Height);
            }
            original = tempBitmap2;
            return original;
        }

        /// <summary>
        /// A Binary AND for images
        /// </summary>
        /// <param name="img1"></param>
        /// <param name="img2"></param>
        /// <returns></returns>
        public static Image overlayMask(Bitmap img1, Bitmap img2)
        {
            Image toUse1 = convertIndexedToUseable(img1);
            Image toUse2 = convertIndexedToUseable(img2);

            byte[,,] image1 = makeImage3DByteArray(toUse1);
            byte[,,] image2 = makeImage3DByteArray(toUse2);

            toUse1.Dispose();
            toUse2.Dispose();

            for (int x = 0; x < image1.GetLength(0) && x < image2.GetLength(0); x++)
            {
                for (int y = 0; y < image1.GetLength(1) && y < image2.GetLength(1); y++)
                {
                    for (int d = 0; d < image1.GetLength(2) && d < image2.GetLength(2); d++)
                    {
                        image1[x, y, d] = (byte)(image1[x, y, d]/2 + image2[x, y, d]/2);
                    }
                }
            }

            return makeImage(image1, img1.PixelFormat);
        }

        /// <summary>
        /// A Binary AND for images
        /// </summary>
        /// <param name="img1"></param>
        /// <param name="img2"></param>
        /// <returns></returns>
        public static Image binaryAnd(Image img1, Image img2)
        {
            byte[, ,] image1 = makeImage3DByteArray(img1);
            byte[, ,] image2 = makeImage3DByteArray(img2);

            for (int x = 0; x < image1.GetLength(0); x++)
            {
                for (int y = 0; y < image1.GetLength(1); y++)
                {
                    for (int d = 0; d < image1.GetLength(2); d++)
                    {
                        image1[x, y, d] &= image2[x, y, d];
                    }
                }
            }

            return makeImage(image1, img1.PixelFormat);
        }

        /// <summary>
        /// Makes a 3D Array an Image
        /// </summary>
        /// <param name="array"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        public static Image makeImage(byte[, ,] array, PixelFormat format)
        {
            int iWidth = array.GetLength(0);
            int iHeight = array.GetLength(1);
            int iPixelByteSize = array.GetLength(2);

            //Hier einschreiten und die iWidth durch 4 teilbar machen
            //int iCompatibleWidth = GetStride(iWidth, format) / iPixelByteSize;
            int iStride = GetStride(iWidth, format);
            byte[] data = new byte[iStride * iHeight];

            int imageBPP = ((int)format >> 8) & 0xFF;
            int imageBytePP = imageBPP / 8;
            int iChannelJumps = 0;

            if (imageBytePP != iPixelByteSize)
            {
                iChannelJumps = imageBytePP - iPixelByteSize;
                if (iChannelJumps < 0)
                {
                    iPixelByteSize = imageBytePP;
                    iChannelJumps = 0;
                }
            }

            Parallel.For(0, iWidth, x =>
            //for (int x = 0; x < iWidth; x++)
            {
                for (int y = 0; y < iHeight; y++)
                {
                    for (int p = 0; p < iPixelByteSize; p++)
                    {
                        //int iPos = p + x * iPixelByteSize + y * iWidth * iPixelByteSize;
                        int iChannelPos = p;
                        for (int jumps = 0; jumps <= iChannelJumps; jumps++)
                        {
                            int iPos = iChannelPos + jumps + x * iPixelByteSize + y * iStride;
                            data[iPos] = array[x, y, p];
                        }
                    }
                }
            });

            return makeImage(data, format, iWidth, iHeight);
        }
    }
}
