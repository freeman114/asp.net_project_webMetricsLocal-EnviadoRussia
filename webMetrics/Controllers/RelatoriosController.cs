using InstaSharper.API;
using InstaSharper.API.Builder;
using InstaSharper.Classes;
using InstaSharper.Classes.Models;
using InstaSharper.Logger;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MoreLinq;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using Uol.PagSeguro.Domain;
using Uol.PagSeguro.Domain.Direct;
using Uol.PagSeguro.Exception;
using Uol.PagSeguro.Resources;
using Uol.PagSeguro.Service;
using webMetrics.Business;
using webMetrics.Models;
using webMetrics.Models.DTO;

namespace webMetrics.Controllers
{
    public class RelatoriosController : Controller
    {
        private bool isSandbox { get; set; }
        public string redirectURI { get; set; }

        private readonly Models.AppSettings _appSettings;
        private readonly IOptions<Models.AppSettings> _settings;

        private readonly double valor1 = 5.9;
        private readonly double valor0 = 2.1;

        public RelatoriosController(IOptions<Models.AppSettings> appSettings)
        {
            _settings = appSettings;
            _appSettings = appSettings.Value;
        }

        public ActionResult Index()
        {
            return RedirectToAction("Login");

            //if (HttpContext.Session.GetString("Message") != null)
            //{
            //    ViewBag.jsExecutar = HttpContext.Session.GetString("Message");
            //}
            ////HttpContext.Session.SetString("UserId", "admin");
            //return View();
        }

        public ActionResult Metrica(string id)
        {

            return View();
        }

        public async Task<ActionResult> Consultar()
        {
            var userId = HttpContext.Session.GetString("UserId");
            var tipo = HttpContext.Session.GetString("Tipo");
            var usuarioInstagram = HttpContext.Session.GetString("UsuarioInstagram");

            if (tipo == "1")
            {

                ////Processo de Instagram API
                //var redir = "";
                //redir = _appSettings.UrlInstagram;
                //ViewBag.jsExecutar = "window.location='" + redir + "';";


                if (string.IsNullOrEmpty(userId))
                {
                    ViewBag.Message = "Usuário não está logado <br /> Por favor faça seu login";
                    return RedirectToAction("Index");
                }
                else
                {
                    return await ConsultarInstagram(new Models.Usuario()
                    {
                        UsuarioInstagram = usuarioInstagram
                    });
                }
            }
            else
            {
                if (string.IsNullOrEmpty(userId))
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    return RedirectToAction("AutorizarMetrica");
                }
            }
        }

        private readonly IInstaApi _instaApi;
        public async Task<ActionResult> Create()
        {
            return View();
        }

        public ActionResult RefreshToken(string onde)
        {
            HttpContext.Session.Remove("instagram_business_account");
            HttpContext.Session.Remove("access_token_page");
            HttpContext.Session.Remove("name_page");

            return RedirectToAction(onde);
        }

        [HttpPost]
        public async Task<string> ChangePassword(string oldPass, string newPass)
        {
            var UserId = HttpContext.Session.GetString("UsuarioFull_id");
            Repository.MongoRep rep = new Repository.MongoRep(UserId, _settings, UserId);
            var feito = await rep.changePassword(new ObjectId(UserId), oldPass, newPass);
            if (!feito.Contains("#"))
            {
                //return "Configuracoes?msg=1";//Sucesso
                return "Perfil?msg=1";//Sucesso
            }
            else if (feito.Contains("#erro"))
            {
                //return "Configuracoes?msg=2";//Erro
                return "Perfil?msg=2";//Erro
            }
            else
            {
                //return "Configuracoes?msg=3";//usuario invalido
                return "Perfil?msg=3";//usuario invalido
            }
        }

        [HttpPost]
        public async Task<string> CancelarAssinatura()
        {
            var UserId = HttpContext.Session.GetString("UsuarioFull_id");
            Repository.MongoRep rep = new Repository.MongoRep(UserId, _settings, UserId);
            var feito = await rep.CancelarAssinatura(new ObjectId(UserId));
            if (!feito.Contains("#"))
            {
                //return "Configuracoes?msg=4";//Sucesso
                return "Perfil?msg=4";//Sucesso
            }
            else if (feito.Contains("#erro"))
            {
                //return "Configuracoes?msg=2";//Erro
                return "Perfil?msg=2";//Erro
            }
            else
            {
                //return "Configuracoes?msg=5";//usuario invalido
                return "Perfil?msg=5";//usuario invalido
            }
        }

        public ActionResult LoginOld(string msg = "", string id = "", string adicional = "")
        {
            HttpContext.Session.Remove("UserId");
            HttpContext.Session.Remove("UsuarioFull_id");
            HttpContext.Session.Remove("access_token_page");
            HttpContext.Session.Remove("name_page");
            HttpContext.Session.Remove("nomeagencia");
            HttpContext.Session.Remove("userType");
            HttpContext.Session.Remove("userNameTitle");
            HttpContext.Session.Remove("ProfilePicture");
            HttpContext.Session.Remove("nomeagencia");

            if (!string.IsNullOrEmpty(id))
            {
                Repository.MongoRep rep = new Repository.MongoRep("", _settings);
                var usuarioLogado = new Models.Usuario();

                var usuarioFull = rep.LoginId(new ObjectId(id)).Result;
                if (usuarioFull == null || (usuarioFull.Obj == null))
                {
                    msg = "99";
                }
                else
                {
                    usuarioLogado = usuarioFull.Obj;
                    HttpContext.Session.SetString("UserId", usuarioFull._id.ToString());// usuarioFull.Obj.Email);
                    HttpContext.Session.SetString("UsuarioFull_id", usuarioFull._id.ToString());
                    HttpContext.Session.SetString("access_token_page", usuarioLogado.access_token_page ?? "");
                    HttpContext.Session.SetString("name_page", usuarioLogado.name_page ?? "");

                    if (usuarioLogado.Tipo == "1")
                    {
                        return RedirectToAction("MinhasAnalises");
                    }
                    else if (usuarioLogado.Tipo == "2")
                    {
                        HttpContext.Session.SetString("instagram_business_account", usuarioLogado.UsuarioInstagram ?? "");
                        HttpContext.Session.SetString("nomeagencia", usuarioLogado.NomeAgencia ?? "");
                        return RedirectToAction("HistoricoMetricas");
                    }
                }
            }

            if (msg == "1")
            {
                ViewBag.ExecutarJS = "abrirModal('#senhainvalida','Seu login ou senha, estão incorretos. <br />Clique em ESQUECEU sua senha caso já tenha feito um cadastro anteriormente, ou<br />CADASTRE-SE. ')";
                ViewBag.Email = adicional;
            }
            if (msg == "2")
            {
                ViewBag.ExecutarJS = "abrirModal('#senhainvalida','Sua senha foi alterada com sucesso<br />sua nova senha foi enviada por email.')";
            }
            if (msg == "3")
            {
                ViewBag.ExecutarJS = "abrirModal('#senhainvalida','Não foi possível resetar sua senha<br />verifique se o email digitado está correto <br /> por favor tente novamente.')";
            }
            if (msg == "99")
            {
                ViewBag.ExecutarJS = "abrirModal('#senhainvalida','Faça seu login ou cadastre-se para prosseguir  <br /> e contratar seu plano.')";
            }
            return View();
        }

        public ActionResult Login(string msg = "", string id = "", string adicional = "")
        {
            HttpContext.Session.Remove("UserId");
            HttpContext.Session.Remove("UsuarioFull_id");
            HttpContext.Session.Remove("access_token_page");
            HttpContext.Session.Remove("name_page");
            HttpContext.Session.Remove("nomeagencia");
            HttpContext.Session.Remove("userType");
            HttpContext.Session.Remove("userNameTitle");
            HttpContext.Session.Remove("ProfilePicture");
            HttpContext.Session.Remove("nomeagencia");

            HttpContext.Session.Remove("ProfilePictureMidiakit");
            HttpContext.Session.Remove("NomeCompleto");
            HttpContext.Session.Remove("UserName");
            HttpContext.Session.Remove("SocialContext");

            if (!string.IsNullOrEmpty(id))
            {
                Repository.MongoRep rep = new Repository.MongoRep("", _settings);
                var usuarioLogado = new Models.Usuario();

                var usuarioFull = rep.LoginId(new ObjectId(id)).Result;
                if (usuarioFull == null || (usuarioFull.Obj == null))
                {
                    msg = "99";
                }
                else
                {
                    usuarioLogado = usuarioFull.Obj;
                    HttpContext.Session.SetString("UserId", usuarioFull._id.ToString());// usuarioFull.Obj.Email);
                    HttpContext.Session.SetString("UsuarioFull_id", usuarioFull._id.ToString());
                    HttpContext.Session.SetString("access_token_page", usuarioLogado.access_token_page ?? "");
                    HttpContext.Session.SetString("name_page", usuarioLogado.name_page ?? "");

                    if (usuarioLogado.Tipo == "1")
                    {
                        return RedirectToAction("MinhasAnalises");
                    }
                    else if (usuarioLogado.Tipo == "2")
                    {
                        HttpContext.Session.SetString("instagram_business_account", usuarioLogado.UsuarioInstagram ?? "");
                        HttpContext.Session.SetString("nomeagencia", usuarioLogado.NomeAgencia ?? "");
                        return RedirectToAction("HistoricoMetricas");
                    }
                }
            }

            if (msg == "1")
            {
                ViewBag.ExecutarJS = "abrirModal('#senhainvalida','Seu login ou senha, estão incorretos. <br />Clique em ESQUECEU sua senha caso já tenha feito um cadastro anteriormente, ou<br />CADASTRE-SE. ')";
                ViewBag.Email = adicional;
            }
            if (msg == "2")
            {
                ViewBag.ExecutarJS = "abrirModal('#senhainvalida','Sua senha foi alterada com sucesso<br />sua nova senha foi enviada por email.')";
            }
            if (msg == "3")
            {
                ViewBag.ExecutarJS = "abrirModal('#senhainvalida','Não foi possível resetar sua senha<br />verifique se o email digitado está correto <br /> por favor tente novamente.')";
            }
            if (msg == "99")
            {
                ViewBag.ExecutarJS = "abrirModal('#senhainvalida','Faça seu login ou cadastre-se para prosseguir  <br /> e contratar seu plano.')";
            }
            return View();
        }

        public async Task<ActionResult> MetricasOLD(string id)
        {
            var _id = new ObjectId(id);
            var UserId = HttpContext.Session.GetString("UserId");
            Repository.MongoRep repMongo = new Repository.MongoRep(UserId, _settings, UserId);
            //if (await ValorCredito(UserId, repMongo) == 0)
            //{
            //    ViewBag.Message = "Você ainda não tem créditos para prosseguir.";
            //    return null;
            //}

            var lstMongoUser = await repMongo.ListarById<Models.Graph.Usuario>(_id);
            var mongoUser = lstMongoUser.ToList();
            var userId = lstMongoUser.FirstOrDefault().UsuarioId;
            repMongo = new Repository.MongoRep(UserId, _settings, userId);

            return await LoadMetricas(mongoUser, repMongo, userId, "MetricasOLD");

        }
        public async Task<ActionResult> Metricas(string id)
        {
            try
            {
                var _id = new ObjectId(id);
                var UserId = HttpContext.Session.GetString("UserId");
                Repository.MongoRep repMongo = new Repository.MongoRep(UserId, _settings, UserId);
                
                var lstMongoUser = await repMongo.ListarById<Models.Graph.Usuario>(_id);
                var mongoUser = lstMongoUser.ToList();
                var userId = lstMongoUser.FirstOrDefault().UsuarioId;
                repMongo = new Repository.MongoRep(UserId, _settings, userId);
                ViewBag.IdPage = _id;

                return await LoadMetricas(mongoUser, repMongo, userId, "Metricas");
            }
            catch (Exception)
            {
                return RedirectToAction("Index", "Home");
            }
            
        }

        public async Task<ActionResult> ResetSenha(string Email)
        {
            Repository.MongoRep rep = new Repository.MongoRep("", _settings);
            var feito = await rep.resetSenha(Email);
            if (feito)
            {
                SenderEmail.ResetSenha(Email);
                return RedirectToAction("Login", new { msg = "2" });
            }
            else
            {
                return RedirectToAction("Login", new { msg = "3" });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(Models.Usuario usuario)
        {
            if (usuario == null)
            {
                return RedirectToAction("Login", new { msg = "1" });
            }

            if ((string.IsNullOrEmpty(usuario.Senha) || string.IsNullOrEmpty(usuario.Email)) &&
                (string.IsNullOrEmpty(usuario.TokenFacebook)))
            {
                return RedirectToAction("Login", new { msg = "1" });
            }

            try
            {
                Repository.MongoRep rep = new Repository.MongoRep("", _settings);
                var usuarioLogado = new Models.Usuario();
                if (!string.IsNullOrEmpty(usuario.TokenFacebook))
                {
                    var usuarioFull = await rep.LoginFacebook(usuario.UserId);
                    if (usuarioFull == null || (usuarioFull.Obj == null))
                    {
                        return await Cadastrar(usuario);
                    }
                    usuarioLogado = usuarioFull.Obj;
                    HttpContext.Session.SetString("UserId", usuarioFull._id.ToString());//usuarioFull.Obj.UserId);
                    HttpContext.Session.SetString("UsuarioFull_id", usuarioFull._id.ToString());
                    HttpContext.Session.SetString("ProfilePicture", LoadPictureProfile(usuarioFull._id.ToString(), rep, usuarioFull.Obj.Tipo).ConfigureAwait(false).GetAwaiter().GetResult());
                }
                else
                {
                    var usuarioFull = await rep.Login(usuario.Email, usuario.Senha);
                    if (usuarioFull == null || (usuarioFull.Obj == null))
                    {
                        return RedirectToAction("Login", new { msg = "1", adicional = usuario.Email });
                    }
                    usuarioLogado = usuarioFull.Obj;
                    HttpContext.Session.SetString("UserId", usuarioFull._id.ToString());//usuarioFull.Obj.Email);
                    HttpContext.Session.SetString("UsuarioFull_id", usuarioFull._id.ToString());
                    HttpContext.Session.SetString("ProfilePicture", LoadPictureProfile(usuarioFull._id.ToString(), rep, usuarioFull.Obj.Tipo).ConfigureAwait(false).GetAwaiter().GetResult());
                }

                HttpContext.Session.SetString("access_token_page", usuarioLogado.access_token_page ?? "");
                HttpContext.Session.SetString("name_page", usuarioLogado.name_page ?? "");
                HttpContext.Session.SetString("userNameTitle", usuarioLogado.name_page ?? "");

                if (usuarioLogado.Tipo == "1")
                {
                    HttpContext.Session.SetString("userType", "Influencer");
                    HttpContext.Session.SetString("userNameTitle", usuarioLogado.name_page ?? "");
                    //Verificar se existe o relatorio
                    //if (1 == 1)//se não possue o plano midia kit
                    //{
                    //    return RedirectToAction("PlanosInfluenciadores", "News");
                    //}
                    return RedirectToAction("MinhasAnalises");
                }
                else if (usuarioLogado.Tipo == "2")
                {
                    HttpContext.Session.SetString("userType", "Agency");
                    HttpContext.Session.SetString("instagram_business_account", usuarioLogado.UsuarioInstagram ?? "");
                    HttpContext.Session.SetString("nomeagencia", usuarioLogado.NomeAgencia ?? "");
                    HttpContext.Session.SetString("userNameTitle", usuarioLogado.NomeAgencia ?? "");
                    //Verificar se existe o relatorio
                    return RedirectToAction("HistoricoMetricas");
                }
                else
                {
                    return RedirectToAction("AutorizarMetrica");
                }
            }
            catch (Exception)
            {
                return RedirectToAction("Login", new { msg = "1" });
            }
        }

        private async static Task<IInstaApi> IniciarInstaSharper(string usuario, string senha,
            IOptions<Models.AppSettings> appSettings)
        {
            Repository.MongoRep rep = new Repository.MongoRep(usuario, appSettings);
            try
            {
                // create user session data and provide login details
                var userSession = new UserSessionData
                {
                    UserName = appSettings.Value.UsuarioInstagram,//"danieljromualdo",//usuario,
                    Password = appSettings.Value.SenhaInstagram//senha
                };

                // create new InstaApi instance using Builder
                var _instaApi = InstaApiBuilder.CreateBuilder()
                        .SetUser(userSession)

                        .UseLogger(new DebugLogger(LogLevel.All)) // use logger for requests and debug messages
                        .SetRequestDelay(TimeSpan.FromSeconds(2))
                        .Build();
                //var result = await rep.Listar<string>(usuario);
                //if (result != null && (result.Count() > 0))
                //{
                //    var contents = result.FirstOrDefault().Obj;
                //    byte[] byteArray = Encoding.UTF8.GetBytes(contents);
                //    MemoryStream _stream = new MemoryStream(byteArray);
                //    _instaApi.LoadStateDataFromStream(_stream);
                //}
                //if (!_instaApi.IsUserAuthenticated)
                //{
                var logInResult = await _instaApi.LoginAsync();
                if (!logInResult.Succeeded)
                {
                    Console.WriteLine($"Unable to login: {logInResult.Info.Message}");
                    throw new Exception("Informações do usuário estão inválidas <br /> Vá em seu perfil e complete suas informações de cadastro");
                }
                //}
                //var state = _instaApi.GetStateDataAsStream();
                //Stream stream = state;
                //byte[] bytes = new byte[stream.Length];
                //stream.Position = 0;
                //stream.Read(bytes, 0, (int)stream.Length);
                //string data = Encoding.ASCII.GetString(bytes); // this is you
                //await rep.GravarOne<string>(data);
                return _instaApi;
            }
            catch (Exception)
            {
                return null;
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Consultar(Models.Usuario usuario)
        {
            return await ConsultarInstagram(usuario);
        }

        public async Task<ActionResult> Planos(string msg="")
        {
            ViewBag.Plano = msg;
            return View();
        }
        public async Task<ActionResult> PlanosOld()
        {
            return View();
        }

        private async Task<ActionResult> ConsultarInstagram(Models.Usuario usuario)
        {
            string linhaErro = "0";
            List<string> lstErros = new List<string>();
            var userId = HttpContext.Session.GetString("UserId");
            try
            {
                //"alanismanu;alvarogarnero;biacorraleiro;guipaganini;leoshehtman;loumontenegrox;thelumagrothe;nataliaodl;thairinegarcia;aredead;jujusalimeni;pietrazigmundo;thafurtados;isabelaeing;roberta.marzolla;luizafrujuelli;camilafreireee;suhmarcelino;daniellepontes";
                //TODO:Listas                 
                var lstUsuarioConsultar = "alanismanu;arianne.botelho;bellabonato;biacorraleiro;carlolocatellii;daniellepontes;fernandatavares_official;gabepascoal;isabelaeing;isadoravieira_;jakbueno;jujusalimeni;leoshehtman;leticialedger;luizapanucci;thelumagrothe;nataliaodl;pollymcosta;thairinegarcia;thafurtados";
                //"biancabin_;carlosmarianoator;cauemoura;eaijundiai;kingsdotoque;hashtagjundiai;jornaldejundiai;juliolmachado;jumparkjundiai;jundiaipontocom;jundiaisecreto;pahdugrau;paulapossani;cidadedejundiai;jessyebe;_mamaris_;giulia.vitaa;danilomartho;jjuliao_;vithelibra;gabbigasparotti;juliana_guttner;connectblogger;danielacomitre;blogsuzymagalhaes;checkinvirtual;taniamadu;amantedofogao;alucinadasporbeleza;alynecervo;viviansakamoto1;blog_lolita;milenagolin;julio_coach_thai;bianca_ruiva;coradiz;meular.minhavida;edivaldobueno;japaograffiti;kikiimary;ariellyubirajara;tiajoecia;marcelopretin;andersonzanchin;bzinhani;chefjanainabarzanelli;dayannerios;segredosdocloset;andressapatelli;sescjundiai;tvtecjundiai;tribunadejundiai;victorbagybr";
                var insta = await IniciarInstaSharper(usuario.UsuarioInstagram, usuario.Senha, _settings);
                //TODO:Listas                 
                foreach (var itemusuario in lstUsuarioConsultar.Split(';'))
                {
                    //TODO:Listas                     
                    usuario.UsuarioInstagram = itemusuario;

                    var usuarioAutorizacaoMetrica = new Models.AutorizacaoMetrica
                    {
                        Key = Guid.NewGuid().ToString(),
                        UsuarioId = HttpContext.Session.GetString("UserId"),
                        DataCriacao = DateTime.Now,
                        UsuarioInstagram = usuario.UsuarioInstagram
                    };
                    usuario.Key = usuarioAutorizacaoMetrica.Key;

                    Repository.MongoRep repMongo = new Repository.MongoRep(usuario.UsuarioInstagram, _settings);
                    Repository.MongoRepLog repLog = new Repository.MongoRepLog(usuario.UsuarioInstagram, _settings);

                    //TODO:Listas                     
                    await repMongo.GravarOne<Models.AutorizacaoMetrica>(usuarioAutorizacaoMetrica);

                    if (insta == null)
                    {
                        ViewBag.jsExecutar = "alert('Sua senha não confere.<br />Por favor digite sua senha corretamente');";
                        return RedirectToAction("AutorizarMetricaError", "relatorios", new Models.DTO.Erro() { Message = "Sua senha não confere.<br />Por favor digite sua senha corretamente" });
                    }

                    try
                    {
                        var lstUserMedia = await insta.GetUserMediaAsync(usuario.UsuarioInstagram.ToString().ToLower(),
                            PaginationParameters.MaxPagesToLoad(2));
                        await repMongo.GravarOne<InstaMediaList>(lstUserMedia.Value);
                        linhaErro = "UserMedia";
                        if (lstUserMedia.Succeeded == false)
                        {
                            ViewBag.Message = "Possivelmente você precisa revalidar o token<br /> Clique aqui para atualizar.";
                            //return RedirectToAction("AutorizarMetricaError", "relatorios", new Models.DTO.Erro() { Message = "Usuario privado ou outros problemas." });
                            //TODO:Listas                             
                            lstErros.Add(itemusuario);
                        }
                        else
                        {
                            var item = await Debitar(userId, 1, repMongo);

                            await repLog.GravarOne<Models.DTO.LogAcao>(new Models.DTO.LogAcao("ConsultarInstagram", linhaErro));
                            var lstMentionsTags = await insta.GetUserTagsAsync(usuario.UsuarioInstagram.ToString().ToLower(),
                                PaginationParameters.MaxPagesToLoad(2));
                            await repMongo.GravarOne<InstaMediaList>(lstMentionsTags.Value);
                            linhaErro = "UserTags";
                            await repLog.GravarOne<Models.DTO.LogAcao>(new Models.DTO.LogAcao("ConsultarInstagram", linhaErro));
                            var user = await insta.GetUserAsync(usuario.UsuarioInstagram.ToString().ToLower());
                            await repMongo.GravarOne<InstaUser>(user.Value);
                            linhaErro = "User";
                            await repLog.GravarOne<Models.DTO.LogAcao>(new Models.DTO.LogAcao("ConsultarInstagram", linhaErro));
                            var lstFollowing = await insta.GetUserFollowingAsync(usuario.UsuarioInstagram.ToString().ToLower(),
                                PaginationParameters.MaxPagesToLoad(1));
                            await repMongo.GravarOne<InstaUserShortList>(lstFollowing.Value);
                            linhaErro = "UserFollowing";
                            await repLog.GravarOne<Models.DTO.LogAcao>(new Models.DTO.LogAcao("ConsultarInstagram", linhaErro));
                            #region Group Comments Unreal
                            try
                            {
                                if (lstUserMedia != null)
                                {
                                    List<Task> lstTsk = new List<Task>();
                                    foreach (var it in lstUserMedia.Value.Where(w => w.CommentsCount != "0").OrderByDescending(x => x.TakenAt).Take(5))
                                    {
                                        if (it.CommentsCount != "0")
                                        {
                                            lstTsk.Add(AddMyComments(insta, repMongo, it));
                                        }
                                    }
                                    Task.WaitAll(lstTsk.ToArray());
                                    linhaErro = "GroupCommentsUnrealUseMedias";

                                    var lstUsersComentarios = await repMongo.Listar<InstaCommentList>(usuario.UsuarioInstagram);
                                    var lstobjUsersComentaristas = lstUsersComentarios.Where(r => r.timeSpan == repMongo.GetTimeSpan());
                                    var lstUsuariosComentaristas = new List<string>();
                                    foreach (var itMed in lstobjUsersComentaristas)
                                    {
                                        foreach (var itComm in itMed.Obj.Comments)
                                        {
                                            var itUsr = itComm.User;
                                            lstUsuariosComentaristas.Add(itUsr.UserName);
                                        }
                                    }
                                    linhaErro = "GroupCommentsUnrealobjcomentaristas";
                                    await repLog.GravarOne<Models.DTO.LogAcao>(new Models.DTO.LogAcao("ConsultarInstagram", linhaErro));
                                    List<Task> lstTskUsers = new List<Task>();
                                    var lstComentariosTerceiros = new ConcurrentBag<InstaCommentList>();
                                    foreach (var itUser in lstUsuariosComentaristas.DistinctBy(x => x))//Todos os comentaristas
                                    {
                                        var lstMediasUser = await insta.GetUserMediaAsync(itUser, PaginationParameters.MaxPagesToLoad(1));
                                        if (lstMediasUser.Succeeded)
                                        {
                                            foreach (var it in lstMediasUser.Value.Where(w => w.CommentsCount != "0").OrderByDescending(x => x.TakenAt).Take(5))//Todas as midias do comentarista
                                            {
                                                if (it.CommentsCount != "0")
                                                {
                                                    lstTskUsers.Add(AddTerceirosComments(insta, lstComentariosTerceiros, it));
                                                }
                                            }
                                        }
                                    }
                                    Task.WaitAll(lstTskUsers.ToArray());
                                    linhaErro = "GroupCommentsUnrealTerceiros";
                                    await repLog.GravarOne<Models.DTO.LogAcao>(new Models.DTO.LogAcao("ConsultarInstagram", linhaErro));
                                    await repMongo.GravarOne<ConcurrentBag<InstaCommentList>>(lstComentariosTerceiros);

                                    if (lstComentariosTerceiros.Count > 0)
                                    {
                                        var lstComentariosTerceirosList = lstComentariosTerceiros.ToList();
                                        var lstBlack = new List<string>();
                                        foreach (var itComentarioTerceiro in lstComentariosTerceirosList)
                                        {
                                            if (itComentarioTerceiro.Comments != null)
                                            {
                                                if (itComentarioTerceiro.Comments.Count > 0)
                                                {
                                                    if (itComentarioTerceiro.Caption != null)
                                                    {
                                                        linhaErro = "GroupCommentsUnreal1";
                                                        await repLog.GravarOne<Models.DTO.LogAcao>(new Models.DTO.LogAcao("ConsultarInstagram", linhaErro));
                                                        var usuarioTerceiro = itComentarioTerceiro.Caption.User.UserName;
                                                        var profilePicture = itComentarioTerceiro.Caption.User.ProfilePicture;
                                                        var existReferencia = false;
                                                        var existReferenciaCounts = 0;

                                                        linhaErro = "GroupCommentsUnreal2";
                                                        await repLog.GravarOne<Models.DTO.LogAcao>(new Models.DTO.LogAcao("ConsultarInstagram", linhaErro));
                                                        existReferencia = itComentarioTerceiro.Comments.Where(e => e.User.UserName == usuario.UsuarioInstagram).Count() > 0;
                                                        if (existReferencia)
                                                        {
                                                            existReferenciaCounts = itComentarioTerceiro.Comments.Count(e => e.User.UserName == usuario.UsuarioInstagram);
                                                        }
                                                        linhaErro = "GroupCommentsUnreal3";
                                                        await repLog.GravarOne<Models.DTO.LogAcao>(new Models.DTO.LogAcao("ConsultarInstagram", linhaErro));
                                                        existReferencia = itComentarioTerceiro.Comments.Exists(e => e.User.UserName == usuario.UsuarioInstagram);

                                                        if (existReferencia) lstBlack.Add(
                                                            usuario.UsuarioInstagram + "|" + usuarioTerceiro + "|" +
                                                            existReferenciaCounts.ToString() + "|" + profilePicture);
                                                        linhaErro = "GroupCommentsUnreal4";
                                                        await repLog.GravarOne<Models.DTO.LogAcao>(new Models.DTO.LogAcao("ConsultarInstagram", linhaErro));
                                                    }
                                                }
                                            }
                                        }
                                        if (lstBlack.Count > 0)
                                        {
                                            await repMongo.GravarOne<List<string>>(lstBlack.OrderBy(x => x).ToList());
                                        }
                                        linhaErro = "GroupCommentsUnreal";
                                        await repLog.GravarOne<Models.DTO.LogAcao>(new Models.DTO.LogAcao("ConsultarInstagram", linhaErro));
                                    }
                                }
                            }
                            catch
                            {
                                await repMongo.GravarOne<string>("Exception no unreal: " + linhaErro);
                                await repLog.GravarOne<Models.DTO.LogAcao>(new Models.DTO.LogAcao("ConsultarInstagram", linhaErro));
                            }
                            #endregion

                            var lstHashWithCaption = lstUserMedia.Value.Where(c => c.Caption != null);
                            var lstHashTags = lstHashWithCaption.Where(w => w.Caption.Text != null)
                                .Select(s => new Models.InstaMediaHash()
                                {
                                    InstaMedia = s,
                                    Hashs = SplitHash(s.Caption.Text.ToString().ToLower())
                                }).ToList();
                            await repMongo.GravarOne<List<Models.InstaMediaHash>>(lstHashTags);
                            linhaErro = "lstHashTags";
                            await repLog.GravarOne<Models.DTO.LogAcao>(new Models.DTO.LogAcao("ConsultarInstagram", linhaErro));

                            await GerarAnnotation(usuario, linhaErro, repMongo, lstUserMedia, repLog);

                            var lstPower = lstMentionsTags.Value.Take(5);
                            #region Calculo de Engajamento 
                            var engComentarios = 0;
                            lstPower.ForEach(x =>
                            {
                                engComentarios += (Convert.ToInt32(x.CommentsCount));
                            });
                            var engCurtidas = lstPower.Sum(x => x.LikesCount);

                            var mediaEngaj = Convert.ToDouble(engComentarios + engCurtidas) / Convert.ToDouble(5);
                            var mediaPercent = (mediaEngaj / user.Value.FollowersCount) * 100;

                            var _percentAvg = Math.Round(mediaPercent, 2);
                            var _aprovado = 0;
                            if (mediaPercent < valor0)
                            {
                                _aprovado = 0;
                            }
                            else if (mediaPercent < valor1)
                            {
                                _aprovado = 1;
                            }
                            else
                            {
                                _aprovado = 2;
                            }
                            usuarioAutorizacaoMetrica.PowerFull = _percentAvg;
                            usuarioAutorizacaoMetrica.Seguidores = user.Value.FollowersCount;
                            usuarioAutorizacaoMetrica.Aprovado = _aprovado;
                            await repMongo.AtualizarAutorizacaoMetrica(usuario.Key, usuarioAutorizacaoMetrica);
                            #endregion
                        }
                    }
                    catch (Exception ex)
                    {
                        ViewBag.jsExecutar = "alert('Problemas ao processar consultaHouve algum problemas ao processar consulta<br />Por favor aguarde 3 minutos e tente novamente.');";
                        await repMongo.GravarOne<string>(linhaErro + ":: " + ex.StackTrace.ToString());
                        //return RedirectToAction("AutorizarMetricaError", "relatorios", new Models.DTO.Erro() { Message = "Problemas ao processar consulta." });
                        //TODO:Listas                         
                        lstErros.Add(itemusuario);
                    }
                }

                return RedirectToAction("Visualizar", usuario);
            }
            catch (Exception ex)
            {
                ViewBag.jsExecutar = "alert('Problema: '" + linhaErro + ": : " + ex.Message.ToString() + "');";
                return RedirectToAction("AutorizarMetricaError", "relatorios", new Models.DTO.Erro() { Message = "Problemas: '" + linhaErro + ": : " + ex.Message.ToString() });
            }
        }

        private static async Task AddTerceirosComments(IInstaApi insta, ConcurrentBag<InstaCommentList> lstComentariosTerceiros, InstaMedia it)
        {
            try
            {
                var lstComments = await insta.GetMediaCommentsAsync(it.InstaIdentifier, PaginationParameters.MaxPagesToLoad(1));
                lstComentariosTerceiros.Add(lstComments.Value);
            }
            catch
            {
            }
        }

        private static async Task AddMyComments(IInstaApi insta, Repository.MongoRep repMongo, InstaMedia it)
        {
            try
            {
                var lstComentarios = await insta.GetMediaCommentsAsync(it.InstaIdentifier, PaginationParameters.MaxPagesToLoad(1));
                await repMongo.GravarOne<InstaCommentList>(lstComentarios.Value);
            }
            catch
            {
            }
        }

        private async Task GerarAnnotation(Models.Usuario usuario, string linhaErro,
            Repository.MongoRep repMongo, IResult<InstaMediaList> lstUserMedia,
            Repository.MongoRepLog repLog)
        {
            linhaErro = "GerarAnnotation";
            try
            {
                var lstFaceDetection = new List<Models.FaceDetection>();
                if (lstUserMedia.Value.Count > 0)
                {
                    foreach (var itUserMedia in lstUserMedia.Value.OrderByDescending(x => x.TakenAt).Take(10))
                    {
                        if (itUserMedia.Images.Count() == 0)
                        {
                            if (itUserMedia.Carousel.Count() > 0)
                            {
                                foreach (var itCarousel in itUserMedia.Carousel)
                                {
                                    var itImage = itCarousel.Images.FirstOrDefault();
                                    linhaErro = "itImage";
                                    await repLog.GravarOne<Models.DTO.LogAcao>(new Models.DTO.LogAcao("ConsultarInstagram", linhaErro));
                                    var facedetection = await getAnnotation(itImage.URI);
                                    linhaErro = "faceDetection";
                                    await repLog.GravarOne<Models.DTO.LogAcao>(new Models.DTO.LogAcao("ConsultarInstagram", linhaErro));
                                    if (facedetection != null) lstFaceDetection.Add(facedetection);
                                }
                            }
                        }
                        else
                        {
                            linhaErro = "elseNotattion";
                            var itImage = itUserMedia.Images.FirstOrDefault();
                            var facedetection = await getAnnotation(itImage.URI);
                            linhaErro = "getAnnotation";
                            if (facedetection != null) lstFaceDetection.Add(facedetection);
                        }
                    }

                    lstFaceDetection.ForEach(x => x.UserName = usuario.UsuarioInstagram);
                    await repMongo.GravarOne<List<Models.FaceDetection>>(lstFaceDetection);
                    linhaErro = "facedetectionFim";
                    await repLog.GravarOne<Models.DTO.LogAcao>(new Models.DTO.LogAcao("ConsultarInstagram", linhaErro));
                }
            }
            catch (Exception ex)
            {
                await repMongo.GravarOne<string>("GerarAnnotation::" + ex.Message.ToString() + "::" + ex.StackTrace.ToString());
                await repLog.GravarOne<Models.DTO.LogAcao>(new Models.DTO.LogAcao("ConsultarInstagram", linhaErro));
            }
        }

        private List<string> SplitHash(string texto)
        {
            try
            {
                List<string> _hashs = new List<string>();
                string text = ((texto == null) ? "" : texto);
                var regex = new Regex(@"(?<=#)\w+");
                var matches = regex.Matches(text);

                foreach (Match m in matches)
                {
                    _hashs.Add(m.Value.ToString());
                }
                return _hashs;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private List<string> SplitMentions(string texto)
        {
            try
            {
                List<string> _hashs = new List<string>();
                string text = ((texto == null) ? "" : texto);
                var regex = new Regex(@"(?<=@)\w+");
                var matches = regex.Matches(text);

                foreach (Match m in matches)
                {
                    _hashs.Add(m.Value.ToString());
                }
                return _hashs;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<ActionResult> Visualizar(Models.Usuario usuario)
        {

            return await VisualizarConsulta(usuario);
            //return await CreateVisualizar(usuario);
        }

        private async Task<ActionResult> VisualizarConsulta(Models.Usuario usuario)
        {
            usuario.DataCriacao = DateTime.Now;
            var linhaerro = "";

            try
            {
                #region Repositorio
                linhaerro = "Repositorio";
                Repository.MongoRep repMongo = new Repository.MongoRep(usuario.UsuarioInstagram, _settings);
                await repMongo.GravarOne<Models.Usuario>(usuario);
                var inf = new Models.DTO.InfluencersResumo();

                var lstMongoUser = await repMongo.ListarByUsuarioInstagram<InstaUser>(usuario.UsuarioInstagram);
                var mongoUser = lstMongoUser.Where(w => w.timeSpan == lstMongoUser.Max(x => x.timeSpan)).ToList();
                var objUser = mongoUser.FirstOrDefault().Obj;

                var lstMongoFollowing = await repMongo.ListarByUsuarioInstagram<InstaUserShortList>(usuario.UsuarioInstagram);
                var mongoFollowing = lstMongoFollowing.Where(w => w.timeSpan == lstMongoFollowing.Max(x => x.timeSpan)).ToList();
                var objFollowing = mongoFollowing.FirstOrDefault().Obj;

                var lstMongoMedias = await repMongo.ListarByUsuarioInstagram<InstaMediaList>(usuario.UsuarioInstagram);
                var mongoMedias = lstMongoMedias.Where(w => w.timeSpan == lstMongoMedias.Max(x => x.timeSpan)).ToList();

                var faceDetection = await repMongo.ListarByUsuarioInstagram<List<Models.FaceDetection>>(usuario.UsuarioInstagram);
                var lstFaceDetection = faceDetection.Where(w => w.timeSpan == faceDetection.Max(x => x.timeSpan)).ToList();
                var countFaceDetection = lstFaceDetection.FirstOrDefault().Obj.Count;

                var lstObjCommentsUnreal = await repMongo.ListarByUsuarioInstagram<List<string>>(usuario.UsuarioInstagram);
                var objCommentsUnreal = lstObjCommentsUnreal.Where(w => w.timeSpan == lstObjCommentsUnreal.Max(x => x.timeSpan)).ToList();
                var lstCommentsUnreal = objCommentsUnreal.OrderByDescending(o => o.timeSpan).ToList().FirstOrDefault();

                var insigths = await repMongo.ListarByUsuarioInstagram<Models.DTO.InsigthDTO>(usuario.UsuarioInstagram);
                var lstInsigthsAge = insigths.ToList();
                var lstObjAges = lstInsigthsAge.Select(s => new
                {
                    data = s.Obj.data.FirstOrDefault(),
                    timeSpan = s.timeSpan
                }
                ).ToList();
                var lstAgesFull = lstObjAges.Where(w => w.data.title.Contains("Gender"));
                var lstAge = lstAgesFull.Where(w => w.timeSpan == lstAgesFull.Max(m => m.timeSpan)).FirstOrDefault();

                var lstInsigthsCities = insigths.ToList();
                var lstObjCities = lstInsigthsCities.Select(s => new
                {
                    data = s.Obj.data.FirstOrDefault(),
                    timeSpan = s.timeSpan
                }
                ).ToList();
                var lstCitiesFull = lstObjCities.Where(w => w.data.title.Contains("City"));
                var lstCities = lstCitiesFull.Where(w => w.timeSpan == lstCitiesFull.Max(m => m.timeSpan)).FirstOrDefault();

                #endregion

                var objMinhasMidias = mongoMedias
                                    .Where(x => x.Obj.FirstOrDefault().User.UserName == usuario.UsuarioInstagram)
                                    .FirstOrDefault();

                var objMinhasMencoes = mongoMedias.Where(w => w.Obj != null && (w.Obj.Count() > 0))
                                    .Where(x => x.Obj.FirstOrDefault().User.UserName != usuario.UsuarioInstagram)
                                    .FirstOrDefault();

                var objEngaj = objMinhasMidias.Obj.OrderByDescending(x => x.TakenAt).Take(5).ToList();

                #region Sumario
                linhaerro = "Sumario";
                inf.Seguidores = objUser.FollowersCount;
                inf.Seguindo = objFollowing.Count();
                inf.SeguindoSeguidores = inf.Seguindo / (decimal)inf.Seguidores * 100;
                inf.SeguidoresUnicos = inf.Seguidores - objUser.MutualFollowers;
                inf.Posts = objMinhasMidias.Obj.Count();
                inf.Curtidas = objMinhasMidias.Obj.Sum(x => x.LikesCount);
                inf.ProfilePicture = objUser.ProfilePicture;
                inf.NomeCompleto = objUser.FullName;
                inf.UserName = objUser.UserName;
                inf.SocialContext = objUser.SocialContext;

                var comentarios = 0;
                objMinhasMidias.Obj.ForEach(x =>
                {
                    comentarios += (Convert.ToInt32(x.CommentsCount));
                });
                inf.Comentarios = comentarios;

                inf.avgPostReach = Math.Round(Convert.ToDouble(inf.Curtidas + inf.Comentarios) / objMinhasMidias.Obj.Count);

                inf.MediaCurtidas = inf.Curtidas / inf.Posts;
                inf.MediaComentarios = inf.Comentarios / (decimal)inf.Posts;
                inf.ComentariosSeguidores = (inf.MediaComentarios / inf.Seguidores) * 100;
                inf.Engajamento = (
                    (inf.Curtidas + (decimal)inf.Comentarios) / inf.Posts) / inf.Seguidores * 100;
                inf.Alcance = inf.Curtidas + inf.Comentarios;
                inf.MediaAlcancePost = inf.Posts / (decimal)inf.Alcance * 100;
                inf.Aprovado = 2;

                #endregion

                #region Calculo de Engajamento 
                linhaerro = "Engajamento";
                var engComentarios = 0;
                objEngaj.ForEach(x =>
                {
                    engComentarios += (Convert.ToInt32(x.CommentsCount));
                });
                var engCurtidas = objEngaj.Sum(x => x.LikesCount);

                var mediaEngaj = Convert.ToDouble(engComentarios + engCurtidas) / Convert.ToDouble(5);
                var mediaPercent = (mediaEngaj / inf.Seguidores) * 100;

                inf.percentAvg = Math.Round(mediaPercent, 2);
                if (mediaPercent < valor0)
                {
                    inf.Aprovado = 0;
                }
                else if (mediaPercent < valor1)
                {
                    inf.Aprovado = 1;
                }
                else
                {
                    inf.Aprovado = 2;
                }
                #endregion

                #region Minha menções
                linhaerro = "MinhaMencoes";
                if (objMinhasMencoes != null)
                {
                    var lstMinhasMencoes = objMinhasMencoes.Obj.ToList();
                    var lstMentions = lstMinhasMencoes.Select(x => new Models.DTO.InstaMentions()
                    {
                        UserName = "@" + x.User.UserName.ToLower(),
                        Used = Math.Round(Convert.ToDouble
                        (
                            Convert.ToDouble(lstMinhasMencoes.Count(c => c.User.UserName == x.User.UserName))
                        )),
                        UsedPerc = Math.Round(Convert.ToDouble
                        (
                                (
                                    Convert.ToDouble(lstMinhasMencoes.Count(c => c.User.UserName == x.User.UserName))
                                /
                                    Convert.ToDouble(lstMinhasMencoes.Count())
                                )
                        ) * 100, 4),
                        Reach = lstMinhasMencoes.Where(c => c.User.UserName == x.User.UserName)
                            .Sum(s => Convert.ToInt32(s.CommentsCount) + s.LikesCount),
                        Engagemer = Math.Round(
                                    Convert.ToDouble(
                                    (Convert.ToDouble(
                                            lstMinhasMencoes.Where(c => c.User.UserName == x.User.UserName)
                                            .Sum(s => Convert.ToInt32(s.CommentsCount))
                                        ) +
                                        Convert.ToDouble(
                                            lstMinhasMencoes.Where(c => c.User.UserName == x.User.UserName)
                                                .Sum(s => Convert.ToInt32(s.LikesCount))
                                            )
                                    )
                                    /
                                    Convert.ToDouble(inf.Seguidores))
                                         * 100, 4),
                        DiffUsedEngag = 1,
                        Imagens = new List<string>()
                        {
                            (   x.Images.Count==0?
                                (
                                    x.Carousel.Count>0?
                                    x.Carousel.FirstOrDefault().Images.FirstOrDefault().URI.ToString()
                                    :"")
                                :x.Images.FirstOrDefault().URI.ToString()
                                )
                        }
                    }).ToList();
                    inf.LstInstaMentions = lstMentions.DistinctBy(d => d.UserName)
                        .ToList();
                }
                #endregion

                #region Minha midias
                linhaerro = "Minha midias";
                var lstMidiasT = objMinhasMidias.Obj.ToList();
                var lstMinhasMidias = lstMidiasT.Where(r => r.Caption != null)
                    .Where(z => z.Caption.Text != "" && !string.IsNullOrEmpty(z.CommentsCount)).ToList()
                    .Where(r => r.Images != null).ToList();

                var lstMidias = lstMinhasMidias
                    .Select(x => new Models.DTO.InstaMentions()
                    {
                        UserName = "" + x.Caption.Text.ToString(),
                        Used = Math.Round(Convert.ToDouble
                    (
                        Convert.ToDouble(lstMinhasMidias.Count(c => c.Caption.Text == x.Caption.Text))
                    ), 0),
                        UsedPerc = Math.Round(Convert.ToDouble
                    (
                            (
                                Convert.ToDouble(lstMinhasMidias.Count(c => c.Caption.Text == x.Caption.Text))
                            /
                                Convert.ToDouble(lstMinhasMidias.Count())
                            )
                    ) * 100, 4),
                        Reach = lstMinhasMidias.Where(c => c.Caption.Text == x.Caption.Text)
                        .Sum(s => Convert.ToInt32(s.CommentsCount) + s.LikesCount),
                        Engagemer = Math.Round(
                                Convert.ToDouble
                                (
                                    (Convert.ToDouble(
                                            lstMinhasMidias.Where(c => c.Caption.Text == x.Caption.Text)
                                            .Sum(s => Convert.ToInt32(s.CommentsCount))
                                        ) +
                                        Convert.ToDouble(
                                            lstMinhasMidias.Where(c => c.Caption.Text == x.Caption.Text)
                                                .Sum(s => Convert.ToInt32(s.LikesCount))
                                            )
                                    )
                                    /
                                    Convert.ToDouble(inf.Seguidores)
                                ) * 100, 4),
                        DiffUsedEngag = 0,
                        Imagens = new List<string>()
                        {
                            (   x.Images.Count==0?
                                (
                                    x.Carousel.Count>0?
                                    x.Carousel.FirstOrDefault().Images.FirstOrDefault().URI.ToString()
                                    :"")
                                :x.Images.FirstOrDefault().URI.ToString()
                                )
                        }
                    }).ToList();
                inf.LstInstaMidias = lstMidias.Take(40) //.DistinctBy(d => d.us.UserName)
                    .ToList();
                #endregion

                #region HashTags
                linhaerro = "Hashtags";
                var mongoHashs = await repMongo.Listar<List<Models.InstaMediaHash>>(usuario.UsuarioInstagram);
                if (mongoHashs.Count > 0)
                {
                    var objHash = mongoHashs.FirstOrDefault().Obj;
                    var lstHash = objHash.Where(x => x.Hashs != null).ToList();
                    var newLstHash = lstHash.Select(s => s.Hashs).ToList();
                    List<string> hashs = new List<string>();
                    lstHash.ForEach(s =>
                    {
                        s.Hashs.ForEach(f =>
                        {
                            hashs.Add(f);
                        }
                                        );
                    }
                    );
                    var lstHashsDist = hashs.DistinctBy(x => x).ToList();
                    var lstImagensEhashs = new List<DtoHash>();
                    foreach (var it in lstHash)
                    {
                        foreach (var h in it.Hashs)
                        {
                            lstImagensEhashs.Add(
                            new DtoHash()
                            {
                                hash = h,
                                URIImagem = (it.InstaMedia.Images.Count > 0 ? it.InstaMedia.Images.FirstOrDefault().URI : "")
                            });
                        }
                    }

                    var lstHashs = lstHashsDist.Select(x => new Models.DTO.InstaMentions()
                    {
                        UserName = x
                        ,
                        UsedPerc = Math.Round(Convert.ToDouble
                        (
                                (
                                    Convert.ToDouble(lstHash.Where(u => u.Hashs.Contains(x)).Count())
                                /
                                    Convert.ToDouble(hashs.Count())
                                )
                        ) * 100, 4),
                        Used = Math.Round(Convert.ToDouble
                        (
                            Convert.ToDouble(lstHash.Where(u => u.Hashs.Contains(x)).Count())

                        ), 0),
                        Reach = lstHash.Where(c => c.Hashs.Contains(x))
                            .Sum(s => Convert.ToInt32(s.InstaMedia.CommentsCount) + s.InstaMedia.LikesCount),
                        Engagemer = Math.Round(
                                    Convert.ToDouble(
                                    (Convert.ToDouble(
                                            lstHash.Where(c => c.Hashs.Contains(x))
                                            .Sum(s => Convert.ToInt32(s.InstaMedia.CommentsCount))
                                        ) +
                                        Convert.ToDouble(
                                            lstHash.Where(c => c.Hashs.Contains(x))
                                                .Sum(s => Convert.ToInt32(s.InstaMedia.LikesCount))
                                            )
                                    )
                                    /
                                    Convert.ToDouble(
                                        lstHash.Where(c => c.Hashs.Contains(x))
                                        .Sum(s => hashs.Count)
                                        )) * 100, 4),
                        DiffUsedEngag = 1,
                        Imagens =
                                        lstImagensEhashs
                                        .Where(c => c.hash == x).DistinctBy(d => d.URIImagem)
                                        .Select(s => s.URIImagem).ToList()
                    }).ToList();
                    inf.LstInstaHashs = lstHashs.OrderByDescending(o => o.Reach).Take(40).ToList();
                }
                #endregion

                #region Locations
                linhaerro = "Locations";
                var lstLocationMaps = lstMinhasMidias.Where(y => y.Location != null)
                    .Select(g => new
                    {
                        Location = (g.Location.Name == null ? g.Location.ShortName : g.Location.Name),
                        LikesCount = g.LikesCount,
                        CommentsCount = g.CommentsCount,
                        FollowersCount = g.User.FollowersCount,
                        Images = g.Images,
                        Carousel = g.Carousel,
                        lat = g.Location.Lat,
                        lng = g.Location.Lng
                    });
                var lstLocationMapsDist = lstMinhasMidias.Where(y => y.Location != null)
                    .Select(g => new
                    {
                        Location = (g.Location.Name == null ? g.Location.ShortName : g.Location.Name),
                        LikesCount = g.LikesCount,
                        CommentsCount = g.CommentsCount,
                        FollowersCount = g.User.FollowersCount,
                        Images = g.Images,
                        Carousel = g.Carousel,
                        lat = g.Location.Lat,
                        lng = g.Location.Lng
                    }).ToList().DistinctBy(x => x.Location);
                var lstMaps = lstLocationMapsDist.Where(y => string.IsNullOrEmpty(y.Location) != true)
                        .Select(x => new Models.DTO.InstaMentions()
                        {

                            UserName = x.Location,
                            Used = Math.Round(Convert.ToDouble
                        (
                            Convert.ToDouble(lstLocationMaps.Count(c => c.Location == x.Location
                            ))
                        ), 0),
                            UsedPerc = Math.Round(Convert.ToDouble
                        (
                                (
                                    Convert.ToDouble(lstLocationMaps.Count(c => c.Location == x.Location
                                    ))
                                /
                                    Convert.ToDouble(lstLocationMaps.Count())
                                )
                        ) * 100, 4),
                            Reach = lstLocationMaps.Where(c => c.Location == x.Location
                        )
                            .Sum(s => Convert.ToInt32(s.CommentsCount) + s.LikesCount),
                            Engagemer = Math.Round(
                                    Convert.ToDouble(
                                    (Convert.ToDouble(
                                            lstLocationMaps.Where(c => c.Location == x.Location
                                            )
                                            .Sum(s => Convert.ToInt32(s.CommentsCount))
                                        ) +
                                        Convert.ToDouble(
                                            lstLocationMaps.Where(c => c.Location == x.Location
                                            )
                                                .Sum(s => Convert.ToInt32(s.LikesCount))
                                            )
                                    )
                                    /
                                    Convert.ToDouble(
                                        lstLocationMaps.Where(c => c.Location == x.Location
                                        )
                                        .Sum(s => s.FollowersCount)
                                        )) * 100, 4),
                            DiffUsedEngag = 0,
                            Imagens = new List<string>()
                                {
                            (   x.Images.Count==0?
                                (
                                    x.Carousel.Count>0?
                                    x.Carousel.FirstOrDefault().Images.FirstOrDefault().URI.ToString()
                                    :"")
                                :x.Images.FirstOrDefault().URI.ToString()
                                )
                                }
                        }).ToList();
                inf.LstInstaMaps = lstMaps.ToList();

                var lstMapsMarcadores = lstLocationMaps.Where(y => string.IsNullOrEmpty(y.Location) != true);
                #region Maps Places
                string markers = "[";
                foreach (var it in lstMapsMarcadores)
                {
                    markers += "{";
                    markers += string.Format("'title': '{0}',", it.Location.Replace("'", " "));
                    markers += string.Format("'lat': '{0}',", it.lat);
                    markers += string.Format("'lng': '{0}',", it.lng);
                    markers += string.Format("'description': '{0}',", (it.Location.Replace("'", " ")));
                    markers += string.Format("'image': '{0}'", (it.Images.Count > 0 ? it.Images.FirstOrDefault().URI.Replace("'", " ") : ""));
                    markers += "},";
                }
                markers += "];";
                ViewBag.Markers = markers;

                #endregion
                #endregion

                #region PhotoTags
                linhaerro = "Phototags";
                var lstPhotoTagsInterna = new List<Models.DTO.InstaMentions>();
                var lstPhotoTags = lstMinhasMidias
                    .Where(w => w.Tags.Count() > 0);
                foreach (var it in lstPhotoTags)
                {
                    foreach (var tg in it.Tags)
                    {
                        lstPhotoTagsInterna.Add(
                            new Models.DTO.InstaMentions()
                            {
                                UserName = tg.User.FullName,
                                Used = 1,
                                UsedPerc = 0,
                                Reach = it.LikesCount + Convert.ToInt32(it.CommentsCount),
                                Engagemer = 0,
                                DiffUsedEngag = 1,
                                Imagens = new List<string>()
                            {
                            (it.Images.Count==0?
                                (
                                    it.Carousel.Count>0?
                                    it.Carousel.FirstOrDefault().Images.FirstOrDefault().URI.ToString()
                                    :"")
                                :it.Images.FirstOrDefault().URI.ToString()
                                )
                            }
                            }
                            );
                    }
                }
                var lstPhotoTagsFull = lstPhotoTagsInterna.Select(x => new Models.DTO.InstaMentions()
                {
                    UserName = x.UserName,
                    Used = lstPhotoTagsInterna
                            .Where(p => p.UserName == x.UserName)
                            .Count(),
                    UsedPerc = Math.Round(
                            (
                            Convert.ToDouble(lstPhotoTagsInterna
                            .Where(p => p.UserName == x.UserName)
                            .Count())
                            /
                            Convert.ToDouble(
                                lstPhotoTagsInterna.Count()
                                )
                            ), 4),
                    Reach = lstPhotoTagsInterna
                            .Where(p => p.UserName == x.UserName)
                            .Sum(s => s.Reach),
                    Engagemer = 0,
                    DiffUsedEngag = 1,
                    Imagens = lstPhotoTagsInterna
                            .Where(p => p.UserName == x.UserName)
                            .Select(s => s.Imagens.FirstOrDefault())
                            .ToList()
                }).ToList();
                inf.LstInstaPhotoTags = lstPhotoTagsFull
                    .DistinctBy(d => d.UserName).ToList();
                #endregion

                #region FaceDetection
                linhaerro = "FaceDetection";
                if (lstFaceDetection.Count > 0)
                {
                    var lstSemNota = lstFaceDetection.FirstOrDefault().Obj
                        .Where(w => w.Joy == 0 && w.Anger == 0 && w.Surprise == 0 && w.Sorrow == 0)
                        .Select(s => s.UrlImagem).ToList();

                    var ls = from s in lstFaceDetection.FirstOrDefault().Obj
                             where !lstSemNota.Any(es => (es == s.UrlImagem))
                             select s;

                    var lstFaceDetections = ls
                        .Take(10).Select(s => new Models.FaceDetection()
                        {
                            Anger = (s.Anger / 2), //Raiva
                            Joy = (s.Joy / 4), //Alegria
                            Sorrow = (s.Sorrow / 2), //Tristeza
                            Surprise = (s.Surprise / 4), //Surpresa
                            DtAvaliacao = s.DtAvaliacao,
                            UrlImagem = s.UrlImagem,
                            UserName = s.UserName
                        }).ToList();

                    var lstAvgFaceDetection = lstFaceDetections.Select(l => new Models.FaceDetection()
                    {
                        Anger = lstFaceDetections.Sum(s => (s.Anger)), //Raiva
                        Joy = lstFaceDetections.Sum(s => (s.Joy)), //Alegria
                        Sorrow = lstFaceDetections.Sum(s => s.Sorrow), //Tristeza
                        Surprise = lstFaceDetections.Sum(s => (s.Surprise)), //Surpresa
                        DtAvaliacao = lstFaceDetections.FirstOrDefault().DtAvaliacao,
                        UrlImagem = "",
                        UserName = ""
                    });
                    var avgFaceDetection = lstAvgFaceDetection.DistinctBy(d => d.UserName).FirstOrDefault();
                    if (avgFaceDetection == null)
                    {

                    }
                    else
                    {
                        inf.LstFaceDetection = lstFaceDetections.ToList();
                        inf.AvgFaceDetection = avgFaceDetection;

                        var listaFaceDetection = (avgFaceDetection.Joy * 100 / countFaceDetection).ToString() + "," +
                            (avgFaceDetection.Sorrow * 100 / countFaceDetection).ToString() + "," +
                            (avgFaceDetection.Anger * 100 / countFaceDetection).ToString() + "," +
                            (avgFaceDetection.Surprise * 100 / countFaceDetection).ToString() + "";
                        var cabecalhoFaceDetection = "'Alegre','Tristeza','Raiva','Surpresa'";

                        ViewBag.CabecalhoFaceDetection = cabecalhoFaceDetection;
                        ViewBag.ListaFaceDetection = listaFaceDetection;

                        inf.CabecalhoFaceDetection = cabecalhoFaceDetection;
                        inf.ListaFaceDetection = listaFaceDetection;
                        inf.Markers = markers;
                    }
                }
                #endregion

                #region Groups Comments Unreal
                linhaerro = "GroupsCommentsUnreal";
                if (lstCommentsUnreal != null)
                {
                    var lstComments = lstCommentsUnreal.Obj
                       .Select(g => new
                       {
                           UsuarioInstagram = g.Split('|')[1],
                           Count = g.Split('|')[2],
                           ProfilePicture = g.Split('|')[3]
                       });
                    var lstCommentsAgrupado = lstComments.Where(y => y.Count != "0" && y.UsuarioInstagram != usuario.UsuarioInstagram)
                        .Select(x => new Models.DTO.InstaMentions()
                        {
                            UserName = x.UsuarioInstagram,
                            Used = Convert.ToInt32(x.Count),
                            UsedPerc = 100,
                            Reach = 0,
                            DiffUsedEngag = 0,
                            Imagens = new List<string>()
                                    {
                                    (
                                        x.ProfilePicture
                                    )
                                    }
                        }).ToList();
                    inf.LstUnrealComments = lstCommentsAgrupado.ToList();
                }
                #endregion

                #region Demographic range
                linhaerro = "Demographicrange";
                var lstLocations = (objMinhasMencoes == null ? objMinhasMidias.Obj : objMinhasMidias.Obj.ToList().Concat(objMinhasMencoes.Obj.ToList()));
                var lstDemographic = lstLocations.Where(y => y.Location != null)
                    .Select(g => new
                    {
                        Location = (!string.IsNullOrEmpty(g.Location.City) ? g.Location.City :
                                        (!string.IsNullOrEmpty(g.Location.Address) ? g.Location.Address :
                                            (!string.IsNullOrEmpty(g.Location.Name) ? g.Location.Name : g.Location.ShortName)
                                        )
                                   ),
                        LikesCount = g.LikesCount,
                        CommentsCount = g.CommentsCount,
                        FollowersCount = g.User.FollowersCount,
                        Images = g.Images,
                        Carousel = g.Carousel,
                        lat = g.Location.Lat,
                        lng = g.Location.Lng
                    });
                var lstDemographicDist = lstMinhasMidias.Where(y => y.Location != null)
                    .Select(g => new
                    {
                        Location = (!string.IsNullOrEmpty(g.Location.City) ? g.Location.City :
                                        (!string.IsNullOrEmpty(g.Location.Address) ? g.Location.Address :
                                            (!string.IsNullOrEmpty(g.Location.Name) ? g.Location.Name : g.Location.ShortName)
                                        )
                                   ),
                        LikesCount = g.LikesCount,
                        CommentsCount = g.CommentsCount,
                        FollowersCount = g.User.FollowersCount,
                        Images = g.Images,
                        Carousel = g.Carousel,
                        lat = g.Location.Lat,
                        lng = g.Location.Lng
                    }).ToList().DistinctBy(x => x.Location);
                var lstDemographicRange = lstDemographicDist.Where(y => string.IsNullOrEmpty(y.Location) != true)
                        .Select(x => new Models.DTO.InstaMentions()
                        {

                            UserName = x.Location,
                            Used = Math.Round(Convert.ToDouble(
                            Convert.ToDouble(lstDemographic.Count(c => c.Location == x.Location
                            ))), 0),
                            UsedPerc = Math.Round(Convert.ToDouble(
                                (
                                    Convert.ToDouble(lstDemographic.Count(c => c.Location == x.Location
                                    ))
                                /
                                    Convert.ToDouble(lstDemographic.Count())
                                )) * 100, 4),
                            Reach = lstDemographic.Where(c => c.Location == x.Location).Sum(s => Convert.ToInt32(s.CommentsCount) + s.LikesCount),
                            Engagemer = Math.Round(
                                    Convert.ToDouble(
                                    (Convert.ToDouble(
                                            lstDemographic.Where(c => c.Location == x.Location
                                            )
                                            .Sum(s => Convert.ToInt32(s.CommentsCount))
                                        ) +
                                        Convert.ToDouble(
                                            lstDemographic.Where(c => c.Location == x.Location
                                            )
                                                .Sum(s => Convert.ToInt32(s.LikesCount))
                                            )
                                    )
                                    /
                                    Convert.ToDouble(
                                        lstDemographic.Where(c => c.Location == x.Location
                                        )
                                        .Sum(s => s.FollowersCount)
                                        )) * 100, 4),
                            DiffUsedEngag = 0,
                            Imagens = new List<string>()
                                {
                            (   x.Images.Count==0?
                                (
                                    x.Carousel.Count>0?
                                    x.Carousel.FirstOrDefault().Images.FirstOrDefault().URI.ToString()
                                    :"")
                                :x.Images.FirstOrDefault().URI.ToString()
                                )
                                }
                        }).ToList();
                inf.LstDemographicRange = lstDemographicRange.ToList().OrderByDescending(o => o.Reach).ToList();
                #endregion

                #region Best Midia Places
                linhaerro = "BestMidiaPlace";
                var lstBestMidiaPlace = lstLocations.Where(y => y.Location != null)
                    .Select(g => new
                    {
                        Location = (!string.IsNullOrEmpty(g.Location.City) ? g.Location.City :
                                        (!string.IsNullOrEmpty(g.Location.Address) ? g.Location.Address :
                                            (!string.IsNullOrEmpty(g.Location.Name) ? g.Location.Name : g.Location.ShortName)
                                        )
                                   ),
                        LikesCount = g.LikesCount,
                        CommentsCount = g.CommentsCount,
                        FollowersCount = g.User.FollowersCount,
                        Images = g.Images,
                        Carousel = g.Carousel,
                        lat = g.Location.Lat,
                        lng = g.Location.Lng
                    });
                var lstBestMidiaPlacesDist = lstMinhasMidias.Where(y => y.Location != null)
                    .Select(g => new
                    {
                        Location = (!string.IsNullOrEmpty(g.Location.City) ? g.Location.City :
                                        (!string.IsNullOrEmpty(g.Location.Address) ? g.Location.Address :
                                            (!string.IsNullOrEmpty(g.Location.Name) ? g.Location.Name : g.Location.ShortName)
                                        )
                                   ),
                        LikesCount = g.LikesCount,
                        CommentsCount = g.CommentsCount,
                        FollowersCount = g.User.FollowersCount,
                        Images = g.Images,
                        Carousel = g.Carousel,
                        lat = g.Location.Lat,
                        lng = g.Location.Lng
                    }).ToList().DistinctBy(x => x.Location);
                var lstBestMidiaPlacesRange = lstBestMidiaPlacesDist.Where(y => string.IsNullOrEmpty(y.Location) != true)
                        .Select(x => new Models.DTO.InstaMentions()
                        {
                            UserName = x.Location,
                            Used = Math.Round(Convert.ToDouble(
                            Convert.ToDouble(lstBestMidiaPlace.Count(c => c.Location == x.Location
                            ))), 0),
                            UsedPerc = Math.Round(Convert.ToDouble(
                                (
                                    Convert.ToDouble(lstBestMidiaPlace.Count(c => c.Location == x.Location
                                    ))
                                /
                                    Convert.ToDouble(lstBestMidiaPlace.Count())
                                )) * 100, 4),
                            Reach = lstBestMidiaPlace.Where(c => c.Location == x.Location).Sum(s => Convert.ToInt32(s.CommentsCount) + s.LikesCount),
                            Engagemer = Math.Round(
                                    Convert.ToDouble(
                                    (Convert.ToDouble(
                                            lstBestMidiaPlace.Where(c => c.Location == x.Location
                                            )
                                            .Sum(s => Convert.ToInt32(s.CommentsCount))
                                        ) +
                                        Convert.ToDouble(
                                            lstBestMidiaPlace.Where(c => c.Location == x.Location
                                            )
                                                .Sum(s => Convert.ToInt32(s.LikesCount))
                                            )
                                    )
                                    /
                                    Convert.ToDouble(
                                        lstBestMidiaPlace.Where(c => c.Location == x.Location
                                        )
                                        .Sum(s => s.FollowersCount)
                                        )) * 100, 4),
                            DiffUsedEngag = 0,
                            Imagens = new List<string>()
                                {
                            (   x.Images.Count==0?
                                (
                                    x.Carousel.Count>0?
                                    x.Carousel.FirstOrDefault().Images.FirstOrDefault().URI.ToString()
                                    :"")
                                :x.Images.FirstOrDefault().URI.ToString()
                                )
                                }
                        }).ToList();
                inf.LstBestMidiaPlacesRange = lstBestMidiaPlacesRange.ToList().OrderByDescending(o => o.Reach).ToList();
                #endregion

                #region Ages e Gender
                linhaerro = "Gender and Ages";
                if (lstAge != null)
                {
                    var lstAgesGender = lstAge.data.values.FirstOrDefault().value
                        .Select(x => new
                        {
                            Gender = x.name.Split('.').GetValue(0),
                            Faixa = x.name.Split('.').GetValue(1),
                            Used = 0,
                            UsedPerc = 0,
                            Reach = Convert.ToInt32(x.valor),
                            Engagemer = 0,
                            DiffUsedEngag = 0
                        }).ToList();

                    var lstAgesMidias = lstAge.data.values.FirstOrDefault().value
                        .Select(x => new Models.DTO.InstaMentions()
                        {
                            UserName = "" + x.name,
                            Used = 0,
                            UsedPerc = 0,
                            Reach = Convert.ToInt32(x.valor),
                            Engagemer = 0,
                            DiffUsedEngag = 0,
                            Imagens = null
                        }).ToList();
                    inf.LstAge = lstAgesMidias.OrderByDescending(o => o.Reach).Take(40) //.DistinctBy(d => d.us.UserName)
                        .ToList();
                }
                #endregion

                #region Top and Botton POST
                inf.LstTopAndBotton = inf.LstInstaMidias.Where(w => w.Engagemer == inf.LstInstaMidias.Max(m => m.Engagemer))
                    .Union(
                    inf.LstInstaMidias.Where(w => w.Engagemer == inf.LstInstaMidias.Min(m => m.Engagemer))).ToList();
                #endregion

                await repMongo.GravarOne<Models.DTO.InfluencersResumo>(inf);

                return View(inf);
            }
            catch (Exception)
            {
                ViewBag.Message = "Erro inesperado ao processar a visualização.<br />Por favor tente novamente em alguns minutos (" + linhaerro + ")";
                return View();
            }
        }

        public async Task<ActionResult> AutorizarMetricaError(Models.DTO.Erro error)
        {
            ViewBag.Message = error.Message;
            return await AutorizarMetrica();
        }

        public async Task<ActionResult> AutorizarAnalise()
        {
            return View();
        }
        public async Task<ActionResult> AutorizarMetrica()
        {
            var UserId = HttpContext.Session.GetString("UserId");
            Repository.MongoRep repMongo = new Repository.MongoRep("", _settings);
            if (string.IsNullOrEmpty(UserId))
            {
                return RedirectToAction("Index");
            }
            else
            {
                var lst = await repMongo.ListarUsuarioId<Models.AutorizacaoMetrica>(UserId);
                var _lst = new Models.DTO.AutorizarMetricaPage()
                {
                    autorizacaoMetricas = lst.Select(s => new Models.AutorizacaoMetrica()
                    {
                        DataCriacao =
                                lst.Where(w => w.Obj.UsuarioInstagram == s.Obj.UsuarioInstagram)
                                .Max(m => m.Obj.DataCriacao),
                        Email = s.Obj.Email,
                        UsuarioInstagram = s.Obj.UsuarioInstagram,
                        Status = (s.Obj.Status == null && s.Obj.Email == null) ? "Disponível" :
                            ((s.Obj.Status == null) ? "Processando" : "Disponível"),
                        PowerFull = 0,
                        Seguidores = 0
                    }).DistinctBy(d => d.UsuarioInstagram),
                    autorizacaoMetrica = new Models.AutorizacaoMetrica()
                    {
                        UsuarioId = UserId
                    }
                };
                var _credito = await GetCredito(UserId, repMongo);
                if (_credito != null)
                {
                    ViewBag.CreditoMetricas = _credito.Qtd - _credito.Debito;
                }
                return View("AutorizarMetrica", _lst);
            }
        }

        public async Task<ActionResult> Authorize(string code)
        {
            ApiClient api = new ApiClient(_settings);

            var resultAccessToken = await api.GetAccessToken(code);
            Repository.MongoRep repMongo = new Repository.MongoRep(resultAccessToken.user.username, _settings);

            await repMongo.GravarOne<Models.AccessToken>(resultAccessToken);

            var resultMediaRecent = await api.GetInsta(code);
            await repMongo.GravarOne<Models.MediaRecent>(resultMediaRecent);

            var resultUserSearch = await api.GetUsuarioData(code);
            await repMongo.GravarOne<Models.UsuarioData>(resultUserSearch);


            return RedirectToAction("visualizar", (new Models.Usuario()
            {
                UsuarioInstagram = resultAccessToken.user.username
            }));

            //return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AutorizarMetrica(webMetrics.Models.DTO.AutorizarMetricaPage _usuario)
        {
            Models.AutorizacaoMetrica usuario = _usuario.autorizacaoMetrica;
            var UserId = HttpContext.Session.GetString("UserId");
            try
            {
                Repository.MongoRep repMongo = new Repository.MongoRep(usuario.UsuarioInstagram, _settings);
                try
                {
                    var lst = await repMongo.Listar<Models.Usuario>(usuario.UsuarioInstagram);
                    usuario.Key = Guid.NewGuid().ToString();
                    usuario.UsuarioId = HttpContext.Session.GetString("UserId");
                    usuario.DataCriacao = DateTime.Now;
                    await repMongo.GravarOne<Models.AutorizacaoMetrica>(usuario);
                    if (lst
                        .Where(x => x.Obj.UsuarioInstagram == usuario.UsuarioInstagram
                           && x.Obj.DataValidade >= DateTime.Now).Count() > 0)
                    {
                        return RedirectToAction("Visualizar", new Models.Usuario()
                        {
                            UsuarioInstagram = usuario.UsuarioInstagram,
                            DataValidade = DateTime.Now.AddDays(6)
                        });
                    }

                    if (string.IsNullOrEmpty(usuario.Email))
                    {
                        return await ConsultarInstagram(new Models.Usuario()
                        {
                            UsuarioInstagram = usuario.UsuarioInstagram,
                            DataValidade = DateTime.Now.AddDays(14),
                            Key = usuario.Key
                        });
                    }
                    else
                    {
                        var snd = SenderEmail.Enviar(usuario.Email, usuario.Key);
                        return await ConsultarInstagram(new Models.Usuario()
                        {
                            UsuarioInstagram = usuario.UsuarioInstagram,
                            DataValidade = DateTime.Now.AddDays(6)
                        });
                    }
                }
                catch (Exception)
                {
                    return View();
                }
            }
            catch (Exception)
            {
                return View();
            }
        }

        public async Task<ActionResult> Cadastrar(string Tipo)
        {
            return RedirectToAction("Login");

            //var usuario = new Models.Usuario
            //{
            //    Tipo = Tipo.ToString()
            //};
            //return View(usuario);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Cadastrar(Models.Usuario usuario)
        {
            try
            {
                Repository.MongoRep repMongo = new Repository.MongoRep(usuario.UsuarioInstagram, _settings);

                usuario.DataValidade = DateTime.Now.AddDays(-1);
                usuario.DataCriacao = DateTime.Now;

                if (usuario.Email == null && usuario.TokenFacebook == null)
                {
                    return RedirectToAction("ComeceAqui", new { msg = "1" });
                }
                if (usuario.Tipo == null)
                {
                    usuario.Tipo = "1";
                }

                if (string.IsNullOrEmpty(usuario.TokenFacebook))
                {

                    var user = await repMongo.Login(usuario.Email, usuario.Senha);
                    if (user == null)
                    {
                        if (string.IsNullOrEmpty(usuario.UserId)) usuario.UserId = usuario.Email;
                        await repMongo.GravarOne<Models.Usuario>(usuario);
                    }

                    var users = await repMongo.Login(usuario.Email, usuario.Senha);
                    if (users != null)
                    {
                        HttpContext.Session.SetString("UsuarioFull_id", users._id.ToString());
                        HttpContext.Session.SetString("UserId", users._id.ToString());
                    }

                    HttpContext.Session.SetInt32("isFacebook", 0);
                    HttpContext.Session.SetString("Tipo", usuario.Tipo);
                    HttpContext.Session.SetString("UsuarioInstagram", (string.IsNullOrEmpty(usuario.UsuarioInstagram) ? "" : usuario.UsuarioInstagram));
                }
                else
                {
                    var user = await repMongo.LoginFacebook(usuario.UserId);
                    if (user == null)
                    {
                        await repMongo.GravarOne<Models.Usuario>(usuario);
                    }

                    var users = await repMongo.LoginFacebook(usuario.UserId);
                    if (users != null)
                    {
                        HttpContext.Session.SetString("UsuarioFull_id", users._id.ToString());
                    }

                    HttpContext.Session.SetInt32("isFacebook", 0);
                    HttpContext.Session.SetString("UserId", users._id.ToString());
                    HttpContext.Session.SetString("Tipo", usuario.Tipo);
                    HttpContext.Session.SetString("UsuarioInstagram", (string.IsNullOrEmpty(usuario.UsuarioInstagram) ? "" : usuario.UsuarioInstagram));

                    HttpContext.Session.SetString("access_token_page", users.Obj.access_token_page ?? "");
                    HttpContext.Session.SetString("name_page", users.Obj.name_page ?? "");
                }

                //Enviar email de bem vindo
                if (!string.IsNullOrEmpty(usuario.Email))
                {
                    var envio = SenderEmail.BemVindo(usuario.Email, HttpContext.Session.GetString("UsuarioFull_id"));
                }

                await repMongo.GravarOne<Models.CreditoMetricas>(new Models.CreditoMetricas()
                {
                    DataCredito = DateTime.Now,
                    DataCriacao = DateTime.Now,
                    DataValidade = DateTime.Now.AddDays(1),
                    Qtd = 1,
                    Debito = 0,
                    UserId = HttpContext.Session.GetString("UserId")
                });

                var pagamentoPage = new Models.DTO.PagamentoPage()
                {

                };
                if (usuario.Tipo == "1")
                {//Influenciador
                    pagamentoPage.Quantidade = 1;
                    pagamentoPage.Valor = 119M;
                    pagamentoPage.Usuario = usuario;
                    pagamentoPage.Total = 119M;
                    return RedirectToAction("MinhasAnalises"); //TODO: Voltar
                }
                if (usuario.Tipo == "2")
                {//Influenciador
                    pagamentoPage.Quantidade = 1;
                    pagamentoPage.Valor = 119M;
                    pagamentoPage.Usuario = usuario;
                    pagamentoPage.Total = 119M;

                    HttpContext.Session.SetString("nomeagencia", usuario.NomeAgencia ?? "");
                    //redirecionar para escolha de plano
                    return RedirectToAction("PesquisarOuAuditarInfluenciador");
                    //return RedirectToAction("historicometricas", "relatorios", new { msg = "2" });
                }

                return RedirectToAction("login", "relatorios", new { msg = "4" });
            }
            catch (Exception x)
            {
                return RedirectToAction("ComeceAqui", new { msg = "2" });
            }

            //return RedirectToAction("Pagamento", usuario);

        }

        public ActionResult PesquisarOuAuditarInfluenciador()
        {
            return View();
        }

        public async Task<ActionResult> Perfil2()
        {
            var UserId = HttpContext.Session.GetString("UserId");
            if (UserId == null) return RedirectToAction("login");
            Repository.MongoRep repMongo = new Repository.MongoRep(UserId, _settings);
            var objeto = repMongo.ListarEmail<Models.Usuario>(UserId).Result.FirstOrDefault();
            var usuario = objeto.Obj;

            return View(usuario);
        }

        public async Task<ActionResult> PerfilOld()
        {
            var UserId = HttpContext.Session.GetString("UserId");
            if (UserId == null) return RedirectToAction("login");

            Repository.MongoRep repMongo = new Repository.MongoRep(UserId, _settings, UserId);
            var objeto = await repMongo.ListarById<Models.Usuario>(new ObjectId(UserId));
            if (objeto == null || (objeto.Count == 0))
            {
                return RedirectToAction("login");
            }

            var usuario = objeto.FirstOrDefault().Obj;

            if (usuario.Tipo == "2")
            {
                var NomeAgencia = HttpContext.Session.GetString("nomeagencia");
                ViewBag.NameUser = NomeAgencia;
                HttpContext.Session.SetString("userNameTitle", NomeAgencia);
            }
            else
            {
                ViewBag.NameUser = usuario.name_page;
                HttpContext.Session.SetString("userNameTitle", usuario.name_page);
            }
            var picture = await LoadPictureProfile(UserId, repMongo, objeto.FirstOrDefault().Obj.Tipo);
            HttpContext.Session.SetString("ProfilePicture", picture);
            ViewBag.ProfilePicture = picture;

            return View(usuario);
        }

        //public async Task<ActionResult> Perfil()
        //{
        //    var UserId = HttpContext.Session.GetString("UserId");
        //    if (UserId == null) return RedirectToAction("login");
        //    Repository.MongoRep repMongo = new Repository.MongoRep(UserId, _settings, UserId);
        //    var objeto = await repMongo.ListarById<Models.Usuario>(new ObjectId(UserId));
        //    if (objeto == null || (objeto.Count == 0))
        //    {
        //        return RedirectToAction("login");
        //    }
        //    var usuario = objeto.FirstOrDefault().Obj;
        //    if (usuario.Tipo == "2")
        //    {
        //        var NomeAgencia = HttpContext.Session.GetString("nomeagencia");
        //        ViewBag.NameUser = NomeAgencia;
        //        HttpContext.Session.SetString("userNameTitle", NomeAgencia);
        //    }
        //    else
        //    {
        //        ViewBag.NameUser = usuario.name_page;
        //        HttpContext.Session.SetString("userNameTitle", usuario.name_page);
        //    }
        //    var picture = await LoadPictureProfile(UserId, repMongo, objeto.FirstOrDefault().Obj.Tipo);
        //    HttpContext.Session.SetString("ProfilePicture", picture);
        //    ViewBag.ProfilePicture = picture;
        //    return View(usuario);
        //}

        private async Task<string> LoadPictureProfile(string UserId, Repository.MongoRep repMongo, string tipo)
        {
            if (tipo == "1")
            {
                var lstUsers = await repMongo.ListarGraphUserId<Models.Graph.Usuario>(UserId);
                if (lstUsers != null && (lstUsers.Count > 0))
                {
                    return lstUsers.OrderByDescending(o => o.DateCreation).FirstOrDefault().Obj.profile_picture_url;
                }
                else
                {
                    return "https://gastroahotel.cz/files/2014/10/silueta.jpg";
                }
            }

            var Profile = await repMongo.Listar<Models.DTO.ImageProfileAgency>(UserId);
            if (Profile != null && (Profile.Count > 0 && (Profile.FirstOrDefault().Obj != null)))
            {
                return "/img_agencias/" + Profile.OrderByDescending(o => o.timeSpan).FirstOrDefault().Obj.ProfilePictureName;
            }
            else
            {
                return "https://gastroahotel.cz/files/2014/10/silueta.jpg";
            }
        }

        public async Task<ActionResult> Pagamento(Models.DTO.PagamentoPage pagamentoPage)
        {
            string sessionId = CreateSession();
            string jsExecutar = "PagSeguroDirectPayment.setSessionId(\"" + sessionId + "\");";
            ViewBag.jsExecutar = jsExecutar;

            string _UrlPagSeguro = _appSettings.UrlPagSeguro;// System.Configuration.Configuration.GetSection("AppConfiguration")["UrlPagSeguro"];

            ViewBag.jsUrlPagaSeguro = _UrlPagSeguro;

            return View(pagamentoPage);
        }

        private string CreateSession()
        {
            string retorno = "";

            EnvironmentConfiguration.ChangeEnvironment(isSandbox);

            try
            {
                AccountCredentials credentials = PagSeguroConfiguration.Credentials(isSandbox);
                Session result = SessionService.CreateSession(credentials);
                retorno = result.id.ToString();
            }
            catch (PagSeguroServiceException exception)
            {
                foreach (ServiceError element in exception.Errors)
                {
                    retorno += element + "\n";
                }
            }
            return retorno;
        }

        public ActionResult CreatePay(string token, string hash, string Cpf, string Nome, string Sobrenome, string Telefone, string Street, string Number, string Complement, string District, string City, string State, string Country, string PostalCode, string Tipo, string InstallmentValue, string DataNascimento, string Email)
        {
            var usuario = new Models.Usuario
            {
                Cpf = Cpf,
                Nome = Nome,
                Sobrenome = Sobrenome,
                Telefone = Telefone,
                Street = Street,
                Number = Number,
                Complement = Complement,
                District = District,
                City = City,
                State = State,
                Country = Country,
                PostalCode = PostalCode,
                DataNascimento = Convert.ToDateTime(DataNascimento),
                Email = Email,
                Tipo = Tipo
            };
            try
            {
                ApiClient api = new ApiClient(_settings)
                {
                    Token = _appSettings.TokenPagSeguro,// ConfigurationManager.AppSettings["TokenPagSeguro"]; //"71A42158824A404F9F3B0CD99318A384";
                    Email = _appSettings.EmailPagSeguro// ConfigurationManager.AppSettings["EmailPagSeguro"]; //"didcompras@gmail.com";
                };

                var trans = new Models.PagSeguroTransaction
                {
                    paymentMethod = "creditCard",
                    paymentMode = "default",
                    receiverEmail = api.Email,
                    currency = "BRL",
                    extraAmount = "0.00",
                    itemId1 = "1",
                    itemDescription1 = "Influencers Metrics",
                    itemQuantity1 = "1",
                    itemAmount1 = usuario.Tipo == "1" ?
                                    "119.00" : (
                                    usuario.Tipo == "2" ?
                                    "3119.00" : "6119.00"),
                    notificationURL = "http://wwww.influencersmetrics.com/newsmetrics/relatorios/notification",
                    reference = usuario.Email,
                    senderCPF = usuario.Cpf,
                    senderEmail = usuario.Email,
                    senderHash = hash,
                    senderName = usuario.Nome + " " + usuario.Sobrenome,
                    senderAreaCode = usuario.Telefone.Substring(0, 2),
                    senderPhone = usuario.Telefone.Substring(2, usuario.Telefone.Length - 2),
                    shippingAddressStreet = usuario.Street,
                    shippingAddressNumber = usuario.Number,
                    shippingAddressComplement = usuario.Complement,
                    shippingAddressDistrict = usuario.District,
                    shippingAddressPostalCode = usuario.PostalCode,
                    shippingAddressCity = usuario.City,
                    shippingAddressState = usuario.State,
                    shippingAddressCountry = "BRA",
                    shippingType = "1",
                    shippingCost = "0.00",
                    creditCardToken = token,
                    installmentValue = InstallmentValue,
                    installmentQuantity = "1",
                    noInterestInstallmentQuantity = "18",
                    creditCardHolderName = usuario.Nome + " " + usuario.Sobrenome,
                    creditCardHolderCPF = usuario.Cpf,
                    creditCardHolderBirthDate = usuario.DataNascimento.ToString("dd/MM/yyyy"),
                    creditCardHolderAreaCode = usuario.Telefone.Substring(0, 2),
                    creditCardHolderPhone = usuario.Telefone.Substring(2, usuario.Telefone.Length - 2),
                    billingAddressStreet = usuario.Street,
                    billingAddressNumber = usuario.Number,
                    billingAddressComplement = usuario.Complement,
                    billingAddressDistrict = usuario.District,
                    billingAddressPostalCode = usuario.PostalCode,
                    billingAddressCity = usuario.City,
                    billingAddressState = usuario.State,
                    billingAddressCountry = "BRA"
                };
                Repository.MongoRep rep = new Repository.MongoRep(usuario.UsuarioInstagram, _settings);
                rep.GravarOne<Models.PagSeguroTransaction>(trans).Wait();

                string codeAssinatura = api.Transactions(trans).Result;

                //inf.Pagamento.DtExpiracao = DateTime.Now.AddMonths(1);
                //inf.Pagamento.HashCartao = hash;
                //inf.Pagamento.TokenCartao = token;
                //inf.Pagamento.CodigoAssinatura = codeAssinatura;

                //inf.Pagamento.InfluencerId = inf.InfluencerId;

                //Repository.PagamentoRespository.Add(inf.Pagamento);
                //var pagamentoId = Repository.PagamentoRespository.Max();
                //inf.Pagamento.PagamentoId = pagamentoId;
                trans.Code = codeAssinatura;
                rep.GravarOne<Models.PagSeguroTransaction>(trans);

                //Mudar a data da expiração

            }
            catch (Exception ex)
            {
                if (ex.InnerException != null)
                {
                    return Content(ex.InnerException.Message.ToString());
                }
                else
                {
                    return Content(ex.Message.ToString());
                }
            }

            return Content("");
        }

        private async Task<ActionResult> CreateVisualizar(Models.Usuario usuario)
        {
            usuario.DataCriacao = DateTime.Now;
            var faixa1 = 100000; //Até
            var faixa2 = 500000;

            var valor1 = 15.5;
            var valor2 = 13.5;
            var valor0 = 12.5;

            #region Repositorio
            Repository.MongoRep repMongo = new Repository.MongoRep(usuario.UsuarioInstagram, _settings);
            await repMongo.GravarOne<Models.Usuario>(usuario);
            var inf = new Models.DTO.InfluencersResumo();

            var mongoUser = await repMongo.Listar<Models.AccessToken>(usuario.UsuarioInstagram);
            var objUser = mongoUser.FirstOrDefault().Obj;

            var mongoMediarecent = await repMongo.Listar<Models.MediaRecent>(usuario.UsuarioInstagram);
            var objMediaRecent = mongoMediarecent.FirstOrDefault().Obj.data;

            var mongoUserData = await repMongo.Listar<Models.UsuarioData>(usuario.UsuarioInstagram);
            var objUserData = mongoUserData.FirstOrDefault().Obj;

            #endregion

            //var objMinhasMencoes = objUser
            //                    .Where(x => x...Obj.FirstOrDefault().User.UserName != usuario.UsuarioInstagram)
            //                    .FirstOrDefault();

            inf.Seguidores = objUserData.data.counts.followed_by;
            inf.Seguindo = objUserData.data.counts.follows;
            inf.SeguindoSeguidores = inf.Seguindo / (decimal)inf.Seguidores * 100;
            //inf.SeguidoresUnicos = inf.Seguidores;// - (objUser.MutualFollowers / 10);
            inf.Posts = objUserData.data.counts.media;
            inf.Curtidas = objMediaRecent.Sum(x => x.likes.count);
            inf.ProfilePicture = objUserData.data.profile_picture;
            inf.NomeCompleto = objUserData.data.full_name;
            inf.UserName = objUserData.data.username;
            inf.SocialContext = objUser.user.bio;

            var comentarios = 0;
            objMediaRecent.ForEach(x =>
            {
                comentarios += (Convert.ToInt32(x.comments.count));
            });
            inf.Comentarios = comentarios;

            inf.avgPostReach = Math.Round(Convert.ToDouble(inf.Curtidas + inf.Comentarios) / objMediaRecent.Count);

            inf.MediaCurtidas = inf.Curtidas / inf.Posts;
            inf.MediaComentarios = inf.Comentarios / (decimal)inf.Posts;
            inf.ComentariosSeguidores = (inf.MediaComentarios / inf.Seguidores) * 100;
            inf.Engajamento = ((inf.Curtidas + (decimal)inf.Comentarios) / inf.Posts) / inf.Seguidores * 100;
            inf.Alcance = inf.Curtidas + inf.Comentarios;
            inf.MediaAlcancePost = inf.Posts / (decimal)inf.Alcance * 100;
            inf.Aprovado = 2;
            ////////////////
            //////PARAR 

            if (Math.Round((
                                           (
                                               Convert.ToDouble(
            (inf.Comentarios + inf.Curtidas) / objMediaRecent.Count)
                                           )
                                           /
                                           (inf.Seguidores) * 100), 2) <
                                           (inf.Seguidores < faixa1 ? valor1 : (inf.Seguidores < faixa2 ? valor2 : valor0))
                                           )
            {
                inf.percentAvg = Math.Round((
                                    (
                                        Convert.ToDouble(
                                            (inf.Comentarios + inf.Curtidas) / objMediaRecent.Count)
                                    )
                                    /
                                    (inf.Seguidores) * 100), 2);
                inf.Aprovado = 2;
            }
            else
            {
                inf.Aprovado = 0;
                inf.percentAvg = Math.Round((
                                    (
                                        Convert.ToDouble(
                                            (inf.Comentarios + inf.Curtidas) / objMediaRecent.Count)
                                    )
                                    /
                                    (inf.Seguidores) * 100), 2);
            }

            var lstMinhasMencoes = objMediaRecent.ToList();
            var lstMentions = lstMinhasMencoes.Select(x => new Models.DTO.InstaMentions()
            {
                UserName = "@" + x.user.username.ToLower(),
                Used = Math.Round(Convert.ToDouble
                (
                    Convert.ToDouble(lstMinhasMencoes.Count(c => c.user.username == x.user.username))
                )),
                UsedPerc = Math.Round(Convert.ToDouble
                (
                        (
                            Convert.ToDouble(lstMinhasMencoes.Count(c => c.user.username == x.user.username))
                        /
                            Convert.ToDouble(lstMinhasMencoes.Count())
                        )
                ) * 100, 4),
                Reach = lstMinhasMencoes.Where(c => c.user.username == x.user.username)
                    .Sum(s => Convert.ToInt32(s.comments.count) + s.likes.count),
                Engagemer = Math.Round(
                            Convert.ToDouble(
                            (Convert.ToDouble(
                                    lstMinhasMencoes.Where(c => c.user.username == x.user.username)
                                    .Sum(s => Convert.ToInt32(s.comments.count))
                                ) +
                                Convert.ToDouble(
                                    lstMinhasMencoes.Where(c => c.user.username == x.user.username)
                                        .Sum(s => Convert.ToInt32(s.likes.count))
                                    )
                            )
                            /
                            Convert.ToDouble(inf.Seguidores))
                                 * 100, 4),
                DiffUsedEngag = 1,
                Imagens = new List<string>()
                        {
                            (   x.images.thumbnail.url)
                        }
            }).ToList();
            inf.LstInstaMentions = lstMentions.DistinctBy(d => d.UserName)
                .ToList();

            var lstMinhasMidias = objMediaRecent.ToList();
            var lstMidias = lstMinhasMidias.Select(x => new Models.DTO.InstaMentions()
            {
                UserName = "@" + x.user.username.ToLower(),
                Used = Math.Round(Convert.ToDouble
                (
                    Convert.ToDouble(lstMinhasMidias.Count(c => c.user.username == x.user.username))
                ), 0),
                UsedPerc = Math.Round(Convert.ToDouble
                (
                        (
                            Convert.ToDouble(lstMinhasMidias.Count(c => c.user.username == x.user.username))
                        /
                            Convert.ToDouble(lstMinhasMidias.Count())
                        )
                ) * 100, 4),
                Reach = lstMinhasMidias.Where(c => c.user.username == x.user.username)
                    .Sum(s => Convert.ToInt32(s.comments.count) + s.likes.count),
                Engagemer = Math.Round(
                            Convert.ToDouble(
                            (Convert.ToDouble(
                                    lstMinhasMidias.Where(c => c.user.username == x.user.username)
                                    .Sum(s => Convert.ToInt32(s.comments.count))
                                ) +
                                Convert.ToDouble(
                                    lstMinhasMidias.Where(c => c.user.username == x.user.username)
                                        .Sum(s => Convert.ToInt32(s.likes.count))
                                    )
                            )
                            /
                            Convert.ToDouble(inf.Seguidores)) * 100, 4),
                DiffUsedEngag = 0,
                Imagens = new List<string>()
                        {
                            (   x.images.thumbnail.url)
                        }
            }).ToList();
            inf.LstInstaMidias = lstMidias.Take(40) //.DistinctBy(d => d.us.UserName)
                .ToList();

            var lstHash = objMediaRecent.Where(x => x.tags.Count > 0).ToList();
            var newLstHash = lstHash.Select(s => s.tags).ToList();
            List<string> hashs = new List<string>();
            lstHash.ForEach(s =>
            {
                s.tags.ForEach(f =>
                {
                    hashs.Add((string)f);
                }
                                );
            }
            );
            var lstHashsDist = hashs.DistinctBy(x => x).ToList();
            var lstImagensEhashs = new List<DtoHash>();
            foreach (var it in lstHash)
            {
                foreach (var h in it.tags)
                {
                    lstImagensEhashs.Add(
                    new DtoHash()
                    {
                        hash = (string)h,
                        URIImagem = (it.images.thumbnail.url)
                    });
                }
            }

            var lstHashs = lstHashsDist.Select(x => new Models.DTO.InstaMentions()
            {
                UserName = x
                ,
                UsedPerc = Math.Round(Convert.ToDouble
                (
                        (
                            Convert.ToDouble(lstHash.Where(u => u.tags.Contains(x)).Count())
                        /
                            Convert.ToDouble(hashs.Count())
                        )
                ) * 100, 4),
                Used = Math.Round(Convert.ToDouble
                (
                    Convert.ToDouble(lstHash.Where(u => u.tags.Contains(x)).Count())

                ), 0),
                Reach = lstHash.Where(c => c.tags.Contains(x))
                    .Sum(s => Convert.ToInt32(s.comments.count) + s.likes.count),
                Engagemer = Math.Round(
                            Convert.ToDouble(
                            (Convert.ToDouble(
                                    lstHash.Where(c => c.tags.Contains(x))
                                    .Sum(s => Convert.ToInt32(s.comments.count))
                                ) +
                                Convert.ToDouble(
                                    lstHash.Where(c => c.tags.Contains(x))
                                        .Sum(s => Convert.ToInt32(s.likes.count))
                                    )
                            )
                            /
                            Convert.ToDouble(
                                lstHash.Where(c => c.tags.Contains(x))
                                .Sum(s => hashs.Count)
                                )) * 100, 4),
                DiffUsedEngag = 1,
                Imagens =
                                lstImagensEhashs
                                .Where(c => c.hash == x).DistinctBy(d => d.URIImagem)
                                .Select(s => s.URIImagem).ToList()
            }).ToList();
            inf.LstInstaHashs = lstHashs.OrderByDescending(o => o.Reach).Take(40).ToList();

            var lstLocationMaps = objMediaRecent.Where(y => y.location != null)
                .Select(g => new
                {
                    Location = (g.location.name),
                    LikesCount = g.likes.count,
                    CommentsCount = g.comments.count,
                    FollowersCount = inf.Seguidores,
                    Images = g.images.thumbnail,
                    Carousel = g.images.thumbnail,
                    lat = g.location.latitude,
                    lng = g.location.longitude
                });
            var lstLocationMapsDist = objMediaRecent.Where(y => y.location != null)
                .Select(g => new
                {
                    Location = (g.location.name),
                    LikesCount = g.likes.count,
                    CommentsCount = g.comments.count,
                    FollowersCount = inf.Seguidores,
                    Images = g.images.thumbnail,
                    Carousel = g.images.thumbnail,
                    lat = g.location.latitude,
                    lng = g.location.longitude
                }).ToList().DistinctBy(x => x.Location);
            var lstMaps = lstLocationMapsDist.Where(y => string.IsNullOrEmpty(y.Location) != true)
                    .Select(x => new Models.DTO.InstaMentions()
                    {

                        UserName = x.Location,
                        Used = Math.Round(Convert.ToDouble
                    (
                        Convert.ToDouble(lstLocationMaps.Count(c => c.Location == x.Location
                        ))
                    ), 0),
                        UsedPerc = Math.Round(Convert.ToDouble
                    (
                            (
                                Convert.ToDouble(lstLocationMaps.Count(c => c.Location == x.Location
                                ))
                            /
                                Convert.ToDouble(lstLocationMaps.Count())
                            )
                    ) * 100, 4),
                        Reach = lstLocationMaps.Where(c => c.Location == x.Location
                    )
                        .Sum(s => Convert.ToInt32(s.CommentsCount) + s.LikesCount),
                        Engagemer = Math.Round(
                                Convert.ToDouble(
                                (Convert.ToDouble(
                                        lstLocationMaps.Where(c => c.Location == x.Location
                                        )
                                        .Sum(s => Convert.ToInt32(s.CommentsCount))
                                    ) +
                                    Convert.ToDouble(
                                        lstLocationMaps.Where(c => c.Location == x.Location
                                        )
                                            .Sum(s => Convert.ToInt32(s.LikesCount))
                                        )
                                )
                                /
                                Convert.ToDouble(
                                    lstLocationMaps.Where(c => c.Location == x.Location
                                    )
                                    .Sum(s => s.FollowersCount)
                                    )) * 100, 4),
                        DiffUsedEngag = 0,
                        Imagens = new List<string>()
                            {
                            (   x.Images.url)
                            }
                    }).ToList();
            inf.LstInstaMaps = lstMaps.ToList();

            var lstMapsMarcadores = lstLocationMaps.Where(y => string.IsNullOrEmpty(y.Location) != true);
            #region Maps Places
            string markers = "[";
            foreach (var it in lstMapsMarcadores)
            {
                markers += "{";
                markers += string.Format("'title': '{0}',", it.Location.Replace("'", " "));
                markers += string.Format("'lat': '{0}',", it.lat);
                markers += string.Format("'lng': '{0}',", it.lng);
                markers += string.Format("'description': '{0}',", (it.Location.Replace("'", " ")));
                markers += string.Format("'image': '{0}'", (it.Images.url));
                markers += "},";
            }
            markers += "];";
            ViewBag.Markers = markers;

            #endregion

            var lstPhotoTagsInterna = new List<Models.DTO.InstaMentions>();

            var lstPhotoTags = objMediaRecent
                .Where(w => w.users_in_photo.Count() > 0);

            foreach (var it in lstPhotoTags)
            {
                foreach (var tg in it.tags)
                {
                    lstPhotoTagsInterna.Add(
                        new Models.DTO.InstaMentions()
                        {
                            UserName = (string)tg,
                            Used = 1,
                            UsedPerc = 0,
                            Reach = it.likes.count + Convert.ToInt32(it.comments.count),
                            Engagemer = 0,
                            DiffUsedEngag = 1,
                            Imagens = new List<string>()
                        {
                            (it.images.thumbnail.url)
                        }
                        }
                        );
                }
            }
            var lstPhotoTagsFull = lstPhotoTagsInterna.Select(x => new Models.DTO.InstaMentions()
            {
                UserName = x.UserName,
                Used = lstPhotoTagsInterna
                        .Where(p => p.UserName == x.UserName)
                        .Count(),
                UsedPerc = Math.Round(
                        (
                        Convert.ToDouble(lstPhotoTagsInterna
                        .Where(p => p.UserName == x.UserName)
                        .Count())
                        /
                        Convert.ToDouble(
                            lstPhotoTagsInterna.Count()
                            )
                        ), 4),
                Reach = lstPhotoTagsInterna
                        .Where(p => p.UserName == x.UserName)
                        .Sum(s => s.Reach),
                Engagemer = 0,
                DiffUsedEngag = 1,
                Imagens = lstPhotoTagsInterna
                        .Where(p => p.UserName == x.UserName)
                        .Select(s => s.Imagens.FirstOrDefault())
                        .ToList()
            }).ToList();

            inf.LstInstaPhotoTags = lstPhotoTagsFull
                .DistinctBy(d => d.UserName).ToList();

            return View(inf);
        }

        public async Task<Models.FaceDetection> getAnnotation(string _urlImagem)
        {
            string value = Environment.GetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS");

            var repMongo = new Repository.MongoRep("", _settings);
            var _mediaId = Guid.NewGuid().ToString();
            try
            {

                var desc = string.Empty;
                WebClient webClient = new WebClient();
                var remoteFileUrl = _urlImagem;
                var _urlLocal = @"c:\output\" + _mediaId + "_imgEmotional.jpg";
                webClient.DownloadFile(remoteFileUrl, _urlLocal);

                await repMongo.GravarOne<string>("DetectFaces:: " + DateTime.Now.ToString());
                int count = 1;
                int joy = 0;
                int anger = 0;
                int sorrow = 0;
                int surprise = 0;
                var response = GetAnnotationDetectionFace(_urlLocal);
                if (response != null && (response.responses != null) && (response.responses.Count > 0))
                {
                    foreach (var faceAnnotation in response.responses.FirstOrDefault().faceAnnotations)
                    {
                        Console.WriteLine("Face {0}:", count++);
                        joy += LikedFace.ConvertFace(faceAnnotation.joyLikelihood);
                        anger += LikedFace.ConvertFace(faceAnnotation.angerLikelihood);
                        sorrow += LikedFace.ConvertFace(faceAnnotation.sorrowLikelihood);
                        surprise += LikedFace.ConvertFace(faceAnnotation.surpriseLikelihood);

                    }
                }

                var it = new Models.FaceDetection()
                {
                    Joy = joy == 0 ? 0 : 1,
                    Anger = anger == 0 ? 0 : 1,
                    Sorrow = sorrow == 0 ? 0 : 1,
                    Surprise = surprise == 0 ? 0 : 1,
                    DtAvaliacao = DateTime.Now,
                    UrlImagem = _urlImagem,
                    MediaId = _mediaId
                };
                return it;
            }
            catch (Exception ex)
            {
                await repMongo.GravarOne<string>("Exception em getAnnotation:: " + ex.Message.ToString() + ":: stacktrace::" + ex.StackTrace.ToString());
                if (ex.InnerException != null)
                {
                    await repMongo.GravarOne<string>(ex.InnerException.Message.ToString());
                }

                var it = new Models.FaceDetection()
                {
                    Joy = 0,
                    Anger = 0,
                    Sorrow = 0,
                    Surprise = 0,
                    DtAvaliacao = DateTime.Now,
                    UrlImagem = _urlImagem
                };
                return it;
            }
        }

        public Models.AnnotationFaceDetection GetAnnotationDetectionFace(string _urlLocal)
        {
            var key = "AIzaSyD5ubW6Dwv0NgRYbWzeh2dCMhZ9dBmcGEc";// "AIzaSyBwInxtzdKkaXWT8wysGFVrKHBGYM4wdtU";
            var req = new Models.FaceDetection();
            var base64String = "";
            byte[] imageBytes;

            try
            {
                using (Image image = Image.FromFile(_urlLocal))
                {
                    using (var m = new MemoryStream())
                    {
                        image.Save(m, image.RawFormat);
                        imageBytes = m.ToArray();

                        base64String = Convert.ToBase64String(imageBytes);
                    }
                }

                var imageParts = base64String.Split(',').ToList<string>();
                using (var client = new WebClient())
                {
                    var Mainrequests = new Models.OptionRequestFaceDetection()
                    {
                        requests = new List<Models.requests>()
                        {
                             new Models.requests()
                            {
                                 image = new Models.image()
                                 {
                                    content = imageParts[0]
                                 },

                                 features = new List<Models.features>()
                                 {
                                     new Models.features()
                                     {
                                         type = "FACE_DETECTION"
                                     }
                                 }
                            }
                        }
                    };
                    var uri = "https://vision.googleapis.com/v1/images:annotate?key=" + key;
                    client.Headers.Add("Content-Type:application/json");
                    client.Headers.Add("Accept:application/json");
                    var response = client.UploadString(uri, JsonConvert.SerializeObject(Mainrequests));
                    var ret = Json(data: response);

                    var retClass = JsonConvert.DeserializeObject<Models.AnnotationFaceDetection>(response.ToString());
                    return retClass;
                }
            }
            catch (Exception ex)
            {
                var repMongo = new Repository.MongoRep("", _settings);
                repMongo.GravarOne<string>(ex.Message.ToString() + "::" + ex.StackTrace.ToString());

                return null;
            }
        }

        public async Task<Models.CreditoMetricas> GetCredito(string UserId, Repository.MongoRep repMongo = null)
        {
            if (repMongo == null)
            {
                repMongo = new Repository.MongoRep("", _settings);
            }
            var list = await repMongo.ListarCreditos(UserId);
            if (list == null || (list.Count == 0)) return null;

            list = list.Where(w =>  //TODO: debitos na conta...
                (w.Obj.DataValidade >= DateTime.Now)).ToList();

            if (list == null || (list.Count == 0)) return null;

            var cred = new Models.CreditoMetricas()
            {
                DataCredito = list.Max(m => m.Obj.DataCredito),
                DataCriacao = DateTime.Now,
                DataValidade = list.Max(m => m.Obj.DataValidade),
                Qtd = list.Sum(m => m.Obj.Qtd),
                Debito = list.Sum(m => m.Obj.Debito),
                UserId = UserId
            };
            return cred;
        }

        public async Task<Models.CreditoMetricas> InserirCredito(string UserId, int qtd, Repository.MongoRep repMongo = null)
        {
            if (repMongo == null)
            {
                repMongo = new Repository.MongoRep("", _settings);
            }

            var cred = new Models.CreditoMetricas()
            {
                DataCredito = DateTime.Now,
                DataCriacao = DateTime.Now,
                DataValidade = DateTime.Now.AddMonths(1),
                Qtd = qtd,
                Debito = 0,
                UserId = HttpContext.Session.GetString("UserId")
            };
            await repMongo.GravarOne<Models.CreditoMetricas>(cred);

            return cred;
        }

        public async Task<Models.CreditoMetricas> Debitar(string UserId, int qtd, Repository.MongoRep repMongo = null)
        {
            if (repMongo == null)
            {
                repMongo = new Repository.MongoRep("", _settings);
            }
            var list = await repMongo.ListarCreditos(UserId);
            if (list.Count() > 0)
            {
                var lstCreditos = list.Where(x => x.Obj.Qtd != x.Obj.Debito);
                var item = lstCreditos
                    .Where(w => w.Obj.DataValidade == lstCreditos.Min(m => m.Obj.DataValidade)).FirstOrDefault();
                if (item != null)
                {
                    item.Obj.Debito = item.Obj.Debito + 1;
                    await repMongo.AlterarCredito(item);
                    return item.Obj;
                }
                else
                {
                    //sem saldo
                    var cred = new Models.CreditoMetricas()
                    {
                        DataCredito = DateTime.Now,
                        DataCriacao = DateTime.Now,
                        DataValidade = DateTime.Now,
                        Qtd = 0,
                        Debito = 0,
                        UserId = UserId
                    };
                    return cred;
                }
            }
            else
            {
                var cred = new Models.CreditoMetricas()
                {
                    DataCredito = DateTime.Now,
                    DataCriacao = DateTime.Now,
                    DataValidade = DateTime.Now,
                    Qtd = 0,
                    Debito = 0,
                    UserId = UserId
                };
                return cred;
            }
        }

        public ActionResult ComeceAqui(string msg = "")
        {
            HttpContext.Session.Remove("userType");
            HttpContext.Session.Remove("userNameTitle");
            if (msg == "1")
            {
                ViewBag.Executar = "alert('Dados obrigatórios não preenchidos!');";
            }
            if (msg == "2")
            {
                ViewBag.Executar = "alert('Problemas ao cadastras!');";
            }

            return View();
        }
        public ActionResult ComeceAquiOld(string msg = "")
        {
            HttpContext.Session.Remove("userType");
            HttpContext.Session.Remove("userNameTitle");
            if (msg == "1")
            {
                ViewBag.Executar = "alert('Dados obrigatórios não preenchidos!');";
            }
            if (msg == "2")
            {
                ViewBag.Executar = "alert('Problemas ao cadastras!');";
            }

            return View();
        }

        public ActionResult MetricasInsightsOld(string cd, string cliente = "")
        {
            if (string.IsNullOrEmpty(cd))
            {
                ViewBag.Message = "Url incorreta.";
                return View();
            }

            var executeJavascript = "_userId=\"" + cd.ToString() + "\";";
            executeJavascript += "_clienteId=\"" + cliente.ToString() + "\";";
            executeJavascript += " verifyFacebookToken();";

            ViewBag.ExecuteGetToken = executeJavascript;

            return View();
        }

        public ActionResult MetricasInsights(string cd, string cliente = "")
        {
            if (string.IsNullOrEmpty(cd))
            {
                ViewBag.Message = "Url incorreta.";
                return View();
            }

            var executeJavascript = "_userId=\"" + cd.ToString() + "\";";
            executeJavascript += "_clienteId=\"" + cliente.ToString() + "\";";
            executeJavascript += " verifyFacebookToken();";

            ViewBag.ExecuteGetToken = executeJavascript;

            return View();
        }

        public async Task<ActionResult> MinhasAnalisesOld(string msg = "")
        {
            return await MinhasAnalises(msg);
        }
        public async Task<ActionResult> MinhasAnalises(string msg = "")
        {
            var UserId = HttpContext.Session.GetString("UserId");
            Repository.MongoRep repMongo = new Repository.MongoRep("", _settings);
            if (string.IsNullOrEmpty(UserId))
            {
                return RedirectToAction("Login", null, null, "influenciador");
            }
            else
            {
                ViewBag.ExecutarJavascript = "";
                if (!string.IsNullOrEmpty(msg))
                {
                    if (msg == "1")
                    {
                        ViewBag.ExecutarJavascript = "abrirplanos();";
                    }
                }

                var lstAutorizacoes = new List<Models.AutorizacaoMetrica>();
                var lstUserIds = await repMongo.ListarUsuariosByUser(UserId);
                foreach (var idUser in lstUserIds)
                {
                    var lstUserI = (await repMongo.ListarGraphUserId<Models.Graph.Usuario>(idUser._id.ToString())).FirstOrDefault();
                    var lstMediaI = await repMongo.ListarGraphUserId<Models.Graph.Media>(idUser._id.ToString());

                    if (lstUserI != null && lstMediaI != null &&
                        (lstUserI.Obj != null && (lstMediaI.Count > 0)
                        ))
                    {
                        var autorizacoes = new Models.AutorizacaoMetrica()
                        {
                            DataCriacao = lstUserI.DateCreation,
                            Email = lstUserI.Obj.username,
                            UsuarioInstagram = lstUserI.Obj.username,
                            Status = EnumStatus.PROCESSADO,
                            Seguidores = Convert.ToInt32(lstUserI.Obj.followers_count.ToString()),
                            _id = lstUserI._id.ToString(),
                            Engajamento = lstMediaI.Where(w => w.DateCreation.Date == lstUserI.DateCreation.Date)
                                            .FirstOrDefault().Obj.data.Sum(u => u.like_count + u.comments_count) /
                                            lstMediaI.Where(w => w.DateCreation.Date == lstUserI.DateCreation.Date)
                                            .FirstOrDefault().Obj.data.Count(),
                            PowerFull = Math.Round((Convert.ToDouble(
                                    CalculoPowerful(repMongo.ListarGraphUserId<Models.Graph.InsightsGenderAge>(UserId, lstUserI.DateCreation).Result.ToList())
                                ) /
                                         Convert.ToDouble(lstUserI.Obj.followers_count)) * 100, 2),
                            Aprovado = (((Convert.ToDouble(
                                CalculoPowerful(repMongo.ListarGraphUserId<Models.Graph.InsightsGenderAge>(UserId, lstUserI.DateCreation).Result.ToList())
                                ) /
                                         Convert.ToDouble(lstUserI.Obj.followers_count)) * 100)
                            < valor0) ? 0 : (((Convert.ToDouble(
                                CalculoPowerful(repMongo.ListarGraphUserId<Models.Graph.InsightsGenderAge>(UserId, lstUserI.DateCreation).Result.ToList())
                                ) /
                                         Convert.ToDouble(lstUserI.Obj.followers_count)) * 100)
                            < valor1) ? 1 : 2,
                            TimeSpan = Convert.ToDateTime(new DateTime(01, 01, 01, 0, 0, 0).Add(lstUserI.timeSpan)),
                            Reprocessar = (lstUserI.DateCreation.AddHours(24) < DateTime.Now)
                        };
                        lstAutorizacoes.Add(autorizacoes);

                        var picture = lstUserI.Obj.profile_picture_url;
                        HttpContext.Session.SetString("ProfilePicture", picture);
                        ViewBag.ProfileFotoURI = picture;
                        ViewBag.NameUser = lstUserI.Obj.username;
                        HttpContext.Session.SetString("userNameTitle", lstUserI.Obj.username);
                        //HttpContext.Session.SetString("nomeagencia", lstUserI.Obj.username);
                    }
                }

                var _lst = new Models.DTO.AutorizarMetricaPage()
                {
                    autorizacaoMetricas = lstAutorizacoes,
                    autorizacaoMetrica = new Models.AutorizacaoMetrica()
                    {
                        UsuarioId = UserId
                    }
                };
                if (_lst.autorizacaoMetricas != null)
                {
                    var lstMeses = _lst.autorizacaoMetricas
                        .Select(s =>
                        new Models.AutorizacaoMetrica()
                        {
                            DataCriacao = new DateTime(s.DataCriacao.Year, s.DataCriacao.Month, 1)
                        }).DistinctBy(x => x.DataCriacao);
                    _lst.Months = lstMeses;
                }
                else
                {
                    _lst.Months = new List<Models.AutorizacaoMetrica>();
                }
                ViewBag.CreditoMetricas = await ValorCredito(UserId, repMongo);

                var executeJavascript = "_userId=\"" + UserId.ToString() + "\";";
                if (string.IsNullOrEmpty(HttpContext.Session.GetString("access_token_page")))
                {
                    executeJavascript += " verifyFacebookToken(); checkLoginState();";
                    executeJavascript += " $('.modal-backdrop.fade.show').css('display', 'none');";

                    var picture = "https://gastroahotel.cz/files/2014/10/silueta.jpg";
                    HttpContext.Session.SetString("ProfilePicture", picture);
                    ViewBag.ProfileFotoURI = picture;
                    ViewBag.NameUser = "Nome do usuário";
                    HttpContext.Session.SetString("userNameTitle", "Nome do usuário");



                }
                else
                {
                    if (ViewBag.CreditoMetricas == 0) ViewBag.ExecutarJavascript = "abrirplanos();";
                }
                ViewBag.ExecuteGetToken = executeJavascript;

                return View(_lst);
            }
        }

        private async Task<int?> ValorCredito(string UserId, Repository.MongoRep repMongo)
        {
            var _credito = await GetCredito(UserId, repMongo);
            if (_credito != null)
            {
                return (_credito.Qtd - _credito.Debito);
            }
            return null;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SolicitacaoMetricas(AutorizarMetricaPage lista)
        {
            var msg = "";
            var UserId = HttpContext.Session.GetString("UserId");
            var access_token = HttpContext.Session.GetString("access_token_page");
            var _instagram_business_account = HttpContext.Session.GetString("instagram_business_account");

            var retClass = JsonConvert.DeserializeObject<string[][]>(lista.UsuariosInstagram);//.Replace("[","").Replace("]",""));
            List<AutorizacaoMetrica> lstAutorizacao = new List<AutorizacaoMetrica>();

            var repMongoFull = new Repository.MongoRep(UserId, _settings, UserId);
            var saldo = await ValorCredito(UserId, repMongoFull);

            for (var id = 0; id < retClass.Count(); id++)
            {
                foreach (var it in retClass[0])
                {
                    if (saldo > 0)
                    {
                        var auth = new AutorizacaoMetrica()
                        {
                            Status = EnumStatus.SOLICITADO,
                            UsuarioInstagram = it,
                            DataCriacao = DateTime.Now,
                            Key = Guid.NewGuid().ToString(),
                            UsuarioId = "",
                            AgenciaUserId = UserId,
                            Client = lista.Client
                        };

                        lstAutorizacao.Add(auth);

                        var repMongo = new Repository.MongoRep(it, _settings);
                        await repMongo.GravarOne<AutorizacaoMetrica>(auth);

                        var result = await ConsultaBasica(access_token, _instagram_business_account, it);
                        if (!string.IsNullOrEmpty(result))
                        {
                            msg += result + ",";
                        }
                        else
                        {
                            saldo--;
                            await Debitar(UserId, 1, repMongoFull);
                        }
                    }
                    else
                    {
                        msg += "Saldo de créditos acabou.";
                    }
                }
            }

            if (msg == "")
            {
                return RedirectToAction("historicometricas");
            }
            else
            {
                ViewBag.Msg = "alert('Influencers (" + msg.Substring(0, msg.Length - 1) + ") sem conta business ou conta privada.');";
                return View("SolicitacaoMetricas");
            }
        }

        public async Task<ActionResult> SolicitacaoMetricas()
        {
            //List<Models.AutorizacaoMetrica> auto = new List<Models.AutorizacaoMetrica>();

            var UserId = HttpContext.Session.GetString("UserId");
            var CodeUsuario = HttpContext.Session.GetString("UsuarioFull_id");
            var nomeAgencia = HttpContext.Session.GetString("nomeagencia");

            Repository.MongoRep repMongo = new Repository.MongoRep(UserId, _settings, UserId);
            var user = await repMongo.ListarById<Models.Usuario>(new ObjectId(UserId));
            var picture = await LoadPictureProfile(UserId, repMongo, user.FirstOrDefault().Obj.Tipo);
            HttpContext.Session.SetString("ProfilePicture", picture);
            ViewBag.ProfilePicture = picture;
            ViewBag.NameUser = nomeAgencia;
            HttpContext.Session.SetString("userNameTitle", nomeAgencia);

            try
            {
                if (string.IsNullOrEmpty(UserId))
                {
                    return RedirectToAction("Login");
                }

                ViewBag.MyCode = CodeUsuario;
                ViewBag.NomeAgencia = nomeAgencia;

                return View();
            }
            catch (Exception)
            {
                return RedirectToAction("HistoricoMetricas");
            }
        }

        //public async Task<ActionResult> HistoricoMetricas(string msg = "")
        //{
        //    var UserId = HttpContext.Session.GetString("UserId");
        //    Repository.MongoRep repMongo = new Repository.MongoRep("", _settings);
        //    if (string.IsNullOrEmpty(UserId))
        //    {
        //        return RedirectToAction("Login", null, null, "influenciador");
        //    }
        //    else
        //    {
        //        ViewBag.ExecutarJavascript = "";
        //        if (!string.IsNullOrEmpty(msg))
        //        {
        //            if (msg == "1")
        //            {
        //                ViewBag.ExecutarJavascript = "abrirplanos();";
        //            }
        //        }
        //        var lstAutorizacoes = new List<Models.AutorizacaoMetrica>();
        //        var lstUserIds = await repMongo.ListarUsuariosByUser(UserId);
        //        foreach (var idUser in lstUserIds)
        //        {
        //            var lstUserI = (await repMongo.ListarGraphUserId<Models.Graph.Usuario>(idUser._id.ToString())).FirstOrDefault();
        //            var lstMediaI = await repMongo.ListarGraphUserId<Models.Graph.Media>(idUser._id.ToString());
        //            if (lstUserI != null && lstMediaI != null &&
        //                (lstUserI.Obj != null && (lstMediaI.Count > 0)
        //                ))
        //            {
        //                var autorizacoes = new Models.AutorizacaoMetrica()
        //                {
        //                    DataCriacao = lstUserI.DateCreation,
        //                    Email = lstUserI.Obj.username,
        //                    UsuarioInstagram = lstUserI.Obj.username,
        //                    Status = EnumStatus.PROCESSADO,
        //                    Seguidores = Convert.ToInt32(lstUserI.Obj.followers_count.ToString()),
        //                    _id = lstUserI._id.ToString(),
        //                    Engajamento = lstMediaI.Where(w => w.DateCreation.Date == lstUserI.DateCreation.Date)
        //                                    .FirstOrDefault().Obj.data.Sum(u => u.like_count + u.comments_count) /
        //                                    lstMediaI.Where(w => w.DateCreation.Date == lstUserI.DateCreation.Date)
        //                                    .FirstOrDefault().Obj.data.Count(),
        //                    PowerFull = Math.Round((Convert.ToDouble(
        //                            CalculoPowerful(repMongo.ListarGraphUserId<Models.Graph.InsightsGenderAge>(UserId, lstUserI.DateCreation).Result.ToList())
        //                        ) /
        //                                 Convert.ToDouble(lstUserI.Obj.followers_count)) * 100, 2),
        //                    Aprovado = (((Convert.ToDouble(
        //                        CalculoPowerful(repMongo.ListarGraphUserId<Models.Graph.InsightsGenderAge>(UserId, lstUserI.DateCreation).Result.ToList())
        //                        ) /
        //                                 Convert.ToDouble(lstUserI.Obj.followers_count)) * 100)
        //                    < valor0) ? 0 : (((Convert.ToDouble(
        //                        CalculoPowerful(repMongo.ListarGraphUserId<Models.Graph.InsightsGenderAge>(UserId, lstUserI.DateCreation).Result.ToList())
        //                        ) /
        //                                 Convert.ToDouble(lstUserI.Obj.followers_count)) * 100)
        //                    < valor1) ? 1 : 2,
        //                    TimeSpan = Convert.ToDateTime(new DateTime(01, 01, 01, 0, 0, 0).Add(lstUserI.timeSpan)),
        //                    Reprocessar = (lstUserI.DateCreation.AddHours(24) < DateTime.Now)
        //                };
        //                lstAutorizacoes.Add(autorizacoes);
        //                ViewBag.ProfileFotoURI = lstUserI.Obj.profile_picture_url;
        //                ViewBag.NameUser = lstUserI.Obj.username;
        //            }
        //        }
        //        var _lst = new Models.DTO.AutorizarMetricaPage()
        //        {
        //            autorizacaoMetricas = lstAutorizacoes,
        //            autorizacaoMetrica = new Models.AutorizacaoMetrica()
        //            {
        //                UsuarioId = UserId
        //            }
        //        };
        //        if (_lst.autorizacaoMetricas != null)
        //        {
        //            var lstMeses = _lst.autorizacaoMetricas
        //                .Select(s =>
        //                new Models.AutorizacaoMetrica()
        //                {
        //                    DataCriacao = new DateTime(s.DataCriacao.Year, s.DataCriacao.Month, 1)
        //                }).DistinctBy(x => x.DataCriacao);
        //            _lst.Months = lstMeses;
        //        }
        //        else
        //        {
        //            _lst.Months = new List<Models.AutorizacaoMetrica>();
        //        }
        //        ViewBag.CreditoMetricas = await ValorCredito(UserId, repMongo);
        //        var executeJavascript = "_userId=\"" + UserId.ToString() + "\";";
        //        if (string.IsNullOrEmpty(HttpContext.Session.GetString("access_token_page")))
        //        {
        //            executeJavascript += " verifyFacebookToken();";
        //            ViewBag.ProfileFotoURI = "https://gastroahotel.cz/files/2014/10/silueta.jpg";
        //            ViewBag.NameUser = "Nome do usuário";
        //        }
        //        else
        //        {
        //            if (ViewBag.CreditoMetricas == 0) ViewBag.ExecutarJavascript = "abrirplanos();";
        //        }
        //        ViewBag.ExecuteGetToken = executeJavascript;
        //        return View("HistoricoMetricas", _lst);
        //    }
        //}

        public async Task<ActionResult> HistoricoMetricasOld(string msg = "", string client = "")
        {
            if (string.IsNullOrEmpty(client))
            {
                client = "";
            }
            if (msg == "3")
            {
                ViewBag.ExecutarJS = "abrirModal('#senhainvalida','Não conseguimos resetar sua senha<br />verifique se o email digitado esta correto e tente novamente.')";
            }

            if (msg == "1")
            {
                ViewBag.ExecutarJS = "abrirModal('#senhainvalida','Parabéns, seu relatório foi processado com sucesso.')";
            }

            if (msg == "4")
            {
                ViewBag.ExecutarJS = "abrirModal('#senhainvalida',' Para reprocessar esse relatório<br />Você precisa refazer o pagamento de seu plano')";
            }

            if (msg == "6")
            {
                ViewBag.ExecutarJS = "abrirModal('#senhainvalida','Você precisa refazer o pagamento de seu plano.')";
            }

            if (msg == "2")
            {
                ViewBag.ExecutarJS = "abrirModal('#senhainvalida','Seja bem vindo <br /> A partir de agora você terá acesso a informações<br />que o colocará em um nível superior no mercado em que atua')";
            }

            var NomeAgencia = HttpContext.Session.GetString("nomeagencia");
            if (string.IsNullOrEmpty(NomeAgencia))
            {
                return RedirectToAction("MinhasAnalises");
            }

            var _id = HttpContext.Session.GetString("UsuarioFull_id");
            var UserId = HttpContext.Session.GetString("UsuarioFull_id");
            Repository.MongoRep repMongo = new Repository.MongoRep("", _settings);
            if (string.IsNullOrEmpty(_id))
            {
                return RedirectToAction("Login", null, null, "agencia");
            }
            else
            {
                var user = await repMongo.ListarById<Models.Usuario>(new ObjectId(UserId));
                var picture = await LoadPictureProfile(UserId, repMongo, user.FirstOrDefault().Obj.Tipo);
                HttpContext.Session.SetString("ProfilePicture", picture);
                ViewBag.ProfilePicture = picture;
                ViewBag.NameUser = NomeAgencia;
                HttpContext.Session.SetString("userNameTitle", NomeAgencia);

                var lstUsers = await repMongo.ListarUserIdByAgencia<Models.Usuario>(_id);
                var UserIds = lstUsers.Select(s => s._id.ToString()).ToList();
                var lstUsersGraphs = (await repMongo.ListarGraphIdByAgencia<Models.Graph.Usuario>(UserIds)).ToList();
                var lstMediaGraphs = (await repMongo.ListarGraphIdByAgencia<Models.Graph.Media>(UserIds)).ToList();
                var lstTokensBloqueados = (await repMongo.ListarGraphIdByAgencia<InfluencersMetricsService.Model.UserBloqueios>(UserIds));
                if (lstTokensBloqueados.Count() > 0)
                {
                    lstTokensBloqueados = lstTokensBloqueados.Where(w => w.Obj.Status == "Bloqueio").ToList();
                }

                var lstDiscoveries = (await repMongo.ListarGraphIdByAgencia<Models.Graph.Discovery>(UserId.ToString())).ToList();
                var lstCreditosUsuarios = await repMongo.ListarCreditosPorAgencia(UserId);

                List<Models.AutorizacaoMetrica> lstAutorizacaoMetricas = new List<AutorizacaoMetrica>();
                lstUsersGraphs.ForEach(f =>
                {
                    if (f.Obj != null)
                    {
                        var credLiberado = false;
                        var _usuario = lstUsers.Where(w => w._id.ToString() == f.UsuarioId).FirstOrDefault();
                        if (_usuario != null)
                        {
                            if (_usuario.Obj.StatusCredito != null)
                            {
                                credLiberado = _usuario.Obj.StatusCredito == EnumStatus.DISPONIVEL;
                            }
                        }

                        var firstMedia = lstMediaGraphs.Where(w => w.UsuarioId == f.UsuarioId.ToString()
                                && w.DateCreation == f.DateCreation).FirstOrDefault();

                        if (firstMedia != null && ((firstMedia.Obj != null) && (firstMedia.Obj.data != null && (firstMedia.Obj.data.Count() > 0))))
                        {
                            lstAutorizacaoMetricas.Add(new Models.AutorizacaoMetrica()
                            {
                                DataCriacao = f.DateCreation,
                                Email = f.Obj.username,
                                UsuarioInstagram = f.Obj.username,
                                Status = EnumStatus.PROCESSADO,
                                Seguidores = Convert.ToInt32(f.Obj.followers_count.ToString()),
                                _id = f._id.ToString(),
                                ProfilePictureUrl = f.Obj.profile_picture_url.ToString(),
                                Alcance = Convert.ToInt32(f.Obj.followers_count.ToString()),
                                Engajamento = Convert.ToInt32(
                                    firstMedia.Obj.data.Average(x => x.like_count) +
                                    firstMedia.Obj.data.Average(x => x.comments_count)
                                ),
                                PowerFull = Math.Round(((firstMedia.Obj.data.OrderByDescending(o => o.timestamp).Take(5).Average(a => a.like_count) +
                                                            firstMedia.Obj.data.OrderByDescending(o => o.timestamp).Take(5).Average(a => a.comments_count))
                                                            /
                                                            Convert.ToInt64(f.Obj.followers_count)) * 100, 2),
                                Reprocessar = f.Obj.reprocessado != true ? f.DateCreation.Date < DateTime.Now.Date : false,
                                UsuarioId = f.UsuarioId,
                                LiberadoCredito = credLiberado,
                                ProblemasToken = (lstTokensBloqueados.Exists(e => e.UsuarioId == f.UsuarioId.ToString())),
                                TimeSpan = Convert.ToDateTime(new DateTime(01, 01, 01, 0, 0, 0).Add(_usuario.timeSpan)),
                                Client = _usuario.Obj.NomeAgencia
                            });
                        }
                        else
                        {
                            lstAutorizacaoMetricas.Add(new Models.AutorizacaoMetrica()
                            {
                                DataCriacao = f.DateCreation,
                                Email = f.Obj.username,
                                UsuarioInstagram = f.Obj.username,
                                Status = EnumStatus.PENDENTE,
                                Seguidores = Convert.ToInt32(f.Obj.followers_count.ToString()),
                                _id = f._id.ToString(),
                                ProfilePictureUrl = f.Obj.profile_picture_url.ToString(),
                                Alcance = Convert.ToInt32(f.Obj.followers_count.ToString()),
                                Engajamento = 0,
                                PowerFull = 0,
                                UsuarioId = f.UsuarioId,
                                LiberadoCredito = credLiberado,
                                Reprocessar = f.Obj.reprocessado != true ? f.DateCreation.Date < DateTime.Now.Date : false,
                                ProblemasToken = (lstTokensBloqueados.Exists(e => e.UsuarioId == f.UsuarioId.ToString())),
                                TimeSpan = Convert.ToDateTime(new DateTime(01, 01, 01).Add(_usuario.timeSpan)),
                                Client = _usuario.Obj.NomeAgencia
                            });
                        }
                    }
                });

                lstDiscoveries.ForEach(f =>
                {
                    {
                        lstAutorizacaoMetricas.Add(new Models.AutorizacaoMetrica()
                        {
                            DataCriacao = f.DateCreation,
                            Email = f.UsuarioInstagram,
                            UsuarioInstagram = f.UsuarioInstagram,
                            Status = EnumStatus.DISPONIVEL,
                            Seguidores = Convert.ToInt32(f.Obj.business_discovery.followers_count.ToString()),
                            _id = f._id.ToString(),
                            ProfilePictureUrl = f.Obj.business_discovery.profile_picture_url.ToString(),
                            Alcance = Convert.ToInt64(f.Obj.business_discovery.followers_count),
                            Engajamento = Convert.ToInt64(
                                f.Obj.business_discovery.media.data.Sum(a => a.comments_count) +
                            f.Obj.business_discovery.media.data.Sum(a => a.like_count)
                            ),
                            PowerFull = Math.Round(((
                                        f.Obj.business_discovery.media.data.Average(av => av.like_count) +
                                        f.Obj.business_discovery.media.data.Average(av => av.comments_count))
                                                        /
                                                        Convert.ToInt64(f.Obj.business_discovery.media_count)) * 100, 2),
                            Reprocessar = f.Obj.reprocessado == false ? f.DateCreation.Date < DateTime.Now.Date : false,
                            TimeSpan = f.DateCreation
                        });
                    }
                });

                var _lst = new Models.DTO.AutorizarMetricaPage()
                {
                    autorizacaoMetricas = lstAutorizacaoMetricas.Where(w => w.Client == client || client == "").ToList(),
                    autorizacaoMetrica = new Models.AutorizacaoMetrica()
                    {
                        UsuarioId = _id
                    }
                };

                var _clientes = lstAutorizacaoMetricas.DistinctBy(d => d.Client).Select(s =>
                    string.Format("{0}", s.Client)
                );
                _lst.Clientes = _clientes.Where(w => w != "").ToList();

                var lstMeses = _lst.autorizacaoMetricas
                    .Select(s =>
                    new Models.AutorizacaoMetrica()
                    {
                        DataCriacao = new DateTime(s.DataCriacao.Year, s.DataCriacao.Month, 1)
                    }).DistinctBy(x => x.DataCriacao);
                _lst.Months = lstMeses.OrderByDescending(o => o.DataCriacao);

                var _credito = await GetCredito(UserId, repMongo);
                if (_credito != null)
                {
                    ViewBag.CreditoMetricas = _credito.Qtd - _credito.Debito;
                }

                //var executeJavascript = "_userId=\"" + UserId.ToString() + "\";";
                //if (string.IsNullOrEmpty(HttpContext.Session.GetString("access_token_page")) && (msg == ""))
                //{
                //    executeJavascript += " verifyFacebookToken();";
                //}
                //ViewBag.ExecuteGetToken = executeJavascript;
                if (client != "")
                {
                    ViewBag.ClienteSeletcted = client;
                }

                return View(_lst);
            }
        }

        public async Task<ActionResult> HistoricoMetricas(string msg = "", string client = "")
        {
            try
            {
                if (string.IsNullOrEmpty(client))
                {
                    client = "";
                }
                if (msg == "3")
                {
                    ViewBag.ExecutarJS = "abrirModal('#senhainvalida','Não conseguimos resetar sua senha<br />verifique se o email digitado esta correto e tente novamente.')";
                }

                if (msg == "1")
                {
                    ViewBag.ExecutarJS = "abrirModal('#senhainvalida','Parabéns, seu relatório foi processado com sucesso.')";
                }

                if (msg == "4")
                {
                    ViewBag.ExecutarJS = "abrirModal('#senhainvalida',' Para reprocessar esse relatório<br />Você precisa refazer o pagamento de seu plano')";
                }

                if (msg == "6")
                {
                    ViewBag.ExecutarJS = "abrirModal('#senhainvalida','Você precisa refazer o pagamento de seu plano.')";
                }

                if (msg == "2")
                {
                    ViewBag.ExecutarJS = "abrirModal('#senhainvalida','Seja bem vindo <br /> A partir de agora você terá acesso a informações<br />que o colocará em um nível superior no mercado em que atua')";
                }

                var NomeAgencia = HttpContext.Session.GetString("nomeagencia");
                if (string.IsNullOrEmpty(NomeAgencia))
                {
                    return RedirectToAction("MinhasAnalises");
                }

                var _id = HttpContext.Session.GetString("UsuarioFull_id");
                var UserId = HttpContext.Session.GetString("UsuarioFull_id");
                Repository.MongoRep repMongo = new Repository.MongoRep("", _settings);
                if (string.IsNullOrEmpty(_id))
                {
                    return RedirectToAction("Login", null, null, "agencia");
                }
                else
                {
                    var user = await repMongo.ListarById<Models.Usuario>(new ObjectId(UserId));
                    var picture = await LoadPictureProfile(UserId, repMongo, user.FirstOrDefault().Obj.Tipo);
                    HttpContext.Session.SetString("ProfilePicture", picture);
                    ViewBag.ProfilePicture = picture;
                    ViewBag.NameUser = NomeAgencia;
                    HttpContext.Session.SetString("nomeagencia", NomeAgencia);

                    var lstUsers = await repMongo.ListarUserIdByAgencia<Models.Usuario>(_id);
                    var UserIds = lstUsers.Select(s => s._id.ToString()).ToList();

                    var _lstUsersGraphs = await repMongo.ListarGraphIdByAgencia<Models.Graph.Usuario>(UserIds);
                    var _lstMediaGraphs = await repMongo.ListarGraphIdByAgencia<Models.Graph.Media>(UserIds);

                    var lstUsersGraphs = _lstUsersGraphs.ToList();
                    var lstMediaGraphs = _lstMediaGraphs.ToList();

                    var lstTokensBloqueados = (await repMongo.ListarGraphIdByAgencia<InfluencersMetricsService.Model.UserBloqueios>(UserIds));
                    if (lstTokensBloqueados.Count() > 0)
                    {
                        lstTokensBloqueados = lstTokensBloqueados.Where(w => w.Obj.Status == "Bloqueio").ToList();
                    }

                    var lstDiscoveries = (await repMongo.ListarGraphIdByAgencia<Models.Graph.Discovery>(UserId.ToString())).ToList();
                    var lstCreditosUsuarios = await repMongo.ListarCreditosPorAgencia(UserId);

                    List<Models.AutorizacaoMetrica> lstAutorizacaoMetricas = new List<AutorizacaoMetrica>();
                    lstUsersGraphs.ForEach(f =>
                    {
                        if (f.Obj != null)
                        {
                            var credLiberado = false;
                            var _usuario = lstUsers.Where(w => w._id.ToString() == f.UsuarioId).FirstOrDefault();
                            if (_usuario != null)
                            {
                                if (_usuario.Obj.StatusCredito != null)
                                {
                                    credLiberado = _usuario.Obj.StatusCredito == EnumStatus.DISPONIVEL;
                                }
                            }

                            var firstMedia = lstMediaGraphs.Where(w => w.UsuarioId == f.UsuarioId.ToString()
                                    && w.DateCreation == f.DateCreation).FirstOrDefault();

                            if (firstMedia != null && ((firstMedia.Obj != null) && (firstMedia.Obj.data != null && (firstMedia.Obj.data.Count() > 0))))
                            {
                                lstAutorizacaoMetricas.Add(new Models.AutorizacaoMetrica()
                                {
                                    DataCriacao = f.DateCreation,
                                    Email = f.Obj.username,
                                    UsuarioInstagram = f.Obj.username,
                                    Status = EnumStatus.PROCESSADO,
                                    Seguidores = Convert.ToInt32(f.Obj.followers_count.ToString()),
                                    _id = f._id.ToString(),
                                    ProfilePictureUrl = f.Obj.profile_picture_url.ToString(),
                                    Alcance = Convert.ToInt32(f.Obj.followers_count.ToString()),
                                    Engajamento = Convert.ToInt32(
                                        firstMedia.Obj.data.Average(x => x.like_count) +
                                        firstMedia.Obj.data.Average(x => x.comments_count)
                                    ),
                                    PowerFull = Math.Round(((firstMedia.Obj.data.OrderByDescending(o => o.timestamp).Take(5).Average(a => a.like_count) +
                                                                firstMedia.Obj.data.OrderByDescending(o => o.timestamp).Take(5).Average(a => a.comments_count))
                                                                /
                                                                Convert.ToInt64(f.Obj.followers_count)) * 100, 2),
                                    Reprocessar = f.Obj.reprocessado != true ? f.DateCreation.Date < DateTime.Now.Date : false,
                                    UsuarioId = f.UsuarioId,
                                    LiberadoCredito = credLiberado,
                                    ProblemasToken = (lstTokensBloqueados.Exists(e => e.UsuarioId == f.UsuarioId.ToString())),
                                    TimeSpan = Convert.ToDateTime(new DateTime(01, 01, 01, 0, 0, 0).Add(_usuario.timeSpan)),
                                    Client = _usuario.Obj.NomeAgencia
                                });
                            }
                            else
                            {
                                lstAutorizacaoMetricas.Add(new Models.AutorizacaoMetrica()
                                {
                                    DataCriacao = f.DateCreation,
                                    Email = f.Obj.username,
                                    UsuarioInstagram = f.Obj.username,
                                    Status = EnumStatus.PENDENTE,
                                    Seguidores = Convert.ToInt32(f.Obj.followers_count.ToString()),
                                    _id = f._id.ToString(),
                                    ProfilePictureUrl = f.Obj.profile_picture_url.ToString(),
                                    Alcance = Convert.ToInt32(f.Obj.followers_count.ToString()),
                                    Engajamento = 0,
                                    PowerFull = 0,
                                    UsuarioId = f.UsuarioId,
                                    LiberadoCredito = credLiberado,
                                    Reprocessar = f.Obj.reprocessado != true ? f.DateCreation.Date < DateTime.Now.Date : false,
                                    ProblemasToken = (lstTokensBloqueados.Exists(e => e.UsuarioId == f.UsuarioId.ToString())),
                                    TimeSpan = Convert.ToDateTime(new DateTime(01, 01, 01).Add(_usuario.timeSpan)),
                                    Client = _usuario.Obj.NomeAgencia
                                });
                            }
                        }
                    });

                    lstDiscoveries.ForEach(f =>
                    {
                        {
                            lstAutorizacaoMetricas.Add(new Models.AutorizacaoMetrica()
                            {
                                DataCriacao = f.DateCreation,
                                Email = f.UsuarioInstagram,
                                UsuarioInstagram = f.UsuarioInstagram,
                                Status = EnumStatus.DISPONIVEL,
                                Seguidores = Convert.ToInt32(f.Obj.business_discovery.followers_count.ToString()),
                                _id = f._id.ToString(),
                                ProfilePictureUrl = f.Obj.business_discovery.profile_picture_url.ToString(),
                                Alcance = Convert.ToInt64(f.Obj.business_discovery.followers_count),
                                Engajamento = Convert.ToInt64(
                                    f.Obj.business_discovery.media.data.Sum(a => a.comments_count) +
                                f.Obj.business_discovery.media.data.Sum(a => a.like_count)
                                ),
                                PowerFull = Math.Round(((
                                            f.Obj.business_discovery.media.data.Average(av => av.like_count) +
                                            f.Obj.business_discovery.media.data.Average(av => av.comments_count))
                                                            /
                                                            Convert.ToInt64(f.Obj.business_discovery.media_count)) * 100, 2),
                                Reprocessar = f.Obj.reprocessado == false ? f.DateCreation.Date < DateTime.Now.Date : false,
                                TimeSpan = f.DateCreation
                            });
                        }
                    });

                    var _lst = new Models.DTO.AutorizarMetricaPage()
                    {
                        autorizacaoMetricas = lstAutorizacaoMetricas.Where(w => w.Client == client || client == "").ToList(),
                        autorizacaoMetrica = new Models.AutorizacaoMetrica()
                        {
                            UsuarioId = _id
                        }
                    };

                    var _clientes = lstAutorizacaoMetricas.DistinctBy(d => d.Client).Select(s =>
                        string.Format("{0}", s.Client)
                    );
                    _lst.Clientes = _clientes.Where(w => w != "").ToList();

                    var lstMeses = _lst.autorizacaoMetricas
                        .Select(s =>
                        new Models.AutorizacaoMetrica()
                        {
                            DataCriacao = new DateTime(s.DataCriacao.Year, s.DataCriacao.Month, 1)
                        }).DistinctBy(x => x.DataCriacao);
                    _lst.Months = lstMeses.OrderByDescending(o => o.DataCriacao);

                    var _credito = await GetCredito(UserId, repMongo);
                    if (_credito != null)
                    {
                        ViewBag.CreditoMetricas = _credito.Qtd - _credito.Debito;
                    }

                    //var executeJavascript = "_userId=\"" + UserId.ToString() + "\";";
                    //if (string.IsNullOrEmpty(HttpContext.Session.GetString("access_token_page")) && (msg == ""))
                    //{
                    //    executeJavascript += " verifyFacebookToken();";
                    //}
                    //ViewBag.ExecuteGetToken = executeJavascript;
                    if (client != "")
                    {
                        ViewBag.ClienteSeletcted = client;
                    }

                    return View(_lst);
                }
            }
            catch (Exception ex)
            {
                return View("");
            }
        }

        public async Task<ActionResult> ChartJS()
        {
            return View();
        }

        public async Task<ActionResult> ViewBasica(string id)
        {
            var _id = new ObjectId(id);
            var UserId = HttpContext.Session.GetString("UserId");
            Repository.MongoRep repMongo = new Repository.MongoRep(UserId, _settings, UserId);

            if (await ValorCredito(UserId, repMongo) == 0)
            {
                ViewBag.Message = "Você ainda não possui um plano ou ele pode estar vencido <br />Clique no botão  abaixo e veja os planos disponíveis <br />  para adquirir ou renovar";
                return null;
            }

            var lstMongoUser = await repMongo.ListarById<Models.Graph.Discovery>(_id);
            var mongoUser = lstMongoUser.ToList();
            var userId = lstMongoUser.FirstOrDefault().UsuarioId;

            return await LoadViewConsultaBasica(mongoUser.FirstOrDefault().Obj, repMongo, id, "ViewFreeBasica");
        }


        public async Task<ActionResult> ViewConsulta(string id)
        {
            var _id = new ObjectId(id);
            var UserId = HttpContext.Session.GetString("UserId");
            Repository.MongoRep repMongo = new Repository.MongoRep(UserId, _settings, UserId);
            if (await ValorCredito(UserId, repMongo) == 0)
            {
                ViewBag.Message = "Você ainda não possui um plano ou ele pode estar vencido <br />Clique no botão  abaixo e veja os planos disponíveis <br />  para adquirir ou renovar";
                return null;
            }

            var lstMongoUser = await repMongo.ListarById<Models.Graph.Usuario>(_id);
            var mongoUser = lstMongoUser.ToList();
            var userId = lstMongoUser.FirstOrDefault().UsuarioId;

            return await LoadViewConsulta(mongoUser, repMongo, userId, "ViewFree");
        }

        public async Task<ActionResult> ViewFreeConsulta(string key)
        {
            var id = new ObjectId(key);
            var UserId = HttpContext.Session.GetString("UserId");

            if (UserId == null) return RedirectToAction("login");
            Repository.MongoRep repMongo = new Repository.MongoRep(UserId, _settings);
            if (await ValorCredito(UserId, repMongo) == 0)
            {
                ViewBag.Message = "Você ainda não possui um plano ou ele pode estar vencido <br />Clique no botão  abaixo e veja os planos disponíveis <br />  para adquirir ou renovar";
                return null;
            }

            var lstMongoUser = await repMongo.Listar<Models.Graph.Usuario>(UserId);
            var mongoUser = lstMongoUser.Where(w => w._id == id).ToList();

            //return await LoadViewConsulta(mongoUser, repMongo, UserId, "ViewFree");
            return await LoadMetricas(mongoUser, repMongo, UserId, "Metricas");
        }

        private async Task<ActionResult> LoadViewConsulta(List<ContractClass<Models.Graph.Usuario>> mongoUser,
            Repository.MongoRep repMongo, string UserId, string _nameView)
        {
            var linhaerro = "";
            var inf = new Models.DTO.InfluencersResumoFree();

            try
            {
                #region Repositorios
                var objUser = mongoUser.FirstOrDefault().Obj;
                var dtCriacao = mongoUser.FirstOrDefault().DateCreation;

                var lstMongoMedias = await repMongo.Listar<Models.Graph.Media>(UserId);
                var mongoMedias = lstMongoMedias.Where(w => w.DateCreation == dtCriacao).Select(s => s.Obj.data).ToList();

                var lstMongoTags = await repMongo.Listar<Models.Graph.Tags>(UserId);
                var mongoTags = lstMongoTags.Where(w => w.DateCreation == dtCriacao && w.Obj != null).Select(s => s.Obj.data).ToList().FirstOrDefault();

                var lstCities = await repMongo.Listar<Models.Graph.InsightsGenderAge>(UserId);
                var mongoCities = lstCities.Where(w => w.DateCreation == dtCriacao).ToList();
                var lstObjCities = mongoCities.Select(s => new
                {
                    data = s.Obj.data.FirstOrDefault().values[0].value,
                    timeSpan = s.timeSpan
                }
                ).ToList();

                var insigths = await repMongo.Listar<Models.DTO.InsigthDTO>(UserId);
                var lstInsigthsAge = insigths.ToList();
                var lstObjAges = lstInsigthsAge.Select(s => new
                {
                    data = s.Obj.data.FirstOrDefault(),
                    timeSpan = s.timeSpan
                }
                ).ToList();
                var lstAgesFull = lstObjAges.Where(w => w.data.title.Contains("Gender"));
                var lstAge = lstAgesFull.Where(w => w.timeSpan == lstAgesFull.Max(m => m.timeSpan)).FirstOrDefault();

                var objMinhasMidias = mongoMedias.FirstOrDefault();
                var objMinhasMidiasSemanal = objMinhasMidias.Where(x => x.timestamp >= dtCriacao.AddDays(-7)).ToList();
                var objEngaj = objMinhasMidias.OrderByDescending(x => x.timestamp).Take(5).ToList();

                var lstStoryGraphs = await repMongo.ListarGraphUserId<InfluencersMetricsService.Model.StoryInsights>(UserId);
                var lstStoryIdsTmp = await repMongo.ListarGraphUserId<InfluencersMetricsService.Model.Stories>(UserId);
                lstStoryIdsTmp = lstStoryIdsTmp.Where(w => w.DateCreation >= dtCriacao && w.DateCreation <= dtCriacao.AddDays(7)).ToList();
                var _lstStoryIds = new List<string>();
                lstStoryIdsTmp.ForEach(f =>
                {
                    f.Obj.data.ForEach(fi =>
                    {
                        _lstStoryIds.Add(fi.id + "|" + fi.media_type);
                    });
                });
                var lstStoryIdTypes = _lstStoryIds.DistinctBy(d => d);
                _lstStoryIds = _lstStoryIds.DistinctBy(d => d).ToList();
                var objUserInsigths = await repMongo.ListarGraphUserId<InfluencersMetricsService.Model.UserInsights>(UserId);
                var faceDetection = await repMongo.ListarGraphUserId<List<Models.FaceDetection>>(UserId);
                var lstFaceDetection = faceDetection.Where(w => w.timeSpan == faceDetection.Max(x => x.timeSpan)).ToList();

                #endregion

                #region Sumario
                linhaerro = "Sumario";
                inf.Seguidores = Convert.ToInt32(objUser.followers_count);
                inf.Seguindo = Convert.ToInt32(objUser.follows_count);
                inf.SeguindoSeguidores = inf.Seguindo / (decimal)inf.Seguidores * 100;
                //inf.SeguidoresUnicos = inf.Seguidores - inf.Seguindo;
                inf.Posts = objMinhasMidias.Count();
                inf.Curtidas = objMinhasMidias.Sum(x => x.like_count);
                inf.ProfilePicture = objUser.profile_picture_url;
                inf.NomeCompleto = objUser.name;
                inf.UserName = objUser.username;
                inf.SocialContext = objUser.biography;

                var comentarios = 0;
                objMinhasMidias.ForEach(x =>
                {
                    comentarios += (Convert.ToInt32(x.comments_count));
                });
                inf.Comentarios = comentarios;

                inf.avgPostReach = Math.Round(Convert.ToDouble(inf.Curtidas + inf.Comentarios) / objMinhasMidias.Count);

                inf.MediaCurtidas = inf.Curtidas / inf.Posts;
                inf.MediaComentarios = inf.Comentarios / (decimal)inf.Posts;
                inf.ComentariosSeguidores = (inf.MediaComentarios / inf.Seguidores) * 100;
                inf.Engajamento = (
                    (inf.Curtidas + (decimal)inf.Comentarios) / inf.Posts) / inf.Seguidores * 100;
                inf.Alcance = inf.Curtidas + inf.Comentarios;
                inf.MediaAlcancePost = inf.Posts / (decimal)inf.Alcance * 100;
                inf.Aprovado = 2;

                if (objUserInsigths.Count > 0)
                {
                    var dataUserInsigths = objUserInsigths.Select(s => new
                    {
                        Data = s.Obj.data,
                        Tipo = s.Obj.data.Exists(e => e.name == "reach") ? "O" : "P"
                    }).ToList();

                    var _impressions = dataUserInsigths.Where(o => o.Tipo == "O").FirstOrDefault().Data.Where(w => w.name == "impressions").FirstOrDefault();
                    var _reach = dataUserInsigths.Where(o => o.Tipo == "O").FirstOrDefault().Data.Where(w => w.name == "reach").FirstOrDefault();
                    inf.Impressions = dataUserInsigths.Where(o => o.Tipo == "O").FirstOrDefault().Data.Where(w => w.name == "impressions").FirstOrDefault().values.FirstOrDefault().value;
                    inf.Reach = _reach.values.FirstOrDefault().value;

                    inf.PeriodImpressions = (_impressions.period == "week") ?
                        dtCriacao.AddDays(-7).Date.ToString("dd/MM/yyyy") + " - " + dtCriacao.Date.ToString("dd/MM/yyyy") :
                        dtCriacao.Date.ToString("dd/MM/yyyy");
                    inf.PeriodReach = (_reach.period == "week") ?
                        dtCriacao.AddDays(-7).Date.ToString("dd/MM/yyyy") + " - " + dtCriacao.Date.ToString("dd/MM/yyyy") :
                        dtCriacao.Date.ToString("dd/MM/yyyy");

                    if (dataUserInsigths.Where(o => o.Tipo == "P").Count() > 0)
                    {
                        var _profileviews = dataUserInsigths.Where(o => o.Tipo == "P").FirstOrDefault().Data.Where(w => w.name == "profile_views").FirstOrDefault();
                        inf.ProfileViews = _profileviews.values.FirstOrDefault().value;
                        inf.PeriodProfileViews = (_profileviews.period == "week") ?
                        dtCriacao.AddDays(-7).Date.ToString("dd/MM/yyyy") + " - " + dtCriacao.Date.ToString("dd/MM/yyyy") :
                        dtCriacao.Date.ToString("dd/MM/yyyy");
                    }
                    else
                    {
                        var _profileviews = dataUserInsigths.Where(o => o.Tipo == "O").FirstOrDefault().Data.Where(w => w.name == "profile_views").FirstOrDefault();
                        inf.ProfileViews = _profileviews.values.FirstOrDefault().value;
                        inf.PeriodProfileViews = (_profileviews.period == "week") ?
                        dtCriacao.AddDays(-7).Date.ToString("dd/MM/yyyy") + " - " + dtCriacao.Date.ToString("dd/MM/yyyy") :
                        dtCriacao.Date.ToString("dd/MM/yyyy");
                    }

                }
                #endregion

                #region Calculo de Engajamento 
                linhaerro = "Engajamento";
                var engComentarios = 0;
                engComentarios = Convert.ToInt32(inf.Comentarios);
                var engCurtidas = inf.Curtidas;

                var mediaEngaj = Convert.ToDouble(engComentarios + engCurtidas) / Convert.ToDouble(objMinhasMidias.Count());
                var mediaPercent = (mediaEngaj / inf.Seguidores) * 100;

                inf.percentAvg = Math.Round(mediaPercent, 2);
                if (mediaPercent < valor0)
                {
                    inf.Aprovado = 0;
                }
                else if (mediaPercent < valor1)
                {
                    inf.Aprovado = 1;
                }
                else
                {
                    inf.Aprovado = 2;
                }

                inf.Powerful = 0;
                if (lstStoryGraphs.Count() > 0)
                {
                    var _maxStory = 0;
                    var _minStory = 0;
                    List<InfluencersMetricsService.Model.ValueStoryInsights> _objStories = new List<InfluencersMetricsService.Model.ValueStoryInsights>();
                    lstStoryGraphs.ForEach(l =>
                    {
                        l.Obj.data.ForEach(s =>
                        {
                            s.values.ForEach(sl =>
                            {
                                _objStories.Add(sl);
                            });
                        });
                    });

                    _maxStory = _objStories.Max(m => m.value);
                    _minStory = _objStories.Min(m => m.value);
                    inf.Powerful = Convert.ToInt32((_maxStory + _minStory) / 2);
                }
                #endregion

                #region Minha midias
                linhaerro = "Minha midias";
                var lstMidiasT = objMinhasMidias.ToList();
                var lstMinhasMidias = lstMidiasT.Where(r => r.caption != null)
                    .Where(z => z.caption.Count() > 0 && z.comments_count > 0).ToList()
                    .Where(r => r.media_url != null).ToList();

                var lstMidias = lstMinhasMidias
                    .Select(x => new Models.DTO.InstaMentions()
                    {
                        UserName = "" + x.caption.ToString(),
                        Used = Math.Round(Convert.ToDouble
                    (
                        Convert.ToDouble(lstMinhasMidias.Count(c => c.caption == x.caption))
                    ), 0),
                        UsedPerc = Math.Round(Convert.ToDouble
                    (
                            (
                                Convert.ToDouble(lstMinhasMidias.Count(c => c.caption == x.caption))
                            /
                                Convert.ToDouble(lstMinhasMidias.Count())
                            )
                    ) * 100, 4),
                        Reach = lstMinhasMidias.Where(c => c.caption == x.caption)
                        .Sum(s => Convert.ToInt32(s.comments_count) + s.like_count),
                        Engagemer = Math.Round(
                                Convert.ToDouble
                                (
                                    (Convert.ToDouble(
                                            lstMinhasMidias.Where(c => c.caption == x.caption)
                                            .Sum(s => Convert.ToInt32(s.comments_count))
                                        ) +
                                        Convert.ToDouble(
                                            lstMinhasMidias.Where(c => c.caption == x.caption)
                                                .Sum(s => Convert.ToInt32(s.like_count))
                                            )
                                    )
                                    /
                                    Convert.ToDouble(inf.Seguidores)
                                ) * 100, 4),
                        DiffUsedEngag = 0,
                        Imagens = new List<string>()
                        {
                            x.media_url.ToString()
                        },
                        Impressions = x.Impressions,
                        Reachs = x.Reach,
                        Saveds = x.Saved,
                        Engagements = x.Engagement
                    }).ToList();
                inf.LstInstaMidias = lstMidias.Take(40) //.DistinctBy(d => d.us.UserName)
                    .ToList();
                #endregion

                #region HashTags
                linhaerro = "Hashtags";
                if (lstMinhasMidias.Count > 0)
                {
                    var objHash = lstMinhasMidias.Where(w => w.caption.Contains("#"));
                    var lstHash = objHash.Where(x => x.comments != null).ToList();
                    var newLstHash = lstHash.Select(s => new Models.InstaMediaHash
                    {
                        Hashs = SplitHash(s.caption.ToUpper()),
                        InstaMedia = new InstaMedia()
                        {
                            Images = new List<InstaImage>() {
                                new InstaImage()
                                {
                                    URI = s.media_url
                                }
                            },
                            CommentsCount = s.comments_count.ToString(),
                            LikesCount = s.like_count
                        },
                        Impressions = s.Impressions,
                        Reachs = s.Reach,
                        Saveds = s.Saved,
                        Engagement = s.Engagement
                    }
                    ).ToList();

                    List<string> hashs = new List<string>();
                    newLstHash.ForEach(s =>
                    {
                        s.Hashs.ForEach(f =>
                        {
                            hashs.Add(f);
                        });
                    }
                    );

                    var lstImagensEhashs = new List<DtoHash>();
                    foreach (var it in newLstHash)
                    {
                        foreach (var h in it.Hashs)
                        {
                            lstImagensEhashs.Add(
                            new DtoHash()
                            {
                                hash = h,
                                URIImagem = (it.InstaMedia.Images.Count > 0 ? it.InstaMedia.Images.FirstOrDefault().URI : "")
                            });
                        }
                    }

                    var lstHashsDist = hashs.DistinctBy(x => x).ToList();
                    var lstHashs = lstHashsDist.Select(x => new Models.DTO.InstaMentions()
                    {
                        UserName = x
                        ,
                        UsedPerc = Math.Round(Convert.ToDouble
                        (
                                (
                                    Convert.ToDouble(newLstHash.Where(u => u.Hashs.Contains(x)).Count())
                                /
                                    Convert.ToDouble(hashs.Count())
                                )
                        ) * 100, 4),
                        Used = Math.Round(Convert.ToDouble
                        (
                            Convert.ToDouble(newLstHash.Where(u => u.Hashs.Contains(x)).Count())

                        ), 0),
                        Reach = newLstHash.Where(c => c.Hashs.Contains(x))
                            .Sum(s => Convert.ToInt32(s.InstaMedia.CommentsCount) + s.InstaMedia.LikesCount),
                        Engagemer = Math.Round(
                                    Convert.ToDouble(
                                    (Convert.ToDouble(
                                            newLstHash.Where(c => c.Hashs.Contains(x))
                                            .Sum(s => Convert.ToInt32(s.InstaMedia.CommentsCount))
                                        ) +
                                        Convert.ToDouble(
                                            newLstHash.Where(c => c.Hashs.Contains(x))
                                                .Sum(s => Convert.ToInt32(s.InstaMedia.LikesCount))
                                            )
                                    )
                                    /
                                    Convert.ToDouble(
                                        newLstHash.Where(c => c.Hashs.Contains(x))
                                        .Sum(s => hashs.Count)
                                        )) * 100, 4),
                        DiffUsedEngag = 1,
                        Imagens =
                                        lstImagensEhashs
                                        .Where(c => c.hash == x).DistinctBy(d => d.URIImagem)
                                        .Select(s => s.URIImagem).ToList(),
                        Impressions = newLstHash.Where(c => c.Hashs.Contains(x)).Sum(s => s.Impressions),
                        Reachs = newLstHash.Where(c => c.Hashs.Contains(x)).Sum(s => s.Reachs),
                        Saveds = newLstHash.Where(c => c.Hashs.Contains(x)).Sum(s => s.Saveds),
                        Engagements = newLstHash.Where(c => c.Hashs.Contains(x)).Sum(s => s.Engagement),
                    }).ToList();
                    inf.LstInstaHashs = lstHashs.OrderByDescending(o => o.Reach).Take(40).ToList();
                }
                #endregion

                #region TagsMentionedBy
                linhaerro = "MentionedByTag";
                if (mongoTags != null && (mongoTags.Count > 0))
                {
                    var lstTag = mongoTags.Where(x => x.caption != null).ToList();

                    var lstTags = lstTag.Select(x => new Models.DTO.InstaMentions()
                    {
                        UserName = x.username,
                        UsedPerc = Math.Round(Convert.ToDouble
                        (
                                (
                                    Convert.ToDouble(lstTag.Where(u => u.username.Equals(x.username)).Count())
                                /
                                    Convert.ToDouble(lstTag.Count())
                                )
                        ) * 100, 4),
                        Used = 1/*Math.Round(Convert.ToDouble
                        (
                            Convert.ToDouble(lstTag.Where(u => u.username.Equals(x.username)).Count())

                        ), 0)*/,
                        Reach = x.comments_count + x.like_count,
                        Engagemer = Math.Round(
                                    Convert.ToDouble(
                                    (Convert.ToDouble(
                                            lstTag.Where(u => u.username.Equals(x.username))
                                            .Sum(s => Convert.ToInt32(s.comments_count))
                                        ) +
                                        Convert.ToDouble(
                                            lstTag.Where(u => u.username.Equals(x.username))
                                                .Sum(s => Convert.ToInt32(s.like_count))
                                            )
                                    )
                                    /
                                    Convert.ToDouble(
                                        lstTag.Where(u => u.username.Equals(x.username))
                                        .Sum(s => lstTag.Count)
                                        )) * 100, 4),
                        DiffUsedEngag = 1,
                        Imagens =
                                        new List<string>(){
                                            x.media_url
                                        }
                    }).ToList();
                    inf.LstInstaTags = lstTags.OrderByDescending(o => o.Reach).Take(40).ToList();
                }
                #endregion

                #region Ages e Gender
                linhaerro = "Gender and Ages";
                if (lstAge != null)
                {
                    var lstAgesGender = lstAge.data.values.FirstOrDefault().value
                        .Select(x => new
                        {
                            Gender = x.name.Split('.').GetValue(0),
                            Faixa = x.name.Split('.').GetValue(1),
                            Used = 0,
                            UsedPerc = 0,
                            Reach = Convert.ToInt32(x.valor),
                            Engagemer = 0,
                            DiffUsedEngag = 0
                        }).ToList();

                    var lstAgesMidias = lstAge.data.values.FirstOrDefault().value
                        .Select(x => new Models.DTO.InstaMentions()
                        {
                            UserName = "" + x.name,
                            Used = 0,
                            UsedPerc = 0,
                            Reach = Convert.ToInt32(x.valor),
                            Engagemer = 0,
                            DiffUsedEngag = 0,
                            Imagens = null
                        }).ToList();
                    inf.LstAge = lstAgesMidias.OrderByDescending(o => o.Reach).Take(40) //.DistinctBy(d => d.us.UserName)
                        .ToList();
                    var listaArrayM = "";
                    lstAgesMidias.ForEach(f =>
                    {
                        if (f.UserName.IndexOf("M") > -1)
                        {
                            listaArrayM += (f.Reach * -1).ToString() + ",";
                        }
                    });
                    var listaArrayF = "";
                    lstAgesMidias.ForEach(f =>
                    {
                        if (f.UserName.IndexOf("F") > -1)
                        {
                            listaArrayF += f.Reach.ToString() + ",";
                        }
                    });
                    ViewBag.listaArrayM = listaArrayM;
                    ViewBag.listaArrayF = listaArrayF;
                }
                #endregion

                #region Cities
                linhaerro = "Cities";
                if (lstObjCities != null)
                {
                    var lstCitiesResult = lstObjCities.FirstOrDefault().data
                        .Select(x => new
                        {
                            City = x.Key,
                            Number = x.Value,
                            Used = 0,
                            UsedPerc = 0,
                            Reach = 0,
                            Engagemer = 0,
                            DiffUsedEngag = 0
                        }).ToList();

                    var lstCitiesResults = lstObjCities.FirstOrDefault().data
                        .Select(x => new Models.DTO.InstaMentions()
                        {
                            UserName = "" + x.Key,
                            Used = 0,
                            UsedPerc = 0,
                            Reach = Convert.ToInt32(x.Value),
                            Engagemer = 0,
                            DiffUsedEngag = 0,
                            Imagens = null
                        }).ToList();
                    inf.LstCities = lstCitiesResults.OrderByDescending(o => o.Reach).Take(20) //.DistinctBy(d => d.us.UserName)
                        .ToList();
                    string lstCitiesArray = "[['City', 'Engagement'],";
                    inf.LstCities.ForEach(f =>
                    {
                        var city = f.UserName.Split(',')[0];
                        city = city == null ? f.UserName : city;
                        lstCitiesArray += "['" + city + "'," + f.Reach.ToString() + "],";
                    });
                    lstCitiesArray += "]";

                    /*
                    [
                        ['City',   'Population', 'Area'],
                        ['São Paulo',      2761477,    1285.31],
                        ['Curitiba',     1324110,    181.76],
                        ['Belo Horizonte',    959574,     117.27],
                        ['Campinas',     907563,     130.17],
                        ['Guarulhos',   655875,     158.9],
                        ['Cotia',     607906,     243.60],
                        ['Belem',   380181,     140.7],
                        ['Tocantins',  371282,     102.41],
                        ['Brasilia', 67370,      213.44],
                        ['Acre',     52192,      43.43],
                        ['Chapecó',  38262,      11]
                      ]
                      */
                    lstCitiesArray = lstCitiesArray.Replace("],]", "]]");
                    ViewBag.LstCitiesArray = lstCitiesArray;
                    //var latitudes = await Coordenates(lstCitiesResults.FirstOrDefault().UserName);
                }
                #endregion

                #region Top and Botton POST
                inf.LstTopAndBotton = inf.LstInstaMidias.Where(w => w.Engagemer == inf.LstInstaMidias.Max(m => m.Engagemer))
                    .Union(
                    inf.LstInstaMidias.Where(w => w.Engagemer == inf.LstInstaMidias.Min(m => m.Engagemer))).ToList();
                #endregion

                #region Stories
                linhaerro = "Stories";

                var lstGraphsStory = lstStoryGraphs.Select(s => new Models.DTO.Story()
                {
                    ImpressionsValue = s.Obj.data.Where(w => w.name == "impressions").FirstOrDefault().values.FirstOrDefault().value,
                    ReachValue = s.Obj.data.Where(w => w.name == "reach").FirstOrDefault().values.FirstOrDefault().value,
                    ExitsValue = s.Obj.data.Where(w => w.name == "exits").FirstOrDefault().values.FirstOrDefault().value,
                    RepliesValue = s.Obj.data.Where(w => w.name == "replies").FirstOrDefault().values.FirstOrDefault().value,
                    TapsForwardValue = s.Obj.data.Where(w => w.name == "taps_forward").FirstOrDefault().values.FirstOrDefault().value,
                    TapsBackValue = s.Obj.data.Where(w => w.name == "taps_back").FirstOrDefault().values.FirstOrDefault().value,
                    DateCreation = s.DateCreation,
                    Id = (s.Obj.data.FirstOrDefault().id.Split('/')[0]).ToString().ToUpper(),
                    TimeSpan = s.timeSpan
                    //(lstStoryGraphs.Where(lw => lw.Obj.data.FirstOrDefault().id.Split('/')[0] ==
                    //                    s.Obj.data.FirstOrDefault().id.Split('/')[0] &&
                    //                    s.DateCreation == lw.DateCreation).Max(mt => mt.timeSpan))
                });

                var lstTeste = lstGraphsStory.ToList();

                var lstGraphsStoryIds = lstGraphsStory
                    .GroupBy(g => new { g.Id })
                    .Select(s => new
                    {
                        Id = s.Key.ToString().ToUpper(),
                        TimeSpanMax = s.Max(ma => ma.TimeSpan)
                    });

                var lstStory = new List<Models.DTO.Story>();
                foreach (var it in lstGraphsStory.DistinctBy(d => d.Id))
                {
                    var _maxTime = lstGraphsStory.Where(w => w.Id.ToString().Equals(it.Id)).ToList();
                    var maxTime = _maxTime.Max(m => m.TimeSpan);
                    var newStory = lstGraphsStory.Where(w => w.Id == it.Id && w.TimeSpan == maxTime).FirstOrDefault();
                    lstStory.Add(new Story()
                    {
                        DateCreation = newStory.DateCreation,
                        Date = newStory.DateCreation.ToString("dd/MM/yyyy"),
                        Id = newStory.Id,
                        TimeSpan = newStory.TimeSpan,
                        ExitsValue = newStory.ExitsValue,
                        ImpressionsValue = newStory.ImpressionsValue,
                        ReachValue = newStory.ReachValue,
                        RepliesValue = newStory.RepliesValue,
                        TapsBackValue = newStory.TapsBackValue,
                        TapsForwardValue = newStory.TapsForwardValue,
                        Imagens =
                                new List<string>(){
                                    //"https://www.influencersmetrics.com/story/image/" +
                                    "https://s3.amazonaws.com/influencersmetrics/" +
                                        (_lstStoryIds.Exists(w=>w.Contains(newStory.Id) && w.Contains("IMAGE"))? newStory.Id:"VIDEO") + ".jpg"
                                }
                    });
                }

                var lstGraphStoriesI = lstStory.Select(s => new
                {
                    Name = "Impressions",
                    DateCreation = s.DateCreation,
                    Id = s.Id,
                    Avg = s.ImpressionsValue
                }).Take(10);

                var lstGraphStoriesR = lstStory.Select(s => new
                {
                    Name = "Reach",
                    DateCreation = s.DateCreation,
                    Id = s.Id,
                    Avg = s.ReachValue
                }).Take(10);

                var lstGraphStoriesE = lstStory.Select(s => new
                {
                    Name = "Exits",
                    DateCreation = s.DateCreation,
                    Id = s.Id,
                    Avg = s.ExitsValue
                }).Take(10);

                var lstGraphStoriesRp = lstStory.Select(s => new
                {
                    Name = "Replies",
                    DateCreation = s.DateCreation,
                    Id = s.Id,
                    Avg = s.RepliesValue
                }).Take(10);

                var lstGraphStoriesTf = lstStory.Select(s => new
                {
                    Name = "Taps forward",
                    DateCreation = s.DateCreation,
                    Id = s.Id,
                    Avg = s.TapsForwardValue
                }).Take(10);

                var lstGraphStoriesTb = lstStory.Select(s => new
                {
                    Name = "Taps back",
                    DateCreation = s.DateCreation,
                    Id = s.Id,
                    Avg = s.TapsBackValue
                }).Take(10);

                var lstGraphStories = lstGraphStoriesI.Union(lstGraphStoriesR).Union(lstGraphStoriesE)
                                      .Union(lstGraphStoriesRp).Union(lstGraphStoriesTf).Union(lstGraphStoriesTb);
                if (lstGraphStories.Count() > 0)
                {
                    var lstStories = lstGraphStories.Select(x => new Models.DTO.InstaMentions()
                    {
                        UserName = x.Name,
                        UsedPerc = 0,
                        Used = 1,
                        Reach = Convert.ToInt32((
                                lstGraphStories.Where(w => w.Id == x.Id && x.Name == w.Name) //dtCriacao.AddDays(1)
                                                .Max(z => z.Avg))
                                                ),
                        Engagemer = 0,
                        DiffUsedEngag = 1,
                        Imagens =
                                        new List<string>(){
                                            //"https://www.influencersmetrics.com/story/image/" +
                                            "https://s3.amazonaws.com/influencersmetrics/" +
                                                (_lstStoryIds.Exists(w=>w.Contains(x.Id) && w.Contains("IMAGE"))? x.Id:"VIDEO") + ".jpg"
                                        }
                    });

                    inf.LstInstaStories = lstStories.Take(60).ToList();
                    inf.LstInstaStory = lstStory.OrderByDescending(x => x.DateCreation).ToList();
                }
                #endregion

                #region Emotional
                linhaerro = "Emotional";
                if (lstFaceDetection.Count > 0)
                {
                    var lstSemNota = lstFaceDetection.FirstOrDefault().Obj
                        .Where(w => w.Joy == 0 && w.Anger == 0 && w.Surprise == 0 && w.Sorrow == 0)
                        .Select(s => s.UrlImagem).ToList();

                    var ls = from s in lstFaceDetection.FirstOrDefault().Obj
                             where !lstSemNota.Any(es => (es == s.UrlImagem))
                             select s;

                    var lstFaceDetections = ls
                        .Select(s => new Models.FaceDetection()
                        {
                            Anger = (s.Anger), //Raiva
                            Joy = (s.Joy), //Alegria
                            Sorrow = (s.Sorrow), //Tristeza
                            Surprise = (s.Surprise), //Surpresa
                            DtAvaliacao = s.DtAvaliacao,
                            UrlImagem = s.UrlImagem,
                            UserName = s.UserName
                        }).ToList();

                    var lstAvgFaceDetection = lstFaceDetections.Select(l => new Models.FaceDetection()
                    {
                        Anger = lstFaceDetections.Sum(s => (s.Anger)), //Raiva
                        Joy = lstFaceDetections.Sum(s => (s.Joy)), //Alegria
                        Sorrow = lstFaceDetections.Sum(s => s.Sorrow), //Tristeza
                        Surprise = lstFaceDetections.Sum(s => (s.Surprise)), //Surpresa
                        DtAvaliacao = lstFaceDetections.FirstOrDefault().DtAvaliacao,
                        UrlImagem = "",
                        UserName = ""
                    });
                    var avgFaceDetection = lstAvgFaceDetection.DistinctBy(d => d.UserName).FirstOrDefault();
                    if (avgFaceDetection == null)
                    {

                    }
                    else
                    {
                        inf.LstFaceDetection = lstFaceDetections.ToList();
                        inf.AvgFaceDetection = avgFaceDetection;

                        var listaFaceDetection = (avgFaceDetection.Joy / lstFaceDetection.Count).ToString() + "," +
                            (avgFaceDetection.Sorrow / lstFaceDetection.Count).ToString() + "," +
                            (avgFaceDetection.Anger / lstFaceDetection.Count).ToString() + "," +
                            (avgFaceDetection.Surprise / lstFaceDetection.Count).ToString() + "";
                        var cabecalhoFaceDetection = "'Alegre','Tristeza','Raiva','Surpresa'";

                        ViewBag.CabecalhoFaceDetection = cabecalhoFaceDetection;
                        ViewBag.ListaFaceDetection = listaFaceDetection;

                        inf.CabecalhoFaceDetection = cabecalhoFaceDetection;
                        inf.ListaFaceDetection = listaFaceDetection;

                        var lstEmotionals = ls.Select(s => new Models.FaceDetection()
                        {
                            Anger = (s.Anger), //Raiva
                            Joy = (s.Joy), //Alegria
                            Sorrow = (s.Sorrow), //Tristeza
                            Surprise = (s.Surprise), //Surpresa
                            DtAvaliacao = s.DtAvaliacao,
                            UrlImagem = s.UrlImagem,
                            UserName = s.UserName
                        }).ToList();

                        var newLstEmotional = lstEmotionals.Select(s => new
                        {
                            Joy = lstEmotionals.Where(w => w.UserName == s.UserName).Sum(sm => sm.Joy),
                            Sorrow = lstEmotionals.Where(w => w.UserName == s.UserName).Sum(sm => sm.Sorrow),
                            Anger = lstEmotionals.Where(w => w.UserName == s.UserName).Sum(sm => sm.Anger),
                            Surprise = lstEmotionals.Where(w => w.UserName == s.UserName).Sum(sm => sm.Surprise)
                        }
                        ).ToList().Distinct();

                        inf.LstInstaEmotionalResume = new List<InstaMentions>();
                        newLstEmotional.ForEach(f =>
                        {
                            inf.LstInstaEmotionalResume.Add(new InstaMentions()
                            {
                                UserName = "ALEGRIA",
                                Reach = f.Joy
                            });
                            inf.LstInstaEmotionalResume.Add(new InstaMentions()
                            {
                                UserName = "TRISTEZA",
                                Reach = f.Sorrow
                            });
                            inf.LstInstaEmotionalResume.Add(new InstaMentions()
                            {
                                UserName = "RAIVA",
                                Reach = f.Anger
                            });
                            inf.LstInstaEmotionalResume.Add(new InstaMentions()
                            {
                                UserName = "SURPRESA",
                                Reach = f.Surprise
                            });
                        });
                    }
                }
                #endregion


                await repMongo.GravarOne<Models.DTO.InfluencersResumoFree>(inf);

                return View(_nameView, inf);
            }
            catch (Exception)
            {
                ViewBag.Message = "Erro inesperado ao processar a visualização.<br />Por favor tent novamente em alguns minutos(" + linhaerro + ")";
                return View("ViewFree");
            }
        }

        private async Task<ActionResult> LoadViewConsultaBasica(Models.Graph.Discovery user, Repository.MongoRep repMongo, string UserId, string _nameView)
        {
            var linhaerro = "";
            var inf = new Models.DTO.InfluencersResumoFree();

            try
            {
                #region Sumario
                inf.Seguidores = Convert.ToInt32(user.business_discovery.followers_count);
                //inf.Seguindo = Convert.ToInt32(user.business_discovery.follows_count);
                //inf.SeguindoSeguidores = inf.Seguindo / (decimal)inf.Seguidores * 100;
                //inf.SeguidoresUnicos = inf.Seguidores - inf.Seguindo;
                inf.Posts = user.business_discovery.media_count;
                inf.Curtidas = user.business_discovery.media.data.Sum(x => x.like_count);
                inf.ProfilePicture = user.business_discovery.profile_picture_url;
                inf.NomeCompleto = user.business_discovery.name;
                inf.UserName = user.business_discovery.username;
                inf.Biography = user.business_discovery.biography;
                //inf.SocialContext = objUser.biography;

                var comentarios = 0;
                user.business_discovery.media.data.ForEach(x =>
                {
                    comentarios += (Convert.ToInt32(x.comments_count));
                });
                inf.Comentarios = comentarios;

                var curtidas = 0;
                user.business_discovery.media.data.ForEach(x =>
                {
                    curtidas += (Convert.ToInt32(x.like_count));
                });
                inf.Curtidas = curtidas;

                inf.avgPostReach = Math.Round(Convert.ToDouble(inf.Curtidas + inf.Comentarios) / user.business_discovery.media_count);

                inf.MediaCurtidas = inf.Curtidas / inf.Posts;
                inf.MediaComentarios = inf.Comentarios / (decimal)inf.Posts;
                inf.ComentariosSeguidores = (inf.MediaComentarios / inf.Seguidores) * 100;
                inf.Engajamento = (
                    (inf.Curtidas + (decimal)inf.Comentarios) / inf.Posts) / inf.Seguidores * 100;
                inf.Alcance = inf.Curtidas + inf.Comentarios;
                inf.MediaAlcancePost = inf.Posts / (decimal)inf.Alcance * 100;
                inf.Aprovado = 2;

                #endregion

                #region Calculo de Engajamento 
                linhaerro = "Engajamento";
                var engComentarios = 0;
                engComentarios = Convert.ToInt32(inf.Comentarios);
                var engCurtidas = inf.Curtidas;

                var mediaEngaj = Convert.ToDouble(engComentarios + engCurtidas) / Convert.ToDouble(user.business_discovery.media_count);
                var mediaPercent = (mediaEngaj / inf.Seguidores) * 100;

                inf.percentAvg = Math.Round(mediaPercent, 2);
                if (mediaPercent < valor0)
                {
                    inf.Aprovado = 0;
                }
                else if (mediaPercent < valor1)
                {
                    inf.Aprovado = 1;
                }
                else
                {
                    inf.Aprovado = 2;
                }
                #endregion

                #region Minha midias
                linhaerro = "Minha midias";
                var lstMinhasMidias = user.business_discovery.media.data.Where(r => r.caption != null)
                    .Where(z => z.caption.Count() > 0 && z.comments_count > 0).ToList()
                    .Where(r => r.media_url != null).ToList();

                var lstMidias = lstMinhasMidias
                    .Select(x => new Models.DTO.InstaMentions()
                    {
                        UserName = "" + x.caption.ToString(),
                        Used = Math.Round(Convert.ToDouble
                    (
                        Convert.ToDouble(lstMinhasMidias.Count(c => c.caption == x.caption))
                    ), 0),
                        UsedPerc = Math.Round(Convert.ToDouble
                    (
                            (
                                Convert.ToDouble(lstMinhasMidias.Count(c => c.caption == x.caption))
                            /
                                Convert.ToDouble(lstMinhasMidias.Count())
                            )
                    ) * 100, 4),
                        Reach = lstMinhasMidias.Where(c => c.caption == x.caption)
                        .Sum(s => Convert.ToInt32(s.comments_count) + s.like_count),
                        Engagemer = Math.Round(
                                Convert.ToDouble
                                (
                                    (Convert.ToDouble(
                                            lstMinhasMidias.Where(c => c.caption == x.caption)
                                            .Sum(s => Convert.ToInt32(s.comments_count))
                                        ) +
                                        Convert.ToDouble(
                                            lstMinhasMidias.Where(c => c.caption == x.caption)
                                                .Sum(s => Convert.ToInt32(s.like_count))
                                            )
                                    )
                                    /
                                    Convert.ToDouble(inf.Seguidores)
                                ) * 100, 4),
                        DiffUsedEngag = 0,
                        Imagens = new List<string>()
                        {
                            x.media_url.ToString()
                        },
                        Impressions = 0,
                        Reachs = 0,
                        Saveds = 0,
                        Engagements = 0
                    }).ToList();
                inf.LstInstaMidias = lstMidias.Take(40) //.DistinctBy(d => d.us.UserName)
                    .ToList();
                #endregion

                #region HashTags
                linhaerro = "Hashtags";
                if (lstMinhasMidias.Count > 0)
                {
                    var objHash = lstMinhasMidias.Where(w => w.caption.Contains("#"));
                    var lstHash = objHash.ToList();//Faz sentido isso?
                    var newLstHash = lstHash.Select(s => new Models.InstaMediaHash
                    {
                        Hashs = SplitHash(s.caption.ToUpper()),
                        InstaMedia = new InstaMedia()
                        {
                            Images = new List<InstaImage>() {
                                new InstaImage()
                                {
                                    URI = s.media_url
                                }
                            },
                            CommentsCount = s.comments_count.ToString(),
                            LikesCount = s.like_count
                        },
                        Impressions = 0,
                        Reachs = 0,
                        Saveds = 0,
                        Engagement = 0
                    }
                    ).ToList();

                    List<string> hashs = new List<string>();
                    newLstHash.ForEach(s =>
                    {
                        s.Hashs.ForEach(f =>
                        {
                            hashs.Add(f);
                        });
                    }
                    );

                    var lstImagensEhashs = new List<DtoHash>();
                    foreach (var it in newLstHash)
                    {
                        foreach (var h in it.Hashs)
                        {
                            lstImagensEhashs.Add(
                            new DtoHash()
                            {
                                hash = h,
                                URIImagem = (it.InstaMedia.Images.Count > 0 ? it.InstaMedia.Images.FirstOrDefault().URI : "")
                            });
                        }
                    }

                    var lstHashsDist = hashs.DistinctBy(x => x).ToList();
                    var lstHashs = lstHashsDist.Select(x => new Models.DTO.InstaMentions()
                    {
                        UserName = x
                        ,
                        UsedPerc = Math.Round(Convert.ToDouble
                        (
                                (
                                    Convert.ToDouble(newLstHash.Where(u => u.Hashs.Contains(x)).Count())
                                /
                                    Convert.ToDouble(hashs.Count())
                                )
                        ) * 100, 4),
                        Used = Math.Round(Convert.ToDouble
                        (
                            Convert.ToDouble(newLstHash.Where(u => u.Hashs.Contains(x)).Count())

                        ), 0),
                        Reach = newLstHash.Where(c => c.Hashs.Contains(x))
                            .Sum(s => Convert.ToInt32(s.InstaMedia.CommentsCount) + s.InstaMedia.LikesCount),
                        Engagemer = Math.Round(
                                    Convert.ToDouble(
                                    (Convert.ToDouble(
                                            newLstHash.Where(c => c.Hashs.Contains(x))
                                            .Sum(s => Convert.ToInt32(s.InstaMedia.CommentsCount))
                                        ) +
                                        Convert.ToDouble(
                                            newLstHash.Where(c => c.Hashs.Contains(x))
                                                .Sum(s => Convert.ToInt32(s.InstaMedia.LikesCount))
                                            )
                                    )
                                    /
                                    Convert.ToDouble(
                                        newLstHash.Where(c => c.Hashs.Contains(x))
                                        .Sum(s => hashs.Count)
                                        )) * 100, 4),
                        DiffUsedEngag = 1,
                        Imagens =
                                        lstImagensEhashs
                                        .Where(c => c.hash == x).DistinctBy(d => d.URIImagem)
                                        .Select(s => s.URIImagem).ToList(),
                        Impressions = newLstHash.Where(c => c.Hashs.Contains(x)).Sum(s => s.Impressions),
                        Reachs = newLstHash.Where(c => c.Hashs.Contains(x)).Sum(s => s.Reachs),
                        Saveds = newLstHash.Where(c => c.Hashs.Contains(x)).Sum(s => s.Saveds),
                        Engagements = newLstHash.Where(c => c.Hashs.Contains(x)).Sum(s => s.Engagement),
                    }).ToList();
                    inf.LstInstaHashs = lstHashs.OrderByDescending(o => o.Reach).Take(40).ToList();
                }
                #endregion

                #region Top and Botton POST
                inf.LstTopAndBotton = inf.LstInstaMidias.Where(w => w.Engagemer == inf.LstInstaMidias.Max(m => m.Engagemer))
                    .Union(
                    inf.LstInstaMidias.Where(w => w.Engagemer == inf.LstInstaMidias.Min(m => m.Engagemer))).ToList();
                #endregion

                #region Emotional
                var faceDetection = await repMongo.ListarGraphUserId<List<Models.FaceDetection>>(UserId);
                var lstFaceDetection = faceDetection.Where(w => w.timeSpan == faceDetection.Max(x => x.timeSpan)).ToList();

                linhaerro = "Emotional";
                if (lstFaceDetection.Count > 0)
                {
                    var lstSemNota = lstFaceDetection.FirstOrDefault().Obj
                        .Where(w => w.Joy == 0 && w.Anger == 0 && w.Surprise == 0 && w.Sorrow == 0)
                        .Select(s => s.UrlImagem).ToList();

                    var ls = from s in lstFaceDetection.FirstOrDefault().Obj
                             where !lstSemNota.Any(es => (es == s.UrlImagem))
                             select s;

                    var lstFaceDetections = ls
                        .Select(s => new Models.FaceDetection()
                        {
                            Anger = (s.Anger / 2), //Raiva
                            Joy = (s.Joy / 4), //Alegria
                            Sorrow = (s.Sorrow / 2), //Tristeza
                            Surprise = (s.Surprise / 4), //Surpresa
                            DtAvaliacao = s.DtAvaliacao,
                            UrlImagem = s.UrlImagem,
                            UserName = s.UserName
                        }).ToList();


                    var lstAvgFaceDetection = lstFaceDetections.Select(l => new Models.FaceDetection()
                    {
                        Anger = lstFaceDetections.Sum(s => (s.Anger)), //Raiva
                        Joy = lstFaceDetections.Sum(s => (s.Joy)), //Alegria
                        Sorrow = lstFaceDetections.Sum(s => s.Sorrow), //Tristeza
                        Surprise = lstFaceDetections.Sum(s => (s.Surprise)), //Surpresa
                        DtAvaliacao = lstFaceDetections.FirstOrDefault().DtAvaliacao,
                        UrlImagem = "",
                        UserName = ""
                    });
                    var avgFaceDetection = lstAvgFaceDetection.DistinctBy(d => d.UserName).FirstOrDefault();
                    if (avgFaceDetection == null)
                    {

                    }
                    else
                    {
                        inf.LstFaceDetection = lstFaceDetections.ToList();
                        inf.AvgFaceDetection = avgFaceDetection;

                        var listaFaceDetection = (avgFaceDetection.Joy * 100 / lstFaceDetection.Count).ToString() + "," +
                            (avgFaceDetection.Sorrow * 100 / lstFaceDetection.Count).ToString() + "," +
                            (avgFaceDetection.Anger * 100 / lstFaceDetection.Count).ToString() + "," +
                            (avgFaceDetection.Surprise * 100 / lstFaceDetection.Count).ToString() + "";
                        var cabecalhoFaceDetection = "'Alegre','Tristeza','Raiva','Surpresa'";

                        ViewBag.CabecalhoFaceDetection = cabecalhoFaceDetection;
                        ViewBag.ListaFaceDetection = listaFaceDetection;

                        inf.CabecalhoFaceDetection = cabecalhoFaceDetection;
                        inf.ListaFaceDetection = listaFaceDetection;

                        var lstEmotionals = ls.Select(s => new Models.FaceDetection()
                        {
                            Anger = (s.Anger), //Raiva
                            Joy = (s.Joy), //Alegria
                            Sorrow = (s.Sorrow), //Tristeza
                            Surprise = (s.Surprise), //Surpresa
                            DtAvaliacao = s.DtAvaliacao,
                            UrlImagem = s.UrlImagem,
                            UserName = s.UserName
                        }).ToList();

                        var newLstEmotional = lstEmotionals.Select(s => new
                        {
                            Joy = lstEmotionals.Where(w => w.UserName == s.UserName).Sum(sm => sm.Joy),
                            Sorrow = lstEmotionals.Where(w => w.UserName == s.UserName).Sum(sm => sm.Sorrow),
                            Anger = lstEmotionals.Where(w => w.UserName == s.UserName).Sum(sm => sm.Anger),
                            Surprise = lstEmotionals.Where(w => w.UserName == s.UserName).Sum(sm => sm.Surprise)
                        }
                        ).ToList().Distinct();

                        inf.LstInstaEmotionalResume = new List<InstaMentions>();
                        newLstEmotional.ForEach(f =>
                        {
                            inf.LstInstaEmotionalResume.Add(new InstaMentions()
                            {
                                UserName = "ALEGRIA",
                                Reach = f.Joy
                            });
                            inf.LstInstaEmotionalResume.Add(new InstaMentions()
                            {
                                UserName = "TRISTEZA",
                                Reach = f.Sorrow
                            });
                            inf.LstInstaEmotionalResume.Add(new InstaMentions()
                            {
                                UserName = "RAIVA",
                                Reach = f.Anger
                            });
                            inf.LstInstaEmotionalResume.Add(new InstaMentions()
                            {
                                UserName = "SURPRESA",
                                Reach = f.Surprise
                            });
                        });
                    }
                }
                #endregion


                ViewBag.Markers = "{}";

                await repMongo.GravarOne<Models.DTO.InfluencersResumoFree>(inf);

                return View(_nameView, inf);
            }
            catch (Exception)
            {
                ViewBag.Message = "Erro inesperado ao processar a visualização.<br />Por favor tent novamente em alguns minutos (" + linhaerro + ")";
                return View("ViewFree");
            }
        }

        public async Task<string> Coordenates(string location)
        {
            //Verifico se já existe

            //Se não existir busco

            //https://maps.googleapis.com/maps/api/geocode/json?address=S%C3%A3o+Paulo,+SP&key=AIzaSyD4jdxk2PGK1OVBXL2QNzQ4whOjFobwt0Y

            try
            {
                var Cidade = location.Split(',')[0];
                var Estado = location.Split(',')[1];

                var client = new HttpClient
                {
                    BaseAddress = new Uri("https://maps.googleapis.com/maps/api/geocode/")
                };
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var response = await client.GetAsync($"json?address={Cidade},+{Estado}&key=AIzaSyD4jdxk2PGK1OVBXL2QNzQ4whOjFobwt0Y");
                if (!response.IsSuccessStatusCode)
                    return "";

                var result = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<string>(result);

            }
            catch (Exception)
            {
                return "";
            }
        }

        public async Task<ActionResult> MeuLinkParaInsights()
        {
            var NomeAgencia = HttpContext.Session.GetString("nomeagencia");
            if (string.IsNullOrEmpty(NomeAgencia))
            {
                return RedirectToAction("MinhasAnalises");
            }

            var UserId = HttpContext.Session.GetString("UserId");
            var CodeUsuario = HttpContext.Session.GetString("UsuarioFull_id");
            var nomeAgencia = HttpContext.Session.GetString("nomeagencia");

            Repository.MongoRep repMongo = new Repository.MongoRep(UserId, _settings, UserId);
            var user = await repMongo.ListarById<Models.Usuario>(new ObjectId(UserId));
            var picture = await LoadPictureProfile(UserId, repMongo, user.FirstOrDefault().Obj.Tipo);
            HttpContext.Session.SetString("ProfilePicture", picture);
            ViewBag.ProfilePicture = picture;
            ViewBag.NameUser = NomeAgencia;
            HttpContext.Session.SetString("userNameTitle", NomeAgencia);

            try
            {
                if (string.IsNullOrEmpty(UserId))
                {
                    return RedirectToAction("Login");
                }

                ViewBag.MyCode = CodeUsuario;
                ViewBag.NomeAgencia = nomeAgencia;

                return View();
            }
            catch (Exception)
            {
                return RedirectToAction("HistoricoMetricas");
            }
        }

        public async Task<ActionResult> MeuLinkParaInsightsOld()
        {
            var NomeAgencia = HttpContext.Session.GetString("nomeagencia");
            if (string.IsNullOrEmpty(NomeAgencia))
            {
                return RedirectToAction("MinhasAnalises");
            }

            var UserId = HttpContext.Session.GetString("UserId");
            var CodeUsuario = HttpContext.Session.GetString("UsuarioFull_id");
            var nomeAgencia = HttpContext.Session.GetString("nomeagencia");

            Repository.MongoRep repMongo = new Repository.MongoRep(UserId, _settings, UserId);
            var user = await repMongo.ListarById<Models.Usuario>(new ObjectId(UserId));
            var picture = await LoadPictureProfile(UserId, repMongo, user.FirstOrDefault().Obj.Tipo);
            HttpContext.Session.SetString("ProfilePicture", picture);
            ViewBag.ProfilePicture = picture;
            ViewBag.NameUser = NomeAgencia;
            HttpContext.Session.SetString("userNameTitle", NomeAgencia);

            try
            {
                if (string.IsNullOrEmpty(UserId))
                {
                    return RedirectToAction("Login");
                }

                ViewBag.MyCode = CodeUsuario;
                ViewBag.NomeAgencia = nomeAgencia;

                return View();
            }
            catch (Exception)
            {
                return RedirectToAction("HistoricoMetricas");
            }
        }

        public async Task<string> ConsultaBasica(string _access_token, string _instagram_business_account, string discovery)
        {
            var result = GetDataGraphAsync<Models.Graph.Discovery>(_access_token,
                        _instagram_business_account + "?fields=business_discovery.username(" +
                        discovery + "){biography,followers_count,profile_picture_url,name,username,media_count,media{comments_count,like_count,caption,media_url,timestamp}}").Result;

            if (result != null)
            {
                var UserId = HttpContext.Session.GetString("UserId");
                var repMongo = new Repository.MongoRep(discovery, _settings, UserId);
                await repMongo.GravarOne<Models.Graph.Discovery>(result);
                return "";
            }

            return discovery;
        }
        public async Task<bool> LoadConsultAgency(string _access_token, string _instagram_business_account, string discovery)
        {
            var result = GetDataGraphAsync<Models.Graph.Discovery>(_access_token,
                        _instagram_business_account + "?fields=business_discovery.username(" +
                        discovery + "){biography,followers_count,profile_picture_url,name,username,media_count,media{comments_count,like_count,caption,media_url,timestamp}}").Result;

            if (result != null)
            {
                var UserId = HttpContext.Session.GetString("UserId");
                var repMongo = new Repository.MongoRep(discovery, _settings, UserId);
                await repMongo.GravarOne<Models.Graph.Discovery>(result);
                return true;
            }

            return false;
        }

        public static async Task<T> GetDataGraphAsync<T>(string accessToken, string uri)
        {
            using (var client = new HttpClient())
            {
                var conc = uri.Contains("?") ? "&" : "?";
                var _urlFull = $"https://graph.facebook.com/v3.2/{uri}{conc}access_token={accessToken}";
                try
                {
                    var response = await client.GetAsync(_urlFull);

                    var result = await response.Content.ReadAsStringAsync();
                    if (!response.IsSuccessStatusCode)
                    {
                        //var erros = JsonConvert.DeserializeObject<Model.ResponseGraph>(result);
                        return default(T);
                    }
                    return JsonConvert.DeserializeObject<T>(result);
                }
                catch (Exception)
                {
                    return default(T);
                }
            }
        }

        public async Task<IActionResult> UploadFile(IFormFile file, string _view)
        {
            if (file == null || file.Length == 0)
                return Content("file not selected");

            var newName = Guid.NewGuid().ToString();
            var path = Path.Combine(
                        Directory.GetCurrentDirectory(), @"wwwroot\img_agencias",
                        newName + Path.GetExtension(file.FileName)
                        );

            using (var stream = new FileStream(path, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var UserId = HttpContext.Session.GetString("UserId");
            Repository.MongoRep repMongo = new Repository.MongoRep(UserId, _settings, UserId);
            var img = new Models.DTO.ImageProfileAgency()
            {
                ProfilePictureName = newName + Path.GetExtension(file.FileName),
                UserId = UserId
            };

            await repMongo.GravarOne<Models.DTO.ImageProfileAgency>(img);

            return RedirectToAction(_view);
        }

        public async Task<ActionResult> NovaConsulta(string id, string origem)
        {
            if (origem == "b")
            {
                var UserId = HttpContext.Session.GetString("UserId");
                var access_token = HttpContext.Session.GetString("access_token_page");
                var _instagram_business_account = HttpContext.Session.GetString("instagram_business_account");

                //var retClass = JsonConvert.DeserializeObject<string[][]>(lista.UsuariosInstagram);//.Replace("[","").Replace("]",""));
                List<AutorizacaoMetrica> lstAutorizacao = new List<AutorizacaoMetrica>();
                var repMongoFull = new Repository.MongoRep(UserId, _settings, UserId);
                //var saldo = await ValorCredito(UserId, repMongoFull);

                //Buscar dados do usuario/relatorio atual que preciso reconsultar
                var reConsulta = await repMongoFull.ListarById<Models.Graph.Discovery>(new ObjectId(id));
                if (reConsulta == null && (reConsulta.Count() > 0 && reConsulta.FirstOrDefault().Obj.reprocessado != null))
                {
                    return RedirectToAction("historicometricas", new { msg = "3" });
                }
                var result = await repMongoFull.AlterarProcessamento(new ObjectId(id));
                //var id = 0; id < retClass.Count(); id++)
                {
                    var it = reConsulta.FirstOrDefault().Obj.business_discovery.username;
                    {
                        //if (saldo > 0)
                        {
                            var auth = new AutorizacaoMetrica()
                            {
                                Status = EnumStatus.SOLICITADO,
                                UsuarioInstagram = it,
                                DataCriacao = DateTime.Now,
                                Key = Guid.NewGuid().ToString(),
                                UsuarioId = "",
                                AgenciaUserId = UserId,
                                Client = ""
                            };

                            lstAutorizacao.Add(auth);

                            var repMongo = new Repository.MongoRep(it, _settings);
                            await repMongo.GravarOne<AutorizacaoMetrica>(auth);

                            var agency = await LoadConsultAgency(access_token, _instagram_business_account, it);
                            //if (!result) RedirectToAction("SolicitacaoMetricas");

                            //saldo--;
                            //await Debitar(UserId, 1, repMongoFull);
                        }
                    }
                }
            }
            return RedirectToAction("HistoricoMetricas");
        }

        [HttpPost]
        public async Task<string> desbloquearRelatorio(string id)
        {
            try
            {
                var repMongo = new Repository.MongoRep("MetricasInsights", _settings, id);
                var UserId = HttpContext.Session.GetString("UserId");

                var saldo = 0;
                var _credito = await GetCredito(UserId, repMongo);
                if (_credito != null)
                {
                    saldo = Convert.ToInt32(_credito.Qtd - _credito.Debito);
                }

                if (saldo > 0)
                {
                    var usuario = new Models.ContractClass<Models.Usuario>()
                    {
                        _id = new ObjectId(id),
                        Obj = new Usuario()
                        {
                            DataUsoCredito = DateTime.Now,
                            StatusCredito = EnumStatus.DISPONIVEL
                        }
                    };
                    await repMongo.AlterarUsuarioCredito(usuario);

                    await Debitar(UserId, 1, repMongo);

                    var usuarioGraph = await repMongo.Listar<Models.Graph.Usuario>(id);
                    var IdUsuarioGraph = usuarioGraph.FirstOrDefault()._id.ToString();

                    return IdUsuarioGraph;
                }

                return "Error#Saldo insuficiente.";
            }
            catch (Exception ex)
            {
                return "Error#Exception = " + ex.Message;
            }
        }

        [HttpPost]
        public async Task<string> removerconsulta(string excluir, string origem)
        {
            var id = excluir;
            var repMongo = new Repository.MongoRep("MetricasInsights", _settings, id);

            if (origem == "b")
            {
                var feito = await repMongo.ExcluirVinculoDiscovery(new ObjectId(id));
            }
            else
            {
                var usuarioGraph = await repMongo.ListarById<Models.Graph.Usuario>(new ObjectId(id));
                if (usuarioGraph == null)
                {
                    return "Error#" + "Usuário não encontrado.";
                }
                var feito = await repMongo.ExcluirVinculo(new ObjectId(usuarioGraph.FirstOrDefault().UsuarioId));
            }
            return "";
        }

        public async Task<ActionResult> Perfil(string msg = "")
        {
            if (string.IsNullOrWhiteSpace(msg))
            {
                var UserId1 = HttpContext.Session.GetString("UserId");
                if (UserId1 == null) return RedirectToAction("login");
                Repository.MongoRep repMongo1 = new Repository.MongoRep(UserId1, _settings, UserId1);
                var objeto1 = await repMongo1.ListarById<Models.Usuario>(new ObjectId(UserId1));
                if (objeto1 == null || (objeto1.Count == 0))
                {
                    return RedirectToAction("login");
                }
                var usuario1 = objeto1.FirstOrDefault().Obj;
                if (usuario1.Tipo == "2")
                {
                    var NomeAgencia = HttpContext.Session.GetString("nomeagencia");
                    ViewBag.NameUser = NomeAgencia;
                    HttpContext.Session.SetString("userNameTitle", NomeAgencia);
                }
                else
                {
                    ViewBag.NameUser = usuario1.name_page;
                    HttpContext.Session.SetString("userNameTitle", usuario1.name_page);
                }
                var picture1 = await LoadPictureProfile(UserId1, repMongo1, objeto1.FirstOrDefault().Obj.Tipo);
                HttpContext.Session.SetString("ProfilePicture", picture1);
                ViewBag.ProfilePicture = picture1;
                return View(usuario1);
            }

            var UserId = HttpContext.Session.GetString("UserId");
            if (UserId == null) return RedirectToAction("login");
            Repository.MongoRep repMongo = new Repository.MongoRep(UserId, _settings, UserId);
            var objeto = await repMongo.ListarById<Models.Usuario>(new ObjectId(UserId));
            if (objeto == null || (objeto.Count == 0))
            {
                return RedirectToAction("login");
            }
            var usuario = objeto.FirstOrDefault().Obj;
            if (usuario.Tipo == "2")
            {
                var NomeAgencia = HttpContext.Session.GetString("nomeagencia");
                ViewBag.NameUser = NomeAgencia;
                HttpContext.Session.SetString("userNameTitle", NomeAgencia);
            }
            else
            {
                ViewBag.NameUser = usuario.name_page;
                HttpContext.Session.SetString("userNameTitle", usuario.name_page);
            }
            var picture = await LoadPictureProfile(UserId, repMongo, objeto.FirstOrDefault().Obj.Tipo);
            HttpContext.Session.SetString("ProfilePicture", picture);
            ViewBag.ProfilePicture = picture;
            if (msg == "1")
            {
                ViewBag.ExecutarJS = "abrirModal('#msgModal','Sua senha foi alterada com sucesso.')";
            }
            if (msg == "2")
            {
                ViewBag.ExecutarJS = "abrirModal('#msgModal','Ocorreu algum erro<br />Por favor tente novamente')";
            }
            if (msg == "3")
            {
                ViewBag.ExecutarJS = "abrirModal('#msgModal','Você está desconectado, faça login novamente.')";
                return RedirectToAction("login");
            }
            if (msg == "4")
            {
                ViewBag.ExecutarJS = "abrirModal('#msgModal','Sua assinatura foi cancelada <br /> Para refazer sua assinatura adquira um novo plano <br />ou, insira dados de um cartão de crédito válido')";
            }
            if (msg == "5")
            {
                ViewBag.ExecutarJS = "abrirModal('#msgModal','Sua assinatura não foi encontrada <br /> Por favor entre em configurações e veja se adquiriu um plano <br /> Caso não tenha um plano ainda, escolha o seu e seja bem vindo')";
            }
            ViewBag.Tipo = usuario.Tipo;
            return View(usuario);
        }
        public async Task<ActionResult> Configuracoes(string msg = "")
        {
            var UserId = HttpContext.Session.GetString("UserId");
            if (UserId == null) return RedirectToAction("login");

            Repository.MongoRep repMongo = new Repository.MongoRep(UserId, _settings, UserId);
            var objeto = await repMongo.ListarById<Models.Usuario>(new ObjectId(UserId));
            if (objeto == null || (objeto.Count == 0))
            {
                return RedirectToAction("login");
            }

            var usuario = objeto.FirstOrDefault().Obj;

            if (usuario.Tipo == "2")
            {
                var NomeAgencia = HttpContext.Session.GetString("nomeagencia");
                ViewBag.NameUser = NomeAgencia;
                HttpContext.Session.SetString("userNameTitle", NomeAgencia);
            }
            else
            {
                ViewBag.NameUser = usuario.name_page;
                HttpContext.Session.SetString("userNameTitle", usuario.name_page);
            }
            var picture = await LoadPictureProfile(UserId, repMongo, objeto.FirstOrDefault().Obj.Tipo);
            HttpContext.Session.SetString("ProfilePicture", picture);
            ViewBag.ProfilePicture = picture;

            if (msg == "1")
            {
                ViewBag.ExecutarJS = "abrirModal('#msgModal','Sua senha foi alterada com sucesso.')";

            }

            if (msg == "2")
            {
                ViewBag.ExecutarJS = "abrirModal('#msgModal','Ocorreu algum erro<br />Por favor tente novamente')";
            }

            if (msg == "3")
            {
                ViewBag.ExecutarJS = "abrirModal('#msgModal','Você está desconectado, faça login novamente.')";
                return RedirectToAction("login");
            }

            if (msg == "4")
            {
                ViewBag.ExecutarJS = "abrirModal('#msgModal','Sua assinatura foi cancelada <br /> Para refazer sua assinatura adquira um novo plano <br />ou, insira dados de um cartão de crédito válido')";
            }

            if (msg == "5")
            {
                ViewBag.ExecutarJS = "abrirModal('#msgModal','Sua assinatura não foi encontrada <br /> Por favor entre em configurações e veja se adquiriu um plano <br /> Caso não tenha um plano ainda, escolha o seu e seja bem vindo')";
            }

            ViewBag.Tipo = usuario.Tipo;

            return View(usuario);
        }

        public async Task<ActionResult> BuscaInfluenciadores(SearchInfluencersRequest request = null)
        {
            var UserId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(UserId))
            {
                return RedirectToAction("Login");
            }
            if (request?.WordSearch == null || request?.WordSearch?.Length != null && request?.WordSearch?.Length < 3)
            {
                return this.View(new SearchInfluencersRequest());
            }
            if (request?.WordSearch == null || request?.WordSearch?.Length != null && request?.WordSearch?.Length < 3)
            {
                return this.View(new SearchInfluencersRequest());
            }
            request = request == null ? new SearchInfluencersRequest() : request;


            var startTime = DateTime.Now;
            var repMongo = new Repository.MongoRep(UserId, _settings, UserId);
            var result = await repMongo.SearchInfluencer(
                request.WordSearch,
                request.Insta,
                request.Youtube,
                request.Twitter,
                request.Tiktopk,
                request.Linkedin,
                request.PodCasts,
                request.CurrentPage,
                request.DisplayLenght,
                request.MaxAge,
                request.MinAge,
                request.GenderMale,
                request.GenderFemale,
                request.MinFollowers,
                request.MaxFollowers);
            var endTime = DateTime.Now;
            var elapsed = endTime - startTime;
            //result.ResultsInTime = "Aproximadamente " + result.CountAllResults + " resultados (" + elapsed.TotalSeconds.ToString("N2") + " segundos)";
            result.ResultsInTime = result.CountAllResults.ToString("N0") + " influenciadores encontrados (" + elapsed.TotalSeconds.ToString("N2") + " segundos)";
            result.PaginationButtonsNameValue = GetPaginationButtons(request.DisplayLenght, result.CountAllResults, request.CurrentPage);
            result.FilterByInstagram = request.Insta;
            result.FilterByYoutube = request.Youtube;
            result.FilterByTwitter = request.Twitter;
            result.FilterByTiktopk = request.Tiktopk;
            result.FilterByLinkedin = request.Linkedin;
            result.FilterByPodCasts = request.PodCasts;

            this.ViewBag.searchResult = result;
            return this.View(request);
        }

        public async Task<ActionResult> InfluenciadorDetalhe(string id)
        {
            var UserId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(UserId))
            {
                return RedirectToAction("Login");
            }
            var repMongo = new Repository.MongoRep(UserId, _settings, UserId);
            var info = await repMongo.GetInfluencerDetail(id);
            return View(info);
        }

        public ActionResult SugestaoRecurso()
        {
            return View();
        }

        private static List<int> GetPaginationButtons(int displayLenght, long countAllResults, int currentPage)
        {
            var result = new List<int>();
            if (countAllResults <= displayLenght)
            {
                result.Add(1);
            }
            else
            {
                var maxPage = (int)Math.Ceiling((double)countAllResults / displayLenght);
                var pageButtons = 3;
                var start = currentPage > 1 ? currentPage - 1 : currentPage;
                var endPage = start + (pageButtons - 1) < maxPage ? start + (pageButtons - 1) : maxPage;
                for (var page = start; page <= endPage; page++)
                {
                    result.Add(page);
                }
                if (result.Count < pageButtons && result.Count < maxPage)
                {
                    result.Insert(0, start - 1);
                }
            }
            return result;
        }

        private static long ConvertToTimestamp(DateTime value)
        {
            long epoch = (value.Ticks - 621355968000000000) / 10000000;
            return epoch;
        }

        private async Task<ActionResult> LoadMetricas(List<ContractClass<Models.Graph.Usuario>> mongoUser,
            Repository.MongoRep repMongo, string UserId, string _nameView)
        {
            var linhaerro = "";
            var inf = new Models.DTO.InfluencersResumoFree();
            var metricas = new Models.Metricas();
            try
            {
                #region Repositorios
                linhaerro = "Repositorios";
                var objUser = mongoUser.FirstOrDefault().Obj;
                var dtCriacao = mongoUser.FirstOrDefault().DateCreation;

                var lstMongoMedias = await repMongo.Listar<Models.Graph.Media>(UserId);
                List<Models.Graph.Datum> mongoMediasUnico = new List<Models.Graph.Datum>();
                var mongoMedias = lstMongoMedias.Where(w => w.DateCreation == dtCriacao && w.Obj != null && (w.Obj.data != null))
                    .Select(s => s.Obj.data).ToList();
                if (mongoMedias.Count() > 0)
                {
                    mongoMediasUnico = mongoMedias.FirstOrDefault()
                        .Where(w => w.timestamp >= dtCriacao.AddDays(-8)).ToList();
                }

                var lstMongoMentions = new List<webMetrics.Models.Graph.Datum>();
                foreach (var itMentions in mongoMediasUnico)
                {
                    if (itMentions.caption != null)
                    {
                        if (itMentions.caption.IndexOf(@"@") > -1)
                        {
                            lstMongoMentions.Add(itMentions);
                        }
                    }
                }

                var lstMongoTags = await repMongo.Listar<Models.Graph.Tags>(UserId);
                List<Models.Graph.Datum> mongoTagsUnico = new List<Models.Graph.Datum>();
                var mongoTags = lstMongoTags.Where(w => w.DateCreation == dtCriacao && w.Obj != null && (w.Obj.data != null))
                    .Select(s => s.Obj.data).ToList();
                if (mongoTags.Count() > 0)
                {
                    mongoTagsUnico = mongoTags.FirstOrDefault()
                        .Where(w => w.timestamp >= dtCriacao.AddDays(-8)).ToList();
                }

                var lstCities = await repMongo.Listar<Models.Graph.InsightsGenderAge>(UserId);
                var mongoCities = lstCities.Where(w => w.DateCreation == dtCriacao).ToList();
                var lstObjCities = mongoCities.Select(s => new
                {
                    data = s.Obj.data.FirstOrDefault().values[0].value,
                    timeSpan = s.timeSpan
                }
                ).ToList();

                var insigths = await repMongo.Listar<Models.DTO.InsigthDTO>(UserId);
                var lstInsigthsAge = insigths.ToList();
                var lstObjAges = lstInsigthsAge.Select(s => new
                {
                    data = s.Obj.data.FirstOrDefault(),
                    timeSpan = s.timeSpan
                }
                ).ToList();
                var lstAgesFull = lstObjAges.Where(w => w.data.title.Contains("Gender") || w.data.title.Contains("Gênero"));
                var lstAge = lstAgesFull.Where(w => w.timeSpan == lstAgesFull.Max(m => m.timeSpan)).FirstOrDefault();

                var objMinhasMidias = mongoMediasUnico;//.FirstOrDefault();
                var objMinhasMidiasSemanal = objMinhasMidias.Where(x => x.timestamp >= dtCriacao.AddDays(-7)).ToList();
                var objEngaj = objMinhasMidias.OrderByDescending(x => x.timestamp).Take(5).ToList();

                var lstStoryGraphs = await repMongo.ListarGraphUserId<InfluencersMetricsService.Model.StoryInsights>(UserId, dtCriacao);
                var lstStoryIdsTmp = await repMongo.ListarGraphUserId<InfluencersMetricsService.Model.Stories>(UserId, dtCriacao);
                lstStoryIdsTmp = lstStoryIdsTmp.Where(w => w.DateCreation >= dtCriacao && w.DateCreation <= dtCriacao.AddDays(7)).ToList();
                var _lstStoryIds = new List<string>();
                lstStoryIdsTmp.ForEach(f =>
                {
                    f.Obj.data.ForEach(fi =>
                    {
                        _lstStoryIds.Add(fi.id + "|" + fi.media_type);
                    });
                });
                var lstStoryIdTypes = _lstStoryIds.DistinctBy(d => d);
                _lstStoryIds = _lstStoryIds.DistinctBy(d => d).ToList();
                var objUserInsigths = await repMongo.ListarGraphUserId<InfluencersMetricsService.Model.UserInsights>(UserId);

                var objBestHourInsights = await repMongo.ListarGraphUserId<List<InfluencersMetricsService.Model.StoryUserBestHour>>(UserId);

                var faceDetection = await repMongo.ListarGraphUserId<List<Models.FaceDetection>>(UserId);
                var lstFaceDetection = faceDetection.Where(w => w.timeSpan == faceDetection.Max(x => x.timeSpan)).ToList();

                var newUserInsightWeekend = new Models.UserInsightWeekend();
                try
                {
                    //Verificar se já existe
                    var exist = (await repMongo.Listar<Models.UserInsightWeekend>(UserId));
                    if (exist == null || (exist.FirstOrDefault() == null || (exist.FirstOrDefault().Obj == null)))
                    {
                        var seguidores = Convert.ToInt32(objUser.followers_count);
                        var usuario = await repMongo.ListarById<Models.Usuario>(new ObjectId(UserId));
                        var access_token = usuario.FirstOrDefault().Obj.access_token_page;
                        var _instagram_business_account = usuario.FirstOrDefault().Obj.UsuarioInstagram;

                        var dtInicial = ConvertToTimestamp(dtCriacao.AddDays(-14));
                        var dtFinal = ConvertToTimestamp(dtCriacao.AddDays(-8));
                        var resultGrowth = GetDataGraphAsync<InfluencersMetricsService.Model.UserInsights>
                            (access_token, _instagram_business_account + "/insights?until=" + dtFinal.ToString() +
                            "&since=" + dtInicial.ToString() + "&period=day&metric=follower_count").Result;
                        var initial = 0;
                        resultGrowth.data[0].values.ForEach(f =>
                        {
                            initial += f.value;
                        });

                        dtInicial = ConvertToTimestamp(dtCriacao.AddDays(-7));
                        dtFinal = ConvertToTimestamp(dtCriacao);
                        resultGrowth = GetDataGraphAsync<InfluencersMetricsService.Model.UserInsights>
                            (access_token, _instagram_business_account + "/insights?until=" + dtFinal.ToString() +
                            "&since=" + dtInicial.ToString() + "&period=day&metric=follower_count").Result;
                        var final = 0;
                        resultGrowth.data[0].values.ForEach(f =>
                        {
                            final += f.value;
                        });

                        var newInsight = new Models.UserInsightWeekend()
                        {
                            Initial = initial,
                            Final = final,
                            PercentFinal = Math.Round((Convert.ToDecimal(final) / Convert.ToDecimal(seguidores) * 100), 2),
                            PercentInitial = Math.Round((Convert.ToDecimal(initial) / Convert.ToDecimal(seguidores) * 100), 2)
                        };
                        await repMongo.GravarOne<Models.UserInsightWeekend>(newInsight);
                        newUserInsightWeekend = newInsight;
                    }
                    else
                    {
                        newUserInsightWeekend = exist.FirstOrDefault().Obj;
                    }
                    ViewBag.GrowthWeekPeriod =
                    dtCriacao.AddDays(-14).ToString("dd/MM") + "-" + dtCriacao.AddDays(-8).ToString("dd/MM") + " | " +
                    dtCriacao.AddDays(-7).ToString("dd/MM") + "-" + dtCriacao.ToString("dd/MM");
                    ViewBag.GrowthWeek = newUserInsightWeekend.Initial.ToString() + " - " + newUserInsightWeekend.Final.ToString();
                    ViewBag.GrowthWeekPercent = newUserInsightWeekend.PercentInitial.ToString() + "% - " + newUserInsightWeekend.PercentFinal.ToString() + "%";
                }
                catch (Exception ex)
                {
                    newUserInsightWeekend = new Models.UserInsightWeekend();
                }

                var lstTipoAudiencia = await repMongo.ListarGraphUserId<List<InfluencerFollower>>(UserId);
                if (lstTipoAudiencia ==null || (lstTipoAudiencia.Count==0))
                {
                    var ta = new TipoDeAudiencia(_settings);
                    var objLstTipoAudiencia = await ta.FindTipoAudiencia(mongoUser.FirstOrDefault().UsuarioId, UserId);

                    await repMongo.GravarOne(objLstTipoAudiencia);
                    lstTipoAudiencia = await repMongo.ListarGraphUserId<List<InfluencerFollower>>(UserId);
                }
                ViewBag.PercSusp = Convert.ToInt32(Math.Round((Convert.ToDouble(Convert.ToDouble(lstTipoAudiencia.FirstOrDefault().Obj.Count(c=>c.Suspect))
                    /Convert.ToDouble(lstTipoAudiencia.FirstOrDefault().Obj.Count()))* 100),0));
                ViewBag.PercMass = Convert.ToInt32(Math.Round((Convert.ToDouble(Convert.ToDouble(lstTipoAudiencia.FirstOrDefault().Obj.Count(c => c.MassFollower))
                    / Convert.ToDouble(lstTipoAudiencia.FirstOrDefault().Obj.Count())) * 100), 0));
                ViewBag.PercInflu = Convert.ToInt32(Math.Round((Convert.ToDouble(Convert.ToDouble(lstTipoAudiencia.FirstOrDefault().Obj.Count(c => c.Influencer))
                    / Convert.ToDouble(lstTipoAudiencia.FirstOrDefault().Obj.Count())) * 100), 0));
                ViewBag.PercReais = Convert.ToInt32(Math.Round((Convert.ToDouble(Convert.ToDouble(lstTipoAudiencia.FirstOrDefault().Obj.Count(c => c.RealPerson))
                    / Convert.ToDouble(lstTipoAudiencia.FirstOrDefault().Obj.Count())) * 100), 0));
                if ((ViewBag.PercSusp + ViewBag.PercMass + ViewBag.PercInflu + ViewBag.PercReais)!=100)
                {
                    var dif = ((ViewBag.PercSusp + ViewBag.PercMass + ViewBag.PercInflu + ViewBag.PercReais) - 100);
                    ViewBag.PercSusp = ViewBag.PercSusp - (dif);
                }
                #endregion

                #region Sumario
                linhaerro = "Novo Sumario";
                var mediaStoryVideo = 0;
                var mediaStoryImage = 0;
                var percentmediaStoryVideo = 0d;
                var percentmediaStoryImage = 0d;

                var infRepliesTotal = 0;
                var infComentariosTotal = 0;
                var comentarios = 0;
                objMinhasMidias.ForEach(x =>
                {
                    comentarios += (Convert.ToInt32(x.comments_count));
                });
                inf.Comentarios = comentarios;

                metricas.Cabecalho = new Cabecalho
                {
                    Audienciacrescimento = 0,
                    Mediaalcance = 0,
                    Mediaengajamento = 0,
                    Mediaimpressoes = 0,
                    Reach = Math.Round(Convert.ToDouble(inf.Curtidas + inf.Comentarios) / objMinhasMidias.Count),
                    Seguidores = Convert.ToInt32(objUser.followers_count),
                    Seguindo = Convert.ToInt32(objUser.follows_count),
                    Viewperfil = 0
                };

                metricas.Sumario = new Sumario
                {
                    Avgpostsdiario = 0,
                    Avgpostsmensal = 0,
                    Avgpostssemanal = 0,
                    Comentariosseguidores = 0,
                    Fotospostfeed = 0,
                    Fotosstories = 0,
                    Mediacomentarios = 0,
                    Medialikes = 0,
                    Mediastories = 0,
                    Posts = objMinhasMidias.Count(),
                    Seguindoseguidores = metricas.Cabecalho.Seguindo / metricas.Cabecalho.Seguidores * 100,
                    Totalcomentarios = comentarios,
                    Totallikes = objMinhasMidias.Sum(x => x.like_count),
                    Videospostfeed = 0,
                    Videosstories = 0
                };

                linhaerro = "Sumario";
                inf.Seguidores = Convert.ToInt32(objUser.followers_count);
                inf.Seguindo = Convert.ToInt32(objUser.follows_count);
                inf.SeguindoSeguidores = inf.Seguidores > 0 ? inf.Seguindo / (decimal)inf.Seguidores * 100 : 0;
                inf.Posts = objMinhasMidias.Count();
                inf.Curtidas = objMinhasMidias.Sum(x => x.like_count);
                inf.ProfilePicture = objUser.profile_picture_url;
                inf.NomeCompleto = objUser.name;
                inf.UserName = objUser.username;
                inf.SocialContext = objUser.biography;


                inf.avgPostReach = objMinhasMidias.Count == 0 ? 0 : Math.Round(Convert.ToDouble(inf.Curtidas + inf.Comentarios) / objMinhasMidias.Count);

                inf.MediaCurtidas = inf.Posts == 0 ? 0 : inf.Curtidas / inf.Posts;
                inf.MediaComentarios = inf.Posts == 0 ? 0 : inf.Comentarios / (decimal)inf.Posts;
                inf.ComentariosSeguidores = inf.Seguidores > 0 ? (inf.MediaComentarios / inf.Seguidores) * 100 : 0;
                inf.Engajamento = inf.Posts == 0 ? 0 : (
                    (inf.Curtidas + (decimal)inf.Comentarios) / inf.Posts) / inf.Seguidores * 100;
                inf.Alcance = inf.Curtidas + inf.Comentarios;
                inf.MediaAlcancePost = inf.Posts == 0 ? 0 : inf.Posts / (decimal)inf.Alcance * 100;
                inf.Aprovado = 2;

                if (objUserInsigths.Count > 0)
                {
                    objUserInsigths = objUserInsigths.Where(w => w.Obj != null).ToList();
                    var dataUserInsigths = objUserInsigths.Select(s => new
                    {
                        Data = s.Obj.data,
                        Tipo = s.Obj.data.Exists(e => e.name == "reach") ? "O" : (s.Obj.data.Exists(e => e.name != "follower_count") ? "P" : "F"),
                        DateCreation = s.DateCreation
                    }).ToList();

                    var _impressions = dataUserInsigths.Where(o => o.Tipo == "O").FirstOrDefault().Data.Where(w => w.name == "impressions").FirstOrDefault();
                    var _reach = dataUserInsigths.Where(o => o.Tipo == "O").FirstOrDefault().Data.Where(w => w.name == "reach").FirstOrDefault();
                    inf.Impressions = dataUserInsigths.Where(o => o.Tipo == "O").FirstOrDefault().Data.Where(w => w.name == "impressions").FirstOrDefault().values.FirstOrDefault().value;
                    inf.Reach = _reach.values.FirstOrDefault().value;

                    inf.PeriodImpressions = (_impressions.period == "week") ?
                        dtCriacao.AddDays(-7).Date.ToString("dd/MM/yyyy") + " - " + dtCriacao.Date.ToString("dd/MM/yyyy") :
                        dtCriacao.Date.ToString("dd/MM/yyyy");
                    inf.PeriodReach = (_reach.period == "week") ?
                        dtCriacao.AddDays(-7).Date.ToString("dd/MM/yyyy") + " - " + dtCriacao.Date.ToString("dd/MM/yyyy") :
                        dtCriacao.Date.ToString("dd/MM/yyyy");

                    ViewBag.GrowthData = "";
                    if (dataUserInsigths.Where(o => o.Tipo == "P").Count() > 0)
                    {
                        var _profileviews = dataUserInsigths.Where(o => o.Tipo == "P").FirstOrDefault().Data.Where(w => w.name == "profile_views").FirstOrDefault();
                        inf.ProfileViews = _profileviews.values.FirstOrDefault().value;
                        inf.PeriodProfileViews = (_profileviews.period == "week") ?
                        dtCriacao.AddDays(-7).Date.ToString("dd/MM/yyyy") + " - " + dtCriacao.Date.ToString("dd/MM/yyyy") :
                        dtCriacao.Date.ToString("dd/MM/yyyy");
                    }
                    else
                    {
                        var _profileviews = dataUserInsigths.Where(o => o.Tipo == "O").FirstOrDefault().Data.Where(w => w.name == "profile_views").FirstOrDefault();
                        inf.ProfileViews = _profileviews.values.FirstOrDefault().value;
                        inf.PeriodProfileViews = (_profileviews.period == "week") ?
                        dtCriacao.AddDays(-7).Date.ToString("dd/MM/yyyy") + " - " + dtCriacao.Date.ToString("dd/MM/yyyy") :
                        dtCriacao.Date.ToString("dd/MM/yyyy");
                    }

                    ViewBag.GrowthData = "[]";
                    ViewBag.GrowthCategoria = "[]";

                    if (dataUserInsigths.Where(o => o.Tipo == "F").Count() > 0)
                    {
                        var lstGrowthCategorias = "";
                        var lstGrowthValores = "";
                        var contador = 0;
                        foreach (var it in dataUserInsigths.Where(o => o.Tipo == "F" && o.DateCreation >= dtCriacao)
                            .OrderByDescending(d => d.DateCreation).ToList())
                        {
                            foreach (var dat in it.Data)
                            {
                                foreach (var _values in dat.values)
                                {
                                    lstGrowthCategorias += "'" + it.DateCreation.AddDays(-(contador++)).ToString("dd/MM/yy") + "',";
                                    lstGrowthValores += _values.value + ",";
                                }
                                break;
                            }
                            break;
                        }
                        ViewBag.GrowthData = lstGrowthValores;
                        ViewBag.GrowthCategoria = lstGrowthCategorias + "";
                    }
                }
                #endregion

                #region BestHour
                ViewBag.BestHourCoordenates = "[]";
                if (objBestHourInsights.Count > 0)
                {
                    var lstBestHourChart = new List<InfluencersMetricsService.Model.StoryUserBestHour>();

                    var lstBestHour = objBestHourInsights.FirstOrDefault().Obj;
                    foreach (var dia in lstBestHour.Select(s => s.DiaDaSemana).Distinct())
                    {
                        foreach (var hour in lstBestHour.Where(w => w.DiaDaSemana == dia).Select(s => s.Hour).Distinct().OrderBy(x => x))
                        {
                            var valorReach = 0;
                            var qtd = 0;
                            lstBestHour.Where(w => w.DiaDaSemana == dia && w.Hour == hour).ForEach(f =>
                            {
                                qtd++;
                                valorReach += f.ValorReach;
                            });

                            lstBestHourChart.Add(new InfluencersMetricsService.Model.StoryUserBestHour()
                            {
                                Hour = hour,
                                DiaDaSemana = dia,
                                ValorReach = valorReach / qtd
                            });
                        }
                    }

                    var strBestHour = "[";
                    lstBestHourChart.ForEach(f =>
                    {
                        strBestHour += "[" + f.Hour.ToString() + "," + ((int)f.DiaDaSemana).ToString() + "," + f.ValorReach.ToString() + "],";
                    });
                    strBestHour += "]";
                    ViewBag.BestHourCoordenates = strBestHour;

                    string MapTo(int maxValue, int maxResult, int value)
                    {
                        var onePercent1 = 100m / maxValue;
                        var percent1 = value * onePercent1;
                        var onePercent2 = maxResult / 100m;
                        var result = percent1 * onePercent2;
                        return result.ToString("N0");
                    }

                    var maxReach = lstBestHourChart.Max(a => a.ValorReach);
                    var bestHoursChartData = lstBestHourChart.Select(a =>
                    {
                        return new string[]
                        {
                            (1 + (int)a.DiaDaSemana).ToString(),
                            a.Hour.ToString(),
                            MapTo(maxReach, 125, a.ValorReach)
                        };
                    }).ToList();
                    ViewBag.BestHourChartData = bestHoursChartData;

                    var best = lstBestHourChart.Where(w => w.ValorReach == lstBestHourChart.Max(m => m.ValorReach)).FirstOrDefault();
                    @ViewBag.BestDay = best.DiaDaSemana.ToString().ToUpper();
                    @ViewBag.BestTime = best.Hour;
                }
                #endregion

                #region Calculo de Engajamento 
                linhaerro = "Engajamento";
                var engComentarios = 0;
                engComentarios = Convert.ToInt32(inf.Comentarios);
                var engCurtidas = inf.Curtidas;

                var mediaEngaj = Convert.ToDouble(engComentarios + engCurtidas) / Convert.ToDouble(objMinhasMidias.Count());
                var mediaPercent = (mediaEngaj / inf.Seguidores) * 100;

                inf.percentAvg = Math.Round(mediaPercent, 2);
                if (mediaPercent < valor0)
                {
                    inf.Aprovado = 0;
                }
                else if (mediaPercent < valor1)
                {
                    inf.Aprovado = 1;
                }
                else
                {
                    inf.Aprovado = 2;
                }

                inf.Powerful = CalculoPowerful(lstCities);//lstStoryGraphs);
                var calcPowerFul = (Convert.ToDouble(inf.Powerful) / Convert.ToDouble(inf.Seguidores) * 100);
                if (calcPowerFul < valor0)
                {
                    inf.Aprovado = 0;
                }
                else if (calcPowerFul < valor1)
                {
                    inf.Aprovado = 1;
                }
                else
                {
                    inf.Aprovado = 2;
                }
                #endregion

                #region Minha midias
                linhaerro = "Minha midias";
                var lstMidiasT = objMinhasMidias.ToList();
                var lstMinhasMidias = lstMidiasT.Where(r => r.caption != null)
                    //.Where(z => z.comments_count > 0).ToList()
                    .Where(r => (r.media_url != null || r.permalink != null)).ToList();

                var lstMidias = lstMinhasMidias
                    .Select(x => new Models.DTO.InstaMentions()
                    {
                        UserName = "" + x.caption.ToString(),
                        Used = Math.Round(Convert.ToDouble
                    (
                        Convert.ToDouble(lstMinhasMidias.Count(c => c.caption == x.caption))
                    ), 0),
                        UsedPerc = Math.Round(Convert.ToDouble
                    (
                            (
                                Convert.ToDouble(lstMinhasMidias.Count(c => c.caption == x.caption))
                            /
                                Convert.ToDouble(lstMinhasMidias.Count())
                            )
                    ) * 100, 4),
                        Reach = lstMinhasMidias.Where(c => c.caption == x.caption)
                        .Sum(s => Convert.ToInt32(s.comments_count) + s.like_count),
                        Engagemer = Math.Round(
                                Convert.ToDouble
                                (
                                    (Convert.ToDouble(
                                            lstMinhasMidias.Where(c => c.caption == x.caption)
                                            .Sum(s => Convert.ToInt32(s.comments_count))
                                        ) +
                                        Convert.ToDouble(
                                            lstMinhasMidias.Where(c => c.caption == x.caption)
                                                .Sum(s => Convert.ToInt32(s.like_count))
                                            )
                                    )
                                    /
                                    Convert.ToDouble(inf.Seguidores)
                                ) * 100, 4),
                        DiffUsedEngag = 0,
                        Imagens = new List<string>()
                        {
                            (x.media_url==null)?x.permalink.ToString():x.media_url.ToString()
                        },
                        Impressions = x.Impressions,
                        Reachs = x.Reach,
                        Saveds = x.Saved,
                        Engagements = x.Engagement
                    }).ToList();
                inf.LstInstaMidias = lstMidias.Take(40) //.DistinctBy(d => d.us.UserName)
                    .ToList();
                var mediaMidias = lstMinhasMidias.DistinctBy(d => d.id).Count();
                var mediaMidiasVideo = lstMinhasMidias.Where(w => w.media_type.Contains("VIDEO")).Count();
                var mediaMidiasImage = lstMinhasMidias.Where(w => !w.media_type.Contains("VIDEO")).Count();

                var percentmediaMidiasVideo = mediaMidias == 0 ? 0d : Convert.ToDouble((Convert.ToDouble(mediaMidiasVideo) / (mediaMidias) * 100));
                var percentmediaMidiasImage = mediaMidias == 0 ? 0d : Convert.ToDouble((Convert.ToDouble(mediaMidiasImage) / (mediaMidias) * 100));

                ViewBag.mediaMidiasVideo = percentmediaMidiasVideo;
                ViewBag.mediaMidiasImage = percentmediaMidiasImage;
                infComentariosTotal = lstMinhasMidias.Sum(s => s.comments_count);
                #endregion

                #region Mentions
                linhaerro = "Hashtags";
                if (lstMongoMentions.Count > 0)
                {
                    var lstMentions = lstMongoMentions.Select(s => new Models.InstaMediaHash()
                    {
                        Hashs = SplitMentions(s.caption.ToUpper()),
                        InstaMedia = new InstaMedia()
                        {
                            Images = new List<InstaImage>() {
                                new InstaImage()
                                {
                                    URI = s.media_url
                                }
                            },
                            CommentsCount = s.comments_count.ToString(),
                            LikesCount = s.like_count
                        },
                        Impressions = s.Impressions,
                        Reachs = s.Reach,
                        Saveds = s.Saved,
                        Engagement = s.Engagement
                    }).ToList();
                    List<string> mentions = new List<string>();
                    lstMentions.ForEach(s =>
                    {
                        s.Hashs.ForEach(f =>
                        {
                            mentions.Add(f);
                        });
                    });
                    var lstImagensEmentions = new List<DtoHash>();
                    foreach (var it in lstMentions)
                    {
                        foreach (var h in it.Hashs)
                        {
                            lstImagensEmentions.Add(
                            new DtoHash()
                            {
                                hash = h,
                                URIImagem = (it.InstaMedia.Images.Count > 0 ? it.InstaMedia.Images.FirstOrDefault().URI : "")
                            });
                        }
                    }
                    var lstMentionsDist = mentions.DistinctBy(x => x).ToList();
                    var lstMentionsFinal = lstMentionsDist.Select(x => new Models.DTO.InstaMentions()
                    {
                        UserName = x
                        ,
                        UsedPerc = Math.Round(Convert.ToDouble
                        (
                                (
                                    Convert.ToDouble(lstMentions.Where(u => u.Hashs.Contains(x)).Count())
                                /
                                    Convert.ToDouble(mentions.Count())
                                )
                        ) * 100, 4),
                        Used = Math.Round(Convert.ToDouble
                        (
                            Convert.ToDouble(lstMentions.Where(u => u.Hashs.Contains(x)).Count())

                        ), 0),
                        Reach = lstMentions.Where(c => c.Hashs.Contains(x))
                            .Sum(s => Convert.ToInt32(s.InstaMedia.CommentsCount) + s.InstaMedia.LikesCount),
                        Engagemer = Math.Round(
                                    Convert.ToDouble(
                                    (Convert.ToDouble(
                                            lstMentions.Where(c => c.Hashs.Contains(x))
                                            .Sum(s => Convert.ToInt32(s.InstaMedia.CommentsCount))
                                        ) +
                                        Convert.ToDouble(
                                            lstMentions.Where(c => c.Hashs.Contains(x))
                                                .Sum(s => Convert.ToInt32(s.InstaMedia.LikesCount))
                                            )
                                    )
                                    /
                                    Convert.ToDouble(
                                        lstMentions.Where(c => c.Hashs.Contains(x))
                                        .Sum(s => mentions.Count)
                                        )) * 100, 4),
                        DiffUsedEngag = 1,
                        Imagens =
                                        lstImagensEmentions
                                        .Where(c => c.hash == x).DistinctBy(d => d.URIImagem)
                                        .Select(s => s.URIImagem).ToList(),
                        Impressions = lstMentions.Where(c => c.Hashs.Contains(x)).Sum(s => s.Impressions),
                        Reachs = lstMentions.Where(c => c.Hashs.Contains(x)).Sum(s => s.Reachs),
                        Saveds = lstMentions.Where(c => c.Hashs.Contains(x)).Sum(s => s.Saveds),
                        Engagements = lstMentions.Where(c => c.Hashs.Contains(x)).Sum(s => s.Engagement),
                    }).ToList();
                    inf.LstInstaMentions = lstMentionsFinal.OrderByDescending(o => o.Reach).Take(40).ToList();
                }
                #endregion

                #region HashTags
                linhaerro = "Hashtags";
                if (lstMinhasMidias.Count > 0)
                {
                    var objHash = lstMinhasMidias.Where(w => w.caption.Contains("#"));
                    var lstHash = objHash.Where(x => x.comments != null).ToList();
                    var newLstHash = lstHash.Select(s => new Models.InstaMediaHash
                    {
                        Hashs = SplitHash(s.caption.ToUpper()),
                        InstaMedia = new InstaMedia()
                        {
                            Images = new List<InstaImage>() {
                                new InstaImage()
                                {
                                    URI = s.media_url
                                }
                            },
                            CommentsCount = s.comments_count.ToString(),
                            LikesCount = s.like_count
                        },
                        Impressions = s.Impressions,
                        Reachs = s.Reach,
                        Saveds = s.Saved,
                        Engagement = s.Engagement
                    }
                    ).ToList();

                    List<string> hashs = new List<string>();
                    newLstHash.ForEach(s =>
                    {
                        s.Hashs.ForEach(f =>
                        {
                            hashs.Add(f);
                        });
                    }
                    );

                    var lstImagensEhashs = new List<DtoHash>();
                    foreach (var it in newLstHash)
                    {
                        foreach (var h in it.Hashs)
                        {
                            lstImagensEhashs.Add(
                            new DtoHash()
                            {
                                hash = h,
                                URIImagem = (it.InstaMedia.Images.Count > 0 ? it.InstaMedia.Images.FirstOrDefault().URI : "")
                            });
                        }
                    }

                    var lstHashsDist = hashs.DistinctBy(x => x).ToList();
                    var lstHashs = lstHashsDist.Select(x => new Models.DTO.InstaMentions()
                    {
                        UserName = x
                        ,
                        UsedPerc = Math.Round(Convert.ToDouble
                        (
                                (
                                    Convert.ToDouble(newLstHash.Where(u => u.Hashs.Contains(x)).Count())
                                /
                                    Convert.ToDouble(hashs.Count())
                                )
                        ) * 100, 4),
                        Used = Math.Round(Convert.ToDouble
                        (
                            Convert.ToDouble(newLstHash.Where(u => u.Hashs.Contains(x)).Count())

                        ), 0),
                        Reach = newLstHash.Where(c => c.Hashs.Contains(x))
                            .Sum(s => Convert.ToInt32(s.InstaMedia.CommentsCount) + s.InstaMedia.LikesCount),
                        Engagemer = Math.Round(
                                    Convert.ToDouble(
                                    (Convert.ToDouble(
                                            newLstHash.Where(c => c.Hashs.Contains(x))
                                            .Sum(s => Convert.ToInt32(s.InstaMedia.CommentsCount))
                                        ) +
                                        Convert.ToDouble(
                                            newLstHash.Where(c => c.Hashs.Contains(x))
                                                .Sum(s => Convert.ToInt32(s.InstaMedia.LikesCount))
                                            )
                                    )
                                    /
                                    Convert.ToDouble(
                                        newLstHash.Where(c => c.Hashs.Contains(x))
                                        .Sum(s => hashs.Count)
                                        )) * 100, 4),
                        DiffUsedEngag = 1,
                        Imagens =
                                        lstImagensEhashs
                                        .Where(c => c.hash == x).DistinctBy(d => d.URIImagem)
                                        .Select(s => s.URIImagem).ToList(),
                        Impressions = newLstHash.Where(c => c.Hashs.Contains(x)).Sum(s => s.Impressions),
                        Reachs = newLstHash.Where(c => c.Hashs.Contains(x)).Sum(s => s.Reachs),
                        Saveds = newLstHash.Where(c => c.Hashs.Contains(x)).Sum(s => s.Saveds),
                        Engagements = newLstHash.Where(c => c.Hashs.Contains(x)).Sum(s => s.Engagement),
                    }).ToList();
                    inf.LstInstaHashs = lstHashs.OrderByDescending(o => o.Reach).Take(40).ToList();
                }
                #endregion

                #region TagsMentionedBy
                linhaerro = "MentionedByTag";
                if (mongoTagsUnico != null && (mongoTagsUnico.Count > 0))
                {
                    var lstTag = mongoTagsUnico.Where(x => x.caption != null).ToList();

                    var lstTags = lstTag.Select(x => new Models.DTO.InstaMentions()
                    {
                        UserName = x.username,
                        UsedPerc = Math.Round(Convert.ToDouble
                        (
                                (
                                    Convert.ToDouble(lstTag.Where(u => u.username.Equals(x.username)).Count())
                                /
                                    Convert.ToDouble(lstTag.Count())
                                )
                        ) * 100, 4),
                        Used = 1/*Math.Round(Convert.ToDouble
                        (
                            Convert.ToDouble(lstTag.Where(u => u.username.Equals(x.username)).Count())

                        ), 0)*/,
                        Reach = x.comments_count + x.like_count,
                        Engagemer = Math.Round(
                                    Convert.ToDouble(
                                    (Convert.ToDouble(
                                            lstTag.Where(u => u.username.Equals(x.username))
                                            .Sum(s => Convert.ToInt32(s.comments_count))
                                        ) +
                                        Convert.ToDouble(
                                            lstTag.Where(u => u.username.Equals(x.username))
                                                .Sum(s => Convert.ToInt32(s.like_count))
                                            )
                                    )
                                    /
                                    Convert.ToDouble(
                                        lstTag.Where(u => u.username.Equals(x.username))
                                        .Sum(s => lstTag.Count)
                                        )) * 100, 4),
                        DiffUsedEngag = 1,
                        Imagens =
                                        new List<string>(){
                                            x.media_url
                                        }
                    }).ToList();
                    inf.LstInstaTags = lstTags.OrderByDescending(o => o.Reach).Take(40).ToList();
                }
                #endregion

                #region Ages e Gender
                linhaerro = "Gender and Ages";
                if (lstAge != null)
                {
                    var lstAgesGender = lstAge.data.values.FirstOrDefault().value
                        .Select(x => new
                        {
                            Gender = x.name.Split('.').GetValue(0),
                            Faixa = x.name.Split('.').GetValue(1),
                            Used = 0,
                            UsedPerc = 0,
                            Reach = Convert.ToInt32(x.valor),
                            Engagemer = 0,
                            DiffUsedEngag = 0
                        }).ToList();

                    var lstAgesMidias = lstAge.data.values.FirstOrDefault().value
                        .Select(x => new Models.DTO.InstaMentions()
                        {
                            UserName = "" + x.name,
                            Used = 0,
                            UsedPerc = 0,
                            Reach = Convert.ToInt32(x.valor),
                            Engagemer = Convert.ToDouble(x.valor) / Convert.ToDouble(metricas.Cabecalho.Seguidores) * 100,
                            DiffUsedEngag = 0,
                            Imagens = null
                        }).ToList();
                    inf.LstAge = lstAgesMidias.OrderByDescending(o => o.Reach).Take(40) //.DistinctBy(d => d.us.UserName)
                        .ToList();

                    var listaArrayM = "";
                    var somaM = 0;
                    lstAgesMidias.ForEach(f =>
                    {
                        if (f.UserName.IndexOf("M") > -1)
                        {
                            listaArrayM += (f.Reach * -1).ToString() + ",";
                            somaM += f.Reach;
                        }
                    });
                    var listaArrayF = "";
                    var somaF = 0;
                    lstAgesMidias.ForEach(f =>
                    {
                        if (f.UserName.IndexOf("F") > -1)
                        {
                            listaArrayF += f.Reach.ToString() + ",";
                            somaF += f.Reach;
                        }
                    });
                    ViewBag.listaArrayM = listaArrayM;
                    ViewBag.listaArrayF = listaArrayF;
                    ViewBag.PercentM = Math.Round((Convert.ToDouble(somaM) / Convert.ToDouble(Convert.ToInt32(objUser.followers_count))) * 100, 2);
                    ViewBag.PercentF = Math.Round((Convert.ToDouble(somaF) / Convert.ToDouble(Convert.ToInt32(objUser.followers_count))) * 100, 2);
                }
                #endregion

                #region Cities
                linhaerro = "Cities";
                if (lstObjCities != null && lstObjCities.Count() > 0 && lstObjCities.FirstOrDefault().data.Count > 0)
                {
                    var lstCitiesResult = lstObjCities.FirstOrDefault().data
                        .Select(x => new
                        {
                            City = x.Key,
                            Number = x.Value,
                            Used = 0,
                            UsedPerc = 0,
                            Reach = 0,
                            Engagemer = 0,
                            DiffUsedEngag = 0
                        }).ToList();

                    var lstCitiesResults = lstObjCities.FirstOrDefault().data
                        .Select(x => new Models.DTO.InstaMentions()
                        {
                            UserName = "" + x.Key,
                            Used = 0,
                            UsedPerc = 0,
                            Reach = Convert.ToInt32(x.Value),
                            Engagemer = Convert.ToDouble(x.Value) / Convert.ToDouble(metricas.Cabecalho.Seguidores) * 100,
                            DiffUsedEngag = 0,
                            Imagens = null
                        }).ToList();
                    inf.LstCities = lstCitiesResults.OrderByDescending(o => o.Reach).Take(20) //.DistinctBy(d => d.us.UserName)
                        .ToList();
                    string lstCitiesArray = "[['City', 'Engagement'],";
                    inf.LstCities.ForEach(f =>
                    {
                        var city = f.UserName.Split(',')[0];
                        city = city == null ? f.UserName : city;
                        lstCitiesArray += "['" + city + "'," + f.Reach.ToString() + "],";
                    });
                    lstCitiesArray += "]";

                    /*
                    [
                        ['City',   'Population', 'Area'],
                        ['São Paulo',      2761477,    1285.31],
                        ['Curitiba',     1324110,    181.76],
                        ['Belo Horizonte',    959574,     117.27],
                        ['Campinas',     907563,     130.17],
                        ['Guarulhos',   655875,     158.9],
                        ['Cotia',     607906,     243.60],
                        ['Belem',   380181,     140.7],
                        ['Tocantins',  371282,     102.41],
                        ['Brasilia', 67370,      213.44],
                        ['Acre',     52192,      43.43],
                        ['Chapecó',  38262,      11]
                      ]
                      */
                    lstCitiesArray = lstCitiesArray.Replace("],]", "]]");
                    ViewBag.LstCitiesArray = lstCitiesArray;
                    var percCities = Math.Round((Convert.ToDouble(inf.LstCities.FirstOrDefault().Reach) / Convert.ToDouble(Convert.ToInt32(objUser.followers_count))) * 100, 2);
                    ViewBag.PercentCities = percCities.ToString() + "% - " + inf.LstCities.FirstOrDefault().UserName;
                    //var latitudes = await Coordenates(lstCitiesResults.FirstOrDefault().UserName);
                }
                #endregion

                #region Top and Botton POST
                if (inf.LstInstaMidias.Count() > 0)
                {
                    inf.LstTopAndBotton = inf.LstInstaMidias.Where(w => w.Engagemer == inf.LstInstaMidias.Max(m => m.Engagemer))
                        .Union(
                        inf.LstInstaMidias.Where(w => w.Engagemer == inf.LstInstaMidias.Min(m => m.Engagemer))).ToList();
                }
                #endregion

                #region Stories
                linhaerro = "Stories";

                var lstGraphsStory = lstStoryGraphs.Where(w=>w.Obj != null).Select(s => new Models.DTO.Story()
                {
                    ImpressionsValue = s.Obj.data.Where(w => w.name == "impressions").FirstOrDefault().values.FirstOrDefault().value,
                    ReachValue = s.Obj.data.Where(w => w.name == "reach").FirstOrDefault().values.FirstOrDefault().value,
                    ExitsValue = s.Obj.data.Where(w => w.name == "exits").FirstOrDefault().values.FirstOrDefault().value,
                    RepliesValue = s.Obj.data.Where(w => w.name == "replies").FirstOrDefault().values.FirstOrDefault().value,
                    TapsForwardValue = s.Obj.data.Where(w => w.name == "taps_forward").FirstOrDefault().values.FirstOrDefault().value,
                    TapsBackValue = s.Obj.data.Where(w => w.name == "taps_back").FirstOrDefault().values.FirstOrDefault().value,
                    DateCreation = s.DateCreation,
                    Id = (s.Obj.data.FirstOrDefault().id.Split('/')[0]).ToString().ToUpper(),
                    TimeSpan = s.timeSpan
                    //(lstStoryGraphs.Where(lw => lw.Obj.data.FirstOrDefault().id.Split('/')[0] ==
                    //                    s.Obj.data.FirstOrDefault().id.Split('/')[0] &&
                    //                    s.DateCreation == lw.DateCreation).Max(mt => mt.timeSpan))
                });

                var lstTeste = lstGraphsStory.ToList();

                var lstGraphsStoryIds = lstGraphsStory
                    .GroupBy(g => new { g.Id })
                    .Select(s => new
                    {
                        Id = s.Key.ToString().ToUpper(),
                        TimeSpanMax = s.Max(ma => ma.TimeSpan)
                    });

                var lstStory = new List<Models.DTO.Story>();
                foreach (var it in lstGraphsStory.DistinctBy(d => d.Id))
                {
                    var _maxTime = lstGraphsStory.Where(w => w.Id.ToString().Equals(it.Id)).ToList();
                    var maxTime = _maxTime.Max(m => m.TimeSpan);
                    var newStory = lstGraphsStory.Where(w => w.Id == it.Id && w.TimeSpan == maxTime).FirstOrDefault();
                    lstStory.Add(new Story()
                    {
                        DateCreation = newStory.DateCreation,
                        Date = Convert.ToDateTime(new DateTime(01, 01, 01, 0, 0, 0).Add(newStory.TimeSpan)).ToString("dd/MM/yyyy hh:mm"), // lstStoryIdsTmp.Where(w=>w.Obj newStory.DateCreation.ToString("dd/MM/yyyy"),
                        Id = newStory.Id,
                        TimeSpan = newStory.TimeSpan,
                        ExitsValue = newStory.ExitsValue,
                        ImpressionsValue = newStory.ImpressionsValue,
                        ReachValue = newStory.ReachValue,
                        RepliesValue = newStory.RepliesValue,
                        TapsBackValue = newStory.TapsBackValue,
                        TapsForwardValue = newStory.TapsForwardValue,
                        Imagens =
                                new List<string>(){
                                    //"https://www.influencersmetrics.com/story/image/" +
                                    "https://s3.amazonaws.com/influencersmetrics/" +
                                        (_lstStoryIds.Exists(w=>w.Contains(newStory.Id) && w.Contains("IMAGE"))? newStory.Id:"VIDEO") + ".jpg"
                                }
                    });
                }

                var lstGraphStoriesI = lstStory.Select(s => new
                {
                    Name = "Impressions",
                    DateCreation = s.DateCreation,
                    Id = s.Id,
                    Avg = s.ImpressionsValue
                }).Take(10);

                var lstGraphStoriesR = lstStory.Select(s => new
                {
                    Name = "Reach",
                    DateCreation = s.DateCreation,
                    Id = s.Id,
                    Avg = s.ReachValue
                }).Take(10);

                var lstGraphStoriesE = lstStory.Select(s => new
                {
                    Name = "Exits",
                    DateCreation = s.DateCreation,
                    Id = s.Id,
                    Avg = s.ExitsValue
                }).Take(10);

                var lstGraphStoriesRp = lstStory.Select(s => new
                {
                    Name = "Replies",
                    DateCreation = s.DateCreation,
                    Id = s.Id,
                    Avg = s.RepliesValue
                }).Take(10);

                var lstGraphStoriesTf = lstStory.Select(s => new
                {
                    Name = "Taps forward",
                    DateCreation = s.DateCreation,
                    Id = s.Id,
                    Avg = s.TapsForwardValue
                }).Take(10);

                var lstGraphStoriesTb = lstStory.Select(s => new
                {
                    Name = "Taps back",
                    DateCreation = s.DateCreation,
                    Id = s.Id,
                    Avg = s.TapsBackValue
                }).Take(10);

                var lstGraphStories = lstGraphStoriesI.Union(lstGraphStoriesR).Union(lstGraphStoriesE)
                                      .Union(lstGraphStoriesRp).Union(lstGraphStoriesTf).Union(lstGraphStoriesTb);
                if (lstGraphStories.Count() > 0)
                {
                    var lstStories = lstGraphStories.Select(x => new Models.DTO.InstaMentions()
                    {
                        UserName = x.Name,
                        UsedPerc = 0,
                        Used = 1,
                        Reach = Convert.ToInt32((
                                lstGraphStories.Where(w => w.Id == x.Id && x.Name == w.Name) //dtCriacao.AddDays(1)
                                                .Max(z => z.Avg))
                                                ),
                        Engagemer = 0,
                        DiffUsedEngag = 1,
                        Imagens =
                                        new List<string>(){
                                            //"https://www.influencersmetrics.com/story/image/" +
                                            "https://s3.amazonaws.com/influencersmetrics/" +
                                                (_lstStoryIds.Exists(w=>w.Contains(x.Id) && w.Contains("IMAGE"))? x.Id:"VIDEO") + ".jpg"
                                        }
                    });

                    var mediaStory = lstGraphsStory.DistinctBy(d => d.Id).Count();
                    var dataMaxStory = lstGraphsStory.Max(m => m.DateCreation);
                    var dataMinStory = lstGraphsStory.Min(m => m.DateCreation);
                    var diffMaxMin = (dataMaxStory.Date - dataMinStory.Date).Days;
                    mediaStory = mediaStory / (diffMaxMin == 0 ? 1 : diffMaxMin);
                    ViewBag.MediaStories = mediaStory;

                    mediaStoryVideo = lstStory.Where(w => w.Imagens.Exists(e => e.Contains("VIDEO.jpg"))).DistinctBy(d => d.Id).Count();
                    mediaStoryImage = lstStory.Where(w => w.Imagens.Exists(e => !e.Contains("VIDEO.jpg"))).DistinctBy(d => d.Id).Count();

                    percentmediaStoryVideo = Convert.ToDouble((Convert.ToDouble(mediaStoryVideo) / (lstStory.DistinctBy(d => d.Id).Count()) * 100));
                    percentmediaStoryImage = Convert.ToDouble((Convert.ToDouble(mediaStoryImage) / (lstStory.DistinctBy(d => d.Id).Count()) * 100));

                    inf.LstInstaStories = lstStories.Take(60).ToList();
                    inf.LstInstaStory = lstStory.OrderByDescending(x => x.DateCreation).ToList();
                    infRepliesTotal = lstStory.Sum(s => s.RepliesValue);
                }

                #endregion

                #region Emotional
                linhaerro = "Emotional";
                if (lstFaceDetection.Count > 0)
                {
                    var lstSemNota = lstFaceDetection.FirstOrDefault().Obj
                        .Where(w => w.Joy == 0 && w.Anger == 0 && w.Surprise == 0 && w.Sorrow == 0)
                        .Select(s => s.UrlImagem).ToList();

                    var ls = from s in lstFaceDetection.FirstOrDefault().Obj
                             where !lstSemNota.Any(es => (es == s.UrlImagem))
                             select s;

                    var lstFaceDetections = ls
                        .Select(s => new Models.FaceDetection()
                        {
                            Anger = (s.Anger), //Raiva
                            Joy = (s.Joy), //Alegria
                            Sorrow = (s.Sorrow), //Tristeza
                            Surprise = (s.Surprise), //Surpresa
                            DtAvaliacao = s.DtAvaliacao,
                            UrlImagem = s.UrlImagem,
                            UserName = s.UserName
                        }).ToList();

                    var lstAvgFaceDetection = lstFaceDetections.Select(l => new Models.FaceDetection()
                    {
                        Anger = lstFaceDetections.Sum(s => (s.Anger)), //Raiva
                        Joy = lstFaceDetections.Sum(s => (s.Joy)), //Alegria
                        Sorrow = lstFaceDetections.Sum(s => s.Sorrow), //Tristeza
                        Surprise = lstFaceDetections.Sum(s => (s.Surprise)), //Surpresa
                        DtAvaliacao = lstFaceDetections.FirstOrDefault().DtAvaliacao,
                        UrlImagem = "",
                        UserName = ""
                    });
                    var avgFaceDetection = lstAvgFaceDetection.DistinctBy(d => d.UserName).FirstOrDefault();
                    if (avgFaceDetection == null)
                    {

                    }
                    else
                    {
                        inf.LstFaceDetection = lstFaceDetections.ToList();
                        inf.AvgFaceDetection = avgFaceDetection;

                        var listaFaceDetection = (avgFaceDetection.Joy / lstFaceDetection.Count).ToString() + "," +
                            (avgFaceDetection.Sorrow / lstFaceDetection.Count).ToString() + "," +
                            (avgFaceDetection.Anger / lstFaceDetection.Count).ToString() + "," +
                            (avgFaceDetection.Surprise / lstFaceDetection.Count).ToString() + "";
                        var cabecalhoFaceDetection = "'Alegre','Tristeza','Raiva','Surpresa'";

                        ViewBag.CabecalhoFaceDetection = cabecalhoFaceDetection;
                        ViewBag.ListaFaceDetection = listaFaceDetection;

                        inf.CabecalhoFaceDetection = cabecalhoFaceDetection;
                        inf.ListaFaceDetection = listaFaceDetection;

                        var lstEmotionals = ls.Select(s => new Models.FaceDetection()
                        {
                            Anger = (s.Anger), //Raiva
                            Joy = (s.Joy), //Alegria
                            Sorrow = (s.Sorrow), //Tristeza
                            Surprise = (s.Surprise), //Surpresa
                            DtAvaliacao = s.DtAvaliacao,
                            UrlImagem = s.UrlImagem,
                            UserName = s.UserName
                        }).ToList();

                        var newLstEmotional = lstEmotionals.Select(s => new
                        {
                            Joy = lstEmotionals.Where(w => w.UserName == s.UserName).Sum(sm => sm.Joy),
                            Sorrow = lstEmotionals.Where(w => w.UserName == s.UserName).Sum(sm => sm.Sorrow),
                            Anger = lstEmotionals.Where(w => w.UserName == s.UserName).Sum(sm => sm.Anger),
                            Surprise = lstEmotionals.Where(w => w.UserName == s.UserName).Sum(sm => sm.Surprise)
                        }
                        ).ToList().Distinct();

                        inf.LstInstaEmotionalResume = new List<InstaMentions>();

                        newLstEmotional.ForEach(f =>
                        {
                            inf.LstInstaEmotionalResume.Add(new InstaMentions()
                            {
                                UserName = "ALEGRIA",
                                Reach = f.Joy
                            });
                            inf.LstInstaEmotionalResume.Add(new InstaMentions()
                            {
                                UserName = "TRISTEZA",
                                Reach = f.Sorrow
                            });
                            inf.LstInstaEmotionalResume.Add(new InstaMentions()
                            {
                                UserName = "RAIVA",
                                Reach = f.Anger
                            });
                            inf.LstInstaEmotionalResume.Add(new InstaMentions()
                            {
                                UserName = "SURPRESA",
                                Reach = f.Surprise
                            });
                        });

                        var strEmotionalNumeros = "";
                        var totalEmotional = inf.LstInstaEmotionalResume.Sum(s => s.Reach);
                        inf.LstInstaEmotionalResume.ForEach(f =>
                        {
                            f.Engagemer = Convert.ToDouble(((f.Reach / totalEmotional) * 100));
                            strEmotionalNumeros += f.Reach.ToString() + ",";
                        });

                        ViewBag.EmotionalResume = strEmotionalNumeros;
                    }
                }
                #endregion

                ViewBag.mediaStoryVideo = percentmediaStoryVideo;
                ViewBag.mediaStoryImage = percentmediaStoryImage;

                ViewBag.infRepliesTotal = infRepliesTotal;
                ViewBag.infComentariosTotal = infComentariosTotal;

                ViewBag.PercSuspTotal = Math.Round(Convert.ToDouble(inf.Reach) * (Convert.ToDouble(ViewBag.PercSusp)/100),0);
                ViewBag.PercMassTotal = Math.Round(Convert.ToDouble(inf.Reach) * (Convert.ToDouble(ViewBag.PercMass) / 100), 0);
                ViewBag.PercInfluTotal = Math.Round(Convert.ToDouble(inf.Reach) * (Convert.ToDouble(ViewBag.PercInflu) / 100), 0);
                ViewBag.PercReaisTotal = Math.Round(Convert.ToDouble(inf.Reach) * (Convert.ToDouble(ViewBag.PercReais) / 100), 0);

                metricas.inf = inf;

                HttpContext.Session.SetString("ProfilePictureMidiakit", HttpUtility.UrlDecode(inf.ProfilePicture));
                HttpContext.Session.SetString("NomeCompleto", inf.NomeCompleto);
                HttpContext.Session.SetString("UserName", inf.UserName);
                HttpContext.Session.SetString("SocialContext", inf.SocialContext.Replace("\n", " "));
                await repMongo.GravarOne<Models.DTO.InfluencersResumoFree>(inf);

                return View(_nameView, metricas);
            }
            catch (Exception ex)
            {
                ViewBag.Message = "Erro inesperado ao processar a visualização.<br />Por favor tent novamente em alguns minutos (" + linhaerro + ")";
                return View("ViewFree", new webMetrics.Models.DTO.InfluencersResumoFree());
            }
        }

        private static object CalculoPowerful(List<ContractClass<Models.Graph.InsightsGenderAge>> lstCitiesInsights)
        //private static object CalculoPowerFul(List<ContractClass<InfluencersMetricsService.Model.StoryInsights>> lstStoryGraphs)
        {
            object _powerful = 0;

            var citiesInsights = lstCitiesInsights.FirstOrDefault();

            var cityTop = citiesInsights == null ? null :
                    citiesInsights.Obj == null ? null : citiesInsights.Obj.data == null ? null : citiesInsights.Obj.data.FirstOrDefault();
            var city = cityTop == null ? null : cityTop.values.FirstOrDefault();
            var lstCity = city == null ? null : city.value.Select(s => new
            {
                valor = s.Value,
                key = s.Key
            });
            var _maxCity = lstCity == null ? null : lstCity.Where(w => w.valor == lstCity.Max(m => m.valor)).FirstOrDefault();
            var maxCity = (_maxCity == null ? null : _maxCity.key);
            _powerful = (_maxCity == null ? 0 : _maxCity.valor);

            /*Powerful por story
            if (lstStoryGraphs.Count() > 0)
            {
                var _maxStory = 0;
                var _minStory = 0;
                List<InfluencersMetricsService.Model.ValueStoryInsights> _objStories =
                    new List<InfluencersMetricsService.Model.ValueStoryInsights>();
                lstStoryGraphs.ForEach(l =>
                {
                    l.Obj.data.ForEach(s =>
                    {
                        s.values.ForEach(sl =>
                        {
                            _objStories.Add(sl);
                        });
                    });
                });

                _maxStory = _objStories.Max(m => m.value);
                _minStory = _objStories.Min(m => m.value);
                _powerful = Convert.ToInt32((_maxStory + _minStory) / 2);
            }
            */
            return _powerful;
        }
    }

    public class DtoHash
    {
        public string hash { get; set; }
        public string URIImagem { get; set; }

    }
    public class PhotoPerfil : HttpPostedFileBase
    {
        //public HttpPostedFileBase photo { get; set; }
    }
}