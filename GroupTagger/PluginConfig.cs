using System.Text.Json.Serialization;
using CounterStrikeSharp.API.Core;

namespace GroupTagger;

public class PluginConfig : BasePluginConfig {
    [JsonPropertyName("host")] public string host { get; set; } = "host";
    [JsonPropertyName("database")] public string database { get; set; } = "database";
    [JsonPropertyName("user")] public string user { get; set; } = "user";
    [JsonPropertyName("pass")] public string pass { get; set; } = "pass";
    [JsonPropertyName("port")] public string port { get; set; } = "3306";
    [JsonPropertyName("sid")] public int serverId { get; set; } = 2;
    [JsonPropertyName("debug")] public int debug { get; set; } = 1;
    [JsonPropertyName("chatPrefix")] public string chatPrefix { get; set; } = "&4[GroupTagger]";
    [JsonPropertyName("successMessage")] public string successMessage { get; set; } = "&aВаша группа была успешно обновлена!";
    [JsonPropertyName("denyMessage")] public string denyMessage { get; set; } = "&cПодождите &e%remain%&c перед повторным использованием команды!";
    [JsonPropertyName("removeexpired")] public int removeexpired { get; set; } = 1;

    [JsonPropertyName("ConvertVips")]
    public Dictionary<string, List<string>> ConvertVips { get; set; } = new() {
        {
            "VipGod", new List<string> {
                "@css/vipgod"
            }
        }, {
            "VipLite", new List<string> {
                "@css/viplite"
            }
        }, {
            "VipPremium", new List<string> {
                "@css/vipprem"
            }
        }
    };
}