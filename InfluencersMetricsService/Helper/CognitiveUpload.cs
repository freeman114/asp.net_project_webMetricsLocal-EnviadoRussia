using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace InfluencersMetricsService.Helper
{
    public class CognitiveUpload
    {
        public CognitiveUpload()
        {
            MakeRequest();
            Console.WriteLine("Hit ENTER to exit...");
            Console.ReadLine();
        }

        static async void MakeRequest()
        {
            //var client = new HttpClient();

            //var parametersToAdd = new System.Collections.Generic.Dictionary<string, string> { { "resource", "foo" } };
            //var someUrl = "http://www.google.com";
            ////var newUri = Microsoft.AspNetCore.WebUtilities.QueryHelpers.AddQueryString(someUrl, parametersToAdd);

            //var queryString = HttpUtility.ParseQueryString(string.Empty);

            //// Request headers
            //client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", "{subscription key}");

            //// Request parameters
            //queryString["visualFeatures"] = "Categories";
            //queryString["details"] = "{string}";
            //queryString["language"] = "en";
            //var uri = "https://westus.api.cognitive.microsoft.com/vision/v2.0/analyze?" + queryString;

            //HttpResponseMessage response;

            //// Request body
            //byte[] byteData = Encoding.UTF8.GetBytes("{body}");

            //using (var content = new ByteArrayContent(byteData))
            //{
            //    content.Headers.ContentType = new MediaTypeHeaderValue("< your content type, i.e. application/json >");
            //    response = await client.PostAsync(uri, content);
            //}

        }
    }
}