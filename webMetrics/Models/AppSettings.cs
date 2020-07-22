namespace webMetrics.Models
{
    public class AppSettings
    {
        public string TokenPagSeguro { get; set; }
        public string EmailPagSeguro { get; set; }
        public string UrlInstagram { get; set; }
        public string UrlPagSeguro { get; set; }
        public string UsuarioInstagram { get; set; }
        public string SenhaInstagram { get; set; }
        public string ConexaoMongoDB { get; set; }
        public string ConexaoMongoDBCrowler { get; set; }
        public string DataBaseNameCrowler { get; set; }
        public string TokenMOIP { get; set; }
        public string ChaveMOIP { get; set; }
    }
}