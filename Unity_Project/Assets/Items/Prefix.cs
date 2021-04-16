using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using TimefulDungeon.Misc;
using UnityEngine;

namespace TimefulDungeon.Items {
    public abstract class Prefix {
        private const float PrefixModifier = 0.2f;
        protected const BindingFlags Flags = BindingFlags.Default;
        private static readonly Regex UpDownRegex = new Regex(@"(.+?)(Up|Down)");

        public string value { get; protected set; }
        public string translatedValue { get; protected set; }

        public static implicit operator string(Prefix p) => p.translatedValue;   

        public void Apply(Equippable equippable) {
            var matches = UpDownRegex.Matches(value);
            foreach (Match match in matches) {
                var field = match.Groups[1].Value.ToLower();
                var up = match.Groups[2].Value == "Up";
                var mode = up ? ModifierMode.Add : ModifierMode.Subtract;
                equippable.AdjustModifier(field, PrefixModifier, mode);
            }
        }
    }

    [SuppressMessage("ReSharper", "StaticMemberInGenericType")]
    public class Prefix<T> : Prefix where T : Equippable {
        private static readonly List<string> Prefixes = new List<string>();
        private static string translationPath;

        static Prefix() {
            translationPath = $"{typeof(T).Name}Prefixes";
            GetPrefixesFromTranslations();
        }

        public Prefix() {
            value = GetRandomPrefix();
            translatedValue = Translations.GetAt(value, translationPath);
        }
        

        private static void GetPrefixesFromTranslations() {
            var keys = Translations.GetAllAt(translationPath).Keys;
            foreach (var key in keys) {
                Prefixes.Add(key);
            }
        }

        private static void GetPrefixesFromFields() {
            var fields = typeof(T).GetFields(Flags).Select(f => f.Name)
                .Where(f => f.Contains("Mod"));
            foreach (var field in fields) {
                Prefixes.Add(field + "Up");
                Prefixes.Add(field + "Down");
            }
        }

        private static string GetRandomPrefix() {
            return Prefixes[Random.Range(0, Prefixes.Count - 1)];
        }
    }
}