using Nistec.IO;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Web;

namespace Nistec.Drawing
{
    /// <summary>
    /// 
    /// </summary>
    public class ImageResizer
    {

        //public static void VaryQualityLevel(string sourceFileName,string path, string filename)
        //{
        //    // Get a bitmap. The using statement ensures objects  
        //    // are automatically disposed from memory after use.  
        //    using (Bitmap bmp1 = new Bitmap(sourceFileName))// @"C:\TestPhoto.jpg"))
        //    {
        //        ImageCodecInfo jpgEncoder = GetEncoder(ImageFormat.Jpeg);

        //        // Create an Encoder object based on the GUID  
        //        // for the Quality parameter category.  
        //        System.Drawing.Imaging.Encoder myEncoder =
        //            System.Drawing.Imaging.Encoder.Quality;

        //        // Create an EncoderParameters object.  
        //        // An EncoderParameters object has an array of EncoderParameter  
        //        // objects. In this case, there is only one  
        //        // EncoderParameter object in the array.  
        //        EncoderParameters myEncoderParameters = new EncoderParameters(1);

        //        EncoderParameter myEncoderParameter = new EncoderParameter(myEncoder, 50L);
        //        myEncoderParameters.Param[0] = myEncoderParameter;
        //        bmp1.Save(@"c:\TestPhotoQualityFifty.jpg", jpgEncoder, myEncoderParameters);


        //        //myEncoderParameter = new EncoderParameter(myEncoder, 100L);
        //        //myEncoderParameters.Param[0] = myEncoderParameter;
        //        //bmp1.Save(@"C:\TestPhotoQualityHundred.jpg", jpgEncoder, myEncoderParameters);

        //        //// Save the bitmap as a JPG file with zero quality level compression.  
        //        //myEncoderParameter = new EncoderParameter(myEncoder, 0L);
        //        //myEncoderParameters.Param[0] = myEncoderParameter;
        //        //bmp1.Save(@"C:\TestPhotoQualityZero.jpg", jpgEncoder, myEncoderParameters);
        //    }
        //}

        //private int DownloadImage(string link)
        //{
        //    try
        //    {
        //        Uri uri = new Uri(link);
        //        HttpWebRequest webrequest = (HttpWebRequest)WebRequest.Create(uri);
        //        webrequest.Method = "GET";

        //        webrequest.Accept = "image/gif, image/x-xbitmap, image/jpeg, image/pjpeg, application/x-shockwave-flash, application/vnd.ms-excel, application/vnd.ms-powerpoint, application/msword, application/xaml+xml, application/vnd.ms-xpsdocument, application/x-ms-xbap, application/x-ms-application, */*";

        //        webrequest.KeepAlive = false;
        //        HttpWebResponse responce = (HttpWebResponse)webrequest.GetResponse();
        //        Stream S = responce.GetResponseStream();
        //        FileStream writeStream = new FileStream(<< FilePath >, FileMode.Create, FileAccess.Write); //Filepath to save
        //        ReadWriteStream(S, writeStream);
        //        responce.Close();
        //        return 1;
        //    }
        //    catch (Exception)
        //    {
        //        return 0;
        //    }
        //}

        //// readStream is the stream you need to read
        //// writeStream is the stream you want to write to
        //private void ReadWriteStream(Stream readStream, Stream writeStream)
        //{
        //    int Length = 256;
        //    Byte[] buffer = new Byte[Length];
        //    int bytesRead = readStream.Read(buffer, 0, Length);
        //    // write the required bytes
        //    while (bytesRead > 0)
        //    {
        //        writeStream.Write(buffer, 0, bytesRead);
        //        bytesRead = readStream.Read(buffer, 0, Length);
        //    }
        //    readStream.Close();
        //    writeStream.Close();
        //}
        /// <summary>
        /// 
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        public static int ImageRatio(string filename, ref int width, ref int height)
        {
            System.Drawing.Image img;
            //System.Drawing.Image thumbnail;

            //Load image from uploaded file stream
            //img = System.Drawing.Image.FromStream(uploadFile.PostedFile.InputStream);

            img = System.Drawing.Image.FromFile(filename);


            //Image variables init
            double img_width = img.Width;
            double img_height = img.Height;
            double img_ratio = img_width / img_height;
            double img_max_width = Convert.ToDouble(width);
            double img_max_height = Convert.ToDouble(height);

            double widthPrc = 0;
            double heightPrc = 0;
            if (img_width > img_height)
            {
                if (img_height > img_max_height)
                {
                    heightPrc = (img_max_height / img_height);
                    img_height = img_max_height;
                    img_width = img_height * img_ratio;
                }
                if (img_width > img_max_width)
                {
                    widthPrc = (img_max_width / img_width);
                    img_width = img_max_width;
                    img_height = img_width / img_ratio;
                }
            }
            else
            {
                if (img_width > img_max_width)
                {
                    widthPrc = (img_max_width / img_width);
                    img_width = img_max_width;
                    img_height = img_width / img_ratio;
                }
                if (img_height > img_max_height)
                {
                    heightPrc = (img_max_height / img_height);
                    img_height = img_max_height;
                    img_width = img_height * img_ratio;
                }
            }

            width = (int)img_width;
            height = (int)img_height;

            if (widthPrc == 0 || heightPrc == 0)
                return (int)(100 * Math.Max(widthPrc, heightPrc));
            else
                return (int)(100 * Math.Min(widthPrc, heightPrc));


        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="inputStream"></param>
        /// <returns></returns>
        public static long GetImageSize(Stream inputStream)
        {
            long size = 0;
            using (NetStream ms = NetStream.CopyStream(inputStream))
            {
                size = ms.Length;
            }
            return size;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="postedFile"></param>
        /// <param name="sourceFilename"></param>
        /// <param name="newfilename"></param>
        /// <param name="maxBytes"></param>
        /// <param name="deleteSource"></param>
        public static void Save(HttpPostedFileBase postedFile,string sourceFilename, string newfilename, long maxBytes, bool deleteSource)
        {
            Save(postedFile.InputStream, sourceFilename, newfilename, maxBytes, deleteSource);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="inputStream"></param>
        /// <param name="sourceFilename"></param>
        /// <param name="newfilename"></param>
        /// <param name="maxBytes"></param>
        /// <param name="deleteSource"></param>
        public static void Save(Stream inputStream, string sourceFilename, string newfilename, long maxBytes, bool deleteSource)
        {
            if (maxBytes <= 0)
            {
                throw new ArgumentNullException("maxBytes");
            }

            long size = GetImageSize(inputStream);
            bool isResize = (size > maxBytes);

            using (var img = System.Drawing.Image.FromStream(inputStream))
            {
                if (isResize)
                {
                    float prc =(float)((double)maxBytes / (double)size);

                    //Image variables init
                    double img_width = img.Width * prc;
                    double img_height = img.Height * prc;

                    ImageResize(img, newfilename, (int)img_width, (int)img_height);
                }
                else
                {
                    //string file_path = Path.Combine(physicalPath, newfilename);
                    img.Save(newfilename);
                }
            }
            if (deleteSource)
                File.Delete(sourceFilename);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sourceFilename"></param>
        /// <param name="newfilename"></param>
        /// <param name="maxWidth"></param>
        /// <param name="deleteSource"></param>
        public static void Save(string sourceFilename, string newfilename, int maxWidth, bool deleteSource)
        {
            if (maxWidth <= 0)
            {
                throw new ArgumentNullException("maxWidth");
            }

            //string file_name = System.IO.Path.GetFileName(sourceFilename);
            //string file_path = Path.Combine(physicalPath, file_name);
            //string virtual_path = Path.Combine(virtualPath, file_name);


            using (System.Drawing.Image img = System.Drawing.Image.FromFile(sourceFilename))
            {
                
                //System.Drawing.Image thumbnail;

                //Load image from uploaded file stream
                //img = System.Drawing.Image.FromStream(uploadFile.PostedFile.InputStream);

                //img = System.Drawing.Image.FromFile(sourceFilename);

                //Image variables init
                double img_width = img.Width;
                double img_height = img.Height;
                double img_ratio = img_width / img_height;
                double img_max_width = Convert.ToDouble(maxWidth);
                //double img_max_height = Convert.ToDouble(height);

                bool isResize = false;
                if (img_width > img_max_width)
                {
                    img_width = img_max_width;
                    img_height = (img_width / img_ratio);
                    isResize = true;
                }
                //if (img_height > img_max_height)
                //{
                //    img_height = img_max_height;
                //    img_width = img_height * img_ratio;
                //}

                if (isResize)
                {
                    ImageResize(img, newfilename, (int)img_width, (int)img_height);
                }
                else
                {
                    //string file_path = Path.Combine(physicalPath, newfilename);
                    //virtual_path = Path.Combine(virtualPath, newfilename);
                    img.Save(newfilename);
                }

            }
            if (deleteSource)
                File.Delete(sourceFilename);

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="image"></param>
        /// <param name="newfilename"></param>
        /// <param name="img_width"></param>
        /// <param name="img_height"></param>
        private static void ImageResize(System.Drawing.Image image, string newfilename, int img_width, int img_height)
        {

            // string newfilename = FormatNewFileName(file_name, (int)img_width, (int)img_height);

            try
            {
                System.Drawing.Image.GetThumbnailImageAbort myCallBack = new System.Drawing.Image.GetThumbnailImageAbort(ThumbnailCallback);
                //thumbnail = img.GetThumbnailImage((int)img_width,(int)img_height,myCallBack,IntPtr.Zero);

                using (System.Drawing.Image thumbnail = new Bitmap((int)img_width, (int)img_height))
                {
                    using (Graphics oGraphic = Graphics.FromImage(thumbnail))
                    {

                        oGraphic.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        oGraphic.SmoothingMode = SmoothingMode.HighQuality;
                        oGraphic.PixelOffsetMode = PixelOffsetMode.HighQuality;
                        oGraphic.CompositingQuality = CompositingQuality.HighQuality;

                        oGraphic.DrawImage(image, 0, 0, (int)img_width, (int)img_height);
                    }

                    //string file_path = Path.Combine(physicalPath, newfilename);
                    //string virtual_path = Path.Combine(virtualPath, newfilename);

                    thumbnail.Save(newfilename);

                    /*
                    //TOD:check this;
                    OctreeQuantizer quantizer = new OctreeQuantizer(255, 8);

                    //DeleteCurrentImg(filename);

                    using (Bitmap bmpQuantized = quantizer.Quantize(thumbnail))
                    {
                        bmpQuantized.Save(file_path);//, ImageFormat.Gif);
                    }
                    */

                    //thumbnail.Dispose();


                   // return new ImageItem(virtual_path, img_width, img_height, zoom);
                }

            }
            catch (Exception ex)
            {
                throw new Exception("Error convert image: " + ex.Message);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private static bool ThumbnailCallback()
        {
            return false;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="original"></param>
        /// <param name="imagePath"></param>
        /// <param name="filename"></param>
        static void DefaultCompressionPng(Image original, string imagePath, string filename)
        {
            MemoryStream ms = new MemoryStream();
            original.Save(ms, ImageFormat.Png);
            Bitmap compressed = new Bitmap(ms);
            ms.Close();

            string fileOutPng = Path.Combine(imagePath, filename);
            compressed.Save(fileOutPng, ImageFormat.Png);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="original"></param>
        /// <param name="imagePath"></param>
        /// <param name="filename"></param>
        static void VariousQuality(Image original, string imagePath, string filename)
        {

            ImageCodecInfo jpgEncoder = null;
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageEncoders();
            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.FormatID == ImageFormat.Jpeg.Guid)
                {
                    jpgEncoder = codec;
                    break;
                }
            }
            if (jpgEncoder != null)
            {
                Encoder encoder = Encoder.Quality;
                EncoderParameters encoderParameters = new EncoderParameters(1);

                for (long quality = 10; quality <= 100; quality += 10)
                {
                    EncoderParameter encoderParameter = new EncoderParameter(encoder, quality);
                    encoderParameters.Param[0] = encoderParameter;

                    string fileOut = Path.Combine(imagePath, "quality_" + quality + ".jpeg");
                    FileStream ms = new FileStream(fileOut, FileMode.Create, FileAccess.Write);
                    original.Save(ms, jpgEncoder, encoderParameters);
                    ms.Flush();
                    ms.Close();
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="format"></param>
        /// <returns></returns>
        public static ImageCodecInfo GetEncoder(ImageFormat format)
        {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();
            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                {
                    return codec;
                }
            }
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sourceFileName"></param>
        /// <param name="path"></param>
        /// <param name="filename"></param>
        /// <param name="format"></param>
        /// <param name="quality"></param>
        public static void SaveJpeg(string sourceFileName, string path, string filename, ImageFormat format, int quality)
        {
            using (Bitmap img = new Bitmap(sourceFileName))
            {
                ImageCodecInfo encoder = GetEncoder(format);
                string output = Path.Combine(path, filename);
                SaveJpeg(output, img, encoder, quality);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <param name="img"></param>
        /// <param name="encoder"></param>
        /// <param name="quality"></param>
        public static void SaveJpeg(string path, Image img, ImageCodecInfo encoder, int quality)
        {
            if (quality < 0 || quality > 100)
                throw new ArgumentOutOfRangeException("quality must be between 0 and 100.");

            // Encoder parameter for image quality 
            EncoderParameter qualityParam =
                new EncoderParameter(Encoder.Quality, quality);

            EncoderParameters encoderParams = new EncoderParameters(1);
            encoderParams.Param[0] = qualityParam;

            img.Save(path, encoder, encoderParams);
        }


        /// <summary> 
        /// Saves an image as a jpeg image, with the given quality 
        /// </summary> 
        /// <param name="path">Path to which the image would be saved.</param> 
        /// <param name="img">Image</param> 
        /// <param name="quality">An integer from 0 to 100, with 100 being the 
        /// highest quality</param> 
        public static void SaveJpeg(string path, Image img, int quality)
        {
            if (quality < 0 || quality > 100)
                throw new ArgumentOutOfRangeException("quality must be between 0 and 100.");


            // Encoder parameter for image quality 
            EncoderParameter qualityParam =
                new EncoderParameter(Encoder.Quality, quality);

            // Jpeg image codec 
            ImageCodecInfo jpegCodec = GetEncoder("image/jpeg");

            EncoderParameters encoderParams = new EncoderParameters(1);
            encoderParams.Param[0] = qualityParam;

            img.Save(path, jpegCodec, encoderParams);
        }

        /// <summary> 
        /// Returns the image codec with the given mime type 
        /// </summary> 
        public static ImageCodecInfo GetEncoder(string mimeType)
        {
            // Get image codecs for all image formats 
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageEncoders();

            // Find the correct image codec 
            for (int i = 0; i < codecs.Length; i++)
                if (codecs[i].MimeType == mimeType)
                    return codecs[i];
            return null;
        }
    }
}
