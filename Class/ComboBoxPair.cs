using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatFeed.Class
{
    public class ComboBoxPair
    {
        public string GameName { get; set; }
        public int SubscriptionID { get; set; }

        public ComboBoxPair (int subscriptionID, string gameName)
        {
            SubscriptionID = subscriptionID;
            GameName = gameName;
        }

        public override string ToString()
        {
            return GameName;
        }

        public List<ComboBoxPair> MyPairs
        {
            get 
            { 
                return MyPairs;
            }
        }
    }
}
