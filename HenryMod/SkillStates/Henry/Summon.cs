﻿using EntityStates;
using RoR2;
using UnityEngine;
using R2API;

namespace HenryMod.SkillStates
{
    public class Summon : BaseSkillState
    {
        public static float damageCoefficient = Modules.StaticValues.gunDamageCoefficient;
        public static float procCoefficient = 1f;
        public static float baseDuration = 0.6f;
        public static float force = 800f;
        public static float recoil = 3f;
        public static float range = 256f;
        public static GameObject tracerEffectPrefab = Resources.Load<GameObject>("Prefabs/Effects/Tracers/TracerGoldGat");

        private float duration;
        private float fireTime;
        private bool hasFired;
        private string muzzleString;
        private BullseyeSearch search;
        private TeamIndex team;


        public override void OnEnter()
        {
            base.OnEnter();
            this.duration = Shoot.baseDuration / this.attackSpeedStat;
            this.fireTime = 0.2f * this.duration;
            base.characterBody.SetAimTimer(2f);
            this.muzzleString = "Muzzle";
            this.search = new BullseyeSearch();
            this.team = base.GetTeam();
            

            base.PlayAnimation("LeftArm, Override", "ShootGun", "ShootGun.playbackRate", 1.8f);
        }


        private void Fire()
        {
            if (!this.hasFired)
            {
                this.hasFired = true;

                base.characterBody.AddSpreadBloom(0f);
                EffectManager.SimpleMuzzleFlash(EntityStates.Commando.CommandoWeapon.FirePistol2.muzzleEffectPrefab, base.gameObject, this.muzzleString, false);
                Util.PlaySound("HenryShootPistol", base.gameObject);

                if (base.isAuthority)
                {
                    Ray aimRay = base.GetAimRay();
                    float d = 7;

                    new BulletAttack
                    {

                        bulletCount = 1,
                        aimVector = aimRay.direction,
                        origin = aimRay.origin,
                        damage = Shoot.damageCoefficient * this.damageStat,
                        damageColorIndex = DamageColorIndex.Default,
                        damageType = DamageType.Generic,
                        falloffModel = BulletAttack.FalloffModel.DefaultBullet,
                        maxDistance = Shoot.range,
                        force = Shoot.force,
                        hitMask = LayerIndex.CommonMasks.bullet,
                        minSpread = 0f,
                        maxSpread = 0f,
                        isCrit = base.RollCrit(),
                        owner = base.gameObject,
                        muzzleName = muzzleString,
                        smartCollision = false,
                        procChainMask = default(ProcChainMask),
                        procCoefficient = procCoefficient,
                        radius = 0.75f,
                        sniper = false,
                        stopperMask = LayerIndex.CommonMasks.bullet,
                        weapon = null,
                        tracerEffectPrefab = Shoot.tracerEffectPrefab,
                        spreadPitchScale = 0f,
                        spreadYawScale = 0f,
                        queryTriggerInteraction = QueryTriggerInteraction.UseGlobal,
                        hitEffectPrefab = EntityStates.Commando.CommandoWeapon.FirePistol2.hitEffectPrefab,
                        hitCallback = SummonWisp
                    }.Fire();

                    bool SummonWisp(ref BulletAttack.BulletHit hitInfo)
                    {
                        CharacterMaster characterMaster = new MasterSummon
                        {
                            masterPrefab = MasterCatalog.FindMasterPrefab("WispMaster"),
                            position = hitInfo.point + Vector3.up * d,
                            rotation = base.characterBody.transform.rotation,
                            //summonerBodyObject = healthComponent2.gameObject,
                            ignoreTeamMemberLimit = false,
                            teamIndexOverride = new TeamIndex?(TeamIndex.Player)
                        }.Perform();
                        characterMaster.GetBody().RecalculateStats();
                        characterMaster.GetBody().AddTimedBuff(RoR2Content.Buffs.Immune, 5f);
                        characterMaster.gameObject.AddComponent<MasterSuicideOnTimer>().lifeTimer = 5.5f;
                        characterMaster.inventory.CopyItemsFrom(base.characterBody.inventory);
                        characterMaster.inventory.ResetItem(RoR2Content.Items.ExtraLife.itemIndex);
                        return false;
                    }


                }
            }
        }
                        
                
            
        
        public override void OnExit()
        {
            base.OnExit();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (base.fixedAge >= this.fireTime)
            {
                this.Fire();
            }

            if (base.fixedAge >= this.duration && base.isAuthority)
            {
                this.outer.SetNextStateToMain();
                return;
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}