using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StatFeed.Class;
using System.Configuration;
using System.Data;
using System.Data.SQLite;
using Dapper;

namespace StatFeed
{
    public class SqliteDataAccess
    {
        //Properties


        public static SubscribedGameModel CurrentSubscriptionSelected;
        public static GameModel CurrentGameSubscriptionSelected;


        //MISC Methods
        private static string LoadConnectionString(string id = "Default")
        {
            return ConfigurationManager.ConnectionStrings[id].ConnectionString;
        }
        public static bool FirstTime()
        {
            //This counts the returned objects of SubscribedGames to check if it's the users first time on the software or not
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                var output = cnn.Query<SubscribedGameModel>("select * from SubscribedGame", new DynamicParameters());
                int count = output.Count();

                if (count > 0)
                {
                    //This isn't the users first time
                    return false;
                }
                else
                {
                    //This is the users first time
                    return true;
                }
            }
        }


        //GAME Methods
        public static List<GameModel> GetAvailableGames()
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                var AvailableGames = cnn.Query<GameModel>("select * from Game", new DynamicParameters()).ToList();
                return AvailableGames;
            }
        }
        public static GameModel SelectGame(int GameID)
        {
            GameModel Game = new GameModel();

            //Takes GameID and returns object of that game
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                List<GameModel> Games = cnn.Query<GameModel>("select * from Game where GameID =" + GameID, new DynamicParameters()).ToList();

                foreach (var item in Games)
                {
                    Game = item;
                }
            }

            return Game;
        }
        public static string GetGameAbbreviation(int GameID, int Chosen_Service)
        {
            string Abbreviation = "0";

            //Takes GameID and returns object of that game
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                List<GameModel> Games = cnn.Query<GameModel>("select * from Game where GameID =" + GameID, new DynamicParameters()).ToList();

                foreach (var item in Games)
                {
                    //If Chosen_Service is PC
                    if (Chosen_Service == 1)
                    {
                        Abbreviation = item.Platform_PC;
                    }
                    //If Chosen_Service is Xbox
                    if (Chosen_Service == 2)
                    {
                        Abbreviation = item.Platform_Xbox;
                    }
                    //If Chosen_Service is PSN
                    if (Chosen_Service == 3)
                    {
                        Abbreviation = item.Platform_PSN;
                    }
                }
            }
            return Abbreviation;
        }


        //SUBSCRIBED GAME Methods
        public static SubscribedGameModel GetSubscription(int SubscriptionID)
        {
            SubscribedGameModel Subscription = new SubscribedGameModel();

            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                List<SubscribedGameModel> Subscriptions = cnn.Query<SubscribedGameModel>("select *  from SubscribedGame where SubscriptionID =" + SubscriptionID, new DynamicParameters()).ToList();

                foreach (var item in Subscriptions)
                {
                    Subscription = item;
                }
            }
            return Subscription;
        }
        public static List<GameModel> GetSubscribedGames()
        {
            List<SubscribedGameModel> SubscriptionList = new List<SubscribedGameModel>();
            SubscriptionList = GetSubscriptionList();

            List<int> TempSubscriptionIDs = new List<int>();

            foreach (var Subscription in SubscriptionList)
            {
                int SubscriptionGameID = Subscription.GetGameID();
                TempSubscriptionIDs.Add(SubscriptionGameID);
            }

            string SubscriptionGameIDs = string.Join(",", TempSubscriptionIDs);

            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                var CurrentGameSubcriptions = cnn.Query<GameModel>("select * from Game Where GameID IN (" + SubscriptionGameIDs + ")", new DynamicParameters()).ToList();
                return CurrentGameSubcriptions;
            }
        }
        public static List<SubscribedGameModel> GetSubscriptionList()
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                List<SubscribedGameModel> output = cnn.Query<SubscribedGameModel>("select * from SubscribedGame", new DynamicParameters()).ToList();
                return output;
            }
        }
        public static void SaveSubscribedGame(int GameID, string UserName, int Chosen_Service, int Last_Selected, string Custom_Background)
        {
            //Takes GameID of current game, Username (After it has been verified) and the Chosen_Service and writes it to the table
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                cnn.Execute("insert into SubscribedGame (GameID, UserName, Chosen_Service, Last_Selected, Custom_Background) values('" + GameID + "','" + UserName + "','" + Chosen_Service + "','" + Last_Selected + "','" + Custom_Background + "')");
            }
        }

        public static void SetToDefaultBackgroundSubscription(int SubscriptionID)
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                cnn.Execute("UPDATE SubscribedGame SET Custom_Background = 'Default' WHERE SubscriptionID =" + SubscriptionID);
            }
        }
        public static void DeleteSubscribedGame(int SubscriptionID)
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                cnn.Query<SubscribedGameModel>("delete from SubscribedGame where SubscriptionID =" + SubscriptionID, new DynamicParameters());
            }
        }


        //STAT Methods
        public static List<StatModel> GetStats(int SubscriptionID)
        {
            //Takes the SubscriptionID of the currently selected game and returns a list that can be placed into the Stat Combobox
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                List<StatModel> Stats = cnn.Query<StatModel>("select * from Stat where SubscriptionID =" + SubscriptionID, new DynamicParameters()).ToList();
                return Stats;
            }
        }
        public static void SaveStats(List<StatModel> Stats)
        {
            //Takes a list of Stats and writes them to the Stat table. This could either be the whole subscription list of stats or could be just one subscription (To save on API usage)            
            foreach (StatModel stat in Stats)
            {
                //Checks if stat exists. Will update or insert based on that search                
                using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
                {
                    //Checks to see if the stat exists
                    List<StatModel> output = cnn.Query<StatModel>("select * from stat where SubscriptionID = @SubscriptionID and StatName = @StatName", stat).ToList();

                    //If the list comes back empty then INSERT information
                    if (output.Count == 0)
                    {
                        cnn.Execute("insert into Stat (SubscriptionID, StatName, StatValue) values(@SubscriptionID, @StatName, @StatValue)", stat);
                    }
                    //If the list comes back full meaning that a stat already exists then UPDATE information
                    else
                    {
                        cnn.Execute("Update Stat Set StatValue = @StatValue where SubscriptionID = @SubscriptionID and StatName = @StatName", stat);
                    }
                }
            }
        }
        public static void DeleteStats(int SubscriptionID)
        {
            //Takes the SubscriptionID, searches the database and deletes what it finds
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                cnn.Query<SubscribedGameModel>("delete from Stat where SubscriptionID =" + SubscriptionID, new DynamicParameters());
            }
        }


        //USER SETTINGS Methods
        public static void SetGamesComboCheckpoint(SubscribedGameModel Subscription)
        {
            //This will revert all the Last_Selected properties to "0" and then set the passed subscriptionID to "1"
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                //This sets all the values to 0
                cnn.Execute("update SubscribedGame set Last_Selected = 0");
                //This sets the Last_Selected to 1 for that last selected game
                cnn.Execute("update SubscribedGame set Last_Selected = 1 where SubscriptionID = @SubscriptionID", Subscription);
            }
        }
        public static void SetStatsComboCheckpoint(int StatID)
        {
            //This will revert all the Last_Selected properties to "0" and then set the passed StatID to "1"
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                //This sets all the values to 0
                cnn.Execute("update Stat set Last_Selected = 0");
                //This sets the Last_Selected to 1 for that last selected stat
                cnn.Execute("update Stat set Last_Selected = 1 where StatID =" + StatID);
            }
        }
        public static int GetGamesComboCheckpoint()
        {
            //Finds the subscription with the Last_Selected property set to "1"
            int SubscriptionID = 0;

            //Takes GameID and returns object of that game
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                List<SubscribedGameModel> Subscription = cnn.Query<SubscribedGameModel>("select * from SubscribedGame where Last_Selected = 1").ToList();

                //If no subscription was last selected 
                if (Subscription.Count == 0)
                {
                    List<SubscribedGameModel> TopSubscription = cnn.Query<SubscribedGameModel>("select * from SubscribedGame limit 1").ToList();

                    foreach (var item in TopSubscription)
                    {
                        SubscriptionID = item.SubscriptionID;
                        return SubscriptionID;
                    }
                }
                else
                {
                    foreach (var item in Subscription)
                    {
                        SubscriptionID = item.SubscriptionID;
                        return SubscriptionID;
                    }
                }
            }
            return SubscriptionID;
        }
        public static int GetStatsComboCheckpoint(int SubscriptionID)
        {
            //Finds the subscription with the Last_Selected property set to "1"
            int StatID = 0;

            //To check that this stat is related to the game chosen, pass the CurrentGame SubscriptionID. If the Stat shares this then leave it
            //If the stat does not have the same subscription ID (The game combobox has been changed by the user) then reset it to the top selected 
            //Of that CurrentGame SubscriptionID

            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                //Creates a list and populates it with the last selected stat of that CurrentGame
                List<StatModel> StatCheck = cnn.Query<StatModel>("select * from Stat where Last_Selected = 1 and SubscriptionID =" + SubscriptionID).ToList();

                //If the game has been changed the list will return empty 
                if (StatCheck.Count == 0)
                {
                    //This will return the top result of the CurrentGame
                    List<StatModel> Stat = cnn.Query<StatModel>("select * from stat where SubscriptionID = " + SubscriptionID + " limit 1" + SubscriptionID).ToList();

                    foreach (var item in Stat)
                    {
                        StatID = item.StatID;
                        return StatID;
                    }

                }
                else
                {
                    foreach (var item in StatCheck)
                    {
                        StatID = item.StatID;
                        return StatID;
                    }
                }
            }
            return StatID;
        }


        //COM PORT Methods
        public static string GetLastCOMPort()
        {
            //This gets the last COM port that the user saved
            string output = "No Port";

            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                List<string> COMport = new List<string>();

                COMport = cnn.Query<string>("select * from UserSettings limit 1").ToList();

                foreach (var item in COMport)
                {
                    output = item;
                    return output;
                }

            }
            return output;
        }

        public static void SetLastCOMPort(string COMPort)
        {
            //This takes a COM port value and sets it to the passed parameter
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                cnn.Execute("update UserSettings set COM_Port = '" + COMPort + "'");

                //cnn.Execute("insert into Stat (SubscriptionID, StatName, StatValue) values(@SubscriptionID, @StatName, @StatValue)", stat);
            }
        }
    }
}



