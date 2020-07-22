namespace webMetrics.Models
{
    public class UsuarioData
    {
        public class Counts
        {
            public int media { get; set; }
            public int follows { get; set; }
            public int followed_by { get; set; }
        }

        public class Data
        {
            public string id { get; set; }
            public string username { get; set; }
            public string full_name { get; set; }
            public string profile_picture { get; set; }
            public string bio { get; set; }
            public string website { get; set; }
            public bool is_business { get; set; }
            public Counts counts { get; set; }
        }

        public Data data { get; set; }

    }
}