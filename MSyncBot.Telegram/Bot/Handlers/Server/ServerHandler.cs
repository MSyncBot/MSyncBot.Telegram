using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json;

namespace MSyncBot.Telegram.Bot.Handlers.Server;

public class ServerHandler
{
    private string IpAddress { get; set; }
    private const int Port = 1689;
    public TcpClient TcpClient { get; set; }

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
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error connecting to the server: " + ex.Message);
        }
    }

    public async Task<string> ReceiveMessageAsync(NetworkStream stream)
    {
        try
        {
            var completeMessage = new StringBuilder();
            var bufferSize = new byte[1024];
           // while (stream.DataAvailable)
            //{
                var bytesRead = await stream.ReadAsync(bufferSize);
                completeMessage.Append(Encoding.UTF8.GetString(bufferSize, 0, bytesRead));
            //}
            return completeMessage.ToString();
        }
        catch (Exception ex)
        {
            Bot.Logger.LogError(ex.Message);
            return string.Empty;
        }
    }
    
    public async Task SendAsync(string message)
    {
        try
        {
            //var jsonData = JsonConvert.SerializeObject(this);
            var data = Encoding.UTF8.GetBytes(message);
            await TcpClient.GetStream().WriteAsync(data);
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