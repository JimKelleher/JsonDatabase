using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Database.Controllers
{
    public class ArtistController : Controller
    {
        public string xFULL_DATABASE;

        // Test with these:

        // ARGUMENT OPTION 1 OF 4:
        // http://localhost:51080/Artist/Get/
        // http://localhost:51080/Artist/Get
        // http://localhost:51080/Artist/get/
        // http://localhost:51080/Artist/get

        // ARGUMENT OPTION 2 OF 4:
        // http://localhost:51080/Artist/Get/Elton%20John

        // ARGUMENT OPTION 3 OF 4:
        // http://localhost:51080/Artist/Get/?id=Elton%20John
        // http://localhost:51080/Artist/Get/?id[0]=Elton%20John
        // http://localhost:51080/Artist/Get/?id[0]=Elton%20John&id[1]=Artist%201

        // ARGUMENT OPTION 4 OF 4:
        // http://localhost:51080/Artist/Get/?decade=1960&genre=ROCK

        // SPECIAL HANDLING:
        // NOTE: See document "artist database query encoding.txt":

        // If the Get version of a query crashes use the ?id= version:
        // http://localhost:51080/Artist/Get/?id=R.E.M.
        // http://localhost:51080/Artist/Get/?id=AC/DC

        // Required encoding/escaping:
        // http://localhost:51080/Artist/Get/?id=Sam%20%26%20Dave   // ampersand %26
        // http://localhost:51080/Artist/Get/?id=The%20Go-Go\\%27s  // apostrophe %27


        public string Get(string[] id = null)
        {
            // Instantiate my homemade JSONPath Services object.  It does all the work:
            JsonPath jsonPathServices = new JsonPath();

            // Get the arguments from the command line URL and prepare them for usage.  Pass
            // them to the JSONPath query and execute it.  Return the result in JSON form:

            string response = jsonPathServices.QueryJsonPathMain((HttpRequestWrapper)Request);

            return response;

        }

        // I might want to use these later:

        //// GET: Artist
        //public ActionResult Index()
        //{
        //    return View();
        //}

        //// GET: Artist/Details/5
        //public ActionResult Details(int id)
        //{
        //    return View();
        //}

        //// GET: Artist/Create
        //public ActionResult Create()
        //{
        //    return View();
        //}

        //// POST: Artist/Create
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

        //// GET: Artist/Edit/5
        //public ActionResult Edit(int id)
        //{
        //    return View();
        //}

        //// POST: Artist/Edit/5
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

        //// GET: Artist/Delete/5
        //public ActionResult Delete(int id)
        //{
        //    return View();
        //}

        //// POST: Artist/Delete/5
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
