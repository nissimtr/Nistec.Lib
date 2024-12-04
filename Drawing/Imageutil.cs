//licHeader
//===============================================================================================================
// System  : Nistec.Lib - Nistec.Lib Class Library
// Author  : 
// Updated : 01/07/2015
// Note    : 
// Compiler: Microsoft Visual C#
//
// This file contains a class that is part of nistec library.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: http://nistec.net/license/nistec.cache-license.txt.  
// This notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who      Comments
// ==============================================================================================================
// 10/01/2006  Nissim   Created the code
//===============================================================================================================
//licHeader|
    using System;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Drawing;
    using System.IO;
    using System.Net;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;
    using System.Threading;
    using System.Windows.Forms.Layout;
    using System.Drawing.Imaging;

#pragma warning disable  CS1591

namespace Nistec.Drawing
{
    // Summary:
    //     Specifies how an image is positioned within a System.Windows.Forms.PictureBox.
    public enum PictureSizeMode
    {
        //     The image is placed in the upper-left corner of the System.Windows.Forms.PictureBox.
        //     The image is clipped if it is larger than the System.Windows.Forms.PictureBox
        //     it is contained in.
        Normal = 0,
        //     The image within the System.Windows.Forms.PictureBox is stretched or shrunk
        //     to fit the size of the System.Windows.Forms.PictureBox.
        StretchImage = 1,
        //     The System.Windows.Forms.PictureBox is sized equal to the size of the image
        //     that it contains.
        AutoSize = 2,
        //     The image is displayed in the center if the System.Windows.Forms.PictureBox
        //     is larger than the image. If the image is larger than the System.Windows.Forms.PictureBox,
        //     the picture is placed in the center of the System.Windows.Forms.PictureBox
        //     and the outside edges are clipped.
        CenterImage = 3,
        //     The size of the image is increased or decreased maintaining the size ratio.
        Zoom = 4,
    }

    /// <summary>Represents a Image Util control for displaying an image.</summary>
    public class ImageUtil : ISupportInitialize
    {

        private int contentLength;
        private AsyncOperation currentAsyncLoadOperation;
        private bool currentlyAnimating;
        private System.Drawing.Image defaultErrorImage;
        [ThreadStatic]
        private static System.Drawing.Image defaultErrorImageForThread = null;
        private static readonly object defaultErrorImageKey = new object();
        private System.Drawing.Image defaultInitialImage;
        [ThreadStatic]
        private static System.Drawing.Image defaultInitialImageForThread = null;
        private static readonly object defaultInitialImageKey = new object();
        private System.Drawing.Image errorImage;
        private static readonly object EVENT_SIZEMODECHANGED = new object();
        private System.Drawing.Image image;
        private ImageInstallationType imageInstallationType;
        private string imageLocation;
        private System.Drawing.Image initialImage;
        private object internalSyncObject = new object();
        private SendOrPostCallback loadCompletedDelegate;
        private static readonly object loadCompletedKey = new object();
        private static readonly object loadProgressChangedKey = new object();
        private SendOrPostCallback loadProgressDelegate;
        private BitVector32 pictureState;

        private const int PICTURESTATE_asyncOperationInProgress = 1;
        private const int PICTURESTATE_cancellationPending = 2;
        private const int PICTURESTATE_inInitialization = 0x40;
        private const int PICTURESTATE_needToLoadImageLocation = 0x20;
        private const int PICTURESTATE_useDefaultErrorImage = 8;
        private const int PICTURESTATE_useDefaultInitialImage = 4;
        private const int PICTURESTATE_waitOnLoad = 0x10;
        private const int readBlockSize = 0x1000;
        private byte[] readBuffer;
        private Size savedSize;
        private PictureSizeMode sizeMode;
        private MemoryStream tempDownloadStream;
        private int totalBytesRead;

        private Size size;

        public Size Size
        {
            get { return size; }
            set { size = value; }
        }

        /// <summary>Occurs when the asynchronous image-load operation is completed, been cancelled or raised an exception.</summary>
        public event AsyncCompletedEventHandler LoadCompleted;


        /// <summary>Occurs when the progress of an asynchronous image-loading operation has changed.</summary>
        public event ProgressChangedEventHandler LoadProgressChanged;

        /// <summary>Occurs when <see cref="P:System.Windows.Forms.PictureBox.SizeMode"></see> changes.</summary>
        public event EventHandler SizeModeChanged;


        /// <summary>Initializes a new instance of the <see cref="T:System.Windows.Forms.PictureBox"></see> class.</summary>
        public ImageUtil()
        {
            this.pictureState = new BitVector32(12);
            this.savedSize = this.Size;
        }

        private void AdjustSize()
        {
            if (this.sizeMode == PictureSizeMode.AutoSize)
            {
                //this.Size = this.PreferredSize;
            }
            else
            {
                this.Size = this.savedSize;
            }
        }
        private void Animate()
        {
            Animate(true);
        }

        private void Animate(bool animate)
        {
            if (animate != this.currentlyAnimating)
            {
                if (animate)
                {
                    if (this.image != null)
                    {
                        ImageAnimator.Animate(this.image, new EventHandler(this.OnFrameChanged));
                        this.currentlyAnimating = animate;
                    }
                }
                else if (this.image != null)
                {
                    ImageAnimator.StopAnimate(this.image, new EventHandler(this.OnFrameChanged));
                    this.currentlyAnimating = animate;
                }
            }
        }

        private void BeginGetResponseDelegate(object arg)
        {
            WebRequest state = (WebRequest)arg;
            state.BeginGetResponse(new AsyncCallback(this.GetResponseCallback), state);
        }

        private Uri CalculateUri(string path)
        {
            try
            {
                return new Uri(path);
            }
            catch (UriFormatException)
            {
                path = Path.GetFullPath(path);
                return new Uri(path);
            }
        }

        /// <summary>Cancels an asynchronous image load.</summary>
        public void CancelAsync()
        {
            this.pictureState[2] = true;
        }

        /// <summary>Releases the unmanaged resources used by the <see cref="T:System.Windows.Forms.PictureBox"></see> and optionally releases the managed resources.</summary>
        /// <param name="disposing">true to release managed and unmanaged resources; false to release unmanaged resources only.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.StopAnimate();
            }
            this.Dispose(disposing);
        }

        private void GetResponseCallback(IAsyncResult result)
        {
            if (this.pictureState[2])
            {
                this.PostCompleted(null, true);
            }
            else
            {
                try
                {
                    WebResponse response = ((WebRequest)result.AsyncState).EndGetResponse(result);
                    this.contentLength = (int)response.ContentLength;
                    this.totalBytesRead = 0;
                    Stream responseStream = response.GetResponseStream();
                    responseStream.BeginRead(this.readBuffer, 0, 0x1000, new AsyncCallback(this.ReadCallBack), responseStream);
                }
                catch (Exception exception)
                {
                    this.PostCompleted(exception, false);
                }
            }
        }

 
        private void InstallNewImage(System.Drawing.Image value, ImageInstallationType installationType)
        {
            this.StopAnimate();
            this.image = value;
            //LayoutTransaction.DoLayoutIf(this.AutoSize, this, this, PropertyNames.Image);
            this.Animate();
            if (installationType != ImageInstallationType.ErrorOrInitial)
            {
                this.AdjustSize();
            }
            this.imageInstallationType = installationType;
            //xClearPreferredSizeCache(this);
        }

 
        public static Image BytesToImage(byte[] bytes)
        {
            Image returnImage = null;
            using (MemoryStream stream = new MemoryStream(bytes))
            {
                returnImage = Image.FromStream(stream);
                stream.Close();
            }
            return returnImage;
        }

        public static System.Drawing.Imaging.ImageFormat GetFormat(string fileName)
        {
            string ext = ".gif";
            if (!string.IsNullOrEmpty(fileName))
            {
                ext = Path.GetExtension(fileName);
            }

            switch (ext.ToLower())
            {
                case ".gif":
                    return System.Drawing.Imaging.ImageFormat.Gif;
                case ".bmp":
                    return System.Drawing.Imaging.ImageFormat.Bmp;
                case ".emf":
                    return System.Drawing.Imaging.ImageFormat.Emf;
                case ".exif":
                    return System.Drawing.Imaging.ImageFormat.Exif;
                case ".ico":
                case ".icon":
                    return System.Drawing.Imaging.ImageFormat.Icon;
                case ".jpg":
                case ".jpeg":
                    return System.Drawing.Imaging.ImageFormat.Jpeg;
                case ".png":
                    return System.Drawing.Imaging.ImageFormat.Png;
                case ".tiff":
                    return System.Drawing.Imaging.ImageFormat.Tiff;
                case ".wmf":
                    return System.Drawing.Imaging.ImageFormat.Wmf;
                default:// "gif":
                    return System.Drawing.Imaging.ImageFormat.Gif;
            }

        }

        public System.Drawing.Imaging.ImageFormat GetFormat()
        {
            return GetFormat(this.ImageLocation);
        }

        public byte[] ImageToBytes()
        {

            byte[] bytes = null;
            Image image = this.image;
            if (image == null)
                return null;
            try
            {

                using (MemoryStream stream = new MemoryStream())
                {
                    image.Save(stream, GetFormat());
                    bytes = stream.ToArray();
                    stream.Close();
                }

                return bytes;
            }
            catch (Exception ex)
            {
                string s = ex.Message;
                return null;
            }
        }


        public string ImageToBase64Stream()
        {
            try
            {
                byte[] bytes = ImageToBytes();
                if (bytes == null)
                    return null;

                return Convert.ToBase64String(bytes);
            }
            catch (Exception ex)
            {
                string s = ex.Message;
                return null;
            }
        }

        public void LoadFromBase64Stream(string s)
        {

            System.Drawing.Image image = null;
            byte[] bytes = System.Convert.FromBase64String(s);
            using (MemoryStream stream = new MemoryStream(bytes))
            {
                image = System.Drawing.Image.FromStream(stream);
                stream.Close();
            }
            this.InstallNewImage(image, ImageInstallationType.DirectlySpecified);
        }

        string imageStream;

        public string ImageStream
        {
            get { return imageStream; }
            set { imageStream = value; }
        }

        private void SetImageStream(Stream stream)
        {

            int numBytesToRead = (int)stream.Length;
            // Now read s into a byte buffer.
            byte[] bytes = new byte[numBytesToRead];

            int numBytesRead = 0;
            while (numBytesToRead > 0)
            {
                // Read may return anything from 0 to numBytesToRead.
                int n = stream.Read(bytes, numBytesRead, numBytesToRead);
                // The end of the file is reached.
                if (n == 0)
                    break;
                numBytesRead += n;
                numBytesToRead -= n;
            }
            imageStream = System.Convert.ToBase64String(bytes);
        }

        /// <summary>Displays the image specified by the property</summary>
        public void Load()
        {
            System.Drawing.Image errorImage;
            if ((this.imageLocation == null) || (this.imageLocation.Length == 0))
            {
                throw new InvalidOperationException("PictureBox No Image Location");
            }
            this.pictureState[0x20] = false;
            ImageInstallationType fromUrl = ImageInstallationType.FromUrl;
            try
            {
                Uri uri = this.CalculateUri(this.imageLocation);
                if (uri.IsFile)
                {
                    using (StreamReader reader = new StreamReader(uri.LocalPath))
                    {
                        SetImageStream(reader.BaseStream);
                        errorImage = System.Drawing.Image.FromStream(reader.BaseStream);
                        goto Label_00C0;
                    }
                }
                using (WebClient client = new WebClient())
                {
                    using (Stream stream = client.OpenRead(uri.ToString()))
                    {
                        SetImageStream(stream);
                        errorImage = System.Drawing.Image.FromStream(stream);
                    }
                }
            }
            catch
            {
                errorImage = this.ErrorImage;
                fromUrl = ImageInstallationType.ErrorOrInitial;
            }
        Label_00C0:
            this.InstallNewImage(errorImage, fromUrl);
        }

        /// <summary>Sets the <see cref="P:System.Windows.Forms.PictureBox.ImageLocation"></see> to the specified URL and displays the image indicated.</summary>
        public void Load(string url)
        {
            this.ImageLocation = url;
            this.Load();
        }

        /// <summary>Loads the image asynchronously.</summary>
        public void LoadAsync()
        {
            if ((this.imageLocation == null) || (this.imageLocation.Length == 0))
            {
                throw new InvalidOperationException("PictureBox No Image Location");
            }
            if (!this.pictureState[1])
            {
                this.pictureState[1] = true;
                if (((this.Image == null) || (this.imageInstallationType == ImageInstallationType.ErrorOrInitial)) && (this.InitialImage != null))
                {
                    this.InstallNewImage(this.InitialImage, ImageInstallationType.ErrorOrInitial);
                }
                this.currentAsyncLoadOperation = AsyncOperationManager.CreateOperation(null);
                if (this.loadCompletedDelegate == null)
                {
                    this.loadCompletedDelegate = new SendOrPostCallback(this.LoadCompletedDelegate);
                    this.loadProgressDelegate = new SendOrPostCallback(this.LoadProgressDelegate);
                    this.readBuffer = new byte[0x1000];
                }
                this.pictureState[0x20] = false;
                this.pictureState[2] = false;
                this.contentLength = -1;
                this.tempDownloadStream = new MemoryStream();
                WebRequest state = WebRequest.Create(this.CalculateUri(this.imageLocation));
                new WaitCallback(this.BeginGetResponseDelegate).BeginInvoke(state, null, null);
            }
        }

        /// <summary>Loads the image at the specified location, asynchronously.</summary>
        public void LoadAsync(string url)
        {
            this.ImageLocation = url;
            this.LoadAsync();
        }

        private void LoadCompletedDelegate(object arg)
        {
            AsyncCompletedEventArgs e = (AsyncCompletedEventArgs)arg;
            System.Drawing.Image errorImage = this.ErrorImage;
            ImageInstallationType errorOrInitial = ImageInstallationType.ErrorOrInitial;
            if (!e.Cancelled && (e.Error == null))
            {
                try
                {
                    errorImage = System.Drawing.Image.FromStream(this.tempDownloadStream);
                    errorOrInitial = ImageInstallationType.FromUrl;
                }
                catch (Exception exception)
                {
                    e = new AsyncCompletedEventArgs(exception, false, null);
                }
            }
            if (!e.Cancelled)
            {
                this.InstallNewImage(errorImage, errorOrInitial);
            }
            this.tempDownloadStream = null;
            this.pictureState[2] = false;
            this.pictureState[1] = false;
            this.OnLoadCompleted(e);
        }

        private void LoadProgressDelegate(object arg)
        {
            this.OnLoadProgressChanged((ProgressChangedEventArgs)arg);
        }


        private void OnFrameChanged(object o, EventArgs e)
        {
            //if (this.InvokeRequired && this.IsHandleCreated)
            //{
            //    lock (this.internalSyncObject)
            //    {
            //        if (this.handleValid)
            //        {
            //            this.BeginInvoke(new EventHandler(this.OnFrameChanged), new object[] { o, e });
            //        }
            //        return;
            //    }
            //}
            //if (!this.IsWindowObscured)
            //{
            //    this.Invalidate();
            //}
            //else
            //{
            //    this.StopAnimate();
            //}
        }


        /// <summary>Raises the <see cref="E:System.Windows.Forms.PictureBox.LoadCompleted"></see> event.</summary>
        /// <param name="e">An <see cref="T:System.ComponentModel.AsyncCompletedEventArgs"></see> that contains the event data. </param>
        protected virtual void OnLoadCompleted(AsyncCompletedEventArgs e)
        {
            AsyncCompletedEventHandler handler = (AsyncCompletedEventHandler)this.LoadCompleted;//[loadCompletedKey];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        /// <summary>Raises the <see cref="E:System.Windows.Forms.PictureBox.LoadProgressChanged"></see> event.</summary>
        /// <param name="e">A <see cref="T:System.ComponentModel.ProgressChangedEventArgs"></see> that contains the event data.</param>
        protected virtual void OnLoadProgressChanged(ProgressChangedEventArgs e)
        {
            ProgressChangedEventHandler handler = (ProgressChangedEventHandler)this.LoadProgressChanged;//.Events[loadProgressChangedKey];
            if (handler != null)
            {
                handler(this, e);
            }
        }


        /// <summary>Raises the <see cref="E:System.Windows.Forms.Control.Resize"></see> event.</summary>
        /// <param name="e">An <see cref="T:System.EventArgs"></see> that contains the event data.</param>
        protected virtual void OnResize(EventArgs e)
        {
            this.OnResize(e);
            //if (((this.sizeMode == PictureSizeMode.Zoom) || (this.sizeMode == PictureSizeMode.StretchImage)) || ((this.sizeMode == PictureSizeMode.CenterImage) || (this.BackgroundImage != null)))
            //{
            //    this.Invalidate();
            //}
            this.savedSize = this.Size;
        }

        /// <summary>Raises the <see cref="E:System.Windows.Forms.PictureBox.SizeModeChanged"></see> event.</summary>
        /// <param name="e">An <see cref="T:System.EventArgs"></see> that contains the event data. </param>
        protected virtual void OnSizeModeChanged(EventArgs e)
        {
            EventHandler handler = this.SizeModeChanged;//.Events[EVENT_SIZEMODECHANGED] as EventHandler;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        private void PostCompleted(Exception error, bool cancelled)
        {
            AsyncOperation currentAsyncLoadOperation = this.currentAsyncLoadOperation;
            this.currentAsyncLoadOperation = null;
            if (currentAsyncLoadOperation != null)
            {
                currentAsyncLoadOperation.PostOperationCompleted(this.loadCompletedDelegate, new AsyncCompletedEventArgs(error, cancelled, null));
            }
        }

        private void ReadCallBack(IAsyncResult result)
        {
            if (this.pictureState[2])
            {
                this.PostCompleted(null, true);
            }
            else
            {
                Stream asyncState = (Stream)result.AsyncState;
                try
                {
                    int count = asyncState.EndRead(result);
                    if (count > 0)
                    {
                        this.totalBytesRead += count;
                        this.tempDownloadStream.Write(this.readBuffer, 0, count);
                        asyncState.BeginRead(this.readBuffer, 0, 0x1000, new AsyncCallback(this.ReadCallBack), asyncState);
                        if (this.contentLength != -1)
                        {
                            int progressPercentage = (int)(100f * (((float)this.totalBytesRead) / ((float)this.contentLength)));
                            if (this.currentAsyncLoadOperation != null)
                            {
                                this.currentAsyncLoadOperation.Post(this.loadProgressDelegate, new ProgressChangedEventArgs(progressPercentage, null));
                            }
                        }
                    }
                    else
                    {
                        this.tempDownloadStream.Seek(0L, SeekOrigin.Begin);
                        if (this.currentAsyncLoadOperation != null)
                        {
                            this.currentAsyncLoadOperation.Post(this.loadProgressDelegate, new ProgressChangedEventArgs(100, null));
                        }
                        this.PostCompleted(null, false);
                        Stream stream2 = asyncState;
                        asyncState = null;
                        stream2.Close();
                    }
                }
                catch (Exception exception)
                {
                    this.PostCompleted(exception, false);
                    if (asyncState != null)
                    {
                        asyncState.Close();
                    }
                }
            }
        }

        private void ResetErrorImage()
        {
            this.pictureState[8] = true;
            this.errorImage = this.defaultErrorImage;
        }

        private void ResetImage()
        {
            this.InstallNewImage(null, ImageInstallationType.DirectlySpecified);
        }

        private void ResetInitialImage()
        {
            this.pictureState[4] = true;
            this.initialImage = this.defaultInitialImage;
        }

        private bool ShouldSerializeErrorImage()
        {
            return !this.pictureState[8];
        }

        private bool ShouldSerializeImage()
        {
            return ((this.imageInstallationType == ImageInstallationType.DirectlySpecified) && (this.Image != null));
        }

        private bool ShouldSerializeInitialImage()
        {
            return !this.pictureState[4];
        }

        private void StopAnimate()
        {
            this.Animate(false);
        }

        void ISupportInitialize.BeginInit()
        {
            this.pictureState[0x40] = true;
        }

        void ISupportInitialize.EndInit()
        {
            if (((this.ImageLocation != null) && (this.ImageLocation.Length != 0)) && this.WaitOnLoad)
            {
                this.Load();
            }
            this.pictureState[0x40] = false;
        }

        /// <summary>Returns a string that represents the current <see cref="T:System.Windows.Forms.PictureBox"></see> control.</summary>
        /// <returns>A string that represents the current <see cref="T:System.Windows.Forms.PictureBox"></see>. </returns>
        /// <filterpriority>2</filterpriority>
        public override string ToString()
        {
            return (this.ToString() + ", SizeMode: " + this.sizeMode.ToString("G"));
        }


        protected virtual Size DefaultSize
        {
            get
            {
                return new Size(100, 50);
            }
        }

        /// <summary>Gets or sets the image to display when an error occurs during the image-loading process or if the image load is cancelled.</summary>
        public System.Drawing.Image ErrorImage
        {
            get
            {
                if ((this.errorImage == null) && this.pictureState[8])
                {
                    if (this.defaultErrorImage == null)
                    {
                        if (defaultErrorImageForThread == null)
                        {
                            defaultErrorImageForThread = new Bitmap(typeof(ImageUtil), "ImageInError.bmp");
                        }
                        this.defaultErrorImage = defaultErrorImageForThread;
                    }
                    this.errorImage = this.defaultErrorImage;
                }
                return this.errorImage;
            }
            set
            {
                if (this.ErrorImage != value)
                {
                    this.pictureState[8] = false;
                }
                this.errorImage = value;
            }
        }


        /// <summary>Gets or sets the image that the <see cref="T:System.Windows.Forms.PictureBox"></see> displays.</summary>
        public System.Drawing.Image Image
        {
            get
            {
                return this.image;
            }
            set
            {
                this.InstallNewImage(value, ImageInstallationType.DirectlySpecified);
            }
        }

        /// <summary>Gets or sets the path for the image to display in the <see cref="T:System.Windows.Forms.PictureBox"></see>.</summary>
        public string ImageLocation
        {
            get
            {
                return this.imageLocation;
            }
            set
            {
                this.imageLocation = value;
                this.pictureState[0x20] = !string.IsNullOrEmpty(this.imageLocation);
                if (string.IsNullOrEmpty(this.imageLocation) && (this.imageInstallationType != ImageInstallationType.DirectlySpecified))
                {
                    this.InstallNewImage(null, ImageInstallationType.DirectlySpecified);
                }
                if ((this.WaitOnLoad && !this.pictureState[0x40]) && !string.IsNullOrEmpty(this.imageLocation))
                {
                    this.Load();
                }
                //this.Invalidate();
            }
        }


        /// <summary>Gets or sets the image displayed in the <see cref="T:System.Windows.Forms.PictureBox"></see> control while the main image is loading.</summary>
        public System.Drawing.Image InitialImage
        {
            get
            {
                if ((this.initialImage == null) && this.pictureState[4])
                {
                    if (this.defaultInitialImage == null)
                    {
                        if (defaultInitialImageForThread == null)
                        {
                            defaultInitialImageForThread = new Bitmap(typeof(ImageUtil), "PictureBox.Loading.bmp");
                        }
                        this.defaultInitialImage = defaultInitialImageForThread;
                    }
                    this.initialImage = this.defaultInitialImage;
                }
                return this.initialImage;
            }
            set
            {
                if (this.InitialImage != value)
                {
                    this.pictureState[4] = false;
                }
                this.initialImage = value;
            }
        }


        /// <summary>Indicates how the image is displayed.</summary>
        public PictureSizeMode SizeMode
        {
            get
            {
                return this.sizeMode;
            }
            set
            {
                if (this.sizeMode != value)
                {
                    if (value == PictureSizeMode.AutoSize)
                    {
                        //this.AutoSize = true;
                    }
                    if (value != PictureSizeMode.AutoSize)
                    {
                        //this.AutoSize = false;
                        this.savedSize = this.Size;
                    }
                    this.sizeMode = value;
                    this.AdjustSize();
                    //this.Invalidate();
                    this.OnSizeModeChanged(EventArgs.Empty);
                }
            }
        }


        /// <summary>Gets or sets a value indicating whether an image is loaded synchronously.</summary>
        public bool WaitOnLoad
        {
            get
            {
                return this.pictureState[0x10];
            }
            set
            {
                this.pictureState[0x10] = value;
            }
        }

        private enum ImageInstallationType
        {
            DirectlySpecified,
            ErrorOrInitial,
            FromUrl
        }
    }
}


