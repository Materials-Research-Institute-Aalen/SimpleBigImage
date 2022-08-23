// This is a conversion to C# of the algorithm which is implemented at:
// https://www.eriksmistad.no/moore-neighbor-contour-tracing-algorithm-in-c/
// http://www.imageprocessingplace.com/downloads_V3/root_downloads/tutorials/contour_tracing_Abeer_George_Ghuneim/moore.html

using System;
using System.Collections.Generic;
using System.Linq;
// The System.Drawing namespace defines types like Bitmap and IntPoint
using System.Drawing;
using System.Drawing.Imaging;

namespace Cornerstones.Poly2DMath
{
    /// <summary>
    /// Traces an image for polygons. Since this class is external, I cannot give clear information about each function.
    /// </summary>
    public class BoundaryTracing
    {
        Pixels _usedPixels;      

        private BitmapData getBitmapData(Bitmap image, bool isReadOnly)
        {
            Rectangle rect = new Rectangle(new Point(0, 0), image.Size);
            return image.LockBits(rect, (isReadOnly) ? ImageLockMode.ReadOnly : ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
        }

        private Pixels2 create(Bitmap image)
        {
            BitmapData bitmapData = getBitmapData(image, false);
            return new Pixels2(image, bitmapData);
        }

        private Pixels create_base(Bitmap image)
        {
            BitmapData bitmapData = getBitmapData(image, true);
            Pixels rc = new Pixels(bitmapData);
            image.UnlockBits(bitmapData);
            return rc;
        }

        private Pixels create(string inputPath)
        {
            using (Bitmap image = new Bitmap(inputPath))
            {
                BitmapData bitmapData = getBitmapData(image, true);
                Pixels rc = new Pixels(bitmapData);
                image.UnlockBits(bitmapData);
                return rc;
            }
        }


        /// <summary>
        /// The constructor; Needs an Input Image
        /// </summary>
        /// <param name="input"></param>
        public BoundaryTracing(Bitmap input)
        {
            _usedPixels = create_base(input);
        }

        public bool holeAt(IntPoint p, int r, int g, int b, int a)
        {
            return _usedPixels.isNotBrighterThen(p, r, g, b, a);
        }

        public List<List<IntPoint>> traceByAlpha()
        {
            return createByAlpha(_usedPixels);
        }

        public List<List<IntPoint>> traceByValue(int r, int g, int b, int a, bool blnInverted = false)
        {
            return createByValue(_usedPixels, r, g, b, a, blnInverted);
        }

        public List<List<IntPoint>> traceByRange(int r, int g, int b, int a, int rb, int gb, int bb, int ab, bool blnInverted = false)
        {
            return createByRange(_usedPixels, r, g, b, a, rb, gb, bb, ab, blnInverted);
        }

        // This gets all boundaries in the given pixels.
        // It assumes you're looking for boundaries between non-transparent shapes on a transparent background
        // (using the isTransparent property);
        // but you could modify this, to pass in a predicate to say what background color you're looking for (e.g. White).
        private List<List<IntPoint>> createByAlpha(Pixels pixels)
        {
            Size size = pixels.size;
            HashSet<IntPoint> found = new HashSet<IntPoint>();
            List<IntPoint> list = null;
            List<List<IntPoint>> lists = new List<List<IntPoint>>();
            bool inside = false;

            // Defines the neighborhood offset position from current position and the neighborhood
            // position we want to check next if we find a new border at checkLocationNr.
            int width = size.Width;
            Tuple<Func<IntPoint, IntPoint>, int>[] neighborhood = new Tuple<Func<IntPoint, IntPoint>, int>[]
            {
                new Tuple<Func<IntPoint, IntPoint>, int>(IntPoint => new IntPoint(IntPoint.X-1,IntPoint.Y), 7),
                new Tuple<Func<IntPoint, IntPoint>, int>(IntPoint => new IntPoint(IntPoint.X-1,IntPoint.Y-1), 7),
                new Tuple<Func<IntPoint, IntPoint>, int>(IntPoint => new IntPoint(IntPoint.X,IntPoint.Y-1), 1),
                new Tuple<Func<IntPoint, IntPoint>, int>(IntPoint => new IntPoint(IntPoint.X+1,IntPoint.Y-1), 1),
                new Tuple<Func<IntPoint, IntPoint>, int>(IntPoint => new IntPoint(IntPoint.X+1,IntPoint.Y), 3),
                new Tuple<Func<IntPoint, IntPoint>, int>(IntPoint => new IntPoint(IntPoint.X+1,IntPoint.Y+1), 3),
                new Tuple<Func<IntPoint, IntPoint>, int>(IntPoint => new IntPoint(IntPoint.X,IntPoint.Y+1), 5),
                new Tuple<Func<IntPoint, IntPoint>, int>(IntPoint => new IntPoint(IntPoint.X-1,IntPoint.Y+1), 5)
            };

            for (int y = 0; y < size.Height; ++y)
            {
                for (int x = 0; x < size.Width; ++x)
                {
                    IntPoint IntPoint = new IntPoint(x, y);
                    // Scan for non-transparent pixel
                    if (found.Contains(IntPoint) && !inside)
                    {
                        // Entering an already discovered border
                        inside = true;
                        continue;
                    }
                    bool isTransparent = pixels.isTransparent(IntPoint);
                    if (!isTransparent && inside)
                    {
                        // Already discovered border IntPoint
                        continue;
                    }
                    if (isTransparent && inside)
                    {
                        // Leaving a border
                        inside = false;
                        continue;
                    }
                    if (!isTransparent && !inside)
                    {
                        lists.Add(list = new List<IntPoint>());

                        // Undiscovered border IntPoint
                        found.Add(IntPoint); list.Add(IntPoint);   // Mark the start pixel
                        int checkLocationNr = 1;  // The neighbor number of the location we want to check for a new border IntPoint
                        IntPoint startPos = IntPoint;      // Set start position
                        int counter = 0;       // Counter is used for the jacobi stop criterion
                        int counter2 = 0;       // Counter2 is used to determine if the IntPoint we have discovered is one single IntPoint

                        // Trace around the neighborhood
                        while (true)
                        {
                            // The corresponding absolute array address of checkLocationNr
                            IntPoint checkPosition = neighborhood[checkLocationNr - 1].Item1(IntPoint);
                            // Variable that holds the neighborhood position we want to check if we find a new border at checkLocationNr
                            int newCheckLocationNr = neighborhood[checkLocationNr - 1].Item2;

                            // Beware that the IntPoint might be outside the bitmap.
                            // The isTransparent method contains the safety check.
                            if (!pixels.isTransparent(checkPosition))
                            {
                                // Next border IntPoint found
                                if (checkPosition == startPos)
                                {
                                    counter++;

                                    // Stopping criterion (jacob)
                                    if (newCheckLocationNr == 1 || counter >= 3)
                                    {
                                        // Close loop
                                        inside = true; // Since we are starting the search at were we first started we must set inside to true
                                        break;
                                    }
                                }

                                checkLocationNr = newCheckLocationNr; // Update which neighborhood position we should check next
                                IntPoint = checkPosition;
                                counter2 = 0;             // Reset the counter that keeps track of how many neighbors we have visited
                                found.Add(IntPoint); list.Add(IntPoint); // Set the border pixel
                            }
                            else
                            {
                                // Rotate clockwise in the neighborhood
                                checkLocationNr = 1 + (checkLocationNr % 8);
                                if (counter2 > 8)
                                {
                                    // If counter2 is above 8 we have traced around the neighborhood and
                                    // therefor the border is a single black pixel and we can exit
                                    counter2 = 0;
                                    list = null;
                                    break;
                                }
                                else
                                {
                                    counter2++;
                                }
                            }
                        }

                    }
                }
            }
            return lists;
        }

        private List<List<IntPoint>> createByValue(Pixels pixels, int r, int g, int b, int a, bool blnInverted = false)
        {
            Size size = pixels.size;
            HashSet<IntPoint> found = new HashSet<IntPoint>();
            List<IntPoint> list = null;
            List<List<IntPoint>> lists = new List<List<IntPoint>>();
            bool inside = false;

            // Defines the neighborhood offset position from current position and the neighborhood
            // position we want to check next if we find a new border at checkLocationNr.
            int width = size.Width;
            Tuple<Func<IntPoint, IntPoint>, int>[] neighborhood = new Tuple<Func<IntPoint, IntPoint>, int>[]
            {
                new Tuple<Func<IntPoint, IntPoint>, int>(IntPoint => new IntPoint(IntPoint.X-1,IntPoint.Y), 7),
                new Tuple<Func<IntPoint, IntPoint>, int>(IntPoint => new IntPoint(IntPoint.X-1,IntPoint.Y-1), 7),
                new Tuple<Func<IntPoint, IntPoint>, int>(IntPoint => new IntPoint(IntPoint.X,IntPoint.Y-1), 1),
                new Tuple<Func<IntPoint, IntPoint>, int>(IntPoint => new IntPoint(IntPoint.X+1,IntPoint.Y-1), 1),
                new Tuple<Func<IntPoint, IntPoint>, int>(IntPoint => new IntPoint(IntPoint.X+1,IntPoint.Y), 3),
                new Tuple<Func<IntPoint, IntPoint>, int>(IntPoint => new IntPoint(IntPoint.X+1,IntPoint.Y+1), 3),
                new Tuple<Func<IntPoint, IntPoint>, int>(IntPoint => new IntPoint(IntPoint.X,IntPoint.Y+1), 5),
                new Tuple<Func<IntPoint, IntPoint>, int>(IntPoint => new IntPoint(IntPoint.X-1,IntPoint.Y+1), 5)
            };

            for (int y = 0; y < size.Height; ++y)
            {
                for (int x = 0; x < size.Width; ++x)
                {
                    IntPoint IntPoint = new IntPoint(x, y);
                    // Scan for non-transparent pixel
                    if (found.Contains(IntPoint) && !inside)
                    {
                        // Entering an already discovered border
                        inside = true;
                        continue;
                    }
                    bool isInteresting;

                    if (blnInverted){
                        isInteresting = pixels.isNotBrighterThen(IntPoint, r, g, b, a);
                    }
                    else
                    {
                        isInteresting = pixels.isBrighterThen(IntPoint, r, g, b, a);
                    }

                    if (!isInteresting && inside)
                    {
                        // Already discovered border IntPoint
                        continue;
                    }
                    if (isInteresting && inside)
                    {
                        // Leaving a border
                        inside = false;
                        continue;
                    }
                    if (!isInteresting && !inside)
                    {
                        lists.Add(list = new List<IntPoint>());

                        // Undiscovered border IntPoint
                        found.Add(IntPoint); list.Add(IntPoint);   // Mark the start pixel
                        int checkLocationNr = 1;  // The neighbor number of the location we want to check for a new border IntPoint
                        IntPoint startPos = IntPoint;      // Set start position
                        int counter = 0;       // Counter is used for the jacobi stop criterion
                        int counter2 = 0;       // Counter2 is used to determine if the IntPoint we have discovered is one single IntPoint

                        // Trace around the neighborhood
                        while (true)
                        {
                            // The corresponding absolute array address of checkLocationNr
                            IntPoint checkPosition = neighborhood[checkLocationNr - 1].Item1(IntPoint);
                            // Variable that holds the neighborhood position we want to check if we find a new border at checkLocationNr
                            int newCheckLocationNr = neighborhood[checkLocationNr - 1].Item2;

                            // Beware that the IntPoint might be outside the bitmap.
                            // The isTransparent method contains the safety check.
                            bool blnValue;
                            if (blnInverted)
                            {
                                blnValue = pixels.isNotBrighterThen(checkPosition, r, g, b, a);
                            }
                            else
                            {
                                blnValue = pixels.isBrighterThen(checkPosition, r, g, b, a);
                            }
                            //if (!pixels.isBrighterThen(checkPosition, r, g, b))
                            if (!blnValue)
                            {
                                // Next border IntPoint found
                                if (checkPosition == startPos)
                                {
                                    counter++;

                                    // Stopping criterion (jacob)
                                    if (newCheckLocationNr == 1 || counter >= 3)
                                    {
                                        // Close loop
                                        inside = true; // Since we are starting the search at were we first started we must set inside to true
                                        break;
                                    }
                                }

                                checkLocationNr = newCheckLocationNr; // Update which neighborhood position we should check next
                                IntPoint = checkPosition;
                                counter2 = 0;             // Reset the counter that keeps track of how many neighbors we have visited
                                found.Add(IntPoint); list.Add(IntPoint); // Set the border pixel
                            }
                            else
                            {
                                // Rotate clockwise in the neighborhood
                                checkLocationNr = 1 + (checkLocationNr % 8);
                                if (counter2 > 8)
                                {
                                    // If counter2 is above 8 we have traced around the neighborhood and
                                    // therefor the border is a single black pixel and we can exit
                                    counter2 = 0;
                                    list = null;
                                    break;
                                }
                                else
                                {
                                    counter2++;
                                }
                            }
                        }

                    }
                }
            }
            return lists;
        }

        private List<List<IntPoint>> createByRange(Pixels pixels, int r, int g, int b, int a, int rb, int gb, int bb, int ab, bool blnInverted = false)
        {
            Size size = pixels.size;
            HashSet<IntPoint> found = new HashSet<IntPoint>();
            List<IntPoint> list = null;
            List<List<IntPoint>> lists = new List<List<IntPoint>>();
            bool inside = false;

            // Defines the neighborhood offset position from current position and the neighborhood
            // position we want to check next if we find a new border at checkLocationNr.
            int width = size.Width;
            Tuple<Func<IntPoint, IntPoint>, int>[] neighborhood = new Tuple<Func<IntPoint, IntPoint>, int>[]
            {
                new Tuple<Func<IntPoint, IntPoint>, int>(IntPoint => new IntPoint(IntPoint.X-1,IntPoint.Y), 7),
                new Tuple<Func<IntPoint, IntPoint>, int>(IntPoint => new IntPoint(IntPoint.X-1,IntPoint.Y-1), 7),
                new Tuple<Func<IntPoint, IntPoint>, int>(IntPoint => new IntPoint(IntPoint.X,IntPoint.Y-1), 1),
                new Tuple<Func<IntPoint, IntPoint>, int>(IntPoint => new IntPoint(IntPoint.X+1,IntPoint.Y-1), 1),
                new Tuple<Func<IntPoint, IntPoint>, int>(IntPoint => new IntPoint(IntPoint.X+1,IntPoint.Y), 3),
                new Tuple<Func<IntPoint, IntPoint>, int>(IntPoint => new IntPoint(IntPoint.X+1,IntPoint.Y+1), 3),
                new Tuple<Func<IntPoint, IntPoint>, int>(IntPoint => new IntPoint(IntPoint.X,IntPoint.Y+1), 5),
                new Tuple<Func<IntPoint, IntPoint>, int>(IntPoint => new IntPoint(IntPoint.X-1,IntPoint.Y+1), 5)
            };

            for (int y = 0; y < size.Height; ++y)
            {
                for (int x = 0; x < size.Width; ++x)
                {
                    IntPoint IntPoint = new IntPoint(x, y);
                    // Scan for non-transparent pixel
                    if (found.Contains(IntPoint) && !inside)
                    {
                        // Entering an already discovered border
                        inside = true;
                        continue;
                    }
                    bool isInteresting;

                    if (!blnInverted)
                    {
                        isInteresting = pixels.isInRange(IntPoint, r, g, b, a, rb, gb, bb, ab);
                    }
                    else
                    {
                        isInteresting = pixels.isOutsideRange(IntPoint, r, g, b, a, rb, gb, bb, ab);
                    }

                    if (!isInteresting && inside)
                    {
                        // Already discovered border IntPoint
                        continue;
                    }
                    if (isInteresting && inside)
                    {
                        // Leaving a border
                        inside = false;
                        continue;
                    }
                    if (!isInteresting && !inside)
                    {
                        lists.Add(list = new List<IntPoint>());

                        // Undiscovered border IntPoint
                        found.Add(IntPoint); list.Add(IntPoint);   // Mark the start pixel
                        int checkLocationNr = 1;  // The neighbor number of the location we want to check for a new border IntPoint
                        IntPoint startPos = IntPoint;      // Set start position
                        int counter = 0;       // Counter is used for the jacobi stop criterion
                        int counter2 = 0;       // Counter2 is used to determine if the IntPoint we have discovered is one single IntPoint

                        // Trace around the neighborhood
                        while (true)
                        {
                            // The corresponding absolute array address of checkLocationNr
                            IntPoint checkPosition = neighborhood[checkLocationNr - 1].Item1(IntPoint);
                            // Variable that holds the neighborhood position we want to check if we find a new border at checkLocationNr
                            int newCheckLocationNr = neighborhood[checkLocationNr - 1].Item2;

                            // Beware that the IntPoint might be outside the bitmap.
                            // The isTransparent method contains the safety check.
                            bool blnValue;
                            if (!blnInverted)
                            {
                                blnValue = pixels.isInRange(checkPosition, r, g, b, a, rb, gb, bb, ab);
                            }
                            else
                            {
                                blnValue = pixels.isOutsideRange(checkPosition, r, g, b, a, rb, gb, bb, ab);
                            }
                            //if (!pixels.isBrighterThen(checkPosition, r, g, b))
                            if (!blnValue)
                            {
                                // Next border IntPoint found
                                if (checkPosition == startPos)
                                {
                                    counter++;

                                    // Stopping criterion (jacob)
                                    if (newCheckLocationNr == 1 || counter >= 3)
                                    {
                                        // Close loop
                                        inside = true; // Since we are starting the search at were we first started we must set inside to true
                                        break;
                                    }
                                }

                                checkLocationNr = newCheckLocationNr; // Update which neighborhood position we should check next
                                IntPoint = checkPosition;
                                counter2 = 0;             // Reset the counter that keeps track of how many neighbors we have visited
                                found.Add(IntPoint); list.Add(IntPoint); // Set the border pixel
                            }
                            else
                            {
                                // Rotate clockwise in the neighborhood
                                checkLocationNr = 1 + (checkLocationNr % 8);
                                if (counter2 > 8)
                                {
                                    // If counter2 is above 8 we have traced around the neighborhood and
                                    // therefor the border is a single black pixel and we can exit
                                    counter2 = 0;
                                    list = null;
                                    break;
                                }
                                else
                                {
                                    counter2++;
                                }
                            }
                        }

                    }
                }
            }
            return lists;
        }
        // This gets the longest boundary (i.e. list of IntPoints), if you don't want all boundaries.
        public List<IntPoint> getIntPoints(List<List<IntPoint>> lists)
        {
            lists.Sort((x, y) => x.Count.CompareTo(y.Count));
            return lists.Last();
        }
    }

    class Pixels
    {
        internal readonly Size size;
        protected readonly int nPixels;
        protected readonly int[] pixels;

        public Pixels(BitmapData bitmapData)
        {
            size = new Size(bitmapData.Width, bitmapData.Height);
            nPixels = size.Width * size.Height;
            pixels = new int[nPixels];
            System.Runtime.InteropServices.Marshal.Copy(bitmapData.Scan0, pixels, 0, nPixels);
        }

        public int this[IntPoint IntPoint]
        {
            get
            {
                int n = ((int)IntPoint.Y * size.Width) + (int)IntPoint.X;
                return pixels[n];
            }
            protected set
            {
                int n = ((int)IntPoint.Y * size.Width) + (int)IntPoint.X;
                pixels[n] = value;
            }
        }

        internal bool contains(IntPoint IntPoint)
        {
            return ((IntPoint.X < 0) || (IntPoint.X >= size.Width) || (IntPoint.Y < 0) || (IntPoint.Y >= size.Height)) ? false : true;
        }

        internal bool isColor(IntPoint IntPoint, Predicate<int> wanted)
        {
            int pixel = this[IntPoint];
            return wanted(pixel);
        }

        internal bool isTransparent(IntPoint IntPoint)
        {
            if (!contains(IntPoint))
                return true;
            return isColor(IntPoint, isTransparent);
        }

        internal bool isOutsideRange(IntPoint IntPoint, int r, int g, int b, int a, int rb, int gb, int bb, int ab)
        {
            return !isInRange(IntPoint, r, g, b, a, rb, gb, bb, ab);
        }

        internal bool isInRange(IntPoint IntPoint, int r, int g, int b, int a, int rb, int gb, int bb, int ab)
        {
            if (!contains(IntPoint))
                return true;
            int pixel = this[IntPoint];
            return isBetween(pixel, r, g, b, a, rb, gb, bb, ab);
        }


        internal bool isNotBrighterThen(IntPoint IntPoint, int r, int g, int b, int a)
        {
            return !isBrighterThen(IntPoint, r, g, b, a);
        }

        internal bool isBrighterThen(IntPoint IntPoint, int r, int g, int b, int a)
        {
            if (!contains(IntPoint))
                return true;
            int pixel = this[IntPoint];
            return isBrigtherThan(pixel, r, g, b, a);
        }

        bool isTransparent(int argb)
        {
            Color color = Color.FromArgb(argb);
            return (color.A == 0);
        }

        bool isBetween(int argb, int rt, int gt, int bt, int at, int rb, int gb, int bb, int ab)
        {
            Color color = Color.FromArgb(argb);

            int rc = color.R;
            int gc = color.G;
            int bc = color.B;
            int ac = color.A;

            bool blnRInRange = (rt >= rb) ? (rc <= rt && rc >= rb) : (rc <= rt || rc >= rb);
            bool blnGInRange = (gt >= gb) ? (gc <= gt && gc >= gb) : (gc <= gt || gc >= gb);
            bool blnBInRange = (bt >= bb) ? (bc <= bt && bc >= bb) : (bc <= bt || bc >= bb);

            bool blnAInRange = (at >= ab) ? (ac <= at && ac >= ab) : (ac <= at || ac >= ab);

            //ToDo: Test out "Range"
            return blnRInRange && blnGInRange && blnBInRange && blnAInRange;
        }


        bool isBrigtherThan(int argb, int r, int g, int b, int a)
        {
            Color color = Color.FromArgb(argb);

            return ((color.A > a) && (color.R > r) && (color.G > g) && (color.B > b));
        }
    }

    // This is a writable version of the Pixels class.
    // Don't forget to call the save method (could refactor this to use Dispose instead),
    // and to dispose the Bitmap it's constructed from after you finish with the Pixels2 instance.
    class Pixels2 : Pixels
    {
        readonly Bitmap image;
        readonly BitmapData bitmapData;

        public Pixels2(Bitmap image, BitmapData bitmapData)
            : base(bitmapData)
        {
            this.image = image;
            this.bitmapData = bitmapData;
        }

        internal void save(string outputPath)
        {
            System.Runtime.InteropServices.Marshal.Copy(pixels, 0, bitmapData.Scan0, nPixels);
            image.UnlockBits(bitmapData);
            image.Save(outputPath);
        }

        internal void setColor(IntPoint IntPoint, int argb)
        {
            base[IntPoint] = argb;
        }
    }
}

