﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;
using image_bot.Models.Commands;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using image_bot.Controllers;
namespace image_bot.Models
{
    public class Bot
    {
        private static TelegramBotClient botClient;
        private static List<Command> commandsList;
        public static IReadOnlyList<Command> Commands => commandsList.AsReadOnly();

        public static async Task<TelegramBotClient> GetBotClientAsync()
        {
            if (botClient != null)
            {
                return botClient;
            }

            commandsList = new List<Command>();
            commandsList.Add(new StartCommand());
            //TODO: Add more commands

            botClient = new TelegramBotClient(AppSettings.Key);
            string hook = string.Format(AppSettings.Url, "api/message/update");
            Debug.WriteLine("Setting webhook on {0}", hook);
            await botClient.SetWebhookAsync(hook);
            Debug.WriteLine("Webhook OK.");
            return botClient;
        }
    }
}
