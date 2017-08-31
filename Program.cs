using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace GetMSIToken
{
    class Program
    {
        const string ApiVersion = "2017-09-01";

        static int Main(string[] args)
        {
            try
            {
                var endpoint = GetEnv("MSI_ENDPOINT");
                var secret = GetEnv("MSI_SECRET");

                // https://management.core.windows.net/ or https://vault.azure.net 
                var resource = new Uri(args.Length == 0 ? "https://management.core.windows.net/" : args[0]);
                Console.WriteLine(GetToken(endpoint, secret, resource.AbsoluteUri, ApiVersion).Result);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return -1;
            }

            return 0;
        }

        static string GetEnv(string name)
        {
            var value = Environment.GetEnvironmentVariable(name);
            if (string.IsNullOrEmpty(value))
            {
                throw new InvalidOperationException("Missing " + name + " env.");
            }

            return value;
        }

        static async Task<string> GetToken(string endpoint, string secret, string resource, string apiVersion)
        {
            var urib = new UriBuilder(endpoint);
            var queryString = new Dictionary<string, string>();
            using (var content = new FormUrlEncodedContent(new[]
            {
                    new KeyValuePair<string, string>("resource", resource),
                    new KeyValuePair<string, string>("api-version", apiVersion)
                }))
            {
                urib.Query = await content.ReadAsStringAsync();
            }

            using (var httpClientHandler = new HttpClientHandler())
            {
                using (var client = new HttpClient(httpClientHandler))
                {
                    client.BaseAddress = new Uri(string.Format(@"{0}://{1}", urib.Scheme, urib.Uri.Authority));

                    var request = new HttpRequestMessage(HttpMethod.Get, urib.Uri.PathAndQuery);
                    request.Headers.Add("Secret", secret);

                    using (var response = await client.SendAsync(request))
                    {
                        response.EnsureSuccessStatusCode();
                        return await response.Content.ReadAsStringAsync();
                    }
                }
            }
        }
    }
}
