using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
//using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace webMetrics.Controllers
{
    [Route("api/[controller]")]
    public class ExtController : ApiController
    {
        private readonly IOptions<Models.AppSettings> _appSettings;

        public ExtController(IOptions<Models.AppSettings> appSettings)
        {
            _appSettings = appSettings;
        }

        [HttpPost]
        public async Task<bool> GravarNovoUsuario([FromUri]string token, [FromUri]string idpagina,[FromUri]string userid)
        {
            try
            {
                Repository.MongoRep repMongo = new Repository.MongoRep("MetricaInsights", _appSettings);
                var usuarioNovo = new Models.Usuario()
                {
                    access_token_page = token,
                    name_page = "",
                    UsuarioInstagram = idpagina,
                    UserId = userid,
                    Tipo = "5",
                    AgenciaUserId = ""
                };
                await repMongo.GravarOne<Models.Usuario>(usuarioNovo);

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
