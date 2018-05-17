using System;
using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Training.Models;

using Xamarin.Forms;

namespace CongnitiveEye.Forms.Views
{
    public class ProjectTabbedView : TabbedPage
    {
        public ProjectTabbedView()
        {
            var tagNavPage = new NavigationPage(new ProjectTagsView());
            tagNavPage.Icon = "Tag.png";
            tagNavPage.Title = "Tags";

            Children.Add(tagNavPage);
        }
    }
}

