using System.Text.Json;
using MSyncBot.Types;
using MSyncBot.Types.Enums;
using Telegram.Bot;
using Message = Telegram.Bot.Types.Message;
using MessageType = Telegram.Bot.Types.Enums.MessageType;

namespace MSyncBot.Telegram.Bot.Handlers
{
    public class MediaFileHandler
    {
        private static readonly Dictionary<string, List<MediaFile>> MediaFiles = new();
        private static readonly Dictionary<string, int> CountMediaFiles = new();
        private static readonly Dictionary<string, int> SendingMediaFiles = new();

        public async Task GetMediaFilesAsync(ITelegramBotClient botClient, Message message)
        {
            try
            {
                switch (message.Type)
                {
                    case MessageType.Photo:
                        await ProcessPhotoMessageAsync(botClient, message);
                        return;

                    case MessageType.Video:
                        await ProcessVideoMessageAsync(botClient, message);
                        return;
                    
                    case MessageType.Animation:
                        await ProcessAnimationMessageAsync(botClient, message);
                        return;
                    
                    case MessageType.Document:
                        await ProcessDocumentMessageAsync(botClient, message);
                        return;
                    
                    case MessageType.Audio:
                        await ProcessAudioMessageAsync(botClient, message);
                        return;
                    
                    case MessageType.Voice:
                        await ProcessVoiceMessageAsync(botClient, message);
                        return;
                    
                    case MessageType.VideoNote:
                        await ProcessVideoNoteMessageAsync(botClient, message);
                        return;
                }
            }
            catch (Exception ex)
            {
                Bot.Logger.LogError(ex.Message);
            }
        }

        private static async Task ProcessPhotoMessageAsync(ITelegramBotClient botClient, Message message)
        {
            var photoId = message.Photo?.Last()?.FileId;
            if (string.IsNullOrEmpty(photoId)) return;

            var photoInfo = await botClient.GetFileAsync(photoId);
            var photoPath = photoInfo.FilePath;

            using var photoStream = new MemoryStream();
            await botClient.DownloadFileAsync(filePath: photoPath, destination: photoStream);
            photoStream.Seek(0, SeekOrigin.Begin);
            var photoBytes = photoStream.ToArray();
            var photoFile = new MediaFile(Guid.NewGuid().ToString(),
                ".png",
                photoBytes,
                FileType.Photo);
            
            var photoMessage = new Types.Message("MSyncBot.Telegram",
                1,
                SenderType.Telegram,
                Types.Enums.MessageType.Photo,
                new Types.User(message.From.FirstName));
            photoMessage.MediaFiles.Add(photoFile);
            
            string? jsonPhotoMessage;
            if (string.IsNullOrEmpty(message.MediaGroupId))
            {
                jsonPhotoMessage = JsonSerializer.Serialize(photoMessage);
                Bot.Server.SendTextAsync(jsonPhotoMessage);
                return;
            }

            if (!MediaFiles.TryGetValue(message.MediaGroupId, out var mediaGroupFiles))
            {
                mediaGroupFiles = new List<MediaFile>();
                MediaFiles.Add(message.MediaGroupId, mediaGroupFiles);
            }

            mediaGroupFiles.Add(photoFile);
            
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
                photoMessage.MediaFiles.AddRange(mediaGroupFiles);
                jsonPhotoMessage = JsonSerializer.Serialize(photoMessage);
                Bot.Server.SendTextAsync(jsonPhotoMessage);
                
                SendingMediaFiles.Remove(message.MediaGroupId);
                MediaFiles.Remove(message.MediaGroupId);
            }
        }

        private static async Task ProcessVideoMessageAsync(ITelegramBotClient botClient, Message message)
        {
            var videoId = message.Video?.FileId;
            if (string.IsNullOrEmpty(videoId)) return;

            var videoInfo = await botClient.GetFileAsync(videoId);
            var videoPath = videoInfo.FilePath;

            using var videoStream = new MemoryStream();
            await botClient.DownloadFileAsync(filePath: videoPath, destination: videoStream);
            videoStream.Seek(0, SeekOrigin.Begin);

            var videoBytes = videoStream.ToArray();
            var videoFile = new MediaFile(Guid.NewGuid().ToString(),
                ".mp4",
                videoBytes,
                FileType.Video);

            string? jsonVideoMessage;
            if (string.IsNullOrEmpty(message.MediaGroupId))
            {
                var videoMessage = new Types.Message("MSyncBot.Telegram",
                    1,
                    SenderType.Telegram,
                    Types.Enums.MessageType.Video,
                    new Types.User(message.From.FirstName));
                videoMessage.MediaFiles.Add(videoFile);
                jsonVideoMessage = JsonSerializer.Serialize(videoMessage);
                Bot.Server.SendTextAsync(jsonVideoMessage);
                return;
            }

            if (!MediaFiles.TryGetValue(message.MediaGroupId, out var mediaGroupFiles))
            {
                mediaGroupFiles = new List<MediaFile>();
                MediaFiles.Add(message.MediaGroupId, mediaGroupFiles);
            }

            mediaGroupFiles.Add(videoFile);
            jsonVideoMessage = JsonSerializer.Serialize(videoFile);

            Bot.Server.SendTextAsync(jsonVideoMessage);
        }

        private static async Task ProcessAnimationMessageAsync(ITelegramBotClient botClient, Message message)
        {
            var animationId = message.Animation?.FileId;
            if (string.IsNullOrEmpty(animationId)) return;

            var animationInfo = await botClient.GetFileAsync(animationId);
            var animationPath = animationInfo.FilePath;

            using var animationStream = new MemoryStream();
            await botClient.DownloadFileAsync(filePath: animationPath, destination: animationStream);
            animationStream.Seek(0, SeekOrigin.Begin);

            var animationBytes = animationStream.ToArray();
            var animationFile = new MediaFile(Guid.NewGuid().ToString(),
                ".gif",
                animationBytes,
                FileType.Animation);

            string? jsonAnimationMessage;
            if (string.IsNullOrEmpty(message.MediaGroupId))
            {
                var animationMessage = new Types.Message("MSyncBot.Telegram",
                    1,
                    SenderType.Telegram,
                    Types.Enums.MessageType.Animation,
                    new User(message.From.FirstName));
                animationMessage.MediaFiles.Add(animationFile);
                jsonAnimationMessage = JsonSerializer.Serialize(animationMessage);
                Bot.Server.SendTextAsync(jsonAnimationMessage);
                return;
            }

            if (!MediaFiles.TryGetValue(message.MediaGroupId, out var mediaGroupFiles))
            {
                mediaGroupFiles = new List<MediaFile>();
                MediaFiles.Add(message.MediaGroupId, mediaGroupFiles);
            }

            mediaGroupFiles.Add(animationFile);
            jsonAnimationMessage = JsonSerializer.Serialize(animationFile);

            Bot.Server.SendTextAsync(jsonAnimationMessage);
        }

        private static async Task ProcessDocumentMessageAsync(ITelegramBotClient botClient, Message message)
        {
            var documentId = message.Document?.FileId;
            if (string.IsNullOrEmpty(documentId)) return;

            var documentInfo = await botClient.GetFileAsync(documentId);
            var documentPath = documentInfo.FilePath;

            using var documentStream = new MemoryStream();
            await botClient.DownloadFileAsync(filePath: documentPath, destination: documentStream);
            documentStream.Seek(0, SeekOrigin.Begin);

            var documentBytes = documentStream.ToArray();
            var documentFile = new MediaFile(Guid.NewGuid().ToString(),
                Path.GetExtension(documentInfo.FilePath),
                documentBytes,
                FileType.Document);

            string? jsonDocumentMessage;
            if (string.IsNullOrEmpty(message.MediaGroupId))
            {
                var documentMessage = new Types.Message("MSyncBot.Telegram",
                    1,
                    SenderType.Telegram,
                    Types.Enums.MessageType.Document,
                    new Types.User(message.From.FirstName));
                documentMessage.MediaFiles.Add(documentFile);
                jsonDocumentMessage = JsonSerializer.Serialize(documentMessage);
                Bot.Server.SendTextAsync(jsonDocumentMessage);
                return;
            }

            if (!MediaFiles.TryGetValue(message.MediaGroupId, out var mediaGroupFiles))
            {
                mediaGroupFiles = new List<MediaFile>();
                MediaFiles.Add(message.MediaGroupId, mediaGroupFiles);
            }

            mediaGroupFiles.Add(documentFile);
            jsonDocumentMessage = JsonSerializer.Serialize(documentFile);

            Bot.Server.SendTextAsync(jsonDocumentMessage);
        }

        private static async Task ProcessAudioMessageAsync(ITelegramBotClient botClient, Message message)
        {
            var audioId = message.Audio?.FileId;
            if (string.IsNullOrEmpty(audioId)) return;

            var audioInfo = await botClient.GetFileAsync(audioId);
            var audioPath = audioInfo.FilePath;

            using var audioStream = new MemoryStream();
            await botClient.DownloadFileAsync(filePath: audioPath, destination: audioStream);
            audioStream.Seek(0, SeekOrigin.Begin);

            var audioBytes = audioStream.ToArray();
            var audioFile = new MediaFile(Guid.NewGuid().ToString(),
                Path.GetExtension(audioInfo.FilePath),
                audioBytes,
                FileType.Audio);

            string? jsonAudioMessage;
            if (string.IsNullOrEmpty(message.MediaGroupId))
            {
                var audioMessage = new Types.Message("MSyncBot.Telegram",
                    1,
                    SenderType.Telegram,
                    Types.Enums.MessageType.Audio,
                    new User(message.From.FirstName));
                audioMessage.MediaFiles.Add(audioFile);
                jsonAudioMessage = JsonSerializer.Serialize(audioMessage);
                Bot.Server.SendTextAsync(jsonAudioMessage);
                return;
            }

            if (!MediaFiles.TryGetValue(message.MediaGroupId, out var mediaGroupFiles))
            {
                mediaGroupFiles = new List<MediaFile>();
                MediaFiles.Add(message.MediaGroupId, mediaGroupFiles);
            }

            mediaGroupFiles.Add(audioFile);
            jsonAudioMessage = JsonSerializer.Serialize(audioFile);

            Bot.Server.SendTextAsync(jsonAudioMessage);
        }
        
        private static async Task ProcessVoiceMessageAsync(ITelegramBotClient botClient, Message message)
        {
            var voiceId = message.Voice?.FileId;
            if (string.IsNullOrEmpty(voiceId)) return;

            var voiceInfo = await botClient.GetFileAsync(voiceId);
            var voicePath = voiceInfo.FilePath;

            using var voiceStream = new MemoryStream();
            await botClient.DownloadFileAsync(filePath: voicePath, destination: voiceStream);
            voiceStream.Seek(0, SeekOrigin.Begin);

            var voiceBytes = voiceStream.ToArray();
            var voiceFile = new MediaFile(Guid.NewGuid().ToString(),
                Path.GetExtension(voiceInfo.FilePath),
                voiceBytes,
                FileType.Voice);

            var jsonVoiceMessage = JsonSerializer.Serialize(voiceFile);
            Bot.Server.SendTextAsync(jsonVoiceMessage);
        }

        private static async Task ProcessVideoNoteMessageAsync(ITelegramBotClient botClient, Message message)
        {
            var videoNoteId = message.VideoNote?.FileId;
            if (string.IsNullOrEmpty(videoNoteId)) return;

            var videoNoteInfo = await botClient.GetFileAsync(videoNoteId);
            var videoNotePath = videoNoteInfo.FilePath;

            using var videoNoteStream = new MemoryStream();
            await botClient.DownloadFileAsync(filePath: videoNotePath, destination: videoNoteStream);
            videoNoteStream.Seek(0, SeekOrigin.Begin);

            var videoNoteBytes = videoNoteStream.ToArray();
            var videoNoteFile = new MediaFile(Guid.NewGuid().ToString(),
                ".mp4",
                videoNoteBytes,
                FileType.VideoNote);

            var jsonVideoNoteMessage = JsonSerializer.Serialize(videoNoteFile);
            Bot.Server.SendTextAsync(jsonVideoNoteMessage);
        }

        public static void CountingMediaFiles(Message message)
        {
            if (string.IsNullOrEmpty(message.MediaGroupId))
                return;

            if (!CountMediaFiles.TryAdd(message.MediaGroupId, 1))
                CountMediaFiles[message.MediaGroupId]++;
        }
    }
}