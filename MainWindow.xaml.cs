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
using Squirrel;
using Microsoft.Win32;
using System.Runtime.InteropServices;

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
            MyNotifyIcon = new NotifyIcon();
            Stream iconStream = System.Windows.Application.GetResourceStream(new Uri("pack://application:,,,/Images/Logo/StatFeed_Icon.ico")).Stream;
            MyNotifyIcon.Icon = new Icon(iconStream);
            MyNotifyIcon.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(MyNotifyIcon_MouseDoubleClick);

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
                MainFrame.Content = new StatFeed.Pages.LoginPage();

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

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
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
            if (!File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.Startup) + "\\StatFeed.lnk"))
            {
                AppShortcutToStartup();
            }
            //Do nothing if the file exists           
        }
        private void AppShortcutToStartup()
        {
            //This creates a shortcut and places it into the startup folder 
            Type t = Type.GetTypeFromCLSID(new Guid("72C24DD5-D70A-438B-8A42-98424B88AFB8")); //Windows Script Host Shell Object
            dynamic shell = Activator.CreateInstance(t);
            try
            {
                var lnk = shell.CreateShortcut(Environment.GetFolderPath(Environment.SpecialFolder.Startup) + "\\StatFeed.lnk");
                try
                {
                    string app = Assembly.GetExecutingAssembly().Location;
                    lnk.TargetPath = Assembly.GetExecutingAssembly().Location;
                    lnk.Arguments = "-m";
                    lnk.IconLocation = app.Replace('\\', '/');
                    lnk.Save();
                }
                finally
                {
                    Marshal.FinalReleaseComObject(lnk);
                }
            }
            finally
            {
                Marshal.FinalReleaseComObject(shell);
            }            
        }       
    }
}
