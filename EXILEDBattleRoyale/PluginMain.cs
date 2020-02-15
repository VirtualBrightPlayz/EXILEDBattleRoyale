using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using EXILED;
using MEC;
using YamlDotNet.Core;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using System.IO;

namespace EXILEDBattleRoyale
{
    public class PluginMain : EXILED.Plugin
    {
        public PluginEvents PLEV;

        public override string getName => "EXILEDBattleRoyale";
        //public static List<int> items;
        public static int itemsSpawned = 100;
        //public static List<int> startingItems;
        //public static List<string> Rooms;
        public static List<string> Tiers;
        public static Dictionary<string, string> TierRooms;
        public static Dictionary<string, List<int>> TierItems;
        public static Dictionary<string, string> TierRoomsStart;
        public static Dictionary<string, List<int>> TierItemsStart;
        public static RoleType zeBorderRole = RoleType.Scp173;
        public static float zeBorderHP = 1000f;
        public static float combatHP = 100f;
        public static bool dbgRooms = false;
        public static string pluginDir;

        public override void OnDisable()
        {
            Events.CheckEscapeEvent -= PLEV.ThereIsNoEscape;
            Events.RoundStartEvent -= PLEV.CommitMassRedacted;
            Events.PlayerDeathEvent -= PLEV.MansNotAlive;
            Events.TeamRespawnEvent -= PLEV.CloseTheBorder;
            Events.DoorInteractEvent -= PLEV.AllowTheBorderTHROUGH;
            Events.PlayerJoinEvent -= PLEV.EnterTheMatchTheyMust;
            PLEV = null;
        }

        public void SetupTiers(IDictionary<object, object> data2)
        {
            foreach (var tier in Tiers)
            {
                Log.Debug(tier);
                List<int> list = new List<int>();
                try
                {
                    var item = (List<object>)data2["battle_tier_" + tier]; //Config.GetIntList("battle_tier_" + tier);
                    foreach (var item2 in item)
                    {
                        list.Add(int.Parse((string)item2));
                    }
                }
                catch (KeyNotFoundException e)
                {
                    Log.Warn("battle_tier_" + tier + " not found in config");
                }
                if (list == null || list.IsEmpty())
                {
                    list = new List<int>();
                    list.Add((int)ItemType.GunCOM15);
                    list.Add((int)ItemType.GrenadeFrag);
                    list.Add((int)ItemType.GunMP7);
                    list.Add((int)ItemType.Adrenaline);
                    list.Add((int)ItemType.KeycardChaosInsurgency);
                    list.Add((int)ItemType.GunProject90);
                    list.Add((int)ItemType.Ammo556);
                    list.Add((int)ItemType.Ammo762);
                    list.Add((int)ItemType.Ammo9mm);
                }
                TierItems.Add(tier, list);
            }
        }

        public void SetupTiersStart(IDictionary<object, object> data2)
        {
            foreach (var tier in Tiers)
            {
                List<int> list = new List<int>();
                try
                {
                    var item = (List<object>)data2["battle_tier_start_" + tier]; //Config.GetIntList("battle_tier_start_" + tier);
                    foreach (var item2 in item)
                    {
                        list.Add(int.Parse((string)item2));
                    }
                }
                catch (KeyNotFoundException e)
                {
                    Log.Warn("battle_tier_start_" + tier + " not found in config");
                }
                if (list == null || list.IsEmpty())
                {
                    list = new List<int>();
                    list.Add((int)ItemType.KeycardScientist);
                    list.Add((int)ItemType.GrenadeFrag);
                }
                TierItemsStart.Add(tier, list);
            }
        }

        public override void OnEnable()
        {
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            pluginDir = Path.Combine(appData, "Plugins", "BattleRoyale");
            if (!Directory.Exists(pluginDir))
                Directory.CreateDirectory(pluginDir);
            if (!File.Exists(Path.Combine(pluginDir, "config.yml")))
                File.WriteAllText(Path.Combine(pluginDir, "config.yml"), "");
            ReloadConfig();
        }

        public void ReloadConfig()
        {
            string data = File.ReadAllText(Path.Combine(pluginDir, "config.yml"));
            var des = new DeserializerBuilder().Build();
            var data2 = (IDictionary<object, object>)des.Deserialize<object>(data);

            try
            {
                itemsSpawned = (int)data2["battle_items_spawned"]; //Config.GetInt("battle_items_spawned", 100);
            }
            catch (KeyNotFoundException e)
            {
                Log.Warn("battle_items_spawned not found in config");
            }
            /*items = Config.GetIntList("battle_items");
            startingItems = Config.GetIntList("battle_items_start");
            Rooms = Config.GetStringList("battle_rooms");*/
            try
            {
                Log.Debug(data2["battle_tiers"].GetType().ToString());
                var item = (IList<object>)data2["battle_tiers"]; //Config.GetStringList("battle_tiers");
                Log.Debug(item.Count.ToString());
                Tiers = new List<string>();
                foreach (var item2 in item)
                {
                    Tiers.Add((string)item2);
                }
            }
            catch (KeyNotFoundException e)
            {
                Log.Warn("battle_tiers not found in config");
            }
            //starting tier
            try
            {
                var item = (IDictionary<object, object>)data2["battle_tier_start_rooms"];
                TierRoomsStart = new Dictionary<string, string>();
                foreach (var item2 in item)
                {
                    TierRoomsStart.Add((string)item2.Key, (string)item2.Value);
                }
            }
            catch (KeyNotFoundException e)
            {
                Log.Warn("battle_tier_start_rooms not found in config");
            }
            //spawn tier
            try
            {
                var item = (IDictionary<object, object>)data2["battle_tier_rooms"]; //Config.GetStringDictionary("battle_tier_rooms");
                TierRooms = new Dictionary<string, string>();
                foreach (var item2 in item)
                {
                    TierRooms.Add((string)item2.Key, (string)item2.Value);
                }
            }
            catch (KeyNotFoundException e)
            {
                Log.Warn("battle_tier_rooms not found in config");
            }
            try
            {
                dbgRooms = bool.Parse((string)data2["battle_rooms_dbg"]); //Config.GetBool("battle_rooms_dbg", false);
            }
            catch (KeyNotFoundException e)
            {
                Log.Warn("battle_rooms_dbg not found in config");
            }
            try
            {
                zeBorderRole = (RoleType)int.Parse((string)data2["battle_scp"]); //(RoleType)Config.GetInt("battle_scp", (int)RoleType.Scp173);
            }
            catch (KeyNotFoundException e)
            {
                Log.Warn("battle_scp not found in config");
            }
            try
            {
                zeBorderHP = float.Parse((string)data2["battle_scp_hp"]); //Config.GetFloat("battle_scp_hp", 1000f);
            }
            catch (KeyNotFoundException e)
            {
                Log.Warn("battle_scp_hp not found in config");
            }
            try
            {
                combatHP = float.Parse((string)data2["battle_classd_hp"]); //Config.GetFloat("battle_classd_hp", 100f);
            }
            catch (KeyNotFoundException e)
            {
                Log.Warn("battle_classd_hp not found in config");
            }
            /*if (items == null || items.IsEmpty())
            {
                items = new List<int>();
                items.Add((int)ItemType.GunCOM15);
                items.Add((int)ItemType.GrenadeFrag);
                items.Add((int)ItemType.GunMP7);
                items.Add((int)ItemType.Adrenaline);
                items.Add((int)ItemType.KeycardChaosInsurgency);
                items.Add((int)ItemType.GunProject90);
                items.Add((int)ItemType.Ammo556);
                items.Add((int)ItemType.Ammo762);
                items.Add((int)ItemType.Ammo9mm);
            }
            if (startingItems == null || startingItems.IsEmpty())
            {
                startingItems.Add((int)ItemType.KeycardScientistMajor);
                startingItems.Add((int)ItemType.GrenadeFrag);
            }
            if (Rooms == null || Rooms.IsEmpty())
            {
                Rooms = new List<string>();
                Rooms.Add("LCZ_Curve");
                Rooms.Add("LCZ_Crossing");
                Rooms.Add("LCZ_TCross");
                Rooms.Add("LCZ_Airlock");
                Rooms.Add("LCZ_Straight");
                //Rooms.Add("EZ_Cafeteria");
            }*/
            TierItems = new Dictionary<string, List<int>>();
            TierItemsStart = new Dictionary<string, List<int>>();
            if (TierRooms == null || TierRooms.IsEmpty())
            {
                Log.Warn("Error finding config battle_tier_rooms. Assuming configs set for tier items for LCZ. battle_tiers override.");
                TierRooms = new Dictionary<string, string>();
                TierRooms.Add("LCZ_Curve", "LCZ");
                TierRooms.Add("LCZ_Crossing", "LCZ");
                TierRooms.Add("LCZ_TCross", "LCZ");
                TierRooms.Add("LCZ_Airlock", "LCZ");
                TierRooms.Add("LCZ_Straight", "LCZ");

                Tiers = new List<string>();
                Tiers.Add("LCZ");


                /*var list = Config.GetIntList("battle_tier_LCZ");
                if (list == null || list.IsEmpty())
                {
                    list = new List<int>();
                    list.Add((int)ItemType.KeycardScientist);
                    list.Add((int)ItemType.GrenadeFrag);
                }
                foreach (var item in list)
                {
                    TierItems.Add("LCZ", item.ToString());
                }*/
            }
            SetupTiers(data2);
            if (TierRoomsStart == null || TierRoomsStart.IsEmpty())
            {
                Log.Warn("Error finding config battle_tier_start_rooms. Assuming configs set for tier items for LCZ. battle_tiers override.");
                TierRoomsStart = new Dictionary<string, string>();
                TierRoomsStart.Add("LCZ_Curve", "LCZ");
                TierRoomsStart.Add("LCZ_Crossing", "LCZ");
                TierRoomsStart.Add("LCZ_TCross", "LCZ");
                TierRoomsStart.Add("LCZ_Airlock", "LCZ");
                TierRoomsStart.Add("LCZ_Straight", "LCZ");

                Tiers = new List<string>();
                Tiers.Add("LCZ");


                /*var list = Config.GetIntList("battle_tier_LCZ");
                if (list == null || list.IsEmpty())
                {
                    list = new List<int>();
                    list.Add((int)ItemType.KeycardScientist);
                    list.Add((int)ItemType.GrenadeFrag);
                }
                foreach (var item in list)
                {
                    TierItems.Add("LCZ", item.ToString());
                }*/
            }
            SetupTiersStart(data2);
            PLEV = new PluginEvents(this);
            Events.CheckEscapeEvent += PLEV.ThereIsNoEscape;
            Events.RoundStartEvent += PLEV.CommitMassRedacted;
            Events.PlayerDeathEvent += PLEV.MansNotAlive;
            Events.TeamRespawnEvent += PLEV.CloseTheBorder;
            Events.DoorInteractEvent += PLEV.AllowTheBorderTHROUGH;
            Events.PlayerJoinEvent += PLEV.EnterTheMatchTheyMust;
        }

        public override void OnReload()
        {
            ReloadConfig();
        }
    }

    public class PluginEvents
    {
        public const float MinDist = 300;
        public Dictionary<Transform, string> rooms;
        public Dictionary<Transform, string> roomsstart;
        private PluginMain PL;

        public PluginEvents(PluginMain pl)
        {
            this.PL = pl;
            rooms = new Dictionary<Transform, string>();
            roomsstart = new Dictionary<Transform, string>();
        }

        public void ThereIsNoEscape(ref CheckEscapeEvent ev)
        {
            ev.Allow = false;
        }

        IEnumerator<float> SpawnAsDClass(GameObject plr)
        {
            yield return Timing.WaitForSeconds(0.5f);
            plr.GetComponent<CharacterClassManager>().SetClassID(RoleType.ClassD);
            plr.GetComponent<PlayerStats>().health = PluginMain.combatHP;
            plr.GetComponent<Broadcast>().TargetAddElement(plr.GetComponent<Broadcast>().connectionToClient, "<color=orange>Fight to the death!\nKILL ALL CLASS-D</color>", 10, true);
            plr.GetComponent<Inventory>().Clear();
            /*foreach (var item in PluginMain.startingItems)
            {
                plr.GetComponent<Inventory>().AddNewItem((ItemType)item);
            }*/
        }

        IEnumerator<float> SpawnAtRNG(GameObject plr)
        {
            yield return Timing.WaitForSeconds(2f);
            var room = roomsstart.ElementAt(UnityEngine.Random.Range(0, roomsstart.Count));
            plr.GetComponent<PlyMovementSync>().OverridePosition(room.Key.position, 0f);
            plr.GetComponent<Inventory>().Clear();
            foreach (int item in PluginMain.TierItemsStart[room.Value])
            {
                plr.GetComponent<Inventory>().AddNewItem((ItemType)item);
            }
        }

        IEnumerator<float> SpawnAsZeBORDER(GameObject plr)
        {
            yield return Timing.WaitForSeconds(0.5f);
            plr.GetComponent<CharacterClassManager>().SetClassID(PluginMain.zeBorderRole);
            plr.GetComponent<PlayerStats>().health = PluginMain.zeBorderHP;
            plr.GetComponent<Broadcast>().TargetAddElement(plr.GetComponent<Broadcast>().connectionToClient, "<color=red>You are the border!\nKILL ALL CLASS-D</color>", 10, true);
            plr.GetComponent<Inventory>().Clear();
        }

        public void CommitMassRedacted()
        {
            try
            {
                /*Log.Debug("Main.Rooms - " + (PluginMain.Rooms == null).ToString());
                Log.Debug("Main.items - " + (PluginMain.items == null).ToString());*/
                Log.Debug("this.rooms - " + (rooms == null).ToString());
                Log.Debug("079IFace " + (Interface079.singleton.allInteractables == null).ToString());
                Log.Debug("Players " + (PlayerManager.players == null).ToString());
                Log.Debug("Inv " + (Pickup.Inv == null).ToString());
            }
            catch (NullReferenceException e)
            {
                Log.Error("Oh noes: " + e.ToString());
            }

            Log.Debug("null chk");
            if (rooms == null)
            {
                rooms = new Dictionary<Transform, string>();
            }
            if (roomsstart == null)
            {
                roomsstart = new Dictionary<Transform, string>();
            }

            Log.Debug("Clear");
            rooms.Clear();
            roomsstart.Clear();
            Log.Debug("RLock");
            RoundSummary.RoundLock = true;


            foreach (var item in Interface079.singleton.allInteractables)
            {
                if (item != null && item.transform != null && item.type == Scp079Interactable.InteractableType.Camera && item.currentZonesAndRooms != null)
                {
                    foreach (var item2 in item.currentZonesAndRooms)
                    {
                        if (PluginMain.dbgRooms)
                            FileManager.AppendFileSafe(item2.currentRoom, "roomsdbg.txt");
                        foreach (var id in PluginMain.TierRooms)
                        {
                            var str = id.Key;
                            if (item2.currentRoom != null && item2.currentRoom.ToLower().Contains(str.ToLower()))
                            {
                                rooms.Add(item.transform, id.Value);
                                break;
                            }
                        }
                        foreach (var id in PluginMain.TierRoomsStart)
                        {
                            var str = id.Key;
                            if (item2.currentRoom != null && item2.currentRoom.ToLower().Contains(str.ToLower()))
                            {
                                Log.Debug(id.Value);
                                roomsstart.Add(item.transform, id.Value);
                                break;
                            }
                        }
                    }
                }
            }
            int idx = 0;
            foreach (var plr in PlayerManager.players)
            {
                /*if (idx == 0)
                    Timing.RunCoroutine(SpawnAsZeBORDER(plr));
                else*/
                if (plr != null)
                {
                    Timing.RunCoroutine(SpawnAsDClass(plr));
                    Timing.RunCoroutine(SpawnAtRNG(plr.gameObject));
                    idx++;
                }
            }
            for (int i = 0; i < PluginMain.itemsSpawned; i++)
            {
                try
                {
                    var room = rooms.ElementAt(UnityEngine.Random.Range(0, rooms.Count));
                    var roompos = room.Key.position;
                    var itemtier = PluginMain.TierItems[room.Value];
                    var newitem = itemtier[UnityEngine.Random.Range(0, itemtier.Count)];
                    Pickup.Inv.SetPickup((ItemType)newitem, 50, roompos, Quaternion.identity, 0, 0, 0);
                }
                catch (Exception e)
                {
                }
            }
        }

        public void MansNotAlive(ref PlayerDeathEvent ev)
        {
            int idx = 0;
            GameObject player = null;
            foreach (var plr in PlayerManager.players)
            {
                if (plr != null && plr.GetComponent<CharacterClassManager>().CurClass == RoleType.ClassD && plr.GetComponent<ReferenceHub>() != ev.Player)
                {
                    player = plr;
                    idx++;
                }
            }
            if (idx <= 1)
            {
                if (player != null)
                {
                    RoundSummary.RoundLock = false;
                    player.GetComponent<Broadcast>().RpcAddElement("<color=red>" + player.GetComponent<NicknameSync>().MyNick + " wins!</color>", 10, true);
                    foreach (var plr in PlayerManager.players)
                    {
                        if (plr != null && plr.GetComponent<CharacterClassManager>().CurClass != RoleType.ClassD)
                        {
                            plr.GetComponent<PlayerStats>().HurtPlayer(new PlayerStats.HitInfo(10000f, "Server", DamageTypes.Nuke, -1), null);
                        }
                    }
                }
            }
            else
            {
                ev.Player.GetComponent<Broadcast>().RpcAddElement("<color=orange>" + ev.Player.nicknameSync.MyNick + " died!\n" + (idx + 0) + " people left!</color>", 3, true);
            }
        }

        public void CloseTheBorder(ref TeamRespawnEvent ev)
        {
            foreach (var plr in ev.ToRespawn)
            {
                Timing.RunCoroutine(SpawnAsZeBORDER(plr.gameObject));
                Timing.RunCoroutine(SpawnAtRNG(plr.gameObject));
            }
        }

        public void AllowTheBorderTHROUGH(ref DoorInteractionEvent ev)
        {
            if (ev.Player.characterClassManager.CurClass == PluginMain.zeBorderRole)
            {
                ev.Allow = true;
            }
        }

        public void EnterTheMatchTheyMust(PlayerJoinEvent ev)
        {
            Timing.RunCoroutine(SpawnAsDClass(ev.Player.gameObject));
            Timing.RunCoroutine(SpawnAtRNG(ev.Player.gameObject));
        }
    }
}
