using UnityEngine;

namespace _93.RPG.Script
{
    public class FighterControl : MonoBehaviour
    {
        [Header("이동 관련 속성")] 
        [Tooltip("기본 이동속도")] public float moveSpeed = 2.0f; 
        public float runSpeed = 3.5f;
        public float directionRotateSpeed = 100.0f;
        public float bodyRotateSpeed = 2.0f;

        [Range(0.01f,5.0f)] public float velocityChangeSpeed = 0.1f;
        
        private Vector3 _currentVelocity = Vector3.zero;
        private Vector3 _moveDirection = Vector3.zero;

        private CharacterController _characterController = null;
        private CollisionFlags _collisionFlags = CollisionFlags.None;
        // Start is called before the first frame update
        void Start()
        {
            _characterController = GetComponent<CharacterController>();
        }

        // Update is called once per frame
        void Update()
        {
        
        }

        /// <summary>
        /// 이동 관련 함수
        /// </summary>
        void Move()
        {
            // MainCamera 게임오브젝트의 트랜스폼 컴포넌트
            if (Camera.main is null) return;
            
            var cameraTransform = Camera.main.transform;
            
            // 카메라가 바라보는 방향의 월드상의 방향을 가져옮
            var forward = cameraTransform.TransformDirection(Vector3.forward);
            forward.y = 0.0f;
            var right = new Vector3(forward.z, 0.0f,-forward.x);
            var vertical = Input.GetAxis("Vertical");
            var horizontal = Input.GetAxis("Horizontal");
            var targetDirection = horizontal * right + vertical * forward;
        }
    }
    
}
