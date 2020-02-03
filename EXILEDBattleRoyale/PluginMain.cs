using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using EXILED;
using MEC;

namespace EXILEDBattleRoyale
{
    public class PluginMain : EXILED.Plugin
    {
        public PluginEvents PLEV;

        public override string getName => "EXILEDBattleRoyale";
        public static List<int> items;
        public static RoleType zeBorderRole = RoleType.Scp173;

        public override void OnDisable()
        {
            Events.CheckEscapeEvent -= PLEV.ThereIsNoEscape;
            Events.RoundStartEvent -= PLEV.CommitMassRedacted;
            Events.PlayerDeathEvent -= PLEV.MansNotAlive;
            Events.TeamRespawnEvent -= PLEV.CloseTheBorder;
            PLEV = null;
        }

        public override void OnEnable()
        {
            items = new List<int>();
            items = Config.GetIntList("battle_items");
            zeBorderRole = (RoleType)Config.GetInt("battle_scp");
            if (items.IsEmpty())
            {
                items.Add((int)ItemType.GunCOM15);
                items.Add((int)ItemType.GrenadeFrag);
                items.Add((int)ItemType.GunMP7);
                items.Add((int)ItemType.Medkit);
                items.Add((int)ItemType.KeycardO5);
                items.Add((int)ItemType.GunProject90);
            }
            PLEV = new PluginEvents();
            Events.CheckEscapeEvent += PLEV.ThereIsNoEscape;
            Events.RoundStartEvent += PLEV.CommitMassRedacted;
            Events.PlayerDeathEvent += PLEV.MansNotAlive;
            Events.TeamRespawnEvent += PLEV.CloseTheBorder;
        }

        public override void OnReload()
        {
        }
    }

    public class PluginEvents
    {
        public const float MinDist = 300;
        public List<Transform> rooms = new List<Transform>();
        public void ThereIsNoEscape(ref CheckEscapeEvent ev)
        {
            ev.Allow = false;
        }

        IEnumerator<float> SpawnAsDClass(GameObject plr)
        {
            yield return (1f);
            plr.GetComponent<CharacterClassManager>().SetClassIDAdv(RoleType.ClassD, false);
            plr.GetComponent<PlyMovementSync>().OverridePosition(rooms[UnityEngine.Random.Range(0, rooms.Count)].position, 0f, true);
            plr.GetComponent<Broadcast>().TargetAddElement(plr.GetComponent<Broadcast>().connectionToClient, "<color=orange>Fight to the death!\nKILL ALL D-CLASS</color>", 10, true);
            plr.GetComponent<Inventory>().AddNewItem(ItemType.KeycardZoneManager);
        }

        IEnumerator<float> SpawnAsZeBORDER(GameObject plr)
        {
            yield return (1f);
            plr.GetComponent<CharacterClassManager>().SetClassIDAdv(RoleType.Scp173, false);
            plr.GetComponent<PlyMovementSync>().OverridePosition(rooms[UnityEngine.Random.Range(0, rooms.Count)].position, 0f, true);
            plr.GetComponent<Broadcast>().TargetAddElement(plr.GetComponent<Broadcast>().connectionToClient, "<color=red>KILL ALL CLASS-D</color>", 10, true);
        }

        public void CommitMassRedacted()
        {
            RoundSummary.RoundLock = true;
            foreach (var item in Interface079.singleton.allInteractables)
            {
                if (item.type == Scp079Interactable.InteractableType.Camera)
                {
                    rooms.Add(item.transform);
                }
            }
            int idx = 0;
            foreach (var plr in PlayerManager.players)
            {
                if (idx == 0)
                    Timing.RunCoroutine(SpawnAsZeBORDER(plr));
                else
                    Timing.RunCoroutine(SpawnAsDClass(plr));
                idx++;
            }
            for (int i = 0; i < 100; i++)
            {
                var room = rooms[UnityEngine.Random.Range(0, rooms.Count)].position;
                var newitem = PluginMain.items[UnityEngine.Random.Range(0, PluginMain.items.Count)];
                Pickup.Inv.SetPickup((ItemType)newitem, 0, room, Quaternion.identity, 0, 0, 0);
            }
        }

        public void MansNotAlive(ref PlayerDeathEvent ev)
        {
            int idx = 0;
            GameObject player = null;
            foreach (var plr in PlayerManager.players)
            {
                if (plr.GetComponent<CharacterClassManager>().CurClass == RoleType.ClassD)
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
                }
            }
        }

        public void CloseTheBorder(ref TeamRespawnEvent ev)
        {
            foreach (var plr in ev.ToRespawn)
            {
                Timing.RunCoroutine(SpawnAsZeBORDER(plr.gameObject));
            }
        }
    }
}
