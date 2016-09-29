using System;
using System.Collections.Generic;

using Windows.UI.Xaml;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

using WindowsPhone_SaveThePass.Model;
using WindowsPhone_SaveThePass.Model_View;
using WindowsPhone_SaveThePass.View;
using WindowsPhone_SaveThePass.Common;
// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556
/// <summary>
/// </summary>
namespace WindowsPhone_SaveThePass
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CentralPage : Page
    {

        public static bool IsToSave { get { return isToSave; } }

        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();

        private AccManager accManager;
        private static bool isToSave;
        private double listHeightOffset = Window.Current.Bounds.Height / 2.5;

        public CentralPage()
        {
            this.InitializeComponent();

            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;

            // Adjust the size of the list according to the screen.
            accountListBox.Height = Window.Current.Bounds.Height - listHeightOffset;
            accManager = new AccManager();
            accManager.LoadAccounts();
            isToSave = true;
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

        private void addAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            SetUpAdd();
        }

        private void EditAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            isToSave = false;
            AccManager.CurrentAccount = (Account)accountListBox.SelectedItem;
            if (AccManager.CurrentAccount != null)
            {
                Frame.Navigate(typeof(AddPage));
            }
        }

        static ContentDialog dialog;
        static ContentDialogResult result;
        private async void DeleteAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            if (accountListBox.SelectedItem != null)
            {
                AccManager.CurrentAccount = (Account)accountListBox.SelectedItem;
                dialog = HelperFunctions.FactoryDialog("Are you sure you want to delete " + AccManager.CurrentAccount.AccountName + "?", "Yes", "Cancel");
                if (dialog != null)
                {
                    dialog.PrimaryButtonClick += Dialog_PrimaryButtonClick;
                    result = await dialog.ShowAsync();
                }
            }
        }

        private async void Dialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            try
            {
                result = ContentDialogResult.None;
                if (AccManager.CurrentAccount != null)
                {
                    if (result == ContentDialogResult.Primary)
                        await accountManager.DeleteAccount();
                    else if (result == ContentDialogResult.Secondary)
                        HelperFunctions.Log("Delete canceled.");
                    else
                        await accountManager.DeleteAccount();
                }
            }
            catch (Exception ex)
            {
                HelperFunctions.Log(ex.Message);
            }
        }

        private void SetUpAdd()
        {
            AccManager.CurrentAccount = Account.Empty();
            Frame.Navigate(typeof(AddPage));
        }

        // Make a single Update Function that will control all the ui updates.
        // This code for checking expired account should go into the account manager.
        //private void UpdateUI()
        //{
        //    accountListBox = accManager.CheckForExpiredAccounts(accountListBox);
        //}

        private void accountListBox_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            AccManager.CurrentAccount = (Account)accountListBox.SelectedItem;

            if (AccManager.CurrentAccount != null)
            {
                HelperFunctions.Log(AccManager.CurrentAccount.AccountName + " was double tapped!");
                Frame.Navigate(typeof(PreviewPage));
            }
        }

        private void SearchAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            accountListBox.Visibility = Visibility.Collapsed;
            searchStack.Visibility = Visibility.Visible;
            searchCancelButton.Visibility = Visibility.Visible;
        }

        private async void searchTextBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            MessageDialog notFoundMSG;
            suggestionText.Text = "Suggestions: ";
            ICollection<string> suggestedAccounts = await accManager.MakeSuggestion(searchTextBox.Text);

            foreach (var account in suggestedAccounts)
                suggestionText.Text += account + ", ";
            
            if (e.Key == Windows.System.VirtualKey.Enter && !string.IsNullOrEmpty(searchTextBox.Text))
            {
                if (accManager.SearchForAccount(searchTextBox.Text))
                {
                    Frame.Navigate(typeof(PreviewPage));
                }
                else
                {
                    notFoundMSG = new MessageDialog("The account you are looking for doen't exist!");
                    searchTextBox.Text = string.Empty;
                    notFoundMSG.ShowAsync();

                    return;
                }
            }
            else
                return;
        }

        private void searchCancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.searchStack.Visibility = Visibility.Collapsed;
            this.searchTextBox.Text = String.Empty;
            this.searchCancelButton.Visibility = Visibility.Collapsed;
            this.accountListBox.Visibility = Visibility.Visible;
        }
    }
}