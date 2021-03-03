using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.DiazTeo.DirectionalSprite
{
    public class GameObjectBillboard : MonoBehaviour
    {


        protected Transform player;
        protected Direction currentDirection;
        [SerializeField]
        protected string lookAtTag = "Player";
        [SerializeField]
        protected SpriteRenderer sprite = default;
        [SerializeField]
        protected GameObject direction = default;
        [SerializeField]
        protected DirectionalBillboard billboardSettings;

        protected int currentFrame = 0;
        protected float t;

        private void Awake()
        {
            player = GameObject.FindGameObjectWithTag(lookAtTag).transform;
        }

        protected void Update()
        {
            t += Time.deltaTime;
        }

        private void LateUpdate()
        {
            Vector3 direction = player.position - transform.position;
            direction.Normalize();
            Quaternion rotation = Quaternion.LookRotation(direction, Vector3.up);
            sprite.transform.rotation = Quaternion.Euler(0, rotation.eulerAngles.y, 0);

            ChangeDirection(direction);
        }

        private void ChangeDirection(Vector3 dir)
        {
            dir.y = 0;
            float angle = Vector3.SignedAngle(this.direction.transform.forward, dir, Vector3.up);
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
