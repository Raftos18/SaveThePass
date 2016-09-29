using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Popups;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

using WindowsPhone_SaveThePass.Model_View;
using WindowsPhone_SaveThePass.View;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=391641

namespace WindowsPhone_SaveThePass
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Required;
            SetUp();
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            // TODO: Prepare page for display here.

            // TODO: If your application contains multiple pages, ensure that you are
            // handling the hardware Back button by registering for the
            // Windows.Phone.UI.Input.HardwareButtons.BackPressed event.
            // If you are using the NavigationHelper provided by some templates,
            // this event is handled for you.
        }

        private void buttonShowPassword_Tapped(object sender, TappedRoutedEventArgs e)
        {
            
        }

        private void SetUp()
        {
            passwordBox.Password = "";
            LoginManager.Instatiate();
            LoginManager.instance.LoadPassword();
            if(LoginManager.PasswordExists)
                helpMessage.Visibility = Visibility.Collapsed;
            else
                helpMessage.Visibility = Visibility.Visible;
        }

        private async void buttonConfirm_Tapped(object sender, TappedRoutedEventArgs e)
        {
            bool firstTime;
            if (LoginManager.instance.PasswordConfirmation(passwordBox.Password, out firstTime))
            {
                MessageDialog success;
                if (firstTime)
                {
                    success = new MessageDialog("Password saved");
                    passwordBox.Password = string.Empty;
                    await success.ShowAsync();
                }
                else
                {
                    success = new MessageDialog("Correct password");
                    passwordBox.Password = string.Empty;
                    await success.ShowAsync();
                }
                Frame.Navigate(typeof(CentralPage));
            }
            else
            {
                MessageDialog failure = new MessageDialog("Wrong password");
                await failure.ShowAsync();
            }
        }

        private void ButtonChangePassword_Tapped(object sender, TappedRoutedEventArgs e)
        {
            // Move to change your password page!
            Frame.Navigate(typeof(ChangePasswordPage));
        }
    
        private void Image_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Frame.Navigate(typeof(HelpPage));
        }
    }
}
