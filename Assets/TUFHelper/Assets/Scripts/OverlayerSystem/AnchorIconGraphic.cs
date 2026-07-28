using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
[RequireComponent(typeof(CanvasRenderer))]
public class AnchorIconGraphic : MaskableGraphic
{
    public Anchor anchor = Anchor.Center;

    [Header("Colors")]
    public Color outerBoxColor = new Color(0.35f, 0.35f, 0.35f, 1f);
    public Color innerBoxColor = new Color(0.25f, 0.25f, 0.25f, 1f);
    public Color lineAxisColor = new Color(0.85f, 0.25f, 0.25f, 1f); // Red guide lines
    public Color dotColor = new Color(0.9f, 0.65f, 0.1f, 1f);        // Yellow anchor dot

    [Header("Sizing")]
    public float dotSize = 5f;
    public float lineThickness = 1.2f;
    public float innerBoxRatio = 0.45f; // Scale of inner box relative to outer box

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();

        Rect r = GetPixelAdjustedRect();
        float halfW = r.width * 0.5f - 2f;
        float halfH = r.height * 0.5f - 2f;

        // 1. Draw Outer Wireframe Box (FIRST)
        Rect outerRect = new Rect(-halfW, -halfH, halfW * 2f, halfH * 2f);
        DrawRectOutline(vh, outerRect, lineThickness, outerBoxColor);

        // 2. Draw Inner Wireframe Box (SECOND)
        float inW = halfW * innerBoxRatio;
        float inH = halfH * innerBoxRatio;
        Rect innerRect = new Rect(-inW, -inH, inW * 2f, inH * 2f);
        DrawRectOutline(vh, innerRect, lineThickness, innerBoxColor);

        // 3. Determine Anchor Coordinates
        Vector2 dotPos = Vector2.zero;
        switch (anchor)
        {
            case Anchor.LeftTop: dotPos = new Vector2(-halfW, halfH); break;
            case Anchor.MiddleTop: dotPos = new Vector2(0f, halfH); break;
            case Anchor.RightTop: dotPos = new Vector2(halfW, halfH); break;

            case Anchor.LeftMiddle: dotPos = new Vector2(-halfW, 0f); break;
            case Anchor.Center: dotPos = Vector2.zero; break;
            case Anchor.RightMiddle: dotPos = new Vector2(halfW, 0f); break;

            case Anchor.LeftBottom: dotPos = new Vector2(-halfW, -halfH); break;
            case Anchor.MiddleBottom: dotPos = new Vector2(0f, -halfH); break;
            case Anchor.RightBottom: dotPos = new Vector2(halfW, -halfH); break;
        }

        // 4. Draw Red Axis Lines (THIRD - Drawn ON TOP of the grey boxes so red stays 100% solid)
        DrawAxisLines(vh, outerRect, dotPos);

        // 5. Draw Yellow Active Anchor Dot (LAST)
        Rect dotRect = new Rect(dotPos.x - dotSize * 0.5f, dotPos.y - dotSize * 0.5f, dotSize, dotSize);
        DrawSolidRect(vh, dotRect, dotColor);
    }

    private void DrawAxisLines(VertexHelper vh, Rect bounds, Vector2 dotPos)
    {
        float halfW = bounds.width * 0.5f;
        float halfH = bounds.height * 0.5f;

        // 1. CENTER: Full crosshair spanning all 4 directions
        if (anchor == Anchor.Center)
        {
            // Horizontal crosshair
            Rect hLine = new Rect(bounds.xMin, -lineThickness * 0.5f, bounds.width, lineThickness);
            DrawSolidRect(vh, hLine, lineAxisColor);

            // Vertical crosshair
            Rect vLine = new Rect(-lineThickness * 0.5f, bounds.yMin, lineThickness, bounds.height);
            DrawSolidRect(vh, vLine, lineAxisColor);
            return;
        }

        // 2. EDGE MIDDLES: Single perpendicular line pointing outward from center
        if (anchor == Anchor.MiddleTop)
        {
            // Vertical line through the top wall
            Rect line = new Rect(-lineThickness * 0.5f, bounds.yMin, lineThickness, bounds.height);
            DrawSolidRect(vh, line, lineAxisColor);
        }
        else if (anchor == Anchor.MiddleBottom)
        {
            // Vertical line through the bottom wall
            Rect line = new Rect(-lineThickness * 0.5f, bounds.yMin, lineThickness, bounds.height);
            DrawSolidRect(vh, line, lineAxisColor);
        }
        else if (anchor == Anchor.LeftMiddle)
        {
            // Horizontal line through the left wall
            Rect line = new Rect(bounds.xMin, -lineThickness * 0.5f, bounds.width, lineThickness);
            DrawSolidRect(vh, line, lineAxisColor);
        }
        else if (anchor == Anchor.RightMiddle)
        {
            // Horizontal line through the right wall
            Rect line = new Rect(bounds.xMin, -lineThickness * 0.5f, bounds.width, lineThickness);
            DrawSolidRect(vh, line, lineAxisColor);
        }

        else if (anchor == Anchor.LeftTop)
        {
            // Top edge
            DrawSolidRect(vh, new Rect(bounds.xMin, bounds.yMax - lineThickness, bounds.width, lineThickness), lineAxisColor);
            // Left edge
            DrawSolidRect(vh, new Rect(bounds.xMin, bounds.yMin, lineThickness, bounds.height), lineAxisColor);
        }
        else if (anchor == Anchor.RightTop)
        {
            // Top edge
            DrawSolidRect(vh, new Rect(bounds.xMin, bounds.yMax - lineThickness, bounds.width, lineThickness), lineAxisColor);
            // Right edge
            DrawSolidRect(vh, new Rect(bounds.xMax - lineThickness, bounds.yMin, lineThickness, bounds.height), lineAxisColor);
        }
        else if (anchor == Anchor.LeftBottom)
        {
            // Bottom edge
            DrawSolidRect(vh, new Rect(bounds.xMin, bounds.yMin, bounds.width, lineThickness), lineAxisColor);
            // Left edge
            DrawSolidRect(vh, new Rect(bounds.xMin, bounds.yMin, lineThickness, bounds.height), lineAxisColor);
        }
        else if (anchor == Anchor.RightBottom)
        {
            // Bottom edge
            DrawSolidRect(vh, new Rect(bounds.xMin, bounds.yMin, bounds.width, lineThickness), lineAxisColor);
            // Right edge
            DrawSolidRect(vh, new Rect(bounds.xMax - lineThickness, bounds.yMin, lineThickness, bounds.height), lineAxisColor);
        }
    }

    private void DrawSolidRect(VertexHelper vh, Rect rect, Color col)
    {
        int startIndex = vh.currentVertCount;

        UIVertex vert = UIVertex.simpleVert;
        vert.color = col;

        vert.position = new Vector3(rect.xMin, rect.yMin); vh.AddVert(vert);
        vert.position = new Vector3(rect.xMin, rect.yMax); vh.AddVert(vert);
        vert.position = new Vector3(rect.xMax, rect.yMax); vh.AddVert(vert);
        vert.position = new Vector3(rect.xMax, rect.yMin); vh.AddVert(vert);

        vh.AddTriangle(startIndex, startIndex + 1, startIndex + 2);
        vh.AddTriangle(startIndex, startIndex + 2, startIndex + 3);
    }

    private void DrawRectOutline(VertexHelper vh, Rect rect, float thickness, Color col)
    {
        // Top
        DrawSolidRect(vh, new Rect(rect.xMin, rect.yMax - thickness, rect.width, thickness), col);
        // Bottom
        DrawSolidRect(vh, new Rect(rect.xMin, rect.yMin, rect.width, thickness), col);
        // Left
        DrawSolidRect(vh, new Rect(rect.xMin, rect.yMin, thickness, rect.height), col);
        // Right
        DrawSolidRect(vh, new Rect(rect.xMax - thickness, rect.yMin, thickness, rect.height), col);
    }
}