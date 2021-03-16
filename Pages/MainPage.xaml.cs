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
using System.Net;
using StatFeed.Class;
using Newtonsoft.Json;
using System.Windows.Threading;
using System.IO;

namespace StatFeed.Pages
{
    /// <summary>
    /// Interaction logic for MainPage.xaml
    /// </summary>
    public partial class MainPage : Page
    {
        //Timer properties
        private int totalSeconds;
        private int totalSecondsConst;
        private DispatcherTimer Update_Timer;
        public static string CurrentBackgroundURL;
        public static string GlobalCurrentStat;

        //Lists
        public List<GameModel> SubscribedGamesList;

        public MainPage()
        {
            InitializeComponent();            
            OnLoad();
            Timer();
        }

        public void OnLoad()
        {
            //Loads in the Subscribed Games
            Games_combobox.ItemsSource = PopulateServiceComboBox();
            //Sets the index to the last selected game
            SetGamesComboboxIndex(SqliteDataAccess.GetGamesComboCheckpoint());           

        }

        public List<ComboBoxPair> PopulateServiceComboBox()
        {
            //runs method to create a list of subscriptions
            var SubscriptionList = SqliteDataAccess.GetSubscriptionList();

            

            List<ComboBoxPair> myPairs = new List<ComboBoxPair>();


            //iterate through each subscription
            foreach (var subscription in SubscriptionList)
            {
                int SubscriptionID = subscription.SubscriptionID;

                //If its a game based subscription
                if (subscription.ServiceTypeID == 1)
                {
                    var Game = SqliteDataAccess.SelectGame(subscription.ID);
                    string Name = Game.Name;

                    //creates a pair for easy viewing of the Service Combo box
                    myPairs.Add(new ComboBoxPair(SubscriptionID, Name));
                }
                if (subscription.ServiceTypeID == 2)
                {
                    var Finance = SqliteDataAccess.SelectFinance(subscription.ID);
                    string Name = Finance.Name;

                    //creates a pair for easy viewing of the Service Combo box
                    myPairs.Add(new ComboBoxPair(SubscriptionID, Name));
                }                     


                
                

            }
            return myPairs;
        }
        public void Games_combobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Games_combobox.SelectedItem != null)
            {
                //When Games combobox is changed then save this latest change
                ComboBoxPair GameComboBoxSelection = (ComboBoxPair)Games_combobox.SelectedItem;
                SubscribedGameModel CurrentSubscription = new SubscribedGameModel();
                CurrentSubscription = SqliteDataAccess.GetSubscription(GameComboBoxSelection.ID);
                SqliteDataAccess.SetGamesComboCheckpoint(CurrentSubscription);

                //Set UpdateTimer based on the service table on the database
                totalSecondsConst = SqliteDataAccess.GetServiceUpdateTimerDuration(CurrentSubscription.ServiceTypeID);               

                //Set Background
                GameModel CurrentGame = new GameModel();
                CurrentGame = SqliteDataAccess.SelectGame(CurrentSubscription.ID);

                //Checks to see if user has set a custom background or not
                if (CurrentSubscription.Custom_Background == "Default")
                {
                    //If the user has not set a custom background                    
                    BitmapImage BackgroundBitmap = new BitmapImage();
                    BackgroundBitmap.BeginInit();
                    BackgroundBitmap.UriSource = new Uri(CurrentGame.BackgroundURL, UriKind.RelativeOrAbsolute);                    
                    BackgroundBitmap.EndInit();
                    Background_Image.Source = BackgroundBitmap;

                    CurrentBackgroundURL = CurrentGame.BackgroundURL;
                }
                else
                {
                    //Check if file exists or not if so then carry on but if doesn't exist then return database value to default and set background to default                
                    if (File.Exists(CurrentSubscription.Custom_Background))
                    {
                        //If the user has set a custom background
                        BitmapImage BackgroundBitmap = new BitmapImage();
                        BackgroundBitmap.BeginInit();
                        BackgroundBitmap.UriSource = new Uri(CurrentSubscription.Custom_Background, UriKind.RelativeOrAbsolute);
                        BackgroundBitmap.EndInit();
                        Background_Image.Source = BackgroundBitmap;
                        
                        CurrentBackgroundURL = CurrentSubscription.Custom_Background;
                    }
                    else
                    {
                        //if the file path isn't valid
                        BitmapImage BackgroundBitmap = new BitmapImage();
                        BackgroundBitmap.BeginInit();
                        BackgroundBitmap.UriSource = new Uri(CurrentGame.BackgroundURL, UriKind.RelativeOrAbsolute);
                        BackgroundBitmap.EndInit();
                        Background_Image.Source = BackgroundBitmap;

                        CurrentBackgroundURL = CurrentGame.BackgroundURL;

                        //SQL to rewrite current subscription's Custom_Background to "Default"
                        SqliteDataAccess.SetToDefaultBackgroundSubscription(CurrentSubscription.SubscriptionID);
                    }                    
                }

                //Takes current SubscriptionID and finds UserName
                Account_Name.Text = CurrentSubscription.GetUserName();

                //Loads relevant stats of currentgame
                Stats_combobox.ItemsSource = SqliteDataAccess.GetStats(CurrentSubscription.SubscriptionID);
                //Sets index of stat combo box 
                SetStatsComboboxIndex(SqliteDataAccess.GetStatsComboCheckpoint(CurrentSubscription.SubscriptionID));                
            }
        }
        private void Stats_combobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Stats_combobox.SelectedItem != null)
            {
                StatModel CurrentStat = new StatModel();
                CurrentStat = (StatModel)Stats_combobox.SelectedItem;                
                string FormatStat = DisplayModel.FormatTo000000(CurrentStat.StatValue_1);

                //Displays the normal number and the formatted display version
                StatValue_label.Text = CurrentStat.StatValue_1;
                OLED_Display_Textbox.Text = FormatStat;

                //Save Stats Combobox Checkpoint
                SqliteDataAccess.SetStatsComboCheckpoint(CurrentStat.StatID);

                //Display stat on available COM port display                
                SendToDisplay(CurrentStat);
                            
            }
        }
        private void OLED_Display_Block_MouseDown(object sender, MouseButtonEventArgs e)
        {
            
            this.NavigationService.Navigate(new OLEDDisplayPage());
        }
        private void Account_Block_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.NavigationService.Navigate(new AddAccountsPage());
        }
        public void Timer()
        {
            totalSeconds = totalSecondsConst;

            Update_Timer = new DispatcherTimer();
            Update_Timer.Interval = new TimeSpan(0, 0, 1);
            Update_Timer.Tick += Update_Timer_Tick;
            Update_Timer.Start();
        }
        void Update_Timer_Tick(object sender, EventArgs e)
        {   
            if (totalSeconds > 0)
            {
                if (totalSeconds % 5 == 0)
                {
                    //Check every 5 seconds to see if the display is connected
                    CheckDisplay();
                }    
                               
                totalSeconds--;
                //If the seconds is smaller than 10 then add the format
                if (totalSeconds % 60 < 10)
                {
                    var formatSeconds = string.Format("0{0}", totalSeconds % 60);
                    NextUpdate_Label.Text = string.Format("{0}:{1}", totalSeconds / 60, formatSeconds);
                }
                else
                {
                    NextUpdate_Label.Text = string.Format("{0}:{1}", totalSeconds / 60, totalSeconds % 60);
                }
            }
            else
            {
                //stops timer
                Update_Timer.Stop();

                //try catch for if not connected to internet
                try
                {
                    //Execute update of current stat      
                    UpdateCurrentStats();
                }
                catch
                {
                    
                }


                totalSeconds = totalSecondsConst;
                Update_Timer.Start();
            }
        }        
        public void SendToDisplay(StatModel currentStat)
        {
            if (DisplayModel.SearchForLastDisplay())
            {
                //If there is a valid connected display found then send to display
                string CurrentPort = SqliteDataAccess.GetLastCOMPort();

                //Creates object of Display Command (Name, Command)
                DisplayCommandModel CurrentDisplayCommand = new DisplayCommandModel();
                CurrentDisplayCommand = SqliteDataAccess.GetCurrentDisplayCommand();
                DisplayModel.SendToPort(currentStat.StatName, currentStat.StatValue_1, currentStat.StatValue_2, currentStat.StatValue_3, CurrentPort, CurrentDisplayCommand.Command);


                //Set Bitmap Image
                BitmapImage DisplayIconBitmap = new BitmapImage();
                DisplayIconBitmap.BeginInit();
                DisplayIconBitmap.UriSource = new Uri(@"/Images/Icons/StatFeed_Display_Connected.png", UriKind.Relative);
                DisplayIconBitmap.DecodePixelHeight = 20;
                DisplayIconBitmap.DecodePixelWidth = 35;
                DisplayIconBitmap.EndInit();
                Display_Icon.Source = DisplayIconBitmap;

            }
            else
            {
                //No display has been found or one has been found but not previous saved one
                string[] FoundPorts = DisplayModel.FindAllPorts();

                if (FoundPorts[0] != "No Port")
                {
                    //This is if the COM port has been changed (Switching USB ports on computer), this attempts to switch it if the original port isn't there
                    //It chooses the top one from the list (Most likely going to be the display)
                    SqliteDataAccess.SetLastCOMPort(FoundPorts[0]);

                    //Set Bitmap Image
                    BitmapImage DisplayIconBitmap = new BitmapImage();
                    DisplayIconBitmap.BeginInit();
                    DisplayIconBitmap.UriSource = new Uri(@"/Images/Icons/StatFeed_Display_Connected.png", UriKind.Relative);
                    DisplayIconBitmap.DecodePixelHeight = 20;
                    DisplayIconBitmap.DecodePixelWidth = 35;
                    DisplayIconBitmap.EndInit();
                    Display_Icon.Source = DisplayIconBitmap;

                    string CurrentPort = SqliteDataAccess.GetLastCOMPort();

                    //Creates object of Display Command (Name, Command)
                    DisplayCommandModel CurrentDisplayCommand = new DisplayCommandModel();
                    CurrentDisplayCommand = SqliteDataAccess.GetCurrentDisplayCommand();
                    DisplayModel.SendToPort(currentStat.StatName, currentStat.StatValue_1, currentStat.StatValue_2, currentStat.StatValue_3, CurrentPort, CurrentDisplayCommand.Command);
                }
                else
                {
                    //This is if there are definitely no ports
                    SqliteDataAccess.SetLastCOMPort("No Port");
                    //Set Bitmap Image
                    BitmapImage DisplayIconBitmap = new BitmapImage();
                    DisplayIconBitmap.BeginInit();
                    DisplayIconBitmap.UriSource = new Uri(@"/Images/Icons/StatFeed_Display_Not_Connecting.png", UriKind.Relative);
                    DisplayIconBitmap.DecodePixelHeight = 20;
                    DisplayIconBitmap.DecodePixelWidth = 35;
                    DisplayIconBitmap.EndInit();
                    Display_Icon.Source = DisplayIconBitmap;
                }
            }
        }
        public void CheckDisplay()
        {
            if (DisplayModel.SearchForLastDisplay())
            {
                //This means that a display is successfully connected to the program
                //Set Bitmap Image
                BitmapImage DisplayIconBitmap = new BitmapImage();
                DisplayIconBitmap.BeginInit();
                DisplayIconBitmap.UriSource = new Uri(@"/Images/Icons/StatFeed_Display_Connected.png", UriKind.Relative);
                DisplayIconBitmap.DecodePixelHeight = 20;
                DisplayIconBitmap.DecodePixelWidth = 35;
                DisplayIconBitmap.EndInit();
                Display_Icon.Source = DisplayIconBitmap;
            }
            else
            {
                string[] FoundPorts = DisplayModel.FindAllPorts();
                if (FoundPorts[0] != "No Port")
                {
                    //This finds another port and connects to that
                    SqliteDataAccess.SetLastCOMPort(FoundPorts[0]);

                    //Set Bitmap Image
                    BitmapImage DisplayIconBitmap = new BitmapImage();
                    DisplayIconBitmap.BeginInit();
                    DisplayIconBitmap.UriSource = new Uri(@"/Images/Icons/StatFeed_Display_Connected.png", UriKind.Relative);
                    DisplayIconBitmap.DecodePixelHeight = 20;
                    DisplayIconBitmap.DecodePixelWidth = 35;
                    DisplayIconBitmap.EndInit();
                    Display_Icon.Source = DisplayIconBitmap;
                }
                else
                {
                    //This is if there are definitely no ports
                    SqliteDataAccess.SetLastCOMPort("No Port");

                    //Set Bitmap Image
                    BitmapImage DisplayIconBitmap = new BitmapImage();
                    DisplayIconBitmap.BeginInit();
                    DisplayIconBitmap.UriSource = new Uri(@"/Images/Icons/StatFeed_Display_Not_Connecting.png", UriKind.Relative);
                    DisplayIconBitmap.DecodePixelHeight = 20;
                    DisplayIconBitmap.DecodePixelWidth = 35;
                    DisplayIconBitmap.EndInit();
                    Display_Icon.Source = DisplayIconBitmap;
                }
            }
        }
        public void SetGamesComboboxIndex(int subscriptionID)
        {
            for (int i = 0; i < Games_combobox.Items.Count; i++)
            {
                if (((ComboBoxPair)Games_combobox.Items[i]).ID == subscriptionID)
                {
                    Games_combobox.SelectedIndex = i;
                    break;
                }
            }
        }
        public void SetStatsComboboxIndex(int statID)
        {
            for (int i = 0; i < Stats_combobox.Items.Count; i++)
            {
                if (((StatModel)Stats_combobox.Items[i]).StatID == statID)
                {
                    Stats_combobox.SelectedIndex = i;
                    break;
                }
            }
        }
        public void UpdateCurrentStats()
        {
            //Find current subscription that needs to be updated
            ComboBoxPair GameComboBoxSelection = (ComboBoxPair)Games_combobox.SelectedItem;
            SubscribedGameModel CurrentSubscription = new SubscribedGameModel();
            CurrentSubscription = SqliteDataAccess.GetSubscription(GameComboBoxSelection.ID);

            //Generate new stats and save them to the database
            SqliteDataAccess.SaveStats(StatModel.GenerateStats(CurrentSubscription));

            //Relink the database to the combo box
            Games_combobox.ItemsSource = PopulateServiceComboBox();

            //Re position the games combobox to that last selected game
            SetGamesComboboxIndex(SqliteDataAccess.GetGamesComboCheckpoint());
        }        
    }
}
