using System.Globalization;
using System.Runtime.CompilerServices;
using CyborgianStates.Enums;

[assembly: InternalsVisibleTo("CyborgianStates.Tests")]

namespace CyborgianStates
{

    public class AppSettings
    {
        public const long API_REQUEST_INTERVAL = 6000000; //0,6 s
        public const int API_VERSION = 10;
        public const long REQUEST_NEW_NATIONS_INTERVAL = 18000000000; //30 m
        public const long SEND_RECRUITMENTTELEGRAM_INTERVAL = 1800000000; //3 m
        public const string VERSION = "v4.0.0-preview-1";

        static internal bool IsTesting = false;
        private static string config = "development";

        public string Footer => $"CyborgianStates {VERSION} by drehtisch · See {SeperatorChar}about";

        public static string Configuration
        {
            get
            {
                if (IsTesting)
                {
                    return config;
                }
                else
                {
#if RELEASE
                    return "production";
#elif DEBUG
                    return "development";
                }
#endif
            }
            internal set
            {
                config = value;
            }
        }

        public string Contact { get; set; }
        public string DbConnection { get; set; }
        public ulong ExternalAdminUserId { get; set; }
        public string DiscordBotLoginToken { get; set; }
        public InputChannel InputChannel { get; set; }

        public CultureInfo CultureInfo
        {
            get
            {
                return !string.IsNullOrWhiteSpace(Locale) ? new CultureInfo(Locale) : CultureInfo.InvariantCulture;
            }
        }

        public string Locale { get; set; }
        public char SeperatorChar { get; set; }
    }
}