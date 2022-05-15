using System.Collections;
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
        public KeplerVector3d[] OrbitPoints;
    }

    [SerializeField] private Camera _targetCamera;
    [SerializeField] private LineRenderer _lineTemplate;
    [SerializeField] private float _camDistance;

    private readonly List<LineRenderer> _linesRend = new List<LineRenderer>();
    private readonly List<TargetItem> _targets = new List<TargetItem>();
    private readonly Dictionary<string, List<List<Vector3>>> _paths = new Dictionary<string, List<List<Vector3>>>();
    private readonly List<List<Vector3>> _pool = new List<List<Vector3>>();
    private float lineAlpha = 1f;

    const float minOrbitLinearSize = 0.001f;

    private void Awake()
    {
        var bodies = FindObjectsOfType<KeplerOrbitMover>();

        foreach (var item in bodies)
            AddTargetBody(item);

        var keys = _lineTemplate.colorGradient.alphaKeys;

        if (keys.Length > 0)
            lineAlpha = keys[0].alpha;
    }

    private void AddTargetBody(KeplerOrbitMover obj)
    {
        if (obj.AttractorSettings.AttractorObject == null || obj.OrbitData.MeanMotion <= 0) return;
        {
            _targets.Add(new TargetItem()
            {
                Body = obj,
                OrbitPoints = new KeplerVector3d[0]
            });
        }
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
            item.Body.OrbitData.GetOrbitPointsNoAlloc(ref orbitPoints, item.Body.OrbitPointsCount, new KeplerVector3d(), item.Body.MaxOrbitWorldUnitsDistance);
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

        RefreshLineRenderersForCurrentSegments(allVisibleSegments);
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

    private void RefreshLineRenderersForCurrentSegments(Dictionary<string, List<List<Vector3>>> allSegments)
    {
        var i = 0;
        foreach (var kv in allSegments)
        {
            var bodyName = kv.Key;

            foreach (var segment in kv.Value)
            {
                LineRenderer instance;

                if (i >= _linesRend.Count)
                {
                    instance = CreateLineRendererInstance();

                    _linesRend.Add(instance);
                }
                else
                {
                    instance = _linesRend[i];
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

        for (int j = i; j < _linesRend.Count; j++)
        {
            _linesRend[j].enabled = false;
        }
    }

    private LineRenderer CreateLineRendererInstance()
    {
        var result = Instantiate(_lineTemplate, _targetCamera.transform);
        SetAlphaLine(result, 0f);
        result.gameObject.SetActive(true);
        return result;
    }

    public IEnumerator EaseLines(float easeTime, bool easeOut = false)
    {
        var timeElapsed = 0f;
        var alpha1 = easeOut ? lineAlpha : 0f;
        var alpha2 = easeOut ? 0f : lineAlpha;

        if (!easeOut) enabled = true;

        while (timeElapsed < easeTime)
        {
            var alpha = Mathf.Lerp(alpha1, alpha2, timeElapsed / easeTime);

            SetAlphaLines(alpha);
            timeElapsed += Time.deltaTime;

            yield return null;
        }

        if (easeOut)
        {
            SetAlphaLines(0f);
            enabled = false;
        }
    }

    private void SetAlphaLines(float alpha)
    {

        for (int i = 0; i < _linesRend.Count; i++)
        {
            var lineRend = _linesRend[i];
            SetAlphaLine(lineRend,alpha);
        }
    }

    private void SetAlphaLine(LineRenderer lineRend, float alpha)
    {
        var gradient = new Gradient();

        if (lineRend != null)
        {
            gradient.SetKeys
            (
                lineRend.colorGradient.colorKeys, new GradientAlphaKey[] { new GradientAlphaKey(alpha, 1f) }
            );
            lineRend.colorGradient = gradient;
        }
    }

    private List<Vector3> GetListFromPool()
    {
        if (_pool.Count == 0)
            return new List<Vector3>();

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