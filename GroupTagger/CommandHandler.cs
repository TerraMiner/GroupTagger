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
            var message = "";
            var chatPrefix = plugin.Config.chatPrefix;
            if (HasCommandDelay(player)) {
                var remainTime = GetCommandDelay(player) - DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                var timeSpan = TimeSpan.FromMilliseconds(remainTime);
                var formattedTime = $"{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";
                var denyMessage = plugin.Config.denyMessage.Replace("%remain%",$"{formattedTime}");
                message = ColorManager.GetColoredText(
                    $"{chatPrefix} {denyMessage}");
                player.PrintToChat(message);
                return;
            }
            var successMessage = plugin.Config.successMessage;
            message = ColorManager.GetColoredText($"{chatPrefix} {successMessage}");
            player.PrintToChat(message);

            plugin.GroupHandler.UpdatePlayer(player);
            AddCommandDelay(player, 5000);
        });

        plugin.AddCommand("css_vip_convert", "", (player, _) => {
            if (player != null) return;
            plugin.GroupHandler.UpdatePlayers();
            plugin.Logger.Print("Executed command css_vip_convert");
        });

        plugin.AddCommand("css_viplist_convert", "", (player, _) => {
            if (player != null) return;
            var grantedAdmins = plugin.GroupHandler.Users;
            if (grantedAdmins.Count == 0) {
                plugin.Logger.Print("No VIPs currently granted.");
                return;
            }

            var adminList = string.Join(", ", grantedAdmins.Select(steamId => steamId.ToString()));
            plugin.Logger.Print($"Granted Admins: {adminList}");
        });
    }
}