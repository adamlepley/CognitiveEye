using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using CodeMill.VMFirstNav;
using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Training.Models;
using Acr.UserDialogs;

namespace CongnitiveEye.Forms.ViewModels
{
    /// <summary>
    /// Totally ripped off from James Montemago's MVVM helps library (https://github.com/jamesmontemagno/mvvm-helpers/blob/master/MvvmHelpers/ObservableObject.cs) - Didn't need the whole library
    /// </summary>

    public class BaseViewModel : INotifyPropertyChanged, IViewModel
    {

        public BaseViewModel()
        {
            NavService = NavigationService.Instance;
            HideBusy();
        }

        public void ShowBusy(string title = null, MaskType maskType = MaskType.None)
        {
            Xamarin.Forms.Device.BeginInvokeOnMainThread(() =>
            {
                IsBusy = true;
                UserDialogs.Instance.ShowLoading(title, maskType);
            });
        }

        public void HideBusy()
        {
            Xamarin.Forms.Device.BeginInvokeOnMainThread(() =>
            {
                IsBusy = false;
                UserDialogs.Instance.HideLoading();
            });
        }

        #region Common Props

        readonly protected INavigationService NavService;

        #endregion

        #region Abstact Methods

        public virtual void OnAppearing() { }

        public virtual void OnDisappearing() { }

        #endregion

        #region Common BindableProps

        string title;
        public string Title
        {
            get => title;
            set => SetProperty(ref title, value); 
        }

        bool isBusy;
        public bool IsBusy
        {
            get => isBusy;
            private set => SetProperty(ref isBusy, value);
        }

        bool usePageTemplate = true;
        public bool UsePageTemplate
        {
            get => usePageTemplate;
            set => SetProperty(ref usePageTemplate, value);
        }

        #endregion

        #region INotifyProperty Implementation

        /// <summary>
        /// Totally ripped off from James Montemago's MVVM helps library (https://github.com/jamesmontemagno/mvvm-helpers/blob/master/MvvmHelpers/ObservableObject.cs) - Didn't need the whole library
        /// </summary>

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
