using Exiled.API.Enums;
using Exiled.API.Interfaces;
using System.Collections.Generic;

namespace EXILEDBattleRoyale
{
    public class Config : IConfig
    {
        public bool IsEnabled { get; set; } = true;
        public float BattleClassDHP { get; set; } = 100f;
        public RoleType BorderRole { get; set; } = RoleType.Scp0492;
        public float BorderHP { get; set; } = 500f;
        public ZoneType SpawnZone { get; set; } = ZoneType.LightContainment;
        public Dictionary<string, List<string>> BattleTiers { get; set; } = new Dictionary<string, List<string>>()
        {
            {
                "LCZ", new List<string>()
                {
                    "LCZ_Curve",
                    "LCZ_TCross",
                    "LCZ_Crossing",
                    "LCZ_Airlock",
                    "LCZ_Straight",
                }
            },
            {
                "HCZ", new List<string>()
                {
                    "HCZ_Curve",
                    "HCZ_Room3",
                    "HCZ_Straight",
                }
            }
        };

        public List<ItemType> StartItems { get; set; } = new List<ItemType>()
        {
            ItemType.KeycardZoneManager,
            ItemType.Painkillers
        };

        public Dictionary<string, List<ItemType>> BattleTierItems { get; set; } = new Dictionary<string, List<ItemType>>()
        {
            {
                "LCZ", new List<ItemType>()
                {
                    ItemType.KeycardFacilityManager,
                    ItemType.GunCOM15,
                    ItemType.GrenadeFrag,
                    ItemType.GunE11SR,
                    ItemType.Medkit,
                    ItemType.Painkillers,
                    ItemType.Ammo556,
                    ItemType.Ammo762,
                    ItemType.Ammo9mm,
                }
            },
            {
                "HCZ", new List<ItemType>()
                {
                    ItemType.SCP018,
                    ItemType.SCP207,
                    ItemType.SCP268,
                    ItemType.SCP500,
                    ItemType.Adrenaline,
                    ItemType.KeycardChaosInsurgency,
                    ItemType.GunE11SR,
                    ItemType.GunLogicer,
                    ItemType.GunMP7,
                    ItemType.GunProject90,
                    ItemType.GunUSP,
                    ItemType.Ammo556,
                    ItemType.Ammo762,
                    ItemType.Ammo9mm,
                }
            }
        };
    }
}