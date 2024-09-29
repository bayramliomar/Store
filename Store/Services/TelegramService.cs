using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Store.Services
{
    public class TelegramService
    {
        private readonly TelegramBotClient _botClient;

        public TelegramService()
        {
            _botClient = new TelegramBotClient(ConfigurationManager.AppSettings["config:TelegramBotToken"]);
        }

        public async Task SendMessageAsync(long chatId, string message)
        {
            await _botClient.SendTextMessageAsync(chatId, message);
        }

        public async Task HandleUpdateAsync(Update update)
        {
            // Implement your logic to handle Telegram updates here
            // For example, you could check for specific commands or messages
        }
    }
}