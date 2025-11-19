using System.Drawing;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Memory;
using CounterStrikeSharp.API.Modules.Utils;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Timers;

using CS2TraceRay.Class;
using CS2TraceRay.Enum;
using CS2TraceRay.Struct;
using SpawnBeam.Extensions;

using Vector = CounterStrikeSharp.API.Modules.Utils.Vector;


namespace SpawnBeam;

[MinimumApiVersion(160)]
public class SpawnBeam : BasePlugin
{
    public override string ModuleName => "SpawnBeam";
    public override string ModuleDescription => "A plugin that displays visual square outlines around spawn points for both teams.";
    public override string ModuleAuthor => "CVHNups";
    public override string ModuleVersion => "1.0.3";

    private List<SpawnPoint> tSpawns = new List<SpawnPoint>();
    private List<SpawnPoint> ctSpawns = new List<SpawnPoint>();
    private Dictionary<int, bool> isPlayerTP = new Dictionary<int, bool>();
    private Dictionary<int, bool> isInSquare = new Dictionary<int, bool>();

    private CounterStrikeSharp.API.Modules.Timers.Timer? tpTimer;

    float squareRadius = 60f;
    float squareWidth = 0.5f;

    // TODO
    bool isBeamDisable = false;

    public override void Load(bool hotReload)
    {
        Console.WriteLine($"{ModuleName} loaded successfully!");

        RegisterEventHandler<EventRoundPoststart>(OnRoundStarted);
        RegisterEventHandler<EventPlayerDisconnect>(OnPlayerDisconnect);
        RegisterEventHandler<EventPlayerSpawn>(OnPlayerSpawn);

        AddCommand("css_tp", "Toggle auto teleport to spawn", Command_Tp);
        AddCommand("css_random", "Teleport to random spawn point", Command_TpRandom);

        AddCommandListener("noclip", OnNoclipCommand);

        tpTimer = AddTimer(0.5f, CheckPlayerPositions, TimerFlags.REPEAT);
    }

    public override void Unload(bool hotReload)
    {
        tpTimer?.Kill();
    }

    private void CheckPlayerPositions()
    {
        var players = Utilities.GetPlayers().Where(p =>
            p.IsValid &&
            p.PlayerPawn.Value != null &&
            isPlayerTP.ContainsKey((int)p.Index) &&
            isPlayerTP[(int)p.Index]);

        foreach (var player in players)
        {
            bool isNoClip = player.PlayerPawn.Value.MoveType == MoveType_t.MOVETYPE_NOCLIP;
            if (isNoClip)
            {
                continue;
            }

            int index = (int)player.Index;
            var playerPos = player.PlayerPawn.Value.AbsOrigin;
            var nearbySpawn = FindNearbySpawn(playerPos);

            bool currentlyInSquare = nearbySpawn != null;
            bool wasInSquare = isInSquare.ContainsKey(index) && isInSquare[index];

            if (currentlyInSquare && !wasInSquare)
            {
                Vector tpPos = new Vector(nearbySpawn.AbsOrigin.X, nearbySpawn.AbsOrigin.Y, playerPos.Z);
                if (player.PlayerPawn.Value.AbsOrigin.X != tpPos.X && player.PlayerPawn.Value.AbsOrigin.Y != tpPos.Y)
                {
                    player.PlayerPawn.Value.Teleport(tpPos, null, new Vector(0, 0, 0));
                    player.PrintToChat($" [Spawn{ChatColors.Lime}Beam{ChatColors.Default}] Teleported to spawn point!");
                }
                isInSquare[index] = currentlyInSquare;
            }
            else if (!currentlyInSquare && wasInSquare)
            {
                isInSquare.Remove(index);
            }
        }
    }

    private SpawnPoint? FindNearbySpawn(Vector playerPos)
    {
        foreach (var spawn in tSpawns.Concat(ctSpawns))
        {
            if (IsInsideSquare(playerPos, spawn.AbsOrigin, squareRadius))
                return spawn;
        }
        return null;
    }

    private bool IsInsideSquare(Vector point, Vector center, float radius)
    {
        float halfRadius = radius / 2;
        return point.X >= center.X - halfRadius &&
               point.X <= center.X + halfRadius &&
               point.Y >= center.Y - halfRadius &&
               point.Y <= center.Y + halfRadius;
    }


    // now it should works, thanks: https://github.com/schwarper/CS2TraceRay/blob/main/CS2TraceRay/Class/PlayerExtensions.cs#L85
    private float GetGroundLevel(Vector spawnOrigin)
    {
        if (spawnOrigin == null)
            return 0.0f;

        CGameTrace trace = TraceRay.TraceShape(spawnOrigin, new QAngle(90, 0, 0), TraceMask.MaskAll, Contents.Sky, 0);

        if (trace.EndPos != null)
        {
            return trace.EndPos.Z;
        }

        return spawnOrigin.Z;
    }

    // thanks https://github.com/exkludera/cs2-noclip/blob/main/noclip.cs
    private HookResult OnNoclipCommand(CCSPlayerController? player, CommandInfo command)
    {
        if (player == null || !player.IsValid || !player.PawnIsAlive)
            return HookResult.Continue;

        if (player.Team == CsTeam.Spectator || player.Team == CsTeam.None)
            return HookResult.Continue;

        int index = (int)player.Index;

        if (player.PlayerPawn.Value.MoveType == MoveType_t.MOVETYPE_NOCLIP)
        {
            player.PlayerPawn.Value.MoveType = MoveType_t.MOVETYPE_WALK;
            Schema.SetSchemaValue(player.PlayerPawn.Value.Handle, "CBaseEntity", "m_nActualMoveType", 2);
            Utilities.SetStateChanged(player.PlayerPawn.Value, "CBaseEntity", "m_MoveType");

            if (isInSquare.ContainsKey(index))
            {
                isInSquare.Remove(index);
            }
        }
        else
        {
            player.PlayerPawn.Value.MoveType = MoveType_t.MOVETYPE_NOCLIP;
            Schema.SetSchemaValue(player.PlayerPawn.Value.Handle, "CBaseEntity", "m_nActualMoveType", 8);
            Utilities.SetStateChanged(player.PlayerPawn.Value, "CBaseEntity", "m_MoveType");

            if (isInSquare.ContainsKey(index))
            {
                isInSquare.Remove(index);
            }
        }

        return HookResult.Handled;
    }

    [CommandHelper(whoCanExecute: CommandUsage.CLIENT_ONLY)]
    private void Command_Tp(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null || !player.IsValid)
            return;

        int index = (int)player.Index;

        if (isPlayerTP.ContainsKey(index))
        {
            isPlayerTP[index] = !isPlayerTP[index];
        }
        else
        {
            isPlayerTP[index] = true;
        }

        string status = isPlayerTP[index] ? $"{ChatColors.Lime}ON" : $"{ChatColors.Orange}OFF";
        info.ReplyToCommand($"[Spawn{ChatColors.Lime}Beam{ChatColors.Default}] Auto teleport: {status}");
    }

    [CommandHelper(whoCanExecute: CommandUsage.CLIENT_ONLY)]
    private void Command_TpRandom(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null || !player.IsValid || player.PlayerPawn.Value == null)
            return;

        List<SpawnPoint> spawns;
        string teamName;

        if (player.Team == CsTeam.Terrorist)
        {
            spawns = tSpawns;
            teamName = $"{ChatColors.Yellow}Terrorist{ChatColors.Default}";
        }
        else if (player.Team == CsTeam.CounterTerrorist)
        {
            spawns = ctSpawns;
            teamName = $"{ChatColors.Blue}Counter Terrorist{ChatColors.Default}";
        }
        else
        {
            info.ReplyToCommand($"[Spawn{ChatColors.Lime}Beam{ChatColors.Default}] You must be on a team to use this command!");
            return;
        }

        if (spawns.Count == 0)
        {
            info.ReplyToCommand($"[Spawn{ChatColors.Lime}Beam{ChatColors.Default}] No spawn points found for your team!");
            return;
        }

        var randomSpawn = spawns[Random.Shared.Next(spawns.Count)];
        player.PlayerPawn.Value.Teleport(randomSpawn.AbsOrigin, randomSpawn.CBodyComponent?.SceneNode?.AbsRotation!, new Vector(0, 0, 0));
        info.ReplyToCommand($"[Spawn{ChatColors.Lime}Beam{ChatColors.Default}] Teleported to random {teamName} spawn point!");
    }

    [GameEventHandler]
    private HookResult OnPlayerDisconnect(EventPlayerDisconnect @event, GameEventInfo info)
    {
        var player = @event.Userid;
        if (player != null && player.IsValid)
        {
            int index = (int)player.Index;

            isPlayerTP.Remove(index);
            isInSquare.Remove(index);

        }
        return HookResult.Continue;
    }

    [GameEventHandler]
    private HookResult OnPlayerSpawn(EventPlayerSpawn @event, GameEventInfo info)
    {
        var player = @event.Userid;
        player.ExecuteClientCommandFromServer("buddha");
        player.ExecuteClientCommandFromServer("buddha_ignore_bots 1");
        return HookResult.Continue;
    }

    [GameEventHandler]
    private HookResult OnRoundStarted(EventRoundPoststart @event, GameEventInfo info)
    {
        tSpawns = Utilities.FindAllEntitiesByDesignerName<SpawnPoint>("info_player_terrorist").ToList();
        ctSpawns = Utilities.FindAllEntitiesByDesignerName<SpawnPoint>("info_player_counterterrorist").ToList();

        var spawnConfigs = new[]
        {
        (tSpawns, Color.Yellow),
        (ctSpawns, Color.Cyan)
    };

        foreach (var (spawns, color) in spawnConfigs)
        {
            foreach (var spawn in spawns)
            {
                Vector centerPoint = spawn.AbsOrigin;

                float groundZ = GetGroundLevel(centerPoint);

                Vector startPos = new Vector(centerPoint.X + squareRadius / 2, centerPoint.Y - squareRadius / 2, groundZ + 20f);
                Vector endPos = new Vector(centerPoint.X - squareRadius / 2, centerPoint.Y + squareRadius / 2, groundZ + 20f);
                BeamHelper.SquareBeam(startPos, endPos, squareRadius, squareWidth, color);
            }
        }

        return HookResult.Continue;
    }
}
