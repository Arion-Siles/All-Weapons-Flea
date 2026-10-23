using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Arion.AllWeaponsFlea;
using SPTarkov.Server.Core.Helpers.Items;
using SPTarkov.Server.Core.Helpers.Profile;
using SPTarkov.Server.Core.Helpers.Ragfair;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Models.Spt.Tables;
using SPTarkov.Server.Core.Services.Items;
using SPTarkov.Server.Core.Utils;
using SPTarkov.Server.Core.Utils.Json;
using SPTarkov.Server.Core.Utils.Cloners;

if (args.Length != 1) throw new Exception("Pass SPT_Data path from SPT 4.1.6 source checkout.");
var root = args[0];
var json = new JsonUtil([new SptJsonConverterRegistrator()]);
var options = new JsonSerializerOptions(JsonUtil.JsonSerializerOptionsNoIndent!);
// The harness needs only three TemplateTable fields; production receives the full table from SPT.
options.TypeInfoResolver = new DefaultJsonTypeInfoResolver
{
    Modifiers = { info => { if (info.Type == typeof(TemplateTable)) foreach(var p in info.Properties) p.IsRequired = false; } }
};
string Read(string relative) => File.ReadAllText(Path.Combine(root, relative));
var templates = JsonSerializer.Deserialize<TemplateTable>("{\"items\":" + Read("database/templates/items.json")
    + ",\"handbook\":" + Read("database/templates/handbook.json")
    + ",\"prices\":" + Read("database/templates/prices.json") + "}", options)!;
var ragfair = json.Deserialize<RagfairConfig>(Read("configs/ragfair.json"))!;
var itemConfig = json.Deserialize<ItemConfig>(Read("configs/item.json"))!;
var cloner = new SPTarkov.Server.Core.Utils.Cloners.FastCloner();
var baseClasses = new ItemBaseClassService(null!, templates, null!);
baseClasses.HydrateItemBaseClassCache();
var handbook = new HandbookHelper(null!, templates, itemConfig, cloner);
var filter = new ItemFilterService(itemConfig);
var helper = new ItemHelper(null!, templates, null!, handbook, baseClasses, filter, null!, null!, cloner);
var fleaHelper = new RagfairServerHelper(null!, null!, null!, null!, helper, null!, null!, null!, ragfair, cloner);
var weapons = templates.Items.Values.Where(i => i.Type == "Item" && helper.IsOfBaseclass(i.Id, BaseClasses.WEAPON) && helper.IsValidItem(i)).ToList();
var weaponIds = weapons.Select(i => i.Id).ToHashSet();
var others = templates.Items.Values.Where(i => !weaponIds.Contains(i.Id)).ToDictionary(i => i.Id, i => i.Properties?.CanSellOnRagfair);
var blocked = weapons.Where(i => i.Properties?.CanSellOnRagfair != true).ToList();
void Assert(bool condition, string message) { if (!condition) throw new Exception(message); }
Assert(blocked.Count > 0, "Database must contain restricted weapons.");
var mdr = weapons.Single(i => i.Name == "weapon_dt_mdr_762x51");
Assert(blocked.Contains(mdr), "MDR 7.62 must initially be restricted.");
// Simulate a custom flea restriction and a zero-offer category as well.
ragfair.Dynamic.Blacklist.Custom.Add(mdr.Id);
ragfair.Dynamic.Blacklist.CustomItemCategoryList.Add(mdr.Parent);
ragfair.Dynamic.OfferItemCount[mdr.Parent.ToString()] = new(0, 0);
var mod = new Mod(templates, ragfair, helper);
await mod.OnLoadAsync();
foreach (var weapon in weapons)
{
    Assert(weapon.Properties?.CanSellOnRagfair == true, "Weapon remains restricted: " + weapon.Name);
    Assert(fleaHelper.IsItemValidRagfairItem(new(true, weapon)), "SPT flea rejected: " + weapon.Name);
    Assert(ragfair.Dynamic.OfferItemCount[weapon.Parent.ToString()].Min >= 1, "Zero offers.");
}
foreach (var (id, before) in others) Assert(templates.Items[id].Properties?.CanSellOnRagfair == before, "Non-weapon modified.");
Assert(ragfair.Dynamic.Blacklist.EnableBsgList, "Global blacklist was disabled.");
Assert(Mod.Unlock(templates, ragfair, weaponIds) == 0, "Not idempotent.");
Console.WriteLine($"PASS: {weapons.Count} valid weapons, {blocked.Count} formerly restricted; {others.Count} other templates unchanged. MDR 7.62 passes real SPT flea validation. Custom restrictions, positive offer counts and idempotence verified.");
foreach (var i in blocked) Console.WriteLine($"UNLOCKED {i.Id} {i.Name}");
