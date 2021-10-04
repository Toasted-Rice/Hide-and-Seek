using UnityEditor;
using UnityEngine;

namespace FIMSpace.Generating
{

    public partial class FieldModificationEditor
    {
        public FieldModification Get { get { if (_get == null) _get = (FieldModification)target; return _get; } }
        private FieldModification _get;


        public static void AddPrefabsContextMenuItems(GenericMenu menu, FieldModification Get)
        {

            if (Get.DrawSetupFor == FieldModification.EModificationMode.CustomPrefabs)
            {
                menu.AddItem(new GUIContent("Empty"), false, () =>
                { Get.AddSpawner(-2, FieldModification.EModificationMode.CustomPrefabs); });

                menu.AddItem(new GUIContent("Random"), false, () =>
                { Get.AddSpawner(-1, Get.DrawSetupFor); });


                if (Get.PrefabsList != null)
                {
                    for (int i = 0; i < Get.PrefabsList.Count; i++)
                    {
                        var sobj = Get.PrefabsList[i];
                        if (sobj == null) continue;
                        int ind = i;

                        if (Get.PrefabsList[i].Prefab == null) continue;

                        menu.AddItem(new GUIContent(Get.PrefabsList[i].Prefab.name), false, () =>
                        {
                            Get.AddSpawner(ind, Get.DrawSetupFor);
                        });
                    }
                }

            }
            else if ( Get.DrawSetupFor == FieldModification.EModificationMode.ObjectsStamp)
            {

                menu.AddItem(new GUIContent("Empty"), false, () =>
                { Get.AddSpawner(-2, FieldModification.EModificationMode.CustomPrefabs); });

                menu.AddItem(new GUIContent("Random"), false, () =>
                { Get.AddSpawner(-1, FieldModification.EModificationMode.ObjectsStamp); });

                menu.AddItem(new GUIContent("Emitter"), false, () =>
                { Get.AddSpawner(-3, FieldModification.EModificationMode.ObjectsStamp); });

                if (Get.OStamp != null)
                    for (int i = 0; i < Get.OStamp.Prefabs.Count; i++)
                    {
                        var sobj = Get.OStamp.Prefabs[i];
                        if (sobj == null) continue;
                        int ind = i;

                        if (Get.OStamp.Prefabs[i].Prefab == null) continue;

                        menu.AddItem(new GUIContent(Get.OStamp.Prefabs[i].Prefab.name), false, () =>
                        {
                            Get.AddSpawner(ind, Get.DrawSetupFor);
                        });
                    }
            }
            else if ( Get.DrawSetupFor == FieldModification.EModificationMode.ObjectMultiEmitter)
            {

                menu.AddItem(new GUIContent("Empty"), false, () =>
                { Get.AddSpawner(-2, FieldModification.EModificationMode.CustomPrefabs); });

                menu.AddItem(new GUIContent("Random Emitter"), false, () =>
                { Get.AddSpawner(-1, FieldModification.EModificationMode.ObjectMultiEmitter); });

                if (Get.OMultiStamp != null)
                    for (int i = 0; i < Get.OMultiStamp.PrefabsSets.Count; i++)
                    {
                        var sobj = Get.OMultiStamp.PrefabsSets[i];
                        if (sobj == null) continue;
                        int ind = i;

                        menu.AddItem(new GUIContent(Get.OMultiStamp.PrefabsSets[i].name), false, () =>
                        {
                            Get.AddSpawner(ind, Get.DrawSetupFor);
                        });
                    }


                //menu.AddItem(new GUIContent("Random Emitter"), false, () =>
                //{ Get.AddSpawner(-2, RoomModification.EModificationMode.ObjectMultiEmitter); });

            }
        }

    }
}