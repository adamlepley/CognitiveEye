using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using CongnitiveEye.Forms.ViewModels;
using Microcharts;
using Xamarin.Forms;
using System.Linq;
using CongnitiveEye.Forms.Utilities;

namespace CongnitiveEye.Forms.Views
{
    public partial class DeviceVisionView : BaseContentPage<DeviceVisionViewModel>
    {
        public static Services.IImageClassifier ImageClassifier { get; set; }

        public DeviceVisionView()
        {
            InitializeComponent();

            if (ImageClassifier == null)
            {
                ImageClassifier = DependencyService.Get<Services.IImageClassifier>();
            }

        }

        protected override void OnAppearing()
        {
            base.OnAppearing();


        }

        void StartClassifier(object sender, System.EventArgs e)
        {
            ViewModel.ConfigImageClassifier(true);
            StartImageClassifier().ConfigureAwait(false);
        }

        async Task TagImage(object sender, System.EventArgs e)
        {
            ViewModel.Tagging = true;

            ViewModel.ImageClassifierRunning = false;

            while (!ViewModel.ImageClassifierStopped)
            {
                await Task.Delay(200);
            }

            Device.BeginInvokeOnMainThread(async () =>
            {
                Stream stream = await Cam.TakePhoto(new Renderers.CropRatios()
                {
                    BottomRatio = 1,
                    TopRatio = 1,
                    LeftRatio = 1,
                    RightRatio = 1
                });

                await ViewModel.TagPhoto(stream);

                ViewModel.Tagging = false;

                StartClassifier(null, null);
            });

        }

        async Task StartImageClassifier()
        {
            while (ViewModel.ImageClassifierRunning)
            {
                ViewModel.ImageClassifierStopped = false;

                Stream stream = await Cam.TakePhoto(new Renderers.CropRatios()
                {
                    BottomRatio = 1,
                    TopRatio = 1,
                    LeftRatio = 1,
                    RightRatio = 1
                });

                var resultSet = await ImageClassifier.ClassifyImage(stream);

                foreach (var result in resultSet.OrderBy((arg) => arg.Tag))
                {
                    var foundResultValue = ViewModel.ResultEntries.Where((arg) => arg.Name == result.Tag).FirstOrDefault();

                    if (foundResultValue != null)
                        foundResultValue.SetTagValue(result.Probability);

                }

                await Task.Delay(ViewModel.VisionClassifierInterval); // arbitrary delay
            }

            ViewModel.ImageClassifierStopped = true;
        }
    }
}
