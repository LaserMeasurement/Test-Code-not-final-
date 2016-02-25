using System;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Drawing.Imaging;


using Emgu.CV;
using Emgu.CV.UI;
using Emgu.Util;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;

namespace LaserBeamMeasurement
{
    public class ImageData
    {
        public Image ImageFromFile;

        public int sizex;
        public int sizey;

        public int centerx=300;
        public int centery=300;

        public int spotsize=300;
        public int maxspotsize=1000;

        public int ChartXstartX=200;
        public int ChartXstartY=200;

        public int ChartYstartX=200;
        public int ChartYstartY=200;

        public int ChartXstopX=210;
        public int ChartXstopY=210;

        public int ChartYstopX=210;
        public int ChartYstopY=210;

        public const int MaxImageSizeX=2004;
        public const int MaxImageSizeY=2004;

        public int[] ChartX = new int[MaxImageSizeX];
        public int[] ChartY = new int[MaxImageSizeY];

        public int[] TreshMedX = new int[MaxImageSizeX];
        public int[] TreshE2X = new int[MaxImageSizeY];

        public int[] zero = new int[MaxImageSizeX];
        public int zero_level;

        public int MaxX;
        public int MaxY;

        public int graphstartx;
        public int graphstarty;
        public int graphstopx;
        public int graphstopy;

        public int graphics_size;

        public bool handzero = false;



        public void GraphFillRotate(Image<Gray, Byte> gf, int x0, int y0,
                                                          int x1, int y1,bool yx) //
        {

            if (yx)
            for (int i = 0; i < ChartX.Length; i++)
            ChartX[i] =0;
            else
            for (int i = 0; i < ChartY.Length; i++)
            ChartY[i] =0;


            int index = 0;
          

            int dx = (x1 > x0) ? (x1 - x0) : (x0 - x1);
            int dy = (y1 > y0) ? (y1 - y0) : (y0 - y1);
    
            int sx = (x1 >= x0) ? (1) : (-1);
            int sy = (y1 >= y0) ? (1) : (-1);

            if (dy < dx)
            {
                int d = (dy << 1) - dx;
                int d1 = dy << 1;
                int d2 = (dy - dx) << 1;

                if (x0 < 0) x0 = 0;
                if (y0 < 0) y0 = 0;

                if (x0 >= gf.Bitmap.Size.Width) x0 = gf.Bitmap.Size.Width-1;
                if (y0 >= gf.Bitmap.Size.Height) y0 = gf.Bitmap.Size.Height-1;

                if (yx)
            
                    ChartX[index] = gf.Bitmap.GetPixel(x0, y0).R;
      
                else
                    ChartY[index] = gf.Bitmap.GetPixel(x0, y0).R;



                index++;

                int x = x0 + sx;
                int y = y0;
                for (int i = 1; i <= dx; i++)
                {
                    if (d > 0)
                    {
                        d += d2;
                        y += sy;
                    }
                    else
                        d += d1;


                    if (x <= 0) x = 0;
                    if (y <= 0) y = 0;

                    if (x >= gf.Bitmap.Size.Width) x = gf.Bitmap.Size.Width-1;
                    if (y >= gf.Bitmap.Size.Height) y = gf.Bitmap.Size.Height-1;

                    if (yx)
           
                        ChartX[index] = gf.Bitmap.GetPixel(x, y).R;
           
                    else
                        ChartY[index] = gf.Bitmap.GetPixel(x, y).R;
                    index++;
                    x +=sx;
                }
            }
            else
            {
                int d = (dx << 1) - dy;
                int d1 = dx << 1;
                int d2 = (dx - dy) << 1;

                if (x0 < 0) x0 = 0;
                if (y0 < 0) y0 = 0;

                if (x0 >= gf.Bitmap.Size.Width) x0 = gf.Bitmap.Size.Width-1;
                if (y0 >= gf.Bitmap.Size.Height) y0 = gf.Bitmap.Size.Height-1;

                if (yx)
                {
                    ChartX[index] = gf.Bitmap.GetPixel(x0, y0).R;
       
                }
                else
                    ChartY[index] = gf.Bitmap.GetPixel(x0, y0).R;

                index++;
                int x = x0;
                int y = y0 + sy;
                for (int i = 1; i <= dy; i++)
                {
                    if (d > 0)
                    {
                        d += d2;
                        x += sx;
                    }
                    else
                        d += d1;

                    if (x <= 0) x = 0;
                    if (y <= 0) y = 0;

                    if (x >= gf.Bitmap.Size.Width) x = gf.Bitmap.Size.Width-1;
                    if (y >= gf.Bitmap.Size.Height) y = gf.Bitmap.Size.Height-1;

                    if (yx)
                    {
                        ChartX[index] = gf.Bitmap.GetPixel(x, y).R;
          
                    }
                    else
                        ChartY[index] = gf.Bitmap.GetPixel(x, y).R;
                    index++;
                    y +=sy;
                }
            }

            graphics_size = index;
           
        }

     

        public void GraphFill(Image<Gray, Byte> gf)
        {
          
            for (int i = 0; i < ChartX.Length; i++)
            ChartX[i] = 0;

            for (int i = 0; i < ChartY.Length; i++)
            ChartY[i] = 0;


            int index = 0;
        
            graphstartx = centerx - spotsize;
            if (graphstartx < 0) graphstartx = 0;

            graphstopx = centerx + spotsize;
            if (graphstopx > sizex - 1) graphstopx = sizex - 1;


               zero_level = 255;

               for (int i = graphstartx; i < graphstopx; i++)
               {
                   ChartX[index] = gf.Bitmap.GetPixel(i, centery).R;
                   if (ChartX[index] < zero_level) zero_level = ChartX[index];
                   index++;
               }

               index = 0;

               graphstarty = centery - spotsize;
               if (graphstarty < 0) graphstarty = 0;

               graphstopy = centery + spotsize;

               if (graphstopy > sizey - 1) graphstopy = sizey - 1;

               for (int i = graphstarty; i < graphstopy; i++)
               {
                   ChartY[index] = gf.Bitmap.GetPixel(centerx, i).R;
                   index++;
               }
  

        }

        public void MakeFalse(Bitmap bmp) // false color 
        {

            System.Drawing.Imaging.PixelFormat pxf = System.Drawing.Imaging.PixelFormat.Format24bppRgb;

            Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);

            BitmapData bmpData = bmp.LockBits(rect, ImageLockMode.ReadWrite, pxf);

            IntPtr ptr = bmpData.Scan0;

            int numBytes = bmpData.Stride * bmp.Height;
            int widthBytes = bmpData.Stride;
            byte[] rgbValues = new byte[numBytes];

            Marshal.Copy(ptr, rgbValues, 0, numBytes);


            for (int counter = 0; counter < rgbValues.Length; counter += 3)
            {

                if (rgbValues[counter] >= 0 && rgbValues[counter] <= 63)
                {
                    rgbValues[counter + 2] = 0;
                    rgbValues[counter + 1] = (byte)(255 / 63 * rgbValues[counter]);
                    rgbValues[counter] = 255;
                }

                else if (rgbValues[counter] > 63 && rgbValues[counter] <= 127)
                {
                    rgbValues[counter + 2] = 0;
                    rgbValues[counter] = (byte)(255 - (255 / (127 - 63) * (rgbValues[counter] - 63)));
                    rgbValues[counter + 1] = 255;
                }

                else if (rgbValues[counter] > 127 && rgbValues[counter] <= 191)
                {
                    rgbValues[counter] = 0;
                    rgbValues[counter + 2] = (byte)((255 / (191 - 127) * (rgbValues[counter] - 127)));
                    rgbValues[counter + 1] = 255;
                }

                else if (rgbValues[counter] > 191 && rgbValues[counter] <= 255)
                {
                    rgbValues[counter] = 0;
                    rgbValues[counter + 1] = (byte)(255 - (255 / (255 - 191) * (rgbValues[counter] - 191)));
                    rgbValues[counter + 2] = 255;
                }


            }

            Marshal.Copy(rgbValues, 0, ptr, numBytes);

            bmp.UnlockBits(bmpData);
        }

    }

    public class Charts
    {


    }

    public class BeamParameters
    {
        public int sizex_med;
        public int sizey_med;
        public int sizex_e2;
        public int sizey_e2;


        public void BeamSizeDetect(double tresh_med, double tresh_e2, ImageData imdata)
        {
            int start = 0;
            int stop = 0;
            
                 for (int i = 0; i < imdata.ChartX.Length-1; i++)
                 {
                     if (imdata.ChartX[i] > tresh_med) { start = i; break; }

                 }
                 for (int i = imdata.ChartX.Length - 1; i >= 0; i--)
                 {
                     if (imdata.ChartX[i] > tresh_med) { stop = i; break; }
                 }

                 sizex_med = stop - start;

                 for (int i = 0; i < imdata.ChartY.Length - 1; i++)
                 {
                     if (imdata.ChartY[i] > tresh_med) { start = i; break; }

                 }
                 for (int i = imdata.ChartY.Length - 1; i >= 0; i--)
                 {
                     if (imdata.ChartY[i] > tresh_med) { stop = i; break; }
                 }

                 sizey_med = stop - start;

                for (int i = 0; i < imdata.ChartX.Length - 1; i++)
                {
                 if (imdata.ChartX[i] > tresh_e2) { start = i; break; }

                }
                for (int i = imdata.ChartX.Length - 1; i >= 0; i--)
                {
                 if (imdata.ChartX[i] > tresh_e2) { stop = i; break; }
                }

                sizex_e2 = stop - start;

                for (int i = 0; i < imdata.ChartY.Length - 1; i++)
               {
                 if (imdata.ChartY[i] > tresh_e2) { start = i; break; }

               }
               for (int i = imdata.ChartY.Length - 1; i >= 0; i--)
               {
                 if (imdata.ChartY[i] > tresh_e2) { stop = i; break; }
               }

               sizey_e2 = stop - start;
           
        }
    }

}
