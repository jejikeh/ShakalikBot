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
        private ITelegramBotClient? m_client;
        private CancellationToken m_cancellationToken;
        private Update? m_update;
        private long? m_chatId = null;
        private Random m_random = new();

        private static List<string> m_emojiSet = new List<string>()
            {
                "Готово 🙃" , "Смотри как получилось 😂", "Лови 🦸", "Я мертв внутри🤦‍", "Я тебя люблю 🤷‍", "Бога нет🥰", "У меня болит душа 🤗", "Ночь 😶‍🌫",
                "Крыши частных секторов 😪", "Собаки воют 😫", "Со дворов 😛", "Лишь луна и солнце 😤", "Тлеют, тлеют🤒", "В темноте....🤢", "Я иду 🥸",
                "Куда? Туда где меня ждет любовь ❤️‍","Я так её давно хотел 👨🏾","Мама не 👩‍","Волнуйся за меня ❤️‍","Малый 💋‍","Повзрослел 👨",
                "Надевай свои 🤖","Колготки 👹","Мы пойдем гулять 👽","По алее с фонарями 🙉","Мы не будем спать 🐻‍","Под дождем и под снегом ❄️",
                "И любой зимой и летом 🐗","Ты когда-нибудь уйдешь, но🦊","Я не думаю 🦄","Об этом🐈","Твои глаза 🐏","Две безмолвные луны 🐧","Мои глаза 👁","В тебя так влюбленны👀",
                "Твои волосы пахнут👀","Лучиком света 👩🏾‍","Я бегу за тобой ❤️‍","Как за горящей кометой 👨🏾","Ты пахнешь как весна 👨🏻‍"
            };


        public CallBackQueryHandler(CancellationToken cancellationToken,ITelegramBotClient client, Update update)
        {
            m_client = client;
            m_update = update;
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
                text: $"Обработка файла, пожалйста {m_emojiSet[m_random.Next(m_emojiSet.Count)]}");

           await CompressPhotoAndReply(@"C:\Users\jejik\Jejikeh\programming\.net\shakaly\ShakallikBot\ShakallyBot\Media\", callbackQuery, callbackQuery.Data);
        }

        internal async Task CompressPhotoAndReply(string savePath, CallbackQuery? callbackQuery, string fileName)
        {
            

            long? chId = callbackQuery.Message!.Chat.Id;
            long actualId;
            if (chId.HasValue)
            {
                actualId = chId.Value;
            }else
            {
                return;
            }

            await Shakkal.CompressAndSaveFileAsync(savePath + actualId + @"\Compress\" + fileName + ".jpg", savePath + actualId + @"\Compress\", fileName + ".jpg", m_random.Next(10));
            await using Stream stream = System.IO.File.OpenRead(savePath + actualId + @"\Compress\" + fileName + ".jpg");
            Message finalMessage = await m_client.SendPhotoAsync(
                chatId: actualId,
                photo: new InputOnlineFile(content: stream));
            stream.Dispose();

            Message message = await m_client.SendTextMessageAsync(
                    chatId: actualId,
                    text: m_emojiSet[m_random.Next(m_emojiSet.Count)],
                    parseMode: ParseMode.Html,
                    disableNotification: true,
                    replyToMessageId: finalMessage.MessageId,
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
