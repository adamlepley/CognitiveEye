﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using CognitiveEye.Forms;
using Microsoft.Cognitive.CustomVision.Training.Models;
using Plugin.Media;
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

        public override void OnAppearing()
        {
            base.OnAppearing();

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

        ICommand addPic;
        public ICommand AddPic =>
            addPic ?? (addPic = new Command(async () => await ExecuteAddTag()));

        private async Task ExecuteAddTag()
        {
            IsBusy = true;

            await CrossMedia.Current.Initialize();
    
            if (!CrossMedia.Current.IsCameraAvailable || !CrossMedia.Current.IsTakePhotoSupported)
            {
                await Application.Current.MainPage.DisplayAlert("No Camera", ":( No camera available.", "OK");
                return;
            }

            var file = await CrossMedia.Current.TakePhotoAsync(new Plugin.Media.Abstractions.StoreCameraMediaOptions
            {
                PhotoSize = Plugin.Media.Abstractions.PhotoSize.Medium,
                SaveToAlbum = false,
                Directory = "Sample",
                Name = "test.jpg"
            });

            if (file == null)
                return;

            List<string> TagIds = new List<string>();
            TagIds.Add(selectedTag.Id.ToString());

            var result = await App.AppTrainingApi.CreateImagesFromDataWithHttpMessagesAsync(App.SelectedProject.Id, file.GetStream(), TagIds);

            if (result.Response.IsSuccessStatusCode && result.Body?.Images != null && result.Body.Images.Count > 0)
                Images.Add(result.Body.Images[0].Image);

            IsBusy = false;
        }

        #endregion


    }
}

