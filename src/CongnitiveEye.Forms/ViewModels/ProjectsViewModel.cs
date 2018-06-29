using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Acr.UserDialogs;
using CognitiveEye.Forms;
using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Training.Models;
using Xamarin.Forms;

namespace CongnitiveEye.Forms.ViewModels
{
    public class ProjectsViewModel : BaseViewModel
    {

        public List<Domain> Domains;

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
            ShowBusy("Loading Projects...");

            // Get Projects
            var projects = await App.AppTrainingApi.GetProjectsWithHttpMessagesAsync();

            if (projects?.Body == null)
                return;
            
            Projects = new ObservableCollection<Project>(projects.Body);

            var domains = await App.AppTrainingApi.GetDomainsWithHttpMessagesAsync();

            if (domains?.Body == null)
                return;

            Domains = new List<Domain>(domains.Body);

            HideBusy();
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

            await NavService.PushAsync<ProjectMenuViewModel>(new ProjectMenuViewModel());

            //var action = await Application.Current.MainPage.DisplayActionSheet("What would you like to do?", "Cancel", null, "Computer Vision (Server)", "Computer Vision (Device)", "Learn & Train");

            //switch (action)
            //{
            //    case "Computer Vision (Server)":
            //        await NavService.PushAsync<ProjectIterationsViewModel>(new ProjectIterationsViewModel());
            //        break;
            //    case "Computer Vision (Device)":
            //        break;
            //    case "Learn & Train":
            //        await NavService.PushAsync<ProjectTagsViewModel>(new ProjectTagsViewModel());
            //        break;
            //    case "Manage Project":
            //        break;
            //    default:
            //        break;
            //}

        }

        ICommand addProject;
        public ICommand AddProject =>
        addProject ?? (addProject = new Command(async () => await ExecuteAddProject()));

        private async Task ExecuteAddProject()
        {
            // Ask for domain
            var listOfDomains = Domains.Select((arg) => string.Format("{0} - {1}", arg.Name, arg.Type)).ToArray<string>();

            var domainSelected = await Application.Current.MainPage.DisplayActionSheet(
                "What type of project would you like to create ? ",
                "Cancel",
                null,
                listOfDomains);

            if (domainSelected == null)
                return;

            var foundDomain = Domains.Where((arg) => string.Format("{0} - {1}", arg.Name, arg.Type) == domainSelected).FirstOrDefault();

            if (foundDomain == null)
                return;

            // Ask for project name
            var promptConfig = new PromptConfig()
            {
                InputType = InputType.Default,
                Title = "Enter the name of the project"
            };

            var newProjectName = await UserDialogs.Instance.PromptAsync(promptConfig);

            if (!newProjectName.Ok || string.IsNullOrWhiteSpace(newProjectName.Text)) { return; }

            // Ask for project description
            promptConfig = new PromptConfig()
            {
                InputType = InputType.Default,
                Title = "Enter a description of the project"
            };


            var newProjectDescription = await UserDialogs.Instance.PromptAsync(promptConfig);

            if (!newProjectDescription.Ok || string.IsNullOrWhiteSpace(newProjectDescription.Text)) { return; }

            ShowBusy("Adding Project...", MaskType.Gradient);

            var newProject = await App.AppTrainingApi.CreateProjectWithHttpMessagesAsync(newProjectName.Text, newProjectDescription.Text, foundDomain.Id);

            Projects.Add(newProject.Body);;

            HideBusy();
        }

        #endregion
    }
}   
