using MSyncBot.Telegram.Bot.Handlers.Server.Types.Enums;

namespace MSyncBot.Telegram.Bot.Handlers.Server.Types;

public class MediaFile(string name, string extension, byte[] data, FileType fileType)
{
    public string Name { get; set; } = name;
    public string Extension { get; set; } = extension;
    public byte[] Data { get; set; } = data;
    public FileType FileType { get; set; } = fileType;
}