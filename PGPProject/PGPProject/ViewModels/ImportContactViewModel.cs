using PGPProject.Models;
using System.ComponentModel;

namespace PGPProject.ViewModels
{
    public class ImportContactViewModel : INotifyPropertyChanged
    {
        private string contactKeyFilePath;
        private string ContactKeyFilePath
        {
            get { return contactKeyFilePath; }
            set
            {
                contactKeyFilePath = value;
                OnPropertyChanged(nameof(ContactKeyFilePath));
            }
        }

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

        public ImportContactViewModel()
        {
            Key = new MyKey();

            // Initialize properties
            Title = "Import Contact";
            InputText = string.Empty;
            ErrorMessage = string.Empty;
            IsErrorVisible = false;
            IsButtonEnabled = false;
        }

        public void SetContactFilePath(string Path)
        {
            ContactKeyFilePath = Path;
        }

        public bool ValidateName(string NewContactName)
        {
            ErrorMessage = string.Empty;
            IsErrorVisible = false;
            IsButtonEnabled = true;

            string error = MyKey.ValidateKeyName(NewContactName, false);
            if (error != null)
            {
                ErrorMessage = error;
                IsErrorVisible = true;
            }

            if (error != null || ContactKeyFilePath == null)
            {
                IsButtonEnabled = false;
                return false;
            }

            return true;
        }

        public void SaveContact(string Name)
        {
            if (!ValidateName(Name))
                return;

            Key.AddRecipientKey(ContactKeyFilePath, Name);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}