using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using webMetrics.Models;

namespace webMetrics.Controllers
{
    public class HomeController : Controller
    {
        private readonly IOptions<Models.AppSettings> _settings;

        public HomeController(IOptions<Models.AppSettings> appSettings)
        {
            _settings = appSettings;
        }

        public IActionResult IndexOLD()
        {
            var UserId = HttpContext.Session.GetString("UserId");
            if (!string.IsNullOrEmpty(UserId))
            {
                var NomeAgencia = HttpContext.Session.GetString("nomeagencia");
                if (string.IsNullOrEmpty(NomeAgencia))
                {
                    return RedirectToAction("MinhasAnalises", "relatorios");
                }
                else
                {
                    return RedirectToAction("HistoricoMetricas", "relatorios");
                }
            }

            return View();
        }
        public IActionResult Index()
        {
            var UserId = HttpContext.Session.GetString("UserId");
            if (!string.IsNullOrEmpty(UserId))
            {
                var NomeAgencia = HttpContext.Session.GetString("nomeagencia");
                if (string.IsNullOrEmpty(NomeAgencia))
                {
                    return RedirectToAction("MinhasAnalises", "relatorios");
                }
                else
                {
                    return RedirectToAction("HistoricoMetricas", "relatorios");
                }
            }
            try
            {
                var repMongo = new Repository.MongoRep("", _settings);
                var crowlerInfo = repMongo.GetInfoHomeCrowler().ConfigureAwait(false).GetAwaiter().GetResult();
                ViewBag.CrowlerInfo = crowlerInfo;
            }
            catch (Exception x)
            {
            }
            HttpContext.Session.Remove("userType");
            HttpContext.Session.Remove("userNameTitle");
            return View();
        }

        public async Task<IActionResult> Resumo()
        {
            var repMongo = new Repository.MongoRep("", _settings);
            var resp = await repMongo.Listar<Models.DTO.InfluencersResumo>("danieljromualdo");
            var inf = resp.FirstOrDefault().Obj;

            #region Emotions
            var avgFaceDetection = inf.AvgFaceDetection;
            var listaFaceDetection = avgFaceDetection.Joy.ToString() + "," +
                avgFaceDetection.Sorrow.ToString() + "," +
                avgFaceDetection.Anger.ToString() + "," +
                avgFaceDetection.Surprise.ToString() + "";
            var cabecalhoFaceDetection = "'Alegre','Tristeza','Raiva','Surpresa'";
            ViewBag.CabecalhoFaceDetection = cabecalhoFaceDetection;
            ViewBag.ListaFaceDetection = listaFaceDetection;
            #endregion

            ViewBag.Markers = inf.Markers;

            return View(inf);
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public ActionResult SobreNos()
        {
            return View();
        }

        public ActionResult ComoFunciona()
        {
            return View();
        }

        public ActionResult Descubra()
        {
            return View();
        }

        public ActionResult PassoApasso()
        {
            return View();
        }

        public ActionResult InfluencersGuide()
        {
            return View();
        }

        public ActionResult Workshop()
        {
            return View();
        }

        public ActionResult ConsulteEngajamento()
        {
            return View();
        }

        public ActionResult EntendaaImportancia()
        {
            return View();
        }
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return base.View(new Models.ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
