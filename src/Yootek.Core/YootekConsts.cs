using Yootek.Debugging;

namespace Yootek
{
    public class YootekConsts
    {
        public const string LocalizationSourceName = "Yootek";

        public const string ConnectionStringName = "Default";

        public const bool MultiTenancyEnabled = true;


        /// <summary>
        /// Default pass phrase for SimpleStringCipher decrypt/encrypt operations
        /// </summary>
        public static readonly string DefaultPassPhrase =
            DebugHelper.IsDebug ? "gsKxGZ012HLL3MI5" : "a1cce4ec81c746bea7626e3dded62699";
    }
}
