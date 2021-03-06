using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatFeed.Class
{
    class AddAccountsSubscriptions
    {

        //This is created to view it on the AddAccounts page 

        //Properties
        public string GameName { get; set; }
        public int SubscriptionID { get; set; }
        public string UserName { get; set; }
        public int Chosen_Service { get; set; }

        public AddAccountsSubscriptions (int subscriptionID, string userName, string gameName, int chosen_Service)
        {
            SubscriptionID = subscriptionID;
            UserName = userName;
            GameName = gameName;
            Chosen_Service = chosen_Service;
        }

        public List<AddAccountsSubscriptions> MySubscriptions
        {
            get
            {
                return MySubscriptions;
            }
        }
    }
}
