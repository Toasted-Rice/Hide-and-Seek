﻿using System.Collections.Generic;
#if UNITY_EDITOR
using FIMSpace.FEditor;
using UnityEditor;
#endif
using UnityEngine;

namespace FIMSpace.Generating.Rules.Placement
{
    public class SR_CellNeightbours : SpawnRuleBase, ISpawnProcedureType
    {
        public override string TitleName() { return "Check cell neightbours"; }
        public override string Tooltip() { return "Flexible rule to check neightbour cells on grid\n[Mediumweight] " + base.Tooltip(); }
        public EProcedureType Type { get { return EProcedureType.Rule; } }

        public ESR_Space CheckedCellsMustBe = ESR_Space.Empty;
        [HideInInspector] public ESR_NeightbourCondition NeightbourNeeds = ESR_NeightbourCondition.AllNeeded;


        [PGG_SingleLineSwitch("CheckMode", 50, "Select if you want to use Tags, SpawnStigma or CellData", 100)]
        [HideInInspector] public string occupiedByTag = "";
        [HideInInspector] public ESR_Details CheckMode = ESR_Details.Tag;

        [HideInInspector] public bool DirectCheck = false;
        [HideInInspector] public Vector3Int OffsetOrigin = Vector3Int.zero;
        [HideInInspector] public NeightbourPlacement placement = new NeightbourPlacement();
        [HideInInspector] public QuarterRotationCheck quartRotor = new QuarterRotationCheck();
        [HideInInspector] public bool OverrideRotation = true;
        [HideInInspector] public float initRotation = 0;
        [HideInInspector] public float rotorEff = 90;
        [HideInInspector] public int spawnOn = 5;
        [HideInInspector] public bool EachRotor = false;
        [HideInInspector] public float[] CustomRotors = new float[4];

        [HideInInspector] public bool GetNeightbourPos = false;
        [HideInInspector] public bool GetNeightbourRot = false;
        [HideInInspector] public bool GetNeightbourScale = false;

        [HideInInspector] public bool CustomCellsCheck = false;
        [Tooltip("Acquiring all available rotation processes out of other nodes for direct check")]
        [HideInInspector] public bool FullRotGet = false;


        #region Editor


#if UNITY_EDITOR
        public override void NodeFooter(SerializedObject so, FieldModification mod)
        {
            if (CheckedCellsMustBe == ESR_Space.Occupied)
            {
                //EditorGUILayout.PropertyField(so.FindProperty("occupiedBy"));
                EditorGUILayout.PropertyField(so.FindProperty("occupiedByTag"));
            }


            GUILayout.Space(9);
            Color c = GUI.color;
            EditorGUILayout.BeginHorizontal();


            // Neightbours
            EditorGUILayout.BeginVertical(GUILayout.Width(90));
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Check On", EditorStyles.centeredGreyMiniLabel, new GUILayoutOption[] { GUILayout.Width(60) });
            if (DrawMultiCellSelector(placement.AdvancedSetup, OwnerSpawner))
            { CheckCellsSelectorWindow.Init(placement.AdvancedSetup, OwnerSpawner); placement.Advanced_OnSelectorSwitch(); }
            EditorGUILayout.EndHorizontal();
            NeightbourPlacement.DrawGUI(placement);
            EditorGUILayout.PropertyField(so.FindProperty("NeightbourNeeds"), GUIContent.none, GUILayout.Width(80));
            EditorGUILayout.EndVertical();



            // Rotor
            EditorGUILayout.BeginVertical(GUILayout.Width(90));
            EditorGUILayout.LabelField("Rotor Check", EditorStyles.centeredGreyMiniLabel, new GUILayoutOption[] { GUILayout.Width(70) });
            GUILayout.Space(2);

            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(10); QuarterRotationCheck.DrawGUI(quartRotor);
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(2);
            if (quartRotor.AnyChecked())
            {
                if (quartRotor.AllChecked())
                    EditorGUILayout.LabelField("(all 4 quarts)", EditorStyles.centeredGreyMiniLabel, new GUILayoutOption[] { GUILayout.Width(70) });
                else
                    if (quartRotor.OnlyOneChecked())
                {
                    if (quartRotor.ISQuarter(1))
                        EditorGUILayout.LabelField("(just 'check on')", EditorStyles.centeredGreyMiniLabel, new GUILayoutOption[] { GUILayout.Width(100) });
                    else
                        EditorGUILayout.LabelField("(rotated 'check on')", EditorStyles.centeredGreyMiniLabel, new GUILayoutOption[] { GUILayout.Width(100) });
                }
                else
                    if (quartRotor.Only180Checked())
                    EditorGUILayout.LabelField("(180° neightbours)", EditorStyles.centeredGreyMiniLabel, new GUILayoutOption[] { GUILayout.Width(108) });
                else
                    EditorGUILayout.LabelField("(90° neightbours)", EditorStyles.centeredGreyMiniLabel, new GUILayoutOption[] { GUILayout.Width(100) });
            }
            else
                EditorGUILayout.LabelField("Select at least one", EditorStyles.miniLabel, new GUILayoutOption[] { GUILayout.Width(100) });

            EditorGUIUtility.labelWidth = 50;
            EditorGUILayout.PropertyField(so.FindProperty("DirectCheck"), new GUIContent("Direct", "If neightbours offsets should be aligned with current cell rotation"));
            EditorGUIUtility.labelWidth = 0;

            EditorGUILayout.EndVertical();



            // Others
            EditorGUILayout.BeginVertical();

            EditorGUILayout.LabelField("Spawn Mods", EditorStyles.centeredGreyMiniLabel, new GUILayoutOption[] { GUILayout.Width(70) });

            EditorGUIUtility.labelWidth = 108;
            EditorGUILayout.PropertyField(so.FindProperty("OverrideRotation"), GUILayout.Width(150));

            bool preEn = GUI.enabled;
            if (OverrideRotation == false) GUI.enabled = false;

            EditorGUIUtility.labelWidth = 78;
            EditorGUILayout.PropertyField(so.FindProperty("initRotation"), GUILayout.Width(110));

            // Rotor effect
            if (quartRotor.CountChecked() > 1)
            {
                EditorGUIUtility.labelWidth = 60;
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(so.FindProperty("rotorEff"), GUILayout.Width(86));

                EditorGUIUtility.labelWidth = 1;
                EditorGUILayout.PropertyField(so.FindProperty("EachRotor"), GUILayout.Width(22));

                EditorGUILayout.EndHorizontal();
                EditorGUIUtility.labelWidth = 0;
            }

            if (OverrideRotation == false) GUI.enabled = preEn;
            EditorGUIUtility.labelWidth = 36;
            EditorGUILayout.PropertyField(so.FindProperty("OffsetOrigin"), new GUIContent("Origin", "Offset neightbours check origin by this number of cells"));
            EditorGUIUtility.labelWidth = 0;

            EditorGUIUtility.labelWidth = 14;
            EditorGUILayout.BeginHorizontal();
            SerializedProperty sp_nn = so.FindProperty("GetNeightbourPos");
            EditorGUILayout.PropertyField(sp_nn, new GUIContent("P", "Copy neightbour position offsets to this"), GUILayout.Width(32));
            sp_nn.Next(false); EditorGUILayout.PropertyField(sp_nn, new GUIContent("R", "Copy neightbour roation offsets to this"), GUILayout.Width(32));
            sp_nn.Next(false); EditorGUILayout.PropertyField(sp_nn, new GUIContent("S", "Copy neightbour scale offsets to this"), GUILayout.Width(32));
            EditorGUILayout.EndHorizontal();
            EditorGUIUtility.labelWidth = 0;

            if (DirectCheck)
            {
                EditorGUILayout.PropertyField(so.FindProperty("FullRotGet"));
            }

            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();

            if (CustomRotors == null || CustomRotors.Length == 0) CustomRotors = new float[4];
            if (EachRotor)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("Rotors", GUILayout.Width(60));
                for (int i = 0; i < CustomRotors.Length; i++)
                    CustomRotors[i] = EditorGUILayout.FloatField(GUIContent.none, CustomRotors[i], GUILayout.Width(38));
                EditorGUILayout.EndHorizontal();
            }
        }
#endif

        #endregion


        public override void CheckRuleOn(FieldModification mod, ref SpawnData spawn, FieldSetup preset, FieldCell cell, FGenGraph<FieldCell, FGenPoint> grid, Vector3? restrictDirection = null)
        {
            if (Enabled == false || Ignore) return;


            bool done = false;
            if (quartRotor.ISQuarter(1))
                if (CheckNeightbourCellAllow(cell, spawn, Vector3.zero, grid, restrictDirection))
                {
                    SetPlacementStats(ref spawn, EachRotor ? CustomRotors[0] : 0);
                    done = true;
                }

            if (!done)
                if (quartRotor.ISQuarter(2))
                    if (CheckNeightbourCellAllow(cell, spawn, new Vector3(0, 90, 0), grid, restrictDirection))
                    {
                        SetPlacementStats(ref spawn, EachRotor ? CustomRotors[1] : rotorEff);
                        done = true;
                    }

            if (!done)
                if (quartRotor.ISQuarter(3))
                    if (CheckNeightbourCellAllow(cell, spawn, new Vector3(0, 180, 0), grid, restrictDirection))
                    {
                        SetPlacementStats(ref spawn, EachRotor ? CustomRotors[2] : rotorEff * 2);
                        done = true;
                    }

            if (!done)
                if (quartRotor.ISQuarter(4))
                    if (CheckNeightbourCellAllow(cell, spawn, new Vector3(0, 270, 0), grid, restrictDirection))
                    {
                        SetPlacementStats(ref spawn, EachRotor ? CustomRotors[3] : rotorEff * 3);
                    }

        }


        SpawnData lastCorrect = null;
        private bool CheckNeightbourCellAllow(FieldCell cell, SpawnData spawn, Vector3 rotationOffset, FGenGraph<FieldCell, FGenPoint> grid, Vector3? restrictDirection = null)
        {
            bool allCorrect = false;

            Quaternion rot;

            if (DirectCheck)
            {
                if (FullRotGet)
                {
                    rot = Quaternion.Euler(spawn.GetFullRotationOffset() + rotationOffset);
                }
                else
                {
                    if ((spawn.RotationOffset + rotationOffset) == Vector3.zero)
                        rot = Quaternion.Euler(spawn.TempRotationOffset);
                    else
                        rot = Quaternion.Euler(spawn.RotationOffset + rotationOffset);
                }
            }
            else
                rot = Quaternion.Euler(rotationOffset);

            List<FieldCell> toCheck = new List<FieldCell>();

            if (placement.UseAdvancedSetup == false)
            {
                // Getting cells to check
                for (int i = 0; i < 9; i++)
                {
                    NeightbourPlacement.ENeightbour nn = (NeightbourPlacement.ENeightbour)i;
                    if (placement.IsSelected(nn) == false) continue;

                    Vector3Int offset = GetOffset(rot, NeightbourPlacement.GetDirection(nn));
                    if (restrictDirection != null) if (restrictDirection.Value != Vector3.zero) if (offset.x != restrictDirection.Value.x || offset.z != restrictDirection.Value.z) continue;

                    Vector3Int oPos = cell.Pos + OffsetOrigin + offset;
                    FieldCell cl = grid.GetCell(oPos, false);

                    toCheck.Add(cl);
                }
            }
            else // Advanced cells checking
            {
                int rotor = 0;
                if (rotationOffset.y == 90) rotor = 1;
                else if (rotationOffset.y == 180) rotor = 2;
                else if (rotationOffset.y == 270) rotor = 3;

                if (DirectCheck == false) rot = Quaternion.identity;

                if (placement.AdvancedSetup == null) placement.AdvancedSetup = new List<Vector3Int>();
                for (int i = 0; i < placement.AdvancedSetup.Count; i++)
                {
                    Vector3Int oPos = cell.Pos + OffsetOrigin + placement.Advanced_Rotate(placement.AdvancedSetup[i], rot, rotor);
                    FieldCell cl = grid.GetCell(oPos, false);
                    toCheck.Add(cl);
                }
            }

            // Checking cells with logics
            int correctCount = 0;

            for (int i = 0; i < toCheck.Count; i++)
            {
                if (SpawnRules.CheckNeightbourCellAllow(CheckedCellsMustBe, toCheck[i], occupiedByTag, CheckMode))
                {
                    if (NeightbourNeeds == ESR_NeightbourCondition.AtLeastOne)
                    {
                        allCorrect = true;
                        if (string.IsNullOrEmpty(occupiedByTag) == false) lastCorrect = GetSpawnDataWithSpecifics(toCheck[i], occupiedByTag, CheckMode);
                        break;
                    }
                    else if (NeightbourNeeds == ESR_NeightbourCondition.AllNeeded)
                    {
                        correctCount += 1;
                        if (placement.UseAdvancedSetup == false)
                        {
                            if (correctCount == placement.SelectedCount())
                            {
                                if (string.IsNullOrEmpty(occupiedByTag) == false) lastCorrect = GetSpawnDataWithSpecifics(toCheck[i], occupiedByTag, CheckMode);
                                allCorrect = true;
                                break;
                            }
                        }
                        else
                        {
                            if (correctCount == placement.AdvancedSetup.Count)
                            {
                                if (string.IsNullOrEmpty(occupiedByTag) == false) lastCorrect = GetSpawnDataWithSpecifics(toCheck[i], occupiedByTag, CheckMode);
                                allCorrect = true;
                                break;
                            }
                        }
                    }
                }
            }


            return allCorrect;
        }




        public override void ResetRule(FGenGraph<FieldCell, FGenPoint> grid, FieldSetup preset)
        {
            base.ResetRule(grid, preset);
            rot = null;
            lastCorrect = null;
        }


        public Vector3? rot = null;

        public void SetPlacementStats(ref SpawnData spawn, float? angle = null)
        {
            if (angle != null)
            {
                rot = Vector3.up * (initRotation + angle.Value);
                if (OverrideRotation) if (rot != null) spawn.RotationOffset = rot.Value;
            }

            CellAllow = true;
        }


        public override void CellInfluence(FieldSetup preset, FieldModification mod, FieldCell cell, ref SpawnData spawn, FGenGraph<FieldCell, FGenPoint> grid)
        {
            if (OverrideRotation) if (rot != null) spawn.RotationOffset = rot.Value;
            if (FGenerators.CheckIfExist_NOTNULL(lastCorrect))
            {
                if (GetNeightbourPos) lastCorrect.CopyPositionTo(spawn);
                if (GetNeightbourRot) lastCorrect.CopyRotationTo(spawn);
                if (GetNeightbourScale) lastCorrect.CopyScaleTo(spawn);
            }
        }

    }
}