using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using FIMSpace.FEditor;
using UnityEditor.Callbacks;
using FIMSpace.Hidden;

namespace FIMSpace.Generating
{
    public partial class FieldDesignWindow : EditorWindow
    {
        public PGGStartupReferences StartupRefs;
        public static FieldDesignWindow Get;
        GameObject mainGeneratedsContainer;
        FieldGenerationInfo generated;

        FieldSetup projectPreset;
        SerializedObject so_preset;

        Vector2 mainScroll = Vector2.zero;

        public int Seed = 0;
        public MinMax SizeX = new MinMax(6, 6);
        public MinMax SizeY = new MinMax(0, 0);
        public MinMax SizeZ = new MinMax(4, 4);
        public Vector3Int OffsetGrid = new Vector3Int(0, 0, 0);
        public MinMax BranchLength = new MinMax(5, 9);
        public MinMax TargetBranches = new MinMax(4, 6);
        public MinMax CellsSpace = new MinMax(1, 3);
        public bool RunAdditionalGenerators = false;
        public bool DrawAdditionalGen = false;

        public GameObject SendMessageAfterGenerateTo;
        public string PostGenerateMessage;

        public GameObject SendMessageOnChangeTo;
        public string OnChangeMessage;


        bool repaint = false;
        static int viewed = 0;

        [MenuItem("Window/FImpossible Creations/Level Design/Grid Field Designer", false, 50)]
        static void Init()
        {
            FieldDesignWindow window = (FieldDesignWindow)GetWindow(typeof(FieldDesignWindow));
            //if (viewed == 0 && Get == null && window.titleContent.text != "Field Designer") FrameCenter();
            viewed++;

            window.titleContent = new GUIContent("Field Designer", Resources.Load<Texture>("SPR_FieldDesigner"));
            window.Show();

            Get = window;
            if (Get) if (Get.Repose) window.position = new Rect(300, 100, 450, 500);
        }

//#if UNITY_2019_4_OR_NEWER
//        public override IEnumerable<System.Type> GetExtraPaneTypes()
//        {
//            return new System.Type[]
//            {
//                 typeof(FieldDesignWindow)
//            };
//        }
//#endif

        public static void FrameCenter(float distance, bool onlyRot = false)
        {
            SceneView view = SceneView.lastActiveSceneView;
            if (view == null) return;

            var tgt = view.camera;

            if (onlyRot == false)
            {
                tgt.transform.position = new Vector3(0, distance, -distance);
                tgt.transform.rotation = Quaternion.LookRotation(-tgt.transform.position.normalized);
            }
            else
            {
                tgt.transform.rotation = Quaternion.Euler(25, 0, 0);
            }

            view.AlignViewToObject(tgt.transform);
        }


        [OnOpenAssetAttribute(1)]
        public static bool OpenFieldScriptableFile(int instanceID, int line)
        {
            Object obj = EditorUtility.InstanceIDToObject(instanceID);
            if (obj as FieldSetup != null)
            {
                if (Get != null) Get.Repose = false;

                Init();
                Get.projectPreset = obj as FieldSetup;

                Get.drawTestGenSetts = false;

                Get.ClearAllGeneratedGameObjects();
                Get.GenerateBaseFieldGrid();
                Get.TriggerRefresh(false);

                return true;
            }

            return false;
        }


        void OnGUI()
        {
            if (Get == null) Init();

            mainScroll = EditorGUILayout.BeginScrollView(mainScroll);

            //EditorGUILayout.HelpBox("Field Setup preview is visible in scene view at position 0,0,0", MessageType.Info);

            GUILayout.Space(6);
            EditorGUILayout.LabelField("Prepare Field Setup with dynamic preview", FGUI_Resources.HeaderStyle);
            GUILayout.Space(2);


            #region Generating or validating base preset

            Get = this;

            if (projectPreset != null)
            {
                so_preset = new SerializedObject(projectPreset);
                projectPreset.Validate();
            }

            #endregion


            GUILayout.Space(3);
            DrawFieldGenWindowGUI(projectPreset); // Main GUI in this method

            GUILayout.Space(3);

            EditorGUILayout.BeginHorizontal();
            if (grid == null)
            {
                GenerateBaseFieldGrid();
                if (AutoRefreshPreview) RunFieldCellsRules();
            }

            EditorGUILayout.EndHorizontal();

            if (projectPreset != null)
            {
                if (grid != null)
                {
                    if (projectPreset.ModificatorPacks.Count > 0)
                    {

                        FGUI_Inspector.FoldHeaderStart(ref GenButtonsFoldout, "Generating Buttons", EditorStyles.helpBox);
                        EditorGUILayout.BeginVertical(FGUI_Resources.BGInBoxBlankStyle);
                        if (GenButtonsFoldout)
                        {


                            GUILayout.Space(5);
                            EditorGUILayout.BeginHorizontal();

                            if (gridMode != EDesignerGridMode.Paint)
                            {
                                if (Seed == 0) if (GUILayout.Button("Randomize"))
                                    {
                                        ClearAllGeneratedGameObjects();
                                        GenerateBaseFieldGrid();
                                        TriggerRefresh(false);
                                    }
                            }
                            else
                            {
                                if (Seed == 0) if (GUILayout.Button("Run Rules"))
                                    {
                                        TriggerRefresh(false);
                                    }
                            }

                            EditorGUIUtility.labelWidth = 85;

                            AutoRefreshPreview = EditorGUILayout.Toggle(new GUIContent("Auto Preview", "Automatically run all rules on grid every change, good for using AUTO SPAWN or PREVIEW (For PREVIEW : Scene Preview Settings -> Alpha set to higher than zero)"), AutoRefreshPreview);
                            if (AutoRefreshPreview) PreviewAutoSpawn = EditorGUILayout.Toggle(new GUIContent("Auto Spawn", "Automatically spawning objects every change occured. WARNING it can make your preview very slow when used on bigger preview grids with many objects to spawn, in such cases switch it off and use 'Run Spawners' button manually"), PreviewAutoSpawn);
                            EditorGUIUtility.labelWidth = 0;

                            if (!AutoRefreshPreview)
                            {
                                if (GUILayout.Button(new GUIContent("Run Mods Rules")))
                                {
                                    RunFieldCellsRules();
                                    if (AutoRefreshPreview) if (PreviewAutoSpawn) RunFieldCellsSpawningGameObjects();
                                }
                            }

                            //if (AutoRefreshPreview)
                            if (GUILayout.Button("Run Spawners"))
                            {
                                RunFieldCellsSpawningGameObjects();
                            }

                            EditorGUILayout.EndHorizontal();

                            if (GUILayout.Button("Generate New Grid and Spawn"))
                            {
                                IGeneration.ClearCells(grid);
                                GenerateBaseFieldGrid();
                                RunFieldCellsRules();
                                RunFieldCellsSpawningGameObjects();
                            }

                            if (generated != null)
                                if (generated.Instantiated != null)
                                    if (generated.Instantiated.Count > 0)
                                        if (GUILayout.Button("Clear Generated")) ClearAllGeneratedGameObjects();

                            //if ( repaint)
                            //for (int m = 0; m < projectPreset.ModificatorPacks.Count; m++)
                            //{
                            //        if (projectPreset.ModificatorPacks[m] == null) continue;
                            //        SerializedObject spm = new SerializedObject(projectPreset.ModificatorPacks[m]);
                            //        spm.ApplyModifiedProperties();
                            //    }


                        }
                        else
                            GUILayout.Space(-7);

                        EditorGUILayout.EndVertical();
                        EditorGUILayout.EndVertical();

                    }

                    GUILayout.Space(3);
                }

                GUILayout.Space(3);

                // Post Events
                FGUI_Inspector.FoldHeaderStart(ref PostEventsFoldout, "Field Setup Post Events", EditorStyles.helpBox);
                EditorGUILayout.BeginVertical(FGUI_Resources.BGInBoxBlankStyle);
                if (PostEventsFoldout) DrawPostEvents(); else GUILayout.Space(-7);
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndVertical();
            }



            EditorGUILayout.EndScrollView();

            if (so_preset != null) so_preset.ApplyModifiedProperties();

            if (repaint)
            {
                SceneView.RepaintAll();
                repaint = false;
            }
        }

        public void TriggerRefresh(bool refreshGrid = true)
        {
            if (AutoRefreshPreview)
            {
                Get.ClearAllGeneratedGameObjects();
                if (gridMode != EDesignerGridMode.Paint) GenerateBaseFieldGrid();

                RunFieldCellsRules();
                if (PreviewAutoSpawn) RunFieldCellsSpawningGameObjects();
            }

            SceneView.RepaintAll();
            repaint = true;
        }

    }
}