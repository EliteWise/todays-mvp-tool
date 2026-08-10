# Today's MVP Tool

A small SMAPI mod for Stardew Valley 1.6 which tracks the player's main tool
uses during the day and names the most-used one **MVP Tool of the Day**.

Supported tools:

- Pickaxe
- Axe
- Hoe
- Watering Can
- Fishing Rod
- Scythe (including its upgraded variants)

The result appears after the nightly earnings screen. If no supported tool was
used, no extra screen is shown. Counters reset at the start of every day.

## Build

Install Stardew Valley and SMAPI, then run:

```sh
dotnet build TodaysMVPTool.csproj
```

The project uses SMAPI events and Stardew Valley's native menu queue; it doesn't
use Harmony patches.
