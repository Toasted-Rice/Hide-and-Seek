using System;
using UnityEngine;

namespace FIMSpace.Generating
{
    public class FGenGrid<T> where T : class, new()
    {
        public FGenTwoDirDynamicList<FGenTwoDirDynamicList<FGenTwoDirDynamicList<T>>> Dimensions { get; private set; }
        public delegate void OnGeneratedElement(T generated);

        public FGenGrid()
        {
            Dimensions = new FGenTwoDirDynamicList<FGenTwoDirDynamicList<FGenTwoDirDynamicList<T>>>();
        }

        public T GetCell(float x, float y, float z, Action<T,int> callback = null, bool generateIfOut = true)
        {
            return Dimensions
                .GetAt((int)x)
                .GetAt((int)y)
                .GetAt((int)z, callback, generateIfOut);
        }

        public T GetCell(float x, float z, Action<T, int> callback = null, bool generateIfOut = true)
        {
            return Dimensions.
            GetAt((int)x)
            .GetAt(0).
            GetAt((int)z, callback, generateIfOut);
        }


        public Vector2Int GetXSize()
        {
            return new Vector2Int(Dimensions.GetNegativeLength(), Dimensions.GetPositiveLength());
        }

        public Vector2Int GetYSize()
        {
            return new Vector2Int(Dimensions.GetAt(0).GetNegativeLength(), Dimensions.GetAt(0).GetPositiveLength());
        }

        public Vector2Int GetZSize()
        {
            return new Vector2Int(Dimensions.GetAt(0).GetAt(0).GetNegativeLength(), Dimensions.GetAt(0).GetAt(0).GetPositiveLength());
        }

        public void Clear()
        {
            Dimensions.Clear();
        }
    }
}
