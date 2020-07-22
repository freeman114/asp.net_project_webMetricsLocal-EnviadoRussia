using influencersMetrics;
using InfluencersMetrics;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using webMetrics.Models;
using Wirecard;
using Wirecard.Models;

namespace InfluencersMetricsService
{
    public static class Library
    {
        public static string _urlBaseApi = "https://localhost:44318/facedata/";
        public static string _urlPayApi = "https://localhost:44318/pagamento/";

        //public static string _urlBaseApi = "https://www.influencersmetrics.com/facedata/";
        //public static string _urlPayApi = "https://www.influencersmetrics.com/pagamento/";

        public static void WriteErrorLog(string Message)
        {
            StreamWriter sw = null;
            try
            {
                sw = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "\\LogFile.txt", true);
                sw.WriteLine(DateTime.Now.ToString() + ": " + Message);
                sw.Flush();
                sw.Close();
            }
            catch
            {
            }
        }

        private static HttpClient client = new HttpClient();

        public static async Task<T> GetDataAsync<T>()
        {
            var _urlFull = _urlBaseApi + "GetUsersTokensAsync";
            try
            {
                var response = await client.GetAsync(_urlFull);

                if (!response.IsSuccessStatusCode)
                {
                    return default(T);
                }

                var result = await response.Content.ReadAsStringAsync();

                return JsonConvert.DeserializeObject<T>(result);
            }
            catch (Exception ex)
            {
                WriteErrorLog("GetDataAsync: " + ex.Message);
                return default(T);
            }
        }

        public static async Task<T> GetInsightUserAsync<T>()
        {
            var _urlFull = _urlBaseApi + "GetInsightUserAsync";
            try
            {
                var response = await client.GetAsync(_urlFull);

                if (!response.IsSuccessStatusCode)
                {
                    return default(T);
                }

                var result = await response.Content.ReadAsStringAsync();

                return JsonConvert.DeserializeObject<T>(result);
            }
            catch (Exception ex)
            {
                WriteErrorLog("GetInsightUserAsync: " + ex.Message);
                return default(T);
            }
        }

        public static async Task<bool> SetPayment(string _id)
        {
            try
            {
                string uri = "GetPayment?pagamentoId=" + _id;
                var _urlFull = _urlPayApi + uri;
                var response = await client.GetAsync(_urlFull);

                if (!response.IsSuccessStatusCode)
                {
                    return false;
                }

                return true;

            }
            catch (Exception ex)
            {
                WriteErrorLog("SetPayment: " + ex.Message);
                return false;
            }
        }


        public static Environments ambiente { get; set; }

        private static Wirecard.WirecardClient SetAmbiente(Wirecard.WirecardClient WC)
        {
            var token = "";
            var chave = "";
            ambiente = Environments.PRODUCTION;
            token = "TE4ZG2UYZJ3GOPQ8KPFMJV9Y2QKS2CHB";
            chave= "NVNSCOEIJORVLF2WB4OKVZ2URHXW6KYY8TLUXR7Z";
            //token = _appSettings.TokenMOIP;
            //chave = _appSettings.ChaveMOIP;
#if DEBUG
            //ambiente = Environments.SANDBOX;
            //token = "I6TNGJK392BZNFOWJNM0BLPR9MDHUUTS";
            //chave = "JZCWSVK7JLTEWBP7ANLMI0TM2IPYNH4CYZQH7YZZ";
#endif
            WC = new Wirecard.WirecardClient(ambiente, token, chave);

            return WC;
        }


        private static IOptions<AppSettings> _settings;
        private static void setSetting()
        {
            _settings = Options.Create(new webMetrics.Models.AppSettings()
            {
                ConexaoMongoDB = "mongodb://myuserMetrics:28111981@168.235.111.153:27018"
            });
        }

        public static async Task<bool> SetPaymentInvoices()
        {
            setSetting();
            Wirecard.WirecardClient WC = null;
            WC = SetAmbiente(WC);

            try
            {
                var repMongo = new MongoRep("", _settings, "");
                var lstPays = await repMongo.ListarPendingPayInvoice();
                var lstInvoices = lstPays.Select(s => s._id.ToString()).ToList();

                foreach (var pagamentoId in lstInvoices)
                {
                    var _id = new ObjectId(pagamentoId);

                    var lstPagamentos = await repMongo.ListarById<webMetrics.Models.DTO.PagamentoPage>(_id);
                    if (lstPagamentos != null)
                    {
                        var userId = lstPagamentos.FirstOrDefault().UsuarioId;
                        var _pagamentoAtual = lstPagamentos.FirstOrDefault().Obj;
                        var _pagamentoAtualContractual = lstPagamentos.FirstOrDefault();
                        if (_pagamentoAtual.paymentResponse != null)
                        {
                            #region Pagamentos Comuns
                            var result = await WC.Payment.Consult(_pagamentoAtual.paymentResponse.Id);

                            if (result.Status != _pagamentoAtual.paymentResponse.Status)
                            {
                                _pagamentoAtual.paymentResponse.Status = result.Status;
                                if (_pagamentoAtual.StatusPagamento == "Pendente" && result.Status == "AUTHORIZED")//Pago
                                {
                                    _pagamentoAtual.StatusPagamento = "Pago";
                                }

                                //Mudar Status
                                await repMongo.AlterarStatusPagamento(new ContractClass<webMetrics.Models.DTO.PagamentoPage>()
                                {
                                    _id = _id,
                                    Obj = _pagamentoAtual
                                });

                                if (_pagamentoAtual.StatusPagamento == "Pago")
                                {
                                    //Inserir credito se for authorizado o pagamento
                                    var credito = new webMetrics.Models.CreditoMetricas()
                                    {
                                        UserId = userId,
                                        Qtd = _pagamentoAtual.Quantidade,
                                        DataCredito = DateTime.Now,
                                        Debito = 0,
                                        DataValidade = DateTime.Now.AddMonths(1),
                                        DataCriacao = DateTime.Now
                                    };
                                    await repMongo.GravarOne<webMetrics.Models.CreditoMetricas>(credito);
                                }
                            }
                            if (result.Status == "CANCELLED" && _pagamentoAtual.StatusPagamento == "Pendente")
                            {
                                _pagamentoAtual.StatusPagamento = "Cancelado";
                                //Mudar Status
                                await repMongo.AlterarStatusPagamento(new ContractClass<webMetrics.Models.DTO.PagamentoPage>()
                                {
                                    _id = _id,
                                    Obj = _pagamentoAtual
                                });
                            }
                            if (result.Status == "REFUNDED" && _pagamentoAtual.StatusPagamento == "Pendente")
                            {
                                _pagamentoAtual.StatusPagamento = "Cancelado";
                                //Mudar Status
                                await repMongo.AlterarStatusPagamento(new ContractClass<webMetrics.Models.DTO.PagamentoPage>()
                                {
                                    _id = _id,
                                    Obj = _pagamentoAtual
                                });
                            }
                            if (result.Status == "REFUNDED" && _pagamentoAtual.StatusPagamento == "Pago")
                            {
                                _pagamentoAtual.StatusPagamento = "Cancelado";
                                //Mudar Status
                                await repMongo.AlterarStatusPagamento(new ContractClass<webMetrics.Models.DTO.PagamentoPage>()
                                {
                                    _id = _id,
                                    Obj = _pagamentoAtual
                                });
                            }
                            #endregion
                        }
                        else
                        {
                            if (_pagamentoAtual.subscriptionResponse != null)
                            {
                                #region Invoices
                                var lstResult = await WC.Signature.ListSignatureInvoices(_pagamentoAtual.subscriptionResponse.Code);
                                foreach (var result in lstResult.Invoices)
                                {
                                    if (_pagamentoAtual.Invoices == null)
                                    {
                                        _pagamentoAtual.Invoices = new List<Invoice>();
                                    }
                                    var _invoice = _pagamentoAtual.Invoices.Where(w => w.Id == result.Id).FirstOrDefault();

                                    var novo = false;
                                    if (_invoice == null)
                                    {
                                        novo = true;
                                        _pagamentoAtual.Invoices.Add(result);
                                        _invoice = result;
                                    }

                                    if (result.Status.Code == 3 &&
                                        (
                                            (_invoice.Status.Code !=3 && !novo) || 
                                            (novo)
                                        )
                                        )//Pago
                                    {
                                        _pagamentoAtual.NextInvoice = _pagamentoAtual.NextInvoice.AddMonths(1);

                                        //Inserir credito se for authorizado o pagamento
                                        var credito = new webMetrics.Models.CreditoMetricas()
                                        {
                                            UserId = userId,
                                            Qtd = _pagamentoAtual.Quantidade,
                                            DataCredito = DateTime.Now,
                                            Debito = 0,
                                            DataValidade = DateTime.Now.AddMonths(1),
                                            DataCriacao = DateTime.Now
                                        };
                                        await repMongo.GravarOne<webMetrics.Models.CreditoMetricas>(credito);

                                        //Email de pagamento
                                        var usuarioId = await repMongo.FindFilter<webMetrics.Models.Usuario>("Obj.UserId", _pagamentoAtual.Usuario.UserId);
                                        var envio = SenderEmail.Pagamento(_pagamentoAtual.Usuario.Email, usuarioId._id.ToString());
                                    }

                                    if (_invoice.Status != result.Status)
                                    {
                                        if (result.Status.Code == 4)//Problemas no pagto
                                        {
                                            _pagamentoAtual.StatusPagamento = "Problemas";

                                            //Mudar Status
                                            await repMongo.AlterarStatusPagamento(new ContractClass<webMetrics.Models.DTO.PagamentoPage>()
                                            {
                                                _id = _id,
                                                Obj = _pagamentoAtual
                                            });

                                        }
                                    }

                                    //Mudar Status
                                    await repMongo.AlterarNextInvoice(new ContractClass<webMetrics.Models.DTO.PagamentoPage>()
                                    {
                                        _id = _id,
                                        Obj = _pagamentoAtual
                                    });

                                }

                                _pagamentoAtualContractual.Obj = _pagamentoAtual;
                                await repMongo.AlterarInvoices(_pagamentoAtualContractual);

                                #endregion
                            }
                        }
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public static async Task<T> GetListPaymentsPendentAsync<T>()
        {
            var _urlFull = _urlBaseApi + "GetListPaymentsPendentAsync";
            try
            {
                var response = await client.GetAsync(_urlFull);

                if (!response.IsSuccessStatusCode)
                {
                    return default(T);
                }

                var result = await response.Content.ReadAsStringAsync();

                return JsonConvert.DeserializeObject<T>(result);
            }
            catch (Exception ex)
            {
                WriteErrorLog("GetListPaymentsPendentAsync: " + ex.Message);
                return default(T);
            }
        }
        public static async Task<T> GetListMediaInsightsPendingAsync<T>(int dias = 60)
        {
            var _urlFull = _urlBaseApi + "GetListMediaInsightsPendingAsync?dias=" + dias.ToString();
            WriteErrorLog("Url de media:" + _urlFull);
            
            try
            {
                var response = await client.GetAsync(_urlFull);

                if (!response.IsSuccessStatusCode) return default(T);

                var result = await response.Content.ReadAsStringAsync();

                //WriteErrorLog(" GetListMediaInsightsPendingAsync-dias:: " + result);
                return JsonConvert.DeserializeObject<T>(result);
            }
            catch (Exception ex)
            {
                WriteErrorLog("GetListMediaInsightsPendingAsync: " + ex.Message);
                return default(T);
            }
        }

        public static async Task<T> GetListMediaInsightsAsync<T>()
        {
            var _urlFull = _urlBaseApi + "GetListMediaInsightsAsync";
            try
            {
                var response = await client.GetAsync(_urlFull);

                if (!response.IsSuccessStatusCode)
                {
                    return default(T);
                }

                var result = await response.Content.ReadAsStringAsync();

                return JsonConvert.DeserializeObject<T>(result);
            }
            catch (Exception ex)
            {
                WriteErrorLog("GetListMediaInsightsAsync: " + ex.Message);
                return default(T);
            }
        }

        public static async Task<T> GetListarMediasWithEmotionalByAgenciaAsync<T>(string userId)
                                    
        {
            try
            {
                string uri = "ListarMediasWithEmotionalByAgenciaAsync";
                var _request = new Model.Request<string>()
                {
                    Obj = "",
                    userId = userId,
                    usuarioinstagram = "MetricasInsights"
                };

                var myContent = JsonConvert.SerializeObject(_request);

                using (var clientPost = new HttpClient())
                {
                    clientPost.DefaultRequestHeaders.Accept.Clear();
                    clientPost.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    var content = new StringContent(myContent, Encoding.UTF8, "application/json");
                    HttpResponseMessage response = await clientPost.PostAsync(_urlBaseApi + uri, content);

                    if (!response.IsSuccessStatusCode)
                    {
                        return default(T);
                    }

                    var result = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<T>(result);
                }
            }
            catch (Exception ex)
            {
                WriteErrorLog("GetListarMediasWithEmotionalByAgenciaAsync: " + ex.Message);
                return default(T);
            }
        }

        public static async Task<T> GetDataGraphAsync<T>(string accessToken, string uri, string userId, List<string> userIds=null)
        {
            if (userIds==null)
            {
                userIds = new List<string>();
                userIds.Add(userId);
            }

            var conc = uri.Contains("?") ? "&" : "?";
            var _urlFull = $"https://graph.facebook.com/v3.2/{uri}{conc}access_token={accessToken}";
            try
            {
                var response = await client.GetAsync(_urlFull);

                var result = await response.Content.ReadAsStringAsync();
                if (!response.IsSuccessStatusCode)
                {
                    foreach (var userid in userIds)
                    {
                        var erros = JsonConvert.DeserializeObject<Model.ResponseGraph>(result);

                        WriteErrorLog("Erro no Graph & Success:false & uri:" + uri + " & accessToken:" + accessToken + " & URLFull:" + _urlFull + " ==> " + result.ToString());

                        //Verificar se o erro suspende temporario ou definitivamente o processo
                        if (erros != null && (erros.error != null))
                        {
                            await SetUserBloqueioAsync(accessToken, "setUserBloqueio", userid, JsonConvert.SerializeObject(erros.error.message + "#" + erros.error.code.ToString()),
                                false, ((erros.error.code == 190) ? "Bloqueio" : ""));

                            if (erros.error.code == 32) //4800 chamadas/pessoa/24 horas
                            {
                                Scheduler.access_token_suspensos.Add(new Model.Suspensao()
                                {
                                    AccessToken = accessToken,
                                    DtExpirou = DateTime.Now.AddHours(6)
                                }
                               );
                            }

                            if (erros.error.code == 190) //4800 chamadas/pessoa/24 horas
                            {//190 - The session has been invalidated because the user changed their password or Facebook has changed the session for security reasons.
                                Scheduler.access_token_suspensos.Add(new Model.Suspensao()
                                {
                                    AccessToken = accessToken,
                                    DtExpirou = DateTime.Now.AddHours(24 * 7)
                                }
                                );
                            }

                            if (erros.error.code == 100) //Media com erro
                            {
                                Scheduler.media_error.Add(uri);
                            }
                        }
                    }

                    return default(T);
                }


                ////WriteErrorLog("Result:" + result.ToString());
                //if (typeof(T).ToString() == "string")
                //{
                //    return result;
                //}
                return JsonConvert.DeserializeObject<T>(result);
            }
            catch (Exception ex)
            {
                //WriteErrorLog(ex);
                WriteErrorLog("Success:false & uri:" + uri + " & accessToken:" + accessToken
                    + " & URLFull:" + _urlFull);
                return default(T);
            }
        }

        public static async Task<bool> SetEmotionalDiscovery(string _id)
        {
            try
            {
                string uri = "setEmotionalDiscovery";

                var myContent = JsonConvert.SerializeObject(_id);

                using (var clientPost = new HttpClient())
                {
                    clientPost.DefaultRequestHeaders.Accept.Clear();
                    clientPost.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    var content = new StringContent(myContent, Encoding.UTF8, "application/json");
                    HttpResponseMessage response = await clientPost.PostAsync(_urlBaseApi + uri, content);
                    response.EnsureSuccessStatusCode();

                    return response.StatusCode == System.Net.HttpStatusCode.Created;
                }
            }
            catch (Exception ex)
            {
                WriteErrorLog("SetEmotionalDiscovery " + ex.Message);
                return false;
            }
        }

        public static async Task<bool> SetAnottation<T>(List<string> listURIs, string userId)
        {
            try
            {
                string uri = "LoadEmotionProfile";
                var _request = new Model.Request<List<string>>()
                {
                    Obj = listURIs,
                    userId = userId,
                    usuarioinstagram = "MetricasInsights"
                };

                var myContent = JsonConvert.SerializeObject(_request);

                using (var clientPost = new HttpClient())
                {
                    clientPost.DefaultRequestHeaders.Accept.Clear();
                    clientPost.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    var content = new StringContent(myContent, Encoding.UTF8, "application/json");
                    HttpResponseMessage response = await clientPost.PostAsync(_urlBaseApi + uri, content);
                    response.EnsureSuccessStatusCode();

                    return (response.StatusCode == System.Net.HttpStatusCode.Created || response.StatusCode == System.Net.HttpStatusCode.OK);
                }
            }
            catch (Exception ex)
            {
                WriteErrorLog("SetAnottation " + ex.Message);
                return false;
            }
        }

        public static async Task<bool> SetDataGraphAsync<T>(string accessToken, string uri, string userId, string usuarioinstagram, T story)
        {
            try
            {
                var _storyRequest = new Model.Request<T>()
                {
                    Obj = story,
                    userId = userId,
                    usuarioinstagram = usuarioinstagram
                };

                var myContent = JsonConvert.SerializeObject(_storyRequest);

                using (var clientPost = new HttpClient())
                {
                    clientPost.DefaultRequestHeaders.Accept.Clear();
                    clientPost.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    var content = new StringContent(myContent, Encoding.UTF8, "application/json");
                    HttpResponseMessage response = await clientPost.PostAsync(_urlBaseApi + uri, content);
                    response.EnsureSuccessStatusCode();

                    return response.StatusCode == System.Net.HttpStatusCode.Created;
                }
            }
            catch (Exception ex)
            {
                WriteErrorLog("SetDataGraphAsync " + ex.Message);
                return false;
            }
        }

        public static async Task<bool> SetMediaUserAsync(string jsonString, string nomePage, string _newId, string _nameData)
        {
            var url = "https://localhost:44318/facedata/" + "setCorrecoes";

            var _request = new
            {
                json = jsonString,
                namePage = nomePage,
                key = _newId,
                nameData = _nameData
            };

            var myContent = JsonConvert.SerializeObject(_request);

            using (var clientPost = new HttpClient())
            {
                clientPost.DefaultRequestHeaders.Accept.Clear();
                clientPost.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var content = new StringContent(myContent, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await clientPost.PostAsync(url, content);
                response.EnsureSuccessStatusCode();

                return response.StatusCode == System.Net.HttpStatusCode.Created;
            }
        }

        public static async Task<bool> SetMediaInsightAsync<T>(string accessToken, string uri, string userId, string mediaId, int impressions, int reach, int saved, int engagement, string mediaGraphId)
        {
            try
            {
                var _request = new
                {
                    userId = userId,
                    mediaId = mediaId,
                    impressions = impressions,
                    reach = reach,
                    saved = saved,
                    engagement = engagement,
                    mediaGraphId = mediaGraphId
                };

                var myContent = JsonConvert.SerializeObject(_request);

                using (var clientPost = new HttpClient())
                {
                    clientPost.DefaultRequestHeaders.Accept.Clear();
                    clientPost.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    var content = new StringContent(myContent, Encoding.UTF8, "application/json");
                    HttpResponseMessage response = await clientPost.PostAsync(_urlBaseApi + uri, content);
                    response.EnsureSuccessStatusCode();

                    return response.StatusCode == System.Net.HttpStatusCode.Created;
                }
            }
            catch (Exception ex)
            {
                WriteErrorLog("SetMediaInsightAsync " + ex);
                return false;
            }
        }

        public static async Task<bool> SetUserBloqueioAsync(string accessToken, string uri,
                string userId, string description, bool retry, string status)
        {
            try
            {
                var bloqueio = new Model.UserBloqueios
                {
                    AccessToken = accessToken,
                    Description = description,
                    Retry = retry,
                    Status = status
                };

                var _request = new Model.Request<Model.UserBloqueios>()
                {
                    Obj = bloqueio,
                    userId = userId,
                    usuarioinstagram = ""
                };

                var myContent = JsonConvert.SerializeObject(_request);

                using (var clientPost = new HttpClient())
                {
                    clientPost.DefaultRequestHeaders.Accept.Clear();
                    clientPost.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    var content = new StringContent(myContent, Encoding.UTF8, "application/json");
                    HttpResponseMessage response = await clientPost.PostAsync(_urlBaseApi + uri,
                        content);
                    response.EnsureSuccessStatusCode();

                    return response.StatusCode == System.Net.HttpStatusCode.Created;
                }
            }
            catch (Exception ex)
            {
                WriteErrorLog("SetUserBloqueioAsync " + ex.Message);
                return false;
            }
        }

        public static async Task<T> GetCorrecoes<T>(DateTime dt)
        {
            WriteErrorLog("GetCorrecoes: " + dt.ToShortDateString());
            var _urlFull = _urlBaseApi + "setCorrecoes?dt=" + dt.Year.ToString("0000") + "-" + dt.Month.ToString("00") + "-" + dt.Day.ToString("00");
            try
            {
                var response = await client.GetAsync(_urlFull);

                if (!response.IsSuccessStatusCode)
                {
                    return default(T);
                }

                var result = await response.Content.ReadAsStringAsync();

                return JsonConvert.DeserializeObject<T>(result);
            }
            catch (Exception ex)
            {
                WriteErrorLog("GetCorrecoes: " + ex.Message);
                return default(T);
            }
        }
    }
}
