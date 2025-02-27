﻿using System;
using System.Collections.Generic;
using UnityEngine;

namespace FIMSpace.Generating
{
    public class FieldCell : FGenCell
    {
        /// <summary> If cell have assigned some spawning datas </summary>
        private List<SpawnData> Spawns;
        /// <summary> If cell is occupied also by other cell (on same grid scale level) </summary>
        public FieldCell ParentCell;
        /// <summary> If cell is occupying other cells, they're saved here (on same grid scale level) </summary>
        public List<FieldCell> ChildCells;
        /// <summary> Additional custom data within cell </summary>
        private List<string> cellCustomData;
        private List<SpawnInstruction> CellInstructions;

        [NonSerialized] public NeightbourPlacement neightbours;

        public Vector2Int PosXZ { get { return new Vector2Int(Pos.x, Pos.z); } }

        public FieldCell()
        {
            Spawns = new List<SpawnData>();
        }

        public virtual void Clear()
        {
            ParentCell = null;
            ResetCellsHierarchy();
            if (ChildCells != null) ChildCells.Clear();
            if (cellCustomData != null) { cellCustomData.Clear(); cellCustomData = null; }
            Spawns.Clear();
            if (CellInstructions != null) CellInstructions.Clear();
        }

        internal void OccupyOther(FieldCell child)
        {
            if (FGenerators.CheckIfIsNull(child )) return;
            if (ChildCells == null) ChildCells = new List<FieldCell>();
            if (ChildCells.Contains(child) == false) ChildCells.Add(child);
            child.ParentCell = this;
        }

        internal void UnOccupyOther(FieldCell child)
        {
            if (ChildCells == null) return;
            if (ChildCells.Contains(child) == false) return;
            ChildCells.Remove(child);
            child.ParentCell = null;
        }

        public void AddSpawnToCell(SpawnData spawn)
        {
            if (Spawns.Contains(spawn) == false) Spawns.Add(spawn);
        }

        public void RemoveSpawnFromCell(SpawnData spawn)
        {
            if (Spawns.Contains(spawn)) Spawns.Remove(spawn);
        }

        public void RemoveAllSpawnsFromCell()
        {
            Spawns.Clear();
        }

        public int GetJustCellSpawnCount()
        {
            return Spawns.Count;
        }

        public List<SpawnData> GetSpawnsJustInsideCell(bool returnCopyOfList = false)
        {
            if (Spawns == null) Spawns = new List<SpawnData>();

            if (returnCopyOfList)
            {
                List<SpawnData> list = new List<SpawnData>();
                PGGUtils.TransferFromListToList<SpawnData>(Spawns, list);
                return list;
            }
            else
                return Spawns;
        }

        /// <summary>
        /// Including parent and child cells if occupied and also scaled grid cells if using access override
        /// </summary>
        public List<SpawnData> CollectSpawns(FieldSpawner.ESR_CellHierarchyAccess access = FieldSpawner.ESR_CellHierarchyAccess.SameScale, bool alwaysNewList = false)
        {
            if (FGenerators.CheckIfIsNull(ParentCell ))
            {
                if (access == FieldSpawner.ESR_CellHierarchyAccess.SameScale)
                {
                    return GetSpawnsJustInsideCell(alwaysNewList);
                    //List<SpawnData> datas = new List<SpawnData>();
                    ////if (ChildCells != null) if (ChildCells.Count > 0)
                    ////        for (int i = 0; i < ChildCells.Count; i++) StreamSpawnListToOther(ChildCells[i].GetSpawnsJustInsideCell(), datas);

                    //StreamSpawnListToOther(Spawns, datas);
                    //return datas;
                }
                else
                {
                    // Handling accessing scaled graphs
                    List<SpawnData> datas = new List<SpawnData>();

                    if (access == FieldSpawner.ESR_CellHierarchyAccess.LowerAndSame || access == FieldSpawner.ESR_CellHierarchyAccess.HigherAndSame)
                    {
                        StreamSpawnListToOther(Spawns, datas);
                    }

                    CellAccessProcess(this, access, datas);

                    return datas;
                }
            }
            else
            {
                if (ParentCell.ChildCells == null)
                {
                    if (access == FieldSpawner.ESR_CellHierarchyAccess.SameScale)
                    {
                        return GetSpawnsJustInsideCell(alwaysNewList);
                        //List<SpawnData> datass = new List<SpawnData>();
                        //StreamSpawnListToOther(Spawns, datass);
                        //return datass;
                    }
                    else
                    {
                        List<SpawnData> datasacc = new List<SpawnData>();

                        if (access == FieldSpawner.ESR_CellHierarchyAccess.LowerAndSame || access == FieldSpawner.ESR_CellHierarchyAccess.HigherAndSame)
                            StreamSpawnListToOther(Spawns, datasacc);

                        CellAccessProcess(this, access, datasacc);

                        return datasacc;
                    }
                }

                List<SpawnData> datas = new List<SpawnData>();
                StreamSpawnListToOther(ParentCell.Spawns, datas);

                for (int i = 0; i < ParentCell.ChildCells.Count; i++)
                {
                    if (FGenerators.CheckIfIsNull(ParentCell.ChildCells[i] )) continue;
                    FieldCell chld = ParentCell.ChildCells[i];

                    for (int s = 0; s < chld.Spawns.Count; s++)
                        if (FGenerators.CheckIfExist_NOTNULL(chld.Spawns[s])) 
                            if (chld.Spawns[s].Enabled) 
                                datas.Add(chld.Spawns[s]);
                }

                StreamSpawnListToOther(Spawns, datas, true);


                if (access != FieldSpawner.ESR_CellHierarchyAccess.SameScale)
                    CellAccessProcess(ParentCell, access, datas);

                return datas;
            }
        }

        static void CellAccessProcess(FieldCell startCell, FieldSpawner.ESR_CellHierarchyAccess access, List<SpawnData> datas)
        {
            if (/*access == FieldSpawner.ESR_CellHierarchyAccess.HigherScale ||*/ access == FieldSpawner.ESR_CellHierarchyAccess.HigherAndSame)
            {
                //UnityEngine.Debug.Log("HaveScaleParentCells = " + startCell.HaveScaleParentCells());
                if (startCell.HaveScaleParentCells()) foreach (FieldCell biggerCell in startCell.GetScaleParentCells()) { if (FGenerators.CheckIfExist_NOTNULL(biggerCell) ) startCell.StreamSpawnListToOther(biggerCell.CollectSpawns(access), datas); }
            }
            else if (/*access == FieldSpawner.ESR_CellHierarchyAccess.LowerScale || */access == FieldSpawner.ESR_CellHierarchyAccess.LowerAndSame)
            {
                if (startCell.HaveScaleChildCells()) foreach (FieldCell lowerCell in startCell.GetScaleChildCells()) { if (FGenerators.CheckIfExist_NOTNULL(lowerCell )) startCell.StreamSpawnListToOther(lowerCell.CollectSpawns(access), datas); }
            }
        }

        void StreamSpawnListToOther(List<SpawnData> from, List<SpawnData> to, bool containsCheck = false)
        {
            if (from == null) return;

            if (!containsCheck) // Faster if contains check not needed
            {
                for (int i = 0; i < from.Count; i++) if (FGenerators.CheckIfExist_NOTNULL(from[i] )) { if (from[i].Enabled) to.Add(from[i]); }
            }
            else
            {
                for (int i = 0; i < from.Count; i++) if (FGenerators.CheckIfExist_NOTNULL(from[i] )) if (!to.Contains(from[i])) { if (from[i].Enabled) to.Add(from[i]); }
            }
        }

        internal Bounds ToBounds(float scale, Vector3 offset)
        {
            return new Bounds(new Vector3(Pos.x + 0.5f + offset.x, offset.y, Pos.z + 0.5f + offset.z) * scale, Vector3.one * scale);
        }


        public void CheckNeightboursRelation(FGenGraph<FieldCell, FGenPoint> onGraph)
        {
            neightbours = new NeightbourPlacement();

            for (int i = 0; i <= 8; i++)
            {
                NeightbourPlacement.ENeightbour n = (NeightbourPlacement.ENeightbour)i;
                if (n == NeightbourPlacement.ENeightbour.Middle) continue;

                Vector3 dir = NeightbourPlacement.GetDirection(n);
                Vector3Int pos = new Vector3Int(Pos.x + (int)dir.x, Pos.y, Pos.z + (int)dir.z);
                FieldCell tempCell = onGraph.GetCell(pos, false);

                if (tempCell == null || tempCell.InTargetGridArea == false)
                {
                    neightbours.Set(n, false);
                }
                else
                {
                    neightbours.Set(n, true);
                }
            }

        }

        internal int GetSpawnsWithModCount(FieldModification mod)
        {
            int count = 0;
            for (int i = 0; i < Spawns.Count; i++)
                if (Spawns[i].OwnerMod == mod) count++;
            return count;
        }

        public void AddCustomData(string dataString)
        {
            if (cellCustomData == null) cellCustomData = new List<string>();
            if (cellCustomData.Contains(dataString) == false)
            {
                cellCustomData.Add(dataString);
            }
        }

        public bool HaveCustomData(string targetData)
        {
            if (targetData.Length > 0) if (targetData[0] == '!') if (cellCustomData == null) return true;

            if (cellCustomData == null) return false;

            if (targetData[0] == '!')
            {
                return !cellCustomData.Contains(targetData.Substring(1, targetData.Length - 1));
            }
            else
                return cellCustomData.Contains(targetData);
        }

        public void AddCellInstruction(SpawnInstruction instruction)
        {
            if (CellInstructions == null) CellInstructions = new List<SpawnInstruction>();

            for (int i = 0; i < CellInstructions.Count; i++)
                if (CellInstructions[i].definition.InstructionType == instruction.definition.InstructionType) return;

            if (CellInstructions.Contains(instruction) == false) CellInstructions.Add(instruction);
        }

        public void ReplaceInstructions(List<SpawnInstruction> instructions)
        {
            CellInstructions = instructions;
        }

        public bool HaveInstructions()
        {
            if (CellInstructions == null) return false;
            if (CellInstructions.Count == 0) return false;
            return true;
        }

        public List<SpawnInstruction> GetInstructions()
        {
            if (CellInstructions == null) CellInstructions = new List<SpawnInstruction>();
            return CellInstructions;
        }

    }
}
