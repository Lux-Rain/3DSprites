using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Com.DiazTeo.DirectionalSprite
{
    [CreateAssetMenu(fileName = "DirectionalBillboard", menuName = "Billboard/DirectionalBillboard", order = 1)]
    public class DirectionalBillboard : ScriptableObject
    {
        public List<Direction> directions = new List<Direction>();
        public float waitTime = 0.25f;
        public bool loop;
        public bool resetFrameWhenChangeDirection;

        public List<Sprite> GetSpritesList(float angle)
        {
            int index;
            if (CheckDirectionList(angle, out index))
            {
                return directions[index].sprites;
            }
            return null;
        }

        public bool CheckDirectionList(float angle, out int index)
        {
            Direction direction;
            for (int i = directions.Count - 1; i >= 0; i--)
            {
                direction = directions[i];
                if (direction.AngleStart <= angle && direction.AngleEnd >= angle)
                {
                    index = i;
                    return true;
                }

            }
            index = 0;
            return false;
        }

        public Direction GetDirection(float angle)
        {
            Direction direction;
            for (int i = directions.Count - 1; i >= 0; i--)
            {
                direction = directions[i];
                if (direction.AngleStart <= angle && direction.AngleEnd >= angle)
                {
                    return direction;
                }

            }
            return null;
        }

        public float GetNewAngleStart(Direction direction, float newAngle)
        {
            for (int i = directions.Count - 1; i >= 0; i--)
            {
                if (directions[i] == direction)
                {
                    continue;
                }
                if (direction.angleEnd >= directions[i].angleEnd)
                {
                    if (newAngle < directions[i].angleEnd)
                    {
                        newAngle = directions[i].AngleEnd;
                        break;
                    }
                } else if (direction.angleEnd <= directions[i].angleStart)
                {
                    if (newAngle > directions[i].angleStart)
                    {
                        newAngle = directions[i].angleStart;
                        break;
                    }
                }
                
            }
            return newAngle;
        }

        public float GetNewAngleEnd(Direction direction, float newAngle)
        {
            for (int i = directions.Count - 1; i >= 0; i--)
            {
                if (directions[i] == direction)
                {
                    continue;
                }

                if (direction.angleStart >= directions[i].angleEnd)
                {
                    if (newAngle < directions[i].angleEnd)
                    {
                        newAngle = directions[i].AngleEnd;
                        break;
                    }
                }
                else if (direction.angleStart <= directions[i].angleStart)
                {
                    if (newAngle > directions[i].angleStart)
                    {
                        newAngle = directions[i].angleStart;
                        break;
                    }
                }
            }
            return newAngle;
        }
    }
}
