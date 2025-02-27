﻿using System;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;

namespace FIMSpace.Generating.Rules.Operations
{
    public class SR_StackSpawner : SpawnRuleBase, ISpawnProcedureType
    {
        public override string TitleName() { return "Stack Spawner"; }
        public override string Tooltip() { return "Spawning multiple instances of choosed prefabs stacked one on another using object stamper algorithms\n" + base.Tooltip(); }
        public override bool CanBeGlobal() { return false; }
        public override bool CanBeNegated() { return false; }

        public EProcedureType Type { get { return EProcedureType.Coded; } }

        public Vector3 DropCastOrigin = Vector3.up;
        public Vector2 DropArea = new Vector2(0.5f, 0.5f);
        public float RaycastDistance = 10f;
        public LayerMask CollisionsLayer = 1 << 0;
        [Space(6)]
        [Range(0f, 1.15f)]
        public float OverlapRestriction = 0.9f;
        [Range(0f, 1.15f)]
        public float MinimumStandSpace = 0.8f;

        [Space(6)]
        public MinMax TargetSpawnCount = new MinMax(3, 5);

        [Space(6)]
        public bool Debug = false;

        #region Editor

#if UNITY_EDITOR
        public override void NodeBody(SerializedObject so)
        {
            EditorGUILayout.HelpBox("To see result in preview, remember to enable 'Run Additional Generators' in 'Test Generating Settings'!", MessageType.None);
            base.NodeBody(so);
        }
#endif

        #endregion

        public override void CheckRuleOn(FieldModification mod, ref SpawnData spawn, FieldSetup preset, FieldCell cell, FGenGraph<FieldCell, FGenPoint> grid, Vector3? restrictDirection = null)
        {
            base.CheckRuleOn(mod, ref spawn, preset, cell, grid, restrictDirection);
            CellAllow = true;
        }

        public override void CellInfluence(FieldSetup preset, FieldModification mod, FieldCell cell, ref SpawnData spawn, FGenGraph<FieldCell, FGenPoint> grid)
        {
            _EditorDebug = Debug;
        }

        public override void OnConditionsMetAction(FieldModification mod, ref SpawnData thisSpawn, FieldSetup preset, FieldCell cell, FGenGraph<FieldCell, FGenPoint> grid)
        {
            SpawnData spwn = thisSpawn;

            Action<GameObject> stackSpawn =
            (o) =>
            {
                GameObject spawner = new GameObject("Spawner");
                Matrix4x4 mx = GetMatrix(spwn);

                spawner.transform.position = mx.MultiplyPoint(Vector3.zero);
                spawner.transform.rotation = mx.rotation;

                var me = spawner.AddComponent<ObjectStampMultiEmitter>();
                me.MultiSet = new OStamperMultiSet();
                me.MultiSet.name = "0";
                me.MultiSet.PrefabsSets = new List<OStamperSet>();

                if (OwnerSpawner.Mode == FieldModification.EModificationMode.ObjectsStamp)
                {
                    if (mod.OStamp) me.MultiSet.PrefabsSets.Add(mod.OStamp);
                }
                else if (OwnerSpawner.Mode == FieldModification.EModificationMode.ObjectMultiEmitter)
                {
                    if (mod.OMultiStamp) me.MultiSet = mod.OMultiStamp;
                }
                else
                {
                    OStamperSet spawns;
                    spawns = new OStamperSet();
                    spawns.Prefabs = new List<OSPrefabReference>();
                    spawns.RayCheckLayer = CollisionsLayer;
                    spawns.OverlapCheckMask = CollisionsLayer;
                    spawns.RayDistanceMul = RaycastDistance;
                    spawns.OverlapCheckScale = OverlapRestriction;
                    spawns.MinimumStandSpace = MinimumStandSpace;

                    if (OwnerSpawner.MultipleToSpawn == false)
                    {
                        if (OwnerSpawner.StampPrefabID < 0) // Random
                        {
                            for (int i = 0; i < mod.PrefabsList.Count; i++)
                            {
                                var pRefs = new OSPrefabReference();
                                pRefs.Prefab = mod.PrefabsList[i].Prefab;
                                spawns.Prefabs.Add(pRefs);
                                pRefs.OnPrefabChanges();
                            }
                        }
                        else
                        {
                            var pRefs = new OSPrefabReference();
                            pRefs.Prefab = spwn.Prefab;
                            spawns.Prefabs.Add(pRefs);
                            pRefs.OnPrefabChanges();
                        }
                    }
                    else // Multiple to spawn
                    {
                        var selected = FEngineering.GetLayermaskValues(OwnerSpawner.StampPrefabID, mod.GetPRSpawnOptionsCount());
                        for (int i = 0; i < selected.Length; i++)
                        {
                            var pRefs = new OSPrefabReference();
                            pRefs.Prefab = mod.PrefabsList[selected[i]].Prefab;
                            spawns.Prefabs.Add(pRefs);
                            pRefs.OnPrefabChanges();
                        }
                    }

                    spawns.name = "0";

                    me.MultiSet.PrefabSetSettings = new List<OStamperMultiSet.MultiStamperSetParameters>();
                    OStamperMultiSet.MultiStamperSetParameters mPar = new OStamperMultiSet.MultiStamperSetParameters();
                    mPar.Prefab = spwn.Prefab;
                    mPar.TargetSet = spawns;
                    me.MultiSet.PrefabSetSettings.Add(mPar);

                    mPar.MinPrefabsSpawnCount = TargetSpawnCount.Min;
                    mPar.MaxPrefabsSpawnCount = TargetSpawnCount.Max;
                    mPar.MaxSpawnCountForWholeSet = TargetSpawnCount.Max;

                    me.MultiSet.PrefabsSets.Add(spawns);
                }

                me.Areas = new List<ObjectStampMultiEmitter.SpawnArea>();
                var sArea = new ObjectStampMultiEmitter.SpawnArea("0");
                sArea.Size = DropArea;
                sArea.Center = Vector3.zero;
                sArea.Sets = new List<int>();
                sArea.Sets.Add(0);
                sArea.Multiply = new List<float>();
                sArea.Multiply.Add(1f);
                me.Areas.Add(sArea);

                me.MultiSet.PrefabSetSettings[0].RefreshReference();
                me.MultiSet.PrefabSetSettings[0].OnPrefabChanges();
                me.MultiSet.PrefabsSets[0].RefreshBounds();

                spwn.AdditionalGenerated = new List<GameObject>();
                spwn.AdditionalGenerated.Add(spawner);
                spwn.DontSpawnMainPrefab = true;
            };

            thisSpawn.OnGeneratedEvents.Add(stackSpawn);

        }

        Matrix4x4 GetMatrix(SpawnData spawn)
        {
            Quaternion spawnRot = spawn.GetRotationOffset();
            Vector3 pos = spawn.GetWorldPositionWithFullOffset() + spawnRot * DropCastOrigin;
            return Matrix4x4.TRS(pos, spawnRot, Vector3.one);
        }

#if UNITY_EDITOR
        public override void OnDrawDebugGizmos(FieldSetup preset, SpawnData spawn, FieldCell cell, FGenGraph<FieldCell, FGenPoint> grid)
        {
            base.OnDrawDebugGizmos(preset, spawn, cell, grid);

            Gizmos.color = new Color(0.8f, 1f, 0.8f, 0.3f);
            Gizmos.matrix = GetMatrix(spawn);

            Gizmos.DrawCube(Vector3.zero, new Vector3(DropArea.x, 0.02f, DropArea.y));

            Gizmos.matrix = Matrix4x4.identity;
            Gizmos.color = _DbPreCol;
        }
#endif

    }
}