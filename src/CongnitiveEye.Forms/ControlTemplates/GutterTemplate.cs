using System;
using CongnitiveEye.Forms.Utilities;
using Xamarin.Forms;

namespace CongnitiveEye.Forms.ControlTemplates
{
    public class GutterTemplate : Grid
    {
        public GutterTemplate()
        {
            Children.Add(new Frame()
            {
                Margin = 0,
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Fill
            });

            Children.Add(new BoxView()
            {
                Color = ColorUtil.GutterBorderColor,
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Fill
            });

            Children.Add(new BoxView()
            {
                Color = ColorUtil.GutterBackgroundColor,
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Fill,
                Margin = new Thickness(0, 1)
            });

            Children.Add(new ContentPresenter()
            {
                Margin = new Thickness(1)
            });
        }
    }
}
