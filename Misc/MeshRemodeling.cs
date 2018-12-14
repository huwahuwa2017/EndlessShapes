using BrilliantSkies.Core.Help;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace EndlessShapes
{
    public class MeshRemodeling
    {
        public static void ChangeScale(Mesh mesh, Vector3 scale)
        {
            mesh.SetVertices(new List<Vector3>(mesh.vertices.Select(V => Vector3.Scale(V, scale))));
        }

        public static void ChangeColor(Mesh mesh, Color color, bool VertexColor)
        {
            if (VertexColor)
            {
                int Smooth = Colors.DecodeFlag(color.r);
                mesh.SetColors(new List<Color>(mesh.colors.Select(C => new Color(Colors.EncodeColourChanel(C.r, Smooth), C.g, C.b, color.a))));
            }
            else
            {
                mesh.SetColors(new List<Color>(new Color[mesh.vertexCount].Select(C => color)));
            }
        }

        public static void Recalculate(Mesh mesh)
        {
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();
        }
    }
}