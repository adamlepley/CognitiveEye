using System;
using System.Threading.Tasks;
using System.Windows.Input;
using CognitiveEye.Forms;
using Xamarin.Forms;

namespace CongnitiveEye.Forms.ViewModels
{
    public class ProjectMenuViewModel : BaseViewModel
    {
        public ProjectMenuViewModel()
        {
            Title = App.SelectedProject.Name;
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

        #endregion
    }
}
