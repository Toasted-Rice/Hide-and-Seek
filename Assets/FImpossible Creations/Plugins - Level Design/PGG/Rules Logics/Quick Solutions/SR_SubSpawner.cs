﻿#if UNITY_EDITOR
using FIMSpace.FEditor;
using UnityEditor;
#endif
using UnityEngine;

namespace FIMSpace.Generating.Rules.QuickSolutions
{
    public class SR_SubSpawner : SpawnRuleBase, ISpawnProcedureType
    {
        public override string TitleName() { return "Sub Spawner"; }
        public override string Tooltip() { return "Use this tule to spawn additional objects when conditions are met"; }
        public EProcedureType Type { get { return EProcedureType.OnConditionsMet; } }

        [Tooltip("If you want to inject position/rotation offsets out of parent spawner")]
        public bool InheritCoords = false;
        public FieldSpawner spawner;

        public override void Refresh()
        {
            base.Refresh();
            RefreshSpawner();
        }

#if UNITY_EDITOR
        public override void NodeBody(SerializedObject so)
        {
            if (GUIIgnore.Count == 0) GUIIgnore.Add("spawner");

            base.NodeBody(so);

            RefreshSpawner();

            EditorGUILayout.BeginVertical(FGUI_Resources.BGInBoxStyleH);
            EditorGUILayout.HelpBox("WARNING: Work In Progress Node, use it wisely.", MessageType.None);
            spawner.DrawSpawnerGUI(so.FindProperty("spawner"), OwnerSpawner.Parent.GetToSpawnNames(), OwnerSpawner.Parent.GetToSpawnIndexes(), false);
            EditorGUILayout.EndVertical();
        }
#endif

        internal override bool AllowDuplicate()
        {
            return false;
        }


        public override void OnConditionsMetAction(FieldModification mod, ref SpawnData thisSpawn, FieldSetup preset, FieldCell cell, FGenGraph<FieldCell, FGenPoint> grid)
        {
            base.OnConditionsMetAction(mod, ref thisSpawn, preset, cell, grid);

            var data = spawner.RunSpawnerOnCell(mod, preset, cell, grid, Vector3.zero, null, true);

            if (data != null)
                if (InheritCoords)
                {
                    data.Offset += thisSpawn.Offset;
                    data.DirectionalOffset += thisSpawn.DirectionalOffset;
                    data.RotationOffset += thisSpawn.RotationOffset;
                    data.LocalRotationOffset += thisSpawn.LocalRotationOffset;
                }
        }

        void RefreshSpawner()
        {
            if (spawner == null)
            {
                spawner = new FieldSpawner(0, OwnerSpawner.Parent.DrawSetupFor, OwnerSpawner.Parent);
                spawner.Name = "";
            }

            if (string.IsNullOrEmpty(spawner.Name))
            {
                spawner.Enabled = true;
                spawner.Name = "Sub spawner";
            }

            spawner.Parent = OwnerSpawner.Parent;
        }

    }
}