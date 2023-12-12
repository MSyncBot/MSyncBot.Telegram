namespace MSyncBot.Telegram.Bot.Handlers.Server;

public class Message
{
    public string SenderName { get; set; }
    public int SenderId { get; set; }
    public SenderType SenderType { get; set; }
    public DateTime Timestamp { get; set; }
    public int MessageId { get; set; }
    public string Content { get; set; }
    
    public Message(string senderName, int senderId, SenderType senderType, string content)
    {
        SenderName = senderName;
        SenderId = senderId;
        SenderType = senderType;
        Timestamp = DateTime.UtcNow;
        MessageId = GenerateMessageId();
        Content = content;
    }
    
    private static int messageIdCounter = 0;
    private static int GenerateMessageId() => messageIdCounter++;
}

public enum SenderType
{
    Telegram,
    Discord,
    VK,
    None,
}