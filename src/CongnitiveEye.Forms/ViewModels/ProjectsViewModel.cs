using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using CognitiveEye.Forms;
using Microsoft.Cognitive.CustomVision.Training.Models;
using Xamarin.Forms;

namespace CongnitiveEye.Forms.ViewModels
{
    public class ProjectsViewModel : BaseViewModel
    {
        public ProjectsViewModel()
        {
            Title = "Projects";
        }


        #region Bindable Props

        ObservableCollection<Project> projects = null;
        public ObservableCollection<Project> Projects
        {
            get => projects;
            set => SetProperty(ref projects, value);
        }


        #endregion

        #region Commands

        ICommand openProject;
        public ICommand OpenProject =>
            openProject ?? (openProject = new Command(async (selectedItem) => await ExecuteOpenProject(selectedItem)));

        private async Task ExecuteOpenProject(object selectedItem)
        {
            var selectedProject = (selectedItem as Project);

            if (selectedProject == null) { return; }

            App.SelectedProject = selectedProject;

            var action = await Application.Current.MainPage.DisplayActionSheet("What would you like to do?", "Cancel", null, "Computer Vision (Server)", "Computer Vision (Device)", "Learn & Train", "Manage Project");

            switch (action)
            {
                case "Computer Vision (Server)":
                    break;
                case "Computer Vision (Device)":
                    break;
                case "Learn & Train":
                    await NavService.PushAsync<ProjectTagsViewModel>(new ProjectTagsViewModel());
                    break;
                case "Manage Project":
                    break;
                default:
                    break;
            }

        }

        #endregion
    }
}   
