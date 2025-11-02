using System;
using System.IO;
using System.Web;
using System.Web.Mvc;
using BDM_P.Services;

namespace BDM_P.Controllers
{
    public class ProcessedController : Controller
    {
        private readonly ProcessedService processedService = new ProcessedService();

        public ActionResult Index()
        {
            var items = processedService.GetAll();
            return View(items);
        }

        public ActionResult Image(int id)
        {
            var data = processedService.GetImageById(id);
            if (data == null)
            {
                System.Diagnostics.Trace.TraceWarning($"Processed image not found id={id}");
                return HttpNotFound();
            }

            try
            {
                Response.AppendHeader("X-Image-Size", data.Length.ToString());
                Response.ContentType = "image/jpeg";
            }
            catch { /* ignore set header errors */ }

            Response.AppendHeader("Access-Control-Allow-Origin", "*");
            return File(data, "image/jpeg");
        }

        // Upload now expects videoId to be passed (the linking id)
        [HttpPost]
        public ActionResult Upload(int processedId, int? videoId)
        {
            var f = Request.Files.Count > 0 ? Request.Files[0] : null;
            if (f == null || f.ContentLength == 0)
                return new HttpStatusCodeResult(400, "No file uploaded");

            using (var ms = new MemoryStream())
            {
                f.InputStream.CopyTo(ms);
                processedService.Insert(processedId, ms.ToArray(), videoId);
            }

            return Content("OK");
        }
    }
}
