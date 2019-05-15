using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Database.Controllers
{
    public class ArtistVideoController : Controller
    {
        // Test with these:
        // OBSOLETE DATA:
        // http://localhost:51080/ArtistVideo/Get/
        // http://localhost:51080/ArtistVideo/Get/Artist%200
        // http://localhost:51080/ArtistVideo/Get/?title=Video%201%201%20title

        public string Get(string[] id = null)
        {
            // Instantiate my homemade JSONPath Services object.  It does all the work:
            JsonPath jsonPathServices = new JsonPath();

            // Get the arguments from the command line URL and prepare them for usage.  Pass
            // them to the JSONPath query and execute it.  Return the result in JSON form:
            string response = jsonPathServices.QueryJsonPathMain((HttpRequestWrapper)Request);

            // NOTE: The core JSON Path module has a bug for this data-type only.  Rather than
            // open that can of worms, I will simply patch the JSON:
            response = response.Replace("}]},", "},");

            return response;

        }

        // I might want to use these later:

        //// GET: ArtistVideo
        //public ActionResult Index()
        //{
        //    return View();
        //}

        //// GET: ArtistVideo/Details/5
        //public ActionResult Details(int id)
        //{
        //    return View();
        //}

        //// GET: ArtistVideo/Create
        //public ActionResult Create()
        //{
        //    return View();
        //}

        //// POST: ArtistVideo/Create
        //[HttpPost]
        //public ActionResult Create(FormCollection collection)
        //{
        //    try
        //    {
        //        // TODO: Add insert logic here

        //        return RedirectToAction("Index");
        //    }
        //    catch
        //    {
        //        return View();
        //    }
        //}

        //// GET: ArtistVideo/Edit/5
        //public ActionResult Edit(int id)
        //{
        //    return View();
        //}

        //// POST: ArtistVideo/Edit/5
        //[HttpPost]
        //public ActionResult Edit(int id, FormCollection collection)
        //{
        //    try
        //    {
        //        // TODO: Add update logic here

        //        return RedirectToAction("Index");
        //    }
        //    catch
        //    {
        //        return View();
        //    }
        //}

        //// GET: ArtistVideo/Delete/5
        //public ActionResult Delete(int id)
        //{
        //    return View();
        //}

        //// POST: ArtistVideo/Delete/5
        //[HttpPost]
        //public ActionResult Delete(int id, FormCollection collection)
        //{
        //    try
        //    {
        //        // TODO: Add delete logic here

        //        return RedirectToAction("Index");
        //    }
        //    catch
        //    {
        //        return View();
        //    }
        //}
    }
}
