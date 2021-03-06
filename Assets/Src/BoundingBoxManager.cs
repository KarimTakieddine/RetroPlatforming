﻿/* ############################################################################## *\

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
    public class CollisionEvent
    {
        public ActiveBoundingBox Other                              { get; set; }
        public ActiveBoundingBox.CollisionStateFlags CollisionState { get; set; } 
        public int PenetrationDepth                                 { get; set; }

        public CollisionEvent(int penetrationDepth, ActiveBoundingBox.CollisionStateFlags collisionState, ActiveBoundingBox other)
        {
            Other               = other;
            CollisionState      = collisionState;
            PenetrationDepth    = penetrationDepth;
        }

        public override bool Equals(System.Object other)
        {
            CollisionEvent collisionEvent = other as CollisionEvent;

            return Other.GetInstanceID() == collisionEvent.Other.GetInstanceID();
        }

        public override int GetHashCode()
        {
            return Other.GetInstanceID();
        }
    }

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

    private static List<CollisionEvent> m_CollisionEventHistory;

    public static List<CollisionEvent> CollisionEventHistory
    {
        get
        {
            if (m_CollisionEventHistory == null)
            {
                m_CollisionEventHistory = new List<CollisionEvent>();
            }

            return m_CollisionEventHistory;
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
                    obstaclePixelMinimumX >= controllerPixelMaximumX ||
                    obstaclePixelMinimumY >= controllerPixelMaximumY ||
                    controllerPixelMinimumX > obstaclePixelMaximumX ||
                    controllerPixelMinimumY > obstaclePixelMaximumY
                )
                {
                    CollisionEvent exitCollisionEvent = new CollisionEvent(0, ActiveBoundingBox.CollisionStateFlags.NONE, obstacleBoundingBoxInstance);

                    if  ( !CollisionEventHistory.Contains(exitCollisionEvent) ||
                        ( ( obstacleBoundingBoxInstance.BehaviourFlag & BoundingBox.BehaviourFlags.SLOPE ) == BoundingBox.BehaviourFlags.SLOPE )
                    )
                    {
                        continue;
                    }

                    controllerBoundingBoxInstance.OnBoundingBoxExit(obstacleBoundingBoxInstance);
                    obstacleBoundingBoxInstance.OnBoundingBoxExit(controllerBoundingBoxInstance);

                    CollisionEventHistory.Remove(exitCollisionEvent);

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
                    controllerCollisionState                            |= ActiveBoundingBox.CollisionStateFlags.RIGHT_WALL;
                    obstacleCollisionState                              |= ActiveBoundingBox.CollisionStateFlags.LEFT_WALL;

                    if ( ( obstacleBoundingBoxInstance.BehaviourFlag & BoundingBox.BehaviourFlags.SLOPE ) == BoundingBox.BehaviourFlags.SLOPE )
                    {
                        SlopeObstacleBoundingBox slopeObstacleBoundingBoxInstance = obstacleBoundingBoxInstance as SlopeObstacleBoundingBox;
                        List<List<bool>> slopeTextureBitmap = slopeObstacleBoundingBoxInstance.TextureBitmap;

                        if (controllerBoundingBoxInstance.VelocityY < 0.0f)
                        {
                            int bitmapColumnIndex = controllerPixelMaximumX - obstaclePixelMinimumX - 1;

                            int pixelPenetrationY = obstaclePixelMaximumY - controllerPixelMinimumY;
                            int pixelPenetrationAllowanceY = 0;

                            for (int k = slopeTextureBitmap.Count - 1; k > 0; --k)
                            {
                                List<bool> bitmapRow = slopeTextureBitmap[k];

                                if (
                                    bitmapColumnIndex < 0 ||
                                    bitmapColumnIndex >= bitmapRow.Count
                                )
                                {
                                    break;
                                }

                                if (bitmapRow[bitmapColumnIndex])
                                {
                                    break;
                                }

                                ++pixelPenetrationAllowanceY;
                            }

                            int pixelPenetrationDifferenceY = pixelPenetrationY - pixelPenetrationAllowanceY;
                            
                            if (pixelPenetrationDifferenceY > 0)
                            {
                                CollisionEvent slopeEnterCollisionEvent = new CollisionEvent(
                                    pixelPenetrationDifferenceY,
                                    ActiveBoundingBox.CollisionStateFlags.GROUND,
                                    obstacleBoundingBoxInstance
                                );

                                if (!CollisionEventHistory.Contains(slopeEnterCollisionEvent))
                                {
                                    controllerBoundingBoxInstance.OnBoundingBoxEnter(ActiveBoundingBox.CollisionStateFlags.GROUND, pixelPenetrationDifferenceY, obstacleBoundingBoxInstance);
                                    obstacleBoundingBoxInstance.OnBoundingBoxEnter(obstacleCollisionState, pixelPenetrationDifferenceY, controllerBoundingBoxInstance);

                                    CollisionEventHistory.Add(slopeEnterCollisionEvent);
                                }

                                controllerPixelMinimumY += pixelPenetrationDifferenceY;
                            }
                            else
                            {
                                CollisionEvent slopeExitCollisionEvent = new CollisionEvent(
                                    0,
                                    ActiveBoundingBox.CollisionStateFlags.NONE,
                                    obstacleBoundingBoxInstance
                                );

                                if (CollisionEventHistory.Contains(slopeExitCollisionEvent))
                                {
                                    controllerBoundingBoxInstance.OnBoundingBoxExit(obstacleBoundingBoxInstance);
                                    obstacleBoundingBoxInstance.OnBoundingBoxExit(controllerBoundingBoxInstance);

                                    CollisionEventHistory.Remove(slopeExitCollisionEvent);
                                }
                            }
                        }
                        else
                        {
                            int pixelPenetrationX = controllerPixelMaximumX - obstaclePixelMinimumX;

                            if (controllerBoundingBoxInstance.VelocityX < 0.0f)
                            {
                                int bitmapColumnIndex = pixelPenetrationX - 1;

                                int pixelPenetrationY = obstaclePixelMaximumY - controllerPixelMinimumY;
                                int pixelPenetrationAllowanceY = 0;

                                for (int k = slopeTextureBitmap.Count - 1; k > 0; --k)
                                {
                                    List<bool> bitmapRow = slopeTextureBitmap[k];

                                    if (
                                        bitmapColumnIndex < 0 ||
                                        bitmapColumnIndex >= bitmapRow.Count
                                    )
                                    {
                                        break;
                                    }

                                    if (bitmapRow[bitmapColumnIndex])
                                    {
                                        break;
                                    }

                                    ++pixelPenetrationAllowanceY;
                                }

                                int pixelPenetrationDifferenceY = pixelPenetrationY - pixelPenetrationAllowanceY;

                                if (pixelPenetrationDifferenceY < 0)
                                {
                                    controllerPixelMinimumY += pixelPenetrationDifferenceY;

                                    if (bitmapColumnIndex == 0)
                                    {
                                        --controllerPixelMinimumY;
                                    }
                                }
                            }
                            else
                            {
                                int bitmapRowCount = slopeTextureBitmap.Count;
                                int bitmapRowIndex = controllerPixelMinimumY - obstaclePixelMinimumY;

                                if (
                                    bitmapRowIndex >= 0 &&
                                    bitmapRowIndex < bitmapRowCount
                                )
                                {
                                    List<bool> bitmapRow = slopeTextureBitmap[bitmapRowIndex];
                                    int pixelPenetrationAllowanceX = 0;

                                    for (int k = 0; k < bitmapRow.Count; ++k)
                                    {
                                        if (bitmapRow[k])
                                        {
                                            break;
                                        }

                                        ++pixelPenetrationAllowanceX;
                                    }

                                    int pixelPenetrationDifferenceX = pixelPenetrationX - pixelPenetrationAllowanceX;

                                    if (pixelPenetrationDifferenceX > 0)
                                    {
                                        CollisionEvent slopeEnterCollisionEvent = new CollisionEvent(
                                            pixelPenetrationDifferenceX,
                                            ActiveBoundingBox.CollisionStateFlags.GROUND,
                                            obstacleBoundingBoxInstance
                                        );

                                        if (!CollisionEventHistory.Contains(slopeEnterCollisionEvent))
                                        {
                                            controllerBoundingBoxInstance.OnBoundingBoxEnter(controllerCollisionState, pixelPenetrationDifferenceX, obstacleBoundingBoxInstance);
                                            obstacleBoundingBoxInstance.OnBoundingBoxEnter(obstacleCollisionState, pixelPenetrationDifferenceX, controllerBoundingBoxInstance);

                                            CollisionEventHistory.Add(slopeEnterCollisionEvent);
                                        }

                                        controllerPixelMinimumX -= pixelPenetrationDifferenceX;
                                        controllerBoundingBoxInstance.RoundingRemainderX = 0.0f;

                                        if (
                                            controllerBoundingBoxInstance.VelocityX > 0 &&
                                            controllerBoundingBoxInstance.VelocityY == 0.0f
                                        )
                                        {
                                            ++controllerPixelMinimumY;
                                            ++controllerPixelMinimumX;
                                        }
                                    }
                                    else
                                    {
                                        CollisionEvent slopeExitCollisionEvent = new CollisionEvent(
                                            0,
                                            ActiveBoundingBox.CollisionStateFlags.NONE,
                                            obstacleBoundingBoxInstance
                                        );

                                        if (CollisionEventHistory.Contains(slopeExitCollisionEvent))
                                        {
                                            controllerBoundingBoxInstance.OnBoundingBoxExit(obstacleBoundingBoxInstance);
                                            obstacleBoundingBoxInstance.OnBoundingBoxExit(controllerBoundingBoxInstance);

                                            CollisionEventHistory.Remove(slopeExitCollisionEvent);
                                        }
                                    }
                                }
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
                    controllerCollisionState                            |= ActiveBoundingBox.CollisionStateFlags.CEILING;
                    obstacleCollisionState                              |= ActiveBoundingBox.CollisionStateFlags.GROUND;
                    controllerPixelMinimumY                             -= minimumPixelPenetration;
                    controllerBoundingBoxInstance.RoundingRemainderY    = 0.0f;
                }
                else if (minimumPixelPenetration == pixelPenetrationList[2])
                {
                    controllerCollisionState                            |= ActiveBoundingBox.CollisionStateFlags.LEFT_WALL;
                    obstacleCollisionState                              |= ActiveBoundingBox.CollisionStateFlags.RIGHT_WALL;
                    controllerPixelMinimumX                             += minimumPixelPenetration;
                    controllerBoundingBoxInstance.RoundingRemainderX    = 0.0f;
                }
                else if (minimumPixelPenetration == pixelPenetrationList[3])
                {
                    controllerCollisionState                            |= ActiveBoundingBox.CollisionStateFlags.GROUND;
                    obstacleCollisionState                              |= ActiveBoundingBox.CollisionStateFlags.CEILING;

                    if ( ( obstacleBoundingBoxInstance.BehaviourFlag & BoundingBox.BehaviourFlags.SLOPE ) == BoundingBox.BehaviourFlags.SLOPE )
                    {
                        SlopeObstacleBoundingBox slopeObstacleBoundingBoxInstance = obstacleBoundingBoxInstance as SlopeObstacleBoundingBox;
                        List<List<bool>> slopeTextureBitmap = slopeObstacleBoundingBoxInstance.TextureBitmap;

                        if (controllerBoundingBoxInstance.VelocityX > 0.0f)
                        {
                            int bitmapRowCount = slopeTextureBitmap.Count;
                            int bitmapRowIndex = controllerPixelMinimumY - obstaclePixelMinimumY;

                            if (
                                bitmapRowIndex >= 0 &&
                                bitmapRowIndex < bitmapRowCount
                            )
                            {
                                int pixelPenetrationX = controllerPixelMaximumX - obstaclePixelMinimumX;

                                List<bool> bitmapRow = slopeTextureBitmap[bitmapRowIndex];
                                int pixelPenetrationAllowanceX = 0;

                                for (int k = 0; k < bitmapRow.Count; ++k)
                                {
                                    if (bitmapRow[k])
                                    {
                                        break;
                                    }

                                    ++pixelPenetrationAllowanceX;
                                }

                                int pixelPenetrationDifferenceX = pixelPenetrationX - pixelPenetrationAllowanceX;

                                if (pixelPenetrationDifferenceX > 0)
                                {
                                    CollisionEvent slopeEnterCollisionEvent = new CollisionEvent(
                                        pixelPenetrationDifferenceX,
                                        obstacleCollisionState,
                                        obstacleBoundingBoxInstance
                                    );

                                    if (!CollisionEventHistory.Contains(slopeEnterCollisionEvent))
                                    {
                                        controllerBoundingBoxInstance.OnBoundingBoxEnter(controllerCollisionState, pixelPenetrationDifferenceX, obstacleBoundingBoxInstance);
                                        obstacleBoundingBoxInstance.OnBoundingBoxEnter(obstacleCollisionState, pixelPenetrationDifferenceX, controllerBoundingBoxInstance);

                                        CollisionEventHistory.Add(slopeEnterCollisionEvent);
                                    }

                                    controllerPixelMinimumX -= pixelPenetrationDifferenceX;
                                    controllerBoundingBoxInstance.RoundingRemainderX = 0.0f;

                                    if (
                                        controllerBoundingBoxInstance.VelocityX > 0 &&
                                        controllerBoundingBoxInstance.VelocityY == 0.0f
                                    )
                                    {
                                        ++controllerPixelMinimumY;
                                        ++controllerPixelMinimumX;
                                    }
                                }
                                else
                                {
                                    CollisionEvent slopeExitCollisionEvent = new CollisionEvent(
                                        0,
                                        ActiveBoundingBox.CollisionStateFlags.NONE,
                                        obstacleBoundingBoxInstance
                                    );

                                    if (CollisionEventHistory.Contains(slopeExitCollisionEvent))
                                    {
                                        controllerBoundingBoxInstance.OnBoundingBoxExit(obstacleBoundingBoxInstance);
                                        obstacleBoundingBoxInstance.OnBoundingBoxExit(controllerBoundingBoxInstance);

                                        CollisionEventHistory.Remove(slopeExitCollisionEvent);
                                    }
                                }
                            }
                        }
                        else
                        {
                            int bitmapColumnIndex = controllerPixelMaximumX - obstaclePixelMinimumX - 1;

                            int pixelPenetrationY = obstaclePixelMaximumY - controllerPixelMinimumY;
                            int pixelPenetrationAllowanceY = 0;

                            for (int k = slopeTextureBitmap.Count - 1; k > 0; --k)
                            {
                                List<bool> bitmapRow = slopeTextureBitmap[k];

                                if (
                                    bitmapColumnIndex < 0 ||
                                    bitmapColumnIndex >= bitmapRow.Count
                                )
                                {
                                    break;
                                }

                                if (bitmapRow[bitmapColumnIndex])
                                {
                                    break;
                                }

                                ++pixelPenetrationAllowanceY;
                            }

                            int pixelPenetrationDifferenceY = pixelPenetrationY - pixelPenetrationAllowanceY;

                            if (
                                controllerBoundingBoxInstance.VelocityX < 0.0f &&
                                controllerBoundingBoxInstance.VelocityY == 0.0f
                            )
                            {
                                if (pixelPenetrationDifferenceY < 0)
                                {
                                    controllerPixelMinimumY += pixelPenetrationDifferenceY;
                                }
                            }
                            else
                            {
                                if (pixelPenetrationDifferenceY > 0)
                                {
                                    CollisionEvent slopeEnterCollisionEvent = new CollisionEvent(
                                        pixelPenetrationDifferenceY,
                                        obstacleCollisionState,
                                        obstacleBoundingBoxInstance
                                    );

                                    if (!CollisionEventHistory.Contains(slopeEnterCollisionEvent))
                                    {
                                        controllerBoundingBoxInstance.OnBoundingBoxEnter(controllerCollisionState, pixelPenetrationDifferenceY, obstacleBoundingBoxInstance);
                                        obstacleBoundingBoxInstance.OnBoundingBoxEnter(obstacleCollisionState, pixelPenetrationDifferenceY, controllerBoundingBoxInstance);

                                        CollisionEventHistory.Add(slopeEnterCollisionEvent);
                                    }

                                    controllerPixelMinimumY += pixelPenetrationDifferenceY;
                                }
                                else
                                {
                                    CollisionEvent slopeExitCollisionEvent = new CollisionEvent(
                                        0,
                                        ActiveBoundingBox.CollisionStateFlags.NONE,
                                        obstacleBoundingBoxInstance
                                    );

                                    if (CollisionEventHistory.Contains(slopeExitCollisionEvent))
                                    {
                                        controllerBoundingBoxInstance.OnBoundingBoxExit(obstacleBoundingBoxInstance);
                                        obstacleBoundingBoxInstance.OnBoundingBoxExit(controllerBoundingBoxInstance);

                                        CollisionEventHistory.Remove(slopeExitCollisionEvent);
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        controllerPixelMinimumY += minimumPixelPenetration;
                        controllerBoundingBoxInstance.RoundingRemainderY = 0.0f;
                    }
                }

                CollisionEvent enterCollisionEvent = new CollisionEvent(minimumPixelPenetration, controllerCollisionState, obstacleBoundingBoxInstance);

                if ( !CollisionEventHistory.Contains(enterCollisionEvent) &&
                   ( ( obstacleBoundingBoxInstance.BehaviourFlag & BoundingBox.BehaviourFlags.SLOPE) != BoundingBox.BehaviourFlags.SLOPE )
                )
                {
                    controllerBoundingBoxInstance.OnBoundingBoxEnter(controllerCollisionState, minimumPixelPenetration, obstacleBoundingBoxInstance);
                    obstacleBoundingBoxInstance.OnBoundingBoxEnter(obstacleCollisionState, minimumPixelPenetration, controllerBoundingBoxInstance);

                    CollisionEventHistory.Add(enterCollisionEvent);
                }
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