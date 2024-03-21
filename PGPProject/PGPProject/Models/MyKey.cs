using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System;
using Xamarin.Forms;
using Org.BouncyCastle.Utilities.IO.Pem;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Crypto.Parameters;
using Xamarin.Essentials;

namespace PGPProject.Models
{
    class MyKey
    {
        private static string privKeyName = "privatekey.pem";
        private static string pubKeyName = "publickey.pem";
        private static string myKeysPath;
        private static string contactKeysPath;
        private static string downloadsDirectory;
        
        public MyKey()
        {
            SetUpDirectories();

            CheckPermissions();
        }

        private async void CheckPermissions()
        {
            var storageRead = await Permissions.CheckStatusAsync<Permissions.StorageRead>();
            var storageWrite = await Permissions.CheckStatusAsync<Permissions.StorageWrite>();

            if (storageRead == PermissionStatus.Denied)
                await Permissions.RequestAsync<Permissions.StorageRead>();
            else if (storageWrite == PermissionStatus.Denied)
                await Permissions.RequestAsync<Permissions.StorageWrite>();

        }

        private void SetUpDirectories()
        {
            // Create a path for my keys directory
            myKeysPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "my_keys");

            // If default directory containing keys doesn't exist, create it
            if (!Directory.Exists(myKeysPath))
                Directory.CreateDirectory(myKeysPath);

            // Create a path for contacts keys directory
            contactKeysPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "contact_keys");

            // If default directory containing keys doesn't exist, create it
            if (!Directory.Exists(contactKeysPath))
                Directory.CreateDirectory(contactKeysPath);

            // Create a path to Download directory
            downloadsDirectory = (string)Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDownloads);
        }

        private string CreateNewKeyDirectory(string Name, bool IsMyKey)
        {
            string NewKeysPath;
            if(IsMyKey)
                NewKeysPath = Path.Combine(myKeysPath, Name);
            else
                NewKeysPath = Path.Combine(contactKeysPath, Name);

            // Create directory for new keys ring
            Directory.CreateDirectory(NewKeysPath);

            return NewKeysPath;
        }

        public void CreateKeyPair(string Name)
        {
            // Create directory for a new key pair
            string NewKeysPath = CreateNewKeyDirectory(Name, true);

            // Create paths for pub and priv keys
            string publicKeyPath = Path.Combine(NewKeysPath, pubKeyName);
            string privateKeyPath = Path.Combine(NewKeysPath, privKeyName);

            // Generate keys
            BouncyCastleLib.GenKeyPair(privateKeyPath, publicKeyPath);

            // Move public key to downloads in order to share it with someone
            CopyFileToDownloads(publicKeyPath);
            //CopyFileToDownloads(privateKeyPath);

            // Send notification to MessagingCenter the key is created
            MessagingCenter.Send(this, "KeysUpdated");
        }

        private void CopyFileToDownloads(string filePath)
        {
            string[] filePathParts = filePath.Split('/');
            string fileName = filePathParts[filePathParts.Length - 1];
            string keyName = filePathParts[filePathParts.Length - 2];

            // Get the path to the destination file in the Downloads folder
            string destinationFilePath = Path.Combine(downloadsDirectory, keyName + "_" + fileName);

            // Copy the file from the source to the destination
            File.Copy(filePath, destinationFilePath, true);
        }

        public void AddRecipientKey(string filePath, string keyName)
        {
            string destinationPath = Path.Combine(contactKeysPath, keyName);
            Directory.CreateDirectory(destinationPath);

            string destinationFileName = Path.Combine(destinationPath, pubKeyName);
            File.WriteAllBytes(destinationFileName, File.ReadAllBytes(filePath));

            // Send notification to MessagingCenter the contast is created
            MessagingCenter.Send(this, "KeysUpdated");
        }

        public void RemoveKey(string Name, bool IsMyKey)
        {
            if(IsMyKey)
                // Delete own key directory with all files
                Directory.Delete(Path.Combine(myKeysPath, Name), true);
            else 
                // Delete contact directory with all files
                Directory.Delete(Path.Combine(contactKeysPath, Name), true);

            // Send notification to MessagingCenter the key is created
            MessagingCenter.Send(this, "KeysUpdated");
        }

        public List<string[]> GetKeyNamesWithDates(bool AreMyKeys)
        {
            string[] dirs;
            if(AreMyKeys)
                dirs = Directory.GetDirectories(myKeysPath);
            else
                dirs = Directory.GetDirectories(contactKeysPath);

            List<string[]> outdirs = new List<string[]>();
            foreach (string dir in dirs)
            {
                
                // Key name and date created pairs
                string[] tuple = new string[2];

                // Split full path to the key and retrieve only directory name
                string[] parts = dir.Split('/');
                
                // Add values to tuples
                tuple[0] = parts[parts.Length - 1];
                // Get the creation date of a key ring
                tuple[1] = new FileInfo(dir).CreationTime.ToString();

                outdirs.Add(tuple);
            }
            return outdirs;
        }

        public List<string> GetKeyNames(bool AreMyKeys)
        {
            string[] dirs;
            if(AreMyKeys)
                dirs = Directory.GetDirectories(myKeysPath);
            else 
                dirs = Directory.GetDirectories(contactKeysPath);

            List<string> outdirs = new List<string>();
            foreach (string dir in dirs)
            {
                // Split full path to the key and retrieve only directory name
                string[] parts = dir.Split('/');

                // Add values to the list
                outdirs.Add(parts[parts.Length - 1]);
            }
            return outdirs;
        }

        public Dictionary<string, string> Encrypt(string Text, string InputFilePath, string RecipientName, string MyKeyName, bool Sign = false)
        {
            // Container for output
            Dictionary<string, string> results = new Dictionary<string, string>();
            
            // Get public key for recipient
            AsymmetricKeyParameter PublicKeyContent = ReadPublicKey(RecipientName, false);

            // Get private key for signing if requested
            AsymmetricKeyParameter PrivateKeyContent = null;
            if (Sign)
                PrivateKeyContent = ReadPrivateKey(MyKeyName);

            // Create symmetric key for encryption
            KeyParameter SymmetricKey = BouncyCastleLib.GenSymKey();

            // If file was selected
            if (InputFilePath != null)
            {
                /* 
                 * Set output file path
                 * 
                 * The name of output file consists of:
                 * - just string "encrypted_"
                 * - Recipient name, e.g. Alice
                 * - current datetime in short format like 03032024002438, which is 03.03.2024 00:24:38
                 * - original filename with extension
                 * 
                 * example encrypted_Alice_03032024002438_todo.txt
                 */
                string encryptedOutputFilePath = null;
                string encryptedOutputFileName = null;
                if (InputFilePath != null)
                {
                    string[] filePathParts = InputFilePath.Split('/');
                    encryptedOutputFileName = "encrypted_" + RecipientName + "_" + DateTime.Now.ToString("ddMMyyyyHHmmss") + "_" + filePathParts[filePathParts.Length - 1];
                    encryptedOutputFilePath = Path.Combine(downloadsDirectory, encryptedOutputFileName);
                }

                BouncyCastleLib.EncryptFile(InputFilePath, encryptedOutputFilePath, PrivateKeyContent, SymmetricKey, Sign);
                results.Add("FileName", encryptedOutputFileName);
            }

            // If text was provided
            if (Text != string.Empty)
            {
                string resText = BouncyCastleLib.EncryptText(Text, PrivateKeyContent, SymmetricKey, Sign);
                Console.WriteLine(resText);
                results.Add("TextBody", resText);
            }
                

            // Create path for the key
            string symmetricKeyName = "keyFor_" + RecipientName + "_" + DateTime.Now.ToString("ddMMyyyyHHmmss") + ".des";
            string symmetricKeyPath = Path.Combine(downloadsDirectory, symmetricKeyName);

            // Encrypt symmetric key with public key
            byte[] keyBytes = BouncyCastleLib.EncryptSymmetricKey(PublicKeyContent, SymmetricKey);
            File.WriteAllBytes(symmetricKeyPath, keyBytes);

            results.Add("SymmKeyPath", symmetricKeyName);

            return results;
        }

        public Dictionary<string, string> Decrypt(string Text, string InputFilePath, string SymmetricKeyInputFilePath, string MyKeyName)
        {
            // Get private key for signing if requested
            AsymmetricKeyParameter PrivateKeyContent = ReadPrivateKey(MyKeyName);

            // Get public keys for all recipients
            Dictionary<string, AsymmetricKeyParameter> PublicKeysContent = new Dictionary<string, AsymmetricKeyParameter>();
            foreach (string RecipientKeyName in GetKeyNames(false))
                PublicKeysContent.Add(RecipientKeyName, ReadPublicKey(RecipientKeyName, false));

            // Create a KeyParameter from the key bytes read from file
            KeyParameter symmetricKey = BouncyCastleLib.DecryptSymmetricKey(PrivateKeyContent, File.ReadAllBytes(SymmetricKeyInputFilePath));

            Dictionary<string, string> results = new Dictionary<string, string>();

            /* 
             * Set output file path
             * 
             * The name of output file consists of:
             * - just string "encrypted_"
             * - current datetime in short format like 03032024002438, which is 03.03.2024 00:24:38
             * - original filename with extension
             * 
             * example decrypted_03032024002438_todo.txt
             */
            if (InputFilePath != null)
            {
                string[] filePathParts = InputFilePath.Split('/');
                string encryptedOutputFileName = "decrypted_" + DateTime.Now.ToString("ddMMyyyyHHmmss") + "_" + filePathParts[filePathParts.Length - 1];

                string[] res = BouncyCastleLib.DecryptFile(InputFilePath, downloadsDirectory, encryptedOutputFileName, symmetricKey, PublicKeysContent);
                results.Add("FileName", res[0]);
                if (res[1] != string.Empty)
                results.Add("SignedBy", res[1]);
            }

            // If text was provided
            if (Text != string.Empty)
            {
                string[] res = BouncyCastleLib.DecryptText(Text, symmetricKey, PublicKeysContent);
                results.Add("TextBody", res[0]);
                if (!results.ContainsKey("SignedBy"))
                    results.Add("SignedBy", res[1]);
            }
                
            return results;
        }

        private AsymmetricKeyParameter ReadPublicKey(string Name, bool IsMyKey)
        {
            string publicKeyFilePath;
            if (IsMyKey)
                publicKeyFilePath = Path.Combine(myKeysPath, Name, pubKeyName);
            else
                publicKeyFilePath = Path.Combine(contactKeysPath, Name, pubKeyName);

            return ReadPublicKey(File.OpenRead(publicKeyFilePath));
        }

        private AsymmetricKeyParameter ReadPublicKey(Stream fileStream)
        {
            // Read the key in a very weird way
            TextReader textReader = new StreamReader(fileStream);
            PemReader pemReader = new PemReader(textReader);
            PemObject pemObject = pemReader.ReadPemObject();

            // Put the key to AsymmetricKeyparameter for further processing
            AsymmetricKeyParameter PublicKey = PublicKeyFactory.CreateKey(pemObject.Content);
            
            return PublicKey;
        }
       
        private AsymmetricKeyParameter ReadPrivateKey(string Name)
        {
            // Create path to retrieve private key from
            string privateKeyFilePath = Path.Combine(myKeysPath, Name, privKeyName);
            
            // Read the key in a very weird way
            TextReader textReader = new StreamReader(File.OpenRead(privateKeyFilePath));
            PemReader pemReader = new PemReader(textReader);
            PemObject pemObject = pemReader.ReadPemObject();

            // Put the key to AsymmetricKeyparameter for further processing
            AsymmetricKeyParameter PrivateKey = PrivateKeyFactory.CreateKey(pemObject.Content);
            
            return PrivateKey;
        }

        public static string ValidateKeyName(string Name, bool IsMyKey)
        {
            Name = Name.ToLower();

            // Check if name is empty
            if (string.IsNullOrEmpty(Name))
                return "Name must not be empty!";

            // Check if name is no longer than 30 chars
            if (Name.Length > 30)
                return "Name must be at most 30 characters long!";

            // Check if name is valid ( contains only alphanumeric characters )
            if (!IsAlphanumeric(Name))
                return "Name must contain only alphanumeric [a-zA-Z0-9] values!";

            // Check if directory exists
            if (IsMyKey && Directory.Exists(Path.Combine(myKeysPath, Name)))
                return "Name already exists!";
            else if(!IsMyKey && Directory.Exists(Path.Combine(contactKeysPath, Name)))
                return "Name already exists!";

            return null;
        }

        private static bool IsAlphanumeric(string input)
        {
            // Regular expression pattern to match only alphanumeric characters
            string pattern = "^[a-zA-Z0-9]*$";

            // Check if the input string matches the pattern
            return Regex.IsMatch(input, pattern);
        }
    }
}
