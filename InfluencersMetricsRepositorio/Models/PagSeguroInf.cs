using System.Collections.Generic;

namespace webMetrics.Models
{
    public class PagSeguroInf
    {
        public class Phone
        {
            public string areaCode { get; set; }
            public string number { get; set; }
        }

        public class Address
        {
            public string street { get; set; }
            public string number { get; set; }
            public string complement { get; set; }
            public string district { get; set; }
            public string city { get; set; }
            public string state { get; set; }
            public string country { get; set; }
            public string postalCode { get; set; }
        }

        public class Document
        {
            public string type { get; set; }
            public string value { get; set; }
        }

        public class Sender
        {
            public string name { get; set; }
            public string email { get; set; }
            public string hash { get; set; }
            public Phone phone { get; set; }
            public Address address { get; set; }
            public List<Document> documents { get; set; }
        }
        
        public class BillingAddress
        {
            public string street { get; set; }
            public string number { get; set; }
            public string complement { get; set; }
            public string district { get; set; }
            public string city { get; set; }
            public string state { get; set; }
            public string country { get; set; }
            public string postalCode { get; set; }
        }

        public class Holder
        {
            public string name { get; set; }
            public string birthDate { get; set; }
            public List<Document> documents { get; set; }
            public Phone phone { get; set; }
            public BillingAddress billingAddress { get; set; }
        }

        public class CreditCard
        {
            public string token { get; set; }
            public Holder holder { get; set; }
        }

        public class PaymentMethod
        {
            public string type { get; set; }
            public CreditCard creditCard { get; set; }
        }

        public class PagamentoPreAprovadoDTO
        {
            public string plan { get; set; }
            public string reference { get; set; }
            public Sender sender { get; set; }
            public PaymentMethod paymentMethod { get; set; }
        }
    }
}