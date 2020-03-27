using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using System.IO;

namespace Bucket.Admin.Web.Controllers
{
    /// <summary>
    /// Home Index
    /// </summary>
    public class HomeController : Controller
    {
        /// <summary>
        /// Index
        /// </summary>
        /// <param name="hostingEnvironment"></param>
        /// <returns></returns>
        public IActionResult Index([FromServices] IHostingEnvironment hostingEnvironment)
        {
            using (var stream = System.IO.File.OpenRead(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "index.html")))
            {
                using (var reader = new StreamReader(stream))
                {
                    return Content(reader.ReadToEnd(), "text/html");
                }
            }
        }
    }
}