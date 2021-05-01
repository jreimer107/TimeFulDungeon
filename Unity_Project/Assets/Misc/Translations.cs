using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace TimefulDungeon.Misc {
    public static class Translations {
        private static readonly Dictionary<string, string> TranslationsDict = new Dictionary<string, string>();

        private static readonly Regex KeyValueRegex = new Regex(@"""(.*)""\s*:\s*""(.*)""");
        private static readonly Regex GroupStartRegex = new Regex(@"""(.*)""\s*:\s*{?");
        private static readonly Regex GroupEndRegex = new Regex(@"}");

        private static readonly Stack<string> KeyPathStack = new Stack<string>();

        static Translations() {
            var currentLanguage = Get2LetterIsoCodeFromSystemLanguage().ToLower();
            Debug.Log("Current language: " + currentLanguage);
            var translationFiles = Resources.LoadAll<TextAsset>(currentLanguage);
            foreach (var translationFile in translationFiles) {
                KeyPathStack.Clear();
                KeyPathStack.Push("");
                Debug.Log("Found translation file");
                foreach (var line in Regex.Split(translationFile.text, @"\r\n|\r|\n")) {
                    ParseJsonLine(line);
                }
            }
        }

        public static string Get(string key) {
            TranslationsDict.TryGetValue(key, out var value);
            return value ?? key;
        }

        public static string GetAt(string key, string path) => Get($"{path}.{key}");

        public static Dictionary<string, string> GetAll(string search) {
            Debug.Log("Getting from " + search);
            return TranslationsDict
                .Where(x => x.Key.Contains(search))
                .ToDictionary(pair => pair.Key, pair => pair.Value);
        }

        public static Dictionary<string, string> GetAllAt(string path) {
            Debug.Log("Getting from " + path);
            var subset = new Dictionary<string, string>();
            var pathRegex = new Regex(path + @"\.(\w+)");
            foreach (var pair in TranslationsDict) {
                var match = pathRegex.Match(pair.Key);
                if (match.Success) {
                    subset.Add(match.Groups[1].Value, pair.Value);
                }
            }

            return subset;
            
            
            // return TranslationsDict
            //     .Where(x => x.Key.StartsWith(path))
            //     .ToDictionary(pair => pair.Key.Split(path)[1], pair => pair.Value);
        }
        
        /// <summary>
        ///     Gets the translation of an Enum key.
        ///     Prepends the type name of the enum to the value of the enum, and uses that as the key.
        /// </summary>
        /// <param name="key">An enum value.</param>
        /// <returns>A translation, or the key if there isn't one.</returns>
        public static string Get(Enum key) {
            return Get(key.GetType().Name + "." + key);
        }

        private static void ParseJsonLine(string line) {
            var match = KeyValueRegex.Match(line);
            if (match.Success) {
                TranslationsDict.Add(KeyPathStack.Peek() + match.Groups[1].Value, match.Groups[2].Value);
                Debug.Log("Adding translation: " + KeyPathStack.Peek() + match.Groups[1].Value + ": "+ match.Groups[2]);
            }
            else {
                match = GroupStartRegex.Match(line);
                if (match.Success) {
                    var newGroup = match.Groups[1].Value;
                    KeyPathStack.Push(KeyPathStack.Peek() + newGroup + ".");
                }
            }

            match = GroupEndRegex.Match(line);
            if (match.Success) {
                KeyPathStack.Pop();
            }
        }

        private static string Get2LetterIsoCodeFromSystemLanguage() {
            var lang = Application.systemLanguage;
            return lang switch {
                SystemLanguage.Afrikaans => "AF",
                SystemLanguage.Arabic => "AR",
                SystemLanguage.Basque => "EU",
                SystemLanguage.Belarusian => "BY",
                SystemLanguage.Bulgarian => "BG",
                SystemLanguage.Catalan => "CA",
                SystemLanguage.Chinese => "ZH",
                SystemLanguage.ChineseSimplified => "ZH",
                SystemLanguage.ChineseTraditional => "ZH",
                SystemLanguage.Czech => "CS",
                SystemLanguage.Danish => "DA",
                SystemLanguage.Dutch => "NL",
                SystemLanguage.English => "EN",
                SystemLanguage.Estonian => "ET",
                SystemLanguage.Faroese => "FO",
                SystemLanguage.Finnish => "FI",
                SystemLanguage.French => "FR",
                SystemLanguage.German => "DE",
                SystemLanguage.Greek => "EL",
                SystemLanguage.Hebrew => "IW",
                SystemLanguage.Hungarian => "HU",
                SystemLanguage.Icelandic => "IS",
                SystemLanguage.Indonesian => "IN",
                SystemLanguage.Italian => "IT",
                SystemLanguage.Japanese => "JA",
                SystemLanguage.Korean => "KO",
                SystemLanguage.Latvian => "LV",
                SystemLanguage.Lithuanian => "LT",
                SystemLanguage.Norwegian => "NO",
                SystemLanguage.Polish => "PL",
                SystemLanguage.Portuguese => "PT",
                SystemLanguage.Romanian => "RO",
                SystemLanguage.Russian => "RU",
                SystemLanguage.SerboCroatian => "SH",
                SystemLanguage.Slovak => "SK",
                SystemLanguage.Slovenian => "SL",
                SystemLanguage.Spanish => "ES",
                SystemLanguage.Swedish => "SV",
                SystemLanguage.Thai => "TH",
                SystemLanguage.Turkish => "TR",
                SystemLanguage.Ukrainian => "UK",
                SystemLanguage.Unknown => "EN",
                SystemLanguage.Vietnamese => "VI",
                _ => "EN"
            };
        }
    }
}