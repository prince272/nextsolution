using System.Text.Json.Serialization;

namespace Next_Solution.WebApi.Providers.Ngrok.Models
{
    public class ErrorResponse
    {
        [JsonPropertyName("error_code")]
        public int ErrorCode { get; set; }

        [JsonPropertyName("status_code")]
        public int StatusCode { get; set; }

        [JsonPropertyName("msg")]
        public string Message { get; set; } = null!;

        public ErrorDetails Details { get; set; } = null!;
    }

    public class ErrorDetails
    {
        [JsonPropertyName("err")]
        public string ErrorMessage { get; set; } = null!;
    }
}

