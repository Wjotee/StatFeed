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
        

        //Lists
        public List<GameModel> SubscribedGamesList;

        public MainPage()
        {
            InitializeComponent();            
            OnLoad();
            Timer();

            //Check if this is the latest version and show update button if so
            if (Update.CheckForUpdate())
            {
                Update_Program_Button.Visibility = Visibility.Visible;
            }
        }

        public void OnLoad()
        {
            //populate the stat textbox
            StatModel LastSavedStat = new StatModel();
            LastSavedStat = SqliteDataAccess.GetLastSavedStat();
            StatValue_label.Text = LastSavedStat.StatValue_1;

            //populate the stat combo box
            Stats_combobox.ItemsSource = PopulateStatComboBox(LastSavedStat.SubscriptionID);
            SetStatsComboboxIndex(LastSavedStat);

            Service_combobox.ItemsSource = PopulateServiceComboBox();
            SetServiceComboboxIndex(LastSavedStat);            
        }


        public List<ComboboxStackedSubscriptions> PopulateServiceComboBox()
        {
            //runs method to create a list of subscriptions
            var AllUserSubscriptions = SqliteDataAccess.GetSubscriptionList();

            

            List<ComboboxStackedSubscriptions> StackableSubscriptions = new List<ComboboxStackedSubscriptions>();
            

            foreach (var Subscription in AllUserSubscriptions)
            {
                //This is the list for the subscriptions that have the same ServiceTypeID and ID
                List<int> StackableSubscriptionIDs = new List<int>();

                //Game
                if (Subscription.ServiceTypeID == 1)
                {
                    //if the list already has an entry
                    if (StackableSubscriptions.Any())
                    {
                        bool foundinstance = false;

                        //Foreach of the subscriptions in the current list, compare the ServiceTypeID and The ID to see if the next subscription can be added to an existing entry's subscriptionlist
                        foreach (var item in StackableSubscriptions.ToList())
                        {
                            if (item.ServiceTypeID == Subscription.ServiceTypeID & item.ID == Subscription.ID)
                            {
                                item.SubscriptionIDs.Add(Subscription.SubscriptionID);
                                foundinstance = true;
                                break;
                            }
                                         
                        }
                        if (!foundinstance)
                        {
                            //Clear subscriptionIDs list and add to it
                            StackableSubscriptionIDs.Clear();
                            StackableSubscriptionIDs.Add(Subscription.SubscriptionID);

                            //Then add to the ComboBoxSubscriptionListPairs
                            var Service = SqliteDataAccess.SelectGame(Subscription.ID);
                            StackableSubscriptions.Add(new ComboboxStackedSubscriptions(StackableSubscriptionIDs, Service.Name, Subscription.ServiceTypeID, Subscription.ID));
                        }
                    }
                    else
                    {
                        //Clear subscriptionIDs list and add to it
                        StackableSubscriptionIDs.Clear();
                        StackableSubscriptionIDs.Add(Subscription.SubscriptionID);

                        //Then add to the ComboBoxSubscriptionListPairs
                        var Service = SqliteDataAccess.SelectGame(Subscription.ID);
                        StackableSubscriptions.Add(new ComboboxStackedSubscriptions(StackableSubscriptionIDs, Service.Name, Subscription.ServiceTypeID, Subscription.ID));
                    }
                }

                //Finance
                if (Subscription.ServiceTypeID == 2)
                {
                    //if the list already has an entry
                    if (StackableSubscriptions.Any())
                    {
                        bool foundinstance = false;

                        //Foreach of the subscriptions in the current list, compare the ServiceTypeID and The ID to see if the next subscription can be added to an existing entry's subscriptionlist
                        foreach (var item in StackableSubscriptions.ToList())
                        {
                            if (item.ServiceTypeID == Subscription.ServiceTypeID & item.ID == Subscription.ID)
                            {
                                item.SubscriptionIDs.Add(Subscription.SubscriptionID);
                                foundinstance = true;
                                break;
                            }
                                        
                        }
                        if (!foundinstance)
                        {
                            //Clear subscriptionIDs list and add to it
                            StackableSubscriptionIDs.Clear();
                            StackableSubscriptionIDs.Add(Subscription.SubscriptionID);

                            //Then add to the ComboBoxSubscriptionListPairs
                            var Service = SqliteDataAccess.SelectFinance(Subscription.ID);
                            StackableSubscriptions.Add(new ComboboxStackedSubscriptions(StackableSubscriptionIDs, Service.Name, Subscription.ServiceTypeID, Subscription.ID));
                        }
                    }
                    else
                    {
                        //Clear subscriptionIDs list and add to it
                        StackableSubscriptionIDs.Clear();
                        StackableSubscriptionIDs.Add(Subscription.SubscriptionID);

                        //Then add to the ComboBoxSubscriptionListPairs
                        var Service = SqliteDataAccess.SelectFinance(Subscription.ID);
                        StackableSubscriptions.Add(new ComboboxStackedSubscriptions(StackableSubscriptionIDs, Service.Name, Subscription.ServiceTypeID, Subscription.ID));
                    }
                }
            }
            return StackableSubscriptions;
        }
        public List<StatModel> PopulateStatComboBox(int subscriptionID)
        {
            List<StatModel> Stats = new List<StatModel>();
            Stats.Clear();
            Stats = SqliteDataAccess.GetStats(subscriptionID);
            return Stats;
        }
        public void Service_combobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Service_combobox.SelectedItem != null)
            {

                bool InServiceCombobox = false;

                //check to see if this selection change is the intial itemssource input or it's an actual change from the user
                //do this by checking to see if the subscriptionID of the Stat stored in UserSettings is the same as the one in the 
                //combo box selection

                ComboboxStackedSubscriptions ServiceComboBoxSelection = (ComboboxStackedSubscriptions)Service_combobox.SelectedItem;
                StatModel LastStat = new StatModel();
                LastStat = SqliteDataAccess.GetLastSavedStat();

                foreach (var subscriptionID in ServiceComboBoxSelection.SubscriptionIDs)
                {
                    if (subscriptionID == LastStat.SubscriptionID)
                    {
                        //This means that the Last_Selected is being shown
                        InServiceCombobox = true;
                    }
                }

                List<StatModel> StackableStats = new List<StatModel>();

                //Loads relevant stats of current service
                for (int i = 0; i < ServiceComboBoxSelection.SubscriptionIDs.Count; i++)
                {
                    List<StatModel> Stackable = new List<StatModel>();
                    Stackable = SqliteDataAccess.GetStats(ServiceComboBoxSelection.SubscriptionIDs[i]);

                    foreach (var item in Stackable)
                    {
                        StackableStats.Add(item);
                    }
                }
                Stats_combobox.ItemsSource = StackableStats;

                if (InServiceCombobox)
                {
                    //find and set index to the LastSavedStat
                    SetStatsComboboxIndex(SqliteDataAccess.GetLastSavedStat());                    
                }
                else
                {                    
                    //save LastStat as first subscription in list ([0])
                    StatModel NewStat = new StatModel();
                    NewStat = SqliteDataAccess.GetTopStat(ServiceComboBoxSelection.SubscriptionIDs[0]);
                    SqliteDataAccess.SetLastSavedStat(NewStat.StatID);

                    //Set index to the LastStat
                    SetStatsComboboxIndex(SqliteDataAccess.GetLastSavedStat());                    
                }        
            }
        }
        private void Stats_combobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Stats_combobox.SelectedItem != null)
            {
                StatModel CurrentStat = new StatModel();
                CurrentStat = (StatModel)Stats_combobox.SelectedItem;

                SubscribedGameModel CurrentSubscription = new SubscribedGameModel();
                CurrentSubscription = SqliteDataAccess.GetSubscription(CurrentStat.SubscriptionID);


                //Displays the normal number and the formatted display version
                StatValue_label.Text = CurrentStat.StatValue_1;

                int CurrentDisplayCommandID = SqliteDataAccess.GetCurrentDisplayCommandID();
                if (CurrentDisplayCommandID == 1)
                {
                    string FormatStat = DisplayModel.FormatTo000000(CurrentStat.StatValue_1);
                    OLED_Display_Textbox.Text = FormatStat;
                }
                if (CurrentDisplayCommandID == 2)
                {
                    OLED_Display_Textbox.Text = CurrentStat.StatValue_1;
                }

                //Set UpdateTimer based on the service table on the database
                totalSecondsConst = SqliteDataAccess.GetServiceUpdateTimerDuration(CurrentSubscription.ServiceTypeID);                

                //Takes current SubscriptionID and finds UserName
                Account_Name.Text = CurrentSubscription.GetUserName();

                #region Change Background 
                //Game
                if (CurrentSubscription.ServiceTypeID == 1)
                {
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
                }
                if (CurrentSubscription.ServiceTypeID == 2)
                {
                    FinanceModel CurrentFinance = new FinanceModel();
                    CurrentFinance = SqliteDataAccess.SelectFinance(CurrentSubscription.ID);

                    //Checks to see if user has set a custom background or not
                    if (CurrentSubscription.Custom_Background == "Default")
                    {
                        //If the user has not set a custom background                    
                        BitmapImage BackgroundBitmap = new BitmapImage();
                        BackgroundBitmap.BeginInit();
                        BackgroundBitmap.UriSource = new Uri(CurrentFinance.BackgroundURL, UriKind.RelativeOrAbsolute);
                        BackgroundBitmap.EndInit();
                        Background_Image.Source = BackgroundBitmap;

                        CurrentBackgroundURL = CurrentFinance.BackgroundURL;
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
                            BackgroundBitmap.UriSource = new Uri(CurrentFinance.BackgroundURL, UriKind.RelativeOrAbsolute);
                            BackgroundBitmap.EndInit();
                            Background_Image.Source = BackgroundBitmap;

                            CurrentBackgroundURL = CurrentFinance.BackgroundURL;

                            //SQL to rewrite current subscription's Custom_Background to "Default"
                            SqliteDataAccess.SetToDefaultBackgroundSubscription(CurrentSubscription.SubscriptionID);
                        }
                    }
                }
                #endregion

                //Save Stats Combobox Checkpoint
                SqliteDataAccess.SetLastSavedStat(CurrentStat.StatID);

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
        public void SetServiceComboboxIndex(StatModel LastStat)
        {
            for (int i = 0; i < Service_combobox.Items.Count; i++)
            {
                for (int s = 0; s < ((ComboboxStackedSubscriptions)Service_combobox.Items[i]).SubscriptionIDs.Count; s++)
                {
                    if (((ComboboxStackedSubscriptions)Service_combobox.Items[i]).SubscriptionIDs[s] == LastStat.SubscriptionID)
                    {
                        Service_combobox.SelectedIndex = i;
                        break;
                    }                   
                }                    
            }
        }
        public void SetStatsComboboxIndex(StatModel LastStat)
        {
            for (int i = 0; i < Stats_combobox.Items.Count; i++)
            {
                if (((StatModel)Stats_combobox.Items[i]).StatID == LastStat.StatID)
                {
                    Stats_combobox.SelectedIndex = i;
                    break;
                }
            }
        }
        public void UpdateCurrentStats()
        {
            //Find current subscription that needs to be updated
            StatModel CurrentStat = new StatModel();
            CurrentStat = (StatModel)Stats_combobox.SelectedItem;

            SubscribedGameModel CurrentSubscription = new SubscribedGameModel();
            CurrentSubscription = SqliteDataAccess.GetSubscription(CurrentStat.SubscriptionID);


            //Generate new stats and save them to the database
            SqliteDataAccess.SaveStats(StatModel.GenerateStats(CurrentSubscription));

            //Relink the database to the combo box
            Service_combobox.ItemsSource = PopulateServiceComboBox();

            //Re position the games combobox to that last selected game
            SetServiceComboboxIndex(SqliteDataAccess.GetLastSavedStat());
        }
        private void Update_Program_Button_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/Wjotee/StatFeed/releases");
        }
    }
}
