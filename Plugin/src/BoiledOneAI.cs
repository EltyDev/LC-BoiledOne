using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using GameNetcodeStuff;
using Unity.Netcode;
using UnityEngine;

namespace BoiledOne {

    class BoiledOneAI : EnemyAI
    {

        enum State {
            IDLE,
            TARGETING,
            FOLLOWING,
            ATTACKING,
        }

        #pragma warning disable 0649
        
        public Transform turnCompass = null!;
        public Transform attackArea = null!;

        public BoxCollider hitBox = null!;
        public GameObject mesh = null!;
        #pragma warning restore 0649
        System.Random enemyRandom = null!;

        AudioClip[] noises = null!;
        float baseNoiseTimer;
        float noiseTimerRandomness;
        float noiseTimer;

        State actualState;

        [Conditional("DEBUG")]
        void LogIfDebugBuild(string text) {
            Plugin.Logger.LogInfo(text);
        }

            public override void Start() {
                base.Start();
                LogIfDebugBuild("A Boiled One spawned");
                enemyRandom = new System.Random(StartOfRound.Instance.randomMapSeed + thisEnemyIndex);
                baseNoiseTimer = 50f;
                noiseTimerRandomness = 20f;
                noiseTimer = baseNoiseTimer + (float)enemyRandom.NextDouble() * noiseTimerRandomness;
                if (Plugin.ModAssets == null)
                    throw new System.Exception("ModAssets is null, cannot load assets");
                noises = [
                    Plugin.ModAssets.LoadAsset<AudioClip>("BoiledOne-Noise-1"),
                    Plugin.ModAssets.LoadAsset<AudioClip>("BoiledOne-Noise-2"),
                    Plugin.ModAssets.LoadAsset<AudioClip>("BoiledOne-Noise-3"),
                    Plugin.ModAssets.LoadAsset<AudioClip>("BoiledOne-Noise-4"),
                    Plugin.ModAssets.LoadAsset<AudioClip>("BoiledOne-Noise-5"),
                    Plugin.ModAssets.LoadAsset<AudioClip>("BoiledPOne-Noise-6"),
                ];
                hitBox.enabled = false;
                mesh.SetActive(false);
                actualState = State.IDLE;
                //StartSearch(transform.position);
            }

        public override void Update() {
            base.Update();
            if (stunNormalizedTimer > 0f) {
                agent.speed = 0f;
            }
        }

        public void DoRandomNoise() {
            noiseTimer -= Time.deltaTime;
            if (noiseTimer <= 0) {
                LogIfDebugBuild($"Random Noise from a Boiled One");
                creatureVoice.PlayOneShot(noises[enemyRandom.Next(0, noises.Length)]);
                noiseTimer = baseNoiseTimer + (float)enemyRandom.NextDouble() * noiseTimerRandomness;
            }
        }

        public bool hasTalkieWalkie(PlayerControllerB player) {
            foreach (GrabbableObject grabbable in player.ItemSlots) {
                if (grabbable != null && grabbable is WalkieTalkie)
                    return true;
            }
            return false;
        }

        public PlayerControllerB? getRandomTarget() {
            if (StartOfRound.Instance.allPlayersDead )
                return null;
            float[] probalities = new float[StartOfRound.Instance.allPlayerScripts.Length];
            for (int i = 0; i < probalities.Length; i++) {
                if (StartOfRound.Instance.allPlayerScripts[i].isPlayerDead)
                    probalities[i] = 0f;
                else if (hasTalkieWalkie(StartOfRound.Instance.allPlayerScripts[i]))
                    probalities[i] = 2/StartOfRound.Instance.allPlayerScripts.Length;
                else
                    probalities[i] = 1/StartOfRound.Instance.allPlayerScripts.Length;
            }
            double randomValue = (float)enemyRandom.NextDouble();
            List<int> possibleTargets = probalities
                .Select((value, index) => new { value, index })
                .Where(x => x.value > randomValue)
                .Select(x => x.index)
                .ToList();
            if (possibleTargets.Count == 0)
                return null;
            return StartOfRound.Instance.allPlayerScripts[possibleTargets[enemyRandom.Next(0, possibleTargets.Count)]];
        }

        public void DoIdle() {
            if (targetPlayer == null || targetPlayer.isPlayerDead)
                targetPlayer = getRandomTarget();
            if (targetPlayer == null || !targetPlayer.isInsideFactory)
                return;
        }

        public override void DoAIInterval() {
            
            base.DoAIInterval();
            if (isEnemyDead || StartOfRound.Instance.allPlayersDead)
                return;
            switch (actualState) {
                case State.IDLE:
                    DoIdle();
                    break;
                case State.TARGETING:
                    break;
                case State.FOLLOWING:
                    break;
                case State.ATTACKING:
                    break;
            }
        }
    }
}