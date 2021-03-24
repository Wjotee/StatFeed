using StatFeed.Class;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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


namespace StatFeed.Pages
{
    /// <summary>
    /// Interaction logic for AddAccountsPage.xaml
    /// </summary>
    public partial class AddAccountsPage : Page
    {
        private ObservableCollection<AddAccountsSubscriptions> oc;
        public AddAccountsPage()
        {
            InitializeComponent();
            OnLoad();
        }

        public void OnLoad()
        {
            try
            {
                //Set background
                BitmapImage BackgroundBitmap = new BitmapImage();
                BackgroundBitmap.BeginInit();
                BackgroundBitmap.UriSource = new Uri(MainPage.CurrentBackgroundURL, UriKind.RelativeOrAbsolute);
                BackgroundBitmap.EndInit();
                Background_Image.Source = BackgroundBitmap;
            }
            catch
            {

            }
            

            //Populate Accounts Listbox
            PopulateAccountsListBox();
        }

        public void PopulateAccountsListBox()
        {
            //Create list of AddAccountsSubscriptions
            List<AddAccountsSubscriptions> AddAccountsSubscriptionList = new List<AddAccountsSubscriptions>();

            //Find all subscriptions 
            List<SubscribedGameModel> SubscriptionList = new List<SubscribedGameModel>();
            SubscriptionList = SqliteDataAccess.GetSubscriptionList();

            //Populate list by iterating through all the subscriptions the user has and finding the relevant details 
            foreach (var Subscription in SubscriptionList)
            {
                //If the Service is a game
                if (Subscription.ServiceTypeID == 1)
                {
                    //Find game name of current subscription
                    GameModel CurrentGame = new GameModel();
                    CurrentGame = SqliteDataAccess.SelectGame(Subscription.ID);

                    //Create new object of AddAccountsSubscription
                    AddAccountsSubscriptions AddAccountsSubscription = new AddAccountsSubscriptions(Subscription.SubscriptionID, Subscription.UserName, CurrentGame.Name, Subscription.Chosen_Service);

                    //Add it to a list
                    AddAccountsSubscriptionList.Add(AddAccountsSubscription);
                }
                if (Subscription.ServiceTypeID == 2)
                {
                    //Find finance name of current subscription
                    FinanceModel CurrentFinance = new FinanceModel();
                    CurrentFinance = SqliteDataAccess.SelectFinance(Subscription.ID);

                    //Create new object of AddAccountSubscription
                    AddAccountsSubscriptions AddAccountsSubscription = new AddAccountsSubscriptions(Subscription.SubscriptionID, Subscription.UserName, CurrentFinance.Name, Subscription.Chosen_Service);
                    
                    //Add it to a list
                    AddAccountsSubscriptionList.Add(AddAccountsSubscription);
                }

                
            }

            oc = new ObservableCollection<AddAccountsSubscriptions>(AddAccountsSubscriptionList);
            //Read through ObserverableCollection and populate subscribed accounts page
            SubscribedPlatformsListBox.ItemsSource = oc;



        }
        private void AddAccountsPage_Background_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.NavigationService.Navigate(new MainPage());
        }

        private void Delete_Button_Click(object sender, RoutedEventArgs e)
        {
            //Clicking the delete button will remove the SubscribedPlatform object from the ObservableCollection
            var button = sender as Button;
            if (button != null)
            {
                var AddAccountsSubscription = button.DataContext as AddAccountsSubscriptions;

                if (SubscribedPlatformsListBox.Items.Count == 1)
                {
                    //This removes it from the visual list on screen
                    ((ObservableCollection<AddAccountsSubscriptions>)SubscribedPlatformsListBox.ItemsSource).Remove(AddAccountsSubscription);

                    //This deletes the subscription and the stats associated with it 
                    SqliteDataAccess.DeleteSubscribedGame(AddAccountsSubscription.SubscriptionID);
                    SqliteDataAccess.DeleteStats(AddAccountsSubscription.SubscriptionID);

                    this.NavigationService.Navigate(new LoginPageGame());
                }
                else
                {
                    //This removes it from the visual list on screen
                    ((ObservableCollection<AddAccountsSubscriptions>)SubscribedPlatformsListBox.ItemsSource).Remove(AddAccountsSubscription);

                    //This deletes the subscription and the stats associated with it 
                    SqliteDataAccess.DeleteSubscribedGame(AddAccountsSubscription.SubscriptionID);
                    SqliteDataAccess.DeleteStats(AddAccountsSubscription.SubscriptionID);
                }
            }
            else
            {
                return;
            }
        }

        private void Add_Account_Button_Click(object sender, RoutedEventArgs e)
        {
            //Navigate to LoginPage for adding another account
            this.NavigationService.Navigate(new LoginPageGame());
        }
    }
}
