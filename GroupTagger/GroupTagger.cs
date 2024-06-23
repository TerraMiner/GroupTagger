using CounterStrikeSharp.API.Core;

namespace GroupTagger;

public class GroupTagger : BasePlugin, IPluginConfig<PluginConfig> {
    public const string Prefix = "[GroupTagger]";

    private CommandHandler CommandHandler;
    private EventHandler EventHandler;
    public GroupHandler GroupHandler;
    public DatabaseHandler Database;
    public SteamHandler Steam;
    public Logger Logger;

    public override string ModuleName => "GroupTagger";
    public override string ModuleVersion => "1.0";
    public override string ModuleAuthor => "2Terra_Miner";

    public PluginConfig Config { get; set; }

    public void OnConfigParsed(PluginConfig config) {
        var server = "Server=" + config.host + ";";
        var dataBase = "Database=" + config.database + ";";
        var port = "port=" + config.port + ";";
        var userId = "User Id=" + config.user + ";";
        var password = "password=" + config.pass + ";";
        var address = server + dataBase + port + userId + password;
        Config = config;

        CommandHandler = new CommandHandler(this);
        Database = new DatabaseHandler(this);
        EventHandler = new EventHandler(this);
        GroupHandler = new GroupHandler(this);
        Steam = new SteamHandler(this);
        Logger = new Logger(this);

        Database.InitializeDatabaseAddress(address);
        EventHandler.RegisterEvents();
        CommandHandler.RegisterCommands();

        Logger.EnableTitle();
        Logger.Print("Debug messages are enabled!");
    }
}