using System;
using System.Collections.Generic;
using System.Linq;
using CodeMill.VMFirstNav;
using CognitiveEye.Forms;

using Foundation;
using Microsoft.AppCenter;
using Plugin.DownloadManager;
using UIKit;

namespace CognitiveEye.iOS
{
    [Register("AppDelegate")]
    public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
    {
        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {

            global::Xamarin.Forms.Forms.Init();

            FFImageLoading.Forms.Platform.CachedImageRenderer.Init();

            new Xamarin.CustomControls.RepeaterView();

            LoadApplication(new App());

            NavigationService.Instance.RegisterViewModels(typeof(App).Assembly);

            return base.FinishedLaunching(app, options);
        }

        public override void HandleEventsForBackgroundUrl(UIApplication application, string sessionIdentifier, Action completionHandler)
        {
            CrossDownloadManager.BackgroundSessionCompletionHandler = completionHandler;
        }
    }
}
