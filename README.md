# All Weapons Flea

A small server-side mod for **SPT 4.1.6** that removes flea-market restrictions from valid weapons while preserving SPT's normal price calculation and restrictions on ammunition, armor, and other non-weapon items.

Version: **1.0.1** | Author: **Arion** | License: **MIT**

[Download v1.0.1 for SPT 4.1.6](https://github.com/Arion-Siles/All-Weapons-Flea/raw/refs/heads/main/downloads/AllWeaponsFlea_SPT_4.1.6_v1.0.1.zip)

Tested in-game by Arion and confirmed working on SPT 4.1.6.

## Features

- Enables flea-market buying and selling for valid weapon templates.
- Removes weapon-specific custom flea blacklist entries and their direct category restrictions.
- Applies changes before SPT generates simulated seller offers.
- Preserves configured offer counts, with a minimum of one offer for each eligible weapon category.
- Uses SPT's normal flea pricing; no fixed prices or price multipliers are added.
- Makes changes in memory without editing SPT's original data files or directly editing player profiles.
- Requires no additional mods, configuration, API keys, or network access.

## Installation

1. Close the game and SPT server.
2. Download the release ZIP, not GitHub's automatically generated source archive or the publishing kit.
3. Extract the ZIP into your SPT installation folder and merge the included `SPT_Runtime` directory with the existing one.
4. Check that the DLL is located at:
   `SPT_Runtime/user/mods/AllWeaponsFlea/AllWeaponsFlea.dll`
5. Start the server, then start the game.
6. Search the flea for a previously restricted weapon, such as the DT MDR 7.62x51. Include player offers in your search filters.

On an unmodified SPT 4.1.6 database, the server message is:

```text
[All Weapons Flea] 158 weapons enabled; 26 BSG restrictions removed.
```

Counts may differ when other mods are installed. Examples of previously restricted weapons include the MDR 7.62, Mk-18, AXMC, and MCX Spear.

## Scope and compatibility

- Targets **SPT 4.1.6 only**. Other versions are not declared compatible.
- Covers usable weapons recognized by SPT, including modded weapons registered before this mod runs.
- Quest items, broken/internal items, and items without a valid price remain excluded by SPT's item validation.
- Seasonal restrictions remain active for event-specific items such as some flares.
- The normal level requirement to access the flea is unchanged.
- Additional item-level locks imposed by other mods are not disabled.
- Ammunition and armor restrictions, trader loyalty requirements, quests, and currency balances are not changed.
- Other flea configuration mods may override or conflict with these changes. SVM and The Blacklist are not dependencies.
- Fika compatibility has not been tested.

## Updating from the earlier package

Close the game and server. Remove the old `SPT_Runtime/user/mods/Arion-AllWeaponsFlea` folder before installing this package. Do not keep both copies installed.

## Uninstallation

Close the game and server, remove only `SPT_Runtime/user/mods/AllWeaponsFlea`, then restart both. No profile reset is required by this mod. Items already purchased remain in the player's inventory.

## Validation status

The Release build and automated checks passed against the official SPT 4.1.6 libraries and database:

- 158 valid weapons enabled, including 26 initially restricted templates.
- All selected weapons pass SPT's `IsItemValidRagfairItem` validation after the changes.
- 4,515 other templates retain their original flea-sale flag.
- Custom weapon restrictions, positive offer counts, and repeated application were checked.

**Arion has tested the mod in-game on SPT 4.1.6 and confirmed that it works.**

## Build from source

Install the .NET 10 SDK, then run from the repository root:

```sh
dotnet build src/AllWeaponsFlea.csproj -c Release
```

Output: `src/bin/Release/net10.0/AllWeaponsFlea.dll`.
Only the mod DLL belongs in the release; SPT supplies its own runtime libraries. Never bundle the build output directory wholesale.

For database checks, obtain the official server source at tag `4.1.6`, fetch its Git LFS assets, and run:

```sh
dotnet run --project tests/SmokeTests.csproj -c Release -- /path/to/SPT_Data
```

Use the actual path to `Libraries/SPTushonka.Server.Assets/SPT_Data` in that checkout.

Reference: https://github.com/SP-Tushonka/server-csharp/tree/4.1.6

## Privacy

This mod does not contain API keys, credentials, telemetry, update checks, or external network calls. Build debug symbols and local build paths are excluded from the release DLL.

## Changelog

### 1.0.1

- Standardized the download, installation folder, assembly, and server message names to AllWeaponsFlea.
- Translated the server message and documentation into English.
- Standardized the runtime metadata name to `AllWeaponsFlea`.
- Kept the credential-free package and excluded debug symbols and build paths.
- No gameplay logic changes from the initial private 1.0.0 build.

## Development

Developed with AI assistance. The source is included for review, and the validation limits are documented above.
