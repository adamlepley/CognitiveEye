using System;
using CongnitiveEye.Forms.ControlTemplates;
using Xamarin.Forms;

namespace CongnitiveEye.Forms.Utilities
{
    public static class ColorUtil
    {
        public static Color PrimaryColor = Color.FromHex("#043151");
        public static Color AccentColor = Color.FromHex("#EA911C");
        public static Color ActionColor = Color.FromHex("#34825D");
        public static Color SecondaryColor = Color.FromHex("#DADFE1");
        public static Color AlertColor = Color.FromHex("#F57469");
        public static Color StartColor = Color.FromHex("#69f595");

        // Tile Colors
        public static Color TileBackgroundColor = Color.FromHex("#464546");
        public static Color TileBorderColor = Color.FromHex("#5d5d5d");

        // Text Colors
        public static Color PrimaryTextColor = Color.FromHex("#FFFFFF");
        public static Color SecondaryTextColor = Color.FromHex("#999999");
        public static Color AccentTextColor1 = Color.FromHex("#739ec6");
        public static Color AccentTextColor2 = Color.FromHex("#e1d155");

        // Gutter Colors
        public static Color GutterBackgroundColor = Color.FromHex("#383A43");
        public static Color GutterBorderColor = Color.FromHex("#242424");

        // Button Colors
        public static Color DeleteButtonColor = Color.FromHex("#fd7258");
        public static Color TrainButtonColor = Color.FromHex("#42c2a7");
        public static Color LoginButtonColor = Color.FromHex("#376c80");

        // Vision
        public static Color PositiveResultBackgroundColor = Color.Transparent;
        public static Color NegitiveResultBackgroundColor = Color.Transparent;
        public static Color PositiveResultTextColor = TrainButtonColor;
        public static Color NegitiveResultTextColor = AccentTextColor2;
    }

    public static class SizeUtil
    {
        public static double FooterHeight = 50d;
    }

    public static class TextUtil
    {
        public static string ApplicationName = "© Cognitive Eye";
        public static string FooterCredit = "Design / Developed by Adam Lepley";
    }

    public static class Templates
    {
        public static ControlTemplate TileTemplate = new ControlTemplate(typeof(TileTemplate));
        public static ControlTemplate GutterTemplate = new ControlTemplate(typeof(GutterTemplate));
    }
       
}
