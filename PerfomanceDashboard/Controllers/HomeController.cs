using System.Web.Mvc;
using PerfomanceDashboard.Models;

namespace PerfomanceDashboard.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            // → put all in ViewData
            ViewData.Clear();
            ViewData.Add("Backlog", Dashboard.backlog);
            ViewData.Add("UnassignedTicket", Dashboard.unassignedTicket);
            if (Dashboard.lastFhr >= 1)
            {
                ViewData.Add("FHR", Dashboard.lastFhr);
            }
            else
            {
                ViewData.Add("FHR", "-");
            }
            ViewData.Add("assignedWithStatusByPerson", Dashboard.assignedWithStatusByPerson);
            ViewData.Add("emplNamesShort", Dashboard.emplNamesShort);
            ViewData.Add("resolvedTicketPerWeekByUser", Dashboard.resolvedTicketPerWeekByUser);
            ViewData.Add("assignedToGroupByDayOfWeek", Dashboard.assignedToGroupByDayOfWeek);
            ViewData.Add("fhrTable", Dashboard.fhrTable);
            ViewData.Add("actionTrigger", Dashboard.actionTrigger);
            return PartialView();

        }

        public ActionResult Dispatcher()
        {
            ViewData.Clear();
            ViewData.Add("assignedWithStatusByPerson", Dashboard.assignedWithStatusByPerson);
            ViewData.Add("emplNamesShort", Dashboard.emplNamesShort);
            ViewData.Add("pereodical", Dashboard.pereodicalBody);
            return PartialView();
        }
    }
}