using CounterStrikeSharp.API.Core;

namespace GroupTagger;

public class CommandHandler(GroupTagger plugin) {
    private Dictionary<ulong, long> Delays = new();

    public void AddCommandDelay(CCSPlayerController player, int delay) {
        var steamId = player.SteamID;
        var currentTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        Delays.Add(steamId, currentTime + delay);
    }

    public bool HasCommandDelay(CCSPlayerController player) {
        var steamId = player.SteamID;
        var currentTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var result = Delays.GetValueOrDefault(steamId, 0) > currentTime;
        if (!result) Delays.Remove(steamId);
        return result;
    }

    public long GetCommandDelay(CCSPlayerController player) {
        var steamId = player.SteamID;
        return Delays.GetValueOrDefault(steamId, 0);
    }

    public void RegisterCommands() {
        plugin.AddCommand("css_ug", "", (player, _) => {
            if (player == null) return;
            if (HasCommandDelay(player)) {
                var remainTime = GetCommandDelay(player) - DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                var timeSpan = TimeSpan.FromMilliseconds(remainTime);
                var formattedTime = $"{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";
                var message = ColorManager.GetColoredText(
                    $"&4{GroupTagger.Prefix} &cПодождите &e{formattedTime}&c перед повторным использованием команды!");
                player.PrintToChat(message);
                return;
            }

            plugin.GroupHandler.UpdateGroup(player);
            AddCommandDelay(player, 1000 * 60 * 5);
        });

        plugin.AddCommand("css_vip_convert", "", (player, _) => {
            if (player != null) return;
            plugin.GroupHandler.UpdateGroup();
        });

        plugin.AddCommand("css_viplist_convert", "", (player, _) => {
            if (player != null) return;
            var grantedAdmins = plugin.GroupHandler.GetGrantedAdmins();
            if (grantedAdmins.Count == 0) {
                plugin.Logger.Print("No VIPs currently granted.");
                return;
            }

            var adminList = string.Join(", ", grantedAdmins.Select(steamId => steamId.ToString()));
            plugin.Logger.Print($"Granted Admins: {adminList}");
        });
    }
}