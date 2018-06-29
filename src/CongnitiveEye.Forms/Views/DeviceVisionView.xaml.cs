using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using CongnitiveEye.Forms.ViewModels;
using Microcharts;
using Xamarin.Forms;
using System.Linq;

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

            //Chart.Chart = new BarChart()
            //{
            //    IsAnimated = false,
            //    Entries = ViewModel.ResultEntries,
            //    LabelTextSize = 20f,
            //};
        }

        void StartClassifier(object sender, System.EventArgs e)
        {
            ViewModel.ConfigImageClassifier(true);
            StartImageClassifier().ConfigureAwait(false);
        }

        async Task StartImageClassifier()
        {
            while (ViewModel.ImageClassifierRunning)
            {
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

                    //ViewModel.ResultEntries.Add(new Microcharts.Entry((float)result.Probability)
                    //{
                    //    Label = result.Tag,
                    //    ValueLabel = Math.Round(result.Probability * 100).ToString() + "%"
                    //});

                }

                //Chart.Chart.Entries = ViewModel.ResultEntries;

                await Task.Delay(1000); // arbitrary delay

            }
        }
    }
}
