﻿using FIMSpace.FEditor;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace FIMSpace.Generating
{
    public class SeparatedModWindow : EditorWindow
    {
        public static SeparatedModWindow Get;
        Vector2 mainScroll = Vector2.zero;
        bool flex = true;
        public FieldModification latestMod;

        [MenuItem("Window/FImpossible Creations/Level Design/Separated Field Modificator Window (Beta)", false, 51)]
        static void Init()
        {
            SeparatedModWindow window = (SeparatedModWindow)GetWindow(typeof(SeparatedModWindow));
            window.titleContent = new GUIContent("Field Mod", Resources.Load<Texture>("SPR_ModificationSmall"));
            window.Show();
            window.minSize = new Vector2(440, 450);
            //window.maxSize = new Vector2(900, 700);
            Get = window;
        }

        private void OnEnable()
        {
            Get = this;
        }

        public static void SelectMod(FieldModification mod, bool show = true)
        {
            SeparatedModWindow window = (SeparatedModWindow)GetWindow(typeof(SeparatedModWindow));
            Get = window;
            Get.latestMod = mod;
            Get.prem = null;

            if (show)
            {
                window = (SeparatedModWindow)GetWindow(typeof(SeparatedModWindow));
                window.Show();
            }
        }

        [OnOpenAssetAttribute(1)]
        public static bool OpenFieldScriptableFile(int instanceID, int line)
        {
            Object obj = EditorUtility.InstanceIDToObject(instanceID);
            if (obj as FieldModification != null)
            {
                Init();
                Get.latestMod = (FieldModification)obj;
                return true;
            }

            return false;
        }

        FieldModification prem = null;

        private void OnGUI()
        {
            //try//
            {
                EditorGUIUtility.labelWidth = 340;
                flex = EditorGUILayout.Toggle("Toggle this if there is too many vertical elements to view", flex);
                EditorGUIUtility.labelWidth = 0;

                mainScroll = EditorGUILayout.BeginScrollView(mainScroll);
                EditorGUILayout.HelpBox("Field Modificator Window is not yet looking fully correct when there are many mod rules to view", MessageType.None);

                GUILayout.Space(5);

                latestMod = (FieldModification)EditorGUILayout.ObjectField("Edited Modificator", latestMod, typeof(FieldModification), false);


                if (latestMod == null)
                    if (Selection.activeObject is FieldModification)
                    {
                        latestMod = (FieldModification)Selection.activeObject;
                        mainScroll = Vector2.zero;
                    }

                if (latestMod == null)
                {
                    EditorGUILayout.HelpBox("Select some Field Modificator to edit it in this window", MessageType.Info);
                    GUILayout.Space(5);
                    flex = EditorGUILayout.Toggle(flex);
                    mainScroll = Vector2.zero;
                }
                else
                {
                    if (prem != latestMod)
                    {
                        FieldModificationEditor.RefreshSpawnersList(latestMod);
                        mainScroll = Vector2.zero;
                    }



                    SerializedObject so = new SerializedObject(latestMod);
                    FieldModificationEditor.DrawHeaderGUI(so, latestMod);


                    bool pre = EditorGUIUtility.wideMode;
                    bool preh = EditorGUIUtility.hierarchyMode;
                    EditorGUIUtility.wideMode = true;
                    EditorGUIUtility.hierarchyMode = true;

                    if (flex)
                        EditorGUILayout.BeginVertical(FGUI_Resources.BGInBoxBlankStyle, GUILayout.Height(2200));
                    else
                        EditorGUILayout.BeginVertical(FGUI_Resources.BGInBoxBlankStyle, GUILayout.ExpandHeight(true), GUILayout.MinHeight(position.height));

                    FieldModificationEditor.DrawInspectorGUI(so, latestMod);
                    if (!flex) GUILayout.FlexibleSpace();


                    GUILayout.Space(5);
                    EditorGUIUtility.hierarchyMode = preh;
                    EditorGUIUtility.wideMode = pre;

                    EditorGUILayout.EndVertical();

                }

                EditorGUILayout.EndScrollView();
                prem = latestMod;
            }
            //catch (System.Exception exc)
            //{
            //    if ( PGGInspectorUtilities.LogPGGWarnings)
            //    {
            //        UnityEngine.Debug.Log(exc);
            //    }
            //}
        }

    }
}
