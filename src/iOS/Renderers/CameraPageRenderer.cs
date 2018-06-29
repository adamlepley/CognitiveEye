using System;
using CognitiveEye.iOS.Renderers;
using CognitiveEye.iOS.Utilities;
using CongnitiveEye.Forms.Renderers;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(CameraView), typeof(CameraViewRenderer))]
namespace CognitiveEye.iOS.Renderers
{
    public class CameraViewRenderer : ViewRenderer<CameraView, IOSCameraView>
    {
        IOSCameraView iosCameraView;

        protected override void OnElementPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            if (e.PropertyName == "Camera")
            {
                iosCameraView.SetCameraPosition((sender as CameraView).DeviceOptions);
            }

            if (e.PropertyName == "Zoom")
            {
                CameraView camView = sender as CameraView;
                double zoomLevel = ((camView.Zoom / 100) * (iosCameraView.GetMaxZoom() - iosCameraView.GetMinZoom())) + iosCameraView.GetMinZoom();

                iosCameraView.SetCameraZoom(zoomLevel);
            }
        }

        protected override void OnElementChanged(ElementChangedEventArgs<CameraView> e)
        {
            base.OnElementChanged(e);
            if (Control == null)
            {
                iosCameraView = new IOSCameraView(e.NewElement.DeviceOptions);
                SetNativeControl(iosCameraView);

                e.NewElement.TakePhoto = async (ratios) =>
                {
                    var imageArray = await iosCameraView.CapturePhoto();
                    return imageArray;
                };

                e.NewElement.GetCameraDeviceSettings = () =>
                {
                    var zoomSettings = new CameraDeviceSettings()
                    {
                        MaxZoom = iosCameraView.GetMaxZoom(),
                        MinZoom = iosCameraView.GetMinZoom(),
                        CurrentZoom = iosCameraView.GetCurrentZoom()
                    };

                    return zoomSettings;
                };

            }
        }

        void OnCameraPreviewTapped(object sender, EventArgs e)
        {
            if (iosCameraView.IsPreviewing)
            {
                iosCameraView.CaptureSession.StopRunning();
                iosCameraView.IsPreviewing = false;
            }
            else
            {
                iosCameraView.CaptureSession.StartRunning();
                iosCameraView.IsPreviewing = true;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Control.CaptureSession.Dispose();
                Control.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
