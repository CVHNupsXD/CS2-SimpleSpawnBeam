using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Memory;
using CounterStrikeSharp.API.Modules.Utils;
using CounterStrikeSharp.API.Modules.Commands;
using Microsoft.Extensions.Logging;
using System.Drawing;
using CBeamHelper;

namespace SpawnBeam;

[MinimumApiVersion(160)]
public class SpawnBeam : BasePlugin
{
    public override string ModuleName => "SpawnBeam";
    public override string ModuleDescription => "";
    public override string ModuleAuthor => "CVHNups";
    public override string ModuleVersion => "1.0.0";

    public static void DebugLog(string message)
    {
        Server.PrintToChatAll($"{ChatColors.Red}[Debug]{ChatColors.Default} {message}");
    }
    public override void Load(bool hotReload)
    {
        Console.WriteLine($"{ModuleName} loaded successfully!");

        RegisterEventHandler<EventRoundPoststart>(OnRoundStarted);

        }

    [GameEventHandler]
    private HookResult OnRoundStarted(EventRoundPoststart @event, GameEventInfo info)
    {
        float squareRadius = 60f;
        float squareWidth = 0.5f;

        var spawnConfigs = new[]
        {
            ("info_player_terrorist", Color.YellowGreen),
            ("info_player_counterterrorist", Color.DarkBlue)
        };

        foreach (var (designerName, color) in spawnConfigs)
        {
            var spawns = Utilities.FindAllEntitiesByDesignerName<SpawnPoint>(designerName);
            foreach (var spawn in spawns)
            {
                Vector centerPoint = spawn.AbsOrigin;
                Vector startPos = new Vector(centerPoint.X + squareRadius / 2, centerPoint.Y - squareRadius / 2, centerPoint.Z);
                Vector endPos = new Vector(centerPoint.X - squareRadius / 2, centerPoint.Y + squareRadius / 2, centerPoint.Z);
                BeamHelper.SquareBeam(startPos, endPos, squareRadius, squareWidth, color);
            }
        }

        return HookResult.Continue;
    }
}
