using UnityEngine;

namespace Com.DiazTeo.DirectionalSprite
{
    public class DirectionalSpriteAnimator : DirectionalSpriteBase
    {
        [SerializeField]
        protected Animator animator;
        public void SetTrigger(string trigger)
        {
            animator.SetTrigger(trigger);
        }

        protected override void ChangeDirection()
        {
            animator.SetFloat("angle", angle);
        }
    }
}
