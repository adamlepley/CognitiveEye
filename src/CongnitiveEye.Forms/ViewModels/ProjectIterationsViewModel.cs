using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using CognitiveEye.Forms;
using Microsoft.Cognitive.CustomVision.Training.Models;
using Plugin.Media;
using Xamarin.Forms;

namespace CongnitiveEye.Forms.ViewModels
{
    public class ProjectIterationsViewModel : BaseViewModel
    {
        public ProjectIterationsViewModel()
        {
            LoadIterations().ConfigureAwait(false);
        }

		public override void OnAppearing()
		{
			base.OnAppearing();

		}

		async Task LoadIterations()
        {
            var iterations = await App.AppTrainingApi.GetIterationsWithHttpMessagesAsync(App.SelectedProject.Id);

            if (iterations.Body == null || iterations.Body.Count == 0) { return; }

            Iterations = new ObservableCollection<Iteration>(iterations.Body.Where((arg) => arg.Status == "Completed"));

            SelectedIteration = Iterations.Where((arg) => arg.IsDefault == true).FirstOrDefault();
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

        #endregion

        #region Commands

        ICommand testModel;
        public ICommand TestModel =>
            testModel ?? (testModel = new Command(async () => await ExecuteTestModel()));

        private async Task ExecuteTestModel()
        {
            IsBusy = true;
            ResultsMessage = "";

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

            SelectedImage = ImageSource.FromStream(() =>
            {
                var stream = file.GetStream();
                return stream;
            });

            Microsoft.Rest.HttpOperationResponse<ImagePredictionResult> result;

            try
            {
                result = await App.AppTrainingApi.QuickTestImageWithHttpMessagesAsync(App.SelectedProject.Id, file.GetStream(), SelectedIteration.Id);
            }
            catch (Microsoft.Rest.HttpOperationException ex)
            {
                ResultsMessage = ex.Response.Content;
                IsBusy = false;
                return;
            }

            if (result.Response.IsSuccessStatusCode && result.Body?.Predictions != null && result.Body.Predictions.Count > 0)
            {

                var positivePrediction = result.Body.Predictions
                                               .OrderBy((arg) => arg.Probability)
                                               .Where((arg) => arg.Probability > .55).OrderBy((arg) => arg.Probability)
                                               .FirstOrDefault();

                if (positivePrediction == null)
                {
                    ResultsMessage = "I have no idea what this is :(";
                }
                else
                {
                    ResultsMessage = string.Format("I am {0}% confident this is a {1}",
                                              Math.Round(result.Body.Predictions[0].Probability * 100).ToString(),
                                              result.Body.Predictions[0].Tag);
                }

            }
            else
            {
                ResultsMessage = "Somthing went wront :(";
            }

            IsBusy = false;
        }

        ICommand reTrain;
        public ICommand ReTrain =>
            reTrain ?? (reTrain = new Command(async () => await ExecuteReTrain()));

        private async Task ExecuteReTrain()
        {
            Microsoft.Rest.HttpOperationResponse<Iteration> createResult = null;

            ResultsMessage = "";
            SelectedImage = null;
            IsBusy = true;
            ResultsMessage = "Training...";

            try
            {
                createResult = await App.AppTrainingApi.TrainProjectWithHttpMessagesAsync(App.SelectedProject.Id);
            }
            catch (Microsoft.Rest.HttpOperationException ex)
            {
                ResultsMessage = ex.Response.Content;
                IsBusy = false;
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

            IsBusy = false;

        }

        #endregion
    }
}
