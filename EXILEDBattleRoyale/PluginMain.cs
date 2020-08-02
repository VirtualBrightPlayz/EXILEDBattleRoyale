using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Exiled;
using MEC;
using System.IO;
using Exiled.API.Features;
using Exiled.Events.EventArgs;

namespace EXILEDBattleRoyale
{
    public class PluginMain : Plugin<Config>
    {
        public override string Name => "BattleRoyale";
        public override string Author => "VirtualBrightPlayz";
        public override Version Version => new Version(1, 0, 0);

        public PluginEvents PLEV;

        public static PluginMain main;
        //public static List<int> items;
        public static int itemsSpawned = 100;
        //public static List<int> startingItems;
        //public static List<string> Rooms;
        /*public static List<string> Tiers;
        public static Dictionary<string, string> TierRooms;
        public static Dictionary<string, List<int>> TierItems;
        public static Dictionary<string, string> TierRoomsStart;
        public static Dictionary<string, List<int>> TierItemsStart;
        public static RoleType zeBorderRole = RoleType.Scp173;
        public static float zeBorderHP = 1000f;
        public static float combatHP = 100f;
        public static bool dbgRooms = false;
        public static string pluginDir;*/

        public override void OnDisabled()
        {
            base.OnDisabled();
            if (PLEV != null)
            {
                Exiled.Events.Handlers.Player.Escaping -= PLEV.ThereIsNoEscape;
                Exiled.Events.Handlers.Server.RoundStarted -= PLEV.CommitMassRedacted;
                Exiled.Events.Handlers.Player.Died += PLEV.MansNotAlive;
                Exiled.Events.Handlers.Server.RespawningTeam -= PLEV.CloseTheBorder;
                Exiled.Events.Handlers.Player.InteractingDoor -= PLEV.AllowTheBorderTHROUGH;
                Exiled.Events.Handlers.Player.Joined -= PLEV.EnterTheMatchTheyMust;
                Exiled.Events.Handlers.Map.Decontaminating -= PLEV.KILLTheLCZ;
                Exiled.Events.Handlers.Player.Left -= PLEV.PersonLeave;
                PLEV = null;
            }
            main = null;
        }

        public override void OnEnabled()
        {
            base.OnEnabled();
            Log.Info("Battle Royale enabled.");
            PLEV = new PluginEvents(this);
            Exiled.Events.Handlers.Player.Escaping += PLEV.ThereIsNoEscape;
            Exiled.Events.Handlers.Server.RoundStarted += PLEV.CommitMassRedacted;
            Exiled.Events.Handlers.Player.Died += PLEV.MansNotAlive;
            Exiled.Events.Handlers.Server.RespawningTeam += PLEV.CloseTheBorder;
            Exiled.Events.Handlers.Player.InteractingDoor += PLEV.AllowTheBorderTHROUGH;
            Exiled.Events.Handlers.Player.Joined += PLEV.EnterTheMatchTheyMust;
            Exiled.Events.Handlers.Map.Decontaminating += PLEV.KILLTheLCZ;
            Exiled.Events.Handlers.Player.Left += PLEV.PersonLeave;
            /*Events.CheckEscapeEvent += PLEV.ThereIsNoEscape;
            Events.RoundStartEvent += PLEV.CommitMassRedacted;
            Events.PlayerDeathEvent += PLEV.MansNotAlive;
            Events.TeamRespawnEvent += PLEV.CloseTheBorder;
            Events.DoorInteractEvent += PLEV.AllowTheBorderTHROUGH;
            Events.PlayerJoinEvent += PLEV.EnterTheMatchTheyMust;
            Events.DecontaminationEvent += PLEV.KILLTheLCZ;
            Events.PlayerLeaveEvent += PLEV.PersonLeave;*/
            main = this;
        }
    }

    public class PluginEvents
    {
        public const float MinDist = 300;
        public List<string> userids;
        public List<Transform> roomsstart;
        public Dictionary<Transform, string> rooms;
        private PluginMain PL;

        public PluginEvents(PluginMain pl)
        {
            this.PL = pl;
            roomsstart = new List<Transform>();
            rooms = new Dictionary<Transform, string>();
        }

        public void ThereIsNoEscape(EscapingEventArgs ev)
        {
            ev.IsAllowed = false;
        }

        void SetffOn()
        {
            Timing.CallDelayed(2f, () =>
            {
                ServerConsole.FriendlyFire = true;
                FriendlyFireConfig.LifeEnabled = false;
                FriendlyFireConfig.RespawnEnabled = false;
                FriendlyFireConfig.RoundEnabled = false;
                FriendlyFireConfig.WindowEnabled = false;
                ServerConfigSynchronizer.RefreshAllConfigs();
            });
        }

        IEnumerator<float> SpawnAsDClass(Player plr)
        {
            if (userids != null)
                userids.Add(plr.UserId);
            yield return Timing.WaitForSeconds(0.5f);
            plr.SetRole(RoleType.ClassD, true, false);
            plr.Health = PluginMain.main.Config.BattleClassDHP;
            plr.MaxHealth = (int)PluginMain.main.Config.BattleClassDHP;
            plr.Broadcast(10, "<color=orange>Fight to the death!\nKILL ALL CLASS-D</color>");
            plr.ClearInventory();
            SetffOn();
            foreach (ItemType item in PluginMain.main.Config.StartItems)
            {
                plr.AddItem(item);
            }
        }

        IEnumerator<float> SpawnAtRNG(Player plr)
        {
            yield return Timing.WaitForSeconds(2f);
            var room = roomsstart.ElementAt(UnityEngine.Random.Range(0, roomsstart.Count));
            plr.Position = room.position + Vector3.up * 1.5f;
            //plr.ClearInventory();
            SetffOn();
            /*foreach (ItemType item in PluginMain.main.Config.StartItems)
            {
                plr.AddItem(item);
            }*/
        }

        IEnumerator<float> SpawnAsZeBORDER(Player plr)
        {
            yield return Timing.WaitForSeconds(0.5f);
            plr.SetRole(PluginMain.main.Config.BorderRole);
            plr.Health = PluginMain.main.Config.BorderHP;
            plr.MaxHealth = (int)PluginMain.main.Config.BorderHP;
            plr.Broadcast(10, "<color=red>You are the border!\nKILL ALL CLASS-D</color>");
            plr.ClearInventory();
            SetffOn();
        }

        public void CalcRooms(bool lcz = true)
        {
            roomsstart.Clear();
            rooms.Clear();
            foreach (var room in Map.Rooms)
            {
                if (room.Zone == PluginMain.main.Config.SpawnZone)
                {
                    foreach (var item in PluginMain.main.Config.BattleTiers)
                    {
                        foreach (var name in item.Value)
                        {
                            if (room.Name.Contains(name))
                            {
                                roomsstart.Add(room.Transform);
                            }
                        }
                    }
                }
                if (room.Zone == Exiled.API.Enums.ZoneType.LightContainment && !lcz)
                {
                    continue;
                }
                foreach (var item in PluginMain.main.Config.BattleTiers)
                {
                    foreach (var name in item.Value)
                    {
                        if (room.Name.Contains(name))
                        {
                            rooms.Add(room.Transform, item.Key);
                        }
                    }
                }
            }
        }

        public void CommitMassRedacted()
        {
            if (roomsstart == null)
            {
                roomsstart = new List<Transform>();
            }
            if (rooms == null)
            {
                rooms = new Dictionary<Transform, string>();
            }
            if (userids == null)
            {
                userids = new List<string>();
            }

            RoundSummary.RoundLock = true;

            CalcRooms();

            int idx = 0;
            foreach (var plr in Player.List)
            {
                /*if (idx == 0)
                    Timing.RunCoroutine(SpawnAsZeBORDER(plr));
                else*/
                if (plr != null)
                {
                    Timing.RunCoroutine(SpawnAsDClass(plr));
                    Timing.RunCoroutine(SpawnAtRNG(plr));
                    idx++;
                }
            }
            for (int i = 0; i < PluginMain.itemsSpawned; i++)
            {
                try
                {
                    var room = rooms.ElementAt(UnityEngine.Random.Range(0, rooms.Count));
                    var roompos = room.Key.position;
                    var itemtier = PluginMain.main.Config.BattleTierItems[room.Value];
                    var newitem = itemtier[UnityEngine.Random.Range(0, itemtier.Count)];
                    Pickup.Inv.SetPickup((ItemType)newitem, 50, roompos + Vector3.up * 1.5f, Quaternion.identity, 0, 0, 0);
                }
                catch (Exception e)
                {
                }
            }
        }

        public void MansNotAlive(DiedEventArgs ev)
        {
            int idx = 0;
            Player player = null;
            foreach (var plr in Player.List)
            {
                if (plr != null && plr.Role == RoleType.ClassD && plr.ReferenceHub != ev.Target.ReferenceHub)
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
                    Map.Broadcast(10, "<color=red>" + player.Nickname + " wins!</color>", Broadcast.BroadcastFlags.Monospaced);
                    foreach (var plr in PlayerManager.players)
                    {
                        if (plr != null && plr.GetComponent<CharacterClassManager>().CurClass != RoleType.ClassD)
                        {
                            plr.GetComponent<CharacterClassManager>().SetClassID(RoleType.Spectator);
                        }
                    }
                }
            }
            else if (ev.Target.Role == RoleType.ClassD)
            {
                Map.Broadcast(3, "<color=orange>" + ev.Target.Nickname + " died!\n" + (idx + 0) + " people left!</color>", Broadcast.BroadcastFlags.Monospaced);
            }
        }

        public void CloseTheBorder(RespawningTeamEventArgs ev)
        {
            foreach (var plr in ev.Players)
            {
                Timing.RunCoroutine(SpawnAsZeBORDER(plr));
                Timing.RunCoroutine(SpawnAtRNG(plr));
            }
        }

        public void AllowTheBorderTHROUGH(InteractingDoorEventArgs ev)
        {
            if (ev.Player.Role == PluginMain.main.Config.BorderRole)
            {
                ev.IsAllowed = true;
            }
        }

        public void EnterTheMatchTheyMust(JoinedEventArgs ev)
        {
            Timing.RunCoroutine(SpawnAsDed(ev.Player));
        }

        public IEnumerator<float> SpawnAsDed(Player gameObject)
        {
            yield return Timing.WaitForSeconds(2f);
            gameObject.SetRole(RoleType.Spectator);
        }

        public void KILLTheLCZ(DecontaminatingEventArgs ev)
        {
            CalcRooms(false);
            return;
        }

        public void PersonLeave(LeftEventArgs ev)
        {
            int idx = 0;
            Player player = null;
            foreach (var plr in Player.List)
            {
                if (plr != null && plr.Role == RoleType.ClassD && plr.ReferenceHub != ev.Player.ReferenceHub)
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
                    Map.Broadcast(10, "<color=red>" + player.Nickname + " wins!</color>", Broadcast.BroadcastFlags.Monospaced);
                    foreach (var plr in PlayerManager.players)
                    {
                        if (plr != null && plr.GetComponent<CharacterClassManager>().CurClass != RoleType.ClassD)
                        {
                            plr.GetComponent<CharacterClassManager>().SetClassID(RoleType.Spectator);
                        }
                    }
                }
            }
            else if (ev.Player.Role == RoleType.ClassD)
            {
                Map.Broadcast(3, "<color=orange>" + ev.Player.Nickname + " died!\n" + (idx + 0) + " people left!</color>", Broadcast.BroadcastFlags.Monospaced);
            }
        }
    }
}
