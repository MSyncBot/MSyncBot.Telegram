namespace MSyncBot.Telegram.Bot.Handlers.Server.Types;

public class User(string firstName, string? lastName = null, string? username = null, ulong? id = null)
{
    public string FirstName { get; set; } = firstName;
    public string? LastName { get; set; } = lastName;
    public string? Username { get; set; } = username;
    public ulong? Id { get; set; } = id;
}