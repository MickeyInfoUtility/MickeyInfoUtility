using MickeyInfoUtility.Interfaces;
using MickeyInfoUtility.Models.Shared;
using System.Text.Json;

namespace MickeyInfoUtility.Services
{
    public class MasterKeyService : IMasterKeyService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<MasterKeyService> _logger;
        private readonly IConfiguration _configuration;
        private readonly string _masterSpreadsheetId;
        private readonly string _apiKey;
        private readonly string _range = "Sheet1!A:C";

        public MasterKeyService(
            HttpClient httpClient,
            ILogger<MasterKeyService> logger,
            IConfiguration configuration)
        {
            _httpClient = httpClient;
            _logger = logger;
            _configuration = configuration;
            _masterSpreadsheetId = configuration["GoogleSheets:MasterSpreadsheetId"];
            _apiKey = configuration["GoogleSheets:ApiKey"];
        }

        public async Task<List<MasterKey>> GetAllMasterKeys()
        {
            try
            {
                var url = $"https://sheets.googleapis.com/v4/spreadsheets/{_masterSpreadsheetId}/values/{_range}?key={_apiKey}";
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var jsonString = await response.Content.ReadAsStringAsync();
                var jsonDocument = JsonDocument.Parse(jsonString);
                var values = jsonDocument.RootElement.GetProperty("values").EnumerateArray().ToList();

                if (values.Count <= 1)
                {
                    return new List<MasterKey>();
                }

                var masterKeys = new List<MasterKey>();
                for (int i = 1; i < values.Count; i++) // Skip header row
                {
                    var row = values[i].EnumerateArray().Select(v => v.GetString()).ToList();
                    if (row.Count >= 3)
                    {
                        masterKeys.Add(new MasterKey
                        {
                            Key = row[0] ?? string.Empty,
                            SpreadsheetId = row[1] ?? string.Empty,
                            Service = row[2] ?? string.Empty
                        });
                    }
                }

                return masterKeys;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetAllMasterKeys");
                throw;
            }
        }

        public async Task<MasterKey> GetMasterKeyByKey(string key)
        {
            var masterKeys = await GetAllMasterKeys();
            return masterKeys.FirstOrDefault(mk => mk.Key == key);
        }

        public async Task<bool> ValidateKey(string key, string service)
        {
            var masterKeys = await GetAllMasterKeys();
            return masterKeys.Any(mk => mk.Key == key && mk.Service == service);
        }
    }
}
