using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Caliburn.Micro;
using RestSharp;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;
using Windows.UI.ApplicationSettings;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace TodaysHappenings
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            Data=new List<DataString>();
            SettingsPane.GetForCurrentView().CommandsRequested+=MainPage_CommandsRequested;
            try
            {
                GetResponseObject((rootsub) => Execute.OnUIThread(() =>
                {
                    ProcessData(rootsub);

                    ProcessVal();
                }
                ), (error) => Execute.OnUIThread(() => DoNothing()));
            }
            catch (Exception)
            {
                
            }
            
        }


        public List<DataString> Data { get; set; }

        void MainPage_CommandsRequested(SettingsPane sender, SettingsPaneCommandsRequestedEventArgs args)
        {
            bool afound = false;
            bool sfound = false;
            bool pfound = false;
            foreach (var command in args.Request.ApplicationCommands.ToList())
            {
                if (command.Label == "About")
                {
                    afound = true;
                }
                if (command.Label == "Settings")
                {
                    sfound = true;
                }
                if (command.Label == "Policy")
                {
                    pfound = true;
                }
            }
            if (!afound)
                args.Request.ApplicationCommands.Add(new SettingsCommand("s", "About", (p) => { cfoAbout.IsOpen = true; }));
            //if (!sfound)
            //    args.Request.ApplicationCommands.Add(new SettingsCommand("s", "Settings", (p) => { cfoSettings.IsOpen = true; }));
            //if (!pfound)
            //    args.Request.ApplicationCommands.Add(new SettingsCommand("s", "Policy", (p) => { cfoPolicy.IsOpen = true; }));
            args.Request.ApplicationCommands.Add(new SettingsCommand("privacypolicy", "Privacy policy", OpenPrivacyPolicy));
        }

        private async void OpenPrivacyPolicy(IUICommand command)
        {
            var uri = new Uri("http://www.thatslink.com/privacy-statment/ ");
            await Launcher.LaunchUriAsync(uri);
        }
        public async void ProcessVal()
        {
            Dispatcher.RunAsync(CoreDispatcherPriority.High, () =>
                {
                    lstItems.ItemsSource = Data.OrderBy(r=>r.year);
                });
            
        }

        

        public string contentVal { get; set; }

        private async void ProcessData(string doc)
        {
            try
            {
                contentVal = doc.Substring(doc.IndexOf("<H3>On this day...</h3>", System.StringComparison.Ordinal)+24);
                int contentLength = contentVal.Length;
                int indexTillRemove = contentVal.IndexOf("<BR><BR>  <BR>", System.StringComparison.Ordinal);
                contentVal = contentVal.Substring(0, indexTillRemove).Replace("<BR>", "@").Replace("<b> ", "").Replace(" </B>   ",":");
                foreach (string str in contentVal.Split(Convert.ToChar("@")))
                {
                    var temp = str.Split(Convert.ToChar(":"));
                    var data = new DataString() {val =temp[1].Replace("\n",""), year =temp[0]};
                    Data.Add(data);
                }
            }
            catch (Exception)
            {

            }
            
            
        }

        

        private void DoNothing()
        {
            
        }

        public void GetResponseObject(Action<string> success, Action<string> failure)
        {
            var client = new RestClient("http://www.scopesys.com/cgi-bin/today2.cgi");

            var request = new RestRequest(Method.POST);


            request.AddHeader("Accept", "text/html, application/xhtml+xml, */*");
            request.AddHeader("Referer", "http://www.scopesys.com/anyday/");
            request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
            request.AddHeader("DNT", "1");
            request.AddHeader("askday", "01");
            request.AddHeader("askday", "01");
            request.AddParameter("askmonth", "01");
            request.AddParameter("askday", "01");
            
            client.ExecuteAsync(request, response =>
            {
                if (response.ResponseStatus == ResponseStatus.Error)
                {
                    failure(response.ErrorMessage);
                }
                else
                {
                    success(response.Content);
                }
            });
        }

        public class DataString
        {
            public string val { get; set; }
            public string year { get; set; }
        }


        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.  The Parameter
        /// property is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
        }

        private void Search_OnClick(object sender, RoutedEventArgs e)
        {
            var tempData = Data;
            if (cmbBox.SelectedIndex == 0)
            {
                
            }

            if (cmbBox.SelectedIndex == 1)
            {
                tempData = tempData.Where(r => Convert.ToInt32(r.year) <= Convert.ToInt32(txtSearch.Text)).ToList();
            }

            if (cmbBox.SelectedIndex == 2)
            {
                tempData = tempData.Where(r => Convert.ToInt32(r.year) > Convert.ToInt32(txtSearch.Text)).ToList();
            }

            if (cmbBox.SelectedIndex == 3)
            {
                tempData = tempData.Where(r => Convert.ToInt32(r.year) == Convert.ToInt32(txtSearch.Text)).ToList();
            }

            lstItems.ItemsSource = tempData;
        }
    }
}
