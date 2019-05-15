using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Database.Controllers
{
    public class CodeController : Controller
    {
        public string xFULL_DATABASE;

        // Test with these:

        // ARGUMENT OPTION 1 OF 4:
        // http://localhost:51080/Code/Get/
        // http://localhost:51080/Code/Get
        // http://localhost:51080/Code/get/
        // http://localhost:51080/Code/get

        // ARGUMENT OPTION 2 OF 4:
        // NOTE: This form only works if the table key is named "id":
        // http://localhost:51080/Code/Get/ArtistDecade // doesn't work

        // ARGUMENT OPTION 3 OF 4:
        // http://localhost:51080/Code/Get/?codeId=ArtistDecade
        // http://localhost:51080/Code/Get/?codeId=ArtistDiscogStyle
        // http://localhost:51080/Code/Get/?codeId=ArtistDiscogYear

        // ARGUMENT OPTION 4 OF 4:
        // NOTE: This form only works if the field named "value" is numeric (ie, has no quotes):
        // http://localhost:51080/Code/Get/?codeId=ArtistDecade&value=1970
        // http://localhost:51080/Code/Get/?codeId=ArtistDecade&description=1970s

       public string Get(string[] id = null)
        {
            // Instantiate my homemade JSONPath Services object.  It does all the work:
            JsonPath jsonPathServices = new JsonPath();

            // Get the arguments from the command line URL and prepare them for usage.  Pass
            // them to the JSONPath query and execute it.  Return the result in JSON form:
            return jsonPathServices.QueryJsonPathMain((HttpRequestWrapper)Request);

        }

        // I might want to use these later:

        //// GET: Code
        //public ActionResult Index()
        //{
        //    return View();
        //}

        //// GET: Code/Details/5
        //public ActionResult Details(int id)
        //{
        //    return View();
        //}

        //// GET: Code/Create
        //public ActionResult Create()
        //{
        //    return View();
        //}

        //// POST: Code/Create
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

        //// GET: Code/Edit/5
        //public ActionResult Edit(int id)
        //{
        //    return View();
        //}

        //// POST: Code/Edit/5
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

        //// GET: Code/Delete/5
        //public ActionResult Delete(int id)
        //{
        //    return View();
        //}

        //// POST: Code/Delete/5
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
