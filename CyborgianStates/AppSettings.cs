using System;
using System.Globalization;

namespace CyborgianStates
{
    public class AppSettings
    {
        public const string VERSION = "v4.0.0";
        public const int API_VERSION = 10;
        public const long API_REQUEST_INTERVAL = 6000000; //0,6 s
        public const long SEND_NON_RECRUITMENTTELEGRAM_INTERVAL = 300000000; //30 s
        public const long SEND_RECRUITMENTTELEGRAM_INTERVAL = 1800000000; //3 m
        public const long REQUEST_NEW_NATIONS_INTERVAL = 18000000000; //30 m 
        public const long REQUEST_REGION_NATIONS_INTERVAL = 432000000000; //12 h 

        public string ClientKey { get; set; }
        public string TelegramId { get; set; }
        public string TelegramSecretKey { get; set; }
        public string Contact { get; set; }
        public string DbConnection { get; set; }
        public string DiscordBotLoginToken { get; set; }
        public ulong DiscordBotAdminUser { get; set; }
        public string NationStatesRegionName { get; set; }
        public char SeperatorChar { get; set; }
        public bool CriteriaCheckOnNations { get; set; }
        public bool EnableRecruitment { get; set; }
        public int MinimumRecruitmentPoolSize { get; set; }
        public string RegionsToRecruitFrom { get; set; }
        public string LocaleString { get; set; }
        public CultureInfo Locale
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(LocaleString))
                {
                    return new CultureInfo(LocaleString);
                }
                else
                {
                    return CultureInfo.InvariantCulture;
                }
            }
        }
        public static string Configuration
        {
            get
            {
#if RELEASE
                return "production";
#elif DEBUG
                return "development";
#endif
            }
        }
    }
}