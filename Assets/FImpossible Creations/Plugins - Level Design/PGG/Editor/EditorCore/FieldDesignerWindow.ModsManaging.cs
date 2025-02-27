﻿using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using FIMSpace.FEditor;
using System.Linq;

namespace FIMSpace.Generating
{
    public partial class FieldDesignWindow
    {
        /// <summary>
        /// Drawing Modificators Packs
        /// </summary>
        void DrawMods()
        {
            EditorGUI.BeginChangeCheck();

            if (!projectPreset.ModificatorPacks.Contains(projectPreset.RootPack)) projectPreset.ModificatorPacks.Add(projectPreset.RootPack);

            if (projectPreset == null) return;
            if (projectPreset.RootPack == null) return;
            projectPreset.RootPack.ParentPreset = projectPreset;

            DrawModificatorPackFieldSetupList(projectPreset.RootPack, projectPreset.ModificatorPacks, FGUI_Resources.BGInBoxStyle, "FieldSetup's Modification Packs", ref drawPacks, ref selectedPackIndex, true, true, projectPreset, true);

            // Selection check
            for (int i = 0; i < projectPreset.ModificatorPacks.Count; i++)
            {
                if (Selection.objects.Contains(projectPreset.ModificatorPacks[i]))
                {
                    if (selectedPackIndex != i) if (projectPreset.ModificatorPacks[i].FieldModificators.Count > 0) AssetDatabase.OpenAsset(projectPreset.ModificatorPacks[i].FieldModificators[0]);

                    selectedPackIndex = i;
                    break;
                }
            }

            if (selectedPackIndex >= 0)
            {
                // Drawing Selected Modificator Pack -----------------------------------
                if (selectedPackIndex >= projectPreset.ModificatorPacks.Count) selectedPackIndex = 0;

                var modPack = projectPreset.ModificatorPacks[selectedPackIndex];

                if (modPack != null)
                {
                    ModificatorsPackEditor.DrawFieldModList(modPack, modPack == projectPreset.RootPack ? "Built-In Field's Mods" : modPack.name, ref drawPack, modPack, true, projectPreset);
                }
            }

            if (EditorGUI.EndChangeCheck() || CheckCellsSelectorWindow.GetChanged()) TriggerRefresh(false);
        }


        /// <summary>
        /// Modificator packs are always outside project files, except built-in packages in room preset
        /// </summary>
        public static void DrawModificatorPackFieldSetupList(ModificatorsPack basePack, List<ModificatorsPack> toDraw, GUIStyle style, string title, ref bool foldout, ref int selected, bool newButton = false, bool moveButtons = false, UnityEngine.Object toDirty = null, bool drawEnableDisablePackSwitch = false)
        {
            if (toDraw == null) return;

            Color bgc = GUI.backgroundColor;
            Color preC = GUI.color;

            GUI.color = Color.green;
            EditorGUILayout.BeginVertical(style);
            GUI.color = bgc;

            EditorGUILayout.BeginHorizontal();
            string fold = foldout ? " ▼" : " ►";
            if (GUILayout.Button(fold + "  " + title + " (" + toDraw.Count + ")", EditorStyles.label, GUILayout.Width(234))) foldout = !foldout;

            if (foldout)
            {
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("+"))
                {
                    toDraw.Add(null);
                    EditorUtility.SetDirty(toDirty);
                }
            }

            EditorGUILayout.EndHorizontal();

            if (foldout)
            {
                GUILayout.Space(4);

                if (toDraw.Count > 0)
                    for (int i = 0; i < toDraw.Count; i++)
                    {

                        bool isBasePackage = false;
                        if (toDraw[i] == basePack) isBasePackage = true;

                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.BeginHorizontal();

                        if (toDraw[i] != null)
                        {
                            EditorGUI.BeginChangeCheck();
                            toDraw[i].DisableWholePackage = !EditorGUILayout.Toggle(!toDraw[i].DisableWholePackage, GUILayout.Width(16));
                            if (EditorGUI.EndChangeCheck()) EditorUtility.SetDirty(toDraw[i]);
                            if (toDraw[i].DisableWholePackage) GUI.color = new Color(1f, 1f, 1f, 0.5f);
                        }

                        GUIContent lbl = new GUIContent(isBasePackage ? "Base" : i.ToString());
                        float wdth = EditorStyles.label.CalcSize(lbl).x;

                        //EditorGUILayout.LabelField(lbl, );
                        if (selected == i) GUI.backgroundColor = Color.green;

                        if (GUILayout.Button(lbl, GUILayout.Width(wdth + 8)))
                        {
                            if (selected == i) selected = -1;
                            else
                            {
                                selected = i;
                                if (toDraw[i]) if (toDraw[i].FieldModificators.Count > 0) AssetDatabase.OpenAsset(toDraw[i].FieldModificators[0]);
                            }
                        }

                        GUI.backgroundColor = preC;

                        bool preE = GUI.enabled;
                        if (isBasePackage) GUI.enabled = false; else EditorGUI.BeginChangeCheck();
                        toDraw[i] = (ModificatorsPack)EditorGUILayout.ObjectField(toDraw[i], typeof(ModificatorsPack), false);
                        if (isBasePackage)
                        {
                            GUI.enabled = preE;
                            ModificatorsPackEditor.DrawRenameScriptableButton(toDraw[i]);
                        }
                        else if (EditorGUI.EndChangeCheck()) EditorUtility.SetDirty(toDirty);

                        if (!isBasePackage)
                            if (newButton)
                                if (toDraw[i] == null)
                                    if (GUILayout.Button(new GUIContent("New", "Generate new ModificatorsPack file in project assets"), GUILayout.Width(52)))
                                    {
                                        ModificatorsPack tempPreset = (ModificatorsPack)FGenerators.GenerateScriptable(ScriptableObject.CreateInstance<ModificatorsPack>(), "ModPack_");
                                        AssetDatabase.SaveAssets();
                                        if (AssetDatabase.Contains(tempPreset)) if (tempPreset != null) toDraw[i] = tempPreset;
                                        EditorUtility.SetDirty(toDirty);
                                    }

                        if (moveButtons)
                        {
                            EditorGUI.BeginChangeCheck();
                            if (i > 0) if (GUILayout.Button(new GUIContent(FGUI_Resources.Tex_ArrowUp, "Move this element to be executed before above one"), GUILayout.Width(24))) { ModificatorsPack temp = toDraw[i - 1]; toDraw[i - 1] = toDraw[i]; toDraw[i] = temp; }
                            if (i < toDraw.Count - 1) if (GUILayout.Button(new GUIContent(FGUI_Resources.Tex_ArrowDown, "Move this element to be executed after below one"), GUILayout.Width(24))) { ModificatorsPack temp = toDraw[i + 1]; toDraw[i + 1] = toDraw[i]; toDraw[i] = temp; }

                            if (EditorGUI.EndChangeCheck()) EditorUtility.SetDirty(toDirty);
                        }

                        if (!isBasePackage)
                            if (GUILayout.Button("X", GUILayout.Width(24))) { toDraw.RemoveAt(i); EditorUtility.SetDirty(toDirty); break; }

                        GUI.color = preC;
                        EditorGUILayout.EndHorizontal();
                        //if (toDirty != null) if (EditorGUI.EndChangeCheck()) { EditorUtility.SetDirty(toDirty); }
                    }
                else
                {
                    EditorGUILayout.LabelField("No object in list", EditorStyles.centeredGreyMiniLabel);
                }
            }

            EditorGUILayout.EndVertical();
        }


        public static readonly int varPerPages = 8;
        public static void DrawCellInstructionDefinitionsList(ref int page, List<InstructionDefinition> toDraw, GUIStyle style, string title, ref bool foldout, bool moveButtons = false, FieldSetup toDirty = null)
        {
            if (toDraw == null) return;

            Color bgc = GUI.backgroundColor;

            int pagesCount = Mathf.FloorToInt(toDraw.Count / varPerPages);

            EditorGUI.BeginChangeCheck();

            GUI.color = Color.green;
            EditorGUILayout.BeginVertical(style);
            GUI.color = bgc;

            EditorGUILayout.BeginHorizontal();
            string fold = foldout ? " ▼" : " ►";
            if (GUILayout.Button(fold + "  " + title + " (" + toDraw.Count + ")", EditorStyles.label, GUILayout.Width(208))) foldout = !foldout;

            if (foldout)
            {
                GUILayout.FlexibleSpace();

                if (pagesCount > 0)
                {
                    GUILayout.Space(2);
                    if (GUILayout.Button("◄", EditorStyles.label, GUILayout.Width(20))) { page -= 1; }
                    GUILayout.Space(5);
                    GUILayout.BeginVertical();
                    GUILayout.Space(2); GUILayout.Label((page + 1) + "/" + (pagesCount + 1), FGUI_Resources.HeaderStyle);
                    GUILayout.EndVertical();
                    GUILayout.Space(5);
                    if (GUILayout.Button("►", EditorStyles.label, GUILayout.Width(20))) { page += 1; }

                    if (page < 0) page = pagesCount;
                    if (page > pagesCount) page = 0;
                    GUILayout.Space(3);
                }

                if (GUILayout.Button("+", GUILayout.Width(20)))
                {
                    toDraw.Add(null);
                    EditorUtility.SetDirty(toDirty);
                }
            }

            EditorGUILayout.EndHorizontal();

            if (foldout)
            {
                GUILayout.Space(4);

                int startIndex = page * varPerPages;

                if (toDraw.Count > 0)
                    for (int i = startIndex; i < Mathf.Min(startIndex + varPerPages, toDraw.Count); i++)
                    {
                        if (toDraw[i] == null) continue;

                        GUI.backgroundColor = bgc;

                        bool preE = GUI.enabled;
                        EditorGUILayout.BeginHorizontal();

                        if (toDraw[i].Foldout) GUI.backgroundColor = new Color(0.5f, 1f, 0.5f, 1f);
                        if (GUILayout.Button("[" + i + "]", FGUI_Resources.ButtonStyle, GUILayout.Width(24), GUILayout.Height(18)))
                        {

                            if (toDirty)
                            {
                                if (FGenerators.IsRightMouseButton())
                                {
                                    GenericMenu menu = new GenericMenu();
                                    FieldModification modd = toDraw[i].TargetModification;

                                    if (modd)
                                        menu.AddItem(new GUIContent("Export Copy"), false, () => { ModificatorsPackEditor.ExportCopyPopup(modd); });
                                    if (modd)
                                        menu.AddItem(new GUIContent("Export Variant"), false, () => { ModificatorsPackEditor.ExportVariantPopup(modd); });

                                    menu.AddItem(new GUIContent(""), false, () => { });

                                    if (modd)
                                        menu.AddItem(new GUIContent("Prepare for Copy"), false, () => { ModificatorsPackEditor.PrepareForCopy(modd); });

                                    if (ModificatorsPackEditor.PreparedToCopyModReference != null)
                                    {
                                        InstructionDefinition src = toDraw[i];
                                        menu.AddItem(new GUIContent("Replace with Copy"), false, () =>
                                        {
                                            var duplicate = ModificatorsPackEditor.GetDuplicateOfPreparedToCopy();
                                            FGenerators.AddScriptableTo(duplicate, toDirty, true, true);
                                            src.TargetModification = duplicate;
                                            EditorUtility.SetDirty(toDirty);
                                            AssetDatabase.SaveAssets();
                                        });
                                    }

                                    FGenerators.DropDownMenu(menu);
                                }
                                else
                                    toDraw[i].Foldout = !toDraw[i].Foldout;
                            }
                            else
                                toDraw[i].Foldout = !toDraw[i].Foldout;

                        }
                        GUI.backgroundColor = bgc;

                        toDraw[i].Title = EditorGUILayout.TextField(toDraw[i].Title);
                        toDraw[i].InstructionType = (InstructionDefinition.EInstruction)EditorGUILayout.EnumPopup(toDraw[i].InstructionType);

                        if (moveButtons)
                        {
                            if (i > 0) if (GUILayout.Button(new GUIContent(FGUI_Resources.Tex_ArrowUp), GUILayout.Width(24))) { var temp = toDraw[i - 1]; toDraw[i - 1] = toDraw[i]; toDraw[i] = temp; }
                            if (i < toDraw.Count - 1) if (GUILayout.Button(new GUIContent(FGUI_Resources.Tex_ArrowDown), GUILayout.Width(24))) { var temp = toDraw[i + 1]; toDraw[i + 1] = toDraw[i]; toDraw[i] = temp; }
                        }

                        //

                        if (GUILayout.Button("X", GUILayout.Width(24))) { toDraw.RemoveAt(i); EditorUtility.SetDirty(toDirty); break; }

                        EditorGUILayout.EndHorizontal();

                        if (toDraw[i].Foldout)
                        {
                            InstructionDefinitionEditor.DrawGUI(toDraw[i], toDirty);
                        }

                        FGUI_Inspector.DrawUILine(0.3f, 0.6f, 1, 5);

                        //GUILayout.Space(7);

                        //if (toDirty != null) if (EditorGUI.EndChangeCheck()) { EditorUtility.SetDirty(toDirty); }
                    }
                else
                {
                    EditorGUILayout.LabelField("No object in list", EditorStyles.centeredGreyMiniLabel);
                }
            }

            EditorGUILayout.EndVertical();

            if (EditorGUI.EndChangeCheck()) if (toDirty != null) EditorUtility.SetDirty(toDirty);
        }




        public static void DrawFieldVariablesList(List<FieldVariable> toDraw, GUIStyle style, string title, ref int selected, ref EDrawVarMode mode, UnityEngine.Object toDirty = null, bool selfInjections = true)
        {
            if (toDraw == null) return;

            Color bgc = GUI.backgroundColor;

            EditorGUI.BeginChangeCheck();

            GUI.color = Color.blue;
            EditorGUILayout.BeginVertical(style);
            GUI.color = bgc;

            EditorGUILayout.BeginHorizontal();
            string fold = (selected != -1) ? " ▼" : " ►";
            if (GUILayout.Button(fold + "  " + title + " (" + toDraw.Count + ")", EditorStyles.label, GUILayout.Width(208))) { if (selected != -1) selected = -1; else selected = -2; }

            if (selected != -1)
            {
                GUILayout.FlexibleSpace();

                mode = (EDrawVarMode)EditorGUILayout.EnumPopup(mode, GUILayout.Width(80));

                if (GUILayout.Button("+"))
                {
                    toDraw.Add(new FieldVariable("Variable " + toDraw.Count, 1f));
                    if (toDirty != null) EditorUtility.SetDirty(toDirty);
                }
            }

            EditorGUILayout.EndHorizontal();

            if (selected != -1)
            {
                GUILayout.Space(4);

                if (toDraw.Count > 0)
                {
                    for (int i = 0; i < toDraw.Count; i++)
                    {
                        if (toDraw[i] == null) continue;

                        if (mode != EDrawVarMode.All)
                        {
                            if (mode == EDrawVarMode.GameObjects)
                            {
                                if (toDraw[i].ValueType != FieldVariable.EVarType.GameObject) continue;
                            }
                            else if (mode == EDrawVarMode.Materials)
                            {
                                if (toDraw[i].ValueType != FieldVariable.EVarType.Material) continue;
                            }
                            else if (mode == EDrawVarMode.Variables)
                            {
                                if (toDraw[i].ValueType == FieldVariable.EVarType.GameObject || toDraw[i].ValueType == FieldVariable.EVarType.Material) continue;
                            }
                        }

                        var v = toDraw[i];
                        EditorGUILayout.BeginHorizontal();
                        if (selected == i) GUI.backgroundColor = Color.green;
                        if (GUILayout.Button("[" + i + "]", GUILayout.Width(26))) { if (selected == i) selected = -2; else selected = i; }
                        if (selected == i) GUI.backgroundColor = Color.white;
                        GUILayout.Space(6);

                        GUIContent cName = new GUIContent(v.Name);
                        float width = EditorStyles.textField.CalcSize(cName).x + 6;
                        if (width > 220) width = 220;

                        v.Name = EditorGUILayout.TextField(v.Name, GUILayout.Width(width));
                        GUILayout.Space(6);

                        if (v.ValueType == FieldVariable.EVarType.Float)
                        {
                            EditorGUIUtility.labelWidth = 10;
                            if (v.helper == Vector3.zero) v.Float = EditorGUILayout.FloatField(" ", v.Float);
                            else v.Float = EditorGUILayout.Slider(" ", v.Float, v.helper.x, v.helper.y);
                        }
                        else if (v.ValueType == FieldVariable.EVarType.Bool)
                        {
                            EditorGUIUtility.labelWidth = 70;
                            v.SetValue(EditorGUILayout.Toggle("Default:", v.GetBoolValue()));
                        }
                        else if (v.ValueType == FieldVariable.EVarType.Material)
                        {
                            EditorGUIUtility.labelWidth = 70;
                            v.SetValue((Material)EditorGUILayout.ObjectField("Material:", v.GetMaterialRef(), typeof(Material), false));
                        }
                        else if (v.ValueType == FieldVariable.EVarType.GameObject)
                        {
                            EditorGUIUtility.labelWidth = 70;
                            v.SetValue((GameObject)EditorGUILayout.ObjectField("Object:", v.GetGameObjRef(), typeof(GameObject), false));
                        }
                        else if (v.ValueType == FieldVariable.EVarType.Vector3)
                        {
                            EditorGUIUtility.labelWidth = 70;
                            v.SetValue(EditorGUILayout.Vector3Field("", v.GetVector3Value()));
                        }

                        EditorGUIUtility.labelWidth = 0;

                        GUILayout.Space(6);
                        if (GUILayout.Button("X", GUILayout.Width(24))) { toDraw.RemoveAt(i); if (toDirty != null) EditorUtility.SetDirty(toDirty); break; }

                        EditorGUILayout.EndHorizontal();

                        if (selected == i)
                        {
                            EditorGUILayout.BeginHorizontal();
                            v.ValueType = (FieldVariable.EVarType)EditorGUILayout.EnumPopup(v.ValueType, GUILayout.Width(56));
                            GUILayout.Width(6);

                            if (v.ValueType == FieldVariable.EVarType.Float)
                            {
                                EditorGUIUtility.labelWidth = 44;
                                v.helper.x = EditorGUILayout.FloatField("Min:", v.helper.x);
                                GUILayout.Space(8);
                                v.helper.y = EditorGUILayout.FloatField("Max:", v.helper.y);
                                EditorGUIUtility.labelWidth = 0;
                            }
                            //else if ( v.ValueType == FieldVariable.EVarType.Bool)
                            //{
                            //    v.SetValue( EditorGUILayout.Toggle("Default:", v.GetBoolValue()));
                            //}

                            EditorGUILayout.EndHorizontal();

                        }

                    }
                }
                else
                {
                    EditorGUILayout.LabelField("No object in list", EditorStyles.centeredGreyMiniLabel);
                }


                GUILayout.Space(5);

                if (selfInjections)
                {
                    //Get.drawSelfInj = EditorGUILayout.Foldout(Get.drawSelfInj, "Self Injections", true);
                    //if (Get.drawSelfInj)
                    //{
                    EditorGUI.indentLevel++;

                    GUILayout.Space(4);
                    EditorGUILayout.PropertyField(Get.so_preset.FindProperty("SelfInjections"));
                    GUILayout.Space(4);

                    EditorGUI.indentLevel--;
                    //}
                }

            }


            EditorGUILayout.EndVertical();

            if (EditorGUI.EndChangeCheck()) if (toDirty != null) EditorUtility.SetDirty(toDirty);
        }


    }
}