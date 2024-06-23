using System.Text.RegularExpressions;

namespace GroupTagger;

public static class ColorManager {
    private static Dictionary<string, int> _colorMap = new() {
        { "&f", 1 },
        { "&4", 2 },
        { "&5", 3 },
        { "&2", 4 },
        { "&a", 5 },
        { "&g", 6 },
        { "&c", 7 },
        { "&7", 8 },
        { "&e", 9 },
        { "&r", 10 },
        { "&b", 11 },
        { "&1", 12 },
        { "&9", 13 },
        { "&d", 14 },
        { "&0", 15 },
        { "&6", 16 },
    };

    private const string Pattern = @"&[0-9a-fA-F]"; // Pattern to match & followed by a hex digit

    public static string GetColoredText(string message) {
        var replaced = Regex.Replace(message, Pattern, match => {
            var colorCode = match.Value.ToLower();
            return _colorMap.TryGetValue(colorCode, out var replacement)
                ? Convert.ToChar(replacement).ToString()
                : match.Value;
        });
        return $"\xA0{replaced}";
    }
}