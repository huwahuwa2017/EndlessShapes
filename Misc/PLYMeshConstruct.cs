using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace EndlessShapes
{
    public class PLYMeshConstruct
    {
        private static int Order_x = -1;

        private static int Order_y = -1;

        private static int Order_z = -1;

        private static int Order_s = -1;

        private static int Order_t = -1;

        private static int Order_red = -1;

        private static int Order_green = -1;

        private static int Order_blue = -1;

        public static int VertexCount;

        public static int PolygonCount;

        public static bool ReadUV = false;

        public static bool ReadColor = false;



        public static StringReader GetPLYState(string TextData)
        {
            Order_x = -1;
            Order_y = -1;
            Order_z = -1;
            Order_s = -1;
            Order_t = -1;
            Order_red = -1;
            Order_green = -1;
            Order_blue = -1;
            VertexCount = 0;
            PolygonCount = 0;
            ReadUV = false;
            ReadColor = false;
            int PropertyCount = 0;
            string InputLine = string.Empty;
            StringReader Reader = new StringReader(TextData);

            while ((InputLine = Reader.ReadLine()) != null)
            {
                string[] SA = InputLine.Split(' ');

                if (SA[0] == "property")
                {
                    switch (SA[2])
                    {
                        case "x":
                            Order_x = PropertyCount;
                            break;
                        case "y":
                            Order_y = PropertyCount;
                            break;
                        case "z":
                            Order_z = PropertyCount;
                            break;
                        case "s":
                            Order_s = PropertyCount;
                            break;
                        case "t":
                            Order_t = PropertyCount;
                            break;
                        case "red":
                            Order_red = PropertyCount;
                            break;
                        case "green":
                            Order_green = PropertyCount;
                            break;
                        case "blue":
                            Order_blue = PropertyCount;
                            break;
                    }

                    ++PropertyCount;
                }
                else if (SA[0] == "element")
                {
                    int num = int.Parse(SA[2]);

                    if (SA[1] == "vertex")
                    {
                        VertexCount = num;
                    }
                    else if (SA[1] == "face")
                    {
                        PolygonCount = num;
                    }
                }
                else if (SA[0] == "end_header")
                {
                    break;
                }
            }

            ReadUV = Order_s != -1 && Order_t != -1;
            ReadColor = Order_red != -1 && Order_green != -1 && Order_blue != -1;
            return Reader;
        }

        public static Mesh MeshConstruct(string TextData)
        {
            List<Vector3> ListVertex = new List<Vector3>();
            List<Vector2> ListUV = new List<Vector2>();
            List<Color> ListColor = new List<Color>();
            List<int> ListTriangle = new List<int>();
            StringReader Reader = GetPLYState(TextData);

            for (int i = 0; i < VertexCount; ++i)
            {
                float[] Array = Reader.ReadLine().Split(' ').Select(S => float.Parse(S)).ToArray();
                ListVertex.Add(new Vector3(Array[Order_x], Array[Order_y], Array[Order_z]));
                if (ReadUV) ListUV.Add(new Vector2(Array[Order_s], Array[Order_t]));
                if (ReadColor) ListColor.Add(new Color(Array[Order_red] / 255, Array[Order_green] / 255, Array[Order_blue] / 255, 0.99f));
            }

            for (int i = 0; i < PolygonCount; ++i)
            {
                int[] Array = Reader.ReadLine().Split(' ').Select(S => int.Parse(S)).ToArray();

                for (int j = 0; j <= Array[0] - 3; ++j)
                {
                    ListTriangle.Add(Array[1]);
                    ListTriangle.Add(Array[2 + j]);
                    ListTriangle.Add(Array[3 + j]);
                }
            }

            Mesh ConstructMesh = new Mesh();
            ConstructMesh.SetVertices(ListVertex);
            if (ReadUV) ConstructMesh.SetUVs(0, ListUV);
            if (ReadColor) ConstructMesh.SetColors(ListColor);
            ConstructMesh.SetTriangles(ListTriangle, 0);
            MeshRemodeling.Recalculate(ConstructMesh);
            return ConstructMesh;
        }

        public static Mesh MeshConstruct(string TextData, string Name)
        {
            Mesh NewMesh = MeshConstruct(TextData);
            NewMesh.name = Name;
            return NewMesh;
        }
    }
}