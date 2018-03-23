using System;
using System.Collections.ObjectModel;
using Microsoft.Cognitive.CustomVision.Training.Models;

namespace CongnitiveEye.Forms.ViewModels
{
    public class ProjectsViewModel : BaseViewModel
    {
        public ProjectsViewModel()
        {
            Title = "Projects";
        }


        #region Bindable Props

        ObservableCollection<Project> projects = null;

        public ObservableCollection<Project> Projects
        {
            get => projects;
            set => SetProperty(ref projects, value);
        }

        #endregion
    }
}   
