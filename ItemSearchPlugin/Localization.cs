using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;

namespace ItemSearchPlugin
{
    internal class Loc
    {
        private static readonly string[] ApplicableLangCodes = ["de", "ja", "fr", "zh"];

        private static Dictionary<string, string> _localizationStrings = new Dictionary<string, string>();

        internal static void LoadLanguage(string langCode)
        {
            if (langCode.ToLower() == "en")
            {
                _localizationStrings = new Dictionary<string, string>();
                return;
            }

            using var s = Assembly.GetExecutingAssembly()
                .GetManifestResourceStream($"ItemSearchPlugin.Localization.{langCode}.json");

            if (s == null)
            {
                PluginLog.Error("Failed to find language file.");
                _localizationStrings = new Dictionary<string, string>();
                return;
            }

            using var sr = new StreamReader(s);
            var deserialized = JsonConvert.DeserializeObject<Dictionary<string, string>>(sr.ReadToEnd());
            _localizationStrings = deserialized ?? new Dictionary<string, string>();
        }

        internal static string Localize(string key, string fallbackValue)
        {
            try
            {
                return _localizationStrings[key];
            }
            catch
            {
                _localizationStrings[key] = fallbackValue;
                return fallbackValue;
            }
        }


        internal static void LoadDefaultLanguage()
        {
            try
            {
                var currentUiLang = CultureInfo.CurrentUICulture;
#if DEBUG
                PluginLog.Debug("Trying to set up Loc for culture {0}", currentUiLang.TwoLetterISOLanguageName);
#endif
                LoadLanguage(ApplicableLangCodes.Any(x => currentUiLang.TwoLetterISOLanguageName == x)
                    ? currentUiLang.TwoLetterISOLanguageName
                    : "en");
            }
            catch (Exception ex)
            {
                PluginLog.Error("Could not get language information. Setting up fallbacks. {0}", ex.ToString());
                LoadLanguage("en");
            }
        }

        internal static void ExportLoadedDictionary()
        {
            string json = JsonConvert.SerializeObject(_localizationStrings, Formatting.Indented);
            PluginLog.Debug(json);
        }
    }
}