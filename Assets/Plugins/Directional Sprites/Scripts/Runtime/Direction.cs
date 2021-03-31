using System;
using System.Collections.Generic;
using UnityEngine;

namespace Com.DiazTeo.DirectionalSprite
{
    [Serializable]
    public class Direction
    {

        public float angleStart;
        public float angleEnd;
        public int order;
        public List<Sprite> sprites;
        public bool invertSprites;

        public float AngleEnd
        {
            get => angleEnd; 
            set
            {
                angleEnd = Mathf.Clamp(value ,-180,180);
            }
        }
        public float AngleStart
        {
            get => angleStart; 
            set
            {
                angleStart = Mathf.Clamp(value, -180, 180);
            }
        }

        public List<Sprite> GetSpriteList()
        {
            return sprites;
        }

        public Sprite GetSprite(int index)
        {
            return sprites[index];
        }
    }
}
