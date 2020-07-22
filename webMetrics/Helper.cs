using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using webMetrics.Models;

namespace webMetrics
{
    public class LikedFace
    {
        public static int ConvertFace(string value)
        {
            if (value == "UNKNOWN") { return 0; }
            if (value == "VERY_UNLIKELY") { return 0; }
            if (value == "UNLIKELY") { return 0; }
            if (value == "POSSIBLE") { return 1; }
            if (value == "LIKELY") { return 1; }
            if (value == "VERY_LIKELY") { return 1; }
            return 0;
        }
    }

    public class Helper
    {
        public Helper(IOptions<Models.AppSettings> appSettings)
        {
            _settings = appSettings;
        }

        private readonly IOptions<Models.AppSettings> _settings;
        
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

        public static long ConvertToTimestamp(DateTime value)
        {
            long epoch = (value.Ticks - 621355968000000000) / 10000000;
            return epoch;
        }

        public static List<string> SplitHash(string texto)
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

        public static List<string> SplitMentions(string texto)
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

        public static object CalculoPowerful(List<ContractClass<Models.Graph.InsightsGenderAge>> lstCitiesInsights)
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

        public async Task<int?> ValorCredito(string UserId, Repository.MongoRep repMongo)
        {
            var _credito = await GetCredito(UserId, repMongo);
            if (_credito != null)
            {
                return (_credito.Qtd - _credito.Debito);
            }
            return null;
        }

        private async Task<Models.CreditoMetricas> GetCredito(string UserId, Repository.MongoRep repMongo = null)
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
        public static string ApenasNumeros(string str)
        {
            var apenasDigitos = new Regex(@"[^\d]");
            return apenasDigitos.Replace(str, "");
        }

        public static CultureInfo culturePtBr => new CultureInfo("pt-BR");

        public static string GetUF(string userName)
        {
            var uf = userName.Split(',')[1].ToString();
            if (uf == " São Paulo (state)") uf = "SP";
            if (uf == " Goiás") uf = "GO";
            if (uf == " Maranhão") uf = "MA";
            if (uf == " Rio de Janeiro (state)") uf = "RJ";
            if (uf == " Alagoas") uf = "AL";
            if (uf == " Minas Gerais") uf = "MG";
            if (uf == " Rio Grande do Sul") uf = "RS";
            if (uf == " Mato Grosso do Sul") uf = "MS";
            if (uf == " Paraná") uf = "PR";
            if (uf == " Paraíba") uf = "PB";
            if (uf == " Distrito Especial") uf = "BR";
            if (uf == " Pernambuco") uf = "PE";
            if (uf == " Federal District") uf = "BR";
            if (uf == " Rio Grande do Norte") uf = "RN";
            if (uf == " Bahia") uf = "BA";
            if (uf == " Ceará") uf = "CE";
            if (uf == " Amazonas") uf = "AM";
            if (uf == " Sergipe") uf = "SE";
            if (uf == " Santa Catarina") uf = "SC";
            if (uf == " Mato Grosso") uf = "MT";
            if (uf == " Pará") uf = "PA";
            if (uf == " Acre") uf = "AC";
            if (uf == " Rondônia") uf = "RN";
            if (uf == " Amazonas") uf = "AM";
            if (uf == " Roraima") uf = "RR";
            if (uf == " Tocantins") uf = "TO";
            if (uf == " Piauí") uf = "PI";
            if (uf == " Espírito Santo") uf = "ES";

            return uf;
        }

        public static string GetRegiao(string userName)
        {
            var regiao = "";
            //Goiás, Mato Grosso, Mato Grosso do Sul e o Distrito Federal.
            //Acre, Amazonas, Amapá, Pará, Rondônia, Roraima e Tocantins
            //Alagoas, Bahia, Ceará, Maranhão, Piauí, Pernambuco, Paraíba, Rio Grande do Norte e Sergipe
            //Paraná, Rio Grande do Sul e Santa Catarina
            //Espírito Santo, Minas Gerais, Rio de Janeiro e São Paulo
            var uf = userName.Split(',')[1].ToString();
            if (uf == " Goiás") regiao = "CE";
            if (uf == " Mato Grosso") regiao = "CE";
            if (uf == " Mato Grosso do Sul") regiao = "CE";
            if (uf == " Distrito Especial") regiao = "CE";
            if (uf == " Federal District") regiao = "CE";

            if (uf == " Acre") regiao = "N";
            if (uf == " Pará") regiao = "N";
            if (uf == " Amapá") regiao = "N";
            if (uf == " Roraima") regiao = "N";
            if (uf == " Tocantins") regiao = "N";
            if (uf == " Amazonas") regiao = "N";

            if (uf == " Alagoas") regiao = "NE";
            if (uf == " Bahia") regiao = "NE";
            if (uf == " Ceará") regiao = "NE";
            if (uf == " Maranhão") regiao = "NE";
            if (uf == " Piauí") regiao = "NE";
            if (uf == " Pernambuco") regiao = "NE";
            if (uf == " Paraíba") regiao = "NE";
            if (uf == " Rio Grande do Norte") regiao = "NE";
            if (uf == " Sergipe") regiao = "NE";

            if (uf == " Rio Grande do Sul") regiao = "S";
            if (uf == " Paraná") regiao = "S";
            if (uf == " Santa Catarina") regiao = "S";
            
            if (uf == " São Paulo (state)") regiao = "SE";
            if (uf == " Rio de Janeiro (state)") regiao = "SE";
            if (uf == " Minas Gerais") regiao = "SE";
            if (uf == " Espírito Santo") regiao = "SE";

            return regiao;
        }
    }

    public class SenderEmail
    {
        public static bool Enviar(string _email, string key)
        {
            try
            {
                string CorpoEmail = "Você precisa consultar suas metricas para que a agência consiga avaliar seu engajamento: " +
                    "acesse <a href='https://www.influencersmetrics.com/facedata/?key=" + key + "'> clicando aqui</a>";
                MailMessage mailMessage = new MailMessage
                {
                    From = new MailAddress("sistema@influencersinc.com.br")
                };
                mailMessage.To.Add(_email);
                mailMessage.Subject = "Análise de Engajamento";
                mailMessage.IsBodyHtml = true;
                mailMessage.Body = CorpoEmail.ToString();
                SmtpClient smtpClient = new SmtpClient("mail.wigroup.com.br", 587)
                {
                    Credentials =
                    new NetworkCredential("sistema@influencersinc.com.br", "123#mudar"),

                    EnableSsl = true
                };

                smtpClient.Send(mailMessage);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool ResetSenha(string _email)
        {
            try
            {
                string CorpoEmail = "Sua senha foi alterada com sucesso. <br />" +
                    "acesse https://www.influencersmetrics.com use seu email e a sua nova senha 12influencers3";
                MailMessage mailMessage = new MailMessage
                {
                    From = new MailAddress("sistema@influencersinc.com.br")
                };
                mailMessage.To.Add(_email);
                mailMessage.Subject = "Metrics - Senha resetada";
                mailMessage.IsBodyHtml = true;
                mailMessage.Body = CorpoEmail.ToString();
                SmtpClient smtpClient = new SmtpClient("mail.wigroup.com.br", 587)
                {
                    Credentials =
                    new NetworkCredential("sistema@influencersinc.com.br", "123#mudar"),

                    EnableSsl = true
                };

                smtpClient.Send(mailMessage);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool BemVindo(string _email, string id)
        {
            try
            {
                string _corpoEmail = string.Format(CorpoEmail.BEMVINDO, id);
                MailMessage mailMessage = new MailMessage
                {
                    From = new MailAddress("sistema@influencersinc.com.br")
                };
                mailMessage.To.Add(_email);
                mailMessage.Bcc.Add("daniel.romualdo@gmail.com");//TODO:
                mailMessage.Subject = "Seja bem vindo - InfluencersMetrics";
                mailMessage.IsBodyHtml = true;
                mailMessage.Body = _corpoEmail.ToString();
                SmtpClient smtpClient = new SmtpClient("mail.wigroup.com.br", 587)
                {
                    Credentials =
                    new NetworkCredential("sistema@influencersinc.com.br", "123#mudar"),

                    EnableSsl = true
                };

                smtpClient.Send(mailMessage);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool Pagamento(string _email, string id)
        {
            try
            {
                string _corpoEmail = string.Format(CorpoEmail.PAGAMENTO, id);
                MailMessage mailMessage = new MailMessage
                {
                    From = new MailAddress("sistema@influencersinc.com.br")
                };
                mailMessage.To.Add(_email);
                mailMessage.Bcc.Add("daniel.romualdo@gmail.com");//TODO:
                mailMessage.Subject = "Pagamento Efetuado - InfluencersMetrics";
                mailMessage.IsBodyHtml = true;
                mailMessage.Body = _corpoEmail.ToString();
                SmtpClient smtpClient = new SmtpClient("mail.wigroup.com.br", 587)
                {
                    Credentials =
                    new NetworkCredential("sistema@influencersinc.com.br", "123#mudar"),

                    EnableSsl = true
                };

                smtpClient.Send(mailMessage);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }

    public static class ObjectExtensions
    {
        public static IDictionary<string, string> ToKeyValue(this object metaToken)
        {
            if (metaToken == null)
            {
                return null;
            }

            JToken token = metaToken as JToken;
            if (token == null)
            {
                return ToKeyValue(JObject.FromObject(metaToken));
            }

            if (token.HasValues)
            {
                var contentData = new Dictionary<string, string>();
                foreach (var child in token.Children().ToList())
                {
                    var childContent = child.ToKeyValue();
                    if (childContent != null)
                    {
                        contentData = contentData.Concat(childContent)
                            .ToDictionary(k => k.Key, v => v.Value);
                    }
                }

                return contentData;
            }

            var jValue = token as JValue;
            if (jValue?.Value == null)
            {
                return null;
            }

            var value = jValue?.Type == JTokenType.Date ?
                jValue?.ToString("o", CultureInfo.InvariantCulture) :
                jValue?.ToString(CultureInfo.InvariantCulture);

            return new Dictionary<string, string> { { token.Path, value } };
        }
    }

    public class CorpoEmail
    {
        public static string BEMVINDO = "" +
            "<html>                                                                            " +
            "<head>                                                                            " +
            "<meta http-equiv='content-type' content='text/html; charset=UTF-8'>               " +
            "<title>email-bem-vindo</title>                                                    " +
            "</head>                                                                           " +
            "<body link='#0000EE' vlink='#551A8B' text='#000000' bgcolor='#cccccc'             " +
            "alink='#EE0000'>                                                                  " +
            "<div align='center'>                                                              " +
            "<table width='608' height='463' cellspacing='2' cellpadding='2'                   " +
            "bgcolor='#ffffff' border='0'>                                                     " +
            "<tbody>                                                                           " +
            "<tr>                                                                              " +
            "<td valign='top'><br>                                                             " +
            "<blockquote> <img                                                                 " +
            "src='https://www.influencersmetrics.com/resourceshome/assets/images/logo3.png'    " +
            "alt='InfluencersMetrics' width='218' height='56'><br>                             " +
            "<font face='Verdana'> </font> </blockquote>                                       " +
            "<hr width='100%' size='2'><font face='Verdana'> </font>                           " +
            "<div align='center'>                                                              " +
            "<blockquote>                                                                      " +
            "<div align='left'><font face='Verdana' size='-1'>Bem                              " +
            "vindo a <b>INFLUENCERS METRICS</b>, somos uma                                     " +
            "empresa do grupo</font><font size='-1'><br>                                       " +
            "<font face='Verdana'> INFLUENCERS INC. A partir de                                " +
            "agora, você entenderá números reais e irreais,</font><br>                         " +
            "<font face='Verdana'> todos os resultados que                                     " +
            "poderá atingir seja um influenciador, agência ou                                  " +
            "</font><br>                                                                       " +
            "<font face='Verdana'> marca.</font></font><br>                                    " +
            "</div>                                                                            " +
            "</blockquote>                                                                     " +
            "<font face='Verdana'><br>                                                         " +
            "</font>                                                                           " +
            "<a href='https://www.influencersmetrics.com/relatorios/login?id={0}'              " +
            "style='text-decoration:none;color=#ffffff'>                                       " +
            "<table width='60%' cellspacing='2' cellpadding='2'                                " +
            "align='center' border='0'>                                                        " +
            "<tbody>                                                                           " +
            "<tr>                                                                              " +
            "<td valign='top' bgcolor='#3366ff'                                                " +
            "align='center'><b><font color='#ffffff'>                                          " +
            "ACESSE A INFLUENCERS METRICS AQUI! </font></b></td>                               " +
            "</tr>                                                                             " +
            "</tbody>                                                                          " +
            "</table>                                                                          " +
            "</a> <font face='Verdana'><br>                                                    " +
            "</font> </div>                                                                    " +
            "<font face='Verdana'> <br>                                                        " +
            "</font>                                                                           " +
            "<hr width='100%' size='2'><font face='Verdana'><br>                               " +
            "<br>                                                                              " +
            "<br>                                                                              " +
            "</font>                                                                           " +
            "<div align='center'><font face='Verdana'><font size='-2'>Essa                     " +
            "                                                                                  " +
            "                                                                                  " +
            "é uma mensagem gerada automaticamente, portanto, não                              " +
            "deve ser respondida.</font><br>                                                   " +
            "<font size='-2'>ALL RIGHTS RESERVED INFLUENCERS                                   " +
            "METRICS © 2019&nbsp; - Termos e Privacidade</font></font><br>                     " +
            "</div>                                                                            " +
            "</td>                                                                             " +
            "</tr>                                                                             " +
            "</tbody>                                                                          " +
            "</table>                                                                          " +
            "<br>                                                                              " +
            "</div>                                                                            " +
            "</body>                                                                           " +
            "</html>																			";

        public static string PAGAMENTO = "" +
            "<html>                                                                            " +
            "<head>                                                                            " +
            "<meta http-equiv='content-type' content='text/html; charset=UTF-8'>               " +
            "<title>email-bem-vindo</title>                                                    " +
            "</head>                                                                           " +
            "<body link='#0000EE' vlink='#551A8B' text='#000000' bgcolor='#cccccc'             " +
            "alink='#EE0000'>                                                                  " +
            "<div align='center'>                                                              " +
            "<table width='608' height='463' cellspacing='2' cellpadding='2'                   " +
            "bgcolor='#ffffff' border='0'>                                                     " +
            "<tbody>                                                                           " +
            "<tr>                                                                              " +
            "<td valign='top'><br>                                                             " +
            "<blockquote> <img                                                                 " +
            "src='https://www.influencersmetrics.com/resourceshome/assets/images/logo3.png'    " +
            "alt='InfluencersMetrics' width='218' height='56'><br>                             " +
            "<font face='Verdana'> </font> </blockquote>                                       " +
            "<hr width='100%' size='2'><font face='Verdana'> </font>                           " +
            "<div align='center'>                                                              " +
            "<blockquote>                                                                      " +
            "   <div align='left'>                                                             " +
            "       <font face='Verdana' size='-1'><b>Seu pagamento foi recebido com sucesso,</b> a partir de agora você poderá </font><font size='-1'><br>      " +
            "       <font face='Verdana' size='-1'>consultar sua análise e aprender como crescer a partir de erros e acertos, ou </font><font size='-1'><br>      " +
            "       <font face='Verdana' size='-1'>acompanhar as análises dos influenciadores que necessita para campanhas </font><font size='-1'><br>      " +
            "       <font face='Verdana' size='-1'>assim como mensurar resultados pré e pós campanhas. </font><font size='-1'><br>      " +
            "       <p></p>" +
            "       <font face='Verdana' size='-1'>Obrigado por se tornar um membro <b>INFLUENCERS METRICS.</b> </font><font size='-1'><br>      " +

            "   </div>                                                                         " +
            "</blockquote>                                                                     " +
            "<font face='Verdana'><br>                                                         " +
            "</font>                                                                           " +
            "<a href='https://www.influencersmetrics.com/relatorios/login?id={0}'              " +
            "style='text-decoration:none;color=#ffffff'>                                       " +
            "<table width='60%' cellspacing='2' cellpadding='2'                                " +
            "align='center' border='0'>                                                        " +
            "<tbody>                                                                           " +
            "<tr>                                                                              " +
            "<td valign='top' bgcolor='#3366ff'                                                " +
            "align='center'><b><font color='#ffffff'>                                          " +
            "CLIQUE AQUI PARA ENTRAR EM SEU PERFIL</font></b></td>                             " +
            "</tr>                                                                             " +
            "</tbody>                                                                          " +
            "</table>                                                                          " +
            "</a> <font face='Verdana'><br>                                                    " +
            "</font> </div>                                                                    " +
            "<font face='Verdana'> <br>                                                        " +
            "</font>                                                                           " +
            "<hr width='100%' size='2'><font face='Verdana'><br>                               " +
            "<br>                                                                              " +
            "<br>                                                                              " +
            "</font>                                                                           " +
            "<div align='center'><font face='Verdana'><font size='-2'>Essa                     " +
            "                                                                                  " +
            "                                                                                  " +
            "é uma mensagem gerada automaticamente, portanto, não                              " +
            "deve ser respondida.</font><br>                                                   " +
            "<font size='-2'>ALL RIGHTS RESERVED INFLUENCERS                                   " +
            "METRICS © 2019&nbsp; - Termos e Privacidade</font></font><br>                     " +
            "</div>                                                                            " +
            "</td>                                                                             " +
            "</tr>                                                                             " +
            "</tbody>                                                                          " +
            "</table>                                                                          " +
            "<br>                                                                              " +
            "</div>                                                                            " +
            "</body>                                                                           " +
            "</html>																			";
    }
}
 



