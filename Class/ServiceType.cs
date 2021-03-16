using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatFeed.Class
{
    public class ServiceTypeModel
    {

        //Properties
        public int ServiceID { get; set; }
        public string ServiceName { get; set; }
        public int ServiceUpdateTimer { get; set; }


        public ServiceTypeModel()
        {

        }

        public ServiceTypeModel(int serviceID, string serviceName, int serviceUpdateTimer)
        {
            ServiceID = serviceID;
            ServiceName = serviceName;
            ServiceUpdateTimer = serviceUpdateTimer;
        }
    }
}
