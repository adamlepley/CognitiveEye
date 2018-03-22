using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace CongnitiveEye.Forms.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        public LoginViewModel()
        {
            //TODO: Load keys from local Storage/cache
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
            var trainingApi = new Microsoft.Cognitive.CustomVision.Training.TrainingApi()
            {
                ApiKey = TrainingKey
            };

            var projects = await trainingApi.GetProjectsWithHttpMessagesAsync();
        }

        #endregion


    }
}
