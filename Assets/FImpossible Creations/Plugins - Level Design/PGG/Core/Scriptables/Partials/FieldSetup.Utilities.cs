﻿using System.Collections.Generic;
using UnityEngine;

namespace FIMSpace.Generating
{
    public partial class FieldSetup : ScriptableObject
    {

        /// <summary>
        /// Returns cell position in world space
        /// </summary>
        public Vector3 GetCellWorldPosition(FieldCell cell)
        {
            Vector3 cSize = GetCellUnitSize();
            Vector3 pos = new Vector3(cell.Pos.x * cSize.x, cell.Pos.y * cSize.y, cell.Pos.z * cSize.z) /** CellSize*/ * cell.Scaler;

            if (cell.Scaler > 1)
            {
                cSize *= (0.5f * (cell.Scaler - 1));
                pos += new Vector3(cSize.x, 0, cSize.z);
            }

            return pos;
        }


        public bool IsEnabled(FieldModification mod)
        {
            for (int i = 0; i < disabledMods.Count; i++) if (disabledMods[i] == mod) return false;
            return true;
        }

        public void AddToDisabled(FieldModification mod)
        {
            if (disabledMods.Contains(mod) == false) disabledMods.Add(mod);
        }

        public void RemoveFromDisabled(FieldModification mod)
        {
            if (disabledMods.Contains(mod)) disabledMods.Remove(mod);
        }



        public InstructionDefinition FindCellInstruction(string title, bool ignoreCase = true)
        {
            InstructionDefinition def = null;

            if (ignoreCase)
            {
                string lower = title.ToLower();
                for (int i = 0; i < CellsInstructions.Count; i++)
                    if (CellsInstructions[i].Title.ToLower().Contains(lower))
                        return CellsInstructions[i];
            }
            else
            {
                for (int i = 0; i < CellsInstructions.Count; i++)
                    if (CellsInstructions[i].Title.Contains(title))
                        return CellsInstructions[i];
            }

            return def;
        }


        public InstructionDefinition FindCellInstruction(InstructionDefinition.EInstruction type)
        {
            InstructionDefinition def = null;

            for (int i = 0; i < CellsInstructions.Count; i++)
            {
                if (CellsInstructions[i].InstructionType == type)
                    return CellsInstructions[i];
            }

            return def;
        }

        /// <summary>
        /// Returns -1 if there is no such variable
        /// </summary>
        internal int GetVariableIndex(string name)
        {
            for (int i = 0; i < Variables.Count; i++)
                if (Variables[i].Name == name) return i;

            return -1;
        }

        internal FieldVariable GetVariable(string name)
        {
            for (int i = 0; i < Variables.Count; i++)
                if (Variables[i].Name == name) return Variables[i];

            return null;
        }

        internal bool SetVariable(string name, float value)
        {
            for (int i = 0; i < Variables.Count; i++)
                if (Variables[i].Name == name)
                {
                    Variables[i].SetValue(value);
                    return true;
                }

            return false;
        }

        internal void SetTemporaryInjections(List<InjectionSetup> injectMods)
        {
            SaveCurrentVariablesState();
            temporaryInjections = new List<InjectionSetup>();
            for (int i = 0; i < injectMods.Count; i++) temporaryInjections.Add(injectMods[i]);
        }

        internal void SetTemporaryInjections(List<FieldModification> injectMods, InjectionSetup.EGridCall call = InjectionSetup.EGridCall.Post)
        {
            SaveCurrentVariablesState();
            temporaryInjections = new List<InjectionSetup>();
            for (int i = 0; i < injectMods.Count; i++) temporaryInjections.Add(new InjectionSetup(injectMods[i], call));
        }

        private List<FieldVariable> variablesMemory = new List<FieldVariable>();
        private void SaveCurrentVariablesState()
        {
            variablesMemory.Clear();

            for (int i = 0; i < Variables.Count; i++)
                variablesMemory.Add(Variables[i].Copy());
        }

        private void RestoreSavedVariablesState()
        {
            if (variablesMemory.Count != Variables.Count) return;

            for (int i = 0; i < variablesMemory.Count; i++)
                Variables[i].SetValue(variablesMemory[i]);
        }

        internal void ClearTemporaryInjections()
        {
            temporaryInjections.Clear();
            RestoreSavedVariablesState();
        }

        public Vector3 TransformCellPosition(Vector3 pos)
        {
            return Vector3.Scale(pos, GetCellUnitSize());
        }

    }

}