using System;
using CodeMill.VMFirstNav;
using Xamarin.Forms;
using CongnitiveEye.Forms.ControlTemplates;
using Xamarin.Forms.Xaml;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace CongnitiveEye.Forms.Views
{
    public class BaseContentPage<T> : ContentPage, IViewFor<T> where T : ViewModels.BaseViewModel
    {
        T vm;

        public T ViewModel
        {
            get => vm;
            set
            {
                vm = value;
                BindingContext = vm;

                if (vm.UsePageTemplate)
                    this.ControlTemplate = new ControlTemplate(typeof(ContentPageTemplate));
                    
            }
        }

        public BaseContentPage()
        {
            //this.SetBinding(Page.TitleProperty, "Title");
            this.ControlTemplate = new ControlTemplate(typeof(ContentPageTemplate));
        }

		protected override void OnAppearing()
		{
			base.OnAppearing();

            ViewModel.OnAppearing();
		}

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            ViewModel.OnDisappearing();
        }

	}
}

