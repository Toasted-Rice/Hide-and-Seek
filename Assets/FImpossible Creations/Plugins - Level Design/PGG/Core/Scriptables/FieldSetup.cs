﻿using System.Collections.Generic;
using UnityEngine;

namespace FIMSpace.Generating
{
    /// <summary>
    /// It's never sub-asset -> it's always project file asset
    /// </summary>
    [CreateAssetMenu(fileName = "FieldSetup_", menuName = "FImpossible Creations/Procedural Generation/Grid Field Setup", order = 0)]
    public partial class FieldSetup : ScriptableObject
    {
        public float CellSize = 2f;
        public Vector3 GetCellUnitSize() { if (NonUniformSize == false) return new Vector3(CellSize, CellSize, CellSize); else return NonUniformCellSize; }

        public bool NonUniformSize = false;
        public Vector3 NonUniformCellSize = new Vector3(2f, 2f, 2f);

        public string InfoText = "Here write custom note about FieldSetup";

        public List<FieldVariable> Variables = new List<FieldVariable>();

        [SerializeField] [HideInInspector] private FieldModification DoorsModificator;
        [SerializeField] [HideInInspector] private FieldModification EraseWallModificator;

        public List<InstructionDefinition> CellsInstructions;
        /// <summary> Returning 'CellsInstructions' but just with different name </summary>
        public List<InstructionDefinition> CellsCommands { get { return CellsInstructions; } }

        [HideInInspector] public ModificatorsPack RootPack;
        public List<ModificatorsPack> ModificatorPacks = new List<ModificatorsPack>();
        public List<FieldModification> Ignores = new List<FieldModification>();
        internal List<InjectionSetup> temporaryInjections = new List<InjectionSetup>();
        public string DontSpawnOn;
        public List<InjectionSetup> SelfInjections;

        [SerializeField] [HideInInspector] private List<FieldModification> disabledMods = new List<FieldModification>();


        private void Awake()
        {
            if (CellsInstructions == null || CellsInstructions.Count == 0)
            {
                CellsInstructions = new List<InstructionDefinition>();
                InstructionDefinition def = new InstructionDefinition();
                def.Title = "Door Hole";
                def.TargetModification = DoorsModificator;
                def.Tags = "Props";
                def.InstructionType = InstructionDefinition.EInstruction.DoorHole;
                CellsInstructions.Add(def);

                def = new InstructionDefinition();
                def.Title = "Clear Wall";
                def.TargetModification = EraseWallModificator;
                def.InstructionType = InstructionDefinition.EInstruction.PostRunModificator;
                CellsInstructions.Add(def);
            }

            if (Variables == null || Variables.Count == 0)
            {
                Variables = new List<FieldVariable>();
                FieldVariable def = new FieldVariable("Spawn Propability Multiplier", 1f);
                def.helper.x = 0; def.helper.y = 5;
                Variables.Add(def);

                def = new FieldVariable("Spawn Count Multiplier", 1f);
                def.helper.x = 0; def.helper.y = 5;
                Variables.Add(def);
            }
        }


        #region Handling multiple scale graphs


        internal FGenGraph<FieldCell, FGenPoint> _tempGraphScale2;
        internal FGenGraph<FieldCell, FGenPoint> _tempGraphScale3;
        internal FGenGraph<FieldCell, FGenPoint> _tempGraphScale4;
        internal FGenGraph<FieldCell, FGenPoint> _tempGraphScale5;
        internal FGenGraph<FieldCell, FGenPoint> _tempGraphScale6;



        public void PrepareGraph()
        {
            for (int mp = 0; mp < ModificatorPacks.Count; mp++)
            {
                for (int md = 0; md < ModificatorPacks[mp].FieldModificators.Count; md++)
                {
                    for (int sp = 0; sp < ModificatorPacks[mp].FieldModificators[md].Spawners.Count; sp++)
                    {
                        var spawner = ModificatorPacks[mp].FieldModificators[md].Spawners[sp];
                        if (spawner.OnScalledGrid > 1)
                        {

                        }
                    }
                }
            }
        }

        // Clearing all temporary scaled graphs variables
        private void ResetScaledGraphs()
        {
            _tempGraphScale2 = null;
            _tempGraphScale3 = null;
            _tempGraphScale4 = null;
            _tempGraphScale5 = null;
            _tempGraphScale6 = null;
        }

        // Construct scaled graphs out of already placed cells
        public void PrepareSubGraphs(FGenGraph<FieldCell, FGenPoint> grid)
        {
            ResetScaledGraphs();

            if (grid.SubGraphs != null) grid.SubGraphs.Clear();

            grid.SubGraphs = new List<FGenGraph<FieldCell, FGenPoint>>();

            for (int s = 2; s <= 6; s++)
            {
                var gr = GetScaledGrid(grid, s, true);
                if (gr != null) grid.SubGraphs.Add(gr);
            }
        }

        public FGenGraph<FieldCell, FGenPoint> GetScaledGrid(FGenGraph<FieldCell, FGenPoint> baseGraph, int scale, bool generate = true)
        {
            if (scale == 2)
            {
                if (_tempGraphScale2 == null) { if (generate) { _tempGraphScale2 = baseGraph.GenerateScaledGraph(scale); } else return null; }
                return _tempGraphScale2;
            }
            else if (scale == 3)
            {
                if (_tempGraphScale3 == null) { if (generate) _tempGraphScale3 = baseGraph.GenerateScaledGraph(scale); else return null; }
                return _tempGraphScale3;
            }
            else if (scale == 4)
            {
                if (_tempGraphScale4 == null) { if (generate) _tempGraphScale4 = baseGraph.GenerateScaledGraph(scale); else return null; }
                return _tempGraphScale4;
            }
            else if (scale == 5)
            {
                if (_tempGraphScale5 == null) { if (generate) _tempGraphScale5 = baseGraph.GenerateScaledGraph(scale); else return null; }
                return _tempGraphScale5;
            }
            else if (scale == 6)
            {
                if (_tempGraphScale6 == null) { if (generate) _tempGraphScale6 = baseGraph.GenerateScaledGraph(scale); else return null; }
                return _tempGraphScale6;
            }

            return baseGraph;
        }

        #endregion



        /// <summary>
        /// Checking if all required references exists
        /// </summary>
        public void Validate()
        {
            if (RootPack == null)
            {
                RootPack = CreateInstance<ModificatorsPack>();
                RootPack.name = "Root";
                FGenerators.AddScriptableTo(RootPack, this, false);
            }

            if (RootPack != null)
            {
#if UNITY_EDITOR
                if (UnityEditor.AssetDatabase.Contains(this))
                {
                    if (FGenerators.AssetContainsAsset(RootPack, this) == false) FGenerators.AddScriptableTo(RootPack, this, false);
                }
#endif

                RootPack.ParentPreset = this;
            }

            if (ModificatorPacks == null)
            {
                ModificatorPacks = new List<ModificatorsPack>();
            }
        }


        internal void ClearTemporaryContainers()
        {
            for (int p = 0; p < ModificatorPacks.Count; p++)
            {
                if (ModificatorPacks[p] == null) continue;

                for (int r = 0; r < ModificatorPacks[p].FieldModificators.Count; r++)
                {
                    if (ModificatorPacks[p].FieldModificators[r] == null) continue;
                    ModificatorPacks[p].FieldModificators[r].TemporaryContainer = null;
                }
            }

            if (DoorsModificator != null) DoorsModificator.TemporaryContainer = null;
            if (EraseWallModificator != null) EraseWallModificator.TemporaryContainer = null;
        }
    }

}