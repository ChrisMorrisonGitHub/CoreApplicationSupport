using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniversalBinary.CoreApplicationSupport
{
    public static class ImageUtilities
    {
        /// <summary>
        /// Attempts to convert the stream given to a TIFF file.
        /// </summary>
        /// <param name="sourceStream">The System.Stream to attempt to work on.</param>
        /// <param name="convertICO">Specifies whether to attempt to convert a Windows icon (.ico) file.</param>
        /// <param name="compress">Specifies whether lossless compression should be applied to the image.</param>
        /// <returns>A System.Stream representing a TIFF file or null if an error occurred.</returns>
        public static MemoryStream ConvertImageStreamToTIFF(FileStream sourceStream, bool convertICO, bool compress)
        {
            Bitmap bitmap;
            MemoryStream destStream;
            ImageCodecInfo imageCodecInfo;
            imageCodecInfo = GetEncoderInfo("image/tiff");
            System.Drawing.Imaging.Encoder compressionEncoder = System.Drawing.Imaging.Encoder.Compression;
            System.Drawing.Imaging.Encoder qualityEncoder = System.Drawing.Imaging.Encoder.Quality;
            System.Drawing.Imaging.Encoder colorDepthEncoder = System.Drawing.Imaging.Encoder.ColorDepth;
            System.Drawing.Imaging.Encoder renderMethodEncoder = System.Drawing.Imaging.Encoder.RenderMethod;
            System.Drawing.Imaging.Encoder scanMethodEncoder = System.Drawing.Imaging.Encoder.ScanMethod;
            EncoderParameters encoderParameters = new EncoderParameters(5);
            EncoderParameter compressionParameter = null;
            if (compress == false)
            {
                compressionParameter = new EncoderParameter(compressionEncoder, (long)EncoderValue.CompressionNone);
            }
            else
            {
                compressionParameter = new EncoderParameter(compressionEncoder, (long)EncoderValue.CompressionLZW);
            }
            EncoderParameter qualityParameter = new EncoderParameter(qualityEncoder, 100L);
            EncoderParameter colorDepthParameter = new EncoderParameter(colorDepthEncoder, 32L);
            EncoderParameter renderMethodParameter = new EncoderParameter(renderMethodEncoder, (long)EncoderValue.RenderProgressive);
            EncoderParameter scanMethodParameter = new EncoderParameter(scanMethodEncoder, (long)EncoderValue.ScanMethodNonInterlaced);
            encoderParameters.Param[0] = compressionParameter;
            encoderParameters.Param[1] = qualityParameter;
            encoderParameters.Param[2] = colorDepthParameter;
            encoderParameters.Param[3] = renderMethodParameter;
            encoderParameters.Param[4] = scanMethodParameter;

            try
            {
                destStream = new MemoryStream();
                bitmap = new Bitmap(sourceStream);
                if ((convertICO == false) && (bitmap.RawFormat.Equals(ImageFormat.Icon) == true)) return null;
                PropertyItem[] pitems = bitmap.PropertyItems;
                foreach(PropertyItem pi in pitems)
                {
                    // Correct orientation
                    if (pi.Id == 0x0112)
                    {
                        int count = pi.Len / 2;

                        ushort[] result = new ushort[count];
                        for (int i = 0; i < count; i++)
                        {
                            result[i] =  BitConverter.ToUInt16(pi.Value, i * 2);
                        }

                        RotateFlipType rft = OrientationToFlipType(result[0]);
                        if (rft != RotateFlipType.RotateNoneFlipNone) bitmap.RotateFlip(rft);
                    }
                    bitmap.RemovePropertyItem(pi.Id);
                }
                bitmap.Save(destStream, imageCodecInfo, encoderParameters);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("An exception was thrown while converting an image {0} : {1}", ex.Message, ex.StackTrace);
                return null;
            }

            destStream.Seek(0, SeekOrigin.Begin);
            return destStream;
        }

        private static ImageCodecInfo GetEncoderInfo(String mimeType)
        {
            int j;
            ImageCodecInfo[] encoders;
            encoders = ImageCodecInfo.GetImageEncoders();
            for (j = 0; j < encoders.Length; ++j)
            {
                if (encoders[j].MimeType == mimeType)
                    return encoders[j];
            }
            return null;
        }

        public static bool ConvertFileToTIFF(string file, bool convertICO, bool compress)
        {
            if (String.IsNullOrWhiteSpace(file) == true) return false;
            if (File.Exists(file) == false) return false;
            string newFile = Path.ChangeExtension(file, "tiff");

            try
            {
                FileStream fileStream = File.Open(file, FileMode.Open, FileAccess.Read, FileShare.Read);
                MemoryStream memStream = ConvertImageStreamToTIFF(fileStream, convertICO, compress);
                if (memStream == null) return false;
            
                if (File.Exists(newFile) == true)
                {
                    if (FilesContainIdenticalImage(file, newFile, true) == true)
                    {
                        fileStream.Close();
                        memStream.Close();
                        return false;
                    }
                    Random rnd = new Random();
                    string newExt = rnd.Next(0, 10000).ToString("D5") + ".tiff";
                    Path.ChangeExtension(newFile, newExt);
                }
                FileStream outStream = File.Create(newFile, 1024, FileOptions.SequentialScan);
                memStream.CopyTo(outStream);
                outStream.Flush(true);
                fileStream.Close();
                memStream.Close();
                outStream.Close();
                File.Delete(file);
            }
            catch
            {
                return false;
            }

            return true;
        }

        private static RotateFlipType OrientationToFlipType(ushort orientation)
        {
            switch (orientation)
            {
                case 1:
                    return RotateFlipType.RotateNoneFlipNone;
                case 2:
                    return RotateFlipType.RotateNoneFlipX;
                case 3:
                    return RotateFlipType.Rotate180FlipNone;
                case 4:
                    return RotateFlipType.Rotate180FlipX;
                case 5:
                    return RotateFlipType.Rotate90FlipX;
                case 6:
                    return RotateFlipType.Rotate90FlipNone;
                case 7:
                    return RotateFlipType.Rotate270FlipX;
                case 8:
                    return RotateFlipType.Rotate270FlipNone;
                default:
                    return RotateFlipType.RotateNoneFlipNone;
            }
        }

        public static bool FilesContainIdenticalImage(string file1, string file2, bool compareFormat = false)
        {
            Bitmap b1;
            Bitmap b2;

            try
            {
                b1 = new Bitmap(file1);
                b2 = new Bitmap(file2);
                if ((compareFormat == true) && (b1.RawFormat != b2.RawFormat)) return false;
                if (BitmapsAreComparable(b1, b2) == false) return false;
                if (b1.Width == b1.Height)
                {
                    // Square image.
                    if (PixelsMatch(b1, b2) == true) return true;
                    b2.RotateFlip(RotateFlipType.Rotate90FlipNone);
                    if (PixelsMatch(b1, b2) == true) return true;
                    b2.RotateFlip(RotateFlipType.Rotate90FlipNone);
                    if (PixelsMatch(b1, b2) == true) return true;
                    b2.RotateFlip(RotateFlipType.Rotate90FlipNone);
                    if (PixelsMatch(b1, b2) == true) return true;
                }
                else
                {
                    if (b1.Width != b2.Width) b2.RotateFlip(RotateFlipType.Rotate90FlipNone);
                    if (PixelsMatch(b1, b2) == true) return true;
                    b2.RotateFlip(RotateFlipType.Rotate180FlipNone);
                    if (PixelsMatch(b1, b2) == true) return true;
                }
            }
            catch
            {
                return false;
            }

            return false;
        }

        private static bool BitmapsAreComparable(Bitmap bmp1, Bitmap bmp2)
        {
            if ((bmp1 == null) || (bmp2 == null)) return false;

            if (((bmp1.Width == bmp2.Width) && (bmp1.Height == bmp2.Height)) || ((bmp1.Width == bmp2.Height) && (bmp1.Height == bmp2.Width))) return true;

            return false;
        }

        private static bool PixelsMatch(Bitmap bmp1, Bitmap bmp2)
        {
            if (bmp1.Size != bmp2.Size) return false;

            int width = bmp1.Width;
            int height = bmp1.Height;
            Color c1;
            Color c2;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    c1 = bmp1.GetPixel(x, y);
                    c2 = bmp2.GetPixel(x, y);
                    if (c1.ToArgb() != c2.ToArgb()) return false;
                }
            }

            return true;
        }
    }
}
