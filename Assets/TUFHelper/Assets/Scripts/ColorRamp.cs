using System.Collections.Generic;
using UnityEngine;

public class ColorRamp
{
    public enum InterpolationType
    {
        Constant,
        Linear,
        CatmullRom,
    }

    public class RampPoint
    {
        public float position;
        public Color color = Color.white;
        public InterpolationType interpolation = InterpolationType.Linear;
    }

    public List<RampPoint> points = new()
    {
        new() { position = 0.0f, color = Color.black },
        new() { position = 1.0f, color = Color.white }
    };

    public Color Evaluate(float t)
    {
        if (points == null || points.Count == 0) return Color.white;
        if (points.Count == 1) return points[0].color;

        t = Mathf.Clamp01(t);

        points.Sort((a, b) => a.position.CompareTo(b.position));

        if (t <= points[0].position) return points[0].color;
        if (t >= points[^1].position) return points[^1].color;

        for (int i = 0; i < points.Count - 1; i++)
        {
            var p0 = points[i];
            var p1 = points[i + 1];

            if (t >= p0.position && t <= p1.position)
            {
                float localT = (t - p0.position) / Mathf.Max(0.0001f, p1.position - p0.position);

                switch (p0.interpolation)
                {
                    case InterpolationType.Constant:
                        return p0.color;

                    case InterpolationType.Linear:
                        return Color.Lerp(p0.color, p1.color, localT);

                    case InterpolationType.CatmullRom:
                        float smoothT = localT * localT * (3f - 2f * localT);
                        return Color.Lerp(p0.color, p1.color, smoothT);
                }
            }
        }

        return points[^1].color;
    }
}
