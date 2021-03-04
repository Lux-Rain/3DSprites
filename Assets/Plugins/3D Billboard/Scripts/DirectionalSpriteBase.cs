using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.DiazTeo.DirectionalSprite
{
    public abstract class DirectionalSpriteBase : MonoBehaviour
    {
        protected Transform player;
        [SerializeField]
        protected string lookAtTag = "Player";
        [SerializeField]
        protected SpriteRenderer sprite = default;
        [SerializeField]
        protected GameObject direction = default;
        protected float angle;
        protected Vector3 vectorDirection;

        protected virtual void Awake()
        {
            player = GameObject.FindGameObjectWithTag(lookAtTag).transform;
        }

        public virtual void LateUpdate()
        {
            Look();
            GetAngle();
            ChangeDirection();
        }

        protected virtual void Look()
        {
            vectorDirection = player.position - transform.position;
            vectorDirection.Normalize();
            Quaternion rotation = Quaternion.LookRotation(vectorDirection, Vector3.up);
            sprite.transform.rotation = Quaternion.Euler(0, rotation.eulerAngles.y, 0);
        }

        protected virtual void GetAngle()
        {
            vectorDirection.y = 0;
            angle = Vector3.SignedAngle(this.direction.transform.forward, vectorDirection, Vector3.up);
        }

        protected abstract void ChangeDirection();
    }
}
