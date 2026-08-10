using System;
using System.Collections.Generic;
using System.Linq;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Tools;

namespace TodaysMVPTool;

internal sealed class ModEntry : Mod
{
    private readonly Dictionary<ToolKind, ToolUsage> usageByTool = new();

    private DailyMvp pendingMvp;

    public override void Entry(IModHelper helper)
    {
        this.ResetCounters();

        helper.Events.GameLoop.SaveLoaded += this.OnSaveLoaded;
        helper.Events.GameLoop.DayStarted += this.OnDayStarted;
        helper.Events.GameLoop.DayEnding += this.OnDayEnding;
        helper.Events.GameLoop.ReturnedToTitle += this.OnReturnedToTitle;
        helper.Events.Display.MenuChanged += this.OnMenuChanged;
        helper.Events.Input.ButtonPressed += this.OnButtonPressed;
    }

    private void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
    {
        this.pendingMvp = null;
        this.ResetCounters();
    }

    private void OnDayStarted(object sender, DayStartedEventArgs e)
    {
        DailyMvp fallbackMvp = this.pendingMvp;
        this.pendingMvp = null;
        this.ResetCounters();

        if (fallbackMvp is null)
            return;

        MvpToolMenu menu = this.CreateMvpMenu(fallbackMvp);
        if (Game1.activeClickableMenu is null)
            Game1.activeClickableMenu = menu;
        else
            Game1.nextClickableMenu.Insert(0, menu);
    }

    private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
    {
        if (
            !Context.IsWorldReady
            || !Context.CanPlayerMove
            || !e.Button.IsUseToolButton()
            || this.Helper.Input.IsSuppressed(e.Button)
            || !TryGetToolKind(Game1.player.CurrentTool, out ToolKind kind)
        )
            return;

        ToolUsage usage = this.usageByTool[kind];
        usage.Count++;
        usage.LastTool = Game1.player.CurrentTool;
    }

    private void OnDayEnding(object sender, DayEndingEventArgs e)
    {
        ToolUsage winner = this.usageByTool.Values
            .Where(usage => usage.Count > 0)
            .OrderByDescending(usage => usage.Count)
            .FirstOrDefault();

        this.pendingMvp = winner is null
            ? null
            : new DailyMvp(winner.Kind, winner.Count, winner.LastTool);
    }

    private void OnMenuChanged(object sender, MenuChangedEventArgs e)
    {
        if (e.NewMenu is not ShippingMenu || this.pendingMvp is null)
            return;

        DailyMvp mvp = this.pendingMvp;
        this.pendingMvp = null;

        Game1.endOfNightMenus.Push(this.CreateMvpMenu(mvp));
    }

    private MvpToolMenu CreateMvpMenu(DailyMvp mvp)
    {
        return new MvpToolMenu(
            title: this.Helper.Translation.Get("menu.title"),
            toolName: this.Helper.Translation.Get($"tool.{GetTranslationKey(mvp.Kind)}"),
            usageText: this.Helper.Translation.Get("menu.used", new { count = mvp.Count }),
            okText: this.Helper.Translation.Get("menu.ok"),
            tool: mvp.Tool
        );
    }

    private void OnReturnedToTitle(object sender, ReturnedToTitleEventArgs e)
    {
        this.pendingMvp = null;
        this.ResetCounters();
    }

    private void ResetCounters()
    {
        this.usageByTool.Clear();
        foreach (ToolKind kind in Enum.GetValues<ToolKind>())
            this.usageByTool[kind] = new ToolUsage(kind);

    }

    private static bool TryGetToolKind(Tool tool, out ToolKind kind)
    {
        switch (tool)
        {
            case Pickaxe:
                kind = ToolKind.Pickaxe;
                return true;
            case Axe:
                kind = ToolKind.Axe;
                return true;
            case Hoe:
                kind = ToolKind.Hoe;
                return true;
            case WateringCan:
                kind = ToolKind.WateringCan;
                return true;
            case FishingRod:
                kind = ToolKind.FishingRod;
                return true;
            case MeleeWeapon weapon when weapon.isScythe():
                kind = ToolKind.Scythe;
                return true;
            default:
                kind = default;
                return false;
        }
    }

    private static string GetTranslationKey(ToolKind kind)
    {
        return kind switch
        {
            ToolKind.Pickaxe => "pickaxe",
            ToolKind.Axe => "axe",
            ToolKind.Hoe => "hoe",
            ToolKind.WateringCan => "watering-can",
            ToolKind.FishingRod => "fishing-rod",
            ToolKind.Scythe => "scythe",
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null)
        };
    }
}

internal enum ToolKind
{
    Pickaxe,
    Axe,
    Hoe,
    WateringCan,
    FishingRod,
    Scythe
}

internal sealed class ToolUsage
{
    public ToolUsage(ToolKind kind)
    {
        this.Kind = kind;
    }

    public ToolKind Kind { get; }
    public int Count { get; set; }
    public Tool LastTool { get; set; }
}

internal sealed class DailyMvp
{
    public DailyMvp(ToolKind kind, int count, Tool tool)
    {
        this.Kind = kind;
        this.Count = count;
        this.Tool = tool;
    }

    public ToolKind Kind { get; }
    public int Count { get; }
    public Tool Tool { get; }
}
