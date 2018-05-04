using Xamarin.Forms;
using CongnitiveEye.Forms.Views;
using CongnitiveEye.Forms.ViewModels;
using DLToolkit.Forms.Controls;
using Microsoft.Cognitive.CustomVision.Training.Models;
using Microsoft.Cognitive.CustomVision.Training;
using Plugin.Settings;
using Plugin.Settings.Abstractions;

namespace CognitiveEye.Forms
{
    public partial class App : Application
    {

        #region Static Props

        public static Project SelectedProject { get; set; }

        public static TrainingApi AppTrainingApi { get; set; }

        private static ISettings AppSettings =>
            CrossSettings.Current;

        public static string TrainingKey
        {
            get => AppSettings.GetValueOrDefault(nameof(TrainingKey), string.Empty);
            set => AppSettings.AddOrUpdateValue(nameof(TrainingKey), value);
        }

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
