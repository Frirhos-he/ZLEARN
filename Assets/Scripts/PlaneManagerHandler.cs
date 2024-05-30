using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class PlaneSizeComparer : Comparer<ARPlane>
{
    public static readonly Vector2 TargetPlaneSize = new() { x = 0.6f, y = 0.6f };
    public static readonly Vector2 MinPlaneSize = new() { x = 0.2f, y = 0.2f };

    public override int Compare(ARPlane plane1, ARPlane plane2)
    {
        float x_diff_plane1 = Mathf.Abs(plane1.size.x - TargetPlaneSize.x);
        float y_diff_plane1 = Mathf.Abs(plane1.size.y - TargetPlaneSize.y);
        float x_diff_plane2 = Mathf.Abs(plane2.size.x - TargetPlaneSize.x);
        float y_diff_plane2 = Mathf.Abs(plane2.size.y - TargetPlaneSize.y);
        float tot_diff_plane1 = x_diff_plane1 + y_diff_plane1;
        float tot_diff_plane2 = x_diff_plane2 + y_diff_plane2;
        return tot_diff_plane1.CompareTo(tot_diff_plane2);
    }
}

public class PlaneManagerHandler : MonoBehaviour
{
    private ARPlaneManager _planeManager;

    private ARPlane _foundedPlane = null;
    // Start is called before the first frame update
    static private bool IsPlaneGood(ARPlane plane, bool supportsPlaneClassification)
    {
        if (supportsPlaneClassification && 
            (plane.classification != PlaneClassification.Floor && plane.classification != PlaneClassification.Table))
        {
            // if not floor or table, discard
            Debug.Log($"Discarding plane because it wasn't a floor or table plane, it was {plane.classification}");
            return false;
        }
        else
        {
            Vector2 planeSize = plane.size;
            if (planeSize.x < PlaneSizeComparer.MinPlaneSize.x || planeSize.y < PlaneSizeComparer.MinPlaneSize.y)
            {
                // if plane is less than 40 cm on either side
                Debug.Log($"Discarding plane because it was only {planeSize.x}x{planeSize.y} meters");
                return false;
            }
        }
        return true;
    }

    public void Start()
    {
        _planeManager = GetComponent<ARPlaneManager>();
        var supportsPlaneClassification = _planeManager.subsystem != null ? _planeManager.subsystem.subsystemDescriptor.supportsClassification : false;
        Debug.Log($"Plane subsystem {(supportsPlaneClassification ? "does" : "doesn't")} support plane classification");
        _planeManager.planesChanged += (ev) =>
        {
            //se è stato trovato un piano allora non viene cambiato
            if (_foundedPlane != null)
            {
                foreach (var plane in ev.added.Concat(ev.updated))
                {
                    plane.gameObject.SetActive(false);
                }
                _foundedPlane.gameObject.SetActive(true);
                return;
            }
            
            // planeManager.requestedDetectionMode = UnityEngine.XR.ARSubsystems.PlaneDetectionMode.None;
            foreach (var plane in ev.added)
            {
                plane.gameObject.SetActive(IsPlaneGood(plane, supportsPlaneClassification));
            }
            foreach (var plane in ev.updated)
            {
                plane.gameObject.SetActive(IsPlaneGood(plane, supportsPlaneClassification));
            }
            List<ARPlane> planes = ev.added.Concat(ev.updated).Where(x => x.gameObject.activeSelf).ToList();
            if (planes.Count > 1)
            {
                // more than one good plane available
                // use some heuristic to pick one
                // in this case, closest to the target plane size wins
                planes.Sort(new PlaneSizeComparer());
                // and then disable all the ones except the first
                foreach (var plane in planes.Skip(1))
                {
                    Debug.Log($"Discarding plane even if it satisfied the requested size ({plane.size}) because there is a better match ({planes[0].size})");
                    plane.gameObject.SetActive(false);
                }
            }
            
            //se ci sono piani allora aggiungo il primo (quello attivo) come piano trovato
            if(planes.Count > 0)
            {
                _foundedPlane = planes.First();
            }
            
            Debug.Log($"Added {ev.added.Count} planes, removed {ev.removed.Count} planes and updated {ev.updated.Count} planes");
        };
    }

}
