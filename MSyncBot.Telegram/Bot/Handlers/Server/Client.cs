using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json;

namespace MSyncBot.Telegram.Bot.Handlers.Server;

public class Client
{
    public string Name { get; set; }
    public TcpClient TcpClient { get; }
    public ClientType ClientType { get; set; }
    public string? Message { get; set; }

    public Client(string name, TcpClient tcpClient, ClientType clientType, string? message = null)
    {
        Name = name;
        TcpClient = tcpClient;
        ClientType = clientType;
        Message = message;
    }

    public async Task SendAsync()
    {
        try
        {
            var jsonData = JsonConvert.SerializeObject(this);
            var data = Encoding.UTF8.GetBytes(jsonData);
            await TcpClient.GetStream().WriteAsync(data);
        }
        catch (Exception ex)
        {
            Bot.Logger.LogError(ex.Message);
        }
    }
}

public enum ClientType
{
    Telegram,
    Discord,
    VK,
    None
}