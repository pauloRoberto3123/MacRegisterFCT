using MacRegister.Dtos;
using MacRegister.Model.MacApi;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace MacRegister.InfraStructure
{
    public class MacAPI
    {
        private readonly HttpClient _httpClient;

        public MacAPI(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri(Constants.BaseUrlMac);

        }


        public async Task<Mac_Record_Logs> GetMacLog(string serialNumber)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{Constants.BaseUrlMac}/api/mac/{serialNumber}");

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<Mac_Record_Logs>();
                }
                else
                {
                    throw new HttpRequestException($"Failed to retrieve Mac log. Status code: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to retrieve Mac log.", ex);
            }
        }
        public string GetPgmVersion(string serialNumber)
        {
            try
            {
                var response = _httpClient.GetAsync($"{Constants.BaseUrlMac}/api/mes/{serialNumber}/PGM_SCRIPT_VERSION").Result;

                if (response.IsSuccessStatusCode)
                {
                    return response.Content.ReadAsStringAsync().Result;
                }
                else
                {
                    return "Fail";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception occurred while getting PGM version: {ex.Message}");
                return "Fail";
            }
        }
        public async Task<Mac_Record_Logs> InsertMacLog(MacInsertRequest request)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync($"{Constants.BaseUrlMac}/api/mac/add-mac-log", request);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<Mac_Record_Logs>();
                }
                else
                {
                    throw new HttpRequestException($"Failed to insert Mac log. Status code: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to insert Mac log.", ex);
            }
        }

    }
}
