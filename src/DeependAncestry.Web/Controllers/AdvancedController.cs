using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using DeependAncestry.Web.Models;
using DeependAncestry.Web.Interface;

namespace DeependAncestry.Web.Controllers
{
    public class AdvancedController : Controller
    {
        private readonly ILogger _logger;
        private readonly IAncestrySearch _ancestrySearch;

        public AdvancedController(ILogger logger, IAncestrySearch ancestrySearch)
        {
            _logger = logger;
            _ancestrySearch = ancestrySearch;
        }

        public ActionResult Index()
        {
            // setup gender setting
            GenderViewModel[] genderList = {
                                                new GenderViewModel {Gender = "M", GenderName = "Male", IsSelected = ""},
                                                new GenderViewModel {Gender = "F", GenderName = "Female", IsSelected = ""}
                                            };

            // setup direction setting
            DirectionViewModel[] directionList = {
                                                new DirectionViewModel {Direction = "A", DirectionName = "Ancestors", IsSelected = ""},
                                                new DirectionViewModel {Direction = "D", DirectionName = "Descendants", IsSelected = ""}
                                            };

            ViewBag.GenderList = genderList;
            ViewBag.DirectionList = directionList;

            return View();
        }

        [HttpPost]
        public ActionResult GetResults(SearchFieldsViewModel model, string gender, string name, string direction)
        {

            if (Request["Name"] != null && Request["Name"] != "")
            {
                name = Request["Name"];
                ViewBag.Name = name;
            }

            if (Request["Gender"] != null && Request["Gender"] != "")
            {
                gender = Request["Gender"];
            }

            if (Request["direction"] != null && Request["direction"] != "")
            {
                direction = Request["direction"];
            }

            IEnumerable<Results> ancestryResults = new List<Results>();

            try
            {
                ancestryResults = _ancestrySearch.Search(name, gender, direction);
            }
            catch (Exception ex)
            {
                _logger.Error("Unable to search ancestry database due to: " + ex.Message, ex);
            }

            if (ancestryResults.Count() < 1)
                ViewBag.NoResults = "true";

            return PartialView("_ResultPartial", ancestryResults.Take(10));
        }
    }
}