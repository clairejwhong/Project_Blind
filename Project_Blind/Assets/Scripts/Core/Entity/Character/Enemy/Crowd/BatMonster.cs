using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Blind
{
    public class BatMonster : CrowdEnemyCharacter
    {
        private Coroutine Co_attack;
        private Coroutine Co_hitted;
        private Coroutine Co_stun;
        private Coroutine Co_die;
        

        protected void Awake()
        {
            base.Awake();

            Data.sensingRange = new Vector2(12f, 8f);
            Data.speed = 0.1f ;
            Data.runSpeed = 0.07f;
            Data.attackCoolTime = 0.5f;
            Data.attackSpeed = 0.3f;
            Data.attackRange = new Vector2(9f, 8f);
            Data.stunTime = 1f;
            _patrolTime = 3f;
        }

        private void Start()
        {
            startingPosition = gameObject.transform;
            _attack.Init(7, 10);
        }

        protected override void FixedUpdate()
        {
            base.FixedUpdate();
        }

        protected override void updateAttack()
        {
            if (_anim.GetBool("Basic Attack") == false && _anim.GetBool("Skill Attack") == false)
            {
                if (Random.Range(0, 100) > 20)
                {
                    _anim.SetBool("Basic Attack", true);
                }
                else
                {
                    isPowerAttack = true;
                    _anim.SetBool("Skill Attack", true);
                }
            }
        }

        protected override void updateHitted()
        {

            if (Co_hitted == null)
            {
                StopAllCoroutines();
                StartCoroutine(CoHitted());
            }

            Vector2 hittedVelocity = Vector2.zero;
            if (playerFinder.ChasePlayer().x > 0) //플레이어가 오른쪽
            {
                hittedVelocity = new Vector2(-0.2f, 0);
            }
            else
            {
                hittedVelocity = new Vector2(0.2f, 0);
            }

            _characterController2D.Move(hittedVelocity);
        }

        protected override void updateStun()
        {
            StopAllCoroutines();
            Co_stun = StartCoroutine(CoStun());
        }

        private IEnumerator CoAttack()
        {
            yield return new WaitForSeconds(0.2f);
            _attack.EnableDamage();
            yield return new WaitForSeconds(0.5f);
            _attack.DisableDamage();

            Co_attack = null;
        }
    }
}