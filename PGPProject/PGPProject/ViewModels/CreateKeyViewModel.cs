using PGPProject.Models;
using System.ComponentModel;
using Xamarin.Forms;


namespace PGPProject.ViewModels
{
	public class CreateKeyViewModel : INotifyPropertyChanged
    {
        private string inputText;
        public string InputText
        {
            get { return inputText; }
            set
            {
                inputText = value;
                OnPropertyChanged(nameof(InputText));
            }
        }

        private string errorMessage;
        public string ErrorMessage
        {
            get { return errorMessage; }
            set
            {
                errorMessage = value;
                OnPropertyChanged(nameof(ErrorMessage));
            }
        }

        private bool isErrorVisible;
        public bool IsErrorVisible
        {
            get { return isErrorVisible; }
            set
            {
                isErrorVisible = value;
                OnPropertyChanged(nameof(IsErrorVisible));
            }
        }

        private bool isButtonEnabled;
        public bool IsButtonEnabled
        {
            get { return isButtonEnabled; }
            set
            {
                isButtonEnabled = value;
                OnPropertyChanged(nameof(IsButtonEnabled));
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

        public CreateKeyViewModel()
        {
            Key = new MyKey();

            // Initialize properties
            Title = "Create Keys";
            InputText = string.Empty;
            ErrorMessage = string.Empty;
            IsErrorVisible = false;
            IsButtonEnabled = false;
        }

        public bool ValidateNewKeyName(string newKeyName)
        {
            ErrorMessage = string.Empty;
            IsErrorVisible = false;
            IsButtonEnabled = true;

            string error = MyKey.ValidateKeyName(newKeyName, true);
            if (error != null)
            {
                ErrorMessage = error;
                IsErrorVisible = true;
                IsButtonEnabled = false;
                return false;
            }
            return true;
        }
        
        public void CreateNewKey(string keyName)
        {
            if (!ValidateNewKeyName(keyName))
                return;
            
            // Create key
            Key.CreateKeyPair(keyName);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}