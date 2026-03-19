using UnityEngine;
using System.Collections.Generic;

public class ColorRoller : MonoBehaviour
{
    public RectTransform strip;
    public float spinSpeed = 600f;      // Initial spin speed
    public float spinDuration = 3f;     // Seconds before slowdown
    public float slowDownRate = 4f;

    private List<RectTransform> tiles = new List<RectTransform>();
    private float timer;
    private bool spinning;
    private bool slowing;

    private float currentSpinSpeed;     // Runtime speed (IMPORTANT FIX)

    private float tileWidth = 160f;      // tile + spacing

    void Start()
    {
        tiles.Clear();

        float x = 0f;

        foreach (Transform child in strip)
        {
            RectTransform tile = child as RectTransform;
            tiles.Add(tile);

            tile.anchorMin = new Vector2(0f, 0.5f);
            tile.anchorMax = new Vector2(0f, 0.5f);
            tile.pivot = new Vector2(0.5f, 0.5f);

            tile.anchoredPosition = new Vector2(x, 0f);
            x += tileWidth;
        }
    }

    void Update()
    {
        if (!spinning) return;

        timer += Time.deltaTime;

        // After N seconds, start slowing down
        if (timer >= spinDuration)
        {
            slowing = true;
            currentSpinSpeed = Mathf.Lerp(
                currentSpinSpeed,
                0f,
                Time.deltaTime * slowDownRate
            );
        }

        MoveTiles(currentSpinSpeed);

        // Stop condition
        if (slowing && currentSpinSpeed < 20f)
        {
            spinning = false;
            SnapToNearest();
        }
    }

    void MoveTiles(float speed)
    {
        foreach (var tile in tiles)
        {
            tile.anchoredPosition += Vector2.left * speed * Time.deltaTime;

            // Recycle tile if it goes offscreen
            if (tile.anchoredPosition.x < -tileWidth)
            {
                float rightMostX = GetRightMostTileX();
                tile.anchoredPosition = new Vector2(rightMostX + tileWidth, 0f);
            }
        }
    }

    float GetRightMostTileX()
    {
        float maxX = float.MinValue;

        foreach (var tile in tiles)
        {
            if (tile.anchoredPosition.x > maxX)
                maxX = tile.anchoredPosition.x;
        }

        return maxX;
    }

    void SnapToNearest()
    {
        RectTransform closest = null;
        float closestDistance = float.MaxValue;

        foreach (var tile in tiles)
        {
            float distance = Mathf.Abs(tile.anchoredPosition.x);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closest = tile;
            }
        }

        Vector2 offset = new Vector2(-closest.anchoredPosition.x, 0f);

        foreach (var tile in tiles)
        {
            tile.anchoredPosition += offset;
        }
    }

    // ✅ SAFE TO CALL FROM A UI BUTTON
    public void StartSpin()
    {
        if (spinning) return;   // Prevent button spam

        timer = 0f;
        slowing = false;
        spinning = true;

        currentSpinSpeed = spinSpeed; // ✅ RESET SPEED EACH SPIN
    }
}