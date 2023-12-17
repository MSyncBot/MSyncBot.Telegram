using System.Text.Json.Serialization;

namespace MSyncBot.Telegram.Bot.Handlers.Server.Types;

public class Message(string senderName, int senderId, Enums.SenderType senderType, Enums.MessageType messageType, User user)
{
    public string SenderName { get; set; } = senderName;
    public int SenderId { get; set; } = senderId;
    public Enums.SenderType SenderType { get; set; } = senderType;
    public DateTime Timestamp { get; set; } = DateTime.Now;
    public int MessageId { get; set; } = GenerateMessageId();
    public string? Content { get; set; }
    public User User { get; set; } = user;

    public Enums.MessageType MessageType { get; set; } = messageType;
    public List<MediaFile> MediaFiles { get; set; } = new();
    
    private static int messageIdCounter;
    private static int GenerateMessageId() => messageIdCounter++;
}