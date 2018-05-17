using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using CognitiveEye.Forms;
using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Training.Models;
using Xamarin.Forms;

namespace CongnitiveEye.Forms.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        public LoginViewModel()
        {
            Title = "Login";

            TrainingKey = App.TrainingKey;
        }

        #region Bindable Props

        string trainingKey = string.Empty;
       
        public string TrainingKey
        {
            get => trainingKey;
            set
            {
                SetProperty(ref trainingKey, value);
                App.TrainingKey = value;
            }
        }

        string predictionKey = string.Empty;

        public string PredictionKey
        {
            get => predictionKey;
            set => SetProperty(ref predictionKey, value);
        }

        #endregion

        #region Commands

        ICommand loginCommand;
        public ICommand LoginCommand =>
            loginCommand ?? (loginCommand = new Command(async () => await ExecuteLoginAsync()));

        private async Task ExecuteLoginAsync()
        {
            ShowBusy("Logging In...", Acr.UserDialogs.MaskType.Gradient);

            // Set the training key
            App.AppTrainingApi = new Microsoft.Azure.CognitiveServices.Vision.CustomVision.Training.TrainingApi()
            {
                ApiKey = TrainingKey
            };

            // Get Projects
            var projects = await App.AppTrainingApi.GetProjectsWithHttpMessagesAsync();

            // Push a project view on the stack and pass the project we just received
            await NavService.PushAsync<ProjectsViewModel>(new ProjectsViewModel
            {
                Projects = new ObservableCollection<Project>(projects.Body)
            });

            HideBusy();
        }

        #endregion


    }
}
