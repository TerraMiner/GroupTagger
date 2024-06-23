using CounterStrikeSharp.API.Modules.Entities;

namespace GroupTagger;

public class SteamHandler(GroupTagger plugin) {
    public int GetAccountId(SteamID steamId) {
        var steamIdStr = steamId.ToString();
        var parts = steamIdStr.Split(',');
        var lastPart = parts.Last().Replace("[U:1:", "").Replace("]", "").Trim();

        return int.TryParse(lastPart, out var accountId) ? accountId : 0;
    }

    public SteamID GetSteamId(int steamId) {
        return new SteamID($"[U:1:{steamId}]");
    }
}