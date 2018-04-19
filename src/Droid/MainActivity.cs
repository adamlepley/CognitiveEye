using System;

using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using CognitiveEye.Forms;
using CodeMill.VMFirstNav;
using Acr.UserDialogs;

namespace CognitiveEye.Droid
{
    [Activity(Label = "CognitiveEye.Droid", Icon = "@drawable/icon", Theme = "@style/MyTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(bundle);

            global::Xamarin.Forms.Forms.Init(this, bundle);

            UserDialogs.Init(this);

            LoadApplication(new App());

            NavigationService.Instance.RegisterViewModels(typeof(App).Assembly);
        }
    }
}
