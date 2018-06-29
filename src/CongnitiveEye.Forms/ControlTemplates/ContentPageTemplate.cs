using System;
using CongnitiveEye.Forms.Utilities;
using Xamarin.Forms;

namespace CongnitiveEye.Forms.ControlTemplates
{
    public class ContentPageTemplate : ContentView
    {
        public ContentPageTemplate()
        {
            var mainGrid = new Grid()
            {
                RowSpacing=0,
                RowDefinitions = new RowDefinitionCollection()
                {
                    new RowDefinition() { Height = GridLength.Star },
                    new RowDefinition() { Height = GridLength.Auto }
                }
            };

            // Background Image
            var backgroundImage = new Image()
            {
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Fill,
                Aspect = Aspect.AspectFill,
                Source = "background5.jpg",
            };

            backgroundImage.SetValue(Grid.RowSpanProperty, 2);
            mainGrid.Children.Add(backgroundImage);

            // Content Presenter
            mainGrid.Children.Add(new ContentPresenter()
            {
                Margin = new Thickness(0)
            });

            // Footer
            var footer = new Frame()
            {
                VerticalOptions = LayoutOptions.End,
                HorizontalOptions = LayoutOptions.Fill,
                HeightRequest = SizeUtil.FooterHeight,
                CornerRadius = 0,
                Margin = 0,
                Padding = 0,
                BackgroundColor = ColorUtil.TileBackgroundColor
            };

            footer.SetValue(Grid.RowProperty, 1);

            footer.Content = new Grid()
            {
                Children = 
                {
                    new Label()
                    {
                        Margin = new Thickness(20, 10),
                        Text = TextUtil.ApplicationName,
                        FontSize = 12,
                        TextColor = ColorUtil.PrimaryTextColor,
                        HorizontalOptions = LayoutOptions.Center
                    },
                    //new Label()
                    //{
                    //    Margin = new Thickness(20, 10),
                    //    Text = TextUtil.FooterCredit,
                    //    FontSize = 12,
                    //    TextColor = ColorUtil.PrimaryTextColor,
                    //    HorizontalOptions = LayoutOptions.End
                    //}
                }
            };

            mainGrid.Children.Add(footer);

            Content = mainGrid;
        }
    }
}
