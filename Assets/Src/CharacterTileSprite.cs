using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class CharacterTileSprite : AgentTileSprite
{
    protected override void ComputeVelocity()
    {
        // TODO!
    }

    protected override void ResolveCollisions()
    {
        List<AgentTileSprite> obstacleTileList  = new List<AgentTileSprite>();
        AgentTileSprite[] tileSpriteInstances   = FindObjectsOfType<AgentTileSprite>();

        for (int i = 0; i < tileSpriteInstances.Length; ++i)
        {
            AgentTileSprite tileSpriteInstance = tileSpriteInstances[i];

            if ( tileSpriteInstance != this && ( tileSpriteInstance.CollisionLayerFlag & CollisionLayerFlags.OBSTACLE ) == CollisionLayerFlags.OBSTACLE )
            {
                obstacleTileList.Add(tileSpriteInstance);
            }
        }

        int obstacleCount = obstacleTileList.Count;

        if (obstacleCount == 0)
        {
            return;
        }

        for (int i = 0; i < obstacleCount; ++i)
        {
            AgentTileSprite obstacleTileSprite = obstacleTileList[i];

            int obstacleMinimumX = obstacleTileSprite.PixelPositionX;
            int obstacleMaximumX = obstacleMinimumX + obstacleTileSprite.PixelWidth * obstacleTileSprite.Width;

            int obstacleMinimumY = obstacleTileSprite.PixelPositionY;
            int obstacleMaximumY = obstacleMinimumY + obstacleTileSprite.PixelHeight * obstacleTileSprite.Height;

            int pixelMinimumX = PixelPositionX;
            int pixelMaximumX = pixelMinimumX + PixelWidth * Width;

            int pixelMinimumY = PixelPositionY;
            int pixelMaximumY = pixelMinimumY + PixelHeight * Height;
            
            if (
                obstacleMinimumX    >= pixelMaximumX ||
                pixelMinimumX       >= obstacleMaximumX ||
                obstacleMinimumY    >= pixelMaximumY ||
                pixelMinimumY       >= obstacleMaximumY
            )
            {
                continue;
            }

            int[] pixelDifferences = new int[4]
            {
                Mathf.Abs(obstacleMinimumX - pixelMaximumX),
                Mathf.Abs(obstacleMaximumX - pixelMinimumX),
                Mathf.Abs(obstacleMinimumY - pixelMaximumY),
                Mathf.Abs(obstacleMaximumY - pixelMinimumY)
            };

            int minimumPixelDifference = pixelDifferences.Min();

            if (minimumPixelDifference == pixelDifferences[0])
            {
                PixelPositionX -= minimumPixelDifference;
                PixelRemainderX = 0.0f;
            }
            else if (minimumPixelDifference == pixelDifferences[1])
            {
                PixelPositionX += minimumPixelDifference;
                PixelRemainderX = 0.0f;
            }
            else if (minimumPixelDifference == pixelDifferences[2])
            {
                PixelPositionY -= minimumPixelDifference;
                PixelRemainderY = 0.0f;
            }
            else if (minimumPixelDifference == pixelDifferences[3])
            {
                PixelPositionY += minimumPixelDifference;
                PixelRemainderY = 0.0f;
            }
        }
    }
};