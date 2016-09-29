using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.Serialization;
using System.Linq;

using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;

using WindowsPhone_SaveThePass.Model;
using Cryptocraphy;

// To change color according to current accent color Foreground="{StaticResource PhoneAccentBrush}"

namespace WindowsPhone_SaveThePass.Model_View
{
    /// <summary>
    /// The class that manages the main page. Serves as a link between the Model and the View.
    /// Notes: Create a proper Update method for the UI
    ///        it will need to also display which accounts are expired.
    /// </summary>
    class AccManager
    {
        public ObservableCollection<Account> LoadedAccounts { get { return loadedAccounts; } }      // Returns a reference to the acounts that are currently loaded.
        public static Account CurrentAccount                                                        // The currently selected account.
        {
            get { return currentAccount; }
            set { currentAccount = value; }
        }

        static Account currentAccount;
        static ObservableCollection<Account> loadedAccounts;             // A list containing all saved accounts.
        IStorageFolder localFolder;                                      // The folder used for the application from the system.
        IStorageFolder accountsFolder;                                   // The folder that is used for storing saved accounts.
        IStorageFile accountFile;                                        // A variable to access the files to be manipulated.

        public AccManager()
        {
            loadedAccounts = new ObservableCollection<Account>();
            localFolder = ApplicationData.Current.LocalFolder;
            CreateOpenAccountsFolder();
            HelperFunctions.Log("Account Manager initialised!");
        }

        private async void CreateOpenAccountsFolder()
        {
            accountsFolder = await localFolder.CreateFolderAsync("Accounts", CreationCollisionOption.OpenIfExists);
            HelperFunctions.Log("Accounts folder was created.\r\n");
        }

        #region Account Code
        public async Task Save(CreationCollisionOption onCollision)
        {
            if (currentAccount != null)
            {
                bool isSaved = await WriteAccount(currentAccount, onCollision);
                if (isSaved)
                    await new MessageDialog(currentAccount.AccountName + " saved").ShowAsync();
                else
                    await new MessageDialog("Account already exists!").ShowAsync();
            }
        }

        public async void SaveAll(CreationCollisionOption onCollision)
        {
            ObservableCollection<Account> tempList = new ObservableCollection<Account>();
            foreach (Account account in loadedAccounts)
                tempList.Add(account);

            loadedAccounts.Clear();
            foreach (Account account in tempList)
                await WriteAccount(account, onCollision);

            HelperFunctions.Log("All accounts have been saved!");
        }

        private async Task<bool> WriteAccount(Account account, CreationCollisionOption onCollision)
        {
            bool succeess = false;

            // The lock may be unessessary! Will look into it.
            object thisLock = new object();
            try
            {
                accountFile = await accountsFolder.CreateFileAsync(HelperFunctions.Crop(account.AccountName) + ".xml", onCollision);
                using (IRandomAccessStream stream = await accountFile.OpenAsync(FileAccessMode.ReadWrite))
                {
                    lock (thisLock)
                    {
                        using (Stream outputStream = stream.AsStreamForWrite())
                        {
                            DataContractSerializer serialiaser = new DataContractSerializer(typeof(Account));

                            Account toSerialise = EncryptAccount(account, LoginManager.Password);
                            serialiaser.WriteObject(outputStream, toSerialise);
                            succeess = true;
                        }
                    }
                }
                if (succeess)
                {
                    HelperFunctions.Log(account.AccountName + " Saved\r\n");
                    LoadAccounts();
                }
            }
            catch (Exception ex)
            {
                HelperFunctions.Log(ex.Message);
            }
            return succeess;
        }

        private Account EncryptAccount(Account account, string key)
        {
            Account toBeSerialised;
            string accountName = string.Empty;
            string username = string.Empty;
            string email = string.Empty;
            string password = string.Empty;
            string notes = string.Empty;

            if (key != null)
            {
                accountName = Chipher.EncryptMessage(account.AccountName, key);
                username = Chipher.EncryptMessage(account.Username, key);
                email = Chipher.EncryptMessage(account.Email, key);
                password = Chipher.EncryptMessage(account.Password, key);
                notes = Chipher.EncryptMessage(account.Notes, key);
                toBeSerialised = new Account(accountName, username, email, password, notes);
                toBeSerialised.SetSavedDate(account.DateSaved);
                toBeSerialised.SetExrirationDate(account.ExpireDate);

                return toBeSerialised;
            }
            else
            {
                HelperFunctions.Log("Major error, No key exists!");
                return Account.Empty();
            }
        }

        public async void LoadAccounts()
        {
            try
            {
                loadedAccounts.Clear();
                accountsFolder = await localFolder.CreateFolderAsync("Accounts", CreationCollisionOption.OpenIfExists);
                IReadOnlyList<StorageFile> accounts = await accountsFolder.GetFilesAsync();
                foreach (var account in accounts)
                {
                    Account toBeAdded = Account.Empty();
                    accountFile = account;
                    toBeAdded = await ReadAccount();
                    loadedAccounts.Add(toBeAdded);
                    HelperFunctions.Log("Account Loaded: " + toBeAdded.ToString());
                }
                HelperFunctions.Log("Accounts were successfully loaded!\r\n");
            }
            catch (Exception ex)
            {
                HelperFunctions.Log(ex.Message);
                await new MessageDialog("Cannot load account files beacause folder is empty.", "Error").ShowAsync();
            }
        }

        private async Task<Account> ReadAccount()
        {
            object thisLock = new object();
            Account temp = Account.Empty();
            using (IRandomAccessStream stream = await accountFile.OpenAsync(FileAccessMode.Read))
            {
                //Thread safe!
                lock (thisLock)
                {
                    using (Stream inputStream = stream.AsStreamForRead())
                    {
                        DataContractSerializer deserialiser = new DataContractSerializer(typeof(Account));
                        temp = deserialiser.ReadObject(inputStream) as Account;
                    }
                }
            }
            return DecryptAccount(temp);
        }

        private Account DecryptAccount(Account account)
        {
            Account Decrypted;
            string accountName = string.Empty;
            string username = string.Empty;
            string email = string.Empty;
            string password = string.Empty;
            string notes = string.Empty;

            if (LoginManager.Password != null)
            {
                accountName = Chipher.DecryptMessage(account.AccountName, LoginManager.Password);

                if (!String.IsNullOrEmpty(account.Username))
                    username = Chipher.DecryptMessage(account.Username, LoginManager.Password);
                else username = "Empty";
                if (!String.IsNullOrEmpty(account.Email))
                    email = Chipher.DecryptMessage(account.Email, LoginManager.Password);
                else email = "Empty";
                if (!String.IsNullOrEmpty(account.Notes))
                    notes = Chipher.DecryptMessage(account.Notes, LoginManager.Password);

                password = Chipher.DecryptMessage(account.Password, LoginManager.Password);
                Decrypted = new Account(accountName, username, email, password, notes);
                Decrypted.SetSavedDate(account.DateSaved);
                Decrypted.SetExrirationDate(account.ExpireDate);
                return Decrypted;
            }
            else
            {
                HelperFunctions.Log("Major error, No password exists!\n\r");
                return Account.Empty();
            }
        }

        public async Task DeleteAccount()
        {
            bool found = false;

            IReadOnlyList<StorageFile> accounts = await accountsFolder.GetFilesAsync();

            for (int i = 0; i < loadedAccounts.Count; i++)
            {
                if (HelperFunctions.Crop(currentAccount.AccountName) == HelperFunctions.Crop(loadedAccounts[i].AccountName) && !found)
                {
                    found = true;
                    loadedAccounts.RemoveAt(i);
                    HelperFunctions.Log("Removed from list!");
                    break;
                }
            }

            foreach (var account in accounts)
            {
                if (account.Name == HelperFunctions.Crop(currentAccount.AccountName) + ".xml")
                {
                    await account.DeleteAsync();
                    HelperFunctions.Log("File (" + account.Name + ") deleted!");
                    string[] fileName = account.Name.Split('.');
                    await new MessageDialog(fileName[0] + " deleted!").ShowAsync();
                    currentAccount = Account.Empty();
                    break;
                }
            }
            if (!found)
                await new MessageDialog("The account you want to delete doesn't exist or has already been deleted!").ShowAsync();
        }

        public async Task DeleteAllFiles()
        {
            accountsFolder = await localFolder.CreateFolderAsync("Accounts", CreationCollisionOption.OpenIfExists);
            IReadOnlyList<StorageFile> accounts = await accountsFolder.GetFilesAsync();

            for (int i = 0; i < accounts.Count; i++)
            {
                await accounts[i].DeleteAsync();
            }
            HelperFunctions.Log("All files have been deleted!");
        }

        // Refactor
        public bool SearchForAccount(string accountName)
        {
            //bool found = false;
            //foreach (var acc in loadedAccounts)
            //{
            //    if (HelperFunctions.Crop(accountName).ToLower() == HelperFunctions.Crop(acc.AccountName).ToLower())
            //    {
            //        currentAccount = acc;
            //        found = true;
            //    }
            //}
            //return found;

            int accNum = 0;
            foreach (var acc in SearchAccountsFor(accountName))
            {
                currentAccount = acc;
                accNum++;
            }
            bool found = accNum > 0 ?  true : false;

            return found;
        }

        private IEnumerable<Account> SearchAccountsFor(string accountName)
        {
            var accounts = from acc in loadedAccounts
                           where accountName.Crop().ToLower() == acc.AccountName.Crop().ToLower()
                           select acc;

            return accounts;
        }

        //public ListBox CheckForExpiredAccounts(ListBox list)
        //{
        //    bool doesExpire;
        //    ListBox listToReturn = new ListBox();
        //    Account changed = Account.Empty();
        //    for (int i = 0; i < loadedAccounts.Count; i++)
        //    {
        //        if (ExpirationDates.DaysUntilExpiration(loadedAccounts[i].ExpireDate, out doesExpire) < 0 && doesExpire)
        //        {
        //            for (int j = 0; j < list.Items.Count; j++)
        //            {
        //                if ((Account)list.Items[j] == loadedAccounts[i])
        //                    listToReturn.Items.Add(loadedAccounts[i].ExpiredAccount());
        //                else
        //                    listToReturn.Items.Add(loadedAccounts[i]);
        //            }
        //        }
        //    }
        //    return listToReturn;
        //}

        /// <summary>
        /// Checks if the given letters are icluded in the loaded accounts
        /// </summary>
        /// <param name="letters"></param>
        /// <returns></returns>
        public Task<ICollection<string>> MakeSuggestion(string letters)
        {
            return Task.Run(delegate
            {
                ICollection<string> suggestions = new List<string>();

                foreach (var item in loadedAccounts)
                    if (!string.IsNullOrEmpty(letters) && item.AccountName.Contains(letters))
                        suggestions.Add(item.AccountName);

                return suggestions;
            });
        }

        #endregion
    }
}
