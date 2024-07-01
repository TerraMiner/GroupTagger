using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Entities;

namespace GroupTagger;

public class GroupHandler(GroupTagger plugin) {
    public Dictionary<int, GroupData> Users = new();

    public async void RemoveAllExpired() {
        if (plugin.Config.removeexpired == 0) return;
        var sid = plugin.Config.serverId;
        var currentTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var sql = plugin.Database.GetQuery3(sid, currentTimestamp);
        await plugin.Database.Query(sql, _ => {});
    }
    
    public async void UpdatePlayer(SteamID steamId) {
        var accountId = plugin.Steam.GetAccountId(steamId);
        GroupData? group = null;

        var sid = plugin.Config.serverId;
        var currentTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var sql = plugin.Database.GetQuery1(accountId, sid, currentTimestamp);

        await plugin.Database.Query(sql, reader => {
            var tag = reader.GetString("group");
            group = new GroupData(steamId, tag, sid);
        });

        UpdateFlags(steamId, group);
    }

    public async Task UpdatePlayers(string accountIds) {
        var sid = plugin.Config.serverId;
        var currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var sql = plugin.Database.GetQuery2(sid, currentTime, accountIds);
        var actualAdmins = new Dictionary<int, GroupData>();
        await plugin.Database.Query(sql, reader => {
            var group = reader.GetString("group");
            var accId = reader.GetInt32("account_id");
            var steamId = plugin.Steam.GetSteamId(accId);
            actualAdmins.Add(accId, new GroupData(steamId, group, sid));
            plugin.Logger.Print($"{accId} is Actual with {group}");
        });
        UpdatePlayers(actualAdmins);
    }

    private void UpdatePlayers(Dictionary<int, GroupData> validAdmins) {
        var oldRemovedUsers = Users.Where(it => !validAdmins.ContainsKey(it.Key)).ToDictionary();
        var newNotAddedUsers = validAdmins.Where(it => !Users.ContainsKey(it.Key)).ToDictionary();

        foreach (var (accId, oldGroupData) in oldRemovedUsers) {
            Revoke(oldGroupData.steamId);
            Users.Remove(accId);
            plugin.Logger.Print($"Removed {accId}");
        }

        foreach (var (accId, newGroupData) in newNotAddedUsers) {
            Grant(newGroupData);
            Users.Add(accId, newGroupData);
            validAdmins.Remove(accId);
            plugin.Logger.Print($"Added {accId}");
        }

        foreach (var groupData in validAdmins.Values) {
            UpdateFlags(groupData.steamId, groupData);
        }
    }

    public void UpdateFlags(SteamID steamId, GroupData? groupData) {
        var accId = plugin.Steam.GetAccountId(steamId);

        if (groupData == null) {
            Revoke(steamId);
            Users.Remove(accId);
            plugin.Logger.Print($"GroupData {accId} is null");
            return;
        }

        if (!Users.TryGetValue(accId, out var oldGroup)) {
            Users[accId] = groupData;
        }

        if (oldGroup != null && (oldGroup.group == groupData.group || !IsPluginFlag(groupData.group))) return;

        if (oldGroup != null) Revoke(steamId);
        else Revoke(steamId);

        Grant(groupData);

        var oldGroupTag = oldGroup != null ? oldGroup.group : "null";
        plugin.Logger.Print($"Updated tag {oldGroupTag} to {groupData.group} for {accId}");
    }

    public void UpdatePlayers() {
        var players = Utilities.GetPlayers();
        if (players.Count == 0) return;
        Task.Run(async () => {
            var filtered = players.Where(it => it.SteamID != 0)
                .Select(it => plugin.Steam.GetAccountId(new SteamID(it.SteamID)))
                .ToList();

            var delimited = string.Join(",", filtered);
            await UpdatePlayers(delimited);
        });
    }

    public void UpdatePlayer(CCSPlayerController? user) {
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
        UpdatePlayer(steamId);
    }

    private void Grant(GroupData groupData) {
        plugin.Config.ConvertVips
            .Where(it => groupData.group.Contains(it.Key))
            .Select(it => it.Value)
            .ToList()
            .ForEach(it => {
                AdminManager.AddPlayerPermissions(groupData.steamId, it.ToArray());
                plugin.Logger.Print($"Granted tag {groupData.group} to {groupData.steamId.SteamId3}");
            });
    }

    private void Revoke(SteamID steamId) {
        var adminData = AdminManager.GetPlayerAdminData(steamId);
        if (adminData != null) {
            adminData.Flags.Values
                .SelectMany(flagSet => flagSet)
                .Where(IsPluginFlag)
                .ToList()
                .ForEach(it => AdminManager.RemovePlayerPermissions(steamId, it));
        }
        else {
            AdminManager.RemovePlayerPermissions(steamId);
        }
    }

    private bool IsPluginFlag(string flag) {
        return plugin.Config.ConvertVips.Values.Any(it => it.Contains(flag));
    }
}