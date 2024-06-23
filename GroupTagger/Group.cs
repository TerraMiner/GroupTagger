using CounterStrikeSharp.API.Modules.Entities;

namespace GroupTagger;

public class Group {
    public SteamID Steamid;
    public string VipFlags;
    public int Sid;

    public Group(SteamID steamId, string group, int sid) {
        Steamid = steamId;
        VipFlags = group;
        Sid = sid;
    }
}