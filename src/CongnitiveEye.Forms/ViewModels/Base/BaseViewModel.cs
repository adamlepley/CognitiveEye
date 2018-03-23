using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using CodeMill.VMFirstNav;

namespace CongnitiveEye.Forms.ViewModels
{
    /// <summary>
    /// Base view model.
    /// Totally ripped off from James Montemago's MVVM helps library (https://github.com/jamesmontemagno/mvvm-helpers/blob/master/MvvmHelpers/ObservableObject.cs) - Didn't need the whole library
    /// </summary>

    public class BaseViewModel : INotifyPropertyChanged, IViewModel
    {
        
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



    }
}
