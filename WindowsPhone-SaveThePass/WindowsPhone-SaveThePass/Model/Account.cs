using System;
using System.Runtime.Serialization;

namespace WindowsPhone_SaveThePass.Model
{
    /// <summary>
    /// Most likely date saved will be removed because we only really need it calculate when the expiration date will
    /// be and this can be done when a account is saved using DateTime.Now.
    /// </summary>
    [DataContract]
    class Account
    {
        [DataMember]
        private string accountName;
        [DataMember]
        private string username;
        [DataMember]
        private string email;
        [DataMember]
        private string password;
        [DataMember]
        private string notes;
        [DataMember]
        private DateTime dateSaved;
        [DataMember]
        private DateTime expireDate;

        //Getters
        public string AccountName => accountName;
        public string Username => username;
        public string Email => email;
        public string Password => password;
        public string Notes => notes;
        public DateTime DateSaved => dateSaved;
        public DateTime ExpireDate => expireDate;

        public Account(string accountName, string username, string email, string password, string notes)
        {
            this.accountName = accountName;
            this.username = username;
            this.email = email;
            this.password = password;
            this.notes = notes;
        }

        #region C# 5

        //public string AccountName { get { return accountName; } }
        //public string Username { get { return username; } }
        //public string Email { get { return email; } }
        //public string Password { get { return password; } }
        //public string Notes { get { return notes; } }
        //public DateTime DateSaved { get { return dateSaved; } }
        //public DateTime ExpireDate { get { return expireDate; } }

        //public override string ToString()
        //{
        //    string AccountInfo = "\n" + accountName + ":\r\n" + "-" + username + "\r\n" + "-" + email + "\r\n" + "-" + password + "\r\n" + "Saved on: " + dateSaved + "\n";
        //    return AccountInfo;
        //}

        //public static Account Empty()
        //{
        //    Account temp = new Account("","","","","");
        //    return temp;
        //}

        /// <summary>
        /// To check acounts for search!
        /// </summary>
        /// <returns></returns>
        //public Account AccountToLower()
        //{
        //    Account temp = this;
        //    temp.accountName = accountName.ToLower();
        //    temp.username = username.ToLower();
        //    temp.email = email.ToLower();
        //    temp.password = password.ToLower();

        //    return temp;
        //}

        //public void SetSavedDate(DateTime date)
        //{
        //    dateSaved = date;
        //}

        //public void SetExrirationDate(DateTime exprDate)
        //{
        //    this.expireDate = exprDate;
        //}
        #endregion


        public Account AccountToLower() =>
            new Account(accountName.ToLower(), username.ToLower(),
                email.ToLower(), password.ToLower(), notes.ToLower());

        public static Account Empty() => new Account("", "", "", "", "");

        public void SetSavedDate(DateTime date) => dateSaved = date;

        public void SetExrirationDate(DateTime exprDate) => expireDate = exprDate;

        public override string ToString() => $"\n {accountName} :\r\n - {username} \r\n - {email }  \r\n -  {password } \r\n Saved on: {dateSaved} \n";
    }
}
