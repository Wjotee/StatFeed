using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Net;
using System.Reflection;

namespace StatFeed.Class
{
    public class Update
    {
        //Properties

        //Methods  
        public static string GetThisVersion()
        {
            //Returns current version of program
            string CurrentTagName = Assembly.GetExecutingAssembly().GetName().Version.ToString();            
            return CurrentTagName;
        }        
        public static string GetLatestVersion()
        {
            //Returns latest tag version from github, if can't find it then return the current version
            string CurrentTagName = GetThisVersion();

            try
            {
                //Create URL
                string URL_Header = "User-Agent: Anything";               
                string URL = "https://api.github.com/repos/Wjotee/StatFeed/releases";

                //Returns JSON Object of API result
                dynamic dobj = StatModel.ReturnDobj(URL, URL_Header);


                string TagName = dobj.First.tag_name;
                bool prerelease = dobj.First.prerelease;

                if (!prerelease)
                {
                    //If the tag is the same but it's a prerelease then return this version tag
                    return CurrentTagName;
                }

                return TagName;
            }
            catch
            {
                return CurrentTagName;
            }
        }
        public static bool CheckForUpdate()
        {
            //This compares the two versions and returns a bool for if there is a new update
            string CurrentTagName = GetThisVersion();
            string NewTagName = GetLatestVersion();

            //Format the numbers the same
            CurrentTagName = CurrentTagName.Remove(CurrentTagName.Length - 2);
            NewTagName = NewTagName.Remove(0, 1);

            if (NewTagName == CurrentTagName)
            {
                return false;
            }
            else
            {
                return true;
            }
        }        
    }
}
