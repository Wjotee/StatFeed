using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Forms;
using StatFeed.Class;

namespace StatFeed.Pages
{
    /// <summary>
    /// Interaction logic for LoginPageGame.xaml
    /// </summary>
    public partial class LoginPageGame : Page
    {
        public int chosen_Service;

        //Defines login page state as a number referring to the service type (Game or Finance)
        int ServiceTypeID = 1;
        string APIKey = "";
        string APISecret = "";
        public LoginPageGame()
        {
            InitializeComponent();
            OnLoad();
        }

        public void OnLoad()
        {
            //Populate dropdown with available platforms
            PopulateLoginPlatformCombobox();

            //Sets game tab as default
            
            PC_Button_Body.Opacity = 1;
            chosen_Service = 1;

            Xbox_Button_Body.Opacity = 0.5;
            PSN_Button_Body.Opacity = 0.5;

            Back_Button.Visibility = Visibility.Collapsed;

            //Checks if there are subscriptions 
            //If its true then add a back button as it would have only been possible to get to this page through the Add Accounts page
            bool result = SqliteDataAccess.FirstTime();

            if (!result)
            {
                Back_Button.Visibility = Visibility.Visible;
            }
        }
        public void PopulateLoginPlatformCombobox()
        {

            AvailableGames_Combobox.ItemsSource = SqliteDataAccess.GetAvailableGames();

            AvailableGames_Combobox.SelectedIndex = 0;
        }
        private void PlatformOptions_Combobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            GameModel currentGame = (GameModel)AvailableGames_Combobox.SelectedItem;            

            //Make all buttons collapsed 
            PC_Button_Body.Visibility = Visibility.Collapsed;
            Xbox_Button_Body.Visibility = Visibility.Collapsed;
            PSN_Button_Body.Visibility = Visibility.Collapsed;


            //PC bool check
            if (currentGame.Platform_PC != "0")
            {
                PC_Button_Body.Visibility = Visibility.Visible;
                PC_Button_Body.Opacity = 1;
                Xbox_Button_Body.Opacity = 0.5;
                PSN_Button_Body.Opacity = 0.5;
                chosen_Service = 1;

            }

            //xbox bool check
            if (currentGame.Platform_Xbox != "0")
            {
                Xbox_Button_Body.Visibility = Visibility.Visible;
            }

            //psn bool check
            if (currentGame.Platform_PSN != "0")
            {
                PSN_Button_Body.Visibility = Visibility.Visible;
            }

            //Background Image
            BitmapImage BackgroundBitmap = new BitmapImage();
            BackgroundBitmap.BeginInit();
            BackgroundBitmap.UriSource = new Uri(currentGame.BackgroundURL, UriKind.RelativeOrAbsolute);
            BackgroundBitmap.EndInit();
            Background_Image.Source = BackgroundBitmap;



        }
        private void PC_Button_Click(object sender, RoutedEventArgs e)
        {
            PC_Button_Body.Opacity = 1;
            chosen_Service = 1;

            Xbox_Button_Body.Opacity = 0.5;
            PSN_Button_Body.Opacity = 0.5;
        }
        private void Xbox_Button_Click(object sender, RoutedEventArgs e)
        {
            Xbox_Button_Body.Opacity = 1;
            chosen_Service = 2;

            PSN_Button_Body.Opacity = 0.5;
            PC_Button_Body.Opacity = 0.5;
        }
        private void PSN_Button_Click(object sender, RoutedEventArgs e)
        {
            PSN_Button_Body.Opacity = 1;
            chosen_Service = 3;

            Xbox_Button_Body.Opacity = 0.5;
            PC_Button_Body.Opacity = 0.5;
        }
       
        private void Grid_GotFocus(object sender, RoutedEventArgs e)
        {
            Username_Textbox.Foreground = new SolidColorBrush(Colors.White);
        }
        
        private void Background_Upload_Button_Click(object sender, RoutedEventArgs e)
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
        private void Login_Button_Click(object sender, RoutedEventArgs e)
        {
            //Creates object of current platform to check if it works
            GameModel CheckCurrentGame = (GameModel)AvailableGames_Combobox.SelectedItem;
            string currentUsername = Username_Textbox.Text;

            string customBackground = Background_Upload_Textbox.Text;

            SubscribedGameModel TempSubscription = new SubscribedGameModel(0, 1, CheckCurrentGame.ID, currentUsername, chosen_Service, APIKey, APISecret, 0, customBackground);

            List<StatModel> TempStats = new List<StatModel>(StatModel.GenerateStats(TempSubscription));

            bool check = !TempStats.Any();

            //If the UserName is legitimate it will return stats from this query
            if (!check)
            {
                //Also check if the UserName, Game and Chosen_Service exists already on database
                bool duplicates = SubscribedGameModel.CheckDuplicates(currentUsername, 1, CheckCurrentGame.ID, chosen_Service);

                if (duplicates)
                {
                    Username_Textbox.Foreground = new SolidColorBrush(Colors.Orange);
                }
                else
                {
                    Username_Textbox.Foreground = new SolidColorBrush(Colors.Green);

                    //Saves the subscription information
                    SqliteDataAccess.SaveSubscribedGame(ServiceTypeID, CheckCurrentGame.ID, currentUsername, chosen_Service, APIKey, APISecret, 0, customBackground);

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
            //If the UserName does not exist it will return an empty list
            else
            {
                Username_Textbox.Foreground = new SolidColorBrush(Colors.Red);
            }
        }

        private void Back_Button_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService.GoBack();
        }

        private void FinanceTab_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.NavigationService.Navigate(new LoginPageFinance());
        }
    }
}
