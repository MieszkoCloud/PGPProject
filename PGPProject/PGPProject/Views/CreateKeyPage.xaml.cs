using PGPProject.Models;
using PGPProject.ViewModels;
using System;
using Xamarin.Forms;

namespace PGPProject.Views
{
	public partial class CreateKeyPage : ContentPage
	{
        private CreateKeyViewModel createKeyViewModel { get; set; }
        public CreateKeyPage()
		{
			InitializeComponent();

            // Create viewmodel object and set binding
            createKeyViewModel = new CreateKeyViewModel();
            BindingContext = createKeyViewModel;
        }

        private void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            // Validate provided key name 
            createKeyViewModel.ValidateNewKeyName(e.NewTextValue);
        }

        private async void OnSaveClicked(object sender, EventArgs e)
        {
            try 
            {
                // Call creating new key process
                createKeyViewModel.CreateNewKey(keyname.Text);
                // Move back to the Home page
                await Navigation.PopAsync();
            }
            catch (Exception ex) 
            {
                Console.WriteLine(ex.ToString());
                await DisplayAlert("Error", $"Could not create a key '{keyname.Text}'!", "OK");
            }
        }
    }
}