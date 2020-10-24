﻿using Microsoft.AspNetCore.WebUtilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace image_bot.Models.Commands
{
    public class ImageResizeCommand : Command
    {
        public override string Name => @"/resize";

        public override bool Contains(Message message)
        {
            if (message == null || message.Type != Telegram.Bot.Types.Enums.MessageType.Text)
                return false;

            return message.Text.Contains(this.Name);
        }

        public override async Task Execute(Message message, TelegramBotClient botClient)
        {
            var chatId = message.Chat.Id;
            HttpClient client = new HttpClient();
            string url = string.Format(AppSettings.Url, "api/user/get-status");
            var query = new Dictionary<string, string>
            {
                ["chatId"] = chatId.ToString()
            };

            var response = await client.GetAsync(QueryHelpers.AddQueryString(url, query));
            if (!response.IsSuccessStatusCode)
            {
                url = string.Format(AppSettings.Url, "api/image/create-request");
                await client.PostAsync(QueryHelpers.AddQueryString(url, query), null);
                await botClient.SendTextMessageAsync(chatId, "Please input parameters of the future image in format heightxwidth.", parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown);
                return;
            }
            string result = await response.Content.ReadAsStringAsync();
            ImageResizeStatus status = JsonConvert.DeserializeObject<ImageResizeStatus>(result);
            switch (status)
            {
                case ImageResizeStatus.AwaitingSize:
                    string height = message.Text.Split(':')[0];
                    string width = message.Text.Split(':')[1];
                    url = string.Format(AppSettings.Url, "api/image/set-parameters");
                    var data = new Dictionary<string, string>()
                    {
                        ["chatId"] = chatId.ToString(),
                        ["height"] = height,
                        ["width"] = width 
                    };
                    await client.PostAsync(QueryHelpers.AddQueryString(url, data), null);
                    await botClient.SendTextMessageAsync(chatId, "Good. Now send me your image.", parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown);
                    break;
            }
        }
    }
}
