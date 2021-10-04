using System;
using System.Collections.Generic;
using UnityEngine;

namespace FIMSpace.Generating
{
    public partial class FGenGraph<T1, T2> where T1 : FGenCell, new() where T2 : FGenPoint, new()
    {
        public List<T1> AllCells = new List<T1>();
        public List<T1> AllApprovedCells = new List<T1>();
        public FGenGrid<T1> Cells = new FGenGrid<T1>();

        /// <summary> Additional scaled grids </summary>
        public List<FGenGraph<T1, T2>> SubGraphs;

        public T1 MinX { get; private set; }
        public T1 MinY { get; private set; }
        public T1 MinZ { get; private set; }
        public T1 MaxX { get; private set; }
        public T1 MaxY { get; private set; }
        public T1 MaxZ { get; private set; }

        public int Width, Height, Depth;
        public int ReferenceScale = 1;
        public float YScale = 1f;

        public FGenGraph(bool reset = false)
        {
            if (reset)
            {
                AllCells = new List<T1>();
                AllApprovedCells = new List<T1>();
                Cells = new FGenGrid<T1>();
            }
        }

        public void Generate(int width, int depth, Vector3Int offset)
        {
            Width = width;
            Height = 1;
            Depth = depth;

            for (int x = offset.x; x < width + offset.x; x++)
                for (int z = offset.z; z < depth + offset.z; z++)
                    AddCell(x, offset.y, z);

        }

        public void Generate(int xWidth, int yHeight, int zDepth, Vector3Int offset)
        {
            Width = xWidth;
            Height = yHeight;
            Depth = zDepth;

            for (int x = offset.x; x < xWidth + offset.x; x++)
                for (int y = 0; y <= yHeight; y++)
                    for (int z = offset.z; z < zDepth + offset.z; z++)
                        AddCell(x, y, z);
        }


        public T1 AddCell(Vector3Int position)
        {
            return AddCell(position.x, position.y, position.z);
        }

        public T1 AddCell(Vector2Int position, int yLevel = 0)
        {
            return AddCell(position.x, yLevel, position.y);
        }

        public T1 AddCell(int x, int y, int z)
        {
            T1 cell = GetCell(x, y, z, false);

            if (FGenerators.CheckIfIsNull((cell)) || cell.InTargetGridArea == false)
            {
                cell = GetCell(x, y, z, true);
                cell.InTargetGridArea = true;
                cell.Scaler = ReferenceScale;
                AllApprovedCells.Add(cell);
                CheckForMinMax(cell);
            }

            return cell;
        }

        private void CheckForMinMax(T1 cell)
        {
            if (FGenerators.CheckIfExist_NOTNULL(cell))
            {
                if (FGenerators.CheckIfIsNull(MinX)) MinX = cell;
                if (FGenerators.CheckIfIsNull(MinY)) MinY = cell;
                if (FGenerators.CheckIfIsNull(MinZ)) MinZ = cell;
                if (FGenerators.CheckIfIsNull(MaxX)) MaxX = cell;
                if (FGenerators.CheckIfIsNull(MaxY)) MaxY = cell;
                if (FGenerators.CheckIfIsNull(MaxZ)) MaxZ = cell;

                if (cell.Pos.x < MinX.Pos.x) MinX = cell;
                if (cell.Pos.y < MinY.Pos.y) MinY = cell;
                if (cell.Pos.z < MinZ.Pos.z) MinZ = cell;

                if (cell.Pos.x > MaxX.Pos.x) MaxX = cell;
                if (cell.Pos.y > MaxY.Pos.y) MaxY = cell;
                if (cell.Pos.z > MaxZ.Pos.z) MaxZ = cell;
            }
        }

        public Vector3Int GetMin()
        {
            return new Vector3Int(MinX.Pos.x, MinY.Pos.y, MinZ.Pos.z);
        }

        public Vector3Int GetMax()
        {
            return new Vector3Int(MaxX.Pos.x, MaxY.Pos.y, MaxZ.Pos.z);
        }

        public T1 GetCell(Vector3Int pos, bool generateIfOut = true)
        {
            return GetCell(pos.x, pos.y, pos.z, generateIfOut);
        }

        public T1 GetCell(Vector2Int pos, bool generateIfOut = true, int y = 0)
        {
            return GetCell(pos.x, y, pos.y, generateIfOut);
        }

        public T1 KickOutCell(Vector3Int pos)
        {
            T1 cell = GetCell(pos);
            if (FGenerators.CheckIfExist_NOTNULL(cell)) cell.InTargetGridArea = false;
            return cell;
        }

        public bool RemoveCell(Vector3Int pos)
        {
            T1 cell = GetCell(pos, false); //!!! zmiana

            if (FGenerators.CheckIfExist_NOTNULL(cell))
            {
                cell.InTargetGridArea = false;
                AllApprovedCells.Remove(cell);
                AllCells.Remove(cell);
                return true;
            }

            return false;
        }

        public void RemoveCell(T1 cell)
        {
            if (FGenerators.CheckIfIsNull(cell)) return;
            cell.InTargetGridArea = false;
            AllApprovedCells.Remove(cell);
            AllCells.Remove(cell);
        }

        public T1 GetCell(int x, int y, int z, bool generateIfOut = true)
        {
            bool wasGenerated = false;

            T1 cell = Cells.GetCell(x, y, z,
                (c, gz) =>
                {
                    c.Pos = new Vector3Int(x, y, gz);
                    AllCells.Add(c);
                    wasGenerated = true;
                },
                generateIfOut);

            if (cell != null) return cell;
            if (wasGenerated) return Cells.GetCell(x, y, z, null, false);
            else return null;
        }

        /// <summary> L R U D </summary>
        public T1[] GetNeightbours(T1 cell, bool generateIfOut = true)
        {
            T1[] verts = new T1[4];

            verts[0] = GetCell(cell.Pos.x - 1, cell.Pos.y, cell.Pos.z, generateIfOut);
            verts[1] = GetCell(cell.Pos.x + 1, cell.Pos.y, cell.Pos.z, generateIfOut);
            verts[2] = GetCell(cell.Pos.x, cell.Pos.y, cell.Pos.z + 1, generateIfOut);
            verts[3] = GetCell(cell.Pos.x, cell.Pos.y, cell.Pos.z - 1, generateIfOut);

            return verts;
        }

        /// <summary> from lu, every x row </summary>
        public T1[] GetCustomSquare(T1 cell, int around)
        {
            T1[] cells = new T1[around * around];
            int iter = 0;

            for (int z = 0; z < around; z++)
            {
                for (int x = 0; x < around; x++)
                {
                    cells[iter] = GetCell(cell.Pos.x + x, cell.Pos.y, cell.Pos.z + z, false);
                }
            }

            return cells;
        }


        /// <summary> from lu, every x row </summary>
        public List<T1> GetCustomSquare(T1 cell, int around, bool ignoreEmpty, bool generateIfOut = false)
        {
            List<T1> cells = new List<T1>();

            if (ignoreEmpty)
            {
                for (int z = 0; z < around; z++)
                    for (int x = 0; x < around; x++)
                    {
                        var nCell = GetCell(cell.Pos.x + x, cell.Pos.y, cell.Pos.z + z, generateIfOut);
                        if (FGenerators.CheckIfIsNull(nCell)) continue;
                        if (nCell.InTargetGridArea == false) continue;
                        cells.Add(nCell);
                    }
            }
            else
                for (int z = 0; z < around; z++)
                    for (int x = 0; x < around; x++)
                        cells.Add(GetCell(cell.Pos.x + x, cell.Pos.y, cell.Pos.z + z, generateIfOut));

            return cells;
        }


        public int CountCellsAround(T1 cell, int size)
        {
            int counter = 0;
            for (int x = 0; x < size; x++)
                for (int z = 0; z < size; z++)
                {
                    var c = GetCell(cell.Pos.x + x, cell.Pos.y, cell.Pos.z + z, false);
                    if (FGenerators.CheckIfExist_NOTNULL(c)) if (c.InTargetGridArea) counter++;
                }

            return counter;
        }

        public bool AreAnyCellsAround(T1 cell, int size)
        {
            for (int x = 0; x < size; x++)
                for (int z = 0; z < size; z++)
                    if (FGenerators.CheckIfExist_NOTNULL(GetCell(cell.Pos.x, +x, cell.Pos.z + z, false))) return true;

            return false;
        }

        public bool AreAnyCellsLacking(T1 cell, int size)
        {
            for (int x = 0; x < size; x++)
                for (int z = 0; z < size; z++)
                    if (FGenerators.CheckIfIsNull(GetCell(cell.Pos.x, +x, cell.Pos.z + z, false))) return false;

            return true;
        }

        /// <summary>
        /// 3x3 square neightbours including initial cell ([4]th index)
        /// </summary>
        public T1[] Get3x3Square(T1 cell, bool generateIfOut = true)
        {
            T1[] verts = new T1[9];

            verts[0] = GetCell(cell.Pos.x - 1, cell.Pos.y, cell.Pos.z + 1, generateIfOut);
            verts[1] = GetCell(cell.Pos.x, cell.Pos.y, cell.Pos.z + 1, generateIfOut);
            verts[2] = GetCell(cell.Pos.x + 1, cell.Pos.y, cell.Pos.z + 1, generateIfOut);

            verts[3] = GetCell(cell.Pos.x - 1, cell.Pos.y, cell.Pos.z, generateIfOut);
            verts[4] = GetCell(cell.Pos.x, cell.Pos.y, cell.Pos.z, generateIfOut);
            verts[5] = GetCell(cell.Pos.x + 1, cell.Pos.y, cell.Pos.z, generateIfOut);

            verts[6] = GetCell(cell.Pos.x - 1, cell.Pos.y, cell.Pos.z - 1, generateIfOut);
            verts[7] = GetCell(cell.Pos.x, cell.Pos.y, cell.Pos.z - 1, generateIfOut);
            verts[8] = GetCell(cell.Pos.x + 1, cell.Pos.y, cell.Pos.z - 1, generateIfOut);

            return verts;
        }

        /// <summary> U L R D </summary>
        public T1[] GetPLUSSquare(T1 cell, bool generateIfOut = true)
        {
            T1[] verts = new T1[4];

            verts[0] = GetCell(cell.Pos.x, cell.Pos.y, cell.Pos.z + 1, generateIfOut);
            verts[1] = GetCell(cell.Pos.x - 1, cell.Pos.y, cell.Pos.z, generateIfOut);
            verts[2] = GetCell(cell.Pos.x + 1, cell.Pos.y, cell.Pos.z, generateIfOut);
            verts[3] = GetCell(cell.Pos.x, cell.Pos.y, cell.Pos.z - 1, generateIfOut);

            return verts;
        }


        /// <summary> LU RU LD RD </summary>
        public T1[] GetDiagonalCross(T1 cell, bool generateIfOut = true)
        {
            T1[] verts = new T1[4];

            verts[0] = GetCell(cell.Pos.x - 1, cell.Pos.y, cell.Pos.z + 1, generateIfOut);
            verts[1] = GetCell(cell.Pos.x + 1, cell.Pos.y, cell.Pos.z + 1, generateIfOut);
            verts[2] = GetCell(cell.Pos.x - 1, cell.Pos.y, cell.Pos.z - 1, generateIfOut);
            verts[3] = GetCell(cell.Pos.x + 1, cell.Pos.y, cell.Pos.z - 1, generateIfOut);

            return verts;
        }

        /// <summary> Getting squares around in cells distance </summary>
        public List<T1> GetDistanceSquare2DList(T1 from, float cellSize, int indexDistance, float worldDistance)
        {
            List<T1> cells = new List<T1>();
            Vector3 refPos = from.Pos;

            for (int x = -indexDistance; x <= indexDistance; x++)
            {
                for (int z = -indexDistance; z <= indexDistance; z++)
                {
                    Vector3 tgtPos = new Vector3(refPos.x + x, refPos.y, refPos.z + z);

                    T1 tgtCell = GetCell(Mathf.RoundToInt(tgtPos.x), Mathf.RoundToInt(tgtPos.y), Mathf.RoundToInt(tgtPos.z), false);

                    if (tgtCell != null)
                    {
                        float cellsWorldDistance = Vector3.Distance(tgtPos * cellSize, refPos * cellSize);
                        if (cellsWorldDistance <= worldDistance) cells.Add(tgtCell);
                    }
                }
            }

            return cells;
        }

        internal FGenGraph<T1, T2> GetCorrespondingSubGraph(int scale)
        {
            if (SubGraphs != null)
                for (int i = 0; i < SubGraphs.Count; i++)
                    if (SubGraphs[i].ReferenceScale == scale) return SubGraphs[i];

            return null;
        }

        internal T1 GetCorrespondingSubGraphCell(FieldCell cell, FGenGraph<T1, T2> rightGraph)
        {
            int scaleDiff = ReferenceScale - rightGraph.ReferenceScale;
            if (scaleDiff < 0) { scaleDiff = -scaleDiff + 1; } else scaleDiff += 1;

            Vector3Int transposedPosition = new Vector3Int();

            transposedPosition.x = Mathf.FloorToInt(cell.Pos.x / scaleDiff);
            transposedPosition.y = Mathf.FloorToInt(cell.Pos.y / scaleDiff);
            transposedPosition.z = Mathf.FloorToInt(cell.Pos.z / scaleDiff);

            T1 newCell = rightGraph.GetCell(transposedPosition, false);

            return newCell;
        }

        internal Vector3 GetCenterUnrounded()
        {
            float xx = Mathf.Lerp(MinX.Pos.x, MaxX.Pos.x, 0.5f);
            float yy = Mathf.Lerp(MinY.Pos.y, MaxY.Pos.y, 0.5f);
            float zz = Mathf.Lerp(MinZ.Pos.z, MaxZ.Pos.z, 0.5f);
            return new Vector3(xx, yy, zz);
        }

        internal Vector3Int GetCenter()
        {
            int xx = Mathf.FloorToInt(Mathf.Lerp(MinX.Pos.x, MaxX.Pos.x, 0.5f));
            int yy = Mathf.FloorToInt(Mathf.Lerp(MinY.Pos.y, MaxY.Pos.y, 0.5f));
            int zz = Mathf.FloorToInt(Mathf.Lerp(MinZ.Pos.z, MaxZ.Pos.z, 0.5f));
            //int y = Height / 2;
            return new Vector3Int(xx, yy, zz);
        }

        public FGenGraph<T1, T2> Copy()
        {
            FGenGraph<T1, T2> copy = new FGenGraph<T1, T2>(true);
            copy.ReferenceScale = ReferenceScale;
            copy.MaxX = MaxX; copy.MinX = MinX;
            copy.MaxY = MaxY; copy.MinY = MinY;
            copy.MaxZ = MaxZ; copy.MinZ = MinZ;

            for (int i = 0; i < AllApprovedCells.Count; i++)
            {
                copy.AddCell(AllApprovedCells[i].Pos);
            }

            return copy;
        }

        public FGenGraph<T1, T2> CopyEmpty()
        {
            FGenGraph<T1, T2> copy = new FGenGraph<T1, T2>(true);
            copy.ReferenceScale = ReferenceScale;
            return copy;
        }

        private Vector3Int DivideAndCeilMax(Vector3Int val, float div)
        {
            if (val.x < 0) val.x = -Mathf.FloorToInt((float)-(val.x + 1) / div); else val.x = Mathf.CeilToInt((float)(val.x + 1) / div);
            if (val.y < 0) val.y = -Mathf.FloorToInt((float)-(val.y + 1) / div); else val.y = Mathf.CeilToInt((float)(val.y + 1) / div);
            if (val.z < 0) val.z = -Mathf.FloorToInt((float)-(val.z + 1) / div); else val.z = Mathf.CeilToInt((float)(val.z + 1) / div);
            return val;
        }

        private Vector3Int DivideAndCeilMin(Vector3Int val, float div)
        {
            if (val.x < 0) val.x = -Mathf.CeilToInt((float)-(val.x - 1) / div); else val.x = Mathf.FloorToInt((float)(val.x + 1) / div);
            if (val.y < 0) val.y = -Mathf.CeilToInt((float)-(val.y - 1) / div); else val.y = Mathf.FloorToInt((float)(val.y + 1) / div);
            if (val.z < 0) val.z = -Mathf.CeilToInt((float)-(val.z - 1) / div); else val.z = Mathf.FloorToInt((float)(val.z + 1) / div);
            return val;
        }

        public FGenGraph<T1, T2> GenerateScaledGraph(int scale, bool inheritCells = true, bool oneCellIsEnough = true)
        {
            FGenGraph<T1, T2> newGraph = new FGenGraph<T1, T2>();
            newGraph.ReferenceScale = scale;
            if (FGenerators.CheckIfIsNull(MinX) || FGenerators.CheckIfIsNull(MaxX)) return null;

            Vector3Int maxXC, minXC, maxZC, minZC;

            if (oneCellIsEnough)
            {
                maxXC = DivideAndCeilMax(MaxX.Pos, scale) * scale;
                minXC = DivideAndCeilMin(MinX.Pos, scale) * scale;
                maxZC = DivideAndCeilMax(MaxZ.Pos, scale) * scale;
                minZC = DivideAndCeilMin(MinZ.Pos, scale) * scale;
            }
            else
            {
                maxXC = MaxX.Pos;
                minXC = MinX.Pos;
                maxZC = MaxZ.Pos;
                minZC = MinZ.Pos;
            }

            for (int x = minXC.x; x <= maxXC.x; x += 1)
            {
                if (x % scale != 0) continue;
                for (int y = MinY.Pos.y; y <= MaxY.Pos.y; y += 1)
                    for (int z = minZC.z; z <= maxZC.z; z += 1)
                    {
                        if (z % scale != 0) continue;

                        T1 cell = GetCell(x, y, z, oneCellIsEnough);
                        if (FGenerators.CheckIfIsNull(cell)) continue;

                        if (CountCellsAround(cell, scale) > 0)
                        {
                            T1 nCell = newGraph.AddCell(( x / scale), y, (z / scale));
                            //T1 nCell = newGraph.AddCell(Mathf.FloorToInt( x / scale), y, Mathf.FloorToInt(z / scale));

                            if (inheritCells)
                            {
                                int ff = 0;
                                var gCells = GetCustomSquare(cell, scale, true);

                                foreach (var sCell in gCells)
                                {
                                    if (FGenerators.CheckIfIsNull(sCell)) continue;
                                    sCell.AddScaleParentCell(nCell);
                                    nCell.AddScaleChildCell(sCell);
                                    ff++;
                                }
                            }

                        }
                    }
            }

            RecalculateGridDimensions();
            newGraph.Width = Mathf.FloorToInt(Width / scale);
            newGraph.Height = Mathf.FloorToInt(Height / scale);
            newGraph.Depth = Mathf.FloorToInt(Depth / scale);

            return newGraph;
        }

        /// <summary>
        /// Recalculating width,height,depth variables
        /// </summary>
        public void RecalculateGridDimensions()
        {
            Width = Mathf.Abs((MaxX.Pos.x ) - MinX.Pos.x)+1;
            Height = Mathf.Abs((MaxY.Pos.y ) - MinY.Pos.y)+1;
            Depth = Mathf.Abs((MaxZ.Pos.z ) - MinZ.Pos.z)+1;
        }

        internal Vector3 GetWorldCenter(Vector3 cellSize, bool withOffset = false)
        {
            cellSize *= ReferenceScale;
            float x = Width / 2f;
            float y = Height / 2f;
            float z = Depth / 2f;

            if (withOffset)
                return Vector3.Scale(new Vector3(x, y, z), cellSize) + GetCenterOffset(cellSize);
            else
                return Vector3.Scale(new Vector3(x, y, z), cellSize);
        }

        internal Vector3 GetCenterOffset(Vector3 cellSize)
        {
            cellSize *= ReferenceScale;
            float x = 0f;
            float y = 0f;
            float z = 0f;

            Vector3 center = GetCenterUnrounded();

            if (Mathf.RoundToInt(center.x * 2) % 2 == 1)
                x += 1f / 2f;

            if (Mathf.RoundToInt(center.z * 2) % 2 == 1)
                z += 1f / 2f;

            return Vector3.Scale(new Vector3(x, y, z), cellSize);
        }


    }
}
