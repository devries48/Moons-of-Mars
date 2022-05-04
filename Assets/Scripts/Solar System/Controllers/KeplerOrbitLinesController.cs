using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Scene orbits lines display on camera projection for the demo scene.
/// </summary>
/// <remarks>
/// Crude implementation of centralized orbit lines display system that solves line width issue of the regular orbits lines components.
/// Utilizes internal pooling of allocated objects to minimize GC usage.
/// </remarks>

public class KeplerOrbitLinesController : MonoBehaviour
{
    public class TargetItem
    {
        public KeplerOrbitMover Body;
        public KeplerOrbitLine OrbitLine;
        public KeplerVector3d[] OrbitPoints;
    }

    [SerializeField] private Camera _targetCamera;
    [SerializeField] private LineRenderer _lineTemplate;
    [SerializeField] private float _camDistance;

    private List<LineRenderer> _instances = new List<LineRenderer>();
    private List<TargetItem> _targets = new List<TargetItem>();
    private Dictionary<string, List<List<Vector3>>> _paths = new Dictionary<string, List<List<Vector3>>>();
    private List<List<Vector3>> _pool = new List<List<Vector3>>();

    const float minOrbitLinearSize = 0.001f;

    private void Awake()
    {
        var bodies = FindObjectsOfType<KeplerOrbitMover>();

        foreach (var item in bodies)
            AddTargetBody(item);
    }

    private void OnDisable()
    {
        foreach (var instance in _instances)
        {
            instance.positionCount = 0;

        }
    }

    private void AddTargetBody(KeplerOrbitMover obj)
    {
        if (obj.AttractorSettings.AttractorObject == null || obj.OrbitData.MeanMotion <= 0) return;

        var orbitLine = obj.GetComponent<KeplerOrbitLine>();

        if (orbitLine != null)
            AddTargetBody(obj, orbitLine);
    }

    private void AddTargetBody(KeplerOrbitMover body, KeplerOrbitLine orbitLine)
    {
        _targets.Add(new TargetItem()
        {
            Body = body,
            OrbitLine = orbitLine,
            OrbitPoints = new KeplerVector3d[0]
        });

        orbitLine.enabled = false;
    }

    private void LateUpdate()
    {
        var allVisibleSegments = _paths;

        foreach (var kv in allVisibleSegments)
        {
            foreach (var points in kv.Value)
                ReleaseList(points);

            kv.Value.Clear();
        }

        foreach (var item in _targets)
        {
            if (!item.Body.enabled || !item.Body.gameObject.activeInHierarchy) continue;

            var orbitPoints = item.OrbitPoints;
            item.Body.OrbitData.GetOrbitPointsNoAlloc(ref orbitPoints, item.OrbitLine.OrbitPointsCount, new KeplerVector3d(), item.OrbitLine.MaxOrbitWorldUnitsDistance);
            item.OrbitPoints = orbitPoints;
            var attrPos = item.Body.AttractorSettings.AttractorObject.position;
            var projectedPoints = GetListFromPool();

            ConvertOrbitPointsToProjectedPoints(orbitPoints, attrPos, _targetCamera, _camDistance, projectedPoints);

            List<List<Vector3>> segments;
            if (allVisibleSegments.ContainsKey(item.Body.name))
            {
                segments = allVisibleSegments[item.Body.name];
            }
            else
            {
                segments = new List<List<Vector3>>();
                allVisibleSegments[item.Body.name] = segments;
            }

            segments.Add(projectedPoints);
        }

        RefreshLineRenderersForCurrentSegments(allVisibleSegments, _instances);
    }

    private static void ConvertOrbitPointsToProjectedPoints(KeplerVector3d[] orbitPoints, Vector3 attractorPos, Camera targetCamera, float camDistance, List<Vector3> projectedPoints)
    {
        projectedPoints.Clear();

        if (projectedPoints.Capacity < orbitPoints.Length)
            projectedPoints.Capacity = orbitPoints.Length;

        var maxDistance = new Vector3();

        foreach (var p in orbitPoints)
        {
            var halfP = new Vector3((float)p.x, (float)p.y, (float)p.z);
            var worldPoint = attractorPos + halfP;
            var screenPoint = targetCamera.WorldToScreenPoint(worldPoint);
            if (screenPoint.z > 0)
            {
                screenPoint.z = camDistance;
            }
            else
            {
                screenPoint.z = -camDistance;
            }

            var projectedPoint = targetCamera.ScreenToWorldPoint(screenPoint);
            projectedPoints.Add(projectedPoint);
            var diff = projectedPoints[projectedPoints.Count - 1] - projectedPoints[0];

            if (diff.x > maxDistance.x) maxDistance.x = diff.x;
            if (diff.y > maxDistance.y) maxDistance.y = diff.y;
            if (diff.z > maxDistance.z) maxDistance.z = diff.z;
        }

        if (maxDistance.magnitude < minOrbitLinearSize)
        {
            projectedPoints.Clear();
        }
    }

    private void RefreshLineRenderersForCurrentSegments(Dictionary<string, List<List<Vector3>>> allSegments,List<LineRenderer> instances)
    {
        var i = 0;
        foreach (var kv in allSegments)
        {
            var bodyName = kv.Key;
            foreach (var segment in kv.Value)
            {
                LineRenderer instance;
                if (i >= instances.Count)
                {
                    instance = CreateLineRendererInstance();
                    instances.Add(instance);
                }
                else
                {
                    instance = instances[i];
                }

                instance.positionCount = segment.Count;
                for (int j = 0; j < segment.Count; j++)
                {
                    instance.SetPosition(j, segment[j]);
                }

                instance.enabled = true;

                i++;
            }
        }

        for (int j = i; j < instances.Count; j++)
        {
            instances[j].enabled = false;
        }
    }

    private LineRenderer CreateLineRendererInstance()
    {
        var result = Instantiate(_lineTemplate, _targetCamera.transform);
        result.gameObject.SetActive(true);
        return result;
    }

    private List<Vector3> GetListFromPool()
    {
        if (_pool.Count == 0)
        {
            return new List<Vector3>();
        }

        int last = _pool.Count - 1;
        var result = _pool[last];
        _pool.RemoveAt(last);
        result.Clear();
        return result;
    }

    private void ReleaseList(List<Vector3> list)
    {
        _pool.Add(list);
    }
}