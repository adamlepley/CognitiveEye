using System;
using System.Collections.Generic;
using CongnitiveEye.Forms.ViewModels;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using Xamarin.Forms;
using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Training.Models;

namespace CongnitiveEye.Forms.Views
{
    public partial class ProjectIterationsView : BaseContentPage<ProjectIterationsViewModel>
    {
        public ProjectIterationsView()
        {
            InitializeComponent();
        }

        void ViewModel_PredictionMade(object sender, Prediction e)
        {
            BoundingBox.InvalidateSurface();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            ViewModel.PredictionMade += ViewModel_PredictionMade;
        }


        void BoundingBox_PaintSurface(object sender, SkiaSharp.Views.Forms.SKPaintSurfaceEventArgs e)
        {
            if (ViewModel.lastPrediction?.BoundingBox == null) { return; }

            var boundingBox = ViewModel.lastPrediction.BoundingBox;

            e.Surface.Canvas.Clear();

            var SKSkiaView = sender as SKCanvasView;

            double scaleFactor = e.Info.Width / SKSkiaView.Width;
            float width = (float)e.Info.Width;
            float height = (float)e.Info.Height;
            float lineStrokeWidth = 2 * (float)scaleFactor;

            float boxX = (float)((width * boundingBox.Left));
            float boxY = (float)((height * boundingBox.Top));
            float boxWidth = (float)((width * boundingBox.Width));
            float boxHeight = (float)((height * boundingBox.Height));


            var paint = new SkiaSharp.SKPaint()
            {
                Style = SkiaSharp.SKPaintStyle.Stroke,
                StrokeWidth = lineStrokeWidth,
                Color = SkiaSharp.SKColors.White,
                StrokeCap = SKStrokeCap.Round,
            };

            e.Surface.Canvas.DrawRect(boxX, boxY, boxWidth, boxHeight, paint);

        }
    }
}
