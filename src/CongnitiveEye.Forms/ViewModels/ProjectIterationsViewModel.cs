using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Acr.UserDialogs;
using CognitiveEye.Forms;
using Microsoft.Cognitive.CustomVision.Training.Models;
using Plugin.Media;
using Plugin.Media.Abstractions;
using Xamarin.Forms;

namespace CongnitiveEye.Forms.ViewModels
{
    public class ProjectIterationsViewModel : BaseViewModel
    {
        private MediaFile lastPhoto;
        private ImageTagPrediction lastPrediction;


        public ProjectIterationsViewModel()
        {
            LoadIterations().ConfigureAwait(false);
        }


		async Task LoadIterations()
        {
            ShowBusy("Loading Iterations...");

            var iterations = await App.AppTrainingApi.GetIterationsWithHttpMessagesAsync(App.SelectedProject.Id);

            if (iterations.Body == null || iterations.Body.Count == 0) { return; }

            Iterations = new ObservableCollection<Iteration>(iterations.Body.Where((arg) => arg.Status == "Completed").OrderByDescending((arg) => arg.TrainedAt));

            SelectedIteration = Iterations.Where((arg) => arg.IsDefault == true).FirstOrDefault();

            ShowPhotoButton = (SelectedIteration != null);

            if (!ShowPhotoButton)
            {
                ResultsMessage = "You must train your first model";
            }

            HideBusy();
        }

        #region Bindable Props

        ImageSource selectedImage = null;
        public ImageSource SelectedImage
        {
            get => selectedImage;
            set => SetProperty(ref selectedImage, value);
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
            set => SetProperty(ref selectedIteration, value);
        }

        string resultsMessage = "";
        public string ResultsMessage
        {
            get => resultsMessage;
            set => SetProperty(ref resultsMessage, value);
        }

        bool isDirty = false;
        public bool IsDirty
        {
            get => isDirty;
            set => SetProperty(ref isDirty, value);
        }

        bool canTagUpload = false;
        public bool CanTagUpload
        {
            get => canTagUpload;
            set => SetProperty(ref canTagUpload, value);
        }

        bool showPic = false;
        public bool ShowPic
        {
            get => showPic;
            set => SetProperty(ref showPic, value);
        }

        bool showPhotoButton = false;
        public bool ShowPhotoButton
        {
            get => showPhotoButton;
            set => SetProperty(ref showPhotoButton, value);
        }

        #endregion

        #region Commands

        ICommand testModel;
        public ICommand TestModel =>
            testModel ?? (testModel = new Command(async () => await ExecuteTestModel()));

        private async Task ExecuteTestModel()
        {
            CanTagUpload = false;

            ResultsMessage = "";

            await CrossMedia.Current.Initialize();

            if (!CrossMedia.Current.IsCameraAvailable || !CrossMedia.Current.IsTakePhotoSupported)
            {
                await Application.Current.MainPage.DisplayAlert("No Camera", ":( No camera available.", "OK");
                return;
            }

            lastPhoto = await CrossMedia.Current.TakePhotoAsync(new Plugin.Media.Abstractions.StoreCameraMediaOptions
            {
                PhotoSize = Plugin.Media.Abstractions.PhotoSize.Medium,
                SaveToAlbum = false,
                Directory = "Sample",
                Name = "test.jpg"
            });

            if (lastPhoto == null)
                return;

            ShowPic = true;

            SelectedImage = ImageSource.FromStream(() =>
            {
                var currentPhotoStream = lastPhoto.GetStream();
                return currentPhotoStream;
            });

            Microsoft.Rest.HttpOperationResponse<ImagePredictionResult> result;

            ShowBusy("Predicting...", Acr.UserDialogs.MaskType.Gradient);

            try
            {
                result = await App.AppTrainingApi.QuickTestImageWithHttpMessagesAsync(App.SelectedProject.Id, lastPhoto.GetStream(), SelectedIteration.Id);
            }
            catch (Microsoft.Rest.HttpOperationException ex)
            {
                ResultsMessage = ex.Response.Content;
                HideBusy();
                return;
            }

            HideBusy();

            if (result.Response.IsSuccessStatusCode && result.Body?.Predictions != null && result.Body.Predictions.Count > 0)
            {

                lastPrediction = result.Body.Predictions
                                       .Where((arg) => arg.Probability > .20)
                                       .OrderByDescending((arg) => arg.Probability)
                                       .FirstOrDefault();

                if (lastPrediction == null)
                {
                    ResultsMessage = "I have no idea what this is :(\nTag and make me smarter :)";
                    CanTagUpload = true;
                }
                else
                {
                    ResultsMessage = string.Format("I am {0}% confident this is a {1}",
                                                   Math.Round(lastPrediction.Probability * 100).ToString(),
                                              result.Body.Predictions[0].Tag);
                    if (lastPrediction.Probability < .90)
                    {
                        CanTagUpload = true;
                    }
                }

            }
            else
            {
                ResultsMessage = "Somthing went wront :(";
            }

        }

        ICommand reTrain;
        public ICommand ReTrain =>
            reTrain ?? (reTrain = new Command(async () => await ExecuteReTrain()));

        private async Task ExecuteReTrain()
        {
            CanTagUpload = false;

            Microsoft.Rest.HttpOperationResponse<Iteration> createResult = null;

            ResultsMessage = "";
            SelectedImage = null;
            ShowBusy("Training...", Acr.UserDialogs.MaskType.Gradient);

            try
            {
                createResult = await App.AppTrainingApi.TrainProjectWithHttpMessagesAsync(App.SelectedProject.Id);
            }
            catch (Microsoft.Rest.HttpOperationException ex)
            {
                ResultsMessage = ex.Response.Content;
                HideBusy();
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

            ResultsMessage = "New Model Trained!";

            HideBusy();

        }

        ICommand tagImage;
        public ICommand TagImage =>
            tagImage ?? (tagImage = new Command(async () => await ExecuteTagImage()));

        private async Task ExecuteTagImage()
        {

            ShowBusy("Loading Tags...");

            var tags = await App.AppTrainingApi.GetTagsWithHttpMessagesAsync(App.SelectedProject.Id);

            HideBusy();

            if (tags?.Body == null)
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Failed to load tags", "OK");
                return;
            }

            string[] listOfTags = null;

            if (lastPrediction == null)
            {
                listOfTags = tags.Body.Tags.Select((arg) => arg.Name).ToArray<string>();
            }
            else
            {
                var confirmConfig = new ConfirmConfig()
                {
                    Title = "Confirm",
                    OkText = "YES",
                    CancelText = "NO",
                    Message = "Was the prediction correct?"
                };

                var wasCorrect = await UserDialogs.Instance.ConfirmAsync(confirmConfig);

                if (!wasCorrect)
                {
                    listOfTags = tags.Body.Tags
                                     .Where((arg) => arg.Id != lastPrediction.TagId)
                                     .Select((arg) => arg.Name).ToArray<string>();
                }
            }

            Guid tagId;

            if (listOfTags != null)
            {

                var tagSelected = await Application.Current.MainPage.DisplayActionSheet(
                    "What type of project would you like to create ? ",
                    "Cancel",
                    null,
                    listOfTags);

                if (tagSelected == null)
                    return;

                var foundTag = tags.Body.Tags.Where((arg) => arg.Name == tagSelected).FirstOrDefault();

                if (foundTag == null)
                    return;

                tagId = foundTag.Id;
            }
            else
            {
                tagId = lastPrediction.TagId;
            }

            ShowBusy("Uploading Image...");


            List<string> TagIds = new List<string>();
            TagIds.Add(tagId.ToString());

            var result = await App.AppTrainingApi.CreateImagesFromDataWithHttpMessagesAsync(App.SelectedProject.Id, lastPhoto.GetStream(), TagIds);

            HideBusy();

            if (result.Response.IsSuccessStatusCode && result.Body?.Images != null && result.Body.Images.Count > 0)
            {
                ResultsMessage = "Image Tagged and Uploaded!";
                CanTagUpload = false;
            }

        }

        #endregion
    }
}
