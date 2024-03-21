using PGPProject.Models;
using System.Collections.Generic;
using System.ComponentModel;
using Xamarin.Forms;

namespace PGPProject.ViewModels
{
    public class HomeViewModel : INotifyPropertyChanged
    {
        private List<string[]> keyNames;
        public List<string[]> KeyNames
        {
            get { return keyNames; }
            set
            {
                keyNames = value;
                OnPropertyChanged(nameof(KeyNames));
            }
        }

        public string title;
        public string Title
        {
            get { return title; }
            set
            {
                title = value;
                OnPropertyChanged(nameof(Title));
            }
        }

        private MyKey Key { get; set; }

        public HomeViewModel()
        {
            Key = new MyKey();

            // Initialize properties
            Title = "My Keys";
            UpdateKeys();

            // Subscribe to the message sent by CreateKeyPage
            MessagingCenter.Subscribe<MyKey>(this, "KeysUpdated", (sender) =>
            {
                // Update a list of "my keys"
                UpdateKeys();
            });
        }

        public void UpdateKeys()
        {
            KeyNames = Key.GetKeyNamesWithDates(true);
        }

        public void RemoveKeyPair(string Name)
        {
            Key.RemoveKey(Name, true);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}