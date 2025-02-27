﻿using FIMSpace.Generating.Checker;
using FIMSpace.Generating.PathFind;
using FIMSpace.Generating.Planning;
using FIMSpace.Generating.Rules;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace FIMSpace.Generating
{
    public static class PGGUtils
    {

        #region Extensions

        public static List<Bounds> GeneratePathFindBounds(this SimplePathGuide guide)
        {
            return SimplePathGuide.GeneratePathFindBounds(guide.Start, guide.End, guide.StartDir.GetDirection2D(), guide.EndDir.GetDirection2D(), guide.PathThickness, guide.ChangeDirCost);
        }

        public static List<Bounds> GeneratePathFindBounds(this SimplePathGuide guide, List<Vector2> pathPoints)
        {
            return SimplePathGuide.GeneratePathFindBounds(guide.Start, guide.End, guide.StartDir.GetDirection2D(), guide.EndDir.GetDirection2D(), pathPoints, guide.PathThickness, guide.ChangeDirCost);
        }

        public static List<Vector2> GeneratePathFindPoints(this SimplePathGuide guide)
        {
            return SimplePathGuide.GeneratePathFindPoints(guide.Start, guide.End, guide.StartDir.GetDirection2D(), guide.EndDir.GetDirection2D(), guide.ChangeDirCost);
        }

        internal static bool Compare(float value, ESR_DistanceRule variableMustBe, float thisValue)
        {
            if (variableMustBe == ESR_DistanceRule.Equal)
            {
                return value == thisValue;
            }
            else if (variableMustBe == ESR_DistanceRule.Greater)
            {
                return value > thisValue;
            }
            else if (variableMustBe == ESR_DistanceRule.Lower)
            {
                return value < thisValue;
            }

            return false;
        }

        public static Vector2 GetProgessPositionOverLines(List<Vector2Int> pathPoints, float progress)
        {
            float fullLength = 0f;
            for (int p = 0; p < pathPoints.Count - 1; p++)
                fullLength += Vector2.Distance(pathPoints[p], pathPoints[p + 1]);

            float progressLength = fullLength * progress;

            float checkProgr = 0f;
            for (int p = 0; p < pathPoints.Count - 1; p++)
            {
                float currProgr = checkProgr;
                checkProgr += Vector2.Distance(pathPoints[p], pathPoints[p + 1]);

                if (currProgr <= progressLength && checkProgr >= progressLength)
                {
                    float progr = Mathf.InverseLerp(currProgr, checkProgr, progressLength);
                    return Vector2.Lerp(pathPoints[p], pathPoints[p + 1], progr);
                }
            }

            return Vector2.zero;
        }

        public static Vector2 GetDirectionOver(List<Vector2Int> pathPoints, int startId, int endId)
        {
            if (endId < pathPoints.Count)
                return ((Vector2)pathPoints[startId + 1] - (Vector2)pathPoints[startId]).normalized;
            else
                return ((Vector2)pathPoints[startId] - (Vector2)pathPoints[startId - 1]).normalized;
        }

        public static Vector2 GetDirectionOverLines(List<Vector2Int> pathPoints, float progress)
        {
            float fullLength = 0f;
            for (int p = 0; p < pathPoints.Count - 1; p++)
                fullLength += Vector2.Distance(pathPoints[p], pathPoints[p + 1]);

            float progressLength = fullLength * progress;

            float checkProgr = 0f;
            for (int p = 0; p < pathPoints.Count - 1; p++)
            {
                float currProgr = checkProgr;
                checkProgr += Vector2.Distance(pathPoints[p], pathPoints[p + 1]);

                if (currProgr <= progressLength && checkProgr >= progressLength)
                {
                    return ((Vector2)pathPoints[p + 1] - (Vector2)pathPoints[p]).normalized;
                }
            }

            return Vector2.zero;
        }

        #endregion


        #region Vectors

        /// <summary> V2Int ToBound V3 </summary>
        public static Vector3 V2toV3Bound(this Vector2Int v, float y = 0f)
        {
            if (y == 0f) return new Vector3(v.x - 0.5f, y, v.y - 0.5f);
            return new Vector3(v.x, y, v.y);
        }

        public static Vector3 V2toV3(this Vector2Int v, float y = 0f)
        {
            return new Vector3(v.x, y, v.y);
        }

        public static Vector3 V2toV3(this Vector2 v, float y = 0f)
        {
            return new Vector3(v.x, y, v.y);
        }

        public static Vector2Int V2toV2Int(this Vector2 v)
        {
            return new Vector2Int(Mathf.RoundToInt(v.x), Mathf.RoundToInt(v.y));
        }

        public static Vector3Int V2toV3Int(this Vector2Int v, int y = 0)
        {
            return new Vector3Int(v.x, y, v.y);
        }

        public static Vector2 V3toV2(this Vector3 v)
        {
            return new Vector2(v.x, v.z);
        }

        /// <summary>
        /// Resetting local position, rotation, scale to zero on 1,1,1 (defaults)
        /// </summary>
        public static void ResetCoords(this Transform t)
        {
            t.localPosition = Vector3.zero;
            t.localRotation = Quaternion.identity;
            t.localScale = Vector3.one;
        }

        public static Vector2Int V3toV2Int(this Vector3 v)
        {
            return new Vector2Int(Mathf.RoundToInt(v.x), Mathf.RoundToInt(v.z));
        }

        public static Vector3Int V3toV3Int(this Vector3 v)
        {
            return new Vector3Int(Mathf.RoundToInt(v.x), Mathf.RoundToInt(v.y), Mathf.RoundToInt(v.z));
        }

        public static int ToInt(this float v)
        {
            return Mathf.RoundToInt(v);
        }


        /// <summary>
        /// If direction is left/right then size is y * cellSize etc.
        /// </summary>
        public static Vector2 GetDirectionalSize(Vector2Int dir, int cellsSize)
        {
            if (cellsSize <= 1) return Vector2.one;
            if (dir.x != 0) return new Vector2(1, cellsSize);
            else return new Vector2(cellsSize, 1);
        }

        /// <summary>
        /// Getting   1,0  -1,0   0,1   0,-1
        /// </summary>
        public static Vector2Int GetFlatDirectionFrom(Vector2Int vect)
        {
            if (vect.x != 0) return new Vector2Int(vect.x, 0);
            else return new Vector2Int(0, vect.y);
        }

        /// <summary>
        /// Getting   1,0  -1,0   0,1  or   0,-1
        /// </summary>
        public static Vector2Int GetRandomDirection()
        {
            int r = FGenerators.GetRandom(0, 5);
            if (r == 0) return new Vector2Int(1, 0);
            else if(r == 1) return new Vector2Int(0, 1);
            else if(r == 2) return new Vector2Int(-1, 0);
            else return new Vector2Int(0, -1);
        }


        /// <summary>
        /// Getting   1,0  -1,0   0,1   0,-1
        /// </summary>
        public static Vector2Int GetRotatedFlatDirectionFrom(Vector2Int vect)
        {
            if (vect.x != 0) return new Vector2Int(0, vect.x);
            else return new Vector2Int(vect.y, 0);
        }

        internal static void TransferFromListToList<T>(List<T> from, List<T> to)
        {
            for (int i = 0; i < from.Count; i++)
                to.Add(from[i]);
        }

        /// <summary>
        /// Generating spawn instruction in desired direction of checker field
        /// After all set definition and add to guides list
        /// </summary>
        internal static SpawnInstruction GenerateInstructionTowards(CheckerField checker, Vector2Int start, Vector3Int dir, int centerRange = 0, bool findAlways = true)
        {
            SpawnInstruction instr = new SpawnInstruction();
            instr.desiredDirection = dir;
            instr.useDirection = true;

            Vector2Int dir2D = new Vector2Int(dir.x, dir.z);
            Vector2Int targetSquare;

            if (centerRange > 0)
            {
                targetSquare = checker.FindEdgeSquareInDirection(start - Vector2Int.one, dir2D);
                targetSquare = checker.GetCenterOnEdge(targetSquare, dir2D, centerRange + 1);
            }
            else
            {
                targetSquare = checker.FindEdgeSquareInDirection(start - new Vector2Int(0, 0), dir2D);
                if (findAlways) targetSquare = checker.GetCenterOnEdge(targetSquare, dir2D, 1);
            }

            instr.helperCoords = targetSquare.V2toV3Int();
            instr.gridPosition = checker.FromWorldToGridPos(targetSquare).V2toV3Int();

            return instr;
        }

        internal static SpawnInstruction GenerateInstructionTowards(CheckerField checker, Vector2Int start, Vector3Int dir, SingleInteriorSettings settings)
        {
            SpawnInstruction instr = GenerateInstructionTowards(checker, start, dir, settings.GetCenterRange());

            if (FGenerators.CheckIfExist_NOTNULL(settings ))
                if (settings.FieldSetup != null)
                    if (settings.DoorHoleCommandID < settings.FieldSetup.CellsCommands.Count)
                        instr.definition = settings.FieldSetup.CellsCommands[settings.DoorHoleCommandID];

            return instr;
        }

        internal static SpawnInstruction GenerateInstructionTowards(CheckerField start, CheckerField other, SingleInteriorSettings settings)
        {
            SpawnInstruction instr = GenerateInstructionTowards(start, other, settings.GetCenterRange());

            if (FGenerators.CheckIfExist_NOTNULL(settings ))
                if (settings.FieldSetup != null)
                    if (settings.DoorHoleCommandID < settings.FieldSetup.CellsCommands.Count)
                        instr.definition = settings.FieldSetup.CellsCommands[settings.DoorHoleCommandID];

            return instr;
        }

        internal static SpawnInstruction GenerateInstructionTowardsSimple(CheckerField start, CheckerField other, int centerRange)
        {
            SpawnInstruction instr = GenerateInstructionTowards(start, other, centerRange);
            return instr;
        }

        /// <summary>
        /// Generating spawn instruction on edge wall to other checker field
        /// </summary>
        /// <param name="helperCoords"> When you create hole using GenerateInstructionTowards then instruction saves used square in instr.helperCoords variable which can be useful when creating counter-door hole </param>
        internal static SpawnInstruction GenerateInstructionTowards(CheckerField start, CheckerField other, int centerRange = 0, Vector2Int? helperCoords = null)
        {
            Vector2Int nearestOwn;
            Vector2Int nearestOther;

            if (helperCoords == null)
            {
                nearestOwn = start.NearestPoint(other);
                nearestOther = other.NearestPoint(nearestOwn);
            }
            else // Used for generating counter-door-holes
            {
                nearestOwn = start.NearestPoint(helperCoords.Value);
                nearestOther = helperCoords.Value;
            }

            // Centering
            nearestOwn = start.GetCenterOnEdge(nearestOwn, nearestOther - nearestOwn, centerRange, other);
            nearestOther = other.NearestPoint(nearestOwn);

            SpawnInstruction instr = new SpawnInstruction();
            instr.helperCoords = nearestOwn.V2toV3Int();
            instr.desiredDirection = (nearestOther - nearestOwn).V2toV3Int();
            instr.useDirection = true;
            instr.gridPosition = start.FromWorldToGridPos(nearestOwn).V2toV3Int();

            return instr;
        }

        #endregion


        #region Core Utilities


        #region Rules Copy Paste Clipboard

        static List<SpawnRuleBase> ruleBaseClipboard = new List<SpawnRuleBase>();
        internal static void CopyProperties(SpawnRuleBase spawnRuleBase)
        {
            bool replaced = false;
            for (int i = 0; i < ruleBaseClipboard.Count; i++)
                if (spawnRuleBase.GetType() == ruleBaseClipboard[i].GetType())
                {
                    ruleBaseClipboard[i] = spawnRuleBase;
                    replaced = true;
                    break;
                }

            if (!replaced) ruleBaseClipboard.Add(spawnRuleBase);
        }

        internal static bool CopyProperties_FindTypeInClipboard(SpawnRuleBase spawnRuleBase)
        {
            if (spawnRuleBase._CopyPasteSupported() == false) return false;
            for (int i = 0; i < ruleBaseClipboard.Count; i++)
                if (spawnRuleBase.GetType() == ruleBaseClipboard[i].GetType())
                {
                    if (ruleBaseClipboard[i] == spawnRuleBase) return false;
                    return true;
                }
            return false;
        }

        internal static void CopyProperties_PasteTo(SpawnRuleBase spawnRuleBase, bool force)
        {
            if (spawnRuleBase._CopyPasteSupported() == false) return;
            for (int i = 0; i < ruleBaseClipboard.Count; i++)
                if (spawnRuleBase.GetType() == ruleBaseClipboard[i].GetType())
                {
                    spawnRuleBase.PasteOtherProperties(ruleBaseClipboard[i], force);
                }
        }

        #endregion


        internal static void CheckForNulls<T>(List<T> rules)
        {
            for (int i = rules.Count - 1; i >= 0; i--)
            {
                if (rules[i] == null) rules.RemoveAt(i);
            }
        }

        internal static void AdjustCount<T>(List<T> list, int targetCount) where T : new()
        {
            if (list.Count == targetCount) return;

            if ( list.Count < targetCount)
            {
                for (int i = 0; i < targetCount - list.Count; i++)
                {
                    list.Add(new T());
                }
            }
            else
            {
                for (int i = 0; i < list.Count- targetCount; i++)
                {
                    list.RemoveAt(list.Count - 1);
                }
            }
        }

        #endregion

    }
}