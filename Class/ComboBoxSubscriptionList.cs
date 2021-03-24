using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatFeed.Class
{
    public class ComboboxStackedSubscriptions
    {
        //Properties
        public string Name { get; set; }
        public List<int> SubscriptionIDs { get; set; }
        public int ServiceTypeID { get; set; }
        public int ID { get; set; }

        //Methods
        public ComboboxStackedSubscriptions(List<int> subscriptionIDs, string name, int serviceTypeID, int iD)
        {
            SubscriptionIDs = subscriptionIDs;
            Name = name;
            ServiceTypeID = serviceTypeID;
            ID = iD;
        }

        public override string ToString()
        {
            return Name;
        }

        public List<ComboboxStackedSubscriptions> MyComboSubscriptions
        {
            get
            {
                return MyComboSubscriptions;
            }
        }
    }
}
