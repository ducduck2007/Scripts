using UnityEngine;

public class SkillConfig
    {
        public string skillName;                // Tên skill (để debug/log)
        public float attackRange = 3f;          // TẦM ĐÁNH RIÊNG CHO TỪNG SKILL
        public float duration = 2f;             // Thời gian thực hiện skill
        public int damage = 3;                  // Sát thương của skill
        public GameObject enemyEffectPrefab;    // Prefab hiệu ứng trên enemy
        public Transform playerEffectPrefab;    // Prefab hiệu ứng trên player
        public string animationBool;            // Tên parameter animation trigger
        public float effectDelay = 0.3f;        // Độ trễ spawn hiệu ứng
        public float damageDelay = 0.5f;        // Độ trễ gây sát thương
        public bool spawnEffectAtStart = false; // Spawn hiệu ứng ngay khi bắt đầu
        public bool spawnEffectOnDamage = true; // Spawn hiệu ứng khi gây damage
        public float effectScale = 1f;          // Tỉ lệ scale hiệu ứng
        public float effectDuration = 2f;       // Thời gian tồn tại hiệu ứng

        // Thêm cho quả cầu lửa
        public GameObject fireballPrefab;       // Prefab quả cầu lửa
        public float fireballSpeed = 10f;       // Tốc độ bay của quả cầu lửa
        public bool hasFireball = false;        // Skill có sử dụng quả cầu lửa không
        public float fireballHeightOffset = 1f; // Độ cao spawn quả cầu lửa
        public float fireballRange = 10f;       // Tầm bay của quả cầu lửa

        // Thêm cho hiệu ứng khởi động
        public GameObject chargeUpEffectPrefab; // Prefab hiệu ứng khởi động (charge-up)
        public float chargeUpDuration = 0.5f;   // Thời gian khởi động trước khi spawn fireball
        public bool hasChargeUpEffect = false;  // Skill có hiệu ứng khởi động không
    }
