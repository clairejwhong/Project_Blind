using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Blind
{
    public class CrowdEnemyCharacter : EnemyCharacter
    {
        protected enum State
        {
            Patrol,
            Default,
            Chase,
            Attack,
            Hitted,
            Stun,
            Avoid,
            Die,
            Test
        }

        protected State state;
        protected float _patrolTime;
        protected Animator _anim;
        protected float _chaseRange = 20;

        public float CurrentStunGauge = 0;
        public float MaxStunGauge = 10f;
        public bool isPowerAttack = false;
        public bool IsAttack = false;

        private Coroutine co_patrol;
        private Coroutine co_stun;
        private Coroutine co_default;

        private State tmp = State.Die;
        protected BoxCollider2D col;
        protected bool createAttackHitBox;

        protected void Awake()
        {
            base.Awake();
            state = State.Patrol;
            patrolDirection = new Vector2(RandomDirection() * Data.speed, 0);
            playerFinder.setRange(Data.sensingRange);
            attackSense = GetComponentInChildren<EnemyAttack>();
            _anim = GetComponent<Animator>();
            player = GameObject.FindGameObjectWithTag("Player");
            //attackSense.setRange(Data.attackRange);
        }

        protected virtual void FixedUpdate()
        {
            switch (state)
            {
                case State.Patrol:
                    updatePatrol();
                    break;

                case State.Default:
                    updateDefault();
                    break;

                case State.Chase:
                    updateChase();
                    break;

                case State.Attack:
                    updateAttack();
                    break;

                case State.Hitted:
                    updateHitted();
                    break;

                case State.Stun:
                    updateStun();
                    break;

                case State.Die:
                    updateDie();
                    break;

                case State.Avoid:
                    updateAvoid();
                    break;
            }
            if (Hp.GetHP() <= 0)
                state = State.Die;
            /*if (state != tmp)
            {
                Debug.Log(state);
                tmp = state;
            }*/
            _characterController2D.OnFixedUpdate();
        }

        protected virtual void updatePatrol()
        {
            if (playerFinder.FindPlayer())
            {
                state = State.Chase;
                if (co_patrol != null) StopCoroutine(co_patrol);
                co_patrol = null;
                _anim.SetBool("Patrol", false);
                return;
            }

            if (Physics2D.OverlapCircle(WallCheck.position, 0.01f, WallLayer))
            {
                state = State.Default;
                if (co_patrol != null) StopCoroutine(co_patrol);
                co_patrol = null;
                _anim.SetBool("Patrol", false);
                return;
            }

            if (co_patrol == null)
            {
                _anim.SetBool("Patrol", true);
                co_patrol = StartCoroutine(CoPatrol(_patrolTime));
            }

            _characterController2D.Move(patrolDirection);
        }
        
        protected virtual void updateDefault()
        {
            if (playerFinder.FindPlayer())
            {
                state = State.Chase;
                _anim.SetBool("Default", false);
                return;
            }

            if (co_default == null)
            {
                co_default = StartCoroutine(CoDefault());
                _anim.SetBool("Default", true);
            }
        }
        
        protected virtual void updateChase()
        {
            if (_anim.GetBool("Chase") == false)
            {
                _anim.SetBool("Chase", true);
            }

            if (playerFinder.MissPlayer())
            {
                state = State.Patrol;
                _anim.SetBool("Chase", false);
                return;
            }

            if (attackSense.Attackable())
            {
                state = State.Attack;
                _anim.SetBool("Chase", false);
                return;
            }

            _characterController2D.Move(playerFinder.ChasePlayer() * Data.runSpeed);
        }

        protected virtual void updateAttack()
        {
            throw new NotImplementedException();
        }

        protected virtual void updateStun()
        {
            throw new NotImplementedException();
        }
        protected virtual void updateHitted()
        {
            _anim.SetTrigger("Hurt");
            if (Hp.GetHP() <= 0)
            {
                state = State.Die;
            }
            NextAction();
        }
        
        protected virtual void updateAttackStandby()
        {
            throw new NotImplementedException();
        }
        
        protected virtual void updateDie()
        {
            gameObject.layer = 0;
            if (_anim.GetBool("Dead") == false)
            {
                _anim.Play("Dead");
                _anim.SetBool("Dead", true);
            }
            _col.isTrigger = false;
        }

        protected virtual void updateAvoid()
        {
            throw new NotImplementedException();
        }
        
        protected int RandomDirection()
        {
            int RanNum = Random.Range(0, 100);
            if (RanNum > 50)
                return 1;
            else
            {
                Flip();
                return -1;
            }
        }

        protected override void onHurt()
        {
            base.onHurt();
            state = State.Hitted;
        }

        protected void Flip()
        {
            Vector2 thisScale = transform.localScale;
            if (patrolDirection.x >= 0)
            {
                thisScale.x = -Mathf.Abs(thisScale.x);
                patrolDirection = new Vector2(-Data.speed, 0f);
            }
            else
            {
                thisScale.x = Mathf.Abs(thisScale.x);
                patrolDirection = new Vector2(Data.speed, 0f);
            }
            transform.localScale = thisScale;
            _unitHPUI.Reverse();
        }

        protected IEnumerator CoPatrol(float patrolTime)
        {
            yield return new WaitForSeconds(patrolTime);
            state = State.Default;
            co_patrol = null;
            _anim.SetBool("Patrol", false);
        }

        public IEnumerator CoStun()
        {
            _anim.SetBool("Stun", true);
            yield return new WaitForSeconds(Data.stunTime);

            if (attackSense.Attackable())
                state = State.Attack;
            else if (playerFinder.FindPlayer())
                state = State.Chase;
            else
                state = State.Default;

            co_stun = null;
            _anim.SetBool("Stun", false);
        }

        public IEnumerator CoDefault()
        {
            yield return new WaitForSeconds(1f);
            _anim.SetBool("Default", false);
            state = State.Patrol;
            Flip();
            co_default = null;
        }

        public virtual void AniAfterAttack()
        {
            if (attackSense.Attackable())
                state = State.Attack;
            else if (playerFinder.FindPlayer())
                state = State.Chase;
            else
                state = State.Default;

            isPowerAttack = false;
            _anim.SetBool("Basic Attack", false);
            _anim.SetBool("Skill Attack", false);
            createAttackHitBox = false;
            Destroy(col);
        }

        public void AniParingenable()
        {
            IsAttack = true;
        }

        public void AniAttackStart()
        {
            IsAttack = false;
            _attack.EnableDamage();
        }

        public void AniAttackEnd()
        {
            _attack.DisableDamage();
        }

        public void AniDestroy()
        {
            Destroy(gameObject, 1f);
        }

        protected virtual void NextAction()
        {
            if (attackSense.Attackable())
                state = State.Attack;
            else if (playerFinder.FindPlayer())
                state = State.Chase;
            else
                state = State.Patrol;
        }

        /*
        void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                player.GetComponent<Character>().HittedWithKnockBack(new AttackInfo(1,
                player.gameObject.transform.position.x < transform.position.x
                ? Facing.Left
                : Facing.Right
                ));
            }
        }*/
    }
}