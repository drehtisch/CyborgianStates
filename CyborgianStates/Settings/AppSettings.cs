using System;
using System.Globalization;

namespace CyborgianStates
{
    public class AppSettings
    {
        public const string VERSION = "v4.0.0-preview";
        public const int API_VERSION = 10;
        public const long API_REQUEST_INTERVAL = 6000000; //0,6 s
        public const long SEND_RECRUITMENTTELEGRAM_INTERVAL = 1800000000; //3 m
        public const long REQUEST_NEW_NATIONS_INTERVAL = 18000000000; //30 m 

        public string Contact { get; set; }
        public string DbConnectionString { get; set; }
        public char SeperatorChar { get; set; }
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