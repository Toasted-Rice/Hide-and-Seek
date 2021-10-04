using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggersRelation : MonoBehaviour
{
    public Collider Trigger;
    public List<TriggersRelation> Neightbours;
    //[SerializeField] [HideInInspector] private List<Vector3> IntersectLocalPoints;
    //public Vector3 GetIntersectionPoint(TriggersRelation other)
    //{
    //    if (Neightbours != null)
    //        for (int i = 0; i < Neightbours.Count; i++)
    //        {
    //            if (Neightbours[i] == other)
    //                return transform.TransformPoint(IntersectLocalPoints[i]);
    //        }

    //    return transform.position;
    //}

    private void Reset()
    {
        TryGetTrigger();
    }

    private void OnValidate()
    {
        TryGetTrigger();
    }

    public void AddNeightbour(TriggersRelation rel)
    {
        if (rel == null) return;
        if (Neightbours.Contains(rel) == false)
        {
            if (Neightbours == null) Neightbours = new List<TriggersRelation>();
            Neightbours.Add(rel);
        }
    }

    public void TryGetTrigger()
    {
        Trigger = GetComponent<Collider>();
        if (Trigger) if (Trigger.isTrigger == false) Trigger = null;
    }

    private void OnDrawGizmosSelected()
    {
        if (Trigger == null) return;

        Color bc = Gizmos.color;

        Gizmos.color = Color.green;
        for (int i = 0; i < Neightbours.Count; i++)
        {
            if (Neightbours[i] == null) continue;
            if (Neightbours[i].Trigger == null) continue;
            Gizmos.DrawLine(Trigger.bounds.center, Neightbours[i].Trigger.bounds.center);
        }

        Gizmos.color = bc;
    }

    internal void Refresh()
    {
        if (Neightbours == null) Neightbours = new List<TriggersRelation>();
        TryGetTrigger();
    }
}
