using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Training.Models;
using Xamarin.Forms;

namespace CongnitiveEye.Forms.Models
{
    public class TagTile : INotifyPropertyChanged
    {
        public Guid Id { get; private set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public int ImageCount { get; private set; }

        bool isNew;
        public bool IsNew
        {
            get => isNew;
            set => SetProperty(ref isNew, value);
        }

        Color backgroundColor;
        public Color BackgroundColor
        {
            get => backgroundColor;
            set => SetProperty(ref backgroundColor, value);
        }

        public TagTile(Tag tag)
        {
            Id = tag.Id;
            Name = tag.Name;
            Description = tag.Description;
            ImageCount = tag.ImageCount;
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
