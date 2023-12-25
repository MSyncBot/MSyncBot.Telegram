﻿using System.Text.Json;
using MSyncBot.Types;
using MSyncBot.Types.Enums;
using Telegram.Bot;
using Message = Telegram.Bot.Types.Message;
using MessageType = Telegram.Bot.Types.Enums.MessageType;

namespace MSyncBot.Telegram.Bot.Handlers
{
    public class FileHandler
    {
        private static readonly Dictionary<string, List<MediaFile>> MediaFiles = new();
        private static readonly Dictionary<string, int> CountMediaFiles = new();
        private static readonly Dictionary<string, int> SendingMediaFiles = new();

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

                var fileInfo = GetFileInfo(message);
                var file = await DownloadFileToRamAsync(botClient, fileInfo.id, (FileType)fileInfo.messageType);
                var fileMessage = new Types.Message("MSyncBot.Telegram",
                    1,
                    SenderType.Telegram,
                    fileInfo.messageType,
                    new User(user.FirstName, user.LastName, user.Username, (ulong?)user.Id));

                fileMessage.MediaFiles.Add(file);

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
            }

            return (fileId, messageType);
        }

        private static async Task<MediaFile?> DownloadFileToRamAsync(ITelegramBotClient botClient, string fileId,
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
            var file = new MediaFile(Guid.NewGuid().ToString(),
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

            if (sentFiles == albumSize)
            {
                var user = message.From;

                var albumMessage = new Types.Message("MSyncBot.Telegram",
                    1,
                    SenderType.Telegram,
                    Types.Enums.MessageType.Album,
                    new User(user.FirstName, user.LastName, user.Username, (ulong?)user.Id));
                albumMessage.MediaFiles.AddRange(mediaGroupFiles);

                var jsonAlbumMessage = JsonSerializer.Serialize(albumMessage);
                Bot.Server.SendTextAsync(jsonAlbumMessage);

                SendingMediaFiles.Remove(message.MediaGroupId);
                MediaFiles.Remove(message.MediaGroupId);
            }
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