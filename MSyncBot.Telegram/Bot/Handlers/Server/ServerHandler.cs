using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json;

namespace MSyncBot.Telegram.Bot.Handlers.Server;

public class ServerHandler
{
    private string IpAddress { get; set; }
    private const int Port = 1689;
    public TcpClient TcpClient { get; set; }
    private Client Client { get; set; }
    public ServerHandler(string ipAddress)
    {
        IpAddress = ipAddress;
        TcpClient = new TcpClient();
    }
    
    public async Task ConnectToServerAsync()
    {
        try
        {
            Bot.Logger.LogProcess($"Connection to server {IpAddress}:{Port}");
            await TcpClient.ConnectAsync(IpAddress, Port);
            Bot.Logger.LogSuccess("Bot has been successfully connected to the server.");

            await SendClientDataAsync();
        }
        catch (Exception ex)
        {
            Bot.Logger.LogError("Error connecting to the server: " + ex.Message);
            await ConnectToServerAsync();
        }
    }
    
    private async Task SendClientDataAsync()
    {
        try
        {
            Bot.Logger.LogProcess("Sending client data to the server...");
            
            Client = new Client("MSyncBot.Telegram", ClientType.Telegram);
            var stream = TcpClient.GetStream();
            var serializedClient = JsonConvert.SerializeObject(Client);
            var data = Encoding.UTF8.GetBytes(serializedClient);
            await stream.WriteAsync(data);

            Bot.Logger.LogSuccess("Client data has been sent to the server.");
        }
        catch (Exception ex)
        {
            Bot.Logger.LogError("Error sending client data: " + ex.Message);
        }
    }

    public async Task<Client?> ReceiveMessageAsync(Stream stream)
    {
        try
        {
            var completeMessage = new byte[1024];
            var bytesRead = await stream.ReadAsync(completeMessage);

            if (bytesRead > 0)
            {
                var message = Encoding.UTF8.GetString(completeMessage, 0, bytesRead);
                return JsonConvert.DeserializeObject<Client>(message);
            }
        }
        catch (Exception ex)
        {
            Bot.Logger.LogError(ex.Message);
        }

        return null;
    }

    public async Task SendMessageAsync(Stream stream, Client client)
    {
        try
        {
            var serializedMessage = JsonConvert.SerializeObject(client);
            var data = Encoding.UTF8.GetBytes(serializedMessage);
            await stream.WriteAsync(data);
        }
        catch (Exception ex)
        {
            Bot.Logger.LogError(ex.Message);
        }
    }

    public void Disconnect()
    {
        try
        {
            Bot.Logger.LogProcess("Disconnecting from the server...");
            TcpClient.Close();
            Bot.Logger.LogSuccess("Bot has been successfully disconnected from the server.");
        }
        catch (Exception ex)
        {
           Bot.Logger.LogError("Error disconnecting from the server: " + ex.Message);
        }
    }
}