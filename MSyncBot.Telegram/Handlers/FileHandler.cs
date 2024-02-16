using System.Text.Json;
using MSyncBot.Types;
using MSyncBot.Types.Enums;
using Telegram.Bot;
using File = MSyncBot.Types.File;
using Message = Telegram.Bot.Types.Message;
using MessageType = Telegram.Bot.Types.Enums.MessageType;
using User = MSyncBot.Types.User;

namespace MSyncBot.Telegram.Handlers
{
    public class FileHandler
    {
        private static readonly Dictionary<string, List<File>> MediaFiles = new();
        private static readonly Dictionary<string, int> CountMediaFiles = new();
        private static readonly Dictionary<string, int> SendingMediaFiles = new();
        private static readonly Dictionary<string, string?> CaptionAlbum = new();

        public async Task FileHandlerAsync(ITelegramBotClient botClient, Message message)
        {
            try
            {
                if (!string.IsNullOrEmpty(message.MediaGroupId))
                {
                    await AlbumHandlerAsync(botClient, message);
                    return;
                }

                var user = message.From;
                var chat = message.Chat;

                var fileInfo = GetFileInfo(message);
                var file = await DownloadFileToRamAsync(botClient, fileInfo.id, (FileType)fileInfo.messageType);

                var fileMessage = new Types.Message(
                    new Messenger("MSyncBot.Telegram", MessengerType.Telegram),
                    fileInfo.messageType,
                    new User(user.FirstName, (ulong)user.Id)
                    {
                        LastName = user.LastName,
                        Username = user.Username
                    },
                    new Chat(chat.FirstName, (ulong)chat.Id)
                    {
                        InviteLink = chat.InviteLink,
                        //Photo = new File()
                    })
                {
                    Text = message.Caption
                };

                fileMessage.Files.Add(file);

                var jsonFileMessage = JsonSerializer.Serialize(fileMessage);
                Bot.Server.SendTextAsync(jsonFileMessage);
            }
            catch (Exception ex)
            {
                Bot.Logger.LogError(ex.Message);
            }
        }

        private static (string id, Types.Enums.MessageType messageType) GetFileInfo(Message message)
        {
            var fileId = string.Empty;
            var messageType = Types.Enums.MessageType.Unknown;
            switch (message.Type)
            {
                case MessageType.Photo:
                    fileId = message.Photo.Last().FileId;
                    messageType = Types.Enums.MessageType.Photo;
                    break;
                case MessageType.Audio:
                    fileId = message.Audio.FileId;
                    messageType = Types.Enums.MessageType.Audio;
                    break;
                case MessageType.Video:
                    fileId = message.Video.FileId;
                    messageType = Types.Enums.MessageType.Voice;
                    break;
                case MessageType.Voice:
                    fileId = message.Voice.FileId;
                    messageType = Types.Enums.MessageType.Voice;
                    break;
                case MessageType.Document:
                    fileId = message.Document.FileId;
                    messageType = Types.Enums.MessageType.Document;
                    break;
                case MessageType.VideoNote:
                    fileId = message.VideoNote.FileId;
                    messageType = Types.Enums.MessageType.VideoNote;
                    break;
                case MessageType.Animation:
                    fileId = message.Animation.FileId;
                    messageType = Types.Enums.MessageType.Animation;
                    break;
                case MessageType.Sticker:
                    fileId = message.Sticker.FileId;
                    messageType = Types.Enums.MessageType.Sticker;
                    break;
            }

            return (fileId, messageType);
        }

        private static async Task<File?> DownloadFileToRamAsync(ITelegramBotClient botClient, string fileId,
            FileType fileType)
        {
            if (string.IsNullOrEmpty(fileId))
                return null;

            var fileInfo = await botClient.GetFileAsync(fileId);
            var filePath = fileInfo.FilePath;

            using var fileStream = new MemoryStream();
            await botClient.DownloadFileAsync(filePath: filePath, destination: fileStream);
            fileStream.Seek(0, SeekOrigin.Begin);

            var fileBytes = fileStream.ToArray();
            var file = new File(Guid.NewGuid().ToString(),
                Path.GetExtension(filePath),
                fileBytes,
                fileType);

            return file;
        }

        private static async Task AlbumHandlerAsync(ITelegramBotClient botClient, Message message)
        {
            if (!MediaFiles.TryGetValue(message.MediaGroupId, out var mediaGroupFiles))
            {
                mediaGroupFiles = [];
                MediaFiles.Add(message.MediaGroupId, mediaGroupFiles);
                CaptionAlbum.Add(message.MediaGroupId, message.Caption);
            }

            var fileInfo = GetFileInfo(message);
            var file = await DownloadFileToRamAsync(botClient, fileInfo.id, (FileType)fileInfo.messageType);

            mediaGroupFiles.Add(file);

            var albumSize = CountMediaFiles[message.MediaGroupId];
            if (!SendingMediaFiles.TryGetValue(message.MediaGroupId, out var sentFiles))
            {
                sentFiles = 1;
                SendingMediaFiles.Add(message.MediaGroupId, sentFiles);
            }
            else
            {
                sentFiles++;
                SendingMediaFiles[message.MediaGroupId] = sentFiles;
            }

            if (sentFiles != albumSize)
                return;

            var user = message.From;
            var chat = message.Chat;

            var albumMessage = new Types.Message(
                new Messenger("MSyncBot.Telegram", MessengerType.Telegram),
                fileInfo.messageType,
                new User(user.FirstName, (ulong)user.Id)
                {
                    LastName = user.LastName, 
                    Username = user.Username
                },
                new Chat(chat.Title, (ulong)chat.Id))
            {
                Text = CaptionAlbum[message.MediaGroupId]
            };
            albumMessage.Files.AddRange(mediaGroupFiles);

            var jsonAlbumMessage = JsonSerializer.Serialize(albumMessage);
            Bot.Server.SendTextAsync(jsonAlbumMessage);

            SendingMediaFiles.Remove(message.MediaGroupId);
            MediaFiles.Remove(message.MediaGroupId);
            CaptionAlbum.Remove(message.MediaGroupId);
        }

        public static void CountingMediaFiles(string groupId)
        {
            if (string.IsNullOrEmpty(groupId))
                return;

            if (CountMediaFiles.TryGetValue(groupId, out var value))
            {
                CountMediaFiles[groupId] = ++value;
                return;
            }

            CountMediaFiles.Add(groupId, 1);
        }
    }
}