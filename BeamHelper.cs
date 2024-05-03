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
            List<Vector> Vertices = new List<Vector>()
            {
                //horizontal top
                new Vector(startPos.X, startPos.Y, startPos.Z),
                new Vector(endPos.X, startPos.Y, startPos.Z),

                //horizontal bottom
                new Vector(endPos.X, endPos.Y, startPos.Z),
                new Vector(startPos.X, endPos.Y, startPos.Z),

                //vertical left
                new Vector(startPos.X, startPos.Y, startPos.Z),
                new Vector(startPos.X, endPos.Y, startPos.Z),

                //vertical right
                new Vector(endPos.X, startPos.Y, startPos.Z),
                new Vector(endPos.X, endPos.Y, startPos.Z),
            }  ;

            for (int i = 0; i < Vertices.Count; i++)
            {
                int nextIndex = (i + 1) % Vertices.Count;
                DrawBeam(Vertices[i], Vertices[nextIndex], width, color);
            }
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
