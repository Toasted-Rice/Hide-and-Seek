﻿
#if UNITY_EDITOR

using FIMSpace.FEditor;
using FIMSpace.Generating.Rules;
using System;
using UnityEditor;
#if UNITY_2019_4_OR_NEWER
using UnityEditor.IMGUI.Controls;
#endif
using UnityEngine;

namespace FIMSpace.Generating
{
    public partial class FieldSpawner
    {
        public bool DisplayPreviewGUI = true;
        public bool IsSubSpawner = false;

        public void DrawInspector()
        {

#if UNITY_2019_4_OR_NEWER
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("+ Add Rule +"))
            {
                GenericMenu menu = new GenericMenu();
                Parent.AddContextMenuItems(menu, this);
                menu.DropDown(new Rect(Event.current.mousePosition + Vector2.left * 100, Vector2.zero));
            }

            if (GUILayout.Button("Add Rule +Search (beta)"))
            {
                var dropdown = new RulesDropdown(new AdvancedDropdownState());
                RulesDropdown.AddTo = this;
                dropdown.Show(new Rect(Event.current.mousePosition + new Vector2(-210, 200), new Vector2(250, 480)));
            }

            if (FGenerators.CheckIfExist_NOTNULL(RulesDropdown.Selected))
            {
                if (RulesDropdown.AddTo == this)
                {
                    try { if (RulesDropdown.Selected.toAdd != null) AddRule(RulesDropdown.Selected.GetInstance()); } catch (Exception) { }
                    RulesDropdown.Selected = null;
                }
            }

            EditorGUILayout.EndHorizontal();
#else
            if (GUILayout.Button("+ Add Rule +"))
            {
                GenericMenu menu = new GenericMenu();
                Parent.AddContextMenuItems(menu, this);
                menu.DropDown(new Rect(Event.current.mousePosition + Vector2.left * 100, Vector2.zero));
            }
#endif

        }

        [HideInInspector] public bool _EditorDisplaySpawnerHeader = true;

        public void DrawSpawnerGUIHeader(SerializedProperty sp_sp, GUIContent[] spawners, int[] indexes, bool drawOrderButton = true)
        {
            if (_EditorDisplaySpawnerHeader == false) return;

            FieldSpawner spawner = this;
            GUILayout.Space(4);

            EditorGUILayout.BeginHorizontal();
            spawner.Enabled = EditorGUILayout.Toggle(spawner.Enabled, GUILayout.Width(24));
            spawner.Name = EditorGUILayout.TextField(spawner.Name);
            EditorGUIUtility.labelWidth = 28;

            float targetWidth = EditorStyles.label.CalcSize(new GUIContent(spawner.SpawnerTag)).x + 40;
            if (targetWidth < 78) targetWidth = 78; if (targetWidth > 160) targetWidth = 160;

            spawner.SpawnerTag = EditorGUILayout.TextField(new GUIContent("Tag:" +
                "", "You can use multiple tags by separating them with ',' commas"), spawner.SpawnerTag, GUILayout.Width(targetWidth));
            EditorGUIUtility.labelWidth = 0;

            if (GUILayout.Button(EditorGUIUtility.IconContent(spawner.DisplayPreviewGUI ? "animationvisibilitytoggleon" : "animationvisibilitytoggleoff"), FGUI_Resources.ButtonStyle, new GUILayoutOption[] { GUILayout.Width(24), GUILayout.Height(22) })) spawner.DisplayPreviewGUI = !spawner.DisplayPreviewGUI;
            EditorGUILayout.EndHorizontal();


            StampPrefabID = EditorGUILayout.IntPopup(new GUIContent("To Spawn"), StampPrefabID, spawners, indexes);

            //EditorGUILayout.PropertyField(sp_sp.FindPropertyRelative("OnConditionsMet"));
            if (drawOrderButton) EditorGUILayout.PropertyField(sp_sp.FindPropertyRelative("CellCheckMode"));

            GUILayout.Space(8);
        }

        public void DrawSpawnerGUIBody(bool drawManagement = true)
        {
            FieldSpawner spawner = this;

            bool preE = GUI.enabled;
            if (spawner.Enabled == false) GUI.enabled = false;

            if (spawner.Rules == null) spawner.Rules = new System.Collections.Generic.List<SpawnRuleBase>();

            for (int i = 0; i < spawner.Rules.Count; i++)
                if (spawner.Rules[i] != null) DrawRule(spawner.Parent, -1, spawner, i, drawManagement);

            if (spawner.Enabled == false) GUI.enabled = preE;
        }

        public void DrawSpawnerGUI(SerializedProperty sp_sp, GUIContent[] spawners, int[] indexes, bool drawOrderButton = true, bool drawAddButtons = true, bool drawManagement = true)
        {
            FieldSpawner spawner = this;

            if (sp_sp != null)
            {
                spawner.DrawSpawnerGUIHeader(sp_sp, spawners, indexes, drawOrderButton);
                spawner.DrawSpawnerGUIBody(drawManagement);
                if (drawAddButtons) spawner.DrawInspector();
            }
        }


        public static void DrawRule(FieldModification Get, int selected, FieldSpawner spawner, int i, bool drawManagement = true)
        {
            Color bg = GUI.backgroundColor;
            Color c = GUI.color;

            SpawnRuleBase rule = spawner.Rules[i];
            SerializedObject so_rule = new SerializedObject(rule);

            Color? colored = null;
            bool drawNegate = true;
            bool drawLogic = true;
            bool isTyped = false;
            int height = 20;

            GUIStyle headerStyle = FGUI_Resources.BGInBoxLightStyle;
            ISpawnProceduresDecorator decor = rule as ISpawnProceduresDecorator;


            #region Decoring

            if (rule is ISpawnProcedureType)
            {
                isTyped = true;
                ISpawnProcedureType t = rule as ISpawnProcedureType;

                if (t.Type != SpawnRuleBase.EProcedureType.Rule)
                {
                    if (t.Type == SpawnRuleBase.EProcedureType.Event)
                    {
                        colored = SpawnRuleBase.GetProcedureColor(SpawnRuleBase.EProcedureType.Event);
                        drawNegate = false;
                        drawLogic = false;
                    }
                    else if (t.Type == SpawnRuleBase.EProcedureType.OnConditionsMet)
                    {
                        colored = SpawnRuleBase.GetProcedureColor(SpawnRuleBase.EProcedureType.OnConditionsMet);
                        drawNegate = false;
                        drawLogic = false;
                    }
                    else if (t.Type == SpawnRuleBase.EProcedureType.Coded)
                    {
                        colored = SpawnRuleBase.GetProcedureColor(SpawnRuleBase.EProcedureType.Coded);
                        drawNegate = true;
                        drawLogic = true;
                    }
                }
                else
                    colored = new Color(0.5f, 1f, .5f, 1f);
            }
            else
            {
                if (decor != null)
                {
                    drawNegate = false;
                    drawLogic = false;
                    colored = decor.DisplayColor;
                    headerStyle = decor.DisplayStyle;
                    height = decor.DisplayHeight;

                    GUILayout.Space(decor.UpPadding);

                    if (rule is ISpawnProcedureType == false) { rule.Ignore = true; }
                }
            }

            #endregion


            #region Drawing rule


            drawLogic = rule.DrawLogicSwitch; // TODO: BETA

            if (colored != null) GUI.color = colored.Value;

            EditorGUILayout.BeginVertical(/*GUILayout.Width(220)*/); // 1
            EditorGUILayout.BeginHorizontal(headerStyle, GUILayout.Height(height)); // 1

            if (colored != null) GUI.color = c;

            if (drawManagement)
            {
                if (rule.Ignore == false) rule.Enabled = EditorGUILayout.Toggle(rule.Enabled, GUILayout.Width(24));
            }

            rule.NodeHeader();


            if (rule.CanBeGlobal() == false) isTyped = false;
            if (rule.DisableDrawingGlobalSwitch) isTyped = false;

            if (isTyped)
            {
                if (rule.Global) GUI.backgroundColor = Color.gray;
                if (GUILayout.Button(new GUIContent(new GUIContent("G", "Make rule global for all spawners in this modification")), GUILayout.Width(20))) { rule.Global = !rule.Global; }
                if (rule.Global) GUI.backgroundColor = bg;
            }

            if (rule.CanBeNegated() == false) drawNegate = false;
            if (drawNegate)
            {
                if (rule.Negate) GUI.backgroundColor = Color.gray;
                if (GUILayout.Button(new GUIContent(rule.Negate ? "Negate!" : "#", "Make rule requirements negative\nSame like if (!something == true)"), GUILayout.Width(rule.Negate ? 60 : 18))) { rule.Negate = !rule.Negate; }
                GUI.backgroundColor = bg;
            }



            if (spawner.Rules.Count > 1)
            {
                if (drawLogic)
                {
                    int width = 70;
                    if (rule.Logic == SpawnRuleBase.ERuleLogic.AND_Required)
                        width = 46;
                    else if (rule.Logic == SpawnRuleBase.ERuleLogic.OR)
                        width = 40;
                    //else if (rule.Logic == SpawnRuleBase.ERuleLogic.BREAK_Stop)
                    //    width = 58;
                    //else if (rule.Logic == SpawnRuleBase.ERuleLogic.HardStopSpawning)
                    //    width = 78;

                    EditorGUILayout.PropertyField(so_rule.FindProperty("Logic"), GUIContent.none, GUILayout.Width(width));
                }
            }
            else
                if (spawner.Rules.Count > 0)
                if (spawner.Rules[0] != null)
                    spawner.Rules[0].Logic = SpawnRuleBase.ERuleLogic.AND_Required;


            int hh = height - 1;

            if (drawManagement)
            {
                if (i > 0) if (GUILayout.Button(new GUIContent(FGUI_Resources.Tex_ArrowUp), FGUI_Resources.ButtonStyle, GUILayout.Width(18), GUILayout.Height(hh))) { FGenerators.SwapElements(spawner.Rules, i, i - 1); return; }
                if (i < spawner.Rules.Count - 1) if (GUILayout.Button(new GUIContent(FGUI_Resources.Tex_ArrowDown), FGUI_Resources.ButtonStyle, GUILayout.Width(18), GUILayout.Height(hh))) { FGenerators.SwapElements(spawner.Rules, i, i + 1); return; }

                if (Get) if (GUILayout.Button("X", FGUI_Resources.ButtonStyle, GUILayout.Width(24), GUILayout.Height(hh)))
                    {
                        //if (spawner.IsSubSpawner)//else
                        spawner.RemoveRule(rule);
                        //Get.Spawners[selected].RemoveRule(rule);
                        return;
                    }
            }

            EditorGUILayout.EndHorizontal(); // 1

            bool en = GUI.enabled;
            if (rule.Enabled == false) GUI.enabled = false;


            if (rule._editor_drawRule)
            {
                try
                {
                    EditorGUI.BeginChangeCheck();

                    if (colored != null) GUI.color = colored.Value * 0.7f;
                    rule._editor_scroll = EditorGUILayout.BeginScrollView(rule._editor_scroll, FGUI_Resources.BGInBoxStyleH);
                    if (colored != null) GUI.color = c;

                    SerializedObject so = new SerializedObject(rule);
                    rule.NodeBody(so);

                    if (rule.EditorDrawPublicProperties())
                    {
                        SerializedProperty sp = so.GetIterator();
                        sp.Next(true);
                        sp.NextVisible(false);
                        bool can = sp.NextVisible(false);

                        if (can)
                        {

                            do
                            {
                                bool cont = false;

                                // Check ignored properties to draw
                                for (int ig = 0; ig < rule.GUIIgnore.Count; ig++)
                                    if (sp.name == rule.GUIIgnore[ig]) { cont = true; break; }

                                if (cont) continue;

                                EditorGUILayout.PropertyField(sp);
                            }
                            while (sp.NextVisible(false) == true);

                        }
                    }

                    if (Get) rule.NodeFooter(so, Get);

                    so.ApplyModifiedProperties();

                    EditorGUILayout.EndScrollView();


                    if (EditorGUI.EndChangeCheck())
                    {
                        EditorUtility.SetDirty(rule);
                        so.Update();
                    }

                }
                catch (Exception exc)
                {
                    if (PGGInspectorUtilities.LogPGGWarnings)
                    {
                        UnityEngine.Debug.Log("Exception when drawing gui, probably harmless");
                        Debug.Log(exc);
                    }
                }
            }


            GUI.enabled = en;
            so_rule.ApplyModifiedProperties();


            EditorGUILayout.EndVertical(); // 1
            GUILayout.Space(6);

            #endregion


            if (decor != null) GUILayout.Space(decor.DownPadding);

        }

        internal void DuplicateRule(SpawnRuleBase spawnRuleBase)
        {
            if (spawnRuleBase == null) return;
            AddRule(GameObject.Instantiate(spawnRuleBase));
        }
    }
}

#endif