using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using Newtonsoft.Json;

namespace StatFeed.Class
{    
    public class SubscribedGameModel
    {

        //Properties
        public int SubscriptionID { get; set; }
        public int GameID { get; set; }
        public string UserName { get; set; }
        //either PC = 1, Xbox = 2, PSN = 3
        public int Chosen_Service { get; set; }

        public int Last_Selected { get; set; }

        public string Custom_Background { get; set; }



        //Instantiating a new game (Without any variables assigned)
        public SubscribedGameModel()
        {
        }

        //Instantiating a new subscribed platform
        public SubscribedGameModel(int subscriptionID, int gameID, string userName, int chosen_Service, int last_Selected, string custom_Background)
        {
            SubscriptionID = subscriptionID;
            GameID = gameID;
            UserName = userName;
            Chosen_Service = chosen_Service;
            Last_Selected = last_Selected;
            Custom_Background = custom_Background;
        }

        //Methods
        public override string ToString()
        {
            return UserName;
        }
        public int GetGameID()
        {
            return this.GameID;
        }
        public string GetUserName()
        {
            return this.UserName;
        }
        public int GetChosen_Service()
        {
            return this.Chosen_Service;
        }

        public int GetSubscriptionID()
        {
            return this.SubscriptionID;
        }

        public string GetCustomBackground()
        {
            return this.Custom_Background;
        }

        public static bool CheckDuplicates(string userName, int gameID, int chosen_Service)
        {
            //Create a list of the current stored subscriptions
            List<SubscribedGameModel> CurrentSubscriptions = new List<SubscribedGameModel>(SqliteDataAccess.GetSubscriptionList());

            //Check to see if there are subscriptions, if not then there is definitely no duplicates
            if (CurrentSubscriptions.Count > 0)
            {
                foreach (var Subscription in CurrentSubscriptions)
                {
                    //If the userName, gameID and chosen_Service match the passed parameters then there are duplicates
                    if (Subscription.UserName == userName & Subscription.GameID == gameID & Subscription.Chosen_Service == chosen_Service)
                    {
                        return true;
                    }
                    //This allows the user to have access to multiple accounts on a game but with different UserNames or Services
                    else
                    {
                        return false;
                    }
                }
            }
            return false;            
        }
    }
}
