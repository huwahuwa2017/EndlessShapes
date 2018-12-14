using System;
using UnityEngine;

namespace EndlessShapes
{
    public class PrimitiveMesh
    {
        private static Mesh[] AllPrimitiveMesh = new Mesh[Enum.GetValues(typeof(PrimitiveType)).Length];

        public static Mesh Create(PrimitiveType PT)
        {
            return Create((int)PT);
        }

        public static Mesh Create(int Index)
        {
            Mathf.Clamp(Index, 0, Enum.GetValues(typeof(PrimitiveType)).Length - 1);

            if (AllPrimitiveMesh[Index] == null)
                AllPrimitiveMesh[Index] = Resources.GetBuiltinResource<Mesh>(((PrimitiveType)Index).ToString() + ".fbx");

            return AllPrimitiveMesh[Index];
        }
    }
}