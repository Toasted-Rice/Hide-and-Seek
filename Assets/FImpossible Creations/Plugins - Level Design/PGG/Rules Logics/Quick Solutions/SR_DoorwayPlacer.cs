#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace FIMSpace.Generating.Rules.QuickSolutions
{
    public class SR_DoorwayPlacer : SpawnRuleBase, ISpawnProcedureType
    {
        public override string TitleName() { return "Doorway Placer"; }
        public override string Tooltip() { return "Quick solution for placing doorway using cell's guides, it's combination of 'Check Cell Neightbours' 'Direct Offset' and 'Remove in Position' rules"; }
        public EProcedureType Type { get { return EProcedureType.Coded; } }

        [PGG_SingleLineSwitch("CheckMode", 50, "Select if you want to use Tags, SpawnStigma or CellData", 110)]
        public string replaceOnTag = "";
        [HideInInspector] public ESR_Details CheckMode = ESR_Details.Tag;

        [PGG_SingleLineSwitch("OffsetMode", 58, "Select if you want to offset postion with cell size or world units", 140, 5)]
        public Vector3 Offset = Vector3.zero;
        [HideInInspector] public ESR_Measuring OffsetMode = ESR_Measuring.Units;
        public float YawRotationOffset = 0f;

        [Space(4)]
        [PGG_SingleLineSwitch("DistanceSource", 78, "Choose if you want to measure distance from prefab origin or first-mesh bounds center", 140, 5)]
        [Tooltip("Distance from 'replaceOnTag' object which will be removed")]
        public float RemoveDistance = 0.1f;
        [HideInInspector] public ESR_Origin DistanceSource = ESR_Origin.SpawnPosition;

        [Space(5)]
        [Tooltip("[TURN TO NONE AFTER DEBUGGING!] When you want to debug if rule is working inside FieldSetup Designer window -> Setting Forward direction as target direction")]
        public EPlanGuideDirecion DebugDirection = EPlanGuideDirecion.None;

        private SpawnData targetSpawn = null;
        public override void CheckRuleOn(FieldModification mod, ref SpawnData spawn, FieldSetup preset, FieldCell cell, FGenGraph<FieldCell, FGenPoint> grid, Vector3? restrictDirection = null)
        {
            CellAllow = false; // Not allow until rules met
            //posToApply = null; rotToApply = null; // Applying for CellInfluence() method

            if (Enabled == false || Ignore)
            {
                return;
            }

            // Determinate desired direction
            // From cell instruction or get direction to nearest spawn with desired tag
            Vector3 targetDirection = Vector3.zero;

            SpawnData anySpawn = null;
            if (cell.GetJustCellSpawnCount() > 0) anySpawn = cell.GetSpawnsJustInsideCell()[0];

            if (restrictDirection != null)
                targetDirection = restrictDirection.Value;
            else
            {
                if (FGenerators.CheckIfExist_NOTNULL(anySpawn))
                    targetDirection = anySpawn.GetRotationOffset() * Vector3.forward;
            }


            if (targetDirection.sqrMagnitude == 0)
            {
                if (DebugDirection == EPlanGuideDirecion.None)
                {
                    return; // No direction
                }
                else
                    targetDirection = DebugDirection.GetDirection();
            }


            // Check if there is desired tagged object on cell with right distance
            targetSpawn = null;

            var spawns = cell.CollectSpawns(OwnerSpawner.ScaleAccess);
            float nearest = float.MaxValue;

            Quaternion dir = Quaternion.LookRotation(targetDirection);
            Vector3 targetPosInCell = dir * Offset;
            targetPosInCell = GetUnitOffset(targetPosInCell, OffsetMode, preset);

            Vector3 thisPos = spawn.GetPosWithFullOffset(true);
            thisPos += GetUnitOffset(dir * Offset, OffsetMode, preset);


            //if (string.IsNullOrEmpty(replaceOnTag) == false)
            //    if (targetSpawn is null)
            //        return; // Not found needed spawn on this cell

            // Applying coords

            //posToApply = thisPos;
            spawn.TempPositionOffset = targetPosInCell;
            spawn.Offset = targetPosInCell;
            spawn.DirectionalOffset = Vector3.zero;

            //rotToApply = dir.eulerAngles + new Vector3(0f, YawRotationOffset, 0f);
            spawn.RotationOffset = dir.eulerAngles + new Vector3(0f, YawRotationOffset, 0f);
            spawn.TempRotationOffset = spawn.RotationOffset;


            Vector3 thisMeasurePos = thisPos;
            if (DistanceSource == ESR_Origin.BoundsCenter)
            {
                if (spawn.Prefab == null)
                {
#if UNITY_EDITOR    
                    UnityEngine.Debug.Log("[PGG Doorway Node] No Prefab in " + OwnerSpawner.Name + " for measure using bounds!");
#endif
                    thisMeasurePos = spawn.GetPosWithFullOffset(true);
                    //return;
                }
                else
                {
                    if (spawn.PreviewMesh)
                        thisMeasurePos = thisPos + spawn.GetRotationOffset() * Vector3.Scale(spawn.Prefab.transform.localScale, spawn.PreviewMesh.bounds.center);
                    else
                        thisMeasurePos = spawn.GetPosWithFullOffset(true);

                }
            }
            else if (DistanceSource == ESR_Origin.RendererCenter)
            {
                if (spawn.Prefab == null)
                {
#if UNITY_EDITOR    
                    UnityEngine.Debug.Log("[PGG Doorway Node] No Prefab in " + OwnerSpawner.Name + " for measure using renderer!");
#endif
                    thisMeasurePos = spawn.GetPosWithFullOffset(true);
                    //return;
                }
                else
                {
                    Renderer thisRend = spawn.Prefab.GetComponentInChildren<Renderer>();
                    if (thisRend)
                    {
                        thisMeasurePos = spawn.GetPosWithFullOffset(true) + spawn.GetRotationOffset() * Vector3.Scale(spawn.Prefab.transform.localScale, spawn.Prefab.transform.InverseTransformPoint(thisRend.bounds.center));
                    }
                    else
                        thisMeasurePos = spawn.GetPosWithFullOffset(true);

                }
            }


            Vector3 spawnPos;
            // Finding nearest cell spawn in desired placement
            for (int s = 0; s < spawns.Count; s++)
            {
                if (spawns[s].OwnerMod == null) continue;
                if (spawns[s] == spawn) continue;

                if (string.IsNullOrEmpty(replaceOnTag) == false)  // assigned tag to search
                    if (SpawnHaveSpecifics(spawns[s], replaceOnTag, CheckMode) == false)
                        continue; // Not found required tags then skip this spawn

                spawnPos = spawns[s].GetPosWithFullOffset(true);
                //spawnPos = spawns[s].GetFullOffset(true);
                float distance;

                if (DistanceSource == ESR_Origin.SpawnPosition)
                {
                    distance = Vector3.Distance(thisPos, spawnPos);
                    //Debug.DrawRay(thisPos, Vector3.up * 5f, Color.blue, 1f);
                    //Debug.DrawRay(spawnPos, Vector3.up * 5f, Color.red, 1f);
                }
                else if (DistanceSource == ESR_Origin.BoundsCenter)
                {

                    if (spawns[s].PreviewMesh == null)
                        distance = Vector3.Distance(thisPos, spawnPos);
                    else
                    {
                        distance = Vector3.Distance(thisMeasurePos,
                            spawns[s].GetPosWithFullOffset(true) + spawns[s].GetRotationOffset() * Vector3.Scale(spawns[s].Prefab.transform.localScale, spawns[s].GetRotationOffset() * spawns[s].PreviewMesh.bounds.center));
                    }

                }
                else //if (DistanceSource == ESR_Origin.ColliderCenter)
                {
                    Renderer thisRend = null;

                    if (spawn.Prefab != null)
                        thisRend = spawn.Prefab.GetComponentInChildren<Renderer>();
                    else
                        thisPos = thisMeasurePos;

                    Renderer otherSpawnRend = null;
                    if (spawns[s].Prefab)
                        otherSpawnRend = spawns[s].Prefab.GetComponentInChildren<Renderer>();


                    if ( thisRend == null)
                    {
                        if ( otherSpawnRend == null) // All null
                        {
                            distance = Vector3.Distance(thisPos, spawnPos);
                        }
                        else // Just other exists
                        {
                            distance = Vector3.Distance(thisMeasurePos,
                            spawns[s].GetPosWithFullOffset(true) + spawns[s].GetRotationOffset() * Vector3.Scale(spawns[s].Prefab.transform.localScale, spawns[s].Prefab.transform.InverseTransformPoint(otherSpawnRend.bounds.center)));
                        }
                    }
                    else // This renderer exists
                    {
                        if (otherSpawnRend == null) // Other renderer not exists
                        {
                            distance = Vector3.Distance(thisPos, spawnPos);
                        }
                        else // All renderer exists
                        {
                            distance = Vector3.Distance(thisMeasurePos,
                            spawns[s].GetPosWithFullOffset(true) + spawns[s].GetRotationOffset() * Vector3.Scale(spawns[s].Prefab.transform.localScale, spawns[s].Prefab.transform.InverseTransformPoint(otherSpawnRend.bounds.center)));
                        }
                    }
                }

                if (distance > RemoveDistance) continue; // Dont remove spawn if it's too far

                if (distance < nearest)
                {
                    targetSpawn = spawns[s]; // remembering previous object spawn to be removed at OnConditionsMet
                    nearest = distance;
                }
            }


            CellAllow = true;
        }


        public override void OnConditionsMetAction(FieldModification mod, ref SpawnData thisSpawn, FieldSetup preset, FieldCell cell, FGenGraph<FieldCell, FGenPoint> grid)
        {
            if (FGenerators.CheckIfIsNull(targetSpawn))
                return;

            cell.RemoveSpawnFromCell(targetSpawn);
            targetSpawn.Enabled = false;
        }

#if UNITY_EDITOR
        public override void NodeFooter(SerializedObject so, FieldModification mod)
        {
            if (DebugDirection != EPlanGuideDirecion.None)
            {
                GUILayout.Space(4);
                EditorGUILayout.HelpBox("Switch DebugDirection back to None when you finish debugging!", MessageType.Warning);
            }

            base.NodeFooter(so, mod);
        }

#endif

    }

}