using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Serialization;
using webMetrics.Models;

namespace webMetrics.Business
{
    public class ApiClient
    {
        readonly RestClient client = null;
        string url = "https://ws.sandbox.pagseguro.uol.com.br/";
        public string Token { get; set; }
        public string Email { get; set; }
        public string redirectURI { get; set; }

        private readonly IOptions<Models.AppSettings> _appSettings;
        public ApiClient(IOptions<Models.AppSettings> appSettings)
        {
            _appSettings = appSettings;
            redirectURI = "https://www.influencersmetrics.com/relatorios/authorize/";
        }

        private RestRequest JsonToUrl<T>(T obj, Method method, bool hasBody)
        {
            string jsonValues = JsonConvert.SerializeObject(obj);
            var _req = new RestRequest(method);
            _req.AddHeader("Accept", "application/vnd.pagseguro.com.br.v3+json;charset=ISO-8859-1");
            _req.AddHeader("Content-Type", "application/json;charset=ISO-8859-1");
            if (hasBody) _req.AddParameter("application/json", jsonValues, ParameterType.RequestBody);
            return _req;
        }

        private RestRequest JsonToUrlEncoded<T>(T obj, Method method)
        {
            var keyValueContent = obj.ToKeyValue();
            var formUrlEncodedContent = new FormUrlEncodedContent(keyValueContent);
            var urlEncodedString = formUrlEncodedContent.ReadAsStringAsync();

            var request = new RestRequest(Method.POST);
            request.AddHeader("Accept", "application/vnd.pagseguro.com.br.v3+json;charset=ISO-8859-1");
            request.AddHeader("Content-Type", "application/x-www-form-urlencoded;charset=ISO-8859-1");
            request.AddParameter("application/x-www-form-urlencoded", formUrlEncodedContent);

            return request;
        }

        public async Task<string> Transactions(Models.PagSeguroTransaction dto)
        {
            string URLConstant = "v2/transactions"; //?email=" + Email + "&token=" + Token;
            try
            {
                dto.email = Email;
                dto.token = Token;

                var client = new RestClient(url + URLConstant); //"https://ws.sandbox.pagseguro.uol.com.br/v2/transactions");
                var request = new RestRequest(Method.POST);

                var keyValueContent = dto.ToKeyValue();
                var formUrlEncodedContent = new FormUrlEncodedContent(keyValueContent);
                var urlEncodedString = await formUrlEncodedContent.ReadAsStringAsync();

                request.AddParameter("application/x-www-form-urlencoded; charset=ISO-8859-1", urlEncodedString, ParameterType.RequestBody);
                request.AddHeader("Accept", "application/xml;charset=ISO-8859-1");
                request.AddHeader("content-type", "application/x-www-form-urlencoded; charset=ISO-8859-1");
                IRestResponse response = client.Execute(request);

                if (response.ResponseStatus == ResponseStatus.Completed)
                {
                    Repository.MongoRep rep = new Repository.MongoRep(dto.senderEmail, _appSettings);
                    await rep.GravarOne<string>(response.Content);

                    XmlSerializer serializer = new XmlSerializer(typeof(webMetrics.Models.TransactionPagseguro.Transaction));
                    using (TextReader reader = new StringReader(response.Content))
                    {
                        var rest = (webMetrics.Models.TransactionPagseguro.Transaction)serializer.Deserialize(reader);
                        //if (rest != null)
                        //{
                        //    throw new Exception(rest.Error.Message.ToString());
                        //}

                        return rest.Code;
                    }

                    return "";
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return null;
        }

        public async Task<ListaOrdensPagamentos> ConsultaOrdensPagamentos(string codePreApproval)
        {
            if (string.IsNullOrEmpty(codePreApproval))
            {
                return null;
            }

            string URLConstant = "pre-approvals/" + codePreApproval + "/payment-orders?email=" + Email + "&token=" + Token;
            try
            {
                var client = new RestClient(url + URLConstant);
                var _req = JsonToUrl<string>(codePreApproval, Method.GET, false);
                IRestResponse response = client.Execute(_req);

                if (response.ResponseStatus == ResponseStatus.Completed)
                {
                    var rest = JsonConvert.DeserializeObject<ListaOrdensPagamentos>(response.Content);
                    return rest;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return null;
        }
        
        public async Task<Models.AccessToken> GetAccessToken(string _code)
        {
            url = "https://api.instagram.com/oauth/";
            string URLConstant = "";
            try
            {
                var client = new RestClient(url + URLConstant);
                var obj = new
                {
                    client_id = "2143b50835c049f0baedb36d52090c46",
                    client_secret = "14d2987940e342278bd4c37c8f1ef8c0",
                    grant_type = "authorization_code",
                    redirect_uri = redirectURI,
                    code = _code
                };
                
                var json = JsonConvert.SerializeObject(obj);
                var restclient = new RestClient(url);
                RestRequest request = new RestRequest("access_token") { Method = Method.POST };
                request.AddHeader("Accept", "application/json");
                request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
                request.AddParameter("client_id", obj.client_id);
                request.AddParameter("client_secret", obj.client_secret);
                request.AddParameter("grant_type", obj.grant_type);
                request.AddParameter("redirect_uri", obj.redirect_uri);
                request.AddParameter("code", obj.code);

                var tResponse = restclient.Execute(request);
                var result = tResponse.Content;
                var retorno = JsonConvert.DeserializeObject<Models.AccessToken>(result);
                access_token = retorno.access_token;

                return retorno;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public string access_token { get; set; }
        public async Task<MediaRecent> GetInsta(string _code)
        {
            //url = "https://api.instagram.com/v1/users/self/?access_token=" + access_token;
            url = "https://api.instagram.com/v1/users/self/media/recent/?count=40&access_token=" + access_token;


            try
            {
                var client = new RestClient(url);
                var _req = JsonToUrl<string>(_code, Method.GET, false);
                IRestResponse response = client.Execute(_req);

                if (response.ResponseStatus == ResponseStatus.Completed)
                {
                    var result = response.Content;
                    var rest = JsonConvert.DeserializeObject<MediaRecent>(response.Content);
                    return rest;// result;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return null;
        }
        public async Task<UsuarioData> GetUsuarioData(string _code)
        {
            url = "https://api.instagram.com/v1/users/self/?access_token=" + access_token;
            
            try
            {
                var client = new RestClient(url);
                var _req = JsonToUrl<string>(_code, Method.GET, false);
                IRestResponse response = client.Execute(_req);

                if (response.ResponseStatus == ResponseStatus.Completed)
                {
                    var result = response.Content;
                    var rest = JsonConvert.DeserializeObject<UsuarioData>(response.Content);
                    return rest;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return null;
        }
    }
}
