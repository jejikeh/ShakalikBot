﻿using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Shakalik {
    class Program
    {
        private static TelegramBotClient m_client = new(PrivateTokenBot.PrivateToken);
        static void Main()
        {
            var cts = new CancellationTokenSource();
            ReceiverOptions receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = new UpdateType[]
                {
                    UpdateType.Message,
                    UpdateType.EditedMessage,
                    UpdateType.CallbackQuery
                }
            };
            m_client.StartReceiving(
                updateHandler: UpdateHandler,
                pollingErrorHandler: HandlePollingErrorAsync,
                receiverOptions: receiverOptions
            );
            Console.ReadLine();
            cts.Cancel();
        }


        private static async Task UpdateHandler(ITelegramBotClient client, Update update, CancellationToken cancellationToken)
        {
            
            ShakalikDefaultReply defaultReply = new(cancellationToken, client, update.Message?.Chat.Id, update);
            CallBackQueryHandler defaultCallBackQuery = new(cancellationToken,client, update);

            if (update.Message?.Type == MessageType.Text)
            {
                if(update.Message?.Text == "/start")
                {
                    await defaultReply.WelcomeReply();
                }
                else
                {
                    await defaultReply.ErrorReply();
                }
            } else if(update.Message?.Type == MessageType.Photo)
            {
                await defaultReply.CompressPhotoAndReply(@"C:\Users\jejik\Jejikeh\programming\.net\shakaly\ShakallikBot\ShakallyBot\Media\");
                await client.DeleteMessageAsync(update.Message.Chat.Id, update.Message.MessageId);
                
            }else if (update.Type == UpdateType.CallbackQuery)
            {
                await defaultCallBackQuery.BotOnCallbackQueryReceived(callbackQuery: update.CallbackQuery);
            }
            Console.WriteLine(
                $"{update.Message?.From?.FirstName} sent message {update.Message?.MessageId} " +
                $"to chat {update.Message?.Chat.Id} at {update.Message?.Date}. "
            );


        }
        private static Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException
                    => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            Console.WriteLine(ErrorMessage);
            return Task.CompletedTask;
        }
    }
}