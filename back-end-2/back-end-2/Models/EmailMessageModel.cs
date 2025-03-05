public class EmailMessageModel
{
    public string ToAddress { get; set; }
    public string Subject { get; set; }
    public string? Body { get; set; }

    public EmailMessageModel(string toAddress, string subject, string? body = "")
    {
        ToAddress = toAddress;
        Subject = subject;
        Body = body;
    }
}
