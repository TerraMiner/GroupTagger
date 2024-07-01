using CounterStrikeSharp.API.Modules.Entities;

namespace GroupTagger;

public class GroupData {
    public SteamID steamId;
    public string group;
    public int serverId;

    public GroupData(SteamID steamId, string group, int serverId) {
        this.steamId = steamId;
        this.group = group;
        this.serverId = serverId;
    }
}