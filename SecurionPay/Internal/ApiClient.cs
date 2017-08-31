﻿using Newtonsoft.Json;
using SecurionPay.Exception;
using SecurionPay.Internal;
using SecurionPay.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SecurionPay
{
    public class ApiClient : IApiClient
    {
        private string _serverUrl = "";
        private string _privateAuthToken;
        private string _version = "2.2.1";
        private HttpClient client;

        public ApiClient(IConfigurationProvider secretKeyProvider)
            : this(secretKeyProvider.GetSecretKey(), secretKeyProvider.GetApiUrl())
        {
        }

        public ApiClient(string secretKey, string serverUrl = "https://api.securionpay.com/", HttpMessageHandler customHttpMessageHandler = null)
        {
            _serverUrl = serverUrl;
            var tokenBytes = Encoding.UTF8.GetBytes(secretKey + ":");
            _privateAuthToken = Convert.ToBase64String(tokenBytes);
            if (customHttpMessageHandler == null)
            {
                client = new HttpClient();
            }
            else
            {
                client = new HttpClient(customHttpMessageHandler);
            }
            client.BaseAddress = new Uri(_serverUrl);
        }

        public async Task<T> SendRequest<T>(HttpMethod method, string action, object parameter)
        {

            HttpRequestMessage request = new HttpRequestMessage(method,action);
            if (parameter != null)
            {
                var requestJson = JsonConvert.SerializeObject(parameter, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
                request.Content = new StringContent(requestJson, Encoding.UTF8, "application/json");
            }

            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", _privateAuthToken);
            client.DefaultRequestHeaders.Add("User-Agent", string.Format("SecurionPay-DOTNET/{0}", _version));
            HttpResponseMessage response = await client.SendAsync(request);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var apiResponseString = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<T>(apiResponseString);
            }
            else
            {
                ErrorResponse errorResponse;
                var apiErrorRsponseString = await response.Content.ReadAsStringAsync();
                errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(apiErrorRsponseString);
                throw new SecurionPayException(errorResponse.Error, typeof(T).Name, action);
            }
        }
    }
}