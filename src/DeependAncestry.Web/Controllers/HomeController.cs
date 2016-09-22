using System;
using System.Collections.Generic;
using System.Web.Mvc;
using DeependAncestry.Web.Models;
using PagedList;
using DeependAncestry.Web.Interface;

namespace DeependAncestry.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger _logger;
        private readonly IAncestrySearch _ancestrySearch;

        public HomeController(ILogger logger, IAncestrySearch ancestrySearch)
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

            ViewBag.GenderList = genderList;

            return View();
        }

        [HttpPost]
        public ActionResult GetResults(SearchFieldsViewModel model, string gender, string name, int? page)
        {

            if (Request["Name"] != null && Request["Name"] != "")
            {
                name = Request["Name"];
                ViewBag.Name = name;
            }

            if (Request["Gender"] != null && Request["Gender"] != "")
            {
                gender = Request["Gender"];
                ViewBag.GenderList = gender;
            }

            List<Results> ancestryResults = new List<Results>();

            try
            {
                ancestryResults = _ancestrySearch.Search(name, gender, "");
            }
            catch (Exception ex)
            {
                _logger.Error("Unable to search ancestry database due to: " + ex.Message, ex);
            }

            if (ancestryResults.Count < 1)
                ViewBag.NoResults = "true";

            int pageSize = 10;
            int pageNumber = (page ?? 1);
            return PartialView("_ResultPartial", ancestryResults.ToPagedList(pageNumber, pageSize));
        }
    }
}