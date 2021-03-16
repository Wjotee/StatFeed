using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using StatFeed.Class;

namespace StatFeed.Pages
{
    /// <summary>
    /// Interaction logic for LoginPageFinance.xaml
    /// </summary>
    public partial class LoginPageFinance : Page
    {
        //Properties
        int ServiceTypeID = 2;
        int chosen_Service = 0;

        public LoginPageFinance()
        {
            InitializeComponent();
            OnLoad();
        }

        public void OnLoad()
        {
            //Populate dropdown with available platforms
            PopulateLoginPageFinanceMarketsCombobox();


            Back_Button.Visibility = Visibility.Collapsed;
            bool result = SqliteDataAccess.FirstTime();

            if (!result)
            {
                Back_Button.Visibility = Visibility.Visible;
            }

        }

        public void PopulateLoginPageFinanceMarketsCombobox()
        {
            Market_Combobox.ItemsSource = SqliteDataAccess.GetAvailableFinance();

            Market_Combobox.SelectedIndex = 0;
        }

        private void Background_Upload_Button_Click(object sender, MouseButtonEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.InitialDirectory = "c:\\";
            dlg.Filter = "Image files (*.jpg)|*.jpg|All Files (*.*)|*.*";
            dlg.RestoreDirectory = true;

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                string selectedFileName = dlg.FileName;
                Background_Upload_Textbox.Text = selectedFileName;
                ChangeBackground(selectedFileName);
            }
        }
        private void GameTab_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.NavigationService.Navigate(new LoginPageGame());
        }
        private void Back_Button_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService.GoBack();
        }
        private void Market_Combobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FinanceModel currentFinance = (FinanceModel)Market_Combobox.SelectedItem;

            //Hide the APIKey_Textbox, button and APISecret textbox
            APIKey_Textbox.Visibility = Visibility.Collapsed;
            APISecret_Textbox.Visibility = Visibility.Collapsed;
            API_Button.Visibility = Visibility.Collapsed;

            //checks the Finance table to see if the keyis required
            if (currentFinance.KeyRequired)
            {
                APIKey_Textbox.Visibility = Visibility.Visible;
                API_Button.Visibility = Visibility.Visible;

                if (currentFinance.SecretRequired)
                {
                    APISecret_Textbox.Visibility = Visibility.Visible;
                }
            }

            //Background Image
            BitmapImage BackgroundBitmap = new BitmapImage();
            BackgroundBitmap.BeginInit();
            BackgroundBitmap.UriSource = new Uri(currentFinance.BackgroundURL, UriKind.RelativeOrAbsolute);
            BackgroundBitmap.EndInit();
            Background_Image.Source = BackgroundBitmap;
        }
        private void API_Button_Click(object sender, RoutedEventArgs e)
        {
            FinanceModel currentFinance = (FinanceModel)Market_Combobox.SelectedItem;

            System.Diagnostics.Process.Start(currentFinance.APIURL);
        }

        private void ChangeBackground(string selectedFileName)
        {
            //Takes the selected filename
            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(selectedFileName, UriKind.RelativeOrAbsolute);
            bitmap.EndInit();

            //Background Image
            Background_Image.Source = bitmap;
        }

        private void StockTicker_Textbox_GotFocus(object sender, RoutedEventArgs e)
        {
            StockTicker_Textbox.Foreground = new SolidColorBrush(Colors.White);
        }

        private void Login_Button_Click(object sender, RoutedEventArgs e)
        {
            //Create object of current finance ticker to check if it works
            FinanceModel CheckCurrentFinance = (FinanceModel)Market_Combobox.SelectedItem;
            string currentTicker = StockTicker_Textbox.Text;
            string APIKey = APIKey_Textbox.Text;
            string APISecret = APISecret_Textbox.Text;

            string customBackground = Background_Upload_Textbox.Text;

            SubscribedGameModel TempSubscription = new SubscribedGameModel(0, 2, CheckCurrentFinance.ID, currentTicker, 0, APIKey, APISecret, 0, customBackground);

            List<StatModel> TempStats = new List<StatModel>(StatModel.GenerateStats(TempSubscription));

            bool check = !TempStats.Any();

            //If the Ticker is legitimate it will return stats from this query
            if (!check)
            {
                bool duplicates = SubscribedGameModel.CheckDuplicates(currentTicker,2, CheckCurrentFinance.ID, chosen_Service);

                if (duplicates)
                {
                    StockTicker_Textbox.Foreground = new SolidColorBrush(Colors.Orange);
                }
                else
                {
                    StockTicker_Textbox.Foreground = new SolidColorBrush(Colors.Green);

                    //Saves the subscription information
                    SqliteDataAccess.SaveSubscribedGame(ServiceTypeID, CheckCurrentFinance.ID, currentTicker, chosen_Service, APIKey, APISecret, 0, customBackground);

                    //Get full list of subscriptions
                    List<SubscribedGameModel> SubscriptionList = SqliteDataAccess.GetSubscriptionList();


                    //Iterate through each subscription to generate new stats
                    foreach (var Subscription in SubscriptionList)
                    {
                        SqliteDataAccess.SaveStats(StatModel.GenerateStats(Subscription));
                    }

                    //Navigates to the main page 


                    NavigationService.Navigate(new MainPage());


                }
            }
            //If the Ticker does not exist it will return an empty list
            else
            {
                StockTicker_Textbox.Foreground = new SolidColorBrush(Colors.Red);
            }
        }
    }

}
