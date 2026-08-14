using System.Reflection.Metadata.Ecma335;

namespace Sms
{
    public class Helper
    {
        public static async Task<(bool isSuccess, string responseBody)> SendRequestAsync(IHttpClientFactory httpClientFactory)
        {
            var client = httpClientFactory.CreateClient();

            var request = new BaseSmsRquest
            {
                Credential = new SmsCredential
                {
                    Password = "C8aykU*GX8",
                    Username = "balaban-bulktest"
                },
                Header = new SmsHeader { },
                Message = "10-Buket Last Demo",
                To = "905307869075"
            };

            var jsonOptions = new System.Text.Json.JsonSerializerOptions
            {
                PropertyNamingPolicy = null
            };
            var jsonBody = System.Text.Json.JsonSerializer.Serialize(request, jsonOptions);

            using var content = new StringContent(jsonBody, System.Text.Encoding.UTF8, "application/json");
            var response =  await client.PostAsync("https://sgw.maradit.net/api/json/reply/Submit", content);

            var resp = await response.Content.ReadAsStringAsync();
            return (response.IsSuccessStatusCode,  resp);
        }
    }
}
