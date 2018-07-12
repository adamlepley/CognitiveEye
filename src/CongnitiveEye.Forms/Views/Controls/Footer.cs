using System;
using CongnitiveEye.Forms.Utilities;
using FFImageLoading.Svg.Forms;
using Xamarin.Forms;

namespace CongnitiveEye.Forms.Views.Controls
{
    public class Footer : ContentView
    {
        double FooterExpandedHeight
        {
            get
            {
                return SizeUtil.FooterButtonIconSize + (SizeUtil.FooterButtonTopBottomMargin * 2);
            }
        }

        public FooterViewModel ViewModel;
        public bool Expanded { get; private set; }

        Frame OutterFrame { get; set; }

        public Footer()
        {
            Expanded = false;

            // Outter Frame
            OutterFrame = new Frame()
            {
                VerticalOptions = LayoutOptions.End,
                HorizontalOptions = LayoutOptions.Fill,
                HeightRequest = FooterExpandedHeight,
                CornerRadius = 0,
                Margin = 0,
                Padding = 0,
                BackgroundColor = ColorUtil.TileBackgroundColor
            };

            OutterFrame.SetValue(Grid.RowProperty, 1);

            var settingsButton = new SvgCachedImage()
            {
                Source = SvgImageSource.FromFile("settings.svg"),
                HeightRequest = SizeUtil.FooterButtonIconSize,
                WidthRequest = SizeUtil.FooterButtonIconSize,
                Margin = new Thickness(SizeUtil.FooterButtonLeftRightMargin, SizeUtil.FooterButtonTopBottomMargin),
                HorizontalOptions = LayoutOptions.End,
                VerticalOptions = LayoutOptions.Center
            };

            settingsButton.GestureRecognizers.Add(new TapGestureRecognizer()
            {
                Command = new Command((Sender) =>
                {

                    if (OutterFrame.AnimationIsRunning("opening"))
                        return;
                        
                    double startingHeight, endingHeight = 0d;

                    if (Expanded)
                    {
                        startingHeight = SizeUtil.FooterHeightExpanded;
                        endingHeight = FooterExpandedHeight;
                    }
                    else
                    {
                        startingHeight = FooterExpandedHeight;
                        endingHeight = SizeUtil.FooterHeightExpanded;
                    }
                  
                    Action<double> callback = input => OutterFrame.HeightRequest = input;
                    uint rate = 16;
                    uint length = 500;
                    Easing easing = Easing.CubicOut;

                    OutterFrame.Animate("opening", callback, startingHeight, endingHeight,rate, length, easing);

                    Expanded = !Expanded;
                })
            });

            OutterFrame.Content = new Grid()
            {
                VerticalOptions = LayoutOptions.Start,
                HeightRequest = FooterExpandedHeight,
                Children =
                {
                    new Label()
                    {
                        Margin = new Thickness(SizeUtil.FooterButtonLeftRightMargin, SizeUtil.FooterButtonTopBottomMargin),
                        Text = TextUtil.ApplicationName,
                        FontSize = 12,
                        TextColor = ColorUtil.PrimaryTextColor,
                        HorizontalOptions = LayoutOptions.Center
                    },
                    settingsButton,
                    new SvgCachedImage()
                    {
                        Source = SvgImageSource.FromFile("info.svg"),
                        HeightRequest = SizeUtil.FooterButtonIconSize,
                        WidthRequest = SizeUtil.FooterButtonIconSize,
                        Margin = new Thickness(SizeUtil.FooterButtonLeftRightMargin, 0),
                        HorizontalOptions = LayoutOptions.Start,
                        VerticalOptions = LayoutOptions.Center
                    }
                }
            };

            Content = OutterFrame;
        }
    }

    public class FooterViewModel
    {
    }
}

