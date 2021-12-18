﻿using Newtonsoft.Json.Linq;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace NearClient.Utilities
{
    public static class Web
    {
        public static async Task<dynamic> FetchJsonAsync(string url, string json = "", WebProxy webProxy = null)
        {
            var header = new HttpClientHandler();
            header.Proxy = webProxy;
            using (var client = new HttpClient(header))
            {
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.Timeout = TimeSpan.FromSeconds(100);
                HttpResponseMessage response;

                if (!string.IsNullOrEmpty(json))
                {
                    HttpContent content = new StringContent(json, Encoding.UTF8, "application/json");
                    response = await client.PostAsync(url, content);
                }
                else
                {
                    response = await client.GetAsync(url);
                }

                if (response.IsSuccessStatusCode)
                {
                    string jsonString = await response.Content.ReadAsStringAsync();

                    dynamic rawResult = JObject.Parse(jsonString);
                    if (rawResult.error != null && rawResult.error.data != null)
                    {
                        throw new Exception($"[{rawResult.error.code}]: {rawResult.error.name}: {rawResult.error.data}");
                    }
                    return rawResult.result;
                }
                else
                {
                    throw new HttpException((int)response.StatusCode, response.Content.ToString());
                }
            }
        }

        public static async Task<dynamic> FetchJsonAsync(ConnectionInfo connection, string json = "")
        {
            var url = connection.Url;
            var result = await FetchJsonAsync(url, json, connection.WebProxy);
            return result;
        }
    }
}