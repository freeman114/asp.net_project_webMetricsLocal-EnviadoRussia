using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MoreLinq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using webMetrics.Models;

namespace webMetrics.Controllers
{
    public class AdminController : Controller
    {
        #region Variaveis
        private bool isSandbox { get; set; }
        public string redirectURI { get; set; }

        private readonly Models.AppSettings _appSettings;
        private readonly IOptions<Models.AppSettings> _settings;

        private readonly double valor1 = 12.5;
        private readonly double valor0 = 10.0;
        #endregion

        public AdminController(IOptions<Models.AppSettings> appSettings)
        {
            _settings = appSettings;
            _appSettings = appSettings.Value;
        }

        public async Task<ActionResult> Index()
        {
            var repMongo = new Repository.MongoRep("", _settings, "");
            var lstUsuarios = await repMongo.ListarUsersAtivos();
            var idUsuarios = lstUsuarios
                .Select(s => s._id.ToString()).Distinct().ToList();
            var idAgencias = lstUsuarios.Where(w => w.Obj.AgenciaUserId != null && !string.IsNullOrEmpty(w.Obj.AgenciaUserId))
                .Select(s => new BsonObjectId(new ObjectId(s.Obj.AgenciaUserId))).Distinct().ToList();
            var lstAgencias = await repMongo.ListarUsuarioByAgencia<Models.Usuario>(idAgencias);
            lstAgencias.ToList();
            var lstUsuariosInsights = await repMongo.ListGraphByUserIds<Models.Graph.Usuario>(idUsuarios);
            lstUsuariosInsights.ToList();
            var lstMediaInsights = await repMongo.ListGraphByUserIds<Models.Graph.Media>(idUsuarios);
            lstMediaInsights.ToList();
            var lstTagInsights = await repMongo.ListGraphByUserIds<Models.Graph.Tags>(idUsuarios);
            lstTagInsights.ToList();
            var lstStoriesInsights = await repMongo.ListGraphByUserIds<Models.Graph.Stories>(idUsuarios);
            lstStoriesInsights.ToList();
            var lstInsights = await repMongo.ListGraphByUserIds<Models.DTO.InsigthDTO>(idUsuarios);
            lstInsights.ToList();
            var lstCitiesInsights = await repMongo.ListGraphByUserIds<Models.Graph.InsightsGenderAge>(idUsuarios);
            lstCitiesInsights.ToList();
            int linhaerro;
            var lstPagamentos = await repMongo.ListGraphByUserIds<Models.DTO.PagamentoPage>(idUsuarios);
            lstPagamentos.ToList();

            var lst = new List<UsuarioAcoes>();
            lstUsuarios.Where(w => w.Obj.Tipo != "2").ForEach(f =>
                {
                    try
                    {
                        var usuarioInsight = lstUsuariosInsights.Where(w => w._id.ToString() == f._id).FirstOrDefault();
                        linhaerro = 0; ;
                        var agencia = lstAgencias.Where(w => w._id.ToString() == f.Obj.AgenciaUserId).FirstOrDefault();
                        linhaerro++;
                        var citiesInsights = lstCitiesInsights.Where(w => w.UsuarioId.ToString() == f._id.ToString()).FirstOrDefault();
                        linhaerro++;
                        var cityTop = citiesInsights == null ? null :
                                citiesInsights.Obj == null ? null : citiesInsights.Obj.data == null ? null : citiesInsights.Obj.data.FirstOrDefault();
                        linhaerro++;
                        var city = cityTop == null ? null : cityTop.values.FirstOrDefault();
                        linhaerro++;
                        var lstCity = city == null ? null : city.value.Select(s => new
                        {
                            valor = s.Value,
                            key = s.Key
                        });
                        linhaerro++;
                        var _maxCity = lstCity == null ? null : lstCity.Where(w => w.valor == lstCity.Max(m => m.valor)).FirstOrDefault();
                        var maxCity = (_maxCity == null ? null : _maxCity.key);
                        linhaerro++;
                        var ageInsights = lstInsights.Where(w => w.UsuarioId.ToString() == f._id.ToString()).FirstOrDefault();
                        linhaerro++;
                        var ageTop = ageInsights == null ? null : ageInsights.Obj.data.FirstOrDefault();
                        linhaerro++;
                        var age = ageTop == null ? null : ageTop.values.FirstOrDefault();
                        linhaerro++;
                        var lstAge = age == null ? null : age.value.Select(s => new
                        {
                            valor = s.valor,
                            key = s.name
                        });
                        linhaerro++;
                        var _maxAge = lstAge == null ? null : lstAge.Where(w => w.valor == lstAge.Max(m => m.valor)).FirstOrDefault();
                        var maxAge = (_maxAge == null ? null : _maxAge.key);
                        linhaerro++;

                        try
                        {
                            lst.Add(
                            new UsuarioAcoes()
                            {
                                UsuarioId = f._id.ToString(),
                                UserName = f.Obj.name_page +
                                    (
                                        usuarioInsight == null ? "" : usuarioInsight.Obj.username
                                    ),
                                Agencia = agencia == null ? "" : agencia.Obj.NomeAgencia,
                                Usuario = lstUsuariosInsights.Exists(x => x.UsuarioId == f._id.ToString()),
                                Media = lstMediaInsights.Exists(x => x.UsuarioId == f._id.ToString()),
                                Tags = lstTagInsights.Exists(x => x.UsuarioId == f._id.ToString()),
                                Stories = lstStoriesInsights.Exists(x => x.UsuarioId == f._id.ToString()),
                                CityTop = city == null ? "" : maxCity,
                                AgeTop = age == null ? "" : maxAge,
                                DataCriacao = f.DateCreation,
                                Status = ((f.Obj.Tipo == "4" && f.Obj.AgenciaUserId == "") ? "Removido" :
                                    (
                                    f.Obj.Tipo == "2" ? "Agência" : ""
                                    )
                                    )
                            });
                        }
                        catch (Exception)
                        {

                        }

                    }
                    catch (Exception)
                    {

                    }
                });

            return View(lst.OrderBy(o => o.UserName));
        }

        public async Task<ActionResult> Agencias()
        {

            var repMongo = new Repository.MongoRep("", _settings, "");
            try
            {
                var lstUsuarios = await repMongo.ListarAgencias();
                return View(lstUsuarios);
            }
            catch (Exception ex)
            {
                return RedirectToAction("Index");
            }
        }

        public async Task<ActionResult> Creditar(string id, int qtd)
        {
            var repMongo = new Repository.MongoRep("", _settings, "");
            var credito = new Models.CreditoMetricas()
            {
                UserId = id,
                Qtd = qtd,
                DataCredito = DateTime.Now,
                Debito = 0,
                DataValidade = DateTime.Now.AddMonths(1),
                DataCriacao = DateTime.Now
            };
            await repMongo.GravarOne<Models.CreditoMetricas>(credito);

            return RedirectToAction("Agencias");
        }
    }
}