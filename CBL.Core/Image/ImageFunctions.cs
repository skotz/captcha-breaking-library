using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.IO;
using System.Drawing.Imaging;
using ScottClayton.Utility;

namespace ScottClayton.Image
{
    public static class ImageFunctions
    {
        /// <summary>
        /// Resize a Bitmap
        /// </summary>
        /// <param name="image">The image to resize</param>
        /// <param name="newWidth">The new width</param>
        /// <param name="newHeight">The new height</param>
        static public Bitmap ResizeBitmap(Bitmap image, int newWidth, int newHeight)
        {
            lock (image)
            {
                if (image.Width == newWidth && image.Height == newHeight)
                {
                    return image;
                }

                Bitmap result = new Bitmap(newWidth, newHeight);

                using (Graphics g = Graphics.FromImage((System.Drawing.Image)result))
                {
                    g.DrawImage(image, 0, 0, newWidth, newHeight);
                }

                return result;
            }
        }

        /// <summary>
        /// Copy a rectangle out of an image
        /// </summary>
        /// <param name="srcBitmap">The source image</param>
        /// <param name="section">The section to cut out</param>
        static public Bitmap Copy(Bitmap srcBitmap, Rectangle section)
        {
            // Create the new bitmap and associated graphics object
            Bitmap bmp = new Bitmap(section.Width, section.Height);
            Graphics g = Graphics.FromImage(bmp);

            // Draw the specified section of the source bitmap to the new one
            g.DrawImage(srcBitmap, 0, 0, section, GraphicsUnit.Pixel);

            // Clean up
            g.Dispose();

            // Return the bitmap
            return bmp;
        }

        /// <summary>
        /// Converts an image to an array of bytes compressed with GZip.
        /// </summary>
        static public byte[] ImageToByteArray(Bitmap image)
        {
            MemoryStream ms = new MemoryStream();

            image.Save(ms, ImageFormat.Bmp);

            return Compressor.Compress(ms.ToArray());
        }

        /// <summary>
        /// Reconstructs an image from an encoded byte array.
        /// </summary>
        static public Bitmap ByteArrayToImage(byte[] byteArrayIn)
        {
            MemoryStream ms = new MemoryStream(Compressor.Decompress(byteArrayIn));
            System.Drawing.Image returnImage = System.Drawing.Image.FromStream(ms);

            return (Bitmap)returnImage;
        }

        /// <summary>
        /// Save a bitmap to a binary file
        /// </summary>
        /// <param name="b">The image to save</param>
        /// <param name="w">The file to save it to</param>
        static public void SaveBitmapToFile(Bitmap b, ref BinaryWriter w)
        {
            byte[] image = ImageFunctions.ImageToByteArray(b);
            w.Write(image.Length);
            w.Write(image);
        }

        /// <summary>
        /// Load a bitmap from a binary file
        /// </summary>
        /// <param name="r">The file to read from</param> 
        static public Bitmap LoadBitmapFromFile(ref BinaryReader r)
        {
            int bytes = r.ReadInt32();
            byte[] image = r.ReadBytes(bytes);

            return ImageFunctions.ByteArrayToImage(image);
        }
    }
}
