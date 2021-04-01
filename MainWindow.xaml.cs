using System;
using System.Collections.Generic;
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
using System.Net;
using StatFeed.Class;
using Newtonsoft.Json;
using System.IO;
using System.Windows.Forms;
using System.Drawing;
using System.Reflection;
using Microsoft.Win32;
using System.Runtime.InteropServices;
using IWshRuntimeLibrary;
using Application = System.Windows.Forms.Application;

namespace StatFeed
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private NotifyIcon MyNotifyIcon;

        public MainWindow()
        {
            InitializeComponent();

            //Set Notification Tray Icon
            LoadNotificationIcon();

            //This checks to see if a shortcut has been added to the startup folder or not
            CheckIfInStartup();

            OnLoad();            
        }

        public void OnLoad()
        {


            //Performs a check to see if it is the users first time
            bool firstTime = SqliteDataAccess.FirstTime();
            if (firstTime)
            {
                //Get a list of available COM ports
                string[] Ports = DisplayModel.FindAllPorts();
                SqliteDataAccess.SetLastCOMPort(Ports[0]);

                //If its the first time then open the Login Page
                MainFrame.Content = new StatFeed.Pages.LoginPageGame();

            }
            else
            {
                //Minimize the window
                this.WindowState = WindowState.Minimized;
                this.ShowInTaskbar = false;
                MyNotifyIcon.Visible = true;
                this.Hide();

                //Update the database                
                List<SubscribedGameModel> SubscriptionList = SqliteDataAccess.GetSubscriptionList();
                foreach (var Subscription in SubscriptionList)
                {
                    SqliteDataAccess.SaveStats(StatModel.GenerateStats(Subscription));
                }

                //Load main page
                MainFrame.Content = new StatFeed.Pages.MainPage();



            }
        }
        private void LoadNotificationIcon()
        {            
            MyNotifyIcon = new NotifyIcon();
            Stream iconStream = System.Windows.Application.GetResourceStream(new Uri("pack://application:,,,/Images/Logo/StatFeed_Icon.ico")).Stream;
            MyNotifyIcon.Icon = new Icon(iconStream);
            MyNotifyIcon.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(MyNotifyIcon_MouseDoubleClick);            
            MyNotifyIcon.Text = "StatFeed";
        }
        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            //Send blank screen to display
            string CurrentPort = SqliteDataAccess.GetLastCOMPort();
            DisplayModel.SendToPort("0", "0", "0", "0", CurrentPort, "BLNK");

            System.Windows.Application.Current.Shutdown();
        }
        private void Minimize_Click(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Normal)
            {
                this.WindowState = WindowState.Minimized;
            }
            else if (this.WindowState == WindowState.Minimized)
            {
                this.WindowState = WindowState.Normal;
            }
        }
        void MyNotifyIcon_MouseDoubleClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            this.Show();
            this.WindowState = WindowState.Normal;
        }
        private void Window_StateChanged_1(object sender, EventArgs e)
        {
            if (this.WindowState == WindowState.Minimized)
            {
                this.ShowInTaskbar = false;
                MyNotifyIcon.Visible = true;
                this.Hide();
            }
            else if (this.WindowState == WindowState.Normal)
            {
                MyNotifyIcon.Visible = false;
                this.ShowInTaskbar = true;
                this.WindowState = WindowState.Normal;
                this.Show();
                this.Activate();
            }
        }
        private void CheckIfInStartup()
        {            
            //If the file does not exist then create it (meaning it is the first time the user has opened the software)
            if (!System.IO.File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.Startup) + "\\StatFeed.lnk"))
            {
                AppShortcutToStartup();
            }
            //Do nothing if the file exists           
        }
        private void AppShortcutToStartup()
        {
            WshShell wshShell = new WshShell();
            IWshRuntimeLibrary.IWshShortcut shortcut;
            string startUpFolderPath =
              Environment.GetFolderPath(Environment.SpecialFolder.Startup);

            // Create the shortcut
            shortcut =
              (IWshRuntimeLibrary.IWshShortcut)wshShell.CreateShortcut(
                startUpFolderPath + "\\" +
                Application.ProductName + ".lnk");

            shortcut.TargetPath = Application.ExecutablePath;
            shortcut.WorkingDirectory = Application.StartupPath;                     
            shortcut.Save();
        }       
    }
}
