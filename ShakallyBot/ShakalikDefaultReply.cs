using Telegram.Bot;
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

        private string pathUncompress = String.Empty;
        private string pathCompress = String.Empty;

        public ShakalikDefaultReply(CancellationToken cancellationToken, ITelegramBotClient? client, long? chatId, Update update)
        {
            m_cancellationToken = cancellationToken;
            m_client = client;
            m_chatId = chatId;
            m_update = update;

            GenerateChatDirectories.Init(m_chatId);
            pathUncompress = ProjectDir.basePath + @"\Media\" + m_chatId + @"\Uncompress\";
            pathCompress = ProjectDir.basePath + @"\Media\" + m_chatId + @"\Compress\";
        }

        // Start and error reply
        internal async Task WelcomeReply()
        {
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
                text: $"Я тебя не понял {GenerateRandom.Emoji()}, пожалуйста, нажми на /start",
                parseMode: ParseMode.Html,
                disableNotification: true,
                cancellationToken: m_cancellationToken);
        }

        // Download photo

        internal async Task CompressPhotoAndReply()
        {
            _ = await m_client.SendTextMessageAsync(
                chatId: m_chatId,
                text: $"<b>Почти готово </b> {GenerateRandom.Emoji()}",
                parseMode: ParseMode.Html,
                disableNotification: true,
                cancellationToken: m_cancellationToken);


            var fileId = m_update.Message?.Photo?.Last().FileId;

            await DownloadFile.ByFileId(m_update, m_client, pathUncompress, fileId, ".jpg");
            await Shakkal.CompressAndSaveFileAsync(pathUncompress + fileId + ".jpg", pathCompress, m_update.Message.MessageId.ToString() + ".jpg", 4);
            await SendFile.ByFileName(pathCompress, m_client,m_chatId, m_update.Message.MessageId.ToString(), ".jpg");

            _ = await m_client.SendTextMessageAsync(
                    chatId: m_chatId,
                    text: $"Файл успешно {GenerateRandom.Emoji()}",
                    parseMode: ParseMode.Html,
                    disableNotification: true,
                    replyMarkup: new InlineKeyboardMarkup(
                        InlineKeyboardButton.WithCallbackData(
                            text: "Сжать еще раз",
                            callbackData: m_update.Message.MessageId.ToString())),
                    cancellationToken: m_cancellationToken);
        }
        /*
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
        */
    }
}
#pragma warning restore CS8602 // Dereference of a possibly null reference.
#pragma warning restore CS8604 // Possible null reference argument.
