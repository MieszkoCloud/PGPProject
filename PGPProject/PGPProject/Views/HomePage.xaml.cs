using PGPProject.ViewModels;
using System;
using Xamarin.Forms;

namespace PGPProject.Views
{
    public partial class HomePage : ContentPage
    {
        public HomeViewModel homeViewModel { get; set; }
        public HomePage()
        {
            InitializeComponent();

            // Create viewmodel object and set binding
            homeViewModel = new HomeViewModel();
            BindingContext = homeViewModel;
        }

        private async void DeleteButton_Clicked(object sender, EventArgs e)
        {
            string keyName = (string)((Button)sender).CommandParameter;
            try
            {
                // Remove key basing on text in selected row
                homeViewModel.RemoveKeyPair(keyName);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                await DisplayAlert("Error", $"Unable to remove key pair '{keyName}'!", "OK");
            }
        }

        private async void AddButton_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new CreateKeyPage()); 
        }
    }
}