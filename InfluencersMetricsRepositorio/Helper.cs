using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Mail;

namespace influencersMetrics
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
            catch (Exception ex)
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

