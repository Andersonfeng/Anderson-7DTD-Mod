using System;
using System.Reflection;
using HarmonyLib;
using HarmonyLib.Tools;
using UnityEngine;

namespace AndersonRandomZombieHealth
{
    public class Demo : IModApi
    {
        public void InitMod(Mod modInstance)
        {
            Log.Out("AndersonRandomZombieHealth: InitMod");
            
            var harmony = new Harmony(base.GetType().Name);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            
            HarmonyFileLog.Enabled= true;

        }
    }

    [HarmonyPatch(typeof(EntityBuffs), nameof(EntityBuffs.ModifyValue))]
    public class PatchEntityBuffsModifyValue
    {
        
        //todo: 再不成功我就修改 PassiveEffect.ValueModifierTypes.base_set
        public static bool Prefix(
            PassiveEffects _effect,
            ref float _value,
            ref float _perc_val,
            FastTags<TagGroup.Global> _tags,
            EntityBuffs __instance)
        {
            if (__instance.parent.entityName.ContainsCaseInsensitive("zombie"))
            {
                
                var world = GameManager.Instance.World;
                var days = world.GetWorldTime() / 24000f;
                var scaleFactor = 1.0f + Mathf.FloorToInt(days / 7) * 0.1f;
                
                // 在这里修改 _value 和 _perc_val
                if (_effect == PassiveEffects.HealthMax) // 示例：仅当效果是 HealthMax 时修改
                {
                    // _value = 100f; // 修改基础血量
                    _perc_val *= scaleFactor; // 将 _perc_val 增加 50%
                }    
            }
            // 返回 true 表示继续执行原始方法，false 表示跳过原始方法
            return true;
        }
    }

    //
    /*
     * 游戏内没有仅删除背包的方法 , 想做一个能自定义death 掉落的MOD
     * inventory
     * equipment
     * 
     * 1.在执行之前删除背包
     * 2. 在执行之后找到落地的背包删除
     * 3. 将落地的背包清空
     * 4. 用XML 临时解决了 , 将落地背包留存时间改为了 3s
     */
    // [HarmonyPatch(typeof(EntityAlive), nameof(EntityAlive.OnEntityDeath))]
    public class PatchEntityAliveOnEntityDeath
    {
        [HarmonyPostfix]
        public static void Postfix(EntityAlive __instance)
        {
            Log.Out("AndersonCustomDropbag: postfix");
        }
        
        [HarmonyPrefix]
        public static bool Prefix(EntityAlive __instance)
        {
            if (__instance.entityType == EntityType.Player)
            {
                Log.Out("AndersonCustomDropbag: prefix");
                ItemStack[] slots = __instance.bag.GetSlots();
                for (int index = 0; index < slots.Length; ++index)
                    slots[index] = ItemStack.Empty.Clone();
                __instance.bag.SetSlots(slots);
                Log.Out("Player death: Backpack cleared, equipment and belt items retained.");    
            }
            return true;
        }
    }

}