using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using BepInEx.Logging;
using HarmonyLib;

namespace LocalizationUtilities
{

    /// <summary>
    /// A utility to support English translations via ./Localization/SimpEn.csv files.
    /// Can generate new keys based on a hash,
    /// Output the keys to the BepInEx log.  Used for creating SimpEn.csv.
    /// Load ./Localization/SimpEn.csv text if the matching LocalizationKey is found in the file.
    /// </summary>
    /// <remarks>Version 1.2.0</remarks>
    /// <example>
    /// Init in the BepInExUnityPlugin::Awake before Harmony patch:
    /// 	private void Awake()
    /// <code>
    ///       LocalizationStringUtility.Init(
    ///           Config.Bind<bool>("Debug", "LogCardInfo", false, "If true, will output the localization keys for the cards. 如果为真，将输出卡片的本地化密钥。")
    ///               .Value,
    ///           Info.Location,
    ///           Logger
    ///       );
    /// </code>
    /// Call set for anything missing a localization key:
    /// <code>
    ///     cardData.CardDescription.DefaultText = 水果名称 + "榨成的果汁，可以喝。";
    ///     cardData.CardDescription.SetLocalizationInfo();
    /// </code>
    /// </example>
    public static class LocalizationStringUtility
    {
        /// <summary>        
        /// If true, will log the generated card's key and current DefaultText
        /// to the BepInEx log.
        /// </summary>
        private static bool LogCardInfo { get; set; }

        /// <summary>
        /// The mod's directory.  EG: Path.GetDirectoryName(Info.Location);
        /// </summary>
        private static string ModPath { get; set; }

        /// <summary>
        /// The BepInEx Logger to write errors to.
        /// </summary>
        private static ManualLogSource Logger { get; set; }

        private static SHA1 Sha1 = SHA1.Create();

        /// <summary>
        /// Lookup for DefaultText translations.
        /// </summary>
        private static Dictionary<string, string> Localization;

        /// <summary>
        /// The prefix to use when creating a key
        /// </summary>
        private static string KeyPrefix { get; set; }

        /// <summary>
        /// Initializes the Utility settings.
        /// </summary>
        /// <param name="logCardInfo"></param>
        /// <param name="dllPath"></param>
        /// <param name="logger"></param>
        public static void Init(bool logCardInfo, string dllPath, ManualLogSource logger, string keyPrefix = "T-")
        {
            LogCardInfo = logCardInfo;
            ModPath = Path.GetDirectoryName(dllPath);
            Logger = logger;
            KeyPrefix = keyPrefix;
        }

        /// <summary>
        /// Generates and sets a localization key based on a hash of the DefaultText.
        /// If there is a Localization file loaded, the DefaultText will be set to that key.
        /// </summary>
        /// <remarks>
        /// This is the same hash as used by CardSurvival-Localization as there can be cross mod usage.
        /// </remarks>
        /// <param name="localizedString"></param>
        public static void SetLocalizationInfo(this ref LocalizedString localizedString)
        {
            if(Localization == null) Localization = GetLocalizationLookup();
            if (String.IsNullOrEmpty(localizedString.DefaultText)) return;


            //----Set LocalizationKey
            string key = KeyPrefix + Convert.ToBase64String(Sha1.ComputeHash(
                UTF8Encoding.UTF8.GetBytes(localizedString.DefaultText.Trim())));

            localizedString.LocalizationKey = key;


            //----Set localized text if available.
            //If there is a Localization lookup loaded, replace the text.
            if(Localization.TryGetValue(key, out string localizedText))
            {
                localizedString.DefaultText = localizedText;
            }

            //Log the keys and current values.  Useful for creating SimpEn.csv entries for the dynamically created cards
            //  in this mod.
            if (LogCardInfo)
            {
                Logger.LogInfo($"{localizedString.LocalizationKey}|{localizedString.DefaultText}");
            }
        }
        
        /// <summary>
        /// Creates a localization lookup from ./Localization/SimpEn.csv if the game's language is English.
        /// </summary>
        /// <returns></returns>
        private static Dictionary<string,string> GetLocalizationLookup()
        {

            try
            {
                if (LocalizationManager.Instance.Languages[LocalizationManager.CurrentLanguage].LanguageName != "English")
                {
                    return new();
                }

                //Check for localization file
                string translationFile = Path.Combine(ModPath, "Localization/SimpEn.csv");

                if (!File.Exists(translationFile))
                {
                    return new();
                }

                Dictionary<string, List<string>> translations = CSVParser.LoadFromPath(translationFile);

                Dictionary<string, string> translationLookup = new();

                foreach (KeyValuePair<string, List<string>> translation in translations)
                {
                    string key = translation.Key;
                    string value = translation.Value[0];

                    //Overwrite if there are multiple entries
                    translationLookup[key] = value;
                }

                return translationLookup;
            }
            catch (Exception ex)
            {
                if(Logger is not null)
                {
                    Logger.LogError($"Error loading SimpEn.csv: {ex}");
                }

                return new();
            }
        }

    }
}
