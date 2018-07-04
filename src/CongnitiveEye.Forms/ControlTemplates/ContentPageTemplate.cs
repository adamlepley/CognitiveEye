using System;
using CongnitiveEye.Forms.Utilities;
using CongnitiveEye.Forms.Views.Controls;
using FFImageLoading.Forms;
using FFImageLoading.Svg.Forms;
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
            var footer = new Footer();

            footer.SetValue(Grid.RowProperty, 1);

            mainGrid.Children.Add(footer);

            Content = mainGrid;
        }
    }
}
