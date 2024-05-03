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

        RegisterEventHandler<EventGrenadeThrown>(OnGrenadeThrown);

        RegisterEventHandler<EventRoundPoststart>(OnRoundStarted);

        }

    [GameEventHandler]
    private HookResult OnRoundStarted(EventRoundPoststart @event, GameEventInfo info)
    {
        float squareRadius = 60f;
        float squareWidth = 0.5f;

        //de_ancient
        double[] x = { -584.000000, -392.000000, -328.000000, -456.000000, -520.000000, -192.000000, -256.000000, -352.000000, -448.000000, -512.000000 };
        double[] y = { -2288.000000, -2224.000000, -2288.000000, -2288.000000, -2224.000000, 1696.000000, 1728.000000, 1728.000000, 1728.000000, 1696.000000 };
        double[] z = { -140.797485, -140.255737, -140.255737, -140.255737, -140.255737, 44.000000, 44.000000, 44.000000, 44.000000, 44.000000 };

        for (int i = 0; i < x.Length; i++)
        {
            Vector centerPoint = new Vector((float)x[i], (float)y[i], (float)z[i]);

            Vector startPos = new Vector(centerPoint.X + squareRadius / 2, centerPoint.Y - squareRadius / 2, centerPoint.Z);
            Vector endPos = new Vector(centerPoint.X - squareRadius / 2, centerPoint.Y + squareRadius / 2, centerPoint.Z);

            BeamHelper.SquareBeam(startPos, endPos, squareRadius, squareWidth, Color.Blue);
        }

        return HookResult.Continue;
    }
}
