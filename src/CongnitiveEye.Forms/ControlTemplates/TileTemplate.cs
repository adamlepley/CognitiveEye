using System;
using CongnitiveEye.Forms.Utilities;
using Xamarin.Forms;

namespace CongnitiveEye.Forms.ControlTemplates
{
    public class TileTemplate : Grid
    {
        public TileTemplate()
        {            
            Children.Add(new Frame()
            {
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Fill
            });

            Children.Add(new BoxView()
            {
                Color = ColorUtil.TileBorderColor,
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Fill
            });

            Children.Add(new BoxView()
            {
                Color = ColorUtil.TileBackgroundColor,
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Fill,
                Margin = new Thickness(1)
            });

            Children.Add(new ContentPresenter()
            {
                Margin =  new Thickness(1)
            });

        }
    }
}
