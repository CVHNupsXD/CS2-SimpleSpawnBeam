using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using System.Drawing;

namespace CBeamHelper
{
    public class BeamHelper
    {
        public static void SquareBeam(Vector startPos, Vector endPos, float height, float width, Color color)
        {
            var corners = new[]
            {
                new Vector(startPos.X, startPos.Y, startPos.Z),
                new Vector(endPos.X, startPos.Y, startPos.Z),
                new Vector(endPos.X, endPos.Y, startPos.Z),
                new Vector(startPos.X, endPos.Y, startPos.Z)
            };

            for (int i = 0; i < 4; i++)
                DrawBeam(corners[i], corners[(i + 1) % 4], width, color);
        }

        public static void DrawBeam(Vector startPos, Vector endPos, float width, Color color)
        {
            CBeam beam = Utilities.CreateEntityByName<CBeam>("beam");
            if (beam == null)
            {
                return;
            }

            beam.Render = color;
            beam.Width = width;
            beam.EndWidth = width;

            beam.StartFrame = 0;
            beam.FrameRate = 0;

            beam.Teleport(startPos, new QAngle(), new Vector());
            beam.EndPos.Add(endPos);
            beam.DispatchSpawn();
        }
    }

    
}
