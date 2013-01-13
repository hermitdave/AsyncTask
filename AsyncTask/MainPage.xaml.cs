using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System.Threading;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace AsyncTask
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.  The Parameter
        /// property is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
        }

        static async Task WaitForMe(int duration, int id, bool bShowMessage = false)
        {
            await Task.Delay(duration*1000);
            if(bShowMessage)
                await new MessageDialog(String.Format("Task {0}: done waiting {1} seconds", id, duration)).ShowAsync();
        }

        static async Task WaitMany_WrongWay(int duration)
        {
            DateTime dtStart = DateTime.Now;

            await WaitForMe(duration, 1);
            await WaitForMe(duration, 2);
            await WaitForMe(duration, 3);
            await WaitForMe(duration, 4);
            await WaitForMe(duration, 5);

            await new MessageDialog(String.Format("It took {0} seconds for all calls to finish", DateTime.Now.Subtract(dtStart).TotalSeconds)).ShowAsync();
        }

        static async Task WaitMany_CorrectWay(int duration)
        {
            DateTime dtStart = DateTime.Now;

            List<Task> tasks = new List<Task>();
            tasks.Add(WaitForMe(duration, 1));
            tasks.Add(WaitForMe(duration, 2));
            tasks.Add(WaitForMe(duration, 3));
            tasks.Add(WaitForMe(duration, 4));
            tasks.Add(WaitForMe(duration, 5));

            await Task.WhenAll(tasks);

            await new MessageDialog(String.Format("It took {0} seconds for all calls to finish", DateTime.Now.Subtract(dtStart).TotalSeconds)).ShowAsync();
        }

        static async Task WaitMany_CorrectWay_Twist(int duration)
        {
            DateTime dtStart = DateTime.Now;

            List<Task> tasks = new List<Task>();
            tasks.Add(WaitForMe(duration, 1));
            tasks.Add(WaitForMe(duration, 2));
            tasks.Add(WaitForMe(duration, 3));
            tasks.Add(WaitForMe(duration*duration, 4));
            tasks.Add(WaitForMe(duration, 5));

            await Task.WhenAny(tasks);

            await new MessageDialog(String.Format("At least one of the tasks finished execution in {0} seconds", DateTime.Now.Subtract(dtStart).TotalSeconds)).ShowAsync();

            await Task.WhenAll(tasks);

            await new MessageDialog(String.Format("All tasks finished execution in {0} seconds", DateTime.Now.Subtract(dtStart).TotalSeconds)).ShowAsync();
        }

        static async Task<string> GetWebData(bool throwException = false)
        {
            HttpClient client = new HttpClient();
            string data = await client.GetStringAsync(throwException ? "http://www.bbc.co.uking" : "http://www.bbc.co.uk");
            return data;
        }

        static async Task RecursiveWait_Works()
        {
            List<string> data = new List<string>();
            for (int i = 0; i < 10; i++)
            {
                HttpClient client = new HttpClient();
                data.Add(await client.GetStringAsync("http://www.bbc.co.uk"));
            }

            await new MessageDialog("all done").ShowAsync();
        }

        void CPUIntensiveWait(IAsyncAction async)
        {
            DateTime dtEnd = DateTime.Now.AddSeconds(10);
            //long l = long.MinValue;
            while (DateTime.Now < dtEnd) ;
                //l++;
        }

        void CPUIntensiveWait2()
        {
            DateTime dtEnd = DateTime.Now.AddSeconds(10);
            //long l = long.MinValue;
            while (DateTime.Now < dtEnd) ;
                //l++;
        }

        private async void btnWait1_Click(object sender, RoutedEventArgs e)
        {
            await WaitForMe(10, 1, true); // wait 10 seconds
        }

        private async void btnWait2_Click(object sender, RoutedEventArgs e)
        {
            await WaitMany_WrongWay(5);
        }

        private async void btnWait3_Click(object sender, RoutedEventArgs e)
        {
            await WaitMany_CorrectWay(5);
        }

        private async void btnWait4_Click(object sender, RoutedEventArgs e)
        {
            await WaitMany_CorrectWay_Twist(5);
        }

        private async void btnWait5_Click(object sender, RoutedEventArgs e)
        {
            await RecursiveWait_Works();
        }

        private async void btnWait6_Click(object sender, RoutedEventArgs e)
        {
            string data = await GetWebData();
            await new MessageDialog(data).ShowAsync();
        }

        private async void btnWait7_Click(object sender, RoutedEventArgs e)
        {
            string data = await GetWebData(true);
            await new MessageDialog(data).ShowAsync();
        }

        private async void btnWait8_Click(object sender, RoutedEventArgs e)
        {
            Task<string> t = GetWebData();
            
            int a = 1;
            int b = 2;
            int c = a + b;

            await new MessageDialog(String.Format("{0} + {1} = {2}", a, b, c)).ShowAsync();

            string data = await t;
            await new MessageDialog(data).ShowAsync();
        }

        private async void btnWait9_Click(object sender, RoutedEventArgs e)
        {
            Task<string> t = GetWebData(true);

            int a = 1;
            int b = 2;
            int c = a + b;

            await new MessageDialog(String.Format("{0} + {1} = {2}", a, b, c)).ShowAsync();

            string exception = null;
            try
            {
                string data = await t;
                await new MessageDialog(data).ShowAsync();
            }
            catch (Exception ex)
            {
                exception = ex.InnerException.Message;
            }
               
            if(!string.IsNullOrEmpty(exception))
                await new MessageDialog(exception).ShowAsync();
        }

        private async void btnWait10_Click(object sender, RoutedEventArgs e)
        {
            List<Task> tasks = new List<Task>();
            tasks.Add(ThreadPool.RunAsync(new WorkItemHandler(CPUIntensiveWait)).AsTask());
            await Task.Delay(1000);
            tasks.Add(ThreadPool.RunAsync(new WorkItemHandler(CPUIntensiveWait)).AsTask());
                
            await Task.WhenAll(tasks);

            await new MessageDialog("all tasks complete").ShowAsync();
        }

        private async void btnWait11_Click(object sender, RoutedEventArgs e)
        {
            List<Task> tasks = new List<Task>();
            tasks.Add(Task.Run(() => CPUIntensiveWait2()));
            await Task.Delay(1000);
            tasks.Add(Task.Run(() => CPUIntensiveWait2()));

            await Task.WhenAll(tasks);

            await new MessageDialog("all tasks complete").ShowAsync();
        }

        private async void btnWait12_Click(object sender, RoutedEventArgs e)
        {
            Task t = WaitForMe(2, 1);
            t.Wait();
            await new MessageDialog("all tasks complete").ShowAsync();
        }

        private async void btnWait13_Click(object sender, RoutedEventArgs e)
        {
            await Task.Run(() =>
            {
                List<Task> tasks = new List<Task>();
                tasks.Add(ThreadPool.RunAsync(new WorkItemHandler(CPUIntensiveWait)).AsTask());

                tasks.Add(ThreadPool.RunAsync(new WorkItemHandler(CPUIntensiveWait)).AsTask());
                Task.WaitAll(tasks.ToArray());
            });

            await new MessageDialog("all tasks complete").ShowAsync();
        }

        
    }
}
