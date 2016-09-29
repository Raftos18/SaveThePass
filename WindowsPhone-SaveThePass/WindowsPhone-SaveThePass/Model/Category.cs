using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Collections.ObjectModel;
using Windows.Storage;

namespace WindowsPhone_SaveThePass.Model
{
    [DataContract]
    class Category
    {
        [DataMember]
        private string name;                                    // The name of the category.
        [DataMember]
        private List<string> accountNames;

        private ObservableCollection<Account> accounts;        // Holds the accounts that are in this category.

        public ObservableCollection<Account> Accounts
        {
            get { return accounts; }
            set { accounts = value; }
        }

        public Category(string name)
        {
            this.name = name;
            accounts = new ObservableCollection<Account>();
        }



    }
}
