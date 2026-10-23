using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Helpers.Items;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Models.Spt.Mod;
using SPTarkov.Server.Core.Models.Spt.Tables;
using SPTarkov.Server.Core.Servers;
using Range = SemanticVersioning.Range;
using Version = SemanticVersioning.Version;

namespace Arion.AllWeaponsFlea;

public sealed class Metadata : IModMetadata
{
    public string ModGuid { get; init; } = "com.arion.allweaponsflea";
    public string Name { get; init; } = "AllWeaponsFlea";
    public string Author { get; init; } = "Arion";
    public List<string>? Contributors { get; init; } = [];
    public Version Version { get; init; } = new("1.0.1");
    public Range SptVersion { get; init; } = new("=4.1.6");
    public bool HasPrepatcher { get; init; } = false;
    public List<string>? Incompatibilities { get; init; } = [];
    public Dictionary<string, Range>? ModDependencies { get; init; } = [];
    public string? Url { get; init; } = null;
    public string License { get; init; } = "MIT";
}

// Run after trader and preset registration, but BEFORE flea offer generation.
[Injectable(TypePriority = OnLoadOrder.RagfairCallbacks - 1)]
public sealed class Mod(TemplateTable templates, RagfairConfig ragfair, ItemHelper items) : IOnLoad
{
    public Task OnLoadAsync(CancellationToken cancellationToken = default)
    {
        var weapons = templates.Items.Values
            .Where(item => string.Equals(item.Type, "Item", StringComparison.OrdinalIgnoreCase)
                && items.IsOfBaseclass(item.Id, BaseClasses.WEAPON)
                && items.IsValidItem(item))
            .Select(item => item.Id).ToHashSet();

        var changed = Unlock(templates, ragfair, weapons);
        Console.WriteLine($"[Arion All Weapons Flea] {weapons.Count} weapons enabled; {changed} BSG restrictions removed.");
        return Task.CompletedTask;
    }

    public static int Unlock(TemplateTable templates, RagfairConfig ragfair, HashSet<MongoId> weapons)
    {
        int changed = 0;
        foreach (var id in weapons)
        {
            var item = templates.Items[id];
            if (item.Properties is null) continue;
            if (item.Properties.CanSellOnRagfair != true) changed++;
            item.Properties.CanSellOnRagfair = true;
            ragfair.Dynamic.Blacklist.Custom.Remove(id);
            ragfair.Dynamic.Blacklist.CustomItemCategoryList.Remove(item.Parent);

            // Keep existing offer counts; ensure a weapon category cannot generate zero offers.
            var key = item.Parent.ToString();
            var current = ragfair.Dynamic.OfferItemCount.GetValueOrDefault(key)
                ?? ragfair.Dynamic.OfferItemCount.GetValueOrDefault("default");
            int min = Math.Max(1, current?.Min ?? 1);
            int max = Math.Max(min, current?.Max ?? 3);
            ragfair.Dynamic.OfferItemCount[key] = new MinMax<int>(min, max);
        }
        return changed;
    }
}
