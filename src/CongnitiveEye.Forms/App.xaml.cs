using Xamarin.Forms;
using CongnitiveEye.Forms.Views;
using CongnitiveEye.Forms.ViewModels;
using DLToolkit.Forms.Controls;
using Microsoft.Cognitive.CustomVision.Training.Models;
using Microsoft.Cognitive.CustomVision.Training;

namespace CognitiveEye.Forms
{
    public partial class App : Application
    {

        #region Static Props

        public static Project SelectedProject { get; set; }

        public static TrainingApi AppTrainingApi { get; set; }

        #endregion

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
