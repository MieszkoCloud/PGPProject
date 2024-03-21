using PGPProject.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace PGPProject.ViewModels
{
    public class ProceedViewModel : INotifyPropertyChanged
    {
        private List<string> recipients;
        public List<string> Recipients
        {
            get { return recipients; }
            set
            {
                recipients = value;
                OnPropertyChanged(nameof(Recipients));
            }
        }

        private List<string> myKeys;
        public List<string> MyKeys
        {
            get { return myKeys; }
            set
            {
                myKeys = value;
                OnPropertyChanged(nameof(MyKeys));
            }
        }

        private string filePath;
        public string FilePath
        {
            get { return filePath; }
            set
            {
                filePath = value;
                OnPropertyChanged(nameof(FilePath));
            }
        }

        private string symmetricKeyFilePath;
        public string SymmetricKeyFilePath
        {
            get { return symmetricKeyFilePath; }
            set
            {
                symmetricKeyFilePath = value;
                OnPropertyChanged(nameof(SymmetricKeyFilePath));
            }
        }

        private string method;
        public string Method
        {
            get { return method; }
            set
            {
                method = value;
                OnPropertyChanged(nameof(Method));
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

        public ProceedViewModel()
        {
            Key = new MyKey();

            // Initialize properties
            Title = "Encrypt/Decrypt data";
            GetKeyNames();
        }

        public void GetKeyNames()
        {
            // Update a list of "my keys"
            MyKeys = Key.GetKeyNames(true);

            // Update a list of recipient names
            Recipients = Key.GetKeyNames(false);
        }

        public void SetInputFilePath(string Path)
        {
            FilePath = Path;
        }

        public void SetInputSymmetricKeyPath(string Path)
        {
            SymmetricKeyFilePath = Path;
        }

        public void SetMethod(string method)
        {
            Method = method;
        }

        public Dictionary<string, string> Proceed(string Text, string RecipientSelected, string KeySelected, bool Sign = false)
        {
            return Key.Encrypt(Text, FilePath, RecipientSelected, KeySelected, Sign);
        }
        public Dictionary<string, string> Proceed(string Text, string KeySelected)
        {
            if (SymmetricKeyFilePath == null)
                return new Dictionary<string, string>();
            return Key.Decrypt(Text, FilePath, SymmetricKeyFilePath, KeySelected);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}