using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Events;

namespace GroupTagger;

public class EventHandler(GroupTagger plugin) {
    public void RegisterEvents() {
        plugin.RegisterEventHandler<EventPlayerConnectFull>((@event, _) => {
            var user = GetPlayer(@event);
            plugin.GroupHandler.UpdateGroup(user);
            return HookResult.Continue;
        });

        plugin.RegisterEventHandler<EventPlayerSpawn>((@event, _) => {
            var user = GetPlayer(@event);
            plugin.GroupHandler.UpdateGroup(user);
            return HookResult.Continue;
        });

        plugin.RegisterEventHandler<EventRoundEnd>((_, _) => {
            plugin.GroupHandler.UpdateGroup();
            return HookResult.Continue;
        });
    }

    private CCSPlayerController? GetPlayer(GameEvent @event) {
        var ptr = NativeAPI.GetEventPlayerController(@event.Handle, "userid");
        return ptr == IntPtr.Zero ? null : new CCSPlayerController(ptr);
    }
}