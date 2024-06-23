namespace GroupTagger;

public class Logger(GroupTagger plugin) {
    public void Print(object value) {
        if (plugin.Config.debug == 1) {
            Console.WriteLine($"{GroupTagger.Prefix} {value}");
        }
    }

    public void EnableTitle() {
        Console.WriteLine($"{"",45}#############################################");
        Console.WriteLine($"{"",83}GroupTagger Loaded");
        Console.WriteLine();
        Console.WriteLine($"{"",75}Author:                         Terra_Miner");
        Console.WriteLine($"{"",75}Discord:                        2terra_miner");
        Console.WriteLine($"{"",75}Github:                         TerraMiner");
        Console.WriteLine($"{"",45}#############################################");
    }
}