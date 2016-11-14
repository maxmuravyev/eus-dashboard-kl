using System.Web.Mvc;
using System.Web.Routing;
using PerfomanceDashboard.Models;
using System.Threading;

namespace PerfomanceDashboard
{

    public class BackgroundThread
    {

        public static void StartCheckingDb()
        {
            var thread = new Thread(new ThreadStart(StartJob));
            thread.IsBackground = true;
            thread.Name = "BackgroundChecker";
            thread.Start();
        }

        private static void StartJob()
        {
            var dbChecker = new DbChecker();
            var timer = new System.Timers.Timer();
            timer.Interval = 60000;
            timer.Elapsed += new System.Timers.ElapsedEventHandler(dbChecker.RequestToDb);
            timer.Enabled = true;
            // If AutoReset=false then the timer will only tick once
            timer.AutoReset = true;
            timer.Start();
        }

        private class DbChecker
        {
            internal void RequestToDb(object sender, System.Timers.ElapsedEventArgs e)
            {
                Dashboard.GetDataFromDb();
                Dashboard.ActionTrigger();
                Dashboard.pereodicalBody = Dashboard.ExchangeConnectorToServiceMailbox();
            }
        }
    }

    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            Dashboard.pereodicalBody = Dashboard.ExchangeConnectorToServiceMailbox();

            Dashboard.GetDataFromDb();
            BackgroundThread.StartCheckingDb(); // → checker per 60 seconds

            AreaRegistration.RegisterAllAreas();
            RouteConfig.RegisterRoutes(RouteTable.Routes);
        }

    }
}
