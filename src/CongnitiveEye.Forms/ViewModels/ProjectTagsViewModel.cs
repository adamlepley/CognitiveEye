using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Acr.UserDialogs;
using CognitiveEye.Forms;
using CongnitiveEye.Forms.Models;
using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Training.Models;
using Xamarin.Forms;

namespace CongnitiveEye.Forms.ViewModels
{
    public class ProjectTagsViewModel : BaseViewModel
    {
        public ProjectTagsViewModel()
        {
            Title = "Tags";
            LoadIterations().ConfigureAwait(false);
        }

		public override void OnAppearing()
		{
			base.OnAppearing();
		}

        public async Task LoadIterations()
        {
            ShowBusy("Loading Iterations...");

            var iterations = await App.AppTrainingApi.GetIterationsWithHttpMessagesAsync(App.SelectedProject.Id);

            HideBusy();

            Iterations = new ObservableCollection<Iteration>(iterations.Body.Where((arg) => arg.Status == "Completed").OrderByDescending((arg) => arg.TrainedAt));

            ShowInterations = (Iterations.Count > 0);

            if (ShowInterations)
                SelectedIteration = Iterations.Where((arg) => arg.IsDefault == true).FirstOrDefault();
            else
                await LoadTags();

        }

		public async Task LoadTags()
        {

            ShowBusy("Loading Tags...");

            Microsoft.Rest.HttpOperationResponse<System.Collections.Generic.IList<Tag>> loadedTags;

            var projectTags = await App.AppTrainingApi.GetTagsWithHttpMessagesAsync(App.SelectedProject.Id);

            Tags = new ObservableCollection<TagTile>();

            foreach (var tag in projectTags.Body)
            {
                tags.Add(new TagTile(tag));
            };

            if (SelectedIteration != null)
            {
                var iterationsTags = await App.AppTrainingApi.GetTagsWithHttpMessagesAsync(App.SelectedProject.Id, SelectedIteration.Id);

                foreach (var tag in Tags)
                {
                    tag.IsNew = !iterationsTags.Body.Any((args) => args.Id == tag.Id);
                };
            }

            HideBusy();
        }

        #region Bindable Props

        ObservableCollection<TagTile> tags = null;
        public ObservableCollection<TagTile> Tags
        {
            get => tags;
            set => SetProperty(ref tags, value);
        }

        ObservableCollection<Iteration> iterations = null;
        public ObservableCollection<Iteration> Iterations
        {
            get => iterations;
            set => SetProperty(ref iterations, value);
        }

        Iteration selectedIteration = null;
        public Iteration SelectedIteration
        {
            get => selectedIteration;
            set {
                // Check if changed
                if (selectedIteration != null && value != null)
                    LoadTags().ConfigureAwait(false);
                
                SetProperty(ref selectedIteration, value);
            }
        }

        bool showIterations = false;
        public bool ShowInterations
        {
            get => showIterations;
            set => SetProperty(ref showIterations, value);
        }

        #endregion

        #region Commands

        ICommand openTag;
        public ICommand OpenTag =>
            openTag ?? (openTag = new Command(async (selectedItem) => await ExecuteOpenTag(selectedItem)));

        private async Task ExecuteOpenTag(object selectedItem)
        {
            var selectedTag = (selectedItem as TagTile);

            if (selectedTag == null) { return; }

            await NavService.PushAsync<ProjectPhotosViewModel>(new ProjectPhotosViewModel(selectedTag));

        }

        ICommand addTag;
        public ICommand AddTag =>
            addTag ?? (addTag = new Command(async () => await ExecuteAddTag()));

        private async Task ExecuteAddTag()
        {

            var promptConfig = new PromptConfig()
            {
                InputType = InputType.Default,
                Title = "Enter the name of the tag"
            };

            var newTag = await UserDialogs.Instance.PromptAsync(promptConfig);

            if (!newTag.Ok || string.IsNullOrWhiteSpace(newTag.Text)) { return; }

            ShowBusy("Adding Tag...");

            await App.AppTrainingApi.CreateTagWithHttpMessagesAsync(App.SelectedProject.Id, newTag.Text);

            await LoadTags();

            HideBusy();

        }

        ICommand reTrain;
        public ICommand ReTrain =>
            reTrain ?? (reTrain = new Command(async () => await ExecuteReTrain()));

        private async Task ExecuteReTrain()
        {
            Microsoft.Rest.HttpOperationResponse<Iteration> createResult = null;

            ShowBusy("Training...");

            try
            {
                createResult = await App.AppTrainingApi.TrainProjectWithHttpMessagesAsync(App.SelectedProject.Id);
            }
            catch (Microsoft.Rest.HttpOperationException ex)
            {
                HideBusy();

                var error = TrainingError.FromJson(ex.Response.Content);
                await Application.Current.MainPage.DisplayAlert("Training Error", error.Message, "Ok");
                return;
            }

            var newIteration = createResult.Body;

            while (newIteration.Status == "Training")
            {
                await Task.Delay(5000);
                var getResult = await App.AppTrainingApi.GetIterationWithHttpMessagesAsync(App.SelectedProject.Id, newIteration.Id);

                newIteration = getResult.Body;
            }

            Iterations.Add(createResult.Body);
            SelectedIteration = createResult.Body;

            SelectedIteration.IsDefault = true;

            var updatedIteration = await App.AppTrainingApi.UpdateIterationWithHttpMessagesAsync(App.SelectedProject.Id, newIteration.Id, SelectedIteration);

            await Application.Current.MainPage.DisplayAlert("Training Complete", "New Model Trained!", "Ok");

            HideBusy();

        }

        #endregion
    }
}

