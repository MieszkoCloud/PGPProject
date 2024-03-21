using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Security;
using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Utilities.IO.Pem;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Crypto.Parameters;
using System.Security.Cryptography;

namespace PGPProject.Models
{
    class BouncyCastleLib
    {
        private static string SignAlghorithm = "SHA256withRSA";
        private static int SignatureLength = 256;

        public BouncyCastleLib() { }
        public static void GenKeyPair(string privateKeyPath, string publicKeyPath) 
        {
            RsaKeyPairGenerator rsaGenerator = new RsaKeyPairGenerator();
            rsaGenerator.Init(new KeyGenerationParameters(new SecureRandom(), 2048));
            AsymmetricCipherKeyPair keyPair = rsaGenerator.GenerateKeyPair();

            // Save public key to file
            using (TextWriter writer = new StreamWriter(publicKeyPath, false))
            {
                var pemWriter = new Org.BouncyCastle.OpenSsl.PemWriter(writer);
                pemWriter.WriteObject(keyPair.Public);
            }

            // Convert private key to PKCS#8 format
            PemObject pemObj = new Pkcs8Generator(keyPair.Private).Generate();

            // Write PKCS#8 private key to a file
            using (TextWriter writer = new StreamWriter(privateKeyPath, false))
            {
                var pemWriter = new Org.BouncyCastle.OpenSsl.PemWriter(writer);
                pemWriter.WriteObject(pemObj);
            }
        }

        public static KeyParameter GenSymKey()
        {
            // Create a DES key
            DESCryptoServiceProvider desProvider = new DESCryptoServiceProvider();

            // Generate a random DES key
            desProvider.GenerateKey();

            // Get the generated key
            return new KeyParameter(desProvider.Key);
        }

        public static string EncryptText(string plaintext, AsymmetricKeyParameter privateKey, KeyParameter symmetricKey, bool Sign = false)
        {
            // Ensure no whitespaces at the beginning and at the end 
            plaintext.Trim();

            // Read input plaintext
            byte[] inputBytes = Encoding.UTF8.GetBytes(plaintext);

            return Convert.ToBase64String(EncryptData(inputBytes, privateKey, symmetricKey, Sign));
        }

        public static string[] DecryptText(string plaintext, KeyParameter symmetricKey, Dictionary<string, AsymmetricKeyParameter> publicKeys)
        {
            // Data bytes
            byte[] inputBytes = Convert.FromBase64String(plaintext);

            byte[][] inputParts = ExtractSignatureIfPresent(inputBytes);
            byte[] rawBytes = inputParts[0];
            byte[] signature = inputParts[1];
            string signedBy = "";

            // If any signature extracted check if signed
            if (signature.Length > 0)
            {
                signedBy = VerifySignature(rawBytes, signature, publicKeys);

                // Remove signature
                if (signedBy != string.Empty)
                    inputBytes = rawBytes;
            }

            // Decrypt
            byte[] decryptedBytes = DecryptData(inputBytes, symmetricKey);

            return new string[2] { Encoding.UTF8.GetString(decryptedBytes), signedBy };
        }

        public static void EncryptFile(string inputFilePath, string outputFilePath, AsymmetricKeyParameter privateKey, KeyParameter symmetricKey, bool Sign = false)
        {
            // Read input file
            byte[] inputBytes = System.IO.File.ReadAllBytes(inputFilePath);

            // Encrypt data
            byte[] encryptedBytes = EncryptData(inputBytes, privateKey, symmetricKey, Sign);


            // Write encrypted data to output file
            System.IO.File.WriteAllBytes(outputFilePath, encryptedBytes);
        }

        public static string[] DecryptFile(string inputFileName, string outputFilePath, string outputFileName, KeyParameter symmetricKey, Dictionary<string, AsymmetricKeyParameter> publicKeys)
        {
            // Read encrypted file
            byte[] encryptedBytes = System.IO.File.ReadAllBytes(inputFileName);

            byte[][] inputParts = ExtractSignatureIfPresent(encryptedBytes);
            byte[] rawBytes = inputParts[0];
            byte[] signature = inputParts[1];
            string signedBy = "";

            // If any signature extracted check if signed
            if (signature.Length > 0)
                signedBy = VerifySignature(rawBytes, signature, publicKeys);

            if (signedBy != string.Empty)
            {
                outputFilePath = Path.Combine(outputFilePath, "verifiedFrom_" + signedBy + "_" + outputFileName);
                // Remove signature from input bytes
                encryptedBytes = rawBytes;
            }
            else
                outputFilePath = Path.Combine(outputFilePath, "unverified_" + outputFileName);
            
            // Decrypt data
            byte[] decryptedBytes = DecryptData(encryptedBytes, symmetricKey);
            
            // Write decrypted data to output file
            System.IO.File.WriteAllBytes(outputFilePath, decryptedBytes);

            return new string[2] { outputFileName, signedBy };
        }

        private static byte[] EncryptData(byte[] inputBytes, AsymmetricKeyParameter privateKey, KeyParameter key, bool Sign = false)
        {
            // First encrypt data 
            var cipher = CipherUtilities.GetCipher("DES/ECB/PKCS5Padding");
            cipher.Init(true, key);
            inputBytes = cipher.DoFinal(inputBytes);

            // Then sign and return
            if (Sign)
                inputBytes = SignData(inputBytes, privateKey);

            return inputBytes;
        }

        private static byte[] DecryptData(byte[] encryptedData, KeyParameter key)
        {
            var cipher = CipherUtilities.GetCipher("DES/ECB/PKCS5Padding");
            cipher.Init(false, key);
            return cipher.DoFinal(encryptedData);
        }

        public static byte[] EncryptSymmetricKey(AsymmetricKeyParameter publicKey, KeyParameter symmetricKey)
        {
            // Encrypt signed data and return
            var cipher = CipherUtilities.GetCipher("RSA/ECB/PKCS1");
            cipher.Init(true, publicKey);
            return cipher.DoFinal(symmetricKey.GetKey());
        }

        public static KeyParameter DecryptSymmetricKey(AsymmetricKeyParameter privateKey, byte[] symmetricKey)
        {
            var symmetricCipher = CipherUtilities.GetCipher("RSA/ECB/PKCS1");
            symmetricCipher.Init(false, privateKey);
            return new KeyParameter(symmetricCipher.DoFinal(symmetricKey));
        }

        private static byte[] SignData(byte[] inputBytes, AsymmetricKeyParameter privateKey)
        {
            // Create signature
            ISigner signer = SignerUtilities.GetSigner(SignAlghorithm);
            signer.Init(true, privateKey);
            signer.BlockUpdate(inputBytes, 0, inputBytes.Length);
            byte[] signature = signer.GenerateSignature();

            // Combine data with signature
            byte[] combinedData = new byte[inputBytes.Length + signature.Length];

            // Concatenate signature with input data
            Array.Copy(inputBytes, 0, combinedData, 0, inputBytes.Length);
            Array.Copy(signature, 0, combinedData, inputBytes.Length, signature.Length);

            /* DEBUG
            Console.WriteLine("AA>A>>> INPUT BYTES: " + BitConverter.ToString(inputBytes).Replace("-", ""));
            Console.WriteLine("AA>A>>> SIGNATURE: " + BitConverter.ToString(signature).Replace("-", ""));
            Console.WriteLine("AA>A>>> COMBINED: " + BitConverter.ToString(combinedData).Replace("-", ""));
            /**/
            return combinedData;
        }

        private static byte[][] ExtractSignatureIfPresent(byte[] bytes)
        {
            if (bytes.Length > SignatureLength)
            {
                // Determine the length and position of the signature within the decrypted data
                int signatureIndex = bytes.Length - SignatureLength;

                // Extract the signature from the decrypted data
                byte[] signature = new byte[SignatureLength];
                Array.Copy(bytes, signatureIndex, signature, 0, SignatureLength);

                Console.WriteLine("AA>A>>> INPUT BYTES: " + BitConverter.ToString(bytes).Replace("-", ""));
                Console.WriteLine("AA>A>>> SIGNATURE: " + BitConverter.ToString(signature).Replace("-", ""));

                // Prepare raw data byte array
                byte[] rawBytes = new byte[signatureIndex];
                Array.Copy(bytes, 0, rawBytes, 0, signatureIndex);

                return new byte[2][] { rawBytes, signature };
            }

            return new byte[2][] { bytes, new byte[0] };
        }

        private static string VerifySignature(byte[] bytes, byte[] signature, Dictionary<string, AsymmetricKeyParameter> publicKeys)
        {
            // Create a signer instance
            ISigner signer = SignerUtilities.GetSigner(SignAlghorithm);

            // Check every recipient's public key to establish who signed it
            foreach (KeyValuePair<string, AsymmetricKeyParameter> pair in publicKeys) 
            {
                Console.WriteLine($">>>> Dict key: {pair.Key}");

                // Initialize the signer with the public key for verification
                signer.Init(false, pair.Value);

                // Update the signer with the data to be verified
                signer.BlockUpdate(bytes, 0, bytes.Length);

                // Verify the signature
                if (signer.VerifySignature(signature))
                    return pair.Key;
            }
            return "";
        }
    }
}
