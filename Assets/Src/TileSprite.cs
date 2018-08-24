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
        COMPONENTS_LOADED   = 1
    };

    public SpriteRenderer   RendererComponent   { get; private set; }
    public Sprite           SpriteAsset         { get; private set; }
    public Geometry         PixelGeometry       { get; private set; }
    public StateFlags       CurrentState        { get; private set; }
    public int              PixelWidth          { get; private set; }
    public int              PixelHeight         { get; private set; }
    public float            PixelsPerUnit       { get; private set; }

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
        SpriteAsset       = RendererComponent.sprite;
        
        if (!SpriteAsset)
        {
            SpriteAsset                 = Resources.Load<Sprite>("Sprites/magenta_32_32");
            RendererComponent.sprite    = SpriteAsset;
        }

        if (!SpriteAsset)
        {
            throw new UnityException("Could not load Sprite asset for GameObject: " + name + '!');
        }

        PixelsPerUnit = SpriteAsset.pixelsPerUnit;

        Texture2D texture   = SpriteAsset.texture;
        PixelWidth          = texture.width;
        PixelHeight         = texture.height;

        SetComponentsLoaded();
    }

    private void ComputePixelGeometry()
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
        
        Vector3 position    = transform.position;
        int pixelMinimumX   = (int)( ( position.x - ( 0.5f * Width ) ) * PixelsPerUnit );
        int pixelMinimumY   = (int)( ( position.y - ( 0.5f * Height ) ) * PixelsPerUnit );

        transform.localScale = new Vector3(Width, Height, 1.0f);
        
        PixelGeometry = new Geometry(
            pixelMinimumX,
            pixelMinimumY,
            pixelMinimumX + PixelWidth,
            pixelMinimumY + PixelHeight
        );
    }

	public void Awake()
    {
		if (!AreComponentsLoaded())
        {
            LoadComponentsAndAssets();
        }
	}
	
	protected virtual void Update ()
    {
        ComputePixelGeometry();
	}
};