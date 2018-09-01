/* ############################################################################## *\

    Copyright (c) 2018 Karim Takieddine

    Permission is hereby granted, free of charge, to any person obtaining a copy
    of this software and associated documentation files (the "Software"), to deal
    in the Software without restriction, including without limitation the rights
    to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
    copies of the Software, and to permit persons to whom the Software is
    furnished to do so, subject to the following conditions:

    The above copyright notice and this permission notice shall be included in all
    copies or substantial portions of the Software.

    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
    IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
    FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
    AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
    LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
    OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
    SOFTWARE.

 \* ############################################################################## */

using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class TileSprite : MonoBehaviour
{
    public struct Geometry
    {
        public int MinimumX { get; set; }
        public int MinimumY { get; set; }

        public int MaximumX { get; set; }
        public int MaximumY { get; set; }

        public Geometry(
            int minimumX,
            int minimumY,
            int maximumX,
            int maximumY
        )
        {
            MinimumX = minimumX;
            MinimumY = minimumY;

            MaximumX = maximumX;
            MaximumY = maximumY;
        }

        public override string ToString()
        {
            return  "xMin: " + MinimumX +
                    " - xMax: " + MaximumX +
                    " ---- yMin: " + MinimumY +
                    " - yMax: " + MaximumY;
        }
    };

    [System.Flags]
    public enum StateFlags
    {
        NONE                = 0,
        COMPONENTS_LOADED   = 1,
        NORMALIZED_SCALE    = 1 << 1
    };

    public SpriteRenderer   RendererComponent   { get; private set; }
    public Sprite           SpriteAsset         { get; private set; }
    public Geometry         PixelGeometry       { get; private set; }
    public StateFlags       CurrentState        { get; private set; }
    public Vector2          LocalScale          { get; private set; }
    public int              PixelsPerUnit       { get; private set; }

    public int Width, Height;

    public bool AreComponentsLoaded()
    {
        return ( ( CurrentState & StateFlags.COMPONENTS_LOADED ) == StateFlags.COMPONENTS_LOADED );
    }

    public void SetComponentsLoaded()
    {
        CurrentState |= StateFlags.COMPONENTS_LOADED;
    }

    public void UnsetComponentsLoaded()
    {
        CurrentState &= ~StateFlags.COMPONENTS_LOADED;
    }

    public void LoadComponentsAndAssets()
    {
        RendererComponent = GetComponent<SpriteRenderer>();
        SpriteAsset = RendererComponent.sprite;
        
        if (!SpriteAsset)
        {
            SpriteAsset = Resources.Load<Sprite>("Sprites/magenta_32_32");
            RendererComponent.sprite = SpriteAsset;
        }

        if (!SpriteAsset)
        {
            throw new UnityException("Could not load Sprite asset for GameObject: " + name + '!');
        }

        PixelsPerUnit = (int)SpriteAsset.pixelsPerUnit;

        SetComponentsLoaded();
    }

    public static int FindClosestMultipleOf(
        int direction,
        int number,
        int factor
    )
    {
        direction /= Mathf.Abs(direction);

        for (int i = number; i < direction * int.MaxValue; i += direction)
        {
            if ( ( i % factor ) == 0 )
            {
                return i;
            }
        }

        return number;
    }

    public void NormalizeTextureScale()
    {
        if (!AreComponentsLoaded())
        {
            return;
        }

        Texture2D texture = SpriteAsset.texture;
        int pixelWidth = texture.width;
        int pixelHeight = texture.height;

        int normalizedPixelWidth = FindClosestMultipleOf( ( pixelWidth < PixelsPerUnit ? -1 : 1 ), pixelWidth, PixelsPerUnit);
        int normalizedPixelHeight = FindClosestMultipleOf( ( pixelHeight < PixelsPerUnit ? -1 : 1 ), pixelHeight, PixelsPerUnit);

        LocalScale = new Vector2( (float)normalizedPixelWidth / pixelWidth, (float)normalizedPixelHeight / pixelHeight );
        transform.localScale = new Vector3(LocalScale.x * Width, LocalScale.y * Height, 1.0f);
    }

    public void ComputePixelGeometry()
    {
        if (!AreComponentsLoaded())
        {
            return;
        }

        if (Width == 0)
        {
            Width = 1;
        }

        if (Height == 0)
        {
            Height = 1;
        }

        Vector3 position = transform.position;
        int pixelMinimumX = (int)( ( position.x - ( 0.5f * Width ) ) * PixelsPerUnit );
        int pixelMinimumY = (int)( ( position.y - ( 0.5f * Height ) ) * PixelsPerUnit );

        PixelGeometry = new Geometry(
            pixelMinimumX,
            pixelMinimumY,
            pixelMinimumX + Width * PixelsPerUnit,
            pixelMinimumY + Height * PixelsPerUnit
        );
    }

    protected virtual void InitializeState() { }

	private void Awake()
    {
		if (!AreComponentsLoaded())
        {
            LoadComponentsAndAssets();
            NormalizeTextureScale();
            ComputePixelGeometry();
        }

        InitializeState();
	}
};