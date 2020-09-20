using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace _93.RPG.Script
{
    public class FighterControl : MonoBehaviour
    {
        public GUISkin skin = null;

        [Header("이동 관련 속성")] [Tooltip("기본 이동속도")]
        public float moveSpeed = 2.0f;

        public float runSpeed = 3.5f;
        public float directionRotateSpeed = 100.0f;
        public float bodyRotateSpeed = 2.0f;

        [Range(0.01f, 5.0f)] public float velocityChangeSpeed = 0.1f;

        private Vector3 _currentVelocity = Vector3.zero;
        private Vector3 _moveDirection = Vector3.zero;

        private CharacterController _characterController = null;
        private CollisionFlags _collisionFlags = CollisionFlags.None;


        [Header("애니메이션 관련 속성")]
        public AnimationClip idleAnimClip = null;
        public AnimationClip walkAnimClip = null;
        public AnimationClip runAnimClip = null;
        public AnimationClip attack1AnimClip = null;
        public AnimationClip attack2AnimClip = null;
        public AnimationClip attack3AnimClip = null;
        public AnimationClip attack4AnimClip = null;
        private Animation _animation = null;

        
        public enum FighterState
        {
            None,
            Idle,
            Walk,
            Run,
            Attack,
            Skill
        }

        [Header("캐릭터 상태")]
        public FighterState _state = FighterState.None;
        public enum FighterAttackState
        {
            Attack1,Attack2,Attack3,Attack4
        }

        public FighterAttackState attackState = FighterAttackState.Attack1;
        public bool nextAttack = false;
        

        private void Start()
        {
            _characterController = GetComponent<CharacterController>();
            _animation = GetComponent<Animation>();
            _animation.playAutomatically = false;
            _animation.Stop();
            _state = FighterState.Idle;
            _animation[idleAnimClip.name].wrapMode = WrapMode.Loop;
            _animation[walkAnimClip.name].wrapMode = WrapMode.Loop;
            _animation[runAnimClip.name].wrapMode = WrapMode.Loop;
            _animation[attack1AnimClip.name].wrapMode = WrapMode.Once;
            _animation[attack2AnimClip.name].wrapMode = WrapMode.Once;
            _animation[attack3AnimClip.name].wrapMode = WrapMode.Once;
            _animation[attack4AnimClip.name].wrapMode = WrapMode.Once;
        }

        // Update is called once per frame
        private void Update()
        {
            // 이동 
            Move();

            // 몸통의 방향을 이동 방향으로 돌려줌
            BodyDirectionChange();
            AnimationControl();
            CheckState();
        }

        private void OnGUI()
        {
            GUI.skin = skin;
            GUILayout.Label($"현재 속도 : ${GetVelocitySpeed()}");
            // 캐릭터 컨트롤러 컴포넌트를 찾았고, 현재 내 캐릭터의 이동속도가 0이 아니라면.
            if (_characterController != null && _characterController.velocity != Vector3.zero)
            {
                GUILayout.Label($"current Velocity Vector : {_characterController.velocity}"); // 가속도 
                GUILayout.Label($"current Velocity Maginitude : {_characterController.velocity.magnitude}"); // 크기
            }
        }

        /// <summary>
        /// 이동 관련 함수
        /// </summary>
        void Move()
        {
            var vertical = Input.GetAxis("Vertical");
            var horizontal = Input.GetAxis("Horizontal");

            // MainCamera 게임오브젝트의 트랜스폼 컴포넌트
            if (Camera.main is null) return;

            var cameraTransform = Camera.main.transform;

            // 카메라가 바라보는 방향의 월드상의 방향을 가져옮
            var forward = cameraTransform.TransformDirection(Vector3.forward);
            forward.y = 0.0f;
            var right = new Vector3(forward.z, 0.0f, -forward.x);
            var targetDirection = horizontal * right + vertical * forward;

            // 현재 이동하는 방향에서 원하는 방향으로 조금씩 회전을 하게됨.
            _moveDirection = Vector3.RotateTowards(_moveDirection, targetDirection,
                directionRotateSpeed * Time.deltaTime, 1000.0f);

            //방향이기떄문에 크기는 없애고 방향만 가져옮 .
            _moveDirection = _moveDirection.normalized;

            var speed = moveSpeed;
            if (_state == FighterState.Run)
            {
                speed = runSpeed;
            }
            var moveAmount = (_moveDirection * (speed * Time.deltaTime));
            _collisionFlags = _characterController.Move(moveAmount);
        }

        /// <summary>
        /// 현재 내 캐릭터의 이동속도를 가져옮
        /// 보간으로 가져옮 
        /// </summary>
        /// <returns></returns>
        float GetVelocitySpeed()
        {
            if (_characterController.velocity == Vector3.zero)
            {
                _currentVelocity = Vector3.zero;
            }
            else
            {
                var goalVelocity = _characterController.velocity;
                goalVelocity.y = 0.0f;
                // 속도 보간
                _currentVelocity = Vector3.Lerp(_currentVelocity, goalVelocity,
                    velocityChangeSpeed * Time.fixedDeltaTime);
            }

            return _currentVelocity.magnitude;
        }

        private void BodyDirectionChange()
        {
            if (GetVelocitySpeed() > 0.0f)
            {
                var newForward = _characterController.velocity;
                newForward.y = 0.0f;
                transform.forward = Vector3.Lerp(transform.forward, newForward, bodyRotateSpeed * Time.deltaTime);
            }
        }

        private void AnimationPlay(AnimationClip clip)
        {
            _animation.clip = clip;
            _animation.CrossFade(clip.name);
        }

        private void AnimationControl()
        {
            switch (_state)
            {
                case FighterState.Idle:
                    AnimationPlay(idleAnimClip);
                    break;
                case FighterState.Walk:
                    AnimationPlay(walkAnimClip);
                    break;
                case FighterState.Run:
                    AnimationPlay(runAnimClip);
                    break;
                case FighterState.Attack:
                    break;
                case FighterState.Skill:
                    break;
                case FighterState.None:
                    break;
            }
        }

        private void CheckState()
        {
            var currentSpeed = GetVelocitySpeed();
            switch (_state)
            {
                case FighterState.Idle:
                    if (currentSpeed > 0.0f)
                    {
                        _state = FighterState.Walk;
                    }

                    break;
                case FighterState.Walk:
                    if (currentSpeed > 0.5f)
                    {
                        _state = FighterState.Run;
                    }
                    else if (currentSpeed < 0.01f)
                    {
                        _state = FighterState.Idle;
                    }

                    break;
                case FighterState.Run:
                    if (currentSpeed < 0.5f)
                    {
                        _state = FighterState.Walk;
                    }
                    if (currentSpeed < 0.1f)
                    {
                        _state = FighterState.Idle;
                    }
                    break;
                case FighterState.Attack:
                    break;
                case FighterState.Skill:
                    break;
            }
        }

        private void InputControl()
        {
            // 0은 왼쪽 버튼, 1은 오른쪽버튼, 2는 휠버튼
            if (Input.GetMouseButtonDown(0) == true)
            {
                if (_state != FighterState.Attack)
                {
                    _state = FighterState.Attack;
                    attackState = FighterAttackState.Attack1;
                }
                else
                {
                    switch (attackState)
                    {
                        case FighterAttackState.Attack1:
                            // 10%이상 진행되었다면
                            if (_animation[attack1AnimClip.name].normalizedTime > 0.1f)
                            {
                                nextAttack = true;
                            }
                            break;
                        case FighterAttackState.Attack2:
                            if (_animation[attack2AnimClip.name].normalizedTime > 0.1f)
                            {
                                nextAttack = true;
                            }
                            break;
                        case FighterAttackState.Attack3:
                            if (_animation[attack3AnimClip.name].normalizedTime > 0.1f)
                            {
                                nextAttack = true;
                            }
                            break;
                        case FighterAttackState.Attack4:
                            if (_animation[attack4AnimClip.name].normalizedTime > 0.1f)
                            {
                                nextAttack = true;
                            }
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }
        }

        /// <summary>
        /// 공격 애니메이션 재생이 끝나면 호출되는 애니메이션 이벤트 함수.
        /// </summary>
        void OnAttackAnimFinished()
        {
            if (nextAttack == true)
            {
                switch (attackState)
                {
                    case FighterAttackState.Attack1:
                        attackState = FighterAttackState.Attack2;
                        break;
                    case FighterAttackState.Attack2:
                        attackState = FighterAttackState.Attack3;
                        break;
                    case FighterAttackState.Attack3:
                        attackState = FighterAttackState.Attack4;
                        break;
                    case FighterAttackState.Attack4:
                        attackState = FighterAttackState.Attack1;
                        break;
                }
            }
            else
            {

                _state = FighterState.Idle;
                attackState = FighterAttackState.Attack1;
            }
        }
    }
}