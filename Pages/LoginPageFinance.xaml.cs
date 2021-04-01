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
using Cursors = System.Windows.Forms.Cursors;

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
        public void CheckandPopulateAPITextbox(int ServiceTypeID, int ID)
        {
            //This method will add the API key from a previous use of the API key 
            //It takes the ServiceTypeID runs sql search for subscription.servicetypeID and returns the APIkey if there is one
            string APIKey = SqliteDataAccess.GetPreviousAPIKey(ServiceTypeID, ID);
            APIKey_Textbox.Text = APIKey;
        }
        public void CheckandPopulateAPISecretTextbox(int ServiceTypeID, int ID)
        {
            //This method will add the API key from a previous use of the API key 
            //It takes the ServiceTypeID runs sql search for subscription.servicetypeID and returns the APIkey if there is one
            string APISecret = SqliteDataAccess.GetPreviousAPISecret(ServiceTypeID, ID);
            APISecret_Textbox.Text = APISecret;

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
            APIKey_Textbox.Opacity = 0.6;
            APIKey_Textbox.IsReadOnly = true;
            APIKey_Textbox.Focusable = false;            

            APISecret_Textbox.Opacity = 0.6;
            APISecret_Textbox.IsReadOnly = true;
            APISecret_Textbox.Focusable = false;

            //checks the Finance table to see if the keyis required
            if (currentFinance.KeyRequired)
            {
                APIKey_Textbox.Opacity = 1;
                APIKey_Textbox.IsReadOnly = false;
                APIKey_Textbox.Focusable = true;

                //Run check for populating API Key 
                CheckandPopulateAPITextbox(currentFinance.ServiceTypeID, currentFinance.ID);

                if (currentFinance.SecretRequired)
                {
                    APISecret_Textbox.Opacity = 1;
                    APISecret_Textbox.IsReadOnly = false;
                    APISecret_Textbox.Focusable = true;

                    CheckandPopulateAPISecretTextbox(currentFinance.ServiceTypeID, currentFinance.ID);
                }
            }

            //Populate Ticker Combo box if necessary
            //Auto fills Ticker based on Market Combobox            
            StockTicker_Combobox.BeginInit();
            StockTicker_Combobox.ItemsSource = FinanceModel.CryptocurrencyAutoFill(currentFinance.ID);
            StockTicker_Combobox.EndInit();
            StockTicker_Combobox.SelectedIndex = 0;

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
            StockTicker_Combobox.Foreground = new SolidColorBrush(Colors.White);
        }
        private void Login_Button_Click(object sender, RoutedEventArgs e)
        {
            //Create object of current finance ticker to check if it works
            FinanceModel CheckCurrentFinance = (FinanceModel)Market_Combobox.SelectedItem;
            string currentTicker = (string)StockTicker_Combobox.SelectedItem;
            string APIKey = APIKey_Textbox.Text;
            string APISecret = APISecret_Textbox.Text;

            //Changes default of chosen service to 4 so it shows a coin stack
            chosen_Service = 4;

            string customBackground = Background_Upload_Textbox.Text;

            SubscribedGameModel TempSubscription = new SubscribedGameModel(0, 2, CheckCurrentFinance.ID, currentTicker, 4, APIKey, APISecret,customBackground);

            List<StatModel> TempStats = new List<StatModel>(StatModel.GenerateStats(TempSubscription));

            bool check = !TempStats.Any();

            //If the Ticker is legitimate it will return stats from this query
            if (!check)
            {
                bool duplicates = SubscribedGameModel.CheckDuplicates(currentTicker,2, CheckCurrentFinance.ID, chosen_Service);

                if (duplicates)
                {
                    StockTicker_Combobox.Foreground = new SolidColorBrush(Colors.Orange);
                }
                else
                {
                    StockTicker_Combobox.Foreground = new SolidColorBrush(Colors.Green);

                    //Saves the subscription information
                    SqliteDataAccess.SaveSubscribedGame(ServiceTypeID, CheckCurrentFinance.ID, currentTicker, chosen_Service, APIKey, APISecret, customBackground);

                    //Get full list of subscriptions
                    List<SubscribedGameModel> SubscriptionList = SqliteDataAccess.GetSubscriptionList();


                    //Iterate through each subscription to generate new stats
                    foreach (var Subscription in SubscriptionList)
                    {
                        SqliteDataAccess.SaveStats(StatModel.GenerateStats(Subscription));
                    }

                    //This sets the stat (most recent) to the last selected
                    SubscribedGameModel LatestSubscription = new SubscribedGameModel();
                    StatModel TopStat = new StatModel();

                    LatestSubscription = SqliteDataAccess.GetLatestSubscription();
                    TopStat = SqliteDataAccess.GetTopStat(LatestSubscription.SubscriptionID);
                    SqliteDataAccess.SetLastSavedStat(TopStat.StatID);


                    //Navigates to the main page 
                    NavigationService.Navigate(new MainPage());
                }
            }
            //If the Ticker does not exist it will return an empty list
            else
            {
                StockTicker_Combobox.Foreground = new SolidColorBrush(Colors.Red);
            }
        }
    }

}
