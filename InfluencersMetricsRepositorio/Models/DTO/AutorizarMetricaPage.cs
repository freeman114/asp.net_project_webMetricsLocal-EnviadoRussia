using System.Collections.Generic;

namespace webMetrics.Models.DTO
{
    public class AutorizarMetricaPage
    {
        public Models.AutorizacaoMetrica autorizacaoMetrica { get; set; }
        public IEnumerable<webMetrics.Models.AutorizacaoMetrica> autorizacaoMetricas { get; set; }
        public string UsuariosInstagram { get; set; }
        public string Client { get; set; }
        public IEnumerable<webMetrics.Models.AutorizacaoMetrica> Months { get; set; }
    }
}