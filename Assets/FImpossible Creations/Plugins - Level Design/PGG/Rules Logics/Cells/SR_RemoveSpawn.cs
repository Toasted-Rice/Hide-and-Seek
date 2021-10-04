#if UNITY_EDITOR
using UnityEditor;
#endif

using FIMSpace.Generating.Rules.Helpers;

namespace FIMSpace.Generating.Rules.Cells
{
    public class SR_RemoveSpawn : SpawnRuleBase, ISpawnProcedureType
    {
        public EProcedureType Type { get { return EProcedureType.OnConditionsMet; } }
        public override string TitleName() { return "Remove Spawn"; }
        public override string Tooltip() { return "Removing desired spawn if some conditions are met"; }

        public RemoveInstruction Remove;

#if UNITY_EDITOR
        public override void NodeFooter(SerializedObject so, FieldModification mod)
        {
            GUIIgnore.Clear(); GUIIgnore.Add("Remove");
            base.NodeFooter(so, mod);

            var sp = so.FindProperty("Remove");

            if (sp != null) RemoveInstruction.DrawGUI(sp, Remove);

            so.ApplyModifiedProperties();
        }
#endif

        public override void OnConditionsMetAction(FieldModification mod, ref SpawnData thisSpawn, FieldSetup preset, FieldCell cell, FGenGraph<FieldCell, FGenPoint> grid)
        {
            Remove.ProceedRemoving(OwnerSpawner, ref thisSpawn, cell, grid);
        }

    }
}