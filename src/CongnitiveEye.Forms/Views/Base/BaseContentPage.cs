using System;
using CodeMill.VMFirstNav;
using Xamarin.Forms;

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
            }
        }

        public BaseContentPage()
        {
            this.SetBinding(Page.TitleProperty, "Title");
        }

		protected override void OnAppearing()
		{
			base.OnAppearing();

            ViewModel.OnAppearing();
		}

	}
}

