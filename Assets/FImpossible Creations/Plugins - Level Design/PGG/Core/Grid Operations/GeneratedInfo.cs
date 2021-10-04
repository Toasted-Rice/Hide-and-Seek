using FIMSpace.Generating.Checker;
using System.Collections.Generic;
using UnityEngine;

namespace FIMSpace.Generating
{
    /// <summary>
    /// Class with informations about generated game objects by FieldSetup
    /// </summary>
    [System.Serializable]
    public class FieldGenerationInfo
    {
        public FieldSetup ParentSetup;

        public List<GameObject> Instantiated;
        public GameObject MainContainer;
        public Transform FieldTransform;

        public FGenGraph<FieldCell, FGenPoint> Grid;

        public Bounds RoomBounds;

        public LightProbeGroup GeneratedLightProbes;
        public List<ReflectionProbe> GeneratedReflectionProbes;
        public List<BoxCollider> GeneratedTriggers;
        
        public List<CheckerField> OptionalCheckerFieldsData;


        /// <summary>
        /// List of grid cells positions in world with center origin
        /// </summary>
        public List<Vector3> GetGridWorldPositions()
        {
            List<Vector3> pos = new List<Vector3>();
            Vector3 size = ParentSetup.GetCellUnitSize();
            for (int i = 0; i < Grid.AllApprovedCells.Count; i++)
            {
                pos.Add(Grid.AllApprovedCells[i].WorldPos(size));
            }

            return pos;
        }


        public Transform GetTriggersContainer()
        {
            for (int i = 0; i < GeneratedTriggers.Count; i++)
            {
                if (GeneratedTriggers[i].transform.parent.name.Contains("Triggers")) return GeneratedTriggers[i].transform.parent;
            }

            return null;
        }


        public float GetCellSize()
        {
            return ParentSetup.GetCellUnitSize().x;
        }
    }

}