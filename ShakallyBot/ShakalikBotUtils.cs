using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shakalik
{
    internal static class GenerateRandom
    {
        private static readonly Random _random = new Random();
        private static readonly List<string> m_emojiSet = new List<string>()
            {
                "🙃" , "😂", "🦸", "🥰", "🤗", "😪", "😫", "😛", "😤", "🤒", "🤢", "🥸",
                "🤖","👹","👽","🙉","❄️","🐗","🦊","🦄","🐈","🐏","🐧","👁️","👀","👀","👨🏾", "👨🏾","👨"
            };

        private static readonly List<string> m_phraseSet = new List<string>()
            {
                "Готово " , "Смотри как получилось ", "Лови ", "Я мертв внутри ", "Я тебя люблю ", "Бога нет ", "У меня болит душа ", "Ночь ",
                "Крыши частных секторов ", "Собаки воют ", "Со дворов ", "Лишь луна и солнце ", "Тлеют, тлеют ", "В темноте....", "Я иду ",
                "Куда? Туда где меня ждет любовь ","Я так её давно хотел ","Мама не ","Волнуйся за меня ","Малый ","Повзрослел ",
                "Надевай свои ","Колготки ","Мы пойдем гулять ","По алее с фонарями ","Мы не будем спать ","Под дождем и под снегом ❄️",
                "И любой зимой и летом ","Ты когда-нибудь уйдешь, но ","Я не думаю ","Об этом ","Твои глаза ","Две безмолвные луны ","Мои глаза ","В тебя так влюбленны👀",
                "Твои волосы пахнут ","Лучиком света ","Я бегу за тобой ","Как за горящей кометой ","Ты пахнешь как весна "
            };

        internal static string Emoji()
        {
            return m_emojiSet[_random.Next(m_emojiSet.Count)];
        }

        internal static string Phrase()
        {
            return m_phraseSet[_random.Next(m_emojiSet.Count)];
        }
    }

    internal static class GenerateChatDirectories
    {
        internal static void Init(long? chatId)
        {
            try
            {
                // create directories for the chat
                Directory.CreateDirectory(ProjectDir.basePath + chatId);
                Directory.CreateDirectory(ProjectDir.basePath + chatId + @"\Compress");
                Directory.CreateDirectory(ProjectDir.basePath + chatId + @"\Uncompress");
            }
            catch
            {
                throw new ArgumentException($"Error while creating chat directories at {ProjectDir.basePath + chatId} path");
            }
        }
    }
}
