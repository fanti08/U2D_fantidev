using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;

public class SpineToMecanim : StateMachineBehaviour
{
    public AnimationReferenceAsset animation;
    private SkeletonAnimation spineAnimation;
    private SkeletonAnimation GetSpineAnim(Animator animator)
    {
        if (!spineAnimation)
            spineAnimation = animator.GetComponent<SkeletonAnimation>();
        return spineAnimation;
    }
    public override void OnStateEnter(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
    {
        Debug.Log(GetSpineAnim(animator));
        if (GetSpineAnim(animator))
        {
            GetSpineAnim(animator).state.SetAnimation(layerIndex, animation, animatorStateInfo.loop);
        }        
    }
}
