using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Database.Controllers
{
    public class ArtistDiscogController : Controller
    {
        // Test with these:
        // OBSOLETE DATA:
        // http://localhost:51080/ArtistDiscog/Get/
        // http://localhost:51080/ArtistDiscog/Get/Artist%200
        // http://localhost:51080/ArtistDiscog/Get/?id=Artist%200
        // http://localhost:51080/ArtistDiscog/Get/?wikipedia=yyy
        // http://localhost:51080/ArtistDiscog/Get/?sectionId=Section%201%200
        // http://localhost:51080/ArtistDiscog/Get/?year=1991
        // http://localhost:51080/ArtistDiscog/Get/?albumId=Album%200%200%200

        public string Get(string[] id = null)
        {
            // Instantiate my homemade JSONPath Services object.  It does all the work:
            JsonPath jsonPathServices = new JsonPath();

            // Get the arguments from the command line URL and prepare them for usage.  Pass
            // them to the JSONPath query and execute it.  Return the result in JSON form:
            return jsonPathServices.QueryJsonPathMain((HttpRequestWrapper)Request);

        }

        // I might want to use these later:

        //// GET: ArtistDiscog
        //public ActionResult Index()
        //{
        //    return View();
        //}

        //// GET: ArtistDiscog/Details/5
        //public ActionResult Details(int id)
        //{
        //    return View();
        //}

        //// GET: ArtistDiscog/Create
        //public ActionResult Create()
        //{
        //    return View();
        //}

        //// POST: ArtistDiscog/Create
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

        //// GET: ArtistDiscog/Edit/5
        //public ActionResult Edit(int id)
        //{
        //    return View();
        //}

        //// POST: ArtistDiscog/Edit/5
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

        //// GET: ArtistDiscog/Delete/5
        //public ActionResult Delete(int id)
        //{
        //    return View();
        //}

        //// POST: ArtistDiscog/Delete/5
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
