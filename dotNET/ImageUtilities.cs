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
        /// <returns>A System.Stream representing a TIFF file or null if an error occurred.</returns>
        public static MemoryStream ConvertImageStreamToTIFF(FileStream sourceStream, bool convertICO)
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
            EncoderParameter compressionParameter = new EncoderParameter(compressionEncoder, (long)EncoderValue.CompressionNone);
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

        public static bool ConvertFileToTIFF(string file, bool convertICO)
        {
            if (String.IsNullOrWhiteSpace(file) == true) return false;
            if (File.Exists(file) == false) return false;

            try
            {
                FileStream fileStream = File.Open(file, FileMode.Open, FileAccess.Read, FileShare.Read);
                MemoryStream memStream = ConvertImageStreamToTIFF(fileStream, convertICO);
                if (memStream == null) return false;
                string newFile = Path.ChangeExtension(file, "tiff");
                if (File.Exists(newFile) == true)
                {
                    if (FileUtilities.FileAndStreamAreIdentical(newFile, memStream) == true)
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
    }
}
