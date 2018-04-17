using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using CognitiveEye.Forms;
using Microsoft.Cognitive.CustomVision.Training.Models;
using Xamarin.Forms;

namespace CongnitiveEye.Forms.ViewModels
{
    public class ProjectPhotosViewModel : BaseViewModel
    {
        public ProjectPhotosViewModel(Tag selectedTag)
        {
            SelectedTag = selectedTag;

            Title = selectedTag.Name;

            LoadPhotos().ConfigureAwait(false);
        }

        public async Task LoadPhotos()
        {
            var photos = await App.AppTrainingApi.GetTaggedImagesWithHttpMessagesAsync(App.SelectedProject.Id, null, new List<string>() { SelectedTag.Id.ToString() });

            Images = new ObservableCollection<Microsoft.Cognitive.CustomVision.Training.Models.Image>(photos.Body);
        }

        #region Bindable Props

        Tag selectedTag = null;
        public Tag SelectedTag
        {
            get => selectedTag;
            set => SetProperty(ref selectedTag, value);
        }

        ObservableCollection<Microsoft.Cognitive.CustomVision.Training.Models.Image> images = null;
        public ObservableCollection<Microsoft.Cognitive.CustomVision.Training.Models.Image> Images
        {
            get => images;
            set => SetProperty(ref images, value);
        }

        #endregion

        #region Commands

        ICommand openImage;
        public ICommand OpenImage =>
            openImage ?? (openImage = new Command(async (selectedItem) => await ExecuteOpenTag(selectedItem)));

        private async Task ExecuteOpenTag(object selectedItem)
        {
            var selectedImage = (selectedItem as Microsoft.Cognitive.CustomVision.Training.Models.Image);

            if (selectedImage == null) { return; }

            await NavService.PushAsync<ProjectImageViewModel>(new ProjectImageViewModel(selectedImage));

        }

        #endregion


    }
}

