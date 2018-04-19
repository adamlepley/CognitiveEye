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
    public class ProjectTagsViewModel : BaseViewModel
    {
        public ProjectTagsViewModel()
        {
            Title = "Tags";

            LoadTags().ConfigureAwait(false);
        }

        public async Task LoadTags()
        {
            var tags = await App.AppTrainingApi.GetTagsWithHttpMessagesAsync(App.SelectedProject.Id);

            Tags = new ObservableCollection<Tag>(tags.Body.Tags);
        }

        #region Bindable Props

        ObservableCollection<Tag> tags = null;
        public ObservableCollection<Tag> Tags
        {
            get => tags;
            set => SetProperty(ref tags, value);
        }

        #endregion

        #region Commands

        ICommand openTag;
        public ICommand OpenTag =>
            openTag ?? (openTag = new Command(async (selectedItem) => await ExecuteOpenTag(selectedItem)));

        private async Task ExecuteOpenTag(object selectedItem)
        {
            var selectedTag = (selectedItem as Tag);

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

            await App.AppTrainingApi.CreateTagWithHttpMessagesAsync(App.SelectedProject.Id, newTag.Text);

            await LoadTags();

        }

        #endregion
    }
}

