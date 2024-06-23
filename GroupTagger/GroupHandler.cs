using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Entities;

namespace GroupTagger;

public class GroupHandler(GroupTagger plugin) {
    private readonly Dictionary<int, SteamID> GrantedAdmins = new();

    public Dictionary<int, SteamID> GetGrantedAdmins() {
        return GrantedAdmins;
    }

    public async Task SetFlagsToVips(SteamID steamId) {
        var accountId = plugin.Steam.GetAccountId(steamId);
        Group? group = null;

        var sid = plugin.Config.sid;
        long currentTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        var sql = plugin.Database.GetQuery1(accountId, sid, currentTimestamp);

        await plugin.Database.Query(sql, reader => {
            var accId = reader.GetInt32("account_id");
            var tag = reader.GetString("group");

            plugin.Logger.Print($"Put into list {accId}, {tag}, {sid}");
            group = new Group(steamId, tag, sid);
        });

        if (group == null) return;
        Server.NextFrame(() => { SetVipFlags(group); });
    }

    public async Task SetFlagsToVips(string accountIds) {
        var admins = new List<Group>();
        var sid = plugin.Config.sid;
        var currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var sql = plugin.Database.GetQuery2(sid, currentTime, accountIds);

        await plugin.Database.Query(sql, reader => {
            var accId = reader.GetInt32("account_id");
            var group = reader.GetString("group");

            plugin.Logger.Print($"Set vip flags to {accId}, {group}");
            admins.Add(new Group(plugin.Steam.GetSteamId(accId), group, sid));
        });

        Server.NextFrame(() => { SetVipFlags(admins); });
    }

    public void SetVipFlags(Group admin) {
        var adminData = AdminManager.GetPlayerAdminData(admin.Steamid);

        if (adminData != null) {
            adminData.Flags.Values
                .SelectMany(flagSet => flagSet)
                .Where(IsPluginFlag)
                .ToList()
                .ForEach(it => AdminManager.RemovePlayerPermissions(admin.Steamid, it));
        }
        else {
            AdminManager.RemovePlayerPermissions(admin.Steamid);
        }


        if (admin.Sid != plugin.Config.sid) return;

        var accountId = plugin.Steam.GetAccountId(admin.Steamid);
        if (!GrantedAdmins.ContainsKey(accountId)) {
            GrantedAdmins[accountId] = admin.Steamid;
        }

        plugin.Config.ConvertVips
            .Where(it => admin.VipFlags.Contains(it.Key))
            .Select(it => it.Value)
            .ToList()
            .ForEach(it => {
                AdminManager.AddPlayerPermissions(admin.Steamid, it.ToArray());
                plugin.Logger.Print($"Admin {admin.Steamid.SteamId3} converted to {string.Join(", ", it)}");
            });
    }

    public void SetVipFlags(List<Group> admins) {
        foreach (var admin in admins) {
            SetVipFlags(admin);
        }
    }

    public async Task ValidateAdminFlags(string accountIds) {
        var sid = plugin.Config.sid;
        var currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var sql = plugin.Database.GetQuery2(sid, currentTime, accountIds);
        var validAdmins = new HashSet<int>();
        await plugin.Database.Query(sql, reader => validAdmins.Add(reader.GetInt32("account_id")));
        ValidateAndRemoveFlags(validAdmins);
    }

    public async Task ValidateAdminFlags(SteamID steamId) {
        var accountId = plugin.Steam.GetAccountId(steamId);
        if (!GrantedAdmins.ContainsKey(accountId)) return;
        var sid = plugin.Config.sid;
        var time = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var sql = plugin.Database.GetQuery1(accountId, sid, time);
        var validAdmins = new HashSet<int>();
        await plugin.Database.Query(sql, reader => validAdmins.Add(reader.GetInt32("account_id")));
        ValidateAndRemoveFlags(validAdmins);
    }

    private void ValidateAndRemoveFlags(HashSet<int> validAdmins) {
        foreach (var (accountId, steamId) in GrantedAdmins) {
            if (!validAdmins.Contains(accountId)) {
                var adminData = AdminManager.GetPlayerAdminData(steamId);
                if (adminData != null) {
                    adminData.Flags
                        .SelectMany(it => it.Value)
                        .Where(IsPluginFlag)
                        .ToList()
                        .ForEach(it => AdminManager.RemovePlayerPermissions(steamId, it));
                }
                else {
                    AdminManager.RemovePlayerPermissions(steamId);
                }

                plugin.Logger.Print($"All flags removed for admin {steamId}");
            }
            else {
                AdminManager.RemovePlayerPermissions(steamId);
            }
        }

        foreach (var validAdmin in validAdmins) {
            GrantedAdmins.Remove(validAdmin);
        }
    }

    private bool IsPluginFlag(string flag) {
        return plugin.Config.ConvertVips.Values.Any(it => it.Contains(flag));
    }

    public void UpdateGroup() {
        var players = Utilities.GetPlayers();
        if (players.Count == 0) return;
        Task.Run(async () => {
            var filtered = players.Where(it => it.SteamID != 0)
                .Select(it => plugin.Steam.GetAccountId(new SteamID(it.SteamID)))
                .ToList();

            var delimited = string.Join(",", filtered);
            await ValidateAdminFlags(delimited);
            await SetFlagsToVips(delimited);
        });
    }

    public void UpdateGroup(SteamID steamId) {
        Task.Run(async () => {
            await ValidateAdminFlags(steamId);
            await SetFlagsToVips(steamId);
        });
    }

    public void UpdateGroup(CCSPlayerController? user) {
        if (user == null) {
            plugin.Logger.Print("Event or Event.Userid is null");
            return;
        }

        var userId = user?.SteamID;

        if (user == null || userId == null || userId == 0) {
            plugin.Logger.Print($"User {user?.PlayerName} is null or bot! SteamID: {userId}");
            return;
        }

        var steamId = new SteamID(userId.Value);
        plugin.GroupHandler.UpdateGroup(steamId);
    }
}