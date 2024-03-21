using PGPProject.Models;
using PGPProject.ViewModels;
using System;
using System.Collections.Generic;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace PGPProject.Views
{
    public partial class ProceedPage : ContentPage
    {
        private ProceedViewModel proceedViewModel { get; set; }
        public ProceedPage()
        {
            InitializeComponent();

            // Create viewmodel object and set binding
            proceedViewModel = new ProceedViewModel();
            BindingContext = proceedViewModel;

            // Set view properties
            ToggleMethodTypeButtons("Encrypt");
            SetFilePath();
            SetSymmetricKeyPath();

            RecipientDropdownList.SelectedIndex = 0;
            MyKeyDropdownList.SelectedIndex = 0;

            // Subscribe to the message sent by CreateKeyPage
            MessagingCenter.Subscribe<MyKey>(this, "KeysUpdated", (sender) =>
            {
                proceedViewModel.GetKeyNames();
                RecipientDropdownList.SelectedIndex = 0;
                MyKeyDropdownList.SelectedIndex = 0;
            });
        }

        public async void OnPasteClick(object sender, EventArgs e)
        {
            // Check if the clipboard has text
            if (!Clipboard.HasText)
                // Put text into the clipboard if empty
                await Clipboard.SetTextAsync("Clipboard was empty, so Hello World!");

            // Get the text from the clipboard and set it in the Editor control
            Textarea.Text = await Clipboard.GetTextAsync();
        }

        public async void OnSelectInputSymmetricKeyClick(object sender, EventArgs e)
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
                    SetSymmetricKeyPath(file.FullPath);
                }
            }
            catch (Exception ex)
            {
                // Handle any errors
                await DisplayAlert("Error", $"Error selecting file: {ex.Message}", "OK");
            }
        }

        private void SetSymmetricKeyPath(string Path = null)
        {
            // Set default value for a label with selected file name
            if (Path == null)
                SymmKeyFilenameLabel.Text = "No symmetric key selected";
            else
            {
                string[] PathParts = Path.Split('/');
                string Name = PathParts[PathParts.Length - 1];
                SymmKeyFilenameLabel.Text = Name;
            }

            // Set FilePath value in viewmodel
            proceedViewModel.SetInputSymmetricKeyPath(Path);
        }

        public async void OnSelectInputFileClick(object sender, EventArgs e)
        {
            try
            {
                // Open the file picker
                FileResult file = await FilePicker.PickAsync(new PickOptions
                {
                    PickerTitle = "Select a file"
                });

                if (file != null)
                {
                    SetFilePath(file.FullPath);
                }
            }
            catch (Exception ex)
            {
                // Handle any errors
                await DisplayAlert("Error", $"Error selecting file: {ex.Message}", "OK");
            }
        }

        private void SetFilePath(string Path = null)
        {
            // Set default value for a label with selected file name
            if (Path == null)
                FilenameLabel.Text = "No file selected";
            else
            {
                string[] PathParts = Path.Split('/');
                string Name = PathParts[PathParts.Length - 1];
                FilenameLabel.Text = Name;
            }

            // Set FilePath value in viewmodel
            proceedViewModel.SetInputFilePath(Path);
        }

        public void OnClearFilenameClick(object sender, EventArgs e)
        {
            // Set filename to null (null is default in SetFilePath method)
            SetFilePath();
        }

        public void OnMethodChange(object sender, EventArgs e)
        {
            ToggleMethodTypeButtons(((Button)sender).Text);
        }

        private void ToggleMethodTypeButtons(string buttonName)
        {
            if (buttonName == "Encrypt")
            {
                // Change buttons look
                EncryptButton.IsEnabled = false;
                EncryptButton.TextColor = Color.Black;
                DecryptButton.IsEnabled = true;
                DecryptButton.TextColor = Color.White;
                RecipientSection.IsVisible = true;

                // Show "sign" section
                SignCheckboxContainer.IsVisible = true;

                // Hide "SymmetricKey" section
                SymmetricKeySection.IsVisible = false;
            }
            else if (buttonName == "Decrypt")
            {
                // Change buttons look
                EncryptButton.IsEnabled = true;
                EncryptButton.TextColor = Color.White;
                DecryptButton.IsEnabled = false;
                DecryptButton.TextColor = Color.Black;
                RecipientSection.IsVisible = false;

                // Hide "sign" section
                SignCheckboxContainer.IsVisible = false;

                // Show "SymmetricKey" section
                SymmetricKeySection.IsVisible = true;
            }

            Textarea.Text = "";

            // Clear path when switching method
            SetFilePath();

            // Clear symmetric key path 
            SetSymmetricKeyPath();

            // Change "proceed" button text
            ProceedButton.Text = buttonName;

            // Set method in viewmodel
            proceedViewModel.SetMethod(buttonName);

            // Reset dropdown list indexes
            RecipientDropdownList.SelectedIndex = 0;
            MyKeyDropdownList.SelectedIndex = 0;
        }
    
        public void OnProceedClick(object sender, EventArgs e)
        {
            // Text from textarea
            string Text = Textarea.Text;

            // Recipient selected
            string RecipientSelected = (string)RecipientDropdownList.SelectedItem;

            // Private key selected
            string KeySelected = (string)MyKeyDropdownList.SelectedItem;

            // Do sign the message?
            bool Sign = SignCheckbox.IsChecked;
            
            try
            {
                Dictionary<string, string> results;
                string resultsMessage = "";

                // Run selected method
                if (proceedViewModel.Method == "Encrypt")
                    results = proceedViewModel.Proceed(Text, RecipientSelected, KeySelected, Sign);
                else if (proceedViewModel.Method == "Decrypt")
                    results = proceedViewModel.Proceed(Text, KeySelected);
                else
                    return;

                // Format results to display
                if (results.ContainsKey("SignedBy") && results["SignedBy"] != string.Empty)
                    resultsMessage += $"• Signed by {results["SignedBy"]} (verified). " + Environment.NewLine;
                else if (results.ContainsKey("SignedBy"))
                    resultsMessage += "• Data was not signed. " + Environment.NewLine;

                if (results.ContainsKey("TextBody"))
                {
                    Clipboard.SetTextAsync(results["TextBody"]);
                    resultsMessage += "• Output text copied to clipboard. " + Environment.NewLine;
                }

                if (results.ContainsKey("FileName"))
                    resultsMessage += "• File saved " + Environment.NewLine + $"/Download/{results["FileName"]} " + Environment.NewLine;

                if (results.ContainsKey("SymmKeyPath"))
                    resultsMessage += "• Symmetric key saved " + Environment.NewLine + $"/Download/{results["SymmKeyPath"]} " + Environment.NewLine;

                // Display results information
                DisplayAlert("Success", resultsMessage, "OK");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occurred while processing data: {ex.Message}, {ex.ToString()}");
                DisplayAlert("Error", $"Error occurred while processing data: {ex.Message}", "OK");
            }
        }
    }
}