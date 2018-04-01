using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Microsoft.Cognitive.CustomVision.Training.Models;
using Xamarin.Forms;

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

        #region Commands

        ICommand openProject;
        public ICommand OpenProject =>
        openProject ?? (openProject = new Command((selectedItem) => ExecuteOpenProject(selectedItem)));

        private void ExecuteOpenProject(object selectedItem)
        {
            var selectedProject = (selectedItem as Project);

            if (selectedProject == null) { return; }


        }

        #endregion
    }
}   
