using Discord;

namespace Discord_Bot
{
    public struct Consts
    {
        public const string DateFormat = "dd.MM.yy HH:mm";

        public const string ProcessingMessage = "processing...";

        public const string DateFormatError = "error: wrong date format. use `dd.MM.yy HH:mm`.";

        public static readonly Color InfoColor = Color.Gold;

        public static readonly Color SuccessColor = Color.Green;

        public static readonly Color ErrorColor = Color.Red;
    }
}
