using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Windows.Threading;
using System.Net;
using StatFeed.Class;

namespace StatFeed.Class
{
    public class StatModel
    {
        //Properties
        public int StatID { get; set; }
        public int SubscriptionID { get; set; }
        public string StatName { get; set; }
        public string StatValue_1 { get; set; }
        public string StatValue_2 { get; set; }
        public string StatValue_3 { get; set; }
        public int Last_Selected { get; set; }

        //Instantiating a new game

        public StatModel()
        {

        }

        //StatModel where only one stat value is needed (For games)
        public StatModel(int statID, int subscriptionID, string statName, string statValue_1, int last_Selected)
        {
            StatID = statID;
            SubscriptionID = subscriptionID;
            StatName = statName;
            StatValue_1 = statValue_1;
            StatValue_2 = "0";
            StatValue_3 = "0";
            Last_Selected = last_Selected;
        }

        //StatModel where 3 stat values are needed (For Finances)
        public StatModel(int statID, int subscriptionID, string statName, string statValue_1, string statValue_2, string statValue_3, int last_Selected)
        {
            StatID = statID;
            SubscriptionID = subscriptionID;
            StatName = statName;
            StatValue_1 = statValue_1;
            StatValue_2 = statValue_2;
            StatValue_3 = statValue_3;
            Last_Selected = last_Selected;
        }

        public static List<StatModel> GenerateStats (SubscribedGameModel Subscription)
        {
            //This function generates stats for a Subscription that is passed to it (To iterate, this method must be integrated into a loop)
            
            //Create list to store new stats in
            List<StatModel> StatList = new List<StatModel>();          
            
            //Check ServiceTypeID to see what to do with it

            //Game
            if (Subscription.ServiceTypeID == 1)
            {
                //Creates a GameModel object
                GameModel Game = new GameModel();
                Game = SqliteDataAccess.SelectGame(Subscription.GetID());


                //Iterates through the ID to find the appropriate API
                //Fortnite
                if (Game.ID == 1)
                {
                    try
                    {

                        //Create URL
                        string URL_Header = "TRN-Api-Key: 417501fe-eeb1-45d1-ace6-57a28ebfc8d5";
                        string URL_Prefix = "https://api.fortnitetracker.com/v1/profile/";
                        string Chosen_Service_Name = SqliteDataAccess.GetGameAbbreviation(Game.ID, Subscription.GetChosen_Service());
                        string UserName = Subscription.UserName;

                        UserName = UserName.Replace(" ", "%20");

                        string URL = URL_Prefix + Chosen_Service_Name + "/" + UserName + "/";

                        //Returns JSON Object of API result
                        dynamic dobj = ReturnDobj(URL, URL_Header);

                        dynamic UserNameCheck = dobj.epicUserHandle;

                        //If UserName doesn't exist
                        if (UserNameCheck is null)
                        {

                        }
                        //If UserName does exist
                        else
                        {
                            dynamic lifetimestats = dobj.lifeTimeStats;
                            dynamic solostats = dobj.stats.p2;
                            dynamic duosstats = dobj.stats.p10;
                            dynamic squadstats = dobj.stats.p9;

                            //Lifetime Stats
                            foreach (var lifetimestat in lifetimestats)
                            {
                                string StatName = lifetimestat.key;
                                string StatValue = lifetimestat.value;

                                //formatting names of statistics to look cleaner 
                                StatName = GameModel.FormatValue(StatName);
                                StatValue = StatModel.FormatValue(StatValue);
                                StatName = "Lifetime " + StatName;

                                //creates an object of that stat and adds it to the combobox
                                StatModel CurrentGameStat = new StatModel(0, Subscription.SubscriptionID, StatName, StatValue, 0);
                                StatList.Add(CurrentGameStat);
                            }

                            //Solo Stats 
                            foreach (var solostat in solostats)
                            {
                                string StatName = solostat.First.label;
                                string StatValue = solostat.First.value;

                                //formatting names of statistics to look cleaner 
                                StatName = GameModel.FormatValue(StatName);
                                StatValue = StatModel.FormatValue(StatValue);
                                StatName = "Solo " + StatName;

                                //creates an object of that stat and adds it to the combobox
                                StatModel CurrentGameStat = new StatModel(0, Subscription.SubscriptionID, StatName, StatValue, 0);
                                StatList.Add(CurrentGameStat);
                            }

                            //Duo Stats 
                            foreach (var duostat in duosstats)
                            {
                                string StatName = duostat.First.label;
                                string StatValue = duostat.First.value;

                                //formatting names of statistics to look cleaner 
                                StatName = GameModel.FormatValue(StatName);
                                StatValue = StatModel.FormatValue(StatValue);
                                StatName = "Duo " + StatName;

                                //creates an object of that stat and adds it to the combobox
                                StatModel CurrentGameStat = new StatModel(0, Subscription.SubscriptionID, StatName, StatValue, 0);
                                StatList.Add(CurrentGameStat);
                            }

                            //Squad Stats 
                            foreach (var squadstat in squadstats)
                            {
                                string StatName = squadstat.First.label;
                                string StatValue = squadstat.First.value;

                                //formatting names of statistics to look cleaner 
                                StatName = GameModel.FormatValue(StatName);
                                StatValue = StatModel.FormatValue(StatValue);
                                StatName = "Squad " + StatName;

                                //creates an object of that stat and adds it to the combobox
                                StatModel CurrentGameStat = new StatModel(0, Subscription.SubscriptionID, StatName, StatValue, 0);
                                StatList.Add(CurrentGameStat);
                            }
                        }
                    }
                    catch
                    {

                    }
                }
                //Apex Legends
                if (Game.ID == 2)
                {
                    try
                    {
                        //Create URL
                        string URL_Header = "TRN-Api-Key: 417501fe-eeb1-45d1-ace6-57a28ebfc8d5";
                        string URL_Prefix = "https://public-api.tracker.gg/v2/apex/standard/profile/";
                        string Chosen_Service_Name = SqliteDataAccess.GetGameAbbreviation(Game.ID, Subscription.GetChosen_Service());
                        string UserName = Subscription.UserName;

                        UserName = UserName.Replace(" ", "%20");

                        string URL = URL_Prefix + Chosen_Service_Name + "/" + UserName + "/";

                        //Returns JSON Object of API result
                        dynamic dobj = ReturnDobj(URL, URL_Header);

                        dynamic UserNameCheck = dobj.data.platformInfo.platformUserHandle;

                        //If UserName doesn't exist
                        if (UserNameCheck is null)
                        {

                        }
                        //If UserName does exist
                        else
                        {
                            dynamic stats = dobj.data.segments.First.stats;

                            foreach (var stat in stats)
                            {
                                string StatName = stat.First.displayName;
                                string StatValue = stat.First.value;

                                //formatting names of statistics to look cleaner                    
                                StatName = GameModel.FormatValue(StatName);
                                StatValue = StatModel.FormatValue(StatValue);

                                //creates an object of that stat and adds it to the combobox
                                StatModel CurrentGameStat = new StatModel(0, Subscription.SubscriptionID, StatName, StatValue, 0);
                                StatList.Add(CurrentGameStat);
                            }
                        }
                    }
                    catch
                    {

                    }
                }
                //Overwatch
                if (Game.ID == 3)
                {
                    try
                    {
                        //Create URL
                        string URL_Header = "TRN-Api-Key: 417501fe-eeb1-45d1-ace6-57a28ebfc8d5";
                        string URL_Prefix = "https://public-api.tracker.gg/v2/overwatch/standard/profile/";
                        string Chosen_Service_Name = SqliteDataAccess.GetGameAbbreviation(Game.ID, Subscription.GetChosen_Service());
                        string UserName = Subscription.UserName;

                        UserName = UserName.Replace(" ", "");
                        UserName = UserName.Replace("#", "%23");

                        string URL = URL_Prefix + Chosen_Service_Name + "/" + UserName + "/";

                        //Returns JSON Object of API result
                        dynamic dobj = ReturnDobj(URL, URL_Header);

                        dynamic UserNameCheck = dobj.data.platformInfo.platformUserHandle;

                        //If UserName doesn't exist
                        if (UserNameCheck is null)
                        {

                        }
                        //If UserName does exist
                        else
                        {
                            dynamic casualstats = dobj.data.segments[0].stats;
                            dynamic compstats = dobj.data.segments[1].stats;

                            //casual stats
                            foreach (var stat in casualstats)
                            {
                                string StatName = stat.First.displayName;
                                string StatValue = stat.First.value;

                                //formatting names of statistics to look cleaner                    
                                StatName = GameModel.FormatValue(StatName);
                                StatValue = StatModel.FormatValue(StatValue);
                                StatName = "Casual " + StatName;

                                //creates an object of that stat and adds it to the combobox
                                StatModel CurrentGameStat = new StatModel(0, Subscription.SubscriptionID, StatName, StatValue, 0);
                                StatList.Add(CurrentGameStat);
                            }

                            //competitive stats
                            foreach (var stat in compstats)
                            {
                                string StatName = stat.First.displayName;
                                string StatValue = stat.First.value;

                                //formatting names of statistics to look cleaner                    
                                StatName = GameModel.FormatValue(StatName);
                                StatName = "Competitive " + StatName;

                                //creates an object of that stat and adds it to the combobox
                                StatModel CurrentGameStat = new StatModel(0, Subscription.SubscriptionID, StatName, StatValue, 0);
                                StatList.Add(CurrentGameStat);
                            }
                        }
                    }
                    catch
                    {

                    }
                }
                //Counter-Strike: Global Offensive
                if (Game.ID == 4)
                {
                    try
                    {
                        //Create URL
                        string URL_Header = "";
                        string URL_Prefix = "http://api.steampowered.com/ISteamUserStats/GetUserStatsForGame/v0002/?appid=730&key=27619344E1EF913B5337CB9D0B3F186E&steamid=";
                        string UserName = Subscription.UserName;

                        //Convert UserName to SteamID
                        string SteamID = GameModel.GetSteamID(UserName);

                        string URL = URL_Prefix + SteamID;

                        //Returns JSON Object of API result
                        dynamic dobj = ReturnDobj(URL, URL_Header);

                        dynamic UserNameCheck = dobj.playerstats.steamID;

                        //If UserName doesn't exist
                        if (UserNameCheck is null)
                        {

                        }
                        //If UserName does exist
                        else
                        {
                            dynamic stats = dobj.playerstats.stats;

                            foreach (var stat in stats)
                            {
                                string StatName = stat.name;
                                string StatValue = stat.value;

                                //formatting names of statistics to look cleaner                    
                                StatName = GameModel.FormatValue(StatName);
                                StatValue = StatModel.FormatValue(StatValue);

                                //creates an object of that stat and adds it to the combobox
                                StatModel CurrentGameStat = new StatModel(0, Subscription.SubscriptionID, StatName, StatValue, 0);
                                StatList.Add(CurrentGameStat);
                            }
                        }
                    }
                    catch
                    {

                    }

                }
            }

            //Finance
            if (Subscription.ServiceTypeID == 2)
            {
                FinanceModel Finance = new FinanceModel();
                Finance = SqliteDataAccess.SelectFinance(Subscription.GetID());

                //Iterate through the ID to find the appropriate API
                //Cryptocurrency
                if (Finance.ID == 1)
                {
                    try
                    {
                        

                        //Create URL
                        string URL_Header = "";
                        string URL_Prefix = "https://api.binance.com/api/v3/ticker/24hr?symbol=";

                        //Breaks the Ticker into two sections (Has to be entered like: "BTCUSDT")
                        string Ticker = Subscription.UserName.ToUpper();
                        string URL = URL_Prefix + Ticker;

                        //Returns JSON Object of API result
                        dynamic dobj = ReturnDobj(URL, URL_Header);

                        dynamic TickerCheck = dobj["symbol"];

                        //If Ticker doesn't exist
                        if (TickerCheck is null)
                        {

                        }
                        //If Ticker does exist
                        else
                        {
                            //Name of Ticker
                            string StatName = Subscription.UserName;
                            //Current price
                            decimal CurrentPrice = dobj["lastPrice"];
                            //price change
                            decimal PointDifference = dobj["priceChange"];
                            //price change percentage
                            decimal PointDifferencePercentage = dobj["priceChangePercent"];

                            //Formats value to 2 decmial places

                            string strCurrentPrice = Math.Round(CurrentPrice, 2).ToString();
                            string strPointDifference = Math.Round(PointDifference, 2).ToString();
                            string strPointDifferencePercentage = Math.Round(PointDifferencePercentage, 2).ToString();


                            //formatting names of statistics to look cleaner                    
                            StatName = GameModel.FormatValue(StatName);                            

                            //creates an object of that stat and adds it to the combobox
                            StatModel CurrentGameStat = new StatModel(0, Subscription.SubscriptionID, StatName, strCurrentPrice, strPointDifference, strPointDifferencePercentage, 0);
                            StatList.Add(CurrentGameStat);

                        }
                    }
                    catch
                    {

                    }
                }
                //US Market
                if (Finance.ID == 2)
                {
                    try
                    {    
                        //Create URL
                        string URL_Header = "";
                        string URL_Prefix = "https://finnhub.io/api/v1/quote?symbol=";

                        //Breaks the Ticker into two sections (Has to be entered like: "Bitcoin USD")
                        string Ticker = Subscription.UserName;                       

                        string URL_Mid = "&token=";
                        string URL = URL_Prefix + Ticker.ToUpper() + URL_Mid + Subscription.APIKey;

                        //Returns JSON Object of API result
                        dynamic dobj = ReturnDobj(URL, URL_Header);

                        dynamic TickerCheck = dobj["c"];

                        //If Ticker doesn't exist
                        if (TickerCheck.Value == 0)
                        {

                        }
                        //If Ticker does exist
                        else
                        {
                            //Name of Ticker
                            string StatName = Ticker.ToUpper();
                            //Current price
                            string CurrentPrice = dobj["c"];

                            decimal CurrentPriceDecimal = dobj["c"];
                            decimal PreviousClosePrice = dobj["pc"];


                            //Do some maths to work out daily difference and percentage difference
                            //Point Difference
                            String PointDifference = Math.Round(CurrentPriceDecimal - PreviousClosePrice, 2).ToString();

                            //Point percentage difference
                            decimal PointDifferencePercentageDecimal = (((CurrentPriceDecimal - PreviousClosePrice) / PreviousClosePrice) * 100);

                            String PointDifferencePercentage = Math.Round(PointDifferencePercentageDecimal, 2).ToString();

                            PointDifferencePercentage = PointDifferencePercentage + "%";

                            //formatting names of statistics to look cleaner                    
                            StatName = GameModel.FormatValue(StatName);                           
                           

                            //creates an object of that stat and adds it to the combobox
                            StatModel CurrentGameStat = new StatModel(0, Subscription.SubscriptionID, StatName, CurrentPrice, PointDifference, PointDifferencePercentage, 0);
                            StatList.Add(CurrentGameStat);
                        }
                    }
                    catch
                    {

                    }
                }
            }
                        
            return StatList;
        }

        public static dynamic ReturnDobj(string url, string header)
        {
            WebClient API_Client = new WebClient();
            if (header != "")
            {
                //If the request requires a header to be added then add it
                API_Client.Headers.Add(header);
            }                        
            string API_Info = API_Client.DownloadString(url);
            //convert the JSON string to a series of objects
            dynamic dobj = JsonConvert.DeserializeObject<dynamic>(API_Info);
            return dobj;
        }

       
        //Methods         
        public override string ToString()
        {
            return StatName;
        }

        public static string FormatValue(string value)
        {
            if (value == null)
                value = "value";
            if (value.Length == 0)
                return value;

            //Removes weird characters            
            value = value.Replace(",", "");

            return value;
        }

    }
}
