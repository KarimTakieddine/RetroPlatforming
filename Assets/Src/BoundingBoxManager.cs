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

public class BoundingBoxManager : MonoBehaviour
{
    public static BoundingBoxManager Instance { get; private set; }

    private List<BoundingBox> m_BoundingBoxInstances;

    public List<BoundingBox> BoundingBoxInstances
    {
        get
        {
            if (m_BoundingBoxInstances == null)
            {
                m_BoundingBoxInstances = new List<BoundingBox>();
            }

            return m_BoundingBoxInstances;
        }
    }

    private List<ActiveBoundingBox> m_ObstacleBoundingBoxInstances;

    public List<ActiveBoundingBox> ObstacleBoundingBoxInstances
    {
        get
        {
            if (m_ObstacleBoundingBoxInstances == null)
            {
                m_ObstacleBoundingBoxInstances = new List<ActiveBoundingBox>();
            }

            return m_ObstacleBoundingBoxInstances;
        }
    }

    private static List<ActiveBoundingBox> m_ControllerBoundingBoxInstances;

    public static List<ActiveBoundingBox> ControllerBoundingBoxInstances
    {
        get
        {
            if (m_ControllerBoundingBoxInstances == null)
            {
                m_ControllerBoundingBoxInstances = new List<ActiveBoundingBox>();
            }

            return m_ControllerBoundingBoxInstances;
        }
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (!Instance != this)
        {
            Destroy(this);
        }
    }

    private void LoadBoundingBoxInstances()
    {
        GameObject[] allGameObjects = FindObjectsOfType<GameObject>();
        int gameObjectCount         = allGameObjects.Length;

        if (gameObjectCount == 1)
        {
            return;
        }

        if (BoundingBoxInstances.Count > 0)
        {
            BoundingBoxInstances.Clear();
        }

        BoundingBoxInstances.Capacity = gameObjectCount;
        
        for (int i = 0; i < gameObjectCount; ++i)
        {
            BoundingBox boundingBoxComponent = allGameObjects[i].GetComponent<BoundingBox>();

            if (!boundingBoxComponent)
            {
                continue;
            }

            BoundingBoxInstances.Add(boundingBoxComponent);
        }
    }

    private void SortBoundingBoxInstances()
    {
        int boundingBoxCount = BoundingBoxInstances.Count;

        if (boundingBoxCount == 0)
        {
            return;
        }

        if (ObstacleBoundingBoxInstances.Count != 0)
        {
            ObstacleBoundingBoxInstances.Clear();
        }

        if (ControllerBoundingBoxInstances.Count != 0)
        {
            ControllerBoundingBoxInstances.Clear();
        }

        for (int i = 0; i < boundingBoxCount; ++i)
        {
            BoundingBox boundingBoxInstance             = BoundingBoxInstances[i];
            BoundingBox.BehaviourFlags behaviourFlags   = boundingBoxInstance.BehaviourFlag;

            if ( ( behaviourFlags & BoundingBox.BehaviourFlags.COLLIDER ) == 0 )
            {
                continue;
            }

            if ( (behaviourFlags & BoundingBox.BehaviourFlags.CONTROLLER ) == BoundingBox.BehaviourFlags.CONTROLLER )
            {
                ControllerBoundingBoxInstances.Add( (ActiveBoundingBox)boundingBoxInstance );
            }
            else
            {
                ObstacleBoundingBoxInstances.Add( (ActiveBoundingBox)boundingBoxInstance );
            }
        }
    }

    private void MoveBoundingBoxInstances()
    {
        for (int i = 0; i < ObstacleBoundingBoxInstances.Count; ++i)
        {
            ActiveBoundingBox obstacleBoundingBoxInstance = ObstacleBoundingBoxInstances[i];

            obstacleBoundingBoxInstance.Awake();
            obstacleBoundingBoxInstance.ComputeVelocity();
            obstacleBoundingBoxInstance.IncrementPositions();
            obstacleBoundingBoxInstance.SetPixelPosition(obstacleBoundingBoxInstance.PixelPositionX, obstacleBoundingBoxInstance.PixelPositionY);
            obstacleBoundingBoxInstance.DrawGeometry();
        }

        for (int i = 0; i < ControllerBoundingBoxInstances.Count; ++i)
        {
            ActiveBoundingBox controllerBoundingBoxInstance = ControllerBoundingBoxInstances[i];

            controllerBoundingBoxInstance.Awake();
            controllerBoundingBoxInstance.ComputeVelocity();
            controllerBoundingBoxInstance.IncrementPositions();

            int controllerPixelMinimumX = controllerBoundingBoxInstance.PixelPositionX;
            int controllerPixelMaximumX = controllerPixelMinimumX + (int)( controllerBoundingBoxInstance.Width * controllerBoundingBoxInstance.PixelsPerUnit );

            int controllerPixelMinimumY = controllerBoundingBoxInstance.PixelPositionY;
            int controllerPixelMaximumY = controllerPixelMinimumY + (int)(controllerBoundingBoxInstance.Height * controllerBoundingBoxInstance.PixelsPerUnit );

            for (int j = 0; j < ObstacleBoundingBoxInstances.Count; ++j)
            {
                ActiveBoundingBox obstacleBoundingBoxInstance   = ObstacleBoundingBoxInstances[j];
                BoundingBox.Geometry obstaclePixelGeometry      = obstacleBoundingBoxInstance.PixelGeometry;

                int obstaclePixelMinimumX = obstaclePixelGeometry.MinimumX;
                int obstaclePixelMinimumY = obstaclePixelGeometry.MinimumY;

                int obstaclePixelMaximumX = obstaclePixelGeometry.MaximumX;
                int obstaclePixelMaximumY = obstaclePixelGeometry.MaximumY;

                if (
                    obstaclePixelMinimumX > controllerPixelMaximumX ||
                    obstaclePixelMinimumY > controllerPixelMaximumY ||
                    controllerPixelMinimumX > obstaclePixelMaximumX ||
                    controllerPixelMinimumY > obstaclePixelMaximumY
                )
                {
                    controllerBoundingBoxInstance.OnBoundingBoxExit(obstacleBoundingBoxInstance);

                    obstacleBoundingBoxInstance.OnBoundingBoxExit(controllerBoundingBoxInstance);

                    continue;
                }
                
                int[] pixelPenetrationList = new int[4]
                {
                    Mathf.Abs(controllerPixelMaximumX - obstaclePixelMinimumX),
                    Mathf.Abs(controllerPixelMaximumY - obstaclePixelMinimumY),
                    Mathf.Abs(obstaclePixelMaximumX - controllerPixelMinimumX),
                    Mathf.Abs(obstaclePixelMaximumY - controllerPixelMinimumY)
                };

                int minimumPixelPenetration                                     = pixelPenetrationList.Min();
                ActiveBoundingBox.CollisionStateFlags controllerCollisionState  = ActiveBoundingBox.CollisionStateFlags.NONE;
                ActiveBoundingBox.CollisionStateFlags obstacleCollisionState    = ActiveBoundingBox.CollisionStateFlags.NONE;

                if (minimumPixelPenetration == pixelPenetrationList[0])
                {
                    controllerCollisionState    |= ActiveBoundingBox.CollisionStateFlags.RIGHT_WALL;
                    obstacleCollisionState      |= ActiveBoundingBox.CollisionStateFlags.LEFT_WALL;

                    if ( ( obstacleBoundingBoxInstance.BehaviourFlag & BoundingBox.BehaviourFlags.SLOPE ) == BoundingBox.BehaviourFlags.SLOPE )
                    {
                        SlopeObstacleBoundingBox slopeBoundingBoxInstance   = (SlopeObstacleBoundingBox)obstacleBoundingBoxInstance;
                        List<List<bool>> slopeTextureBitmap                 = slopeBoundingBoxInstance.TextureBitmap;
                        int bitmapRowIndex                                  = Mathf.Abs(controllerPixelMinimumY - obstaclePixelMinimumY);

                        List<bool> bitmapRow            = slopeTextureBitmap[bitmapRowIndex];
                        int pixelPenetrationAllowance   = 0;

                        for (int l = 0; l < bitmapRow.Count; ++l)
                        {
                            bool bitmapValue = bitmapRow[l];

                            if (bitmapValue)
                            {
                                break;
                            }

                            ++pixelPenetrationAllowance;
                        }

                        int pixelPenetrationDifference = minimumPixelPenetration - pixelPenetrationAllowance;

                        if (pixelPenetrationDifference != 0)
                        {
                            controllerPixelMinimumX -= pixelPenetrationDifference;
                            
                            if (controllerBoundingBoxInstance.VelocityX > 0.0f)
                            {
                                ++controllerPixelMinimumX;
                                ++controllerPixelMinimumY;
                            }
                            else if (controllerBoundingBoxInstance.VelocityX < 0.0f)
                            {
                                --controllerPixelMinimumX;
                                --controllerPixelMinimumY;
                            }
                        }
                    }
                    else
                    {
                        controllerPixelMinimumX -= minimumPixelPenetration;
                    }

                    controllerBoundingBoxInstance.RoundingRemainderX = 0.0f;
                }
                else if (minimumPixelPenetration == pixelPenetrationList[1])
                {
                    controllerCollisionState                        |= ActiveBoundingBox.CollisionStateFlags.CEILING;
                    obstacleCollisionState                          |= ActiveBoundingBox.CollisionStateFlags.GROUND;
                    controllerPixelMinimumY                         -= minimumPixelPenetration;
                    controllerBoundingBoxInstance.RoundingRemainderY = 0.0f;
                }
                else if (minimumPixelPenetration == pixelPenetrationList[2])
                {
                    controllerCollisionState                        |= ActiveBoundingBox.CollisionStateFlags.LEFT_WALL;
                    obstacleCollisionState                          |= ActiveBoundingBox.CollisionStateFlags.RIGHT_WALL;
                    controllerPixelMinimumX                         += minimumPixelPenetration;
                    controllerBoundingBoxInstance.RoundingRemainderX = 0.0f;
                }
                else if (minimumPixelPenetration == pixelPenetrationList[3])
                {
                    controllerCollisionState    |= ActiveBoundingBox.CollisionStateFlags.GROUND;
                    obstacleCollisionState      |= ActiveBoundingBox.CollisionStateFlags.CEILING;
                    
                    if ( ( obstacleBoundingBoxInstance.BehaviourFlag & BoundingBox.BehaviourFlags.SLOPE ) == BoundingBox.BehaviourFlags.SLOPE )
                    {
                        SlopeObstacleBoundingBox slopeBoundingBoxInstance   = (SlopeObstacleBoundingBox)obstacleBoundingBoxInstance;
                        List<List<bool>> slopeTextureBitmap                 = slopeBoundingBoxInstance.TextureBitmap;
                        int bitmapColumnIndex                               = Mathf.Abs(controllerPixelMaximumX - obstaclePixelMinimumX - 1);
                        int bitmapRowCount                                  = slopeTextureBitmap.Count;
                        int pixelPenetrationAllowance                       = 0;

                        for (int l = bitmapRowCount - 1; l >= 0; --l)
                        {
                            List<bool> bitmapRow = slopeTextureBitmap[l];

                            if (bitmapColumnIndex > bitmapRow.Count - 1)
                            {
                                break;
                            }

                            if (bitmapRow[bitmapColumnIndex])
                            {
                                break;
                            }

                            ++pixelPenetrationAllowance;
                        }

                        int pixelPenetrationDifference = minimumPixelPenetration - pixelPenetrationAllowance;
                        
                        if (pixelPenetrationDifference != 0)
                        {
                            controllerPixelMinimumY += pixelPenetrationDifference;
                        }
                    }
                    else
                    {
                        controllerPixelMinimumY += minimumPixelPenetration;
                    }

                    controllerBoundingBoxInstance.RoundingRemainderY = 0.0f;
                }

                controllerBoundingBoxInstance.OnBoundingBoxEnter(controllerCollisionState, minimumPixelPenetration, obstacleBoundingBoxInstance);
                obstacleBoundingBoxInstance.OnBoundingBoxEnter(obstacleCollisionState, minimumPixelPenetration, controllerBoundingBoxInstance);
            }

            controllerBoundingBoxInstance.SetPixelPosition(controllerPixelMinimumX, controllerPixelMinimumY);
            controllerBoundingBoxInstance.DrawGeometry();
        }
    }
    
    void Update ()
    {
        LoadBoundingBoxInstances();
        SortBoundingBoxInstances();
        MoveBoundingBoxInstances();
    }
};