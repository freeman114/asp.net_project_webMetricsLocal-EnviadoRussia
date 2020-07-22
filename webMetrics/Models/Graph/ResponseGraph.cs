namespace webMetrics.Models.Graph
{
    public class ResponseGraph
    {

        public Error error { get; set; }
    }

    public class Error
    {
        public string message { get; set; }
        public string type { get; set; }
        public int code { get; set; }
        public string fbtrace_id { get; set; }

        public string error_subcode { get; set; }
        public bool is_transient { get; set; }
        public string error_user_title { get; set; }
        public string error_user_msg { get; set; }
    }
}