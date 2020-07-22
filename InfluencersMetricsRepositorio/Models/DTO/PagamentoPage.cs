using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using Wirecard.Models;

namespace webMetrics.Models.DTO
{
    public class PagamentoPage
    {
        public ObjectId _id { get; set; }
        public Usuario Usuario { get; set; }

        public decimal Valor { get; set; }
        public decimal Total { get; set; }
        public int Quantidade { get; set; }
        public string chave { get; set; }

        public string codPlan { get; set; }

        public string OrderId { get; set; }

        public CustomerResponse customerResponse { get; set; }
        public OrderResponse orderResponse { get; set; }
        public PaymentResponse paymentResponse { get; set; }

        public SubscriptionResponse subscriptionResponse { get; set; }

        public string StatusPagamento { get; set; }

        public string expirationMonth { get; set; }
        public string expirationYear { get; set; }
        public string cardNumber { get; set; }

        public List<Invoice> Invoices { get; set; }

        public DateTime NextInvoice { get; set; }
    }
}