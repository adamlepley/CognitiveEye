using System;
using Xamarin.Forms;
using System.Threading.Tasks;
using System.IO;

namespace CongnitiveEye.Forms.Renderers
{
    public enum CameraDeviceOptions
    {
        Rear,
        Front
    }

    public class CameraDeviceSettings
    {
        public double MinZoom { get; set; }

        public double MaxZoom { get; set; }

        public double CurrentZoom { get; set; }
    }

    public class CameraView : View
    {

        public Func<CropRatios, Task<Stream>> TakePhoto;

        public Func<CameraDeviceSettings> GetCameraDeviceSettings;

        public static readonly BindableProperty DeviceOptionsProperty = BindableProperty.Create(
            propertyName: "DeviceOptions",
            returnType: typeof(CameraDeviceOptions),
            declaringType: typeof(CameraView),
            defaultValue: CameraDeviceOptions.Rear);

        public CameraDeviceOptions DeviceOptions
        {
            get { return (CameraDeviceOptions)GetValue(DeviceOptionsProperty); }
            set { SetValue(DeviceOptionsProperty, value); }
        }

        public static readonly BindableProperty ZoomProperty = BindableProperty.Create(
            propertyName: "Zoom",
            returnType: typeof(double),
            declaringType: typeof(CameraView),
            defaultValue: 1.0);

        public double Zoom
        {
            get { return (double)GetValue(ZoomProperty); }
            set { SetValue(ZoomProperty, value); }
        }
    }

    public class CropRatios
    {
        public double LeftRatio { get; set; }
        public double TopRatio { get; set; }
        public double RightRatio { get; set; }
        public double BottomRatio { get; set; }
        public bool IsPortait { get; set; }
    }
}


