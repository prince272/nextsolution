namespace Next_Solution.WebApi.Providers.Ngrok
{
    public class NgrokOptions
    {
        public bool ShowNgrokWindow { get; set; }

        public string AuthToken { get; set; } = null!;

        public string? Domain { get; set; } = null!;
    }
}
