using UnityEditor;
using UnityEngine;

namespace FIMSpace.Generating
{
    [UnityEditor.CanEditMultipleObjects]
    [UnityEditor.CustomEditor(typeof(FieldSetup))]
    public class FieldSetupEditor : UnityEditor.Editor
    {
        public FieldSetup Get { get { if (_get == null) _get = (FieldSetup)target; return _get; } }
        private FieldSetup _get;

        public override void OnInspectorGUI()
        {
            if (GUILayout.Button("Open Field Setup in designer window", GUILayout.Height(38))) AssetDatabase.OpenAsset(Get);
            EditorGUILayout.HelpBox("Field Setup should be edited through FieldDesigner window", MessageType.Info);

            GUILayout.Space(4f);

            DrawDefaultInspector();
            GUILayout.Space(4f);

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Rename"))
            {
                string filename = EditorUtility.SaveFilePanelInProject("Type your title (no file will be created)", Get.name, "", "Type your title (no file will be created)");
                if (!string.IsNullOrEmpty(filename))
                {
                    filename = System.IO.Path.GetFileNameWithoutExtension(filename);
                    if (!string.IsNullOrEmpty(filename))
                    {
                        Get.name = filename;
                        string path = AssetDatabase.GetAssetPath(Get);
                        string noExt = path.Replace(".asset", "");
                        noExt += filename + ".asset";

                        AssetDatabase.RenameAsset(path, noExt);
                        EditorUtility.SetDirty(Get);
                        AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(Get));
                    }
                }
            }

            //if (GUILayout.Button("Clone"))
            //{

            //}

            EditorGUILayout.EndHorizontal();
        }
    }
}