using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sample
{
    public class GhostScript : MonoBehaviour
    {
        private Animator Anim;

        // Cache hash values
        private static readonly int IdleState = Animator.StringToHash("Base Layer.idle");
        private static readonly int MoveState = Animator.StringToHash("Base Layer.move");
        private static readonly int SurprisedState = Animator.StringToHash("Base Layer.surprised");
        private static readonly int AttackState = Animator.StringToHash("Base Layer.attack_shift");
        private static readonly int DissolveState = Animator.StringToHash("Base Layer.dissolve");
        private static readonly int AttackTag = Animator.StringToHash("Attack");

        void Start()
        {
            Anim = this.GetComponent<Animator>();
        }

        void Update()
        {
            UpdateStatus();
            HandleAttackInput();
        }

        // 상태 갱신 (애니메이션 태그/상태)
        private void UpdateStatus()
        {
            // 공격 상태
            if (Anim.GetCurrentAnimatorStateInfo(0).tagHash == AttackTag)
            {
                // 필요하면 공격 상태 처리 가능
            }

            // Surprised 상태
            if (Anim.GetCurrentAnimatorStateInfo(0).fullPathHash == SurprisedState)
            {
                // 필요하면 Surprised 상태 처리 가능
            }

            // Dissolve 상태
            if (Anim.GetCurrentAnimatorStateInfo(0).fullPathHash == DissolveState)
            {
                // 필요하면 Dissolve 상태 처리 가능
            }
        }

        // 공격 입력 처리
        private void HandleAttackInput()
        {
            if (Input.GetKeyDown(KeyCode.A))
            {
                Anim.CrossFade(AttackState, 0.1f, 0, 0);
            }
        }
    }
}
