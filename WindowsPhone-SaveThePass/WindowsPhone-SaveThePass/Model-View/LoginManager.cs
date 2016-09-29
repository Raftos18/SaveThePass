using System;
using System.IO;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Popups;
using WindowsPhone_SaveThePass.Model;

namespace WindowsPhone_SaveThePass.Model_View
{

    /// <summary>
    /// Use Hash for the key and apply an update methods so the program will not break from the prev version.
    /// </summary>

    class LoginManager
    {
        public static LoginManager instance;     // Static variable from which you can have access to the class.

        IStorageFolder localFolder;              // The folder used for the application from the system.
        IStorageFolder passwordFolder;           // The folder that is used for storing the application password
        IStorageFile passwordFile;               // A variable to access the password file.
        public static string Password { get; private set; }                         // Holds the current password
        public static string HashedPassword { get; private set; }                          // Holds a reference to the password hash saved in the file.
        public static bool PasswordExists { get; private set; }                     // A static bool to indicate if a password file exists.

        
        object thisLock = new object();

        private LoginManager()
        {
            localFolder = ApplicationData.Current.LocalFolder;
            CreatePasswordFolder();
        }

        public static LoginManager Instatiate()
        {
            if (instance == null)
            {
                instance = new LoginManager();
                Password = string.Empty;
                HashedPassword = string.Empty;
                PasswordExists = false;
            }
            return instance;
        }

        public async void CreatePasswordFolder()
        {
            if (localFolder != null)
            {
                passwordFolder = await localFolder.CreateFolderAsync("Password", CreationCollisionOption.OpenIfExists);
                HelperFunctions.Log("Password folder created/Opened!");
            }
        }

        public async void SavePassword(string password)
        {
            try
            {
                passwordFile = await passwordFolder.CreateFileAsync("password.xml", CreationCollisionOption.ReplaceExisting);
                using (IRandomAccessStream stream = await passwordFile.OpenAsync(FileAccessMode.ReadWrite))
                {
                    // Thread safe
                    lock (this)
                    {
                        using (Stream outputStream = stream.AsStreamForWrite())
                        {
                            DataContractSerializer serialiser = new DataContractSerializer(typeof(string));
                            serialiser.WriteObject(outputStream, password.GetHashCode());
                            HelperFunctions.Log("Password saved");
                        }
                    }
                }
            }
            catch (System.UnauthorizedAccessException ex)
            {
                HelperFunctions.Log(ex.Message);
            }
        }

        public async void LoadPassword()
        {
            try
            {
                PasswordExists = true;
                string password = "";
                passwordFolder = await localFolder.CreateFolderAsync("Password", CreationCollisionOption.OpenIfExists);
                passwordFile = await passwordFolder.GetFileAsync("password.xml");
                using (IRandomAccessStream stream = await passwordFile.OpenAsync(FileAccessMode.Read))
                {
                    using (Stream inputStream = stream.AsStreamForRead())
                    {
                        DataContractSerializer deserialiser = new DataContractSerializer(typeof(string));
                        password = deserialiser.ReadObject(inputStream) as string;
                        HashedPassword = password;
                    }
                }
            }
            catch (Exception ex)
            {
                PasswordExists = false;
                // For Debug Only!
                HelperFunctions.Log(ex.Message + "\n Loading password failed!.");
            }
        }

        public async Task<bool> ChangePassword(string oldPassword, string newPassword)
        {
            bool changed = false;
            if (PasswordExists)
            {
                AccManager KeyChanger;

                if (HashedPassword == oldPassword.GetHashCode().ToString())
                {
                    // Load all accounts and decrypts them with the old password.
                    // So now loadedAccounts hold the decrypted values of all accounts.
                    KeyChanger = new AccManager();
                    KeyChanger.LoadAccounts();

                    // We delete all the old files.
                    await KeyChanger.DeleteAllFiles();

                    // Change the password to the new one.
                    Password = newPassword;

                    SavePassword(newPassword);

                    // Saves again all accounts but with the new password as a key!
                    KeyChanger.SaveAll(CreationCollisionOption.ReplaceExisting);

                    await new MessageDialog("Your password has been changed!").ShowAsync();
                    changed = true;
                }
                else
                {
                    await new MessageDialog("Old password is wrong!", "Error").ShowAsync();
                    changed = false;
                }
            }
            else
            {
                // Message to inform that no Password exist as of yet.
                await new MessageDialog("No password file exists!", "Error").ShowAsync();
            }
            return changed;
        }

        public bool PasswordConfirmation(string password, out bool firstTime)
        {
            if (password != null && !PasswordExists)
            {
                Password = password;
                SavePassword(password);
                firstTime = true;
                return true;
            }
            else
            {
                firstTime = false;
                LoadPassword();
                if (HashedPassword == password.GetHashCode().ToString())
                {
                    Password = password;
                    return true;
                }
                else
                    return false;
            }
        }
    }
}
