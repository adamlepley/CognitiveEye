using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Acr.UserDialogs;
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

		public override void OnAppearing()
		{
			base.OnAppearing();

            LoadProjects().ConfigureAwait(false);
		}

		async Task LoadProjects()
        {
            // Get Projects
            var projects = await App.AppTrainingApi.GetProjectsWithHttpMessagesAsync();

            Projects = new ObservableCollection<Project>(projects.Body);
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

            var action = await Application.Current.MainPage.DisplayActionSheet("What would you like to do?", "Cancel", null, "Computer Vision (Server)", "Computer Vision (Device)", "Learn & Train");

            switch (action)
            {
                case "Computer Vision (Server)":
                    await NavService.PushAsync<ProjectIterationsViewModel>(new ProjectIterationsViewModel());
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

        ICommand addProject;
        public ICommand AddProject =>
        addProject ?? (addProject = new Command(async () => await ExecuteAddProject()));

        private async Task ExecuteAddProject()
        {
            var promptConfig = new PromptConfig()
            {
                InputType = InputType.Default,
                Title = "Enter the name of the project"
            };

            var newProjectName = await UserDialogs.Instance.PromptAsync(promptConfig);

            if (!newProjectName.Ok || string.IsNullOrWhiteSpace(newProjectName.Text)) { return; }

            promptConfig = new PromptConfig()
            {
                InputType = InputType.Default,
                Title = "Enter a description of the project"
            };

            var newProjectDescription = await UserDialogs.Instance.PromptAsync(promptConfig);

            if (!newProjectDescription.Ok || string.IsNullOrWhiteSpace(newProjectDescription.Text)) { return; }

            var newProject = await App.AppTrainingApi.CreateProjectWithHttpMessagesAsync(newProjectName.Text, newProjectDescription.Text);

            Projects.Add(newProject.Body);
        }

        #endregion
    }
}   
