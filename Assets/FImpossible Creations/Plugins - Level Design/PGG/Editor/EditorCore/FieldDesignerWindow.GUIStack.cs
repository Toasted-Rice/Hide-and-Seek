using UnityEngine;
using UnityEditor;
using FIMSpace.FEditor;
using FIMSpace.Hidden;

namespace FIMSpace.Generating
{
    public partial class FieldDesignWindow
    {
        enum EDesignerGridMode { RectangleGrid, RandomGenerate, Paint, BranchedGeneration }
        EDesignerGridMode gridMode = EDesignerGridMode.RectangleGrid;

        bool drawPreviewSetts = false;
        bool drawTestGenSetts = false;
        bool drawPresetParams = true;
        bool drawInstructions = false;
        //bool drawSelfInj = false;
        int drawVariables = -1;
        public enum EDrawVarMode { All, Variables, Materials, GameObjects }
        EDrawVarMode drawVariablesMode = EDrawVarMode.All;
        bool drawPacks = false;
        bool drawPack = true;
        int selectedPackIndex = 0;

        [HideInInspector]
        public FieldSetup UsingDraft = null;
        static int variablesPage = 0;

        void DrawFieldGenWindowGUI(FieldSetup set)
        {

            #region Preview Settings

            FGUI_Inspector.FoldHeaderStart(ref drawPreviewSetts, " Scene Preview Settings", FGUI_Resources.HeaderBoxStyle);
            EditorGUI.BeginChangeCheck();

            if (drawPreviewSetts)
            {
                GUILayout.Space(3);
                EditorGUILayout.BeginHorizontal();
                EditorGUIUtility.labelWidth = 90;
                AutoRefreshPreview = EditorGUILayout.Toggle(new GUIContent("Auto Preview", "Automatically run all rules on grid every change, good for using AUTO SPAWN or PREVIEW (For PREVIEW : Scene Preview Settings -> Alpha set to higher than zero)"), AutoRefreshPreview);

                GUILayout.FlexibleSpace();
                EditorGUIUtility.labelWidth = 40;
                if (PreviewAlpha < Mathf.Epsilon)
                {
                    EditorGUIUtility.fieldWidth = 26;
                    PreviewAlpha = EditorGUILayout.Slider("Alpha", PreviewAlpha, 0f, 1f);
                    EditorGUILayout.LabelField(new GUIContent(FGUI_Resources.Tex_Info, "Preview without spawning is disabled -> It's quicker than spawning but needs clean mesh setup inside prefabs"), GUILayout.Width(16));
                }
                else
                {
                    PreviewAlpha = EditorGUILayout.Slider("Alpha", PreviewAlpha, 0f, 1f);
                }

                EditorGUIUtility.fieldWidth = 0;

                EditorGUILayout.EndHorizontal();

                GUILayout.Space(1);

                EditorGUILayout.BeginHorizontal();
                EditorGUIUtility.labelWidth = 110;
                ColorizePreview = EditorGUILayout.Toggle("Colorize Preview", ColorizePreview);

                EditorGUIUtility.labelWidth = 78;

                GUILayout.FlexibleSpace();
                DrawScreenGUI = EditorGUILayout.Toggle("Screen GUI", DrawScreenGUI);
                //if (AutoRefreshPreview)
                //{
                //    GUILayout.FlexibleSpace();
                //    PreviewAutoSpawn = EditorGUILayout.Toggle("Auto Spawn", PreviewAutoSpawn);
                //}

                EditorGUILayout.EndHorizontal();

                GUILayout.Space(1);
                EditorGUILayout.BeginHorizontal();
                EditorGUIUtility.labelWidth = 92;
                DrawGrid = EditorGUILayout.Toggle("Draw Grid", DrawGrid);

                GUILayout.FlexibleSpace();
                EditorGUIUtility.labelWidth = 110;
                AutoDestroy = EditorGUILayout.Toggle(new GUIContent("Destroy On Close", "Destroy generated preview objects on closing window"), AutoDestroy);
                //EditorGUIUtility.labelWidth = 62;
                //Repose = EditorGUILayout.Toggle("Repose", Repose);
                EditorGUILayout.EndHorizontal();

                GUILayout.Space(5);
            }

            if (EditorGUI.EndChangeCheck())
            {
                repaint = true;
            }

            EditorGUI.BeginChangeCheck();

            EditorGUILayout.EndVertical();

            #endregion

            GUILayout.Space(4f);


            #region Test Generating Grid Settings


            // Generation Help Fields

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.BeginHorizontal();
            FGUI_Inspector.FoldHeaderStart(ref drawTestGenSetts, new GUIContent(" Test Generating Settings"), FGUI_Resources.FoldStyle, null);
            
            // Center Button
            SceneView view = SceneView.lastActiveSceneView;
            if (view != null)
                if (view.camera != null)
                {
                    float referenceScale = 8f;
                    if (projectPreset != null) referenceScale = projectPreset.GetCellUnitSize().x * 8;


                    if (Vector3.Distance(view.camera.transform.position, new Vector3(0, referenceScale, -referenceScale)) > referenceScale)
                    {
                        GUILayout.FlexibleSpace();
                        if (GUILayout.Button(new GUIContent(" Center", EditorGUIUtility.IconContent("Camera Icon").image), FGUI_Resources.ButtonStyle, GUILayout.Height(19)))
                        {
                            FrameCenter(referenceScale);
                        }
                    }

                    float angleDiff = Quaternion.Angle(view.camera.transform.rotation, Quaternion.identity);

                    if (angleDiff > 125)
                    {
                        if (GUILayout.Button(new GUIContent(EditorGUIUtility.IconContent("RotateTool").image), FGUI_Resources.ButtonStyle, GUILayout.Width(22), GUILayout.Height(19)))
                        {
                            FrameCenter(referenceScale, true);
                        }
                    }
                }

            EditorGUILayout.EndHorizontal();


            if (drawTestGenSetts)
            {
                GUILayout.Space(5);
                EditorGUILayout.BeginVertical(FGUI_Resources.BGInBoxStyle);
                EditorGUILayout.BeginHorizontal();

                int preSeed = Seed;
                Seed = EditorGUILayout.IntField("Seed", Seed);
                if (Seed == 0)
                {
                    EditorGUILayout.LabelField("(Random)", EditorStyles.centeredGreyMiniLabel, GUILayout.Width(60));
                }
                else
                {
                    if (GUILayout.Button("Randomize"))
                    {
                        Seed = Random.Range(-999, 999);
                        so_preset.ApplyModifiedProperties();
                        TriggerPreview();
                    }
                    else
                    {
                        if (Seed != preSeed)
                        {
                            if (so_preset != null) so_preset.ApplyModifiedProperties();
                            TriggerPreview();
                        }
                    }
                }

                EditorGUILayout.EndVertical();
                EditorGUILayout.EndVertical();

                GUILayout.Space(2);
                EditorGUIUtility.labelWidth = 175;
                EditorGUILayout.BeginVertical(FGUI_Resources.BGInBoxStyle);
                RunAdditionalGenerators = EditorGUILayout.Toggle(new GUIContent("Run Additional Generators", " Triggering Generate() on spawned components inplementing IGenerating interface"), RunAdditionalGenerators);
                EditorGUILayout.EndVertical();
                EditorGUIUtility.labelWidth = 0;

                GUILayout.Space(5);

                gridMode = (EDesignerGridMode)EditorGUILayout.EnumPopup("Mode: ", gridMode);
                GUILayout.Space(4);


                if (gridMode != EDesignerGridMode.Paint)
                {
                    EditorGUILayout.BeginVertical(FGUI_Resources.BGInBoxStyle);
                    GUILayout.Space(3);

                    if (gridMode != EDesignerGridMode.RectangleGrid)
                    {
                        SizeX = MinMax.DrawGUI(SizeX, new GUIContent("SizeX"));
                        SizeY = MinMax.DrawGUI(SizeY, new GUIContent("SizeY"));
                        SizeZ = MinMax.DrawGUI(SizeZ, new GUIContent("SizeZ"));
                    }
                    else
                    {
                        Vector3Int size = new Vector3Int(SizeX.Min, SizeY.Min, SizeZ.Min);
                        size = EditorGUILayout.Vector3IntField("Test Grid Size (in cells):", size);
                        if (size.x < 1) size.x = 1;
                        if (size.y <= 0) size.y = 0;
                        if (size.z < 1) size.z = 1;

                        SizeX.Min = size.x; SizeX.Max = size.x;
                        SizeY.Min = size.y; SizeY.Max = size.y;
                        SizeZ.Min = size.z; SizeZ.Max = size.z;
                    }

                    GUILayout.Space(4);

                    if (gridMode == EDesignerGridMode.BranchedGeneration)
                    {
                        GUILayout.Space(4);
                        BranchLength = MinMax.DrawGUI(BranchLength, new GUIContent("BranchLength"));
                        TargetBranches = MinMax.DrawGUI(TargetBranches, new GUIContent("TargetBranches"));
                        CellsSpace = MinMax.DrawGUI(CellsSpace, new GUIContent("CellsSpace"));
                    }

                    EditorGUILayout.EndVertical();

                    GUILayout.Space(3);
                    DrawAdditionalGen = EditorGUILayout.Foldout(DrawAdditionalGen, " Draw Advanced Options", true);

                    if (DrawAdditionalGen)
                    {
                        EditorGUI.indentLevel++;
                        GUILayout.Space(4);
                        OffsetGrid = EditorGUILayout.Vector3IntField("Offset Grid", OffsetGrid);
                        GUILayout.Space(6);

                        EditorGUILayout.BeginHorizontal();
                        EditorGUIUtility.labelWidth = 200;
                        SendMessageAfterGenerateTo = (GameObject)EditorGUILayout.ObjectField("After Generate Send Message To", SendMessageAfterGenerateTo, typeof(GameObject), true);
                        EditorGUIUtility.labelWidth = 0;
                        PostGenerateMessage = EditorGUILayout.TextField(PostGenerateMessage);
                        EditorGUILayout.EndHorizontal();
                        GUILayout.Space(3);

                        EditorGUILayout.BeginHorizontal();
                        EditorGUIUtility.labelWidth = 200;
                        SendMessageOnChangeTo = (GameObject)EditorGUILayout.ObjectField("On Change Send Message To", SendMessageOnChangeTo, typeof(GameObject), true);
                        EditorGUIUtility.labelWidth = 0;
                        OnChangeMessage = EditorGUILayout.TextField(OnChangeMessage);
                        EditorGUILayout.EndHorizontal();
                        EditorGUI.indentLevel--;
                        GUILayout.Space(3);
                    }

                }

                GUILayout.Space(4);


                #region Guide Drawers List

                //EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                //EditorGUILayout.BeginHorizontal();

                //EditorGUILayout.LabelField("Spawn Guides (doors)");
                //GUILayout.FlexibleSpace();
                //if (guides.Count > 0) if (GUILayout.Button("-", GUILayout.Width(24))) guides.RemoveAt(guides.Count - 1);
                //if (GUILayout.Button("+", GUILayout.Width(24))) guides.Add(CreateInstance<GuideDrawer>());
                //EditorGUILayout.EndHorizontal();

                //for (int i = guides.Count - 1; i >= 0; i--) if (guides[i] == null) guides.RemoveAt(i); // Cleaning from nulls

                //GUILayout.Space(2);
                //for (int i = 0; i < guides.Count; i++)
                //{
                //    GUILayout.Space(2);
                //    guides[i].DrawMe();
                //}

                //EditorGUILayout.EndVertical();

                #endregion


                #region Restrictions Drawers List

                //EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                //EditorGUILayout.BeginHorizontal();

                //EditorGUILayout.LabelField("Spawn Restrictions (preset's globals)", GUILayout.Width(220));
                //GUILayout.FlexibleSpace();
                //if (restrictions.Count > 0) if (GUILayout.Button("-", GUILayout.Width(24))) restrictions.RemoveAt(restrictions.Count - 1);
                //if (GUILayout.Button("+", GUILayout.Width(24))) restrictions.Add(CreateInstance<RestrictionDrawer>());
                //EditorGUILayout.EndHorizontal();

                //for (int i = restrictions.Count - 1; i >= 0; i--) if (restrictions[i] == null) restrictions.RemoveAt(i); // Cleaning from nulls

                //GUILayout.Space(2);
                //for (int i = 0; i < restrictions.Count; i++)
                //{
                //    GUILayout.Space(2);
                //    restrictions[i].DrawMe();
                //}

                //EditorGUILayout.EndVertical();

                #endregion

            }

            EditorGUILayout.EndVertical();


            #endregion


            GUILayout.Space(4f);


            // Preset field
            EditorGUILayout.BeginHorizontal(FGUI_Resources.BGInBoxStyle);
            EditorGUIUtility.labelWidth = 42;

            if (projectPreset == null) GUI.color = new Color(1f, 1f, 0.4f, 1f);
            projectPreset = (FieldSetup)EditorGUILayout.ObjectField("Preset:", projectPreset, typeof(FieldSetup), false);
            if (projectPreset == null) GUI.color = new Color(1f, 1f, 1f, 1f);

            if (StartupRefs)
                if (StartupRefs.FSDraftsdirectory)
                {
                    if (UsingDraft == null)
                    {
                        if (GUILayout.Button("Use Draft", GUILayout.Width(64)))
                        {
                            UsingDraft = CreateInstance<FieldSetup>();
                            UsingDraft.name = "FS_Draft";
                            UnityEditor.AssetDatabase.CreateAsset(UsingDraft, AssetDatabase.GetAssetPath(StartupRefs.FSDraftsdirectory) + "/FS_Draft.asset");
                            projectPreset = UsingDraft;
                        }
                    }
                    else
                    {
                        if (projectPreset == UsingDraft)
                        {
                            if (GUILayout.Button("Export Draft", GUILayout.Width(78)))
                            {
                                AssetDatabase.SaveAssets();
                                string p = FGenerators.GenerateScriptablePath(UsingDraft, "FS_");
                                AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(UsingDraft), p);
                                UsingDraft = null;
                                AssetDatabase.SaveAssets();
                                projectPreset = AssetDatabase.LoadAssetAtPath<FieldSetup>(p);
                            }

                            if (GUILayout.Button(new GUIContent(FGUI_Resources.Tex_Remove, "Clear Draft Field Setup File"), FGUI_Resources.ButtonStyle, GUILayout.Width(20), GUILayout.Height(18)))
                            {
                                projectPreset = null;
                                UsingDraft = null;
                            }
                        }
                    }

                }


            if (GUILayout.Button("Create New", GUILayout.Width(94))) { projectPreset = (FieldSetup)FGenerators.GenerateScriptable(CreateInstance<FieldSetup>(), "FS_"); }
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(4f);

            if (so_preset != null)
                if (projectPreset != null)
                {

                    #region Room Preset Draw with Modificators Packs etc.

                    EditorGUIUtility.labelWidth = 0;

                    so_preset.Update();
                    SerializedProperty iter = so_preset.GetIterator();

                    if (iter != null)
                    {
                        iter.Next(true);
                        iter.NextVisible(false);

                        GUI.backgroundColor = new Color(.98f, .98f, .79f, 1f);
                        FGUI_Inspector.FoldHeaderStart(ref drawPresetParams, " Field Setup Parameters", FGUI_Resources.HeaderBoxStyle);
                        GUI.backgroundColor = Color.white;

                        if (drawPresetParams)
                        {
                            GUILayout.Space(3);
                            EditorGUI.BeginChangeCheck();

                            SerializedProperty sp_uni = so_preset.FindProperty("NonUniformSize");
                            var sp_univ = sp_uni.Copy(); sp_univ.Next(false);
                            iter.NextVisible(false);

                            EditorGUILayout.BeginHorizontal();

                            if (sp_uni.boolValue == false)
                                EditorGUILayout.PropertyField(iter);
                            else
                                EditorGUILayout.PropertyField(sp_univ);

                            if (EditorGUI.EndChangeCheck())
                            {
                                so_preset.ApplyModifiedProperties();
                                TriggerRefresh(false);
                            }

                            EditorGUIUtility.labelWidth = 8;
                            EditorGUILayout.PropertyField(sp_uni, new GUIContent(" ", "Enabling / disabling switch for non uniform size for cells"), GUILayout.Width(32));
                            EditorGUILayout.EndHorizontal();

                            iter.NextVisible(false);
                            iter.NextVisible(false);

                            SerializedProperty sp_prestOffs = iter.Copy();

                            #region Preset multipliers

                            //EditorGUI.BeginChangeCheck();

                            //GUILayout.Space(3);
                            //FGUI_Inspector.BeginVertical(FGUI_Resources.BGInBoxStyle, new Color(1f, 1f, 1f, 1f));

                            //EditorGUILayout.BeginHorizontal();
                            //EditorGUIUtility.labelWidth = 90;
                            //EditorGUIUtility.fieldWidth = 32;
                            //sp_prestOffs.NextVisible(false); EditorGUILayout.PropertyField(sp_prestOffs);
                            //GUILayout.Space(9);
                            //sp_prestOffs.NextVisible(false); EditorGUILayout.PropertyField(sp_prestOffs);
                            //EditorGUIUtility.fieldWidth = 0;
                            //EditorGUIUtility.labelWidth = 0;
                            //EditorGUILayout.EndHorizontal();

                            //sp_prestOffs.NextVisible(false); EditorGUILayout.PropertyField(sp_prestOffs);
                            //sp_prestOffs.NextVisible(false); EditorGUILayout.PropertyField(sp_prestOffs);

                            //EditorGUILayout.EndVertical();

                            //if (EditorGUI.EndChangeCheck())
                            //{
                            //    so_preset.ApplyModifiedProperties();
                            //    TriggerRefresh(false);
                            //}

                            #endregion


                            GUILayout.Space(3);

                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.LabelField(new GUIContent("Info: ", "There you can write your custom info for the field setup, like minimum cell count info for all logics to work correctly etc."), GUILayout.Width(32));
                            projectPreset.InfoText = EditorGUILayout.TextArea(projectPreset.InfoText);
                            EditorGUILayout.EndHorizontal();
                            EditorGUIUtility.labelWidth = 0;


                            GUILayout.Space(5);
                            DrawCellInstructionDefinitionsList(ref variablesPage, projectPreset.CellsInstructions, FGUI_Resources.BGInBoxStyle, "Cells Instructions Commands", ref drawInstructions, true, projectPreset);

                            EditorGUI.BeginChangeCheck();

                            GUILayout.Space(5);
                            DrawFieldVariablesList(projectPreset.Variables, FGUI_Resources.BGInBoxStyle, "Field Variables", ref drawVariables, ref drawVariablesMode, projectPreset);

                            if (EditorGUI.EndChangeCheck())
                            {
                                so_preset.ApplyModifiedProperties();
                                TriggerRefresh(false);
                            }

                            GUILayout.Space(6);
                        }

                        EditorGUILayout.EndVertical();


                        GUILayout.Space(6);
                        FGUI_Inspector.BeginVertical(FGUI_Resources.BGInBoxStyle, new Color(1f, 1f, 1f, 1f));
                        DrawMods();
                        EditorGUILayout.EndVertical();

                    }

                    #endregion

                }


            if (EditorGUI.EndChangeCheck()) repaint = true;
            EditorGUIUtility.labelWidth = 0;
        }


        void TriggerPreview()
        {
            Get.ClearAllGeneratedGameObjects();
            if (gridMode != EDesignerGridMode.Paint) Get.GenerateBaseFieldGrid();

            Get.RunFieldCellsRules();
            if (Get.PreviewAutoSpawn) Get.RunFieldCellsSpawningGameObjects();
        }


        public bool GenButtonsFoldout = true;
        public bool PostEventsFoldout = false;
        void DrawPostEvents()
        {
            GUILayout.Space(-3);
            if (so_preset == null) return;
            SerializedProperty sp = so_preset.FindProperty("AddReflectionProbes");
            if (sp == null) return;
            EditorGUILayout.PropertyField(sp);
            sp.NextVisible(false); if (projectPreset.AddReflectionProbes) EditorGUILayout.PropertyField(sp);
            sp.NextVisible(false); if (projectPreset.AddReflectionProbes && projectPreset.MainReflectionSettings) EditorGUILayout.PropertyField(sp);
            if (projectPreset.AddReflectionProbes && projectPreset.AddMultipleProbes)
            {
                SerializedProperty spp = so_preset.FindProperty("SmallerReflSettings");
                EditorGUILayout.PropertyField(spp);
                spp.Next(false); EditorGUILayout.PropertyField(spp);
                spp.Next(false); EditorGUILayout.PropertyField(spp);
            }

            sp.NextVisible(false); EditorGUILayout.PropertyField(sp);

            sp.NextVisible(false); if (projectPreset.AddLightProbes) EditorGUILayout.PropertyField(sp);
            EditorGUIUtility.labelWidth = 180;
            sp.NextVisible(false); EditorGUILayout.PropertyField(sp);
            EditorGUIUtility.labelWidth = 0;
            if (projectPreset.TriggerColliderGeneration == FieldSetup.ETriggerGenerationMode.MultipleBoxesFill)
            {
                SerializedProperty spp = so_preset.FindProperty("TriggerGenSettings");
                EditorGUILayout.PropertyField(spp, true);
            }
            //sp.NextVisible(false); EditorGUILayout.PropertyField(sp);
        }

    }
}