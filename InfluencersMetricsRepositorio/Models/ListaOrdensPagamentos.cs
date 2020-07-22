using System;
using System.Collections.Generic;

namespace webMetrics.Models
{
    public class Transaction
    {
        public string code { get; set; }
        public DateTime date { get; set; }
        public int status { get; set; }
    }

    public class Discount
    {
        public string type { get; set; }
        public int value { get; set; }
    }

    public class orderItem
    {
        public string code { get; set; }
        public int status { get; set; }
        public decimal amount { get; set; }
        public decimal grossAmount { get; set; }
        public DateTime lastEventDate { get; set; }
        public DateTime schedulingDate { get; set; }
        public List<Transaction> transactions { get; set; }
        public Discount discount { get; set; }
    }

    public class PaymentOrders : Dictionary<string, orderItem>
    {

    }

    public class ListaOrdensPagamentos
    {
        public DateTime date { get; set; }
        public int resultsInThisPage { get; set; }
        public int currentPage { get; set; }
        public int totalPages { get; set; }
        public PaymentOrders paymentOrders { get; set; }
    }
}