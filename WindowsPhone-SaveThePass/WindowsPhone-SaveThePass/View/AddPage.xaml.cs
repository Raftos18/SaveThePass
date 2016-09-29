using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Media;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using WindowsPhone_SaveThePass.Common;


using WindowsPhone_SaveThePass.Model;
using WindowsPhone_SaveThePass.Model_View;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace WindowsPhone_SaveThePass.View
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AddPage : Page
    {
        private AccManager accManager;              // Account manager reference variable.

        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();

        public AddPage()
        {
            this.InitializeComponent();

            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;

            // Set up Add page
            MainPanel.Width = Window.Current.Bounds.Width;

            if (!CentralPage.IsToSave)
                DeactivateAccountTextBox();

            accManager = new AccManager();
            UpdateCurrentAccount();
            ExpirationDates.AddToComboBox(expirationComboBox);
        }

        /// <summary>
        /// Gets the <see cref="NavigationHelper"/> associated with this <see cref="Page"/>.
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }

        /// <summary>
        /// Gets the view model for this <see cref="Page"/>.
        /// This can be changed to a strongly typed view model.
        /// </summary>
        public ObservableDictionary DefaultViewModel
        {
            get { return this.defaultViewModel; }
        }

        /// <summary>
        /// Populates the page with content passed during navigation.  Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="sender">
        /// The source of the event; typically <see cref="NavigationHelper"/>
        /// </param>
        /// <param name="e">Event data that provides both the navigation parameter passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested and
        /// a dictionary of state preserved by this page during an earlier
        /// session.  The state will be null the first time a page is visited.</param>
        private void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="sender">The source of the event; typically <see cref="NavigationHelper"/></param>
        /// <param name="e">Event data that provides an empty dictionary to be populated with
        /// serializable state.</param>
        private void NavigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
        }

        #region NavigationHelper registration

        /// <summary>
        /// The methods provided in this section are simply used to allow
        /// NavigationHelper to respond to the page's navigation methods.
        /// <para>
        /// Page specific logic should be placed in event handlers for the  
        /// <see cref="NavigationHelper.LoadState"/>
        /// and <see cref="NavigationHelper.SaveState"/>.
        /// The navigation parameter is available in the LoadState method 
        /// in addition to page state preserved during an earlier session.
        /// </para>
        /// </summary>
        /// <param name="e">Provides data for navigation methods and event
        /// handlers that cannot cancel the navigation request.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedFrom(e);
        }

        #endregion


        private async void SaveAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            if (accountTextBox.Text != "" && passwordTextBox.Text != "")
            {
                // Check if all input are valid and then proceed.
                if (await ValidateInputs())
                {
                    DateTime today = DateTime.Today;
                    Account temp = new Account(accountTextBox.Text, usernameTextBox.Text, emailTextBox.Text, passwordTextBox.Text, notesTextBox.Text);
                    temp.SetSavedDate(today);
                    temp.SetExrirationDate(ExpirationDates.CalculateExpirationDate((string)expirationComboBox.SelectedItem, today));

                    //Set temp as current account
                    AccManager.CurrentAccount = temp;

                    if (CentralPage.IsToSave)
                    {
                        await accManager.Save(CreationCollisionOption.FailIfExists);
                        Frame.GoBack();
                    }
                    else
                    {
                        await accManager.Save(CreationCollisionOption.ReplaceExisting);
                     
                        Frame.GoBack();
                    }
                }
            }
            else
            {
                await new MessageDialog("Fill all the nessessary fields").ShowAsync();
            }
        }

        private async void GenerateAppBarButton_Click(object sender, RoutedEventArgs e)
        {
               if (passwordTextBox.Text == "")
                {
                    string generatedPassword = HelperFunctions.GeneratePassword();
                    HelperFunctions.Log("Password Generated (" + generatedPassword + ").");
                    passwordTextBox.Text = generatedPassword;
                }
                else
                    await new MessageDialog("Password already generated.").ShowAsync();
        }

        /// <summary>
        /// Clears all boxes
        /// </summary>
        private void ClearBoxes()
        {
            accountTextBox.Text = "";
            usernameTextBox.Text = "";
            emailTextBox.Text = "";
            passwordTextBox.Text = "";
        }

        /// <summary>
        /// Update UI with the current account in the system
        /// </summary>
        private void UpdateCurrentAccount()
        {
            accountTextBox.Text = AccManager.CurrentAccount.AccountName;
            usernameTextBox.Text = AccManager.CurrentAccount.Username;
            emailTextBox.Text = AccManager.CurrentAccount.Email;
            passwordTextBox.Text = AccManager.CurrentAccount.Password;
            notesTextBox.Text = AccManager.CurrentAccount.Notes;
        }

        /// <summary>
        /// Validates input values
        /// </summary>
        /// <returns></returns>
        private async Task<bool> ValidateInputs()
        {
            char invalid;
            if (!IsValidString(accountTextBox.Text, out invalid))
            {
                await new MessageDialog("Invalid character in Account " + "(" + invalid + ")").ShowAsync();
                return false;
            }
            else if (!IsValidString(usernameTextBox.Text, out invalid))
            {
                await new MessageDialog("Invalid character in Username " + "(" + invalid + ")").ShowAsync();
                return false;
            }
            else if (!String.IsNullOrEmpty(emailTextBox.Text) && !IsValidEmail(emailTextBox.Text))
            {
                await new MessageDialog("Please provide a valid email address!").ShowAsync();

                return false;
            }
            else if (!IsValidString(passwordTextBox.Text, out invalid))
            {
                await new MessageDialog("Invalid character in password " + "(" + invalid + ")").ShowAsync();
                return false;
            }
            else return true;
        }

        /// <summary>
        /// Returns false if input contains some unwanted character.
        /// Also returns a reference to that character.
        /// </summary>
        /// <param name="inputString"></param>
        /// <param name="invalidChar"></param>
        /// <returns></returns>
        private bool IsValidString(string inputString, out char invalidChar)
        {
            invalidChar = default(char);
            char[] invalidChars = { '=', '<', '>', '+','/','\\', ',',
                                    '#', '$', '%', '&', '?','%' };
            

            foreach (var symbol in inputString)
            {
                foreach (var invChars in invalidChars)
                {
                    if (symbol == invChars)
                    {
                        invalidChar = symbol;
                        return false;
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// Check if the input email is of valid format
        /// </summary>
        /// <param name="inputEmail"></param>
        /// <returns></returns>
        private bool IsValidEmail(string inputEmail)
        {
            string strRegex = @"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}" +
                  @"\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\" +
                  @".)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$";

            Regex re = new Regex(strRegex);

            if (re.IsMatch(inputEmail))
                return true;
            else
                return false;
        }

        /// <summary>
        /// If the user wants to edit disable the account text box
        /// </summary>
        private void DeactivateAccountTextBox()
        {
            this.accountTextBox.IsEnabled = false;
        }

        /// <summary>
        /// Move the panel up in order for the combo box to fit
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void expirationComboBox_DropDownOpened(object sender, object e)
        {
            MainPanel.Margin = new Thickness(0, -100, 0, 0);
            LogoTextBlock.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Return to default position
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void expirationComboBox_DropDownClosed(object sender, object e)
        {
            MainPanel.Margin = new Thickness(0, 0, 0, 0);
            LogoTextBlock.Visibility = Visibility.Visible;
        }

        private void MainScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            var inputPane = InputPane.GetForCurrentView();
            inputPane.TryHide();
        }
    }
}

