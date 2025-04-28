using MacRegister.Model.Jems4Api;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace MacRegister.InfraStructure
{
    public class Jems4Api
    {

        private readonly HttpClient _httpClient;
        private readonly Constants _constants;

        public Jems4Api(Constants constants)
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri(Constants.BaseUrlJems4);
            _constants = constants;
        }
        public async Task<string> AdSignin(string username, string password)
        {
            username = $"JABIL\\{username}";
            var formData = new MultipartFormDataContent();
            formData.Add(new StringContent(username), "name");
            formData.Add(new StringContent(password), "password");

            try
            {
                var response = await _httpClient.PostAsync($"{Constants.BaseUrlJems4}api/user/adsignin", formData);
                response.EnsureSuccessStatusCode();

                var responseBody = await response.Content.ReadAsStringAsync();
                var tokenPrefix = "UserToken=";
                var tokenStartIndex = responseBody.IndexOf(tokenPrefix, StringComparison.Ordinal);
                if (tokenStartIndex >= 0)
                {
                    var tokenEndIndex = responseBody.IndexOf(';', tokenStartIndex);
                    var token = responseBody.Substring(tokenStartIndex + tokenPrefix.Length, tokenEndIndex - tokenStartIndex - tokenPrefix.Length);
                    _constants.TokenApi = token;
                    return token;
                }

                throw new Exception("Token not found in the response");
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine($"Request error: {e.Message}");
                throw;
            }
        }
        public async Task<List<GetWipInformationBySerialNumberResponse>> GetWipInformationBySerialNumber
        (
           string siteName,
           string customerName,
           string assembly,
            string serialNumber
        )
        {
            try
            {
                var baseAddress = Constants.BaseUrlJems4;
                var token = _constants.TokenApi;

                var cookieContainer = new CookieContainer();
                cookieContainer.Add(new Uri(baseAddress), new Cookie("UserToken", token));

                using (var handler = new HttpClientHandler() { CookieContainer = cookieContainer, UseCookies = true })
                using (var client = new HttpClient(handler) { BaseAddress = new Uri(baseAddress) })
                {
                    client.DefaultRequestHeaders.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/plain"));

                    var requestUri = $"api/Wips/GetWipInformationBySerialNumber" +
                                     $"?SiteName={Uri.EscapeDataString(siteName)}" +
                                     $"&CustomerName={Uri.EscapeDataString(customerName)}" +
                                     $"&MaterialName={Uri.EscapeDataString(assembly)}" +
                                     $"&SerialNumber={Uri.EscapeDataString(serialNumber)}";

                    var response = await client.GetAsync(requestUri);

                    if (response.IsSuccessStatusCode)
                    {
                        var responseContent = await response.Content.ReadAsStringAsync();
                        var result = JsonConvert.DeserializeObject<List<GetWipInformationBySerialNumberResponse>>(responseContent); ;


                        return result;
                    }
                    else
                    {
                        Console.WriteLine($"Request failed with status code: {response.StatusCode}");
                        return null;
                    }
                }
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine($"Request error: {e.Message}");
                throw;
            }
        }

        public async Task<OkToStartResponse> OkToStart(int wipId, string resourceName)
        {
            try
            {
                var requestUri = $"{Constants.BaseUrlJems4}api/Wips/{wipId}/oktostart" +
                                 $"?resourceName={Uri.EscapeDataString(resourceName)}";

                // Create the request
                var request = new HttpRequestMessage(HttpMethod.Get, requestUri);

                // Add the cookie header with the token
                request.Headers.Add("Cookie", $"UserToken={_constants.TokenApi}");

                // Send the request
                var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                // Read and deserialize the response content
                var responseContent = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<OkToStartResponse>(responseContent);

                return result;
            }
            catch (HttpRequestException e)
            {
                // Log and rethrow the exception
                Console.WriteLine($"Request error: {e.Message}");
                throw;
            }
        }


        public async Task<StartWipResponse> StartWip(StartWipRequest requestData)
        {
            try
            {
                var requestUri = $"{Constants.BaseUrlJems4}api/Wips/{requestData.WipId}/start";

                var jsonContent = JsonConvert.SerializeObject(requestData);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json-patch+json");

                // Create the request
                var request = new HttpRequestMessage(HttpMethod.Post, requestUri)
                {
                    Content = content
                };

                // Add the cookie header with the token
                request.Headers.Add("Cookie", $"UserToken={_constants.TokenApi}");

                // Send the request
                var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                // Read and deserialize the response content
                var responseContent = await response.Content.ReadAsStringAsync();
                var startWipResponse = JsonConvert.DeserializeObject<StartWipResponse>(responseContent);

                return startWipResponse;
            }
            catch (HttpRequestException e)
            {
                // Log and rethrow the exception
                Console.WriteLine($"Request error: {e.Message}");
                throw;
            }
        }


        public async Task<CompleteWipResponse> CompleteWip(CompleteWipRequest requestData)
        {
            try
            {
                var requestUri = $"{Constants.BaseUrlJems4}api/Wips/{requestData.WipId}/complete";

                var jsonContent = JsonConvert.SerializeObject(requestData, new JsonSerializerSettings
                {
                    DateTimeZoneHandling = DateTimeZoneHandling.Utc
                });
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json-patch+json");

                // Create the request
                var request = new HttpRequestMessage(HttpMethod.Post, requestUri)
                {
                    Content = content
                };

                // Add the cookie header with the token
                request.Headers.Add("Cookie", $"UserToken={_constants.TokenApi}");

                // Send the request
                var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                // Read and deserialize the response content
                var responseContent = await response.Content.ReadAsStringAsync();
                var completeWipResponse = JsonConvert.DeserializeObject<CompleteWipResponse>(responseContent);

                return completeWipResponse;
            }
            catch (HttpRequestException e)
            {
                // Log and rethrow the exception
                Console.WriteLine($"Request error: {e.Message}");
                throw;
            }
        }



    }

}

