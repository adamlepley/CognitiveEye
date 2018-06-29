using System;
using System.Drawing;
using System.Threading.Tasks;
using AVFoundation;
using CongnitiveEye.Forms.Renderers;
using CoreGraphics;
using Foundation;
using UIKit;
using System.Linq;
using System.IO;

namespace CognitiveEye.iOS.Utilities
{
    public class IOSCameraView : UIView
    {
        AVCaptureVideoPreviewLayer previewLayer;
        CameraDeviceOptions cameraOptions;

        public event EventHandler<EventArgs> Tapped;

        public AVCaptureSession CaptureSession { get; private set; }

        public AVCaptureDevice CaptureDevice { get; private set; }

        public AVCaptureStillImageOutput StillImageOutput { get; private set; }

        public bool IsPreviewing { get; set; }

        public IOSCameraView(CameraDeviceOptions options)
        {
            NSNotificationCenter.DefaultCenter.AddObserver(UIApplication.DidChangeStatusBarOrientationNotification, PositionCameraPreview);

            cameraOptions = options;
            IsPreviewing = false;

            Initialize();
        }

        public override void Draw(CGRect rect)
        {
            base.Draw(rect);
            previewLayer.Frame = rect;
        }

        public override void TouchesBegan(NSSet touches, UIEvent evt)
        {
            base.TouchesBegan(touches, evt);
            OnTapped();
        }

        protected virtual void OnTapped()
        {
            var eventHandler = Tapped;
            if (eventHandler != null)
            {
                eventHandler(this, new EventArgs());
            }
        }

        void Initialize()
        {
            CaptureSession = new AVCaptureSession();

            previewLayer = new AVCaptureVideoPreviewLayer(CaptureSession)
            {
                Frame = Bounds,
                VideoGravity = AVLayerVideoGravity.Resize,
            };


            var videoDevices = AVCaptureDevice.DevicesWithMediaType(AVMediaType.Video);
            var cameraPosition = (cameraOptions == CameraDeviceOptions.Front) ? AVCaptureDevicePosition.Front : AVCaptureDevicePosition.Back;
            CaptureDevice = videoDevices.FirstOrDefault(d => d.Position == cameraPosition);

            if (CaptureDevice == null)
            {
                return;
            }

            ConfigureCameraForDevice(CaptureDevice);

            NSError error;
            var input = new AVCaptureDeviceInput(CaptureDevice, out error);

            StillImageOutput = new AVCaptureStillImageOutput()
            {
                OutputSettings = new NSDictionary()
            };

            CaptureSession.AddOutput(StillImageOutput);
            CaptureSession.AddInput(input);
            Layer.AddSublayer(previewLayer);


            CaptureSession.StartRunning();

            PositionCameraPreview(null);

            IsPreviewing = true;
        }

        public override CGRect Frame
        {
            get
            {
                return base.Frame;
            }
            set
            {
                base.Frame = value;
                if (previewLayer != null)
                {
                    previewLayer.Frame = value;
                }
            }
        }

        //public override void LayoutSubviews()
        //{
        //    base.LayoutSubviews();
        //    previewLayer.Frame = Bounds;

        //}

        void PositionCameraPreview(NSNotification notification)
        {
            var currentPosition = GetVideoOrientationFromDevice();

            if (previewLayer?.Connection != null)
            {
                if (previewLayer.Connection.SupportsVideoOrientation)
                    previewLayer.Connection.VideoOrientation = currentPosition;
            }

            foreach (var conn in StillImageOutput.Connections)
            {
                if (conn.SupportsVideoOrientation)
                    conn.VideoOrientation = currentPosition;
            }
        }

        AVCaptureVideoOrientation GetVideoOrientationFromDevice()
        {
            var orientation = UIApplication.SharedApplication.StatusBarOrientation;
            AVCaptureVideoOrientation result = AVCaptureVideoOrientation.Portrait;

            switch (orientation)
            {
                case UIInterfaceOrientation.PortraitUpsideDown:
                    result = AVCaptureVideoOrientation.PortraitUpsideDown;
                    break;
                case UIInterfaceOrientation.Portrait:
                    result = AVCaptureVideoOrientation.Portrait;
                    break;
                case UIInterfaceOrientation.LandscapeLeft:
                    result = AVCaptureVideoOrientation.LandscapeLeft;
                    break;
                case UIInterfaceOrientation.LandscapeRight:
                    result = AVCaptureVideoOrientation.LandscapeRight;
                    break;
                default:
                    break;
            }

            return result;
        }

        public void ConfigureCameraForDevice(AVCaptureDevice device)
        {
            var error = new NSError();
            if (device.IsFocusModeSupported(AVCaptureFocusMode.ContinuousAutoFocus))
            {
                device.LockForConfiguration(out error);
                device.FocusMode = AVCaptureFocusMode.ContinuousAutoFocus;
                device.UnlockForConfiguration();
            }
            else if (device.IsExposureModeSupported(AVCaptureExposureMode.ContinuousAutoExposure))
            {
                device.LockForConfiguration(out error);
                device.ExposureMode = AVCaptureExposureMode.ContinuousAutoExposure;
                device.UnlockForConfiguration();
            }
            else if (device.IsWhiteBalanceModeSupported(AVCaptureWhiteBalanceMode.ContinuousAutoWhiteBalance))
            {
                device.LockForConfiguration(out error);
                device.WhiteBalanceMode = AVCaptureWhiteBalanceMode.ContinuousAutoWhiteBalance;
                device.UnlockForConfiguration();
            }
        }

        public void SetCameraPosition(CameraDeviceOptions cameraPosition)
        {
            cameraOptions = cameraPosition;
            Initialize();
        }

        public void SetCameraZoom(double zoomFactor)
        {
            var error = new NSError();

            if (CaptureDevice != null && !CaptureDevice.RampingVideoZoom)
            {
                CaptureDevice.LockForConfiguration(out error);
                CaptureDevice.VideoZoomFactor = (nfloat)zoomFactor;
                CaptureDevice.UnlockForConfiguration();
            }

        }

        public async Task<Byte[]> CapturePhotoCropped(CropRatios cropRatios)
        {
            var videoConnection = StillImageOutput.ConnectionFromMediaType(AVMediaType.Video);
            var sampleBuffer = await StillImageOutput.CaptureStillImageTaskAsync(videoConnection);
            var jpegImageAsNsData = AVCaptureStillImageOutput.JpegStillToNSData(sampleBuffer);

            UIImage imageInfo = new UIImage(jpegImageAsNsData);

            // The following code would rotate the image based on the device orientation, but currently we lock to landscape.
            //****************************************

            //UIImageOrientation? orientationToApply = null;

            //switch (GetVideoOrientationFromDevice())
            //{
            //    case AVCaptureVideoOrientation.Portrait:
            //        orientationToApply = UIImageOrientation.Right;
            //        break;
            //    case AVCaptureVideoOrientation.LandscapeLeft:
            //        orientationToApply = UIImageOrientation.Down;
            //        break;
            //    case AVCaptureVideoOrientation.LandscapeRight:
            //        orientationToApply = null;
            //        break;
            //    case AVCaptureVideoOrientation.PortraitUpsideDown:
            //        orientationToApply = UIImageOrientation.Left;
            //        break;
            //    default:
            //        break;
            //}

            //var rotatedImage = ScaleAndRotateImage(imageInfo, orientationToApply);

            //****************************************

            nfloat cropPhotoX = (nfloat)(cropRatios.LeftRatio * imageInfo.Size.Width);
            nfloat cropPhotoY = (nfloat)(cropRatios.TopRatio * imageInfo.Size.Height);

            nfloat cropPhotoWidth = (nfloat)(imageInfo.Size.Width * (1 - (cropRatios.LeftRatio + cropRatios.RightRatio)));
            nfloat cropPhotoHeight = (nfloat)(imageInfo.Size.Height * (1 - (cropRatios.TopRatio + cropRatios.BottomRatio)));

            var croppedImage = CropImage(imageInfo, cropPhotoX, cropPhotoY, cropPhotoWidth, cropPhotoHeight);

            // Rotate after cropping since we are locking orentation to landscape. Otherwise this line should be removed.
            var rotatedImage = ScaleAndRotateImage(croppedImage, UIImageOrientation.Left);

            Byte[] imageByteArray;

            using (Foundation.NSData imageData = imageInfo.AsJPEG())
            {
                imageByteArray = new Byte[imageData.Length];
                System.Runtime.InteropServices.Marshal.Copy(imageData.Bytes, imageByteArray, 0, Convert.ToInt32(imageData.Length));
            }

            return imageByteArray;
        }

        public async Task<Stream> CapturePhoto()
        {
            var videoConnection = StillImageOutput.ConnectionFromMediaType(AVMediaType.Video);
            var sampleBuffer = await StillImageOutput.CaptureStillImageTaskAsync(videoConnection);
            var jpegImageAsNsData = AVCaptureStillImageOutput.JpegStillToNSData(sampleBuffer);

            return jpegImageAsNsData.AsStream();
        }

        public double GetMaxZoom()
        {
            return (float)Math.Min(CaptureDevice != null ? CaptureDevice.ActiveFormat.VideoMaxZoomFactor : 1, 6); ;
        }

        public double GetCurrentZoom()
        {
            return CaptureDevice.VideoZoomFactor;
        }

        public double GetMinZoom()
        {
            return 1;
        }

        public UIImage CropImage(UIImage imageIn, nfloat crop_x, nfloat crop_y, nfloat width, nfloat height)
        {
            var imgSize = imageIn.Size;
            UIGraphics.BeginImageContext(new SizeF(float.Parse(width.ToString()), float.Parse(height.ToString())));
            var context = UIGraphics.GetCurrentContext();
            var clippedRect = new RectangleF(0, 0, float.Parse(width.ToString()), float.Parse(height.ToString()));
            context.ClipToRect(clippedRect);
            var drawRect = new CGRect(-crop_x, -crop_y, imgSize.Width, imgSize.Height);
            imageIn.Draw(drawRect);
            var modifiedImage = UIGraphics.GetImageFromCurrentImageContext();
            UIGraphics.EndImageContext();


            return modifiedImage;

        }

        UIImage ScaleAndRotateImage(UIImage imageIn, UIImageOrientation? orient)
        {
            int kMaxResolution = 2048;

            CGImage imgRef = imageIn.CGImage;
            float width = imgRef.Width;
            float height = imgRef.Height;
            CGAffineTransform transform = CGAffineTransform.MakeIdentity();
            RectangleF bounds = new RectangleF(0, 0, width, height);

            if (width > kMaxResolution || height > kMaxResolution)
            {
                float ratio = width / height;

                if (ratio > 1)
                {
                    bounds.Width = kMaxResolution;
                    bounds.Height = bounds.Width / ratio;
                }
                else
                {
                    bounds.Height = kMaxResolution;
                    bounds.Width = bounds.Height * ratio;
                }
            }

            float scaleRatio = bounds.Width / width;
            SizeF imageSize = new SizeF(width, height);

            if (orient != null)
            {

                float boundHeight;

                switch (orient)
                {
                    case UIImageOrientation.Up:                                        //EXIF = 1
                        transform = CGAffineTransform.MakeIdentity();
                        break;

                    case UIImageOrientation.UpMirrored:                                //EXIF = 2
                        transform = CGAffineTransform.MakeTranslation(imageSize.Width, 0f);
                        transform = CGAffineTransform.MakeScale(-1.0f, 1.0f);
                        break;

                    case UIImageOrientation.Down:                                      //EXIF = 3
                        transform = CGAffineTransform.MakeTranslation(imageSize.Width, imageSize.Height);
                        transform = CGAffineTransform.Rotate(transform, (float)Math.PI);
                        break;

                    case UIImageOrientation.DownMirrored:                              //EXIF = 4
                        transform = CGAffineTransform.MakeTranslation(0f, imageSize.Height);
                        transform = CGAffineTransform.MakeScale(1.0f, -1.0f);
                        break;

                    case UIImageOrientation.LeftMirrored:                              //EXIF = 5
                        boundHeight = bounds.Height;
                        bounds.Height = bounds.Width;
                        bounds.Width = boundHeight;
                        transform = CGAffineTransform.MakeTranslation(imageSize.Height, imageSize.Width);
                        transform = CGAffineTransform.MakeScale(-1.0f, 1.0f);
                        transform = CGAffineTransform.Rotate(transform, 3.0f * (float)Math.PI / 2.0f);
                        break;

                    case UIImageOrientation.Left:                                      //EXIF = 6
                        boundHeight = bounds.Height;
                        bounds.Height = bounds.Width;
                        bounds.Width = boundHeight;
                        transform = CGAffineTransform.MakeTranslation(0.0f, imageSize.Width);
                        transform = CGAffineTransform.Rotate(transform, 3.0f * (float)Math.PI / 2.0f);
                        break;

                    case UIImageOrientation.RightMirrored:                             //EXIF = 7
                        boundHeight = bounds.Height;
                        bounds.Height = bounds.Width;
                        bounds.Width = boundHeight;
                        transform = CGAffineTransform.MakeScale(-1.0f, 1.0f);
                        transform = CGAffineTransform.Rotate(transform, (float)Math.PI / 2.0f);
                        break;

                    case UIImageOrientation.Right:                                     //EXIF = 8
                        boundHeight = bounds.Height;
                        bounds.Height = bounds.Width;
                        bounds.Width = boundHeight;
                        transform = CGAffineTransform.MakeTranslation(imageSize.Height, 0.0f);
                        transform = CGAffineTransform.Rotate(transform, (float)Math.PI / 2.0f);
                        break;

                    default:
                        throw new Exception("Invalid image orientation");
                        break;
                }
            }

            UIGraphics.BeginImageContext(bounds.Size);

            CGContext context = UIGraphics.GetCurrentContext();

            if (orient == UIImageOrientation.Right || orient == UIImageOrientation.Left)
            {
                context.ScaleCTM(-scaleRatio, scaleRatio);
                context.TranslateCTM(-height, 0);
            }
            else
            {
                context.ScaleCTM(scaleRatio, -scaleRatio);
                context.TranslateCTM(0, -height);
            }

            context.ConcatCTM(transform);
            context.DrawImage(new RectangleF(0, 0, width, height), imgRef);

            UIImage imageCopy = UIGraphics.GetImageFromCurrentImageContext();
            UIGraphics.EndImageContext();

            return imageCopy;
        }
    }
    namespace CognitiveEye.iOS.Utilities
    {

        public class IOSCameraView : UIView
        {
            AVCaptureVideoPreviewLayer previewLayer;
            CameraDeviceOptions cameraOptions;

            public event EventHandler<EventArgs> Tapped;

            public AVCaptureSession CaptureSession { get; private set; }

            public AVCaptureDevice CaptureDevice { get; private set; }

            public AVCaptureStillImageOutput StillImageOutput { get; private set; }

            public bool IsPreviewing { get; set; }

            public IOSCameraView(CameraDeviceOptions options)
            {
                NSNotificationCenter.DefaultCenter.AddObserver(UIApplication.DidChangeStatusBarOrientationNotification, PositionCameraPreview);

                cameraOptions = options;
                IsPreviewing = false;

                Initialize();
            }

            public override void Draw(CGRect rect)
            {
                base.Draw(rect);
                previewLayer.Frame = rect;
            }

            public override void TouchesBegan(NSSet touches, UIEvent evt)
            {
                base.TouchesBegan(touches, evt);
                OnTapped();
            }

            protected virtual void OnTapped()
            {
                var eventHandler = Tapped;
                if (eventHandler != null)
                {
                    eventHandler(this, new EventArgs());
                }
            }

            void Initialize()
            {
                CaptureSession = new AVCaptureSession();

                previewLayer = new AVCaptureVideoPreviewLayer(CaptureSession)
                {
                    Frame = Bounds,
                    VideoGravity = AVLayerVideoGravity.Resize,
                };


                var videoDevices = AVCaptureDevice.DevicesWithMediaType(AVMediaType.Video);
                var cameraPosition = (cameraOptions == CameraDeviceOptions.Front) ? AVCaptureDevicePosition.Front : AVCaptureDevicePosition.Back;
                CaptureDevice = videoDevices.FirstOrDefault(d => d.Position == cameraPosition);

                if (CaptureDevice == null)
                {
                    return;
                }

                ConfigureCameraForDevice(CaptureDevice);

                NSError error;
                var input = new AVCaptureDeviceInput(CaptureDevice, out error);

                StillImageOutput = new AVCaptureStillImageOutput()
                {
                    OutputSettings = new NSDictionary()
                };

                CaptureSession.AddOutput(StillImageOutput);
                CaptureSession.AddInput(input);
                Layer.AddSublayer(previewLayer);


                CaptureSession.StartRunning();

                PositionCameraPreview(null);

                IsPreviewing = true;
            }

            public override CGRect Frame
            {
                get
                {
                    return base.Frame;
                }
                set
                {
                    base.Frame = value;
                    if (previewLayer != null)
                    {
                        previewLayer.Frame = value;
                    }
                }
            }

            //public override void LayoutSubviews()
            //{
            //    base.LayoutSubviews();
            //    previewLayer.Frame = Bounds;

            //}

            void PositionCameraPreview(NSNotification notification)
            {
                var currentPosition = GetVideoOrientationFromDevice();

                if (previewLayer?.Connection != null)
                {
                    if (previewLayer.Connection.SupportsVideoOrientation)
                        previewLayer.Connection.VideoOrientation = currentPosition;
                }

                foreach (var conn in StillImageOutput.Connections)
                {
                    if (conn.SupportsVideoOrientation)
                        conn.VideoOrientation = currentPosition;
                }
            }

            AVCaptureVideoOrientation GetVideoOrientationFromDevice()
            {
                var orientation = UIApplication.SharedApplication.StatusBarOrientation;
                AVCaptureVideoOrientation result = AVCaptureVideoOrientation.Portrait;

                switch (orientation)
                {
                    case UIInterfaceOrientation.PortraitUpsideDown:
                        result = AVCaptureVideoOrientation.PortraitUpsideDown;
                        break;
                    case UIInterfaceOrientation.Portrait:
                        result = AVCaptureVideoOrientation.Portrait;
                        break;
                    case UIInterfaceOrientation.LandscapeLeft:
                        result = AVCaptureVideoOrientation.LandscapeLeft;
                        break;
                    case UIInterfaceOrientation.LandscapeRight:
                        result = AVCaptureVideoOrientation.LandscapeRight;
                        break;
                    default:
                        break;
                }

                return result;
            }

            public void ConfigureCameraForDevice(AVCaptureDevice device)
            {
                var error = new NSError();
                if (device.IsFocusModeSupported(AVCaptureFocusMode.ContinuousAutoFocus))
                {
                    device.LockForConfiguration(out error);
                    device.FocusMode = AVCaptureFocusMode.ContinuousAutoFocus;
                    device.UnlockForConfiguration();
                }
                else if (device.IsExposureModeSupported(AVCaptureExposureMode.ContinuousAutoExposure))
                {
                    device.LockForConfiguration(out error);
                    device.ExposureMode = AVCaptureExposureMode.ContinuousAutoExposure;
                    device.UnlockForConfiguration();
                }
                else if (device.IsWhiteBalanceModeSupported(AVCaptureWhiteBalanceMode.ContinuousAutoWhiteBalance))
                {
                    device.LockForConfiguration(out error);
                    device.WhiteBalanceMode = AVCaptureWhiteBalanceMode.ContinuousAutoWhiteBalance;
                    device.UnlockForConfiguration();
                }
            }

            public void SetCameraPosition(CameraDeviceOptions cameraPosition)
            {
                cameraOptions = cameraPosition;
                Initialize();
            }

            public void SetCameraZoom(double zoomFactor)
            {
                var error = new NSError();

                if (CaptureDevice != null && !CaptureDevice.RampingVideoZoom)
                {
                    CaptureDevice.LockForConfiguration(out error);
                    CaptureDevice.VideoZoomFactor = (nfloat)zoomFactor;
                    CaptureDevice.UnlockForConfiguration();
                }

            }

            public async Task<Byte[]> CapturePhoto(CropRatios cropRatios)
            {
                var videoConnection = StillImageOutput.ConnectionFromMediaType(AVMediaType.Video);
                var sampleBuffer = await StillImageOutput.CaptureStillImageTaskAsync(videoConnection);
                var jpegImageAsNsData = AVCaptureStillImageOutput.JpegStillToNSData(sampleBuffer);

                UIImage imageInfo = new UIImage(jpegImageAsNsData);

                // The following code would rotate the image based on the device orientation, but currently we lock to landscape.
                //****************************************

                //UIImageOrientation? orientationToApply = null;

                //switch (GetVideoOrientationFromDevice())
                //{
                //    case AVCaptureVideoOrientation.Portrait:
                //        orientationToApply = UIImageOrientation.Right;
                //        break;
                //    case AVCaptureVideoOrientation.LandscapeLeft:
                //        orientationToApply = UIImageOrientation.Down;
                //        break;
                //    case AVCaptureVideoOrientation.LandscapeRight:
                //        orientationToApply = null;
                //        break;
                //    case AVCaptureVideoOrientation.PortraitUpsideDown:
                //        orientationToApply = UIImageOrientation.Left;
                //        break;
                //    default:
                //        break;
                //}

                //var rotatedImage = ScaleAndRotateImage(imageInfo, orientationToApply);

                //****************************************

                nfloat cropPhotoX = (nfloat)(cropRatios.LeftRatio * imageInfo.Size.Width);
                nfloat cropPhotoY = (nfloat)(cropRatios.TopRatio * imageInfo.Size.Height);

                nfloat cropPhotoWidth = (nfloat)(imageInfo.Size.Width * (1 - (cropRatios.LeftRatio + cropRatios.RightRatio)));
                nfloat cropPhotoHeight = (nfloat)(imageInfo.Size.Height * (1 - (cropRatios.TopRatio + cropRatios.BottomRatio)));

                var croppedImage = CropImage(imageInfo, cropPhotoX, cropPhotoY, cropPhotoWidth, cropPhotoHeight);

                // Rotate after cropping since we are locking orentation to landscape. Otherwise this line should be removed.
                var rotatedImage = ScaleAndRotateImage(croppedImage, UIImageOrientation.Left);

                Byte[] imageByteArray;

                using (Foundation.NSData imageData = rotatedImage.AsPNG())
                {
                    imageByteArray = new Byte[imageData.Length];
                    System.Runtime.InteropServices.Marshal.Copy(imageData.Bytes, imageByteArray, 0, Convert.ToInt32(imageData.Length));
                }

                return imageByteArray;
            }

            public double GetMaxZoom()
            {
                return (float)Math.Min(CaptureDevice != null ? CaptureDevice.ActiveFormat.VideoMaxZoomFactor : 1, 6); ;
            }

            public double GetCurrentZoom()
            {
                return CaptureDevice.VideoZoomFactor;
            }

            public double GetMinZoom()
            {
                return 1;
            }

            public UIImage CropImage(UIImage imageIn, nfloat crop_x, nfloat crop_y, nfloat width, nfloat height)
            {
                var imgSize = imageIn.Size;
                UIGraphics.BeginImageContext(new SizeF(float.Parse(width.ToString()), float.Parse(height.ToString())));
                var context = UIGraphics.GetCurrentContext();
                var clippedRect = new RectangleF(0, 0, float.Parse(width.ToString()), float.Parse(height.ToString()));
                context.ClipToRect(clippedRect);
                var drawRect = new CGRect(-crop_x, -crop_y, imgSize.Width, imgSize.Height);
                imageIn.Draw(drawRect);
                var modifiedImage = UIGraphics.GetImageFromCurrentImageContext();
                UIGraphics.EndImageContext();


                return modifiedImage;

            }

            UIImage ScaleAndRotateImage(UIImage imageIn, UIImageOrientation? orient)
            {
                int kMaxResolution = 2048;

                CGImage imgRef = imageIn.CGImage;
                float width = imgRef.Width;
                float height = imgRef.Height;
                CGAffineTransform transform = CGAffineTransform.MakeIdentity();
                RectangleF bounds = new RectangleF(0, 0, width, height);

                if (width > kMaxResolution || height > kMaxResolution)
                {
                    float ratio = width / height;

                    if (ratio > 1)
                    {
                        bounds.Width = kMaxResolution;
                        bounds.Height = bounds.Width / ratio;
                    }
                    else
                    {
                        bounds.Height = kMaxResolution;
                        bounds.Width = bounds.Height * ratio;
                    }
                }

                float scaleRatio = bounds.Width / width;
                SizeF imageSize = new SizeF(width, height);

                if (orient != null)
                {

                    float boundHeight;

                    switch (orient)
                    {
                        case UIImageOrientation.Up:                                        //EXIF = 1
                            transform = CGAffineTransform.MakeIdentity();
                            break;

                        case UIImageOrientation.UpMirrored:                                //EXIF = 2
                            transform = CGAffineTransform.MakeTranslation(imageSize.Width, 0f);
                            transform = CGAffineTransform.MakeScale(-1.0f, 1.0f);
                            break;

                        case UIImageOrientation.Down:                                      //EXIF = 3
                            transform = CGAffineTransform.MakeTranslation(imageSize.Width, imageSize.Height);
                            transform = CGAffineTransform.Rotate(transform, (float)Math.PI);
                            break;

                        case UIImageOrientation.DownMirrored:                              //EXIF = 4
                            transform = CGAffineTransform.MakeTranslation(0f, imageSize.Height);
                            transform = CGAffineTransform.MakeScale(1.0f, -1.0f);
                            break;

                        case UIImageOrientation.LeftMirrored:                              //EXIF = 5
                            boundHeight = bounds.Height;
                            bounds.Height = bounds.Width;
                            bounds.Width = boundHeight;
                            transform = CGAffineTransform.MakeTranslation(imageSize.Height, imageSize.Width);
                            transform = CGAffineTransform.MakeScale(-1.0f, 1.0f);
                            transform = CGAffineTransform.Rotate(transform, 3.0f * (float)Math.PI / 2.0f);
                            break;

                        case UIImageOrientation.Left:                                      //EXIF = 6
                            boundHeight = bounds.Height;
                            bounds.Height = bounds.Width;
                            bounds.Width = boundHeight;
                            transform = CGAffineTransform.MakeTranslation(0.0f, imageSize.Width);
                            transform = CGAffineTransform.Rotate(transform, 3.0f * (float)Math.PI / 2.0f);
                            break;

                        case UIImageOrientation.RightMirrored:                             //EXIF = 7
                            boundHeight = bounds.Height;
                            bounds.Height = bounds.Width;
                            bounds.Width = boundHeight;
                            transform = CGAffineTransform.MakeScale(-1.0f, 1.0f);
                            transform = CGAffineTransform.Rotate(transform, (float)Math.PI / 2.0f);
                            break;

                        case UIImageOrientation.Right:                                     //EXIF = 8
                            boundHeight = bounds.Height;
                            bounds.Height = bounds.Width;
                            bounds.Width = boundHeight;
                            transform = CGAffineTransform.MakeTranslation(imageSize.Height, 0.0f);
                            transform = CGAffineTransform.Rotate(transform, (float)Math.PI / 2.0f);
                            break;

                        default:
                            throw new Exception("Invalid image orientation");
                            break;
                    }
                }

                UIGraphics.BeginImageContext(bounds.Size);

                CGContext context = UIGraphics.GetCurrentContext();

                if (orient == UIImageOrientation.Right || orient == UIImageOrientation.Left)
                {
                    context.ScaleCTM(-scaleRatio, scaleRatio);
                    context.TranslateCTM(-height, 0);
                }
                else
                {
                    context.ScaleCTM(scaleRatio, -scaleRatio);
                    context.TranslateCTM(0, -height);
                }

                context.ConcatCTM(transform);
                context.DrawImage(new RectangleF(0, 0, width, height), imgRef);

                UIImage imageCopy = UIGraphics.GetImageFromCurrentImageContext();
                UIGraphics.EndImageContext();

                return imageCopy;
            }

        }
    }
}
