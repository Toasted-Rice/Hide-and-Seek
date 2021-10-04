using FIMSpace.Generating.Checker;
using FIMSpace.Generating.Planning;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace FIMSpace.Generating
{
    public static partial class IGeneration
    {
        public static bool Debugging = false;
        public static FGenGraph<FieldCell, FGenPoint> GetEmptyFieldGraph()
        {
            return new FGenGraph<FieldCell, FGenPoint>();
        }

        public static void ClearCells(FGenGraph<FieldCell, FGenPoint> grid)
        {
            if (grid != null)
            {
                for (int i = 0; i < grid.AllCells.Count; i++)
                    grid.AllCells[i].Clear();
            }
        }

        public static List<FieldCell> GetRandomizedCells(FGenGraph<FieldCell, FGenPoint> grid)
        {
            List<FieldCell> pack = new List<FieldCell>();
            List<FieldCell> randomizedCells = new List<FieldCell>();

            if (grid != null)
                for (int i = 0; i < grid.AllApprovedCells.Count; i++)
                    pack.Add(grid.AllApprovedCells[i]);

            while (pack.Count > 0)
            {
                int index = FGenerators.GetRandom(0, pack.Count);
                randomizedCells.Add(pack[index]);
                pack.RemoveAt(index);
            }

            return randomizedCells;
        }


        /// <summary>
        /// Spawning data placed inside grid's cells 
        /// Returning list of objects with IGenerating interface implemented for post-generating events
        /// </summary>
        public static List<IGenerating> RunGraphSpawners(FGenGraph<FieldCell, FGenPoint> grid, Transform container, FieldSetup preset, List<GameObject> listToFillWithSpawns, Matrix4x4? spawnMatrix = null)
        {
            List<IGenerating> generatorsCollected = new List<IGenerating>();

            Matrix4x4 matrix = Matrix4x4.identity;
            if (spawnMatrix != null) matrix = spawnMatrix.Value;

            FGenGraph<FieldCell, FGenPoint> runGraph;
            int subCount = 0;
            if (grid.SubGraphs != null) subCount = grid.SubGraphs.Count;

            for (int gr = -1; gr < subCount; gr++)
            {
                if (gr == -1) runGraph = grid; else runGraph = grid.SubGraphs[gr];

                for (int c = 0; c < runGraph.AllCells.Count; c++)
                {
                    var cell = runGraph.AllCells[c];
                    var spawns = cell.GetSpawnsJustInsideCell();

                    for (int s = 0; s < spawns.Count; s++)
                    {
                        var spawn = spawns[s];

                        if (spawn.OnPreGeneratedEvents.Count != 0)
                            for (int pe = 0; pe < spawn.OnPreGeneratedEvents.Count; pe++)
                                spawn.OnPreGeneratedEvents[pe].Invoke(spawn);


                        if (spawn.Prefab != null || spawn.DontSpawnMainPrefab)
                        {

                            Transform targetContainer = spawn.GetModContainer(container);
                            if (targetContainer == null) targetContainer = container;

                            if (spawn.DontSpawnMainPrefab == false)
                            {
                                GameObject spawned = null;

                                if (spawn.idInStampObjects == -2)
                                {
                                    if (spawn.OwnerMod.DrawSetupFor == FieldModification.EModificationMode.ObjectsStamp)
                                    {
                                        spawned = new GameObject(spawn.OwnerMod.name);
                                        ObjectStampEmitter emitter = spawned.AddComponent<ObjectStampEmitter>();
                                        emitter.PrefabsSet = spawn.OwnerMod.OStamp;
                                        emitter.AlwaysDrawPreview = true;
                                    }
                                }
                                else if (spawn.idInStampObjects == -1)
                                {
                                    if (spawn.OwnerMod.DrawSetupFor == FieldModification.EModificationMode.ObjectMultiEmitter)
                                    {
                                        OStamperSet stamp = spawn.OwnerMod.OMultiStamp.PrefabsSets[FGenerators.GetRandom(0, spawn.OwnerMod.OMultiStamp.PrefabsSets.Count)];
                                        spawned = new GameObject(stamp.name);
                                        ObjectStampEmitter emitter = spawned.AddComponent<ObjectStampEmitter>();
                                        emitter.PrefabsSet = stamp;
                                        emitter.AlwaysDrawPreview = true;
                                    }
                                }

                                if (spawned == null)
                                {
                                    spawned = FGenerators.InstantiateObject(spawn.Prefab);
                                }

                                spawned.transform.SetParent(targetContainer, true);

                                Vector3 targetPosition = preset.GetCellWorldPosition(cell);

                                Quaternion rotation = spawn.Prefab.transform.rotation * Quaternion.Euler(spawn.RotationOffset);

                                spawned.transform.position = matrix.MultiplyPoint(targetPosition + spawn.Offset + rotation * spawn.DirectionalOffset);

                                if (spawn.LocalRotationOffset != Vector3.zero) rotation *= Quaternion.Euler(spawn.LocalRotationOffset);
                                spawned.transform.rotation = matrix.rotation * rotation;
                                spawned.transform.localScale = Vector3.Scale(spawn.LocalScaleMul, spawn.Prefab.transform.lossyScale);


                                // Collecting generators
                                if (spawn.OwnerMod != null)
                                {
                                    if (spawned.transform.childCount > 0)
                                    {
                                        for (int ch = 0; ch < spawned.transform.childCount; ch++)
                                        {
                                            IGenerating[] emitters = spawned.transform.GetChild(ch).GetComponentsInChildren<IGenerating>();
                                            for (int i = 0; i < emitters.Length; i++) generatorsCollected.Add(emitters[i]);
                                        }
                                    }

                                    IGenerating emitter = spawned.GetComponent<IGenerating>();
                                    if (emitter != null) generatorsCollected.Add(emitter);
                                }

                                listToFillWithSpawns.Add(spawned);

                                // Post Events support
                                if (spawn.OnGeneratedEvents.Count != 0)
                                    for (int pe = 0; pe < spawn.OnGeneratedEvents.Count; pe++)
                                        spawn.OnGeneratedEvents[pe].Invoke(spawned);
                            }
                            else
                            {
                                // Post Events support
                                if (spawn.OnGeneratedEvents.Count != 0)
                                    for (int pe = 0; pe < spawn.OnGeneratedEvents.Count; pe++)
                                        spawn.OnGeneratedEvents[pe].Invoke(null);
                            }


                            // Additional generated feature
                            if (spawn.AdditionalGenerated != null)
                            {
                                for (int sa = 0; sa < spawn.AdditionalGenerated.Count; sa++)
                                {
                                    var spwna = spawn.AdditionalGenerated[sa];

                                    if (spwna == null) continue;
                                    spwna.transform.SetParent(targetContainer, true);

                                    if (spwna.transform.childCount > 0)
                                    {
                                        for (int ch = 0; ch < spwna.transform.childCount; ch++)
                                        {
                                            IGenerating[] emitters = spwna.transform.GetChild(ch).GetComponentsInChildren<IGenerating>();
                                            for (int i = 0; i < emitters.Length; i++) generatorsCollected.Add(emitters[i]);
                                        }
                                    }

                                    IGenerating emitter = spwna.GetComponent<IGenerating>();
                                    if (emitter != null) generatorsCollected.Add(emitter);

                                    listToFillWithSpawns.Add(spwna);
                                }

                            }

                        }
                    }
                }

            }

            preset.ClearTemporaryContainers();

            return generatorsCollected;
        }

        internal static Vector3 V2ToV3(Vector2Int p)
        {
            return new Vector3(p.x, 0, p.y);
        }



        /// <summary>
        /// Creating field cells grid, running field modificator rules and spawning game objects, returning them in list
        /// </summary>
        public static FieldGenerationInfo GenerateFieldObjectsRectangleGrid(FieldSetup preset, Vector3Int size, int seed, Transform container, bool runRules = true, List<SpawnInstruction> guides = null, bool runEmitters = false, Vector3Int? offset = null)
        {
            var grid = GetEmptyFieldGraph();
            grid.Generate(size.x, size.y, size.z, offset == null ? Vector3Int.zero : offset.Value);

            FGenerators.SetSeed(seed);

            return GenerateFieldObjects(preset, grid, container, runRules, guides, null, runEmitters);
        }


        public static FieldGenerationInfo GenerateFieldObjectsWithContainer(string name, FieldSetup setup, FGenGraph<FieldCell, FGenPoint> grid, Transform container, List<SpawnInstruction> guides = null, List<InjectionSetup> inject = null, Vector3? offset = null, bool runRules = true, bool runEmitters = true)
        {
            if (setup == null)
            {
                Debug.LogError("No assigned Field Setup in " + name + "!");
                return new FieldGenerationInfo();
            }

            Transform nContainer = new GameObject(name + " - Generated").transform;
            nContainer.SetParent(container);
            nContainer.ResetCoords();

            if (offset != Vector3.zero) offset = offset / setup.GetCellUnitSize().x;

            if (inject != null) setup.SetTemporaryInjections(inject); else setup.ClearTemporaryContainers();

            FieldGenerationInfo gen = IGeneration.GenerateFieldObjects(setup, grid, nContainer, runRules, guides, offset, runEmitters);

            if (inject != null) setup.ClearTemporaryInjections();

            gen.MainContainer = nContainer.gameObject;

            return gen;
        }

        static bool CheckIfScaledGraphsNeeded(FieldSetup preset, List<SpawnInstruction> guides = null)
        {
            if (preset.RequiresScaledGraphs()) return true;

            if (guides != null)
                for (int g = 0; g < guides.Count; g++)
                    if (guides[g].definition != null)
                        if (guides[g].definition.TargetModification)
                            if (guides[g].definition.TargetModification.RequiresScaledGraphs()) return true;

            return false;

        }

        /// <summary>
        /// Must be called after calling preset.SetTemporaryInjections()
        /// </summary>
        public static void PreparePresetVariables(FieldSetup preset)
        {

            #region Preparing all rules variables if used


            #region Refreshing all rules variables preparement state

            for (int p = 0; p < preset.ModificatorPacks.Count; p++)
            {
                if (preset.ModificatorPacks[p] == null) continue;
                if (preset.ModificatorPacks[p].DisableWholePackage) continue;

                for (int r = 0; r < preset.ModificatorPacks[p].FieldModificators.Count; r++)
                {
                    if (preset.ModificatorPacks[p].FieldModificators[r] == null) continue;
                    if (preset.ModificatorPacks[p].FieldModificators[r].Enabled == false) continue;
                    if (preset.Ignores.Contains(preset.ModificatorPacks[p].FieldModificators[r])) continue;
                    if (preset.IsEnabled(preset.ModificatorPacks[p].FieldModificators[r]) == false) continue;

                    for (int s = 0; s < preset.ModificatorPacks[p].FieldModificators[r].Spawners.Count; s++)
                    {
                        if (FGenerators.CheckIfIsNull(preset.ModificatorPacks[p].FieldModificators[r].Spawners[s] )) continue;
                        preset.ModificatorPacks[p].FieldModificators[r].Spawners[s].Prepared = false;

                        // Supporting "Tag for all spawners"
                        //if ( !string.IsNullOrEmpty(preset.ModificatorPacks[p].TagForAllSpawners))
                        //{
                        //    preset.ModificatorPacks[p].FieldModificators[r].Spawners[s].SpawnerTag = preset.ModificatorPacks[p].TagForAllSpawners;
                        //}

                        for (int rl = 0; rl < preset.ModificatorPacks[p].FieldModificators[r].Spawners[s].Rules.Count; rl++)
                        {
                            if (FGenerators.CheckIfIsNull(preset.ModificatorPacks[p].FieldModificators[r].Spawners[s].Rules[rl] )) continue;
                            preset.ModificatorPacks[p].FieldModificators[r].Spawners[s].Rules[rl].VariablesPrepared = false;
                        }
                    }
                }
            }

            for (int c = 0; c < preset.CellsCommands.Count; c++)
            {
                if (FGenerators.CheckIfIsNull(preset.CellsCommands[c] )) continue;
                if (FGenerators.CheckIfIsNull(preset.CellsCommands[c].TargetModification )) continue;

                for (int s = 0; s < preset.CellsCommands[c].TargetModification.Spawners.Count; s++)
                {
                    if (FGenerators.CheckIfIsNull(preset.CellsCommands[c].TargetModification.Spawners[s] )) continue;

                    for (int rl = 0; rl < preset.CellsCommands[c].TargetModification.Spawners[s].Rules.Count; rl++)
                    {
                        if (FGenerators.CheckIfIsNull(preset.CellsCommands[c].TargetModification.Spawners[s].Rules[rl] )) continue;
                        preset.CellsCommands[c].TargetModification.Spawners[s].Rules[rl].VariablesPrepared = false;
                    }
                }
            }

            #endregion

            // Applying field setup variables for temporary injections
            if (preset.temporaryInjections != null)
                for (int i = 0; i < preset.temporaryInjections.Count; i++)
                {
                    var inj = preset.temporaryInjections[i];
                    if (FGenerators.CheckIfIsNull(inj )) continue;

                    // No assigned pack and no assigned modificator -> checking for variable override
                    if (inj.ModificatorsPack == null && inj.Modificator == null)
                    {
                        if (inj.OverrideVariables)
                        {
                            if (inj.Overrides == null || inj.Overrides.Count == 0) continue;

                            for (int ov = 0; ov < inj.Overrides.Count; ov++)
                            {
                                var vari = preset.GetVariable(inj.Overrides[ov].Name);

                                if (i < inj.Overrides.Count)
                                    if (FGenerators.CheckIfExist_NOTNULL(vari )) vari.SetValue(inj.Overrides[i]);
                            }
                        }

                        continue;
                    }

                    if (inj.Inject == InjectionSetup.EInjectTarget.Pack)
                    {
                        if (inj.ModificatorsPack == null) continue;
                        for (int m = 0; m < inj.ModificatorsPack.FieldModificators.Count; m++)
                        {
                            if (inj.ModificatorsPack.FieldModificators[m] == null) continue;
                            inj.ModificatorsPack.FieldModificators[m].PrepareVariablesWith(preset, true, inj);
                        }
                    }
                    else if (inj.Inject == InjectionSetup.EInjectTarget.Modificator)
                    {
                        if (inj.Modificator == null) continue;
                        inj.Modificator.PrepareVariablesWith(preset, true, inj);
                    }
                    else if (inj.Inject == InjectionSetup.EInjectTarget.ModOnlyForAccessingVariables)
                    {
                        if (inj.Modificator == null) continue;
                        inj.Modificator.PrepareVariablesWith(preset, true, inj);

                        // Preparing all modificators with this variable injection

                        for (int p = 0; p < preset.ModificatorPacks.Count; p++)
                        {
                            if (preset.ModificatorPacks[p] == null) continue;
                            if (preset.ModificatorPacks[p].DisableWholePackage) continue;

                            for (int r = 0; r < preset.ModificatorPacks[p].FieldModificators.Count; r++)
                            {
                                if (preset.ModificatorPacks[p].FieldModificators[r] == null) continue;
                                if (preset.ModificatorPacks[p].FieldModificators[r].Enabled == false) continue;
                                if (preset.Ignores.Contains(preset.ModificatorPacks[p].FieldModificators[r])) continue;
                                if (preset.IsEnabled(preset.ModificatorPacks[p].FieldModificators[r]) == false) continue;
                                preset.ModificatorPacks[p].FieldModificators[r].PrepareVariablesWith(preset, true, inj);
                            }
                        }

                        for (int c = 0; c < preset.CellsCommands.Count; c++)
                        {
                            if (FGenerators.CheckIfIsNull(preset.CellsCommands[c] )) continue;
                            if (FGenerators.CheckIfIsNull(preset.CellsCommands[c].TargetModification )) continue;
                            preset.CellsCommands[c].TargetModification.PrepareVariablesWith(preset, true, inj);
                        }
                    }
                }


            // Preparing default variables for modificators if not injected
            for (int p = 0; p < preset.ModificatorPacks.Count; p++)
            {
                if (preset.ModificatorPacks[p] == null) continue;
                if (preset.ModificatorPacks[p].DisableWholePackage) continue;

                for (int r = 0; r < preset.ModificatorPacks[p].FieldModificators.Count; r++)
                {
                    if (preset.ModificatorPacks[p].FieldModificators[r] == null) continue;
                    if (preset.ModificatorPacks[p].FieldModificators[r].Enabled == false) continue;
                    if (preset.Ignores.Contains(preset.ModificatorPacks[p].FieldModificators[r])) continue;
                    if (preset.IsEnabled(preset.ModificatorPacks[p].FieldModificators[r]) == false) continue;
                    preset.ModificatorPacks[p].FieldModificators[r].PrepareVariablesWith(preset);
                }

                for (int c = 0; c < preset.CellsCommands.Count; c++)
                {
                    if (FGenerators.CheckIfIsNull(preset.CellsCommands[c] )) continue;
                    if (FGenerators.CheckIfIsNull(preset.CellsCommands[c].TargetModification )) continue;
                    preset.CellsCommands[c].TargetModification.PrepareVariablesWith(preset);
                }
            }


            #endregion

        }


        public static void RestorePresetVariables(FieldSetup preset)
        {
            //for (int p = 0; p < preset.ModificatorPacks.Count; p++)
            //{
            //    if (preset.ModificatorPacks[p] == null) continue;
            //    if (preset.ModificatorPacks[p].DisableWholePackage) continue;

            //    for (int r = 0; r < preset.ModificatorPacks[p].FieldModificators.Count; r++)
            //    {
            //        if (preset.ModificatorPacks[p].FieldModificators[r] == null) continue;
            //        if (preset.ModificatorPacks[p].FieldModificators[r].Enabled == false) continue;
            //        if (preset.Ignores.Contains(preset.ModificatorPacks[p].FieldModificators[r])) continue;
            //        if (preset.IsEnabled(preset.ModificatorPacks[p].FieldModificators[r]) == false) continue;

            //        for (int s = 0; s < preset.ModificatorPacks[p].FieldModificators[r].Spawners.Count; s++)
            //        {
            //            if (FGenerators.CheckIfIsNull(preset.ModificatorPacks[p].FieldModificators[r].Spawners[s])) continue;

            //            // Supporting "Tag for all spawners"
            //            //if (!string.IsNullOrEmpty(preset.ModificatorPacks[p].TagForAllSpawners))
            //            //{
            //            //    preset.ModificatorPacks[p].FieldModificators[r].Spawners[s].TempSpawnerTag = "";
            //            //}
            //        }
            //    }
            //}

        }


        #region Temporary Field Setup Generating


        /// <summary>
        /// Generating temporary FieldSetup to be able to run mod package on some grid
        /// </summary>
        public static FieldSetup GenerateTemporaryFieldSetupWith(ModificatorsPack putModPackInside, FieldSetup scaleReferenceField = null)
        {
            FieldSetup singlePackField = FieldSetup.CreateInstance<FieldSetup>();

            // Field Setup needs to know what scale to use
            FieldSetup parentField = scaleReferenceField;

            // If no scale reference pack provided let's try find modificators parent for it
            if ( parentField == null)
            {
                parentField = putModPackInside.ParentPreset;
            }

            if (parentField == null)
            {
                UnityEngine.Debug.Log("[PGG - Rectangle Fill] Can't find parent FieldSetup of " + putModPackInside + " to define size of grid!");
                return null;
            }

            singlePackField.CellSize = parentField.CellSize;
            singlePackField.ModificatorPacks.Add(putModPackInside);

            return singlePackField;
        }


        /// <summary>
        /// Generating temporary FieldSetup to be able to run single modificator on some grid
        /// </summary>
        public static FieldSetup GenerateTemporaryFieldSetupWith(FieldModification singleModificator, FieldSetup scaleReferenceField = null)
        {
            FieldSetup singleModField = FieldSetup.CreateInstance<FieldSetup>();

            // Field Setup needs to know what scale to use
            FieldSetup parentField = scaleReferenceField;

            // If no scale reference pack provided let's try find modificators parent for it
            if (parentField == null)
            {
                parentField = singleModificator.TryGetParentSetup();
            }

            if (parentField == null)
            {
                UnityEngine.Debug.Log("[PGG - Rectangle Fill] Can't find parent FieldSetup of " + singleModificator + " to define size of grid!");
                return null;
            }

            singleModField.CellSize = parentField.CellSize;
            singleModField.RootPack = ModificatorsPack.CreateInstance<ModificatorsPack>();
            singleModField.ModificatorPacks = new List<ModificatorsPack>();
            ModificatorsPack tempPack = ModificatorsPack.CreateInstance<ModificatorsPack>();
            tempPack.FieldModificators = new List<FieldModification>();
            tempPack.FieldModificators.Add(singleModificator);
            singleModField.ModificatorPacks.Add(tempPack);

            return singleModField;
        }

        /// <summary>
        /// Generating temporary FieldSetup to be able to run single modificator on some grid
        /// </summary>
        public static FieldSetup GenerateTemporaryFieldSetupWith(InstructionDefinition command, FieldSetup scaleReferenceField )
        {
            FieldSetup singleModField = FieldSetup.CreateInstance<FieldSetup>();

            // Field Setup needs to know what scale to use
            FieldSetup parentField = scaleReferenceField;

            if (parentField == null)
            {
                UnityEngine.Debug.Log("[PGG - Rectangle Fill] Can't find parent FieldSetup of " + command.Title + " to define size of grid!");
                return null;
            }

            singleModField.CellSize = parentField.CellSize;
            singleModField.RootPack = ModificatorsPack.CreateInstance<ModificatorsPack>();
            singleModField.ModificatorPacks = new List<ModificatorsPack>();
            ModificatorsPack tempPack = ModificatorsPack.CreateInstance<ModificatorsPack>();
            tempPack.FieldModificators = new List<FieldModification>();
            tempPack.FieldModificators.Add(command.TargetModification);
            singleModField.ModificatorPacks.Add(tempPack);

            return singleModField;
        }


        /// <summary>
        /// Generating temporary FieldSetup to be able to run few selected modificators on some grid
        /// </summary>
        public static FieldSetup GenerateTemporaryFieldSetupWith(List<FieldModification> fewModificators, FieldSetup scaleReferenceField = null)
        {
            if (fewModificators.Count == 0 || fewModificators[0] == null)
            {
                UnityEngine.Debug.Log("[PGG - Temporary Field Setup] Modificators list don't have some required elements!");
                return null;
            }

            FieldSetup fewModsField = FieldSetup.CreateInstance<FieldSetup>();

            // Field Setup needs to know what scale to use
            FieldSetup parentField = scaleReferenceField;

            // If no scale reference pack provided let's try find modificators parent for it
            if (parentField == null)
            {
                parentField = fewModificators[0].TryGetParentSetup();
            }

            if (parentField == null)
            {
                UnityEngine.Debug.Log("[PGG - Rectangle Fill] Can't find parent FieldSetup of " + fewModificators + " to define size of grid!");
                return null;
            }

            fewModsField.CellSize = parentField.CellSize;
            fewModsField.RootPack = ModificatorsPack.CreateInstance<ModificatorsPack>();
            fewModsField.ModificatorPacks = new List<ModificatorsPack>();
            ModificatorsPack tempPack = ModificatorsPack.CreateInstance<ModificatorsPack>();
            tempPack.FieldModificators = new List<FieldModification>();
            for (int i = 0; i < fewModificators.Count; i++) tempPack.FieldModificators.Add(fewModificators[i]);
            fewModsField.ModificatorPacks.Add(tempPack);

            return fewModsField;
        }

        #endregion


        public static FieldGenerationInfo GenerateFieldObjects(FieldSetup preset, FGenGraph<FieldCell, FGenPoint> grid, Transform container, bool runRules = true, List<SpawnInstruction> guides = null, Vector3? fieldOffset = null, bool runEmitters = true, CheckerField optionalUsedChecker = null)
        {
            // Prepare needed lists
            FieldGenerationInfo gen = new FieldGenerationInfo();
            gen.ParentSetup = preset;
            gen.Instantiated = new List<GameObject>();
            gen.MainContainer = null;
            gen.FieldTransform = container;
            gen.Grid = grid;

            gen.OptionalCheckerFieldsData = new List<CheckerField>();
            if (FGenerators.CheckIfExist_NOTNULL(optionalUsedChecker))
            {
                gen.OptionalCheckerFieldsData.Add(optionalUsedChecker);
                optionalUsedChecker.HelperReference = preset;
            }

            List<FieldCell> newCells = new List<FieldCell>();

            // Configure grid
            if (CheckIfScaledGraphsNeeded(preset, guides))
                if (preset._tempGraphScale2 == null || grid.SubGraphs == null)
                    preset.PrepareSubGraphs(grid);

            // Coordinates in world
            Vector3 pos = container.position;
            if (fieldOffset != null) pos += preset.TransformCellPosition(fieldOffset.Value);
            Matrix4x4 transformMatrix = Matrix4x4.TRS(pos, container.rotation, Vector3.one);


            #region Refreshing self injections if used

            if (preset != null)
                if (preset.SelfInjections != null)
                    if (preset.SelfInjections.Count > 0)
                    {
                        if (preset.temporaryInjections == null)
                            preset.temporaryInjections = new List<InjectionSetup>();

                        for (int i = 0; i < preset.SelfInjections.Count; i++)
                            preset.temporaryInjections.Add(preset.SelfInjections[i]);
                    }

            #endregion


            PreparePresetVariables(preset);


            #region If using isolated grid injection then preparing separated grid for each definition

            Dictionary<InstructionDefinition, List<IGenerating>> isolatedGenerated = null;

            if (guides != null)
            {
                Dictionary<InstructionDefinition, FGenGraph<FieldCell, FGenPoint>> isolatedGrids = new Dictionary<InstructionDefinition, FGenGraph<FieldCell, FGenPoint>>();
                isolatedGenerated = new Dictionary<InstructionDefinition, List<IGenerating>>();

                for (int i = 0; i < preset.CellsInstructions.Count; i++)
                    if (preset.CellsInstructions[i].InstructionType == InstructionDefinition.EInstruction.IsolatedGrid)
                    {
                        if (preset.CellsInstructions[i].TargetModification == null) continue;
                        isolatedGrids.Add(preset.CellsInstructions[i], grid.CopyEmpty());
                        isolatedGenerated.Add(preset.CellsInstructions[i], new List<IGenerating>());
                    }

                // First fill all cells for all isolated grids
                bool any = false;
                for (int i = 0; i < guides.Count; i++)
                {
                    if (guides[i].definition == null) continue;
                    if (guides[i].definition.InstructionType != InstructionDefinition.EInstruction.IsolatedGrid) continue;
                    var iGrid = isolatedGrids[guides[i].definition];
                    if (iGrid == null) continue;
                    iGrid.AddCell(guides[i].gridPosition);
                    any = true;
                }

                if (any) // If any isolated process occured
                {
                    for (int i = 0; i < preset.CellsInstructions.Count; i++) // Running modificators on isolated grids
                        if (preset.CellsInstructions[i].InstructionType == InstructionDefinition.EInstruction.IsolatedGrid)
                        {
                            var mod = preset.CellsInstructions[i].TargetModification;
                            if (mod == null) continue;

                            var iGrid = isolatedGrids[preset.CellsInstructions[i]];
                            var randCells = GetRandomizedCells(iGrid);
                            var randCells2 = GetRandomizedCells(iGrid);

                            if (mod.VariantOf != null)
                                mod.VariantOf.ModifyGraph(preset, iGrid, randCells, randCells2, mod);
                            else
                                mod.ModifyGraph(preset, iGrid, randCells, randCells2);
                        }

                    for (int i = 0; i < preset.CellsInstructions.Count; i++) // Getting spawn datas for all isolated grids and guides 
                        if (preset.CellsInstructions[i].InstructionType == InstructionDefinition.EInstruction.IsolatedGrid)
                        {
                            var mod = preset.CellsInstructions[i].TargetModification;
                            if (mod == null) continue;

                            var iGrid = isolatedGrids[preset.CellsInstructions[i]];
                            isolatedGenerated[preset.CellsInstructions[i]] = RunGraphSpawners(iGrid, container, preset, gen.Instantiated, transformMatrix);
                        }
                }
            }

            #endregion




            if (runRules)
            {
                #region Preparing grid 

                var randCells = GetRandomizedCells(grid);
                var randCells2 = GetRandomizedCells(grid);

                #endregion

                // First -> Running spawning guides -> Doors / Door Holes / Pre spawners
                if (guides != null) preset.RunPreInstructionsOnGraph(grid, guides);

                #region Temporary Pre Injections

                if (preset.temporaryInjections != null)
                    for (int i = 0; i < preset.temporaryInjections.Count; i++)
                    {
                        if (preset.temporaryInjections[i].Call == InjectionSetup.EGridCall.Pre)
                        {
                            if (preset.temporaryInjections[i].Inject == InjectionSetup.EInjectTarget.Modificator)
                            {
                                if (preset.temporaryInjections[i].Modificator != null)
                                    preset.RunModificatorOnGrid(grid, randCells, randCells2, preset.temporaryInjections[i].Modificator, false);
                            }
                            else if (preset.temporaryInjections[i].Inject == InjectionSetup.EInjectTarget.Pack)
                            {
                                if (preset.temporaryInjections[i].ModificatorsPack != null)
                                    if (preset.temporaryInjections[i].ModificatorsPack.DisableWholePackage == false)
                                        preset.RunModificatorPackOn(preset.temporaryInjections[i].ModificatorsPack, grid, randCells, randCells2);
                            }
                        }

                    }

                #endregion

                preset.RunRulesOnGraph(grid, randCells, randCells2, guides);

                #region Filling with new cells for grid, Only with post modificator

                if (guides != null)
                    for (int i = 0; i < guides.Count; i++)
                        if (guides[i].IsModRunner)
                        {
                            FieldCell nCell = grid.GetCell(guides[i].gridPosition, false);

                            if (FGenerators.CheckIfIsNull(nCell ))
                            {
                                nCell = grid.GetCell(guides[i].gridPosition, true);
                                newCells.Add(nCell);
                            }

                            nCell.InTargetGridArea = true;
                        }

                #endregion

                #region Temporary Post Injections

                if (preset.temporaryInjections != null)
                    for (int i = 0; i < preset.temporaryInjections.Count; i++)
                        if (preset.temporaryInjections[i].Call == InjectionSetup.EGridCall.Post)
                        {
                            if (preset.temporaryInjections[i].Inject == InjectionSetup.EInjectTarget.Modificator)
                            {
                                if (preset.temporaryInjections[i].Modificator != null)
                                {
                                    preset.RunModificatorOnGrid(grid, randCells, randCells2, preset.temporaryInjections[i].Modificator, false);
                                }
                            }
                            else if (preset.temporaryInjections[i].Inject == InjectionSetup.EInjectTarget.Pack)
                            {
                                if (preset.temporaryInjections[i].ModificatorsPack != null)
                                    if (preset.temporaryInjections[i].ModificatorsPack.DisableWholePackage == false)
                                        preset.RunModificatorPackOn(preset.temporaryInjections[i].ModificatorsPack, grid, randCells, randCells2);
                            }
                        }

                #endregion

                if (guides != null) preset.RunPostInstructionsOnGraph(grid, guides);
            }

            RestorePresetVariables(preset);

            List<IGenerating> generatorsSpawned = RunGraphSpawners(grid, container, preset, gen.Instantiated, transformMatrix);

            Bounds fullBounds = GetBounds(grid, gen.Instantiated, preset, transformMatrix, container.position);
            gen.RoomBounds = fullBounds;

            preset.PostEvents(ref gen, grid, fullBounds, container);


            #region Applying isolated grids to generators spawned list

            if (isolatedGenerated != null)
            {
                foreach (var item in isolatedGenerated)
                {
                    var spawns = item.Value;
                    if (spawns == null) continue;

                    for (int i = 0; i < spawns.Count; i++)
                    {
                        generatorsSpawned.Add(spawns[i]);
                    }
                }
            }

            #endregion


            // Running generators implementing IGenerating interface
            if (runEmitters)
            {
                for (int g = 0; g < generatorsSpawned.Count; g++)
                    generatorsSpawned[g].Generate();
            }
            else
            {
                for (int g = 0; g < generatorsSpawned.Count; g++)
                    generatorsSpawned[g].PreviewGenerate();
            }


            // Restoring grid
            for (int i = 0; i < newCells.Count; i++)
            {
                grid.RemoveCell(newCells[i]);
            }

            //preset.ClearTemporaryContainers();

            if (preset.SelfInjections != null)
                if (preset.temporaryInjections != null)
                    for (int i = 0; i < preset.SelfInjections.Count; i++)
                        preset.temporaryInjections.Remove(preset.SelfInjections[i]);

            return gen;
        }



        private static Bounds GetBounds(FGenGraph<FieldCell, FGenPoint> grid, List<GameObject> generateds, FieldSetup preset, Matrix4x4 transformMatrix, Vector3 worldOffset)
        {
            bool setted = false;
            Bounds modelBounds = new Bounds();

            for (int i = 0; i < generateds.Count; i++)
            {
                Renderer r = generateds[i].GetComponentInChildren<Renderer>();
                if (r)
                {
                    if (setted == false)
                    {
                        modelBounds = new Bounds(r.bounds.center, r.bounds.size);
                        setted = true;
                    }
                    else
                        modelBounds.Encapsulate(r.bounds);
                }
                else
                {
                    Collider c = generateds[i].GetComponentInChildren<Collider>();
                    if (c)
                    {
                        if (setted == false)
                        {
                            modelBounds = new Bounds(c.bounds.center, c.bounds.size);
                            setted = true;
                        }
                        else
                            modelBounds.Encapsulate(c.bounds);
                    }
                }
            }

            if (!setted) modelBounds.center += worldOffset;
            return modelBounds;
        }


        /// <summary>
        /// Converts bound to cell position starting in left down corner
        /// </summary>
        public static Vector3Int ConvertBoundsStartPosition(Bounds bound)
        {
            return new Vector3Int(Mathf.RoundToInt(bound.min.x), 0, Mathf.RoundToInt(bound.min.z));
        }

        internal static List<GameObject> GenerateFieldObjects(object fieldSetup, FGenGraph<FieldCell, FGenPoint> grid, Transform nContainer, bool v, List<SpawnInstruction> guides)
        {
            throw new NotImplementedException();
        }
    }
}