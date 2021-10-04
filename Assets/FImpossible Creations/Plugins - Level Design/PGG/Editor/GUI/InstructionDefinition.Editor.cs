using FIMSpace.FEditor;
using FIMSpace.Generating.Planning;
using UnityEditor;
using UnityEngine;
using FIMSpace.Generating;

namespace FIMSpace.Generating
{

    public static class InstructionDefinitionEditor
    {

#if UNITY_EDITOR
        public static void DrawGUI(InstructionDefinition def, FieldSetup setup)
        {
            if (def.InstructionType == InstructionDefinition.EInstruction.PreventSpawnSelective)
            {
                def.Tags = EditorGUILayout.TextField(new GUIContent("Tags:"), def.Tags);
            }
            else if (def.InstructionType == InstructionDefinition.EInstruction.DoorHole)
            {
                EditorGUILayout.HelpBox("Door Hole: Prevents spawning and after all mods runs modificator", MessageType.None);
                def.Tags = EditorGUILayout.TextField(new GUIContent("Prevent Spawn Tagged:"), def.Tags);
                DrawMod("Door Modificator:", ref def.TargetModification, setup, false);
            }
            else if (def.InstructionType == InstructionDefinition.EInstruction.PreRunModificator)
            {
                DrawMod("To Run:", ref def.TargetModification, setup, false);
            }
            else if (def.InstructionType == InstructionDefinition.EInstruction.PostRunModificator)
            {
                DrawMod("To Run:", ref def.TargetModification, setup, false);
            }
            else if (def.InstructionType == InstructionDefinition.EInstruction.InjectStigma)
            {
                def.Tags = EditorGUILayout.TextField(new GUIContent("On Tags:"), def.Tags);
                def.InstructionArgument = EditorGUILayout.TextField(new GUIContent("Spawn Stigma:"), def.InstructionArgument);
            }
            else if (def.InstructionType == InstructionDefinition.EInstruction.InjectDataString)
            {
                //def.Tags = EditorGUILayout.TextField(new GUIContent("On Tags:"), def.Tags);
                def.InstructionArgument = EditorGUILayout.TextField(new GUIContent("Cell Data String:"), def.InstructionArgument);
            }
            else if (def.InstructionType == InstructionDefinition.EInstruction.IsolatedGrid)
            {
                EditorGUILayout.HelpBox("IsolatedGrid is not working with scaled grids mode", MessageType.None);
                DrawMod("To Run:", ref def.TargetModification, setup, false);
            }
        }

        private static void DrawMod(string title, ref FieldModification mod, FieldSetup setup, bool drawExportButtons = true)
        {
            EditorGUILayout.BeginHorizontal();
            mod = (FieldModification)EditorGUILayout.ObjectField(new GUIContent(title), mod, typeof(FieldModification), false);

            if (mod == null)
            {
                mod = ModificatorsPackEditor.DrawNewScriptableCreateButton<FieldModification>();
                if (setup != null) mod = ModificatorsPackEditor.DrawModInjectButton(null, null, setup);
            }
            else
            {
                FieldModification nMod;

                if (mod != null)
                {
                    ModificatorsPackEditor.DrawRenameScriptableButton(mod);
                }

                if (drawExportButtons)
                    if (mod.VariantOf == null)
                    {
                        nMod = ModificatorsPackEditor.DrawVariantExportButton(mod);
                        if (nMod != null) mod = nMod;
                    }

                if (setup != null)
                {
                    if (FGenerators.AssetContainsAsset(mod, setup) == false)
                    {
                        nMod = ModificatorsPackEditor.DrawModInjectButton(mod, null, setup);
                        if (nMod != null) mod = nMod;
                    }
                }

                if (drawExportButtons)
                {
                    nMod = ModificatorsPackEditor.DrawExportModButton(mod);
                    if (nMod != null) mod = nMod;
                }
            }

            EditorGUILayout.EndHorizontal();
        }
#endif

    }
}