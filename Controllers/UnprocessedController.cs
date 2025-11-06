using BDM_P.Data;
using BDM_P.Models;
using BDM_P.Services;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BDM_P.Controllers
{
    public class UnprocessedController : Controller
    {
        private readonly UnprocessedService s = new UnprocessedService();
        private readonly ProcessedService pSvc = new ProcessedService();

        [HttpGet]
        public ActionResult Upload()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Upload(int id, int? vidId)
        {
            var f = Request.Files.Count > 0 ? Request.Files[0] : null;
            if (f == null || f.ContentLength == 0) return new HttpStatusCodeResult(400, "No file uploaded");

            using (var ms = new MemoryStream())
            {
                f.InputStream.CopyTo(ms);
                s.Insert(id, ms.ToArray(), vidId);
            }
            return Content("OK");
        }

        public ActionResult Index()
        {
            var items = s.GetAll();
            return View(items);
        }

        public ActionResult Image(int id)
        {
            var data = s.GetImageById(id);
            if (data == null)
            {
                System.Diagnostics.Trace.TraceWarning($"Unprocessed image not found id={id}");
                return HttpNotFound();
            }

            try
            {
                Response.AppendHeader("X-Image-Size", data.Length.ToString());
                Response.ContentType = "image/jpeg";
            }
            catch { 
            
            }

            Response.AppendHeader("Access-Control-Allow-Origin", "*");
            return File(data, "image/jpeg");
        }


        [HttpGet]
        public ActionResult Process(int id)
        {
            ViewBag.UnprocessedId = id;
            return View();
        }

        [HttpPost]
        public ActionResult Process(int unprocessedId, int processedId)
        {
            var f = Request.Files.Count > 0 ? Request.Files[0] : null;
            if (f == null || f.ContentLength == 0) return new HttpStatusCodeResult(400, "No file uploaded");

            int? videoId = s.GetVideoIdByUnprocessedId(unprocessedId);

            using (var ms = new MemoryStream())
            {
                f.InputStream.CopyTo(ms);
                pSvc.Insert(processedId, ms.ToArray(), videoId);
            }
            return RedirectToAction("Index");
        }
    }
}
