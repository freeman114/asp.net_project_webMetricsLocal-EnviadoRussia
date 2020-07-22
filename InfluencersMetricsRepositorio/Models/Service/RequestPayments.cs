using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfluencersMetricsService.Model
{
    public class RequestPayments
    {
        public string userId { get; set; }
        public string PaymentId { get; set; }
        public string StatusPagamentoAtual { get; set; }
        public string StatusPagamentoNovo { get; set; }
        public DateTime DataPagamento { get; set; }

    }
}
