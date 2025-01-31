using System.Collections.Generic;
using System.Linq;
using Model;
using Model.Runtime.Projectiles;
using UnityEngine;
using Utilities;
using static UnityEngine.GraphicsBuffer;

namespace UnitBrains.Player
{
    public class SecondUnitBrain : DefaultPlayerUnitBrain
    {
        public override string TargetUnitName => "Cobra Commando";
        private const float OverheatTemperature = 3f;
        private const float OverheatCooldown = 2f;
        private float _temperature = 0f;
        private float _cooldownTime = 0f;
        private bool _overheated;
        public List<Vector2Int> UnreachableTargets = new List<Vector2Int>();  

        protected override void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
        {
            float overheatTemperature = OverheatTemperature;
            //////////////////////////
            if (GetTemperature() >= overheatTemperature)
                return;
            else
            {
                for (int i = 0; i <= GetTemperature(); i++)
                {
                    var projectile = CreateProjectile(forTarget);
                    AddProjectileToList(projectile, intoList);
                }
            }
            IncreaseTemperature();
            ////////////////////////////
        }

        public override Vector2Int GetNextStep()
        {
            if (UnreachableTargets.Count > 0)
            {
                Vector2Int target = UnreachableTargets[0];
                Vector2Int nextPosition = Vector2Int.right;
                if (UnreachableTargets.Count > 0 && !IsTargetInRange(target))
                {
                    return unit.Pos.CalcNextStepTowards(target);
                }
                else
                {
                    return unit.Pos;
                }
            }
            return unit.Pos;
        }

        protected override List<Vector2Int> SelectTargets()
        {
            ///////////////////////////////////////
            List<Vector2Int> allTargets = GetAllTargets().ToList();
            List<Vector2Int> result = new List<Vector2Int>();

            var closestTarget = Vector2Int.zero;
            var closestDistance = float.MaxValue;

            if (allTargets.Count > 0)
            {
                foreach (var enemy in allTargets)
                {
                    float distance = DistanceToOwnBase(enemy);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestTarget = enemy;
                    }

                }

                if (IsTargetInRange(closestTarget))
                {
                    result.Add(closestTarget);
                    UnreachableTargets.Clear();
                }
                else
                {
                    UnreachableTargets.Add(closestTarget);
                }



            }

            var target = runtimeModel.RoMap.Bases[IsPlayerUnitBrain ? RuntimeModel.BotPlayerId : RuntimeModel.PlayerId];
            if (IsTargetInRange(target))
            {
                result.Add(target);
                return result;
            }

            UnreachableTargets.Add(target);
            return result;
        }
            /////////////////////////////////////////
        

        public override void Update(float deltaTime, float time)
        {
            if (_overheated)
            {              
                _cooldownTime += Time.deltaTime;
                float t = _cooldownTime / (OverheatCooldown/10);
                _temperature = Mathf.Lerp(OverheatTemperature, 0, t);
                if (t >= 1)
                {
                    _cooldownTime = 0;
                    _overheated = false;
                }
            }
        }

        private int GetTemperature()
        {
            if(_overheated) return (int) OverheatTemperature;
            else return (int)_temperature;
        }

        private void IncreaseTemperature()
        {
            _temperature += 1f;
            if (_temperature >= OverheatTemperature) _overheated = true;
        }
    }
}