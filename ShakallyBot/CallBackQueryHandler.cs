using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Shakkaler;
using Telegram.Bot.Types.InputFiles;

namespace Shakalik
{
#pragma warning disable CS8604 // Possible null reference argument.
#pragma warning disable CS8602 // Dereference of a possibly null reference.
    internal class CallBackQueryHandler
    {
        private readonly ITelegramBotClient? m_client;
        private CancellationToken m_cancellationToken;
        private long? m_chatId = null;

        public CallBackQueryHandler(CancellationToken cancellationToken,ITelegramBotClient client)
        {
            m_client = client;
            m_cancellationToken = cancellationToken;
        }


        internal async Task BotOnCallbackQueryReceived(CallbackQuery? callbackQuery)
        {
            if(callbackQuery == null) { return; }

            await m_client.AnswerCallbackQueryAsync(
                callbackQueryId: callbackQuery.Id,
                text: $"Received");

            Console.WriteLine(callbackQuery.Data);

            await m_client.SendTextMessageAsync(
                chatId: callbackQuery.Message!.Chat.Id,
                text: $"Обработка файла, пожалйста {GenerateRandom.Phrase()}");

           await CompressPhotoAndReply(callbackQuery, callbackQuery.Data);
        }

        internal async Task CompressPhotoAndReply(CallbackQuery? callbackQuery, string fileName)
        {


            long? getValue = callbackQuery.Message!.Chat.Id;
            if (getValue.HasValue)
            {
                m_chatId = getValue.Value;
            } else
            {
                return;
            }

            string pathToMedia = ProjectDir.basePath + @"\Media\";
            await Shakkal.CompressAndSaveFileAsync(pathToMedia + m_chatId + @"\Compress\" + fileName + ".jpg", pathToMedia + m_chatId + @"\Compress\", fileName + ".jpg", 4);
            await SendFile.ByFileName(pathToMedia + m_chatId + @"\Compress\", m_client, m_chatId, fileName, ".jpg");

            _ = await m_client.SendTextMessageAsync(
                    chatId: m_chatId,
                    text: $"Файл успешно сжат {GenerateRandom.Emoji()}",
                    parseMode: ParseMode.Html,
                    disableNotification: true,
                    replyMarkup: new InlineKeyboardMarkup(
                        InlineKeyboardButton.WithCallbackData(
                            text: "Сжать еще раз",
                            callbackData: fileName)),
                    cancellationToken: m_cancellationToken);
        }
    }
#pragma warning restore CS8602 // Dereference of a possibly null reference.
#pragma warning restore CS8604 // Possible null reference argument.
}
