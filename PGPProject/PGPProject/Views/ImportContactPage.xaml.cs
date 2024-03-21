using PGPProject.Models;
using PGPProject.ViewModels;
using System;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace PGPProject.Views
{
    public partial class ImportContactPage : ContentPage
    {
        private ImportContactViewModel importContactViewModel;

        public ImportContactPage()
        {
            InitializeComponent();

            // Create viewmodel object and set binding
            importContactViewModel = new ImportContactViewModel();
            BindingContext = importContactViewModel;

            SetContactKeyPath();
        }
        
        private void SetContactKeyPath(string Path = null)
        {
            if (Path == null)
                ContactKeyFileNameLabel.Text = "No public key selected";
            else
            {
                string[] PathParts = Path.Split('/');
                string Name = PathParts[PathParts.Length - 1];
                ContactKeyFileNameLabel.Text = Name;
            }

            importContactViewModel.SetContactFilePath(Path);       
            
        }

        private void OnContactKeyNameTextChanged(object sender, TextChangedEventArgs e)
        {
            // Validate provided key name and file
            importContactViewModel.ValidateName(e.NewTextValue);
        }

        private async void OnContactKeyFileNameClick(object sender, EventArgs e)
        {
            try
            {
                // Open the file picker
                FileResult file = await FilePicker.PickAsync(new PickOptions
                {
                    PickerTitle = "Select a file",
                });

                if (file != null)
                {
                    SetContactKeyPath(file.FullPath);
                }
            }
            catch (Exception ex)
            {
                // Handle any errors
                await DisplayAlert("Error", $"Error selecting file: {ex.Message}", "OK");
            }
        }

        private async void OnContactKeySaveClicked(object sender, EventArgs e)
        {
            try
            {
                // Call saving contact key process
                importContactViewModel.SaveContact(ContactName.Text);
                await DisplayAlert("Success", $"Contact named '{ContactName.Text}' has been added successfuly.", "OK");
                await Navigation.PopAsync();
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
                await DisplayAlert("Error", $"Could not create a contact named '{ContactName.Text}'!", "OK");
            }
        }
    }
}