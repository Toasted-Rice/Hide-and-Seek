﻿using System.Collections.Generic;
using UnityEngine;

namespace FIMSpace.Generating
{
    public partial class FieldSetup : ScriptableObject
    {


        /// <summary>
        /// Running all field guides on choosed grid and placing spawn data inside cells
        /// </summary>
        public void RunPreInstructionsOnGraph(FGenGraph<FieldCell, FGenPoint> grid, List<SpawnInstruction> guides)
        {
            if (guides != null)
            {
                // First add all custom data
                for (int i = 0; i < guides.Count; i++)
                {
                    if (FGenerators.CheckIfIsNull(guides[i].definition )) continue;

                    if (guides[i].definition.InstructionType == InstructionDefinition.EInstruction.InjectDataString)
                    {
                        var cell = grid.GetCell(guides[i].gridPosition, false);
                        if (FGenerators.CheckIfExist_NOTNULL(cell )) cell.AddCustomData(guides[i].definition.InstructionArgument);
                    }
                }

                // Then run pre modificators
                for (int i = 0; i < guides.Count; i++)
                {
                    if (FGenerators.CheckIfIsNull(guides[i].definition )) continue;
                    if (guides[i].definition.InstructionType == InstructionDefinition.EInstruction.PreRunModificator /*|| guides[i].definition.InstructionType == InstructionDefinition.EInstruction.DoorHole*/)
                    {
                        if (guides[i].definition.TargetModification != null)
                            RunModificatorWithInstruction(grid, guides[i].definition.TargetModification, guides[i]);
                    }
                }
            }
        }


        /// <summary>
        /// Running all field guides on choosed grid and placing spawn data inside cells
        /// </summary>
        public void RunPostInstructionsOnGraph(FGenGraph<FieldCell, FGenPoint> grid, List<SpawnInstruction> guides)
        {
            if (guides != null)
                for (int i = 0; i < guides.Count; i++)
                {
                    if (FGenerators.CheckIfIsNull(guides[i].definition )) continue;
                    if (guides[i].definition.InstructionType == InstructionDefinition.EInstruction.PostRunModificator || guides[i].definition.InstructionType == InstructionDefinition.EInstruction.DoorHole)
                    {
                        var cl = grid.GetCell(guides[i].gridPosition, true);

                        if (guides[i].definition.TargetModification != null)
                            RunModificatorWithInstruction(grid, guides[i].definition.TargetModification, guides[i]);
                    }
                }
        }


        /// <summary>
        /// Running all field modificators on choosed grid and placing spawn data inside cells
        /// </summary>
        public void RunRulesOnGraph(FGenGraph<FieldCell, FGenPoint> grid, List<FieldCell> randCells, List<FieldCell> randCells2, List<SpawnInstruction> instructions)
        {
            // Preparing provided instructions for cells
            if (instructions != null)
                for (int i = 0; i < instructions.Count; i++)
                {
                    if (instructions[i].definition == null) continue;
                    if (instructions[i].IsPreDefinition && instructions[i].definition.InstructionType != InstructionDefinition.EInstruction.DoorHole) continue;
                    if (instructions[i].IsPostDefinition) continue;
                    var cell = grid.GetCell(instructions[i].gridPosition, false);
                    if (cell == null) continue;
                    cell.AddCellInstruction(instructions[i]);
                }


            //for (int p = 0; p < ModificatorPacks.Count; p++)
            //{
            //    if (ModificatorPacks[p] == null) continue;
            //    if (ModificatorPacks[p].DisableWholePackage) continue;

            //    for (int r = 0; r < ModificatorPacks[p].FieldModificators.Count; r++)
            //    {
            //        if (ModificatorPacks[p].FieldModificators[r] == null) continue;
            //        if (ModificatorPacks[p].FieldModificators[r].Enabled == false) continue;
            //        if (Ignores.Contains(ModificatorPacks[p].FieldModificators[r])) continue;
            //        if (IsEnabled(ModificatorPacks[p].FieldModificators[r]) == false) continue;

            //        ModificatorPacks[p].FieldModificators[r].PrepareVariablesWith(this);
            //    }
            //}

            // Cleaning nulls
            for (int i = disabledMods.Count - 1; i >= 0; i--) if (disabledMods[i] == null) disabledMods.RemoveAt(i);

            // Running grid rules
            for (int p = 0; p < ModificatorPacks.Count; p++)
            {
                if (ModificatorPacks[p] == null) continue;
                if (ModificatorPacks[p].DisableWholePackage) continue;
                RunModificatorPackOn(ModificatorPacks[p], grid, randCells, randCells2);
            }
        }

        public void RunModificatorPackOn(ModificatorsPack pack, FGenGraph<FieldCell, FGenPoint> grid, List<FieldCell> randCells, List<FieldCell> randCells2)
        {
            if ( pack.SeedMode != ModificatorsPack.ESeedMode.None)
            {
                if ( pack.SeedMode == ModificatorsPack.ESeedMode.Reset)
                {
                    FGenerators.SetSeed(FGenerators.LatestSeed);
                }
                else if (pack.SeedMode == ModificatorsPack.ESeedMode.Custom)
                {
                    FGenerators.SetSeed(pack.CustomSeed);
                }
            }

            for (int r = 0; r < pack.FieldModificators.Count; r++)
            {
                if (pack.FieldModificators[r] == null) continue;
                if (pack.FieldModificators[r].Enabled == false) continue;
                if (Ignores.Contains(pack.FieldModificators[r])) continue;
                if (IsEnabled(pack.FieldModificators[r]) == false) continue;

                if (pack.FieldModificators[r].VariantOf != null)
                    pack.FieldModificators[r].VariantOf.ModifyGraph(this, grid, randCells, randCells2, pack.FieldModificators[r]);
                else
                    pack.FieldModificators[r].ModifyGraph(this, grid, randCells, randCells2);
            }
        }



        public void RunModificatorOnGrid(FGenGraph<FieldCell, FGenPoint> grid, List<FieldCell> randCells, List<FieldCell> randCells2, FieldModification modificator, bool dontRunIfDisabledByFieldSetup = true)
        {
            if (modificator == null)
            {
                UnityEngine.Debug.Log("[Interior Generator] Not assigned target modificator to " + name + "!");
                return;
            }

            if (modificator.Enabled == false) return;
            if (dontRunIfDisabledByFieldSetup) if (IsEnabled(modificator) == false) return;

            if (modificator.VariantOf != null)
                modificator.VariantOf.ModifyGraph(this, grid, randCells, randCells2, modificator);
            else
                modificator.ModifyGraph(this, grid, randCells, randCells2);
        }


        public void RunModificatorWithInstruction(FGenGraph<FieldCell, FGenPoint> grid, FieldModification modificator, SpawnInstruction guide)
        {
            if (modificator == null)
            {
                UnityEngine.Debug.Log("[Interior Generator] Not assigned target modificator to " + name + "!");
                return;
            }

            FieldCell cell = grid.GetCell(guide.gridPosition, false);
            if (FGenerators.CheckIfExist_NOTNULL(cell ))
            {
                if (cell.InTargetGridArea)
                {
                    bool ignoreRestr = false;
                    if (FGenerators.CheckIfExist_NOTNULL(guide.definition )) if (guide.definition.InstructionType == InstructionDefinition.EInstruction.DoorHole) ignoreRestr = true;

                    //UnityEngine.Debug.Log(" use dir = " + guide.useDirection);
                    if (guide.useDirection)
                        modificator.ModifyGraphCell(this, cell, grid, true, guide.FlatDirection, true, ignoreRestr);
                    else
                        modificator.ModifyGraphCell(this, cell, grid, true, null, true, ignoreRestr);
                }
            }
        }





    }

}