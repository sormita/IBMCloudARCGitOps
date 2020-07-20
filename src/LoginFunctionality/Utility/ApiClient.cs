﻿using LoginFunctionality.Utility.UtilityModel;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace LoginFunctionality.Utility
{
    public class ApiClient: IApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly IOptions<SettingsModel> appSettings;
        private ILogger _logger;
        private Uri BaseEndpoint { get; set; }

        private static JsonSerializerSettings MicrosoftDateFormatSettings
        {
            get
            {
                return new JsonSerializerSettings
                {
                    DateFormatHandling = DateFormatHandling.MicrosoftDateFormat
                };
            }
        }

        public ApiClient(IOptions<SettingsModel> app, ILogger<ApiClient> logger)
        {
            _logger = logger;
            appSettings = app;
           
                IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

                var baseEndpoint = configuration["WebApiBaseUrl"];
            _logger.LogInformation(baseEndpoint);
                if (baseEndpoint == null)
                {
                    throw new ArgumentNullException("baseEndpoint");
                }
                BaseEndpoint = new Uri(baseEndpoint);
                _httpClient = new HttpClient();
            
            
        }

        /// <summary>  
        /// Common method for making GET calls  
        /// </summary>  
        public async Task<T> GetAsync<T>(string requestUrl)
        {

            //addHeaders();
            _logger.LogInformation(BaseEndpoint.ToString());
            var response = await _httpClient.GetAsync(BaseEndpoint+requestUrl, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();
            var data = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(data);
        }

        /// <summary>  
        /// Common method for making GET calls that return list objects
        /// </summary>  
        public async Task<List<T>> GetListAsync<T>(string requestUrl)
        {

            //addHeaders();
            _logger.LogInformation(BaseEndpoint.ToString());
            var response = await _httpClient.GetAsync(BaseEndpoint + requestUrl, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();
            var data = await response.Content.ReadAsStringAsync();
            //return data.ToString();
            return JsonConvert.DeserializeObject<List<T>>(data);
        }

        /// <summary>  
        /// Common method for making POST calls  
        /// </summary>  
        public async Task<T> PostAsync<T>(string requestUrl, T content)
        {
            //addHeaders();
            var response = await _httpClient.PostAsync(BaseEndpoint + requestUrl, CreateHttpContent<T>(content));
            response.EnsureSuccessStatusCode();
            var data = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(data);
        }

        /// <summary>  
        /// Common method for making POST calls  
        /// </summary>  
        public async Task<List<T>> PostAsyncGraphQL<T>(string requestUrl, StringContent stringContent)
        {
            //addHeaders();
            var response = await _httpClient.PostAsync(BaseEndpoint + requestUrl, stringContent);
            response.EnsureSuccessStatusCode();
            var data = await response.Content.ReadAsStringAsync();
            var data_actual = data.Substring(data.LastIndexOf('[')).Trim('}');            
            return JsonConvert.DeserializeObject<List<T>>(data_actual);
        }

        private HttpContent CreateHttpContent<T>(T content)
        {
            var json = JsonConvert.SerializeObject(content, MicrosoftDateFormatSettings);
            return new StringContent(json, Encoding.UTF8, "application/json");
        }
    }
}
