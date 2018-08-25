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
using System.Collections.Generic;
using System.Linq;

public class AgentTileSpriteManager : MonoBehaviour
{
    public static List<AgentTileSprite> ObstacleTileSpriteList  { get; private set; }
    public static List<AgentTileSprite> CharacterTileSpriteList { get; private set; }
    public static int ObstacleTileSpriteCount                   { get; private set; }
    public static int CharacterTileSpriteCount                  { get; private set; }

    public List<AgentTileSprite> UpdatedObstacleList { get; private set; }

    private void Awake()
    {
        ObstacleTileSpriteList  = new List<AgentTileSprite>();
        CharacterTileSpriteList = new List<AgentTileSprite>();
        UpdatedObstacleList     = new List<AgentTileSprite>();
    }

    private void LoadAgentTileSprites()
    {
        AgentTileSprite[] instances = FindObjectsOfType<AgentTileSprite>();
        int instanceCount    		= instances.Length;

        if (instanceCount == 0)
        {
            return;
        }

        if (ObstacleTileSpriteList.Count > 0)
        {
            ObstacleTileSpriteList.Clear();
        }

        if (CharacterTileSpriteList.Count > 0)
        {
            CharacterTileSpriteList.Clear();
        }

        for (int i = 0; i < instances.Length; ++i)
        {
            AgentTileSprite instance = instances[i];

            if (instance.IsObstacle())
            {
                ObstacleTileSpriteList.Add(instance);
            }
            else if (instance.IsCharacter())
            {
                CharacterTileSpriteList.Add(instance);
            }
        }

        ObstacleTileSpriteCount     = ObstacleTileSpriteList.Count;
        CharacterTileSpriteCount    = CharacterTileSpriteList.Count;
    }

    void Update ()
    {
		LoadAgentTileSprites();

        if (
            CharacterTileSpriteCount == 0 ||
            ObstacleTileSpriteCount == 0
        )
        {
            return;
        }

        UpdatedObstacleList.Clear();

        for (int i = 0; i < CharacterTileSpriteCount; ++i)
        {
            AgentTileSprite characterTileSprite = CharacterTileSpriteList[i];

            characterTileSprite.ComputePixelGeometry();
            characterTileSprite.ComputeVelocity();
            characterTileSprite.IncrementPositions();

            int characterPixelMinimumX = characterTileSprite.PixelPositionX;
            int characterPixelMinimumY = characterTileSprite.PixelPositionY;

            TileSprite.Geometry characterPixelGeometry = new TileSprite.Geometry(
                characterPixelMinimumX,
                characterPixelMinimumY,
                characterPixelMinimumX + ( characterTileSprite.PixelWidth * characterTileSprite.Width ),
                characterPixelMinimumY + ( characterTileSprite.PixelHeight * characterTileSprite.Height )
            );

            for (int j = 0; j < ObstacleTileSpriteCount; ++j)
            {
                AgentTileSprite obstacleTileSprite  = ObstacleTileSpriteList[j];
                bool isUpdated                      = false;

                for (int k = 0; k < UpdatedObstacleList.Count; ++k)
                {
                    if ( UpdatedObstacleList[k].GetInstanceID() == obstacleTileSprite.GetInstanceID() )
                    {
                        isUpdated = true;
                        break;
                    }
                }

                if (!isUpdated)
                {
                    obstacleTileSprite.ComputePixelGeometry();
                    obstacleTileSprite.ComputeVelocity();
                    obstacleTileSprite.IncrementPositions();
                    obstacleTileSprite.UpdatePosition();
                    obstacleTileSprite.ComputePixelGeometry();

                    UpdatedObstacleList.Add(obstacleTileSprite);
                }

                TileSprite.Geometry obstaclePixelGeometry = obstacleTileSprite.PixelGeometry;

                int obstacleMinimumX = obstaclePixelGeometry.MinimumX;
                int obstacleMaximumX = obstaclePixelGeometry.MaximumX;

                int obstacleMinimumY = obstaclePixelGeometry.MinimumY;
                int obstacleMaximumY = obstaclePixelGeometry.MaximumY;

                int characterPixelMaximumX  = characterPixelGeometry.MaximumX;
                int characterPixelMaximumY  = characterPixelGeometry.MaximumY;

                if (
                    obstacleMinimumX >= characterPixelMaximumX  ||
                    characterPixelMinimumX >= obstacleMaximumX  ||
                    obstacleMinimumY >= characterPixelMaximumY ||
                    characterPixelMinimumY >= obstacleMaximumY
                )
                {
                    continue;
                }

                int[] pixelDifferences = new int[4]
                {
                    Mathf.Abs(obstacleMinimumX - characterPixelMaximumX),
                    Mathf.Abs(obstacleMaximumX - characterPixelMinimumX),
                    Mathf.Abs(obstacleMinimumY - characterPixelMaximumY),
                    Mathf.Abs(obstacleMaximumY - characterPixelMinimumY)
                };

                int minimumPixelDifference = pixelDifferences.Min();

                if (minimumPixelDifference == pixelDifferences[0])
                {
                    characterTileSprite.PixelPositionX -= minimumPixelDifference;

                    if (characterTileSprite.PixelRemainderX > 0.0f)
                    {
                        characterTileSprite.PixelRemainderX = 0.0f;
                    }
                }
                else if (minimumPixelDifference == pixelDifferences[1])
                {
                    characterTileSprite.PixelPositionX += minimumPixelDifference;

                    if (characterTileSprite.PixelRemainderX < 0.0f)
                    {
                        characterTileSprite.PixelRemainderX = 0.0f;
                    }
                }
                else if (minimumPixelDifference == pixelDifferences[2])
                {
                    characterTileSprite.PixelPositionY -= minimumPixelDifference;

                    if (characterTileSprite.PixelRemainderY > 0.0f)
                    {
                        characterTileSprite.PixelRemainderY = 0.0f;
                    }
                }
                else if (minimumPixelDifference == pixelDifferences[3])
                {
                    characterTileSprite.PixelPositionY += minimumPixelDifference;

                    if (characterTileSprite.PixelRemainderY < 0.0f)
                    {
                        characterTileSprite.PixelRemainderY = 0.0f;
                    }
                }
            }

            characterTileSprite.UpdatePosition();
        }
    }
};