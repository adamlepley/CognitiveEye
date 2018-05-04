using System;
using System.Collections.Generic;
using CongnitiveEye.Forms.ViewModels;
using Xamarin.Forms;

namespace CongnitiveEye.Forms.Views
{
    public partial class ProjectImageView : BaseContentPage<ProjectImageViewModel>
    {
        public ProjectImageView()
        {
            InitializeComponent();
        }

        void TagPicker_Unfocused(object sender, Xamarin.Forms.FocusEventArgs e)
        {
            ViewModel.CheckIfIsDirty();
        }
    }
}
