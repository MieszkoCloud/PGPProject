using System;
using System.ComponentModel;

namespace PGPProject.ViewModels
{
    public class InfoViewModel : INotifyPropertyChanged
    {
        private string Platform;
        private string Smth1;
        private string Desc;
        public string Title { get; set; }
        public InfoViewModel()
        {
            Title = "App info";
            Platform = "Android";
            Smth1 = "Xamarin.Forms";
            Desc = "some desc";
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}