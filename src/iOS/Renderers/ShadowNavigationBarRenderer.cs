using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;
using UIKit;
using CoreGraphics;
using CognitiveEye.iOS.Renderers;

[assembly: ExportRenderer(typeof(NavigationPage), typeof(ShadowNavigationBarRenderer))]
namespace CognitiveEye.iOS.Renderers
{
    public class ShadowNavigationBarRenderer : NavigationRenderer
    {
        protected override void OnElementChanged(VisualElementChangedEventArgs e)
        {
            base.OnElementChanged(e);

            if (this.Element == null) return;

            //NavigationBar.Layer.ShadowColor = UIColor.Black.CGColor;
            //NavigationBar.Layer.ShadowOffset = new CGSize(0, 3);
            //NavigationBar.Layer.ShadowOpacity = .5f;

            NavigationBar.Layer.ShadowRadius = 5;
            NavigationBar.Layer.ShadowColor = UIColor.Black.CGColor;
            NavigationBar.Layer.ShadowOpacity = 0.6f;
            NavigationBar.Layer.ShadowOffset = new CGSize(0, 3);;
        }
    }
}