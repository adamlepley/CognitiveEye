using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using CognitiveEye.Forms;
using CongnitiveEye.Forms.Views;
using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Training.Models;
using Xamarin.Forms;

namespace CongnitiveEye.Forms.ViewModels
{
    public class ProjectMenuViewModel : BaseViewModel
    {
        public Domain CurrentDomain;

        public ProjectMenuViewModel()
        {
            Title = App.SelectedProject.Name;

            LoadDomains().ConfigureAwait(false);
        }

        async Task LoadDomains()
        {
            var domains = await App.AppTrainingApi.GetDomainsWithHttpMessagesAsync();

            if (domains?.Body == null)
                return;

            var domainList = new List<Domain>(domains.Body);

            CurrentDomain = domains.Body.Where((arg) => arg.Id == App.SelectedProject.Settings.DomainId).FirstOrDefault();
        }

        #region Commands

        ICommand selectTrain;
        public ICommand SelectTrain =>
            selectTrain ?? (selectTrain = new Command(async () => await ExecuteSelectTrain()));

        private async Task ExecuteSelectTrain()
        {
            await NavService.PushAsync<ProjectTagsViewModel>(new ProjectTagsViewModel());      }

        ICommand selectCloudVision;
        public ICommand SelectCloudVision =>
        selectCloudVision ?? (selectCloudVision = new Command(async () => await ExecuteSelectCloudVision()));

        private async Task ExecuteSelectCloudVision()
        {
            await NavService.PushAsync<ProjectIterationsViewModel>(new ProjectIterationsViewModel());
        }

        ICommand selectDeviceVision;
        public ICommand SelectDeviceVision =>
        selectDeviceVision ?? (selectDeviceVision = new Command(async () => await ExecuteSelectDeviceVision()));

        private async Task ExecuteSelectDeviceVision()
        {
            if (CurrentDomain == null)
                await LoadDomains();

            if (CurrentDomain != null && CurrentDomain.Exportable)
            {
                await Application.Current.MainPage.Navigation.PushModalAsync(new DeviceVisionView()
                {
                    ViewModel = new DeviceVisionViewModel()
                });
            }
            else
            {
                await Application.Current.MainPage.DisplayAlert("Feature Unavailable", "Your project must be a compact type to use this feature", "OK");
            }

        }

        #endregion
    }
}
