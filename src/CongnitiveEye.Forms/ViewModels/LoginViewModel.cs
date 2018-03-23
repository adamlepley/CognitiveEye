using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Cognitive.CustomVision.Training.Models;
using Xamarin.Forms;

namespace CongnitiveEye.Forms.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        public LoginViewModel()
        {
            Title = "Login";
        }

        #region Bindable Props

        string trainingKey = string.Empty;
       
        public string TrainingKey
        {
            get => trainingKey;
            set => SetProperty(ref trainingKey, value);
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
            // Set the training key
            var trainingApi = new Microsoft.Cognitive.CustomVision.Training.TrainingApi()
            {
                ApiKey = TrainingKey
            };

            // Get Projects
            var projects = await trainingApi.GetProjectsWithHttpMessagesAsync();

            // Push a project view on the stack and pass the project we just received
            await NavService.PushAsync<ProjectsViewModel>(new ProjectsViewModel
            {
                Projects = new ObservableCollection<Project>(projects.Body)
            });
        }

        #endregion


    }
}
