using Xamarin.Forms;
using CongnitiveEye.Forms.Views;
using CongnitiveEye.Forms.ViewModels;
using DLToolkit.Forms.Controls;

namespace CognitiveEye.Forms
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            FlowListView.Init();

            MainPage = new NavigationPage(new LoginView()
            {
                ViewModel = new LoginViewModel()
            });
        }

        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}
