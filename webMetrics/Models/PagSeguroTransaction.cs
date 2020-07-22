namespace webMetrics.Models
{

    public class PagSeguroTransaction
    {
        public string paymentMode { get; set; }
        public string paymentMethod { get; set; }
        public string receiverEmail { get; set; }
        public string currency { get; set; }
        public string extraAmount { get; set; }
        public string itemId1 { get; set; }
        public string itemDescription1 { get; set; }
        public string itemAmount1 { get; set; }
        public string itemQuantity1 { get; set; }
        public string notificationURL { get; set; }
        public string reference { get; set; }
        public string senderName { get; set; }
        public string senderCPF { get; set; }
        public string senderAreaCode { get; set; }
        public string senderPhone { get; set; }
        public string senderEmail { get; set; }
        public string senderHash { get; set; }
        public string shippingAddressStreet { get; set; }
        public string shippingAddressNumber { get; set; }
        public string shippingAddressComplement { get; set; }
        public string shippingAddressDistrict { get; set; }
        public string shippingAddressPostalCode { get; set; }
        public string shippingAddressCity { get; set; }
        public string shippingAddressState { get; set; }
        public string shippingAddressCountry { get; set; }
        public string shippingType { get; set; }
        public string shippingCost { get; set; }
        public string creditCardToken { get; set; }
        public string installmentQuantity { get; set; }
        public string installmentValue { get; set; }
        public string noInterestInstallmentQuantity { get; set; }
        public string creditCardHolderName { get; set; }
        public string creditCardHolderCPF { get; set; }
        public string creditCardHolderBirthDate { get; set; }
        public string creditCardHolderAreaCode { get; set; }
        public string creditCardHolderPhone { get; set; }
        public string billingAddressStreet { get; set; }
        public string billingAddressNumber { get; set; }
        public string billingAddressComplement { get; set; }
        public string billingAddressDistrict { get; set; }
        public string billingAddressPostalCode { get; set; }
        public string billingAddressCity { get; set; }
        public string billingAddressState { get; set; }
        public string billingAddressCountry { get; set; }

        public string token { get; set; }
        public string email { get; set; }

        public string Code { get; set; }
    }
}