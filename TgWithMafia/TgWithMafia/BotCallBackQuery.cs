using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types;
using Telegram.Bot;

namespace TgWithMafia
{
    internal class BotCallBackQuery
    {
        private static BotCallBackQuery _instance;
        public readonly Dictionary<long, long> _operationMap;

        public BotCallBackQuery()
        {
            _operationMap = new Dictionary<long, long>();
        }
        public Dictionary<long, long> getOperationMap() { return _operationMap; }
        public static BotCallBackQuery Instance
        {
            get
            {
                if (_instance == null)
                {
                    throw new InvalidOperationException("Instance not initialized. Call Initialize method first.");
                }

                return _instance;
            }
        }

        public static void Initialize()
        {
            if (_instance == null)
            {
                _instance = new BotCallBackQuery();
            }
        }

        public async Task HandleCallbackQuery(CallbackQuery e)
        {
            var userId = e.From.Id;
            var buttonUserId = GetUserIdFromButton(e.Data);

            RecordOperation(userId, buttonUserId);

            await BotCommandsHandler.NotifyUser(userId, $"Вы выбрали: {e.Data}\nПо окончанию этапа ваш голос / навык будет использован на него");
        }
        public async Task DisplayMenuAsync(long userId, Dictionary<long, string> otherPlayers)
        {
            try
            {
                var buttons = otherPlayers.Select(kvp =>
                    new InlineKeyboardButton($"{kvp.Value}")
                    {
                        Text = kvp.Value,
                        CallbackData = kvp.Key.ToString()
                    }
                ).ToList();

                buttons.Add(new InlineKeyboardButton("Reset")
                {
                    Text = "Reset",
                    CallbackData = "0"
                });

                var inlineKeyboard = new InlineKeyboardMarkup(buttons);

                await BotCommandsHandler.NotifyUserWithInlineKeyboard(userId, "Select a player:", inlineKeyboard);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error during menu display: " + ex.Message);
            }
        }
        private void RecordOperation(long userId, long buttonUserId)
        {
            if (_operationMap.ContainsKey(userId))
            {
                if (buttonUserId == 0)
                    _operationMap.Remove(userId);
                else
                    _operationMap[userId] = buttonUserId;
            }
            else
            {
                _operationMap.Add(userId, buttonUserId);
            }
        }

        private long GetUserIdFromButton(string buttonText)
        {
            return long.Parse(buttonText);
        }

        public void PrintOperationMap()
        {
            foreach (var entry in _operationMap)
            {
                Console.WriteLine($"User ID: {entry.Key}, Button User ID: {entry.Value}");
            }
        }
    }
}
