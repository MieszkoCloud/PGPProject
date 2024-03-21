using PGPProject.Models;
using System.Collections.Generic;
using System.ComponentModel;
using Xamarin.Forms;

namespace PGPProject.ViewModels
{
    public class ContactsViewModel : INotifyPropertyChanged
    {
        private List<string[]> contactNames;
        public List<string[]> ContactNames
        {
            get { return contactNames; }
            set
            {
                contactNames = value;
                OnPropertyChanged(nameof(ContactNames));
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

        public ContactsViewModel()
        {
            Key = new MyKey();

            // Initialize properties
            Title = "My Contacts";
            UpdateContacts();

            // Subscribe to the message sent by ImportContactPage
            MessagingCenter.Subscribe<MyKey>(this, "KeysUpdated", (sender) =>
            {
                // Update a list of contacts
                UpdateContacts();
            });
        }

        public void UpdateContacts()
        {
            ContactNames = Key.GetKeyNamesWithDates(false);
        }

        public void RemoveContact(string Name)
        {
            Key.RemoveKey(Name, false);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}