using PGPProject.ViewModels;
using System;
using Xamarin.Forms;

namespace PGPProject.Views
{
    public partial class ContactsPage : ContentPage
    {
        public ContactsViewModel contactsViewModel { get; set; }

        public ContactsPage()
        {
            InitializeComponent();

            // Create viewmodel object and set binding
            contactsViewModel = new ContactsViewModel();
            BindingContext = contactsViewModel;
        }

        private async void DeleteContactButton_Clicked(object sender, EventArgs e)
        {
            string keyName = (string)((Button)sender).CommandParameter;
            try
            {
                // Remove contact basing on text in selected row
                contactsViewModel.RemoveContact(keyName);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                await DisplayAlert("Error", $"Unable to remove contact '{keyName}'!", "OK");
            }
        }

        private async void AddContactButton_Clicked(object sender, EventArgs e)
        {
            try
            {
                await Navigation.PushAsync(new ImportContactPage());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.ToString()}");
            }
        }
    }
}