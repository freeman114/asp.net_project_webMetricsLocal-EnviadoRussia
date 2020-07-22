using System;
using System.Collections.Generic;

namespace webMetrics.Models.DTO
{
    public class CreditoAgencia
    {
        public string AgenciaUserId { get; set; }
        public string UserId { get; set; }
        public DateTime DataCredito { get; set; }
        public DateTime DataExpiracao { get; set; }

    }
}