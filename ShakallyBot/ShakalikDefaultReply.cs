﻿using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Shakkaler;
using System.IO;
using Telegram.Bot.Types.InputFiles;


namespace Shakalik
#pragma warning disable CS8604 // Possible null reference argument.
#pragma warning disable CS8602 // Dereference of a possibly null reference.
{
    internal class ShakalikDefaultReply
    {
        private CancellationToken m_cancellationToken;
        private readonly Update m_update;
        private readonly ITelegramBotClient? m_client;
        private long? m_chatId;
        private readonly Random m_random = new();

        public ShakalikDefaultReply(CancellationToken cancellationToken, ITelegramBotClient? client, long? chatId, Update update)
        {
            m_cancellationToken = cancellationToken;
            m_client = client;
            m_chatId = chatId;
            m_update = update;
        }

        // Start and error reply
        internal async Task WelcomeReply()
        {
            GenerateChatDirectories.Init(m_chatId);

            _ = await m_client.SendTextMessageAsync(
                chatId: m_chatId,
                text: "Привет 👋.\nЯ <b>Shakalik</b>, твой друг в мире сохранения <b>фото</b> 🖼 в удобоваримом  качестве😊.\nПросто скинь мне что ты хочешь сжать и я сделаю это 🤖.",
                parseMode: ParseMode.Html,
                disableNotification: true,
                replyToMessageId: m_update.Message.MessageId,
                replyMarkup: new InlineKeyboardMarkup(
                    InlineKeyboardButton.WithUrl(
                        "Посмотреть исходный код",
                        "https://github.com/jejikeh/ShakalikBot")),
                cancellationToken: m_cancellationToken);
        }
        internal async Task ErrorReply()
        {
            _ = await m_client.SendTextMessageAsync(
                chatId: m_chatId,
                text: $"Я тебя не понял {GenerateRandom.Emoji}, пожалуйста, нажми на /start",
                parseMode: ParseMode.Html,
                disableNotification: true,
                cancellationToken: m_cancellationToken);
        }

        // Download photo

        internal async Task CompressPhotoAndReply(string savePath)
        {
            _ = await m_client.SendTextMessageAsync(
                chatId: m_chatId,
                text: $"<b>Почти готово </b> {GenerateRandom.Emoji}",
                parseMode: ParseMode.Html,
                disableNotification: true,
                cancellationToken: m_cancellationToken);


            var fileId = m_update.Message?.Photo?.Last().FileId;

            

            string dirPath = savePath + m_chatId + @"\Uncompress";
            string compressPath = savePath + m_chatId + @"\Compress\";

            await using FileStream fileStream = System.IO.File.OpenWrite(ProjectDir.basePath + @"\Media\" + m_chatId + @"\Uncompress\" + fileId + ".jpg");
            _ = await m_client.GetInfoAndDownloadFileAsync(
                fileId: fileId,
                destination: fileStream);

            var fileInfoSize = await m_client.GetFileAsync(fileId);
            var fileSizeBeforeCompression = fileInfoSize.FileSize;

            int compressionLevel = m_random.Next(0, 10);
            int compressionDisplayLevel = 10 - compressionLevel;
            
            string messageText = "<b>Сжимаем фото в </b>" + compressionDisplayLevel + "<b> раз! </b>" + emojiSet[m_random.Next(emojiSet.Count)];
            if (compressionDisplayLevel < 5 && compressionDisplayLevel != 1){
                messageText = "<b>Сжимаем фото в </b>" + compressionDisplayLevel + "<b> разa! </b>" + emojiSet[m_random.Next(emojiSet.Count)];
            }
            Message? compressionMessage = await m_client.SendTextMessageAsync(
                chatId: m_chatId,
                text: messageText,
                parseMode: ParseMode.Html,
                disableNotification: true,
                cancellationToken: m_cancellationToken);
            fileStream.Dispose();
    

            await Shakkal.CompressAndSaveFileAsync(dirPath + @"\" + fileId + ".jpg", compressPath + @"\", m_update.Message.MessageId.ToString() + ".jpg",compressionLevel);
            
            await using Stream stream = System.IO.File.OpenRead(compressPath + @"\" + m_update.Message.MessageId.ToString() + ".jpg");
            Message finalMessage = await m_client.SendPhotoAsync(
                chatId: m_chatId,
                photo: new InputOnlineFile(content: stream));
            stream.Dispose();


            long sizeStream = new System.IO.FileInfo(compressPath + @"\" + m_update.Message.MessageId.ToString() + ".jpg").Length;
            string pathFileCompleted = m_update.Message.MessageId.ToString() + ".jpg";
            string callbackDataOff = m_update.Message.MessageId.ToString();
            if (fileSizeBeforeCompression != null)
            {
                long bytesSaved = fileSizeBeforeCompression.Value - sizeStream;
                Message message = await m_client.SendTextMessageAsync(
                    chatId: m_chatId,
                    text: bytesSaved + " байт сохранено! " + emojiSet[m_random.Next(emojiSet.Count)],
                    parseMode: ParseMode.Html,
                    disableNotification: true,
                    replyToMessageId: finalMessage.MessageId,
                    replyMarkup: new InlineKeyboardMarkup(
                        InlineKeyboardButton.WithCallbackData(
                            text:"Сжать еще раз",
                            callbackData: callbackDataOff)),
                    cancellationToken: m_cancellationToken);

            }
        }

        internal async Task CompressAudioAndReply(string savePath)
        {
            Message? middleMessage = await m_client.SendTextMessageAsync(
                chatId: m_chatId,
                text: "<b>Немного терпения... </b>" + emojiSet[m_random.Next(emojiSet.Count)],
                parseMode: ParseMode.Html,
                disableNotification: true,
                cancellationToken: m_cancellationToken);

            var fileId = m_update.Message?.Audio?.FileId;
            Directory.CreateDirectory(savePath + m_chatId);
            Directory.CreateDirectory(savePath + m_chatId + @"\Uncompress");
            Directory.CreateDirectory(savePath + m_chatId + @"\Compress");

            string dirPath = savePath + m_chatId + @"\Uncompress";
            string compressPath = savePath + m_chatId + @"\Compress\";

            await using FileStream fileStream = System.IO.File.OpenWrite(savePath + m_chatId + @"\Uncompress\" + fileId + ".mp3");
            var file = await m_client.GetInfoAndDownloadFileAsync(
                fileId: fileId,
                destination: fileStream);
            fileStream.Dispose();

            await Shakkal.CompressAudioFileAsync(@"\Media\" + m_chatId + @"\Uncompress\" + fileId + ".mp3", @"\Media\" + m_chatId + @"\Compress\" + fileId + ".mp3");

            await using Stream streamFile = System.IO.File.OpenRead(compressPath + @"\" + fileId + ".mp3");
            Message finalMessage = await m_client.SendAudioAsync(
                chatId: m_chatId,
                audio: new InputOnlineFile(content: streamFile));
            streamFile.Dispose();

            long sizeStream = new System.IO.FileInfo(compressPath + @"\" + fileId + ".mp3").Length;
            var fileInfoSize = await m_client.GetFileAsync(fileId);
            var fileSizeBeforeCompression = fileInfoSize.FileSize;
            long bytesSaved = fileSizeBeforeCompression.Value - sizeStream;
            Message? compressionMessage = await m_client.SendTextMessageAsync(
                chatId: m_chatId,
                text: bytesSaved + " байт сохранено! " + emojiSet[m_random.Next(emojiSet.Count)],
                parseMode: ParseMode.Html,
                disableNotification: true,
                cancellationToken: m_cancellationToken);
        }

        internal async Task CompressVoiceAndReply(string savePath)
        {
            Message? middleMessage = await m_client.SendTextMessageAsync(
                chatId: m_chatId,
                text: "<b>Немного терпения... </b>" + emojiSet[m_random.Next(emojiSet.Count)],
                parseMode: ParseMode.Html,
                disableNotification: true,
                cancellationToken: m_cancellationToken);

            var fileId = m_update.Message?.Voice?.FileId;
            Directory.CreateDirectory(savePath + m_chatId);
            Directory.CreateDirectory(savePath + m_chatId + @"\Uncompress");
            Directory.CreateDirectory(savePath + m_chatId + @"\Compress");

            string dirPath = savePath + m_chatId + @"\Uncompress";
            string compressPath = savePath + m_chatId + @"\Compress\";

            await using FileStream fileStream = System.IO.File.OpenWrite(savePath + m_chatId + @"\Uncompress\" + fileId + ".mp3");
            var file = await m_client.GetInfoAndDownloadFileAsync(
                fileId: fileId,
                destination: fileStream);
            
            fileStream.Dispose();
            await Shakkal.CompressAudioFileAsync(@"\Media\" + m_chatId + @"\Uncompress\" + fileId + ".mp3", @"\Media\" + m_chatId + @"\Compress\" + fileId + ".mp3");

            await using Stream stream = System.IO.File.OpenRead(compressPath + @"\" + fileId + ".mp3");
            Message finalMessage = await m_client.SendVoiceAsync(
                chatId: m_chatId,
                voice: new InputOnlineFile(content: stream));
            stream.Dispose();

            long sizeStream = new System.IO.FileInfo(compressPath + @"\" + fileId + ".mp3").Length;
            var fileInfoSize = await m_client.GetFileAsync(fileId);
            var fileSizeBeforeCompression = fileInfoSize.FileSize;
            long bytesSaved = fileSizeBeforeCompression.Value - sizeStream;
            Message? compressionMessage = await m_client.SendTextMessageAsync(
                chatId: m_chatId,
                text: bytesSaved + " байт сохранено! " + emojiSet[m_random.Next(emojiSet.Count)],
                parseMode: ParseMode.Html,
                disableNotification: true,
                cancellationToken: m_cancellationToken);
        }

        internal async Task CompressVideoAndReply(string savePath)
        {
            Message? middleMessage = await m_client.SendTextMessageAsync(
                chatId: m_chatId,
                text: "<b>Немного терпения... </b>" + emojiSet[m_random.Next(emojiSet.Count)],
                parseMode: ParseMode.Html,
                disableNotification: true,
                cancellationToken: m_cancellationToken);

            var fileId = m_update.Message?.Video?.FileId;
            Directory.CreateDirectory(savePath + m_chatId);
            Directory.CreateDirectory(savePath + m_chatId + @"\Uncompress");
            Directory.CreateDirectory(savePath + m_chatId + @"\Compress");

            string dirPath = savePath + m_chatId + @"\Uncompress";
            string compressPath = savePath + m_chatId + @"\Compress\";

            await using FileStream fileStream = System.IO.File.OpenWrite(savePath + m_chatId + @"\Uncompress\" + fileId + ".mp4");
            var file = await m_client.GetInfoAndDownloadFileAsync(
                fileId: fileId,
                destination: fileStream);

            fileStream.Dispose();
            await Shakkal.CompressVideoFileAsync(@"\Media\" + m_chatId + @"\Uncompress\" + fileId + ".mp4", @"\Media\" + m_chatId + @"\Compress\" + fileId + ".mp4");

            await using Stream stream = System.IO.File.OpenRead(compressPath + @"\" + fileId + ".mp4");
            Message finalMessage = await m_client.SendVideoAsync(
                chatId: m_chatId,
                video: new InputOnlineFile(content: stream));
            stream.Dispose();

            long sizeStream = new System.IO.FileInfo(compressPath + @"\" + fileId + ".mp4").Length;
            var fileInfoSize = await m_client.GetFileAsync(fileId);
            var fileSizeBeforeCompression = fileInfoSize.FileSize;
            long bytesSaved = fileSizeBeforeCompression.Value - sizeStream;
            Message? compressionMessage = await m_client.SendTextMessageAsync(
                chatId: m_chatId,
                text: bytesSaved + " байт сохранено! " + emojiSet[m_random.Next(emojiSet.Count)],
                parseMode: ParseMode.Html,
                disableNotification: true,
                cancellationToken: m_cancellationToken);
        }

        internal async Task CompressVideoNoteAndReply(string savePath)
        {
            Message? middleMessage = await m_client.SendTextMessageAsync(
                chatId: m_chatId,
                text: "<b>Немного терпения... </b>" + emojiSet[m_random.Next(emojiSet.Count)],
                parseMode: ParseMode.Html,
                disableNotification: true,
                cancellationToken: m_cancellationToken);

            var fileId = m_update.Message?.VideoNote?.FileId;
            Directory.CreateDirectory(savePath + m_chatId);
            Directory.CreateDirectory(savePath + m_chatId + @"\Uncompress");
            Directory.CreateDirectory(savePath + m_chatId + @"\Compress");

            string dirPath = savePath + m_chatId + @"\Uncompress";
            string compressPath = savePath + m_chatId + @"\Compress\";

            await using FileStream fileStream = System.IO.File.OpenWrite(savePath + m_chatId + @"\Uncompress\" + fileId + ".mp4");
            var file = await m_client.GetInfoAndDownloadFileAsync(
                fileId: fileId,
                destination: fileStream);

            fileStream.Dispose();
            await Shakkal.CompressVideoFileAsync(@"\Media\" + m_chatId + @"\Uncompress\" + fileId + ".mp4", @"\Media\" + m_chatId + @"\Compress\" + fileId + ".mp4");

            await using Stream stream = System.IO.File.OpenRead(compressPath + @"\" + fileId + ".mp4");
            Message finalMessage = await m_client.SendVideoNoteAsync(
                chatId: m_chatId,
                videoNote: new InputOnlineFile(content: stream));
            stream.Dispose();

            long sizeStream = new System.IO.FileInfo(compressPath + @"\" + fileId + ".mp4").Length;
            var fileInfoSize = await m_client.GetFileAsync(fileId);
            var fileSizeBeforeCompression = fileInfoSize.FileSize;
            long bytesSaved = fileSizeBeforeCompression.Value - sizeStream;
            Message? compressionMessage = await m_client.SendTextMessageAsync(
                chatId: m_chatId,
                text: bytesSaved + " байт сохранено! " + emojiSet[m_random.Next(emojiSet.Count)],
                parseMode: ParseMode.Html,
                disableNotification: true,
                cancellationToken: m_cancellationToken);
        }
        internal async Task YoutubeVideoReply()
        {
            await YouTubeDLWrapper.DownloadVideo(m_chatId.ToString(), m_update.Message.Text);

            Message? message = await m_client.SendTextMessageAsync(
                chatId: m_chatId,
                text: m_update.Message.Text,
                parseMode: ParseMode.Html,
                disableNotification: true,
                cancellationToken: m_cancellationToken);
        }
    }
}
#pragma warning restore CS8602 // Dereference of a possibly null reference.
#pragma warning restore CS8604 // Possible null reference argument.
