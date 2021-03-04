using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.DiazTeo.DirectionalSprite
{
    public class DirectionalSprite: DirectionalSpriteBase
    {
        [SerializeField]
        protected DirectionalBillboard billboardSettings;

        protected int currentFrame = 0;
        protected float t;

        protected void Update()
        {
            t += Time.deltaTime;
        }


        protected override void ChangeDirection()
        {
            Direction direction = billboardSettings.GetDirection(angle);
            if(direction == null)
            {
                return;
            }
            List<Sprite> sprites = direction.GetSpriteList();
            if(t >= billboardSettings.waitTime)
            {
                t = 0;
                currentFrame++;
                if(currentFrame >= sprites.Count - 1)
                {
                    if (billboardSettings.loop)
                    {
                        currentFrame = 0;
                    } else
                    {
                        currentFrame = sprites.Count -1;
                    }
                }
                sprite.flipX = direction.invertSprites;
                sprite.sprite = sprites[currentFrame];
            }
        }
    }
}
