using MickeyInfoUtility.Interfaces;
using MickeyInfoUtility.Models;
using MickeyInfoUtility.Models.Shared;
using System.Text.Json;

namespace MickeyInfoUtility.Services
{
    public class RenovationService : IRenovationService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<RenovationService> _logger;
        private readonly IMasterKeyService _masterKeyService;
        private readonly string _apiKey;
        private readonly string _range = "Sheet1!A:K";

        public RenovationService(
            HttpClient httpClient,
            ILogger<RenovationService> logger,
            IMasterKeyService masterKeyService,
            IConfiguration configuration)
        {
            _httpClient = httpClient;
            _logger = logger;
            _masterKeyService = masterKeyService;
            _apiKey = configuration["GoogleSheets:ApiKey"];
        }

        public async Task<List<KeyListItem>> GetAvailableKeys()
        {
            try
            {
                var masterKeys = await _masterKeyService.GetAllMasterKeys();
                return masterKeys
                    .Where(mk => mk.Service == "Renovation")
                    .Select(mk => new KeyListItem { Key = mk.Key, Description = mk.Key })
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available keys");
                throw;
            }
        }

        public async Task<List<RenovationItem>> GetRenovationItems(string accessKey)
        {
            try
            {
                if (string.IsNullOrEmpty(accessKey))
                {
                    _logger.LogWarning("No access key provided");
                    return new List<RenovationItem>();
                }

                // Validate the access key and get spreadsheet ID
                var masterKey = await _masterKeyService.GetMasterKeyByKey(accessKey);
                if (masterKey == null || masterKey.Service != "Renovation")
                {
                    _logger.LogWarning($"Invalid access key: {accessKey}");
                    throw new UnauthorizedAccessException("Invalid access key");
                }

                var url = $"https://sheets.googleapis.com/v4/spreadsheets/{masterKey.SpreadsheetId}/values/{_range}?key={_apiKey}";
                _logger.LogInformation($"Requesting data from URL: {url}");

                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var jsonString = await response.Content.ReadAsStringAsync();
                _logger.LogInformation($"Received response: {jsonString}");

                var jsonDocument = JsonDocument.Parse(jsonString);
                var values = jsonDocument.RootElement.GetProperty("values").EnumerateArray().ToList();

                _logger.LogInformation($"Number of rows in response: {values.Count}");

                if (values.Count <= 1)
                {
                    _logger.LogWarning("No data rows found in the response");
                    return new List<RenovationItem>();
                }

                var items = new List<RenovationItem>();
                for (int i = 1; i < values.Count; i++) // Skip header row
                {
                    var row = values[i].EnumerateArray().Select(v => v.GetString()).ToList();
                    _logger.LogInformation($"Processing row {i}: {string.Join(", ", row)}");

                    if (row.Count >= 11)
                    {
                        var item = new RenovationItem
                        {
                            ItemName = row[0] ?? string.Empty,
                            Quantity = ParseDecimal(row[1]),
                            Measurement = row[2] ?? string.Empty,
                            UnitPrice = ParseDecimal(row[3]),
                            TotalPrice = ParseDecimal(row[4]),
                            PurchaseDate = ParseDateTime(row[5]),
                            ShopName = row[6] ?? string.Empty,
                            Salesperson = row[7] ?? string.Empty,
                            Contact = row[8] ?? string.Empty,
                            InvoiceQuotationNumber = row[9] ?? string.Empty,
                            Category = row[10] ?? string.Empty
                        };
                        items.Add(item);
                    }
                    else
                    {
                        _logger.LogWarning($"Row {i} does not have enough columns. Skipping.");
                    }
                }

                _logger.LogInformation($"Parsed {items.Count} renovation items");
                return items;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetRenovationItems");
                throw;
            }
        }

        private decimal ParseDecimal(string value)
        {
            if (decimal.TryParse(value, out decimal result))
                return result;

            _logger.LogWarning($"Failed to parse decimal value: {value}");
            return 0;
        }

        private DateTime? ParseDateTime(string value)
        {
            if (DateTime.TryParse(value, out DateTime result))
                return result;

            _logger.LogWarning($"Failed to parse date value: {value}");
            return null;
        }
    }
}
