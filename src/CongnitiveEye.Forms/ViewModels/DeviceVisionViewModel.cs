using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using Acr.UserDialogs;
using CognitiveEye.Forms;
using CongnitiveEye.Forms.Models;
using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Training.Models;
using Plugin.Media;
using Plugin.Media.Abstractions;
using Xamarin.Forms;

namespace CongnitiveEye.Forms.ViewModels
{
    public class DeviceVisionViewModel : BaseViewModel
    {

        public TimeSpan VisionClassifierInterval = new TimeSpan(0, 0, 0, 1, 0);
        
        public DeviceVisionViewModel() : base()
        {
            LoadModel().ConfigureAwait(false);
        }

        public void ConfigImageClassifier(bool activate)
        {
            ShowStart = !activate;
            ShowStop = activate;
            ImageClassifierRunning = activate;
        }


        async Task LoadModel()
        {
            ShowBusy("Downloading / Compiling the Model...");
            try
            {

                var iterationsResults = await App.AppTrainingApi.GetIterationsWithHttpMessagesAsync(App.SelectedProject.Id);

                Iterations = new ObservableCollection<Iteration>(iterationsResults.Body.Where((arg) => arg.Status == "Completed").OrderByDescending((arg) => arg.TrainedAt));

                SelectedIteration = Iterations.Where((arg) => arg.IsDefault == true).FirstOrDefault();

                ResultEntries = new ObservableCollection<TagPredicitionResult>();

                var tagsResults = await App.AppTrainingApi.GetTagsWithHttpMessagesAsync(App.SelectedProject.Id);

                Tags = new ObservableCollection<Tag>(tagsResults.Body);

                foreach (var tag in Tags.OrderBy((arg) => arg.Name))
                {
                    ResultEntries.Add(new TagPredicitionResult(tag.Name));
                }

                if (IsPhysicalDevice)
                    await ExecuteDownload();

                ShowStart = true;

            }
            catch (Microsoft.Rest.HttpOperationException ex)
            {
                var error = TrainingError.FromJson(ex.Response.Content);
                await Application.Current.MainPage.DisplayAlert("Error", error.Message, "Ok");
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", ex.Message, "Ok");
            }
            finally
            {
                HideBusy(); 
            }


        }

        public async Task TagPhoto(Stream imageStream)
        {
            var listOfTags = Tags.Select((arg) => arg.Name).ToArray<string>();

            var tagSelected = await Application.Current.MainPage.DisplayActionSheet(
                    "Select a Tag",
                    "Cancel",
                    null,
                    listOfTags);
            
            if (tagSelected == null)
                return;

            var foundTag = Tags.Where((arg) => arg.Name == tagSelected).FirstOrDefault();

            if (foundTag == null)
                return;

            var tagId = foundTag.Id;

            ShowBusy("Uploading Image...");

            List<string> TagIds = new List<string>();
            TagIds.Add(tagId.ToString());

            var result = await App.AppTrainingApi.CreateImagesFromDataWithHttpMessagesAsync(App.SelectedProject.Id, imageStream, TagIds);

            HideBusy();

            if (result.Response.IsSuccessStatusCode && result.Body?.Images != null && result.Body.Images.Count > 0)
            {
                ResultsMessage = "Image Tagged and Uploaded!";
                CanTagUpload = false;
            }

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

        ObservableCollection<Tag> tags = null;
        public ObservableCollection<Tag> Tags
        {
            get => tags;
            set => SetProperty(ref tags, value);
        }

        ObservableCollection<TagPredicitionResult> resultEntries = new ObservableCollection<TagPredicitionResult>();
        public ObservableCollection<TagPredicitionResult> ResultEntries
        {
            get => resultEntries;
            set => SetProperty(ref resultEntries, value);
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

        bool showStart = false;
        public bool ShowStart
        {
            get => showStart;
            set => SetProperty(ref showStart, value);
        }

        bool showStop = false;
        public bool ShowStop
        {
            get => showStop;
            set => SetProperty(ref showStop, value);
        }

        bool imageClassifierRunning = false;
        public bool ImageClassifierRunning
        {
            get => imageClassifierRunning;
            set => SetProperty(ref imageClassifierRunning, value);
        }

        bool imageClassifierStopped = true;
        public bool ImageClassifierStopped
        {
            get => imageClassifierStopped;
            set => SetProperty(ref imageClassifierStopped, value);
        }

        bool tagging = false;
        public bool Tagging
        {
            get => tagging;
            set => SetProperty(ref tagging, value);
        }

        bool showPhotoButton = false;
        public bool ShowPhotoButton
        {
            get => showPhotoButton;
            set => SetProperty(ref showPhotoButton, value);
        }

        string exportStatus = "";
        public string ExportStatus
        {
            get => exportStatus;
            set => SetProperty(ref exportStatus, value);
        }

        #endregion

        #region Commands

        ICommand download;
        public ICommand Download =>
            download ?? (download = new Command(async () => await ExecuteDownload()));

        private async Task ExecuteDownload(int count = 0)
        {
            if (count > 100)
            {
                HideBusy();
                return;
            }
            
            string exportPlatform = "CoreML";

            var models = await App.AppTrainingApi.GetExportsWithHttpMessagesAsync(App.SelectedProject.Id, selectedIteration.Id);

            var foundModel = models.Body.Where((arg) => arg.Platform == exportPlatform).FirstOrDefault();

            if (foundModel == null)
            {
                await App.AppTrainingApi.ExportIterationWithHttpMessagesAsync(App.SelectedProject.Id, selectedIteration.Id, exportPlatform);
                await ExecuteDownload();
                return;
            }

            if (foundModel.Status == "Done")
            {
                ShowBusy(string.Format("Compliling {0} Neural Net", exportPlatform));

                if (Views.DeviceVisionView.ImageClassifier == null)
                    Views.DeviceVisionView.ImageClassifier = DependencyService.Get<Services.IImageClassifier>();

                Views.DeviceVisionView.ImageClassifier.Init(foundModel.DownloadUri, Services.ModelType.General);
                HideBusy();
            }
            else
            {
                await Task.Delay(2000);
                await ExecuteDownload(count++);
            }
        }


        ICommand tagImage;
        public ICommand TagImage =>
            tagImage ?? (tagImage = new Command(async () => await ExecuteTagImage()));

        private async Task ExecuteTagImage()
        {
            

        }

        ICommand stopClassifier;
        public ICommand StopClassifier =>
            stopClassifier ?? (stopClassifier = new Command(async () => ExecuteStopClassifier()));

        private void ExecuteStopClassifier(int count = 0)
        {
            ConfigImageClassifier(false);
        }

        ICommand close;
        public ICommand Close =>
            close ?? (close = new Command(async () => await ExecuteClose()));

        private async Task ExecuteClose()
        {
            await Application.Current.MainPage.Navigation.PopModalAsync();
        }

        #endregion
    }

    public class TagPredicitionResult : INotifyPropertyChanged
    {
        public TagPredicitionResult(string tagName)
        {
            Name = tagName;
        }

        public void SetTagValue(double tagValue)
        {
            Value = tagValue;
            ValueText = Math.Round(tagValue * 100).ToString() + "%";

            if (tagValue > 0.3)
            {
                BackgroundColor = Utilities.ColorUtil.PositiveResultBackgroundColor;
                TextColor = Utilities.ColorUtil.PositiveResultTextColor;
                TextSize = 18d;
            }
            else
            {
                BackgroundColor = Utilities.ColorUtil.NegitiveResultBackgroundColor;
                TextColor = Utilities.ColorUtil.NegitiveResultTextColor;
                TextSize = 16d;
            }
        }

        string name = "";
        public string Name
        {
            get => name;
            set => SetProperty(ref name, value);
        }
        double value = 0;
        public double Value
        {
            get => value;
            set => SetProperty(ref value, value);
        }
        string valueText = "";
        public string ValueText
        {
            get => valueText;
            set => SetProperty(ref valueText, value);
        }
        Color backgroundColor = Color.Transparent;
        public Color BackgroundColor
        {
            get => backgroundColor;
            set => SetProperty(ref backgroundColor, value);
        }

        Color textColor = Color.Transparent;
        public Color TextColor
        {
            get => textColor;
            set => SetProperty(ref textColor, value);
        }

        double textSize = 12d;
        public double TextSize
        {
            get => textSize;
            set => SetProperty(ref textSize, value);
        }

        #region INotifyProperty Implementation

        protected virtual bool SetProperty<T>(
            ref T backingStore, T value,
            [CallerMemberName]string propertyName = "",
            Action onChanged = null,
            Func<T, T, bool> validateValue = null)
        {
            //if value didn't change
            if (EqualityComparer<T>.Default.Equals(backingStore, value))
                return false;

            //if value changed but didn't validate
            if (validateValue != null && !validateValue(backingStore, value))
                return false;

            backingStore = value;
            onChanged?.Invoke();
            OnPropertyChanged(propertyName);
            return true;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName]string propertyName = "") =>
         PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        #endregion
    }
}
