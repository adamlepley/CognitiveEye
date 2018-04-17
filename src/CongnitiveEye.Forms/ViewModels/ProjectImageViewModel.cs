using System;
namespace CongnitiveEye.Forms.ViewModels
{
    public class ProjectImageViewModel : BaseViewModel
    {
        public ProjectImageViewModel(Microsoft.Cognitive.CustomVision.Training.Models.Image selectedImage)
        {
            Title = "Computer Vision";
            SelectedImage = selectedImage;
        }

        #region Bindable Props

        Microsoft.Cognitive.CustomVision.Training.Models.Image selectedImage = null;
        public Microsoft.Cognitive.CustomVision.Training.Models.Image SelectedImage
        {
            get => selectedImage;
            set => SetProperty(ref selectedImage, value);
        }

        #endregion
    }
}
