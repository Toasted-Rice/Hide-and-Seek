#if UNITY_EDITOR
using FIMSpace.FEditor;
using UnityEditor;
#endif

using System.Collections.Generic;
using UnityEngine;
using FIMSpace.Generating.Rules.Helpers;

namespace FIMSpace.Generating.Rules.Cells
{
    public class SR_RemoveSpawnsTool : SpawnRuleBase, ISpawnProcedureType
    {
        public EProcedureType Type { get { return EProcedureType.OnConditionsMet; } }
        public override string TitleName() { return "Remove Spawns Tool"; }
        public override string Tooltip() { return "Removing desired spawn if some conditions are met, multiple conditions can be defined within this single rule"; }

        public List<RemoveInstruction> Removing = new List<RemoveInstruction>();


#if UNITY_EDITOR
        private SerializedProperty sp_list;
        private int selectedElement = 0;
        public override void NodeFooter(SerializedObject so, FieldModification mod)
        {
            GUIIgnore.Clear(); GUIIgnore.Add("Removing");
            base.NodeFooter(so, mod);

            sp_list = so.FindProperty("Removing");

            if (sp_list != null)
            {
                if (sp_list.arraySize == 0 && Removing.Count == 0) Removing.Add(new RemoveInstruction());

                EditorGUILayout.BeginHorizontal();

                EditorGUILayout.LabelField("Remove Instructions (" + sp_list.arraySize + ")", GUILayout.Width(154));
                GUILayout.FlexibleSpace();

                if (sp_list.arraySize > 1)
                {
                    EditorGUILayout.BeginHorizontal();
                    if (GUILayout.Button(new GUIContent(FGUI_Resources.Tex_ArrowLeft), GUILayout.Width(20))) selectedElement--;
                    EditorGUILayout.LabelField((selectedElement + 1) + " / " + (sp_list.arraySize), FGUI_Resources.HeaderStyle, GUILayout.Width(40));
                    if (GUILayout.Button(new GUIContent(FGUI_Resources.Tex_ArrowRight), GUILayout.Width(20))) selectedElement++;
                    EditorGUILayout.EndHorizontal();
                    GUILayout.Space(6);

                    if (selectedElement > sp_list.arraySize - 1) selectedElement = 0;
                    if (selectedElement < 0) selectedElement = sp_list.arraySize - 1;
                }

                if (GUILayout.Button("+", GUILayout.Width(20))) Removing.Add(new RemoveInstruction());
                if (sp_list.arraySize > 1) if (GUILayout.Button("-", GUILayout.Width(20)))
                    {
                        Removing.RemoveAt(selectedElement);
                        if (selectedElement > Removing.Count - 1) selectedElement = 0;
                        if (selectedElement < 0) selectedElement = Removing.Count - 1;
                        return;
                    }

                EditorGUILayout.EndHorizontal();
                if (selectedElement >= sp_list.arraySize) selectedElement = 0;

                if (selectedElement < sp_list.arraySize)
                {
                    SerializedProperty sp_r = sp_list.GetArrayElementAtIndex(selectedElement);
                    GUILayout.Space(4);
                    RemoveInstruction.DrawGUI(sp_r, Removing[selectedElement]);
                }

            }
            else
            {
                UnityEngine.Debug.Log("Cant find prop Removing ");
            }

            so.ApplyModifiedProperties();
        }
#endif


        public override void OnConditionsMetAction(FieldModification mod, ref SpawnData thisSpawn, FieldSetup preset, FieldCell cell, FGenGraph<FieldCell, FGenPoint> grid)
        {
            for (int i = 0; i < Removing.Count; i++)
            {
                Removing[i].ProceedRemoving(OwnerSpawner, ref thisSpawn, cell, grid);
            }
        }


    }
}