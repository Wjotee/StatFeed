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
        public int ServiceTypeID { get; set; }
        public int ID { get; set; }
        public string UserName { get; set; }
        //either PC = 1, Xbox = 2, PSN = 3
        public int Chosen_Service { get; set; }
        public string APIKey { get; set; }
        public string APISecret { get; set; }        
        public string Custom_Background { get; set; }



        //Instantiating a new game (Without any variables assigned)
        public SubscribedGameModel()
        {
        }

        //Instantiating a new subscribed platform
        public SubscribedGameModel(int subscriptionID, int serviceTypeID, int iD, string userName, int chosen_Service, string aPIKey, string aPISecret, string custom_Background)
        {
            SubscriptionID = subscriptionID;
            ServiceTypeID = serviceTypeID;
            ID = iD;
            UserName = userName;
            Chosen_Service = chosen_Service;
            APIKey = aPIKey;
            APISecret = aPISecret;            
            Custom_Background = custom_Background;
        }

        //Methods
        public override string ToString()
        {
            return UserName;
        }
        public int GetID()
        {
            return this.ID;
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

        public static bool CheckDuplicates(string userName, int ServiceTypeID, int ID, int chosen_Service)
        {
            //Create a list of the current stored subscriptions
            List<SubscribedGameModel> CurrentSubscriptions = new List<SubscribedGameModel>(SqliteDataAccess.GetSubscriptionList());

            //Check to see if there are subscriptions, if not then there is definitely no duplicates
            if (CurrentSubscriptions.Count > 0)
            {
                foreach (var Subscription in CurrentSubscriptions)
                {
                    //If the userName, gameID, chosen_Service and ServiceTypeID match the passed parameters then there are duplicates
                    if (Subscription.UserName == userName & Subscription.ID == ID & Subscription.Chosen_Service == chosen_Service & Subscription.ServiceTypeID == ServiceTypeID)
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