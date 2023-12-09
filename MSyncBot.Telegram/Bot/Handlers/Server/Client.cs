namespace MSyncBot.Telegram.Bot.Handlers.Server
{
    public class Client
    {
        public string Name { get; }
        public ClientType ClientType { get; set; }
        public string? Message { get; set; }
        
        public Client(string name, ClientType clientType, string? message = null)
        {
            Name = name;
            ClientType = clientType;
            Message = message;
        }
    }

    public enum ClientType
    {
        Telegram,
        Discord,
        VK,
        None
    }
}