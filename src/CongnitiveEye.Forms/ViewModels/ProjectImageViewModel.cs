using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using CognitiveEye.Forms;
using Microsoft.Cognitive.CustomVision.Training.Models;
using Xamarin.Forms;

namespace CongnitiveEye.Forms.ViewModels
{
    public class ProjectImageViewModel : BaseViewModel
    {
        public ProjectImageViewModel(Microsoft.Cognitive.CustomVision.Training.Models.Image selectedImage)
        {
            Title = "Computer Vision";
            SelectedImage = selectedImage;
        }

		public override void OnAppearing()
		{
			base.OnAppearing();

            LoadTags().ConfigureAwait(false);
		}

		public async Task LoadTags()
        {
            ShowBusy("Loading Tags...");

            var tags = await App.AppTrainingApi.GetTagsWithHttpMessagesAsync(App.SelectedProject.Id);

            Tags = new ObservableCollection<Tag>(tags.Body.Tags);

            if (selectedImage?.Tags != null && selectedImage.Tags.Count > 0)
            {
                SelectedTag = Tags.Where((args) => args.Id == SelectedImage.Tags[0].TagId).FirstOrDefault();
            }

            HideBusy();
        }

        public void CheckIfIsDirty()
        {
            IsDirty = (SelectedImage.Tags == null)
                    || (SelectedImage.Tags.Count == 0)
                    || SelectedImage.Tags[0].TagId != SelectedTag.Id;
        }

        #region Bindable Props

        Microsoft.Cognitive.CustomVision.Training.Models.Image selectedImage = null;
        public Microsoft.Cognitive.CustomVision.Training.Models.Image SelectedImage
        {
            get => selectedImage;
            set => SetProperty(ref selectedImage, value);
        }

        ObservableCollection<Tag> tags = null;
        public ObservableCollection<Tag> Tags
        {
            get => tags;
            set => SetProperty(ref tags, value);
        }

        Tag selectedTag = null;
        public Tag SelectedTag
        {
            get => selectedTag;
            set => SetProperty(ref selectedTag, value);
        }

        bool isDirty = false;
        public bool IsDirty
        {
            get => isDirty;
            set
            {
                SetProperty(ref isDirty, value);
                if (value)
                    ExecuteSaveTag().ConfigureAwait(false);
            }

        }

        #endregion

        #region Commands

        ICommand saveTag;
        public ICommand SaveTag =>
            saveTag ?? (saveTag = new Command(async () => await ExecuteSaveTag()));

        private async Task ExecuteSaveTag()
        {
            List<string> ImageIds = new List<string>();
            ImageIds.Add(SelectedImage.Id.ToString());

            List<string> TagIds = new List<string>();

            foreach (var t in selectedImage.Tags)
            {
                TagIds.Add(t.TagId.ToString());
            }

            if (TagIds.Count > 0)
            {
                await App.AppTrainingApi.DeleteImageTagsWithHttpMessagesAsync(App.SelectedProject.Id, ImageIds,TagIds);
            }

            var imageTags = new List<ImageTagCreateEntry>();
            imageTags.Add(new ImageTagCreateEntry(SelectedImage.Id, SelectedTag.Id));

            ShowBusy("Updating Tag...");

            await App.AppTrainingApi.PostImageTagsWithHttpMessagesAsync(App.SelectedProject.Id, new ImageTagCreateBatch(imageTags));

            HideBusy();

            IsDirty = false;
        }


        ICommand removeImage;
        public ICommand RemoveImage =>
            removeImage ?? (removeImage = new Command(async () => await ExecuteRemoveImage()));

        private async Task ExecuteRemoveImage()
        {
            List<string> tagIds = new List<string>();

            foreach (var t in SelectedImage.Tags)
            {
                tagIds.Add(t.TagId.ToString());
            }

            List<string> imageIds = new List<string>();
            imageIds.Add(SelectedImage.Id.ToString());

            ShowBusy("Removing Image...", Acr.UserDialogs.MaskType.Gradient);

            await App.AppTrainingApi.DeleteImageTagsWithHttpMessagesAsync(App.SelectedProject.Id, imageIds,tagIds);
            await NavService.PopAsync();

            HideBusy();
        }

        #endregion
    }
}
