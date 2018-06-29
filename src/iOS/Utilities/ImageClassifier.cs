using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using CongnitiveEye.Forms.Services;
using CoreGraphics;
using CoreML;
using CoreVideo;
using Foundation;
using Plugin.DownloadManager;
using UIKit;
using Vision;
using Xamarin.Forms;
using static CognitiveEye.iOS.Utilities.ImageClassifier;

[assembly: Dependency(typeof(ImageClassifierImplementation))]
namespace CognitiveEye.iOS.Utilities
{
    public class ImageClassifier
    {
        
        public class ImageClassifierImplementation : ImageClassifierBase
        {
            private static readonly CGSize _targetImageSize = new CGSize(227, 227);
            private VNCoreMLModel _model;

            private VNCoreMLModel LoadModel(string modelUrl)
            {

                var webClient = new WebClient();

                string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                string localFilename = "current.mlmodel";
                string localPath = Path.Combine(documentsPath, localFilename);

                webClient.DownloadFile(modelUrl,localPath);

                var fileUrl = NSUrl.FromFilename(localPath);

                //                var downloadManager = CrossDownloadManager.Current;
                //                var file = downloadManager.CreateDownloadFile(modelUrl);
                //                downloadManager.Start(file);

                //                while(file.Status != Plugin.DownloadManager.Abstractions.DownloadFileStatus.COMPLETED)
                //                {
                //                    System.Threading.Thread.Sleep(1000);
                //file.
                //}

                //var url = new NSUrl(file.Url);
                //var modelPath = NSBundle.MainBundle.GetUrlForResource(modelName, "mlmodelc") ?? CompileModel(modelName);

                //if (modelPath == null)
                //throw new ImageClassifierException($"Model {modelName} does not exist");
                var compliedModel = MLModel.CompileModel(fileUrl, out NSError complieErr);

                if (complieErr != null)
                    throw new NSErrorException(complieErr);
                       
                var mlModel = MLModel.Create(compliedModel, out NSError createErr);

                if (createErr != null)
                    throw new NSErrorException(createErr);

                var model = VNCoreMLModel.FromMLModel(mlModel, out NSError err);

                if (err != null)
                    throw new NSErrorException(err);

                return model;
            }

            private NSUrl CompileModel(string modelName)
            {
                var uncompiled = NSBundle.MainBundle.GetUrlForResource(modelName, "mlmodel");
                var modelPath = MLModel.CompileModel(uncompiled, out NSError err);

                if (err != null)
                    throw new NSErrorException(err);

                return modelPath;
            }

            private async Task<IReadOnlyList<ImageClassification>> Classify(UIImage source)
            {
                var tcs = new TaskCompletionSource<IEnumerable<ImageClassification>>();

                var request = new VNCoreMLRequest(_model, (response, e) =>
                {
                    if (e != null)
                        tcs.SetException(new NSErrorException(e));
                    else
                    {
                        var results = response.GetResults<VNClassificationObservation>();
                        tcs.SetResult(results.Select(r => new ImageClassification(r.Identifier, r.Confidence)).ToList());
                    }
                });

                var buffer = source.ToCVPixelBuffer(_targetImageSize);
                var requestHandler = new VNImageRequestHandler(buffer, new NSDictionary());

                requestHandler.Perform(new[] { request }, out NSError error);

                var classifications = await tcs.Task;

                if (error != null)
                    throw new NSErrorException(error);

                return classifications.OrderByDescending(p => p.Probability)
                                      .ToList()
                                      .AsReadOnly();
            }

            public override async Task<IReadOnlyList<ImageClassification>> ClassifyImage(Stream imageStream)
            {
                if (_model == null)
                    throw new ImageClassifierException("You must call Init before classifying images");

                try
                {
                    var image = await imageStream.ToUIImage();
                    return await Classify(image);
                }
                catch (Exception ex)
                {
                    throw new ImageClassifierException("Failed to classify image - check the inner exception for more details", ex);
                }
            }

            public override void Init(string modelUrl, ModelType modelType)
            {
                base.Init(modelUrl, modelType);

                try
                {
                    _model = LoadModel(modelUrl);
                }
                catch (Exception ex)
                {
                    throw new ImageClassifierException("Failed to load the model - check the inner exception for more details", ex);
                }
            }

        }




    }

    public static class ImageExtensions
    {
        public static CVPixelBuffer ToCVPixelBuffer(this UIImage image, CGSize size)
        {
            var attrs = new CVPixelBufferAttributes
            {
                CGImageCompatibility = true,
                CGBitmapContextCompatibility = true
            };
            var cgImg = image.Scale(size).CGImage;

            var pb = new CVPixelBuffer(cgImg.Width, cgImg.Height, CVPixelFormatType.CV32ARGB, attrs);
            pb.Lock(CVPixelBufferLock.None);
            var pData = pb.BaseAddress;
            var colorSpace = CGColorSpace.CreateDeviceRGB();
            var ctxt = new CGBitmapContext(pData, cgImg.Width, cgImg.Height, 8, pb.BytesPerRow, colorSpace, CGImageAlphaInfo.NoneSkipFirst);
            ctxt.TranslateCTM(0, cgImg.Height);
            ctxt.ScaleCTM(1.0f, -1.0f);
            UIGraphics.PushContext(ctxt);
            image.Draw(new CGRect(0, 0, cgImg.Width, cgImg.Height));
            UIGraphics.PopContext();
            pb.Unlock(CVPixelBufferLock.None);

            return pb;
        }

        public static async Task<UIImage> ToUIImage(this Stream imageStream)
        {
            var bytes = new byte[imageStream.Length];
            await imageStream.ReadAsync(bytes, 0, bytes.Length);

            return UIImage.LoadFromData(NSData.FromArray(bytes));
        }
    }


    public abstract class ImageClassifierBase : IImageClassifier
    {
        protected string ModelUrl { get; private set; }
        protected ModelType ModelType { get; private set; }

        public virtual void Init(string modelUrl, ModelType modelType)
        {
            if (string.IsNullOrEmpty(modelUrl))
                throw new ArgumentException("modelName must be set", nameof(modelUrl));

            ModelUrl = modelUrl;
            ModelType = modelType;
        }

        public abstract Task<IReadOnlyList<ImageClassification>> ClassifyImage(Stream imageStream);
    }



//    public static class CrossImageClassifier
//    {
//#if !NETSTANDARD1_0
//        private static ImageClassifierImplementation _implementation;
//#endif

//        public static IImageClassifier Current
//        {
//            get
//            {
//#if NETSTANDARD1_0
//                throw new NotImplementedException("Please ensure you have install the Xam.Plugins.OnDeviceCustomVision NuGet package into your iOS and Android projects");
//#else
//                return _implementation ?? (_implementation = new ImageClassifierImplementation());
//#endif
    //        }
    //    }
    //}
}
