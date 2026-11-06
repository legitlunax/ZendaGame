using System;
using System.Globalization;
using System.Text;
using Newtonsoft.Json.Linq;

namespace Genesis
{
    // Presentation-only player identity. Never changes progression or story IDs.
    public sealed class GenesisProtagonistProfile
    {
        public const int MaximumNameLength = 24;
        private static readonly string[] LightNames = { "Aurelia", "Lumina", "Elara", "Serena", "Liora", "Celestia", "Alina", "Elysia" };
        private static readonly string[] DarkNames = { "Nyra", "Selene", "Vesper", "Noctia", "Ravenna", "Lyra", "Mirelle", "Umbra" };
        private readonly Random random = new Random(Guid.NewGuid().GetHashCode());
        public string Selected { get; private set; } = "light";
        public string LightName { get; private set; }
        public string DarknessName { get; private set; }
        public bool Confirmed { get; private set; }
        public string Name => NameFor(Selected);

        public GenesisProtagonistProfile()
        {
            LightName = SuggestName("light");
            DarknessName = SuggestName("darkness");
        }

        public static bool IsValid(string id) => id == "light" || id == "darkness";
        public string NameFor(string id) => id == "darkness" ? DarknessName : LightName;
        public string SuggestName(string id)
        {
            var names = id == "darkness" ? DarkNames : LightNames;
            return names[random.Next(names.Length)];
        }

        public static string CleanName(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return "";
            var text = new StringBuilder();
            foreach (char c in value.Trim())
                if (!char.IsControl(c) && c != '<' && c != '>' &&
                    CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.Format) text.Append(c);
            string result = text.ToString().Trim();
            var info = new StringInfo(result);
            return info.LengthInTextElements > MaximumNameLength ? info.SubstringByTextElements(0, MaximumNameLength) : result;
        }

        public bool Choose(string id, string name)
        {
            string clean = CleanName(name);
            if (!IsValid(id) || clean.Length == 0) return false;
            if (id == "darkness") DarknessName = clean;
            else LightName = clean;
            Selected = id;
            Confirmed = true;
            return true;
        }

        public JObject Save() => new JObject
        {
            ["selected"] = Selected, ["confirmed"] = Confirmed,
            ["lightName"] = LightName, ["darknessName"] = DarknessName
        };

        public void Restore(JObject data)
        {
            string light = CleanName((string)data?["lightName"]);
            string dark = CleanName((string)data?["darknessName"]);
            if (light.Length > 0) LightName = light;
            if (dark.Length > 0) DarknessName = dark;
            string selected = (string)data?["selected"];
            Selected = IsValid(selected) ? selected : "light";
            Confirmed = data != null && IsValid(selected) && (bool?)data["confirmed"] == true;
        }
    }
}