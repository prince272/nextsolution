namespace NextSolution.Core.Extensions.EmailSender
{
    public class EmailAttachment
    {
        public string FileName { get; set; } = default!;

        public Stream Content { get; set; } = default!;

        public string ContentType { get; set; } = default!;
    }
}
