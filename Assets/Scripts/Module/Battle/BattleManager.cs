using Framework.UI;
using Framework.Web;
using Game;
using Proto;
using System;
using System.Collections.Generic;
using Tank;
using UI;
using UnityEngine;

namespace Battle
{

    public class BattleManager : MonoBehaviour
    {
        public static Dictionary<string, BaseTank> tanks = new Dictionary<string, BaseTank>();

        public static void Init()
        {
            NetManager.AddMsgListener("MsgEnterBattle", OnMsgEnterBattle);
            NetManager.AddMsgListener("MsgBattleResult", OnMsgBattleResult);
            NetManager.AddMsgListener("MsgLeaveBattle", OnMsgLeaveBattle);
            NetManager.AddMsgListener("MsgSyncTank", OnMsgSyncTank);
            NetManager.AddMsgListener("MsgFire", OnMsgFire);
            NetManager.AddMsgListener("MsgHit", OnMsgHit);
        }

        private static void OnMsgHit(MsgBase msgBase)
        {
            MsgHit msg = (MsgHit)msgBase;

            BaseTank hitTank = GetTank(msg.id);
            if (hitTank == null) return;

            hitTank.Attacked(msg.damage);
            hitTank.hp = msg.hp;
            if (hitTank.hp <= 0)
            {
                hitTank.Attacked(0);
            }
        }

        private static void OnMsgFire(MsgBase msgBase)
        {
            MsgFire msg = (MsgFire)msgBase;
            if (msg.id == GameMain.id) return;

            SyncTank syncTank = GetTank(msg.id) as SyncTank;
            if (syncTank == null) return;

            syncTank.SyncFire(msg);
        }

        private static void OnMsgSyncTank(MsgBase msgBase)
        {
            Debug.Log("[뇰랙MsgSyncTank쀼딧]");

            MsgSyncTank msg = (MsgSyncTank)msgBase;
            if (msg.id == GameMain.id) return;
            Debug.Log("[뇰랙MsgSyncTank쀼딧] 繫법菱성법쫀");

            SyncTank syncTank = GetTank(msg.id) as SyncTank;
            if (syncTank == null) return;
            Debug.Log("[뇰랙MsgSyncTank쀼딧] 繫법꿴璂syncTank법쫀");

            syncTank.SyncPos(msg);
        }

        public static void AddTank(string id, BaseTank tank)
        {
            tanks.Add(id, tank);
        }
        public static void RemoveTank(string id)
        {
            tanks.Remove(id);
        }
        public static BaseTank GetTank(string id)
        {
            if (!tanks.ContainsKey(id)) return null;

            return tanks[id];
        }
        public static BaseTank GetCtrlTank()
        {
            return GetTank(GameMain.id);
        }

        public static void Reset()
        {
            foreach(BaseTank tank in tanks.Values)
            {
                Destroy(tank.gameObject);
            }
            tanks.Clear();
        }

        private static void OnMsgLeaveBattle(MsgBase msgBase)
        {
            MsgLeaveBattle msg = (MsgLeaveBattle)msgBase;

            BaseTank tank = GetTank(msg.id);
            if (tank == null) return;

            RemoveTank(tank.id);
            Destroy(tank.gameObject);
        }

        private static void OnMsgBattleResult(MsgBase msgBase)
        {
            MsgBattleResult msg = (MsgBattleResult)msgBase;

            bool isWin = false;
            BaseTank tank = GetCtrlTank();
            if(tank!=null && tank.camp == msg.winCamp)
            {
                isWin = true;
            }

            PanelManager.Open<ResultPanel>(isWin);
        }

        private static void OnMsgEnterBattle(MsgBase msgBase)
        {
            MsgEnterBattle msg = (MsgEnterBattle)msgBase;
            EnterBattle(msg);
        }
        private static void EnterBattle(MsgEnterBattle msg)
        {
            BattleManager.Reset();

            PanelManager.Close("Module.Room.RoomPanel");
            PanelManager.Close("UI.ResultPanel");

            for (int i=0; i<msg.tankInfos.Length; ++i)
            {
                GenerateTanks(msg.tankInfos[i]);
            }

        }
        private static void GenerateTanks(TankInfo tankInfo)
        {
            GameObject tankGO = new GameObject("Tank_" + tankInfo.id);

            BaseTank tank = null;
            if(tankInfo.id == GameMain.id)
            {
                tank = tankGO.AddComponent<CtrlTank>();
                tankGO.AddComponent<Rigidbody>();
                tankGO.AddComponent<CameraFollow>();
            }
            else
            {
                tank = tankGO.AddComponent<SyncTank>();
                tankGO.AddComponent<Rigidbody>();
            }

            tank.id = tankInfo.id;
            tank.camp = tankInfo.camp;
            tank.hp = tankInfo.hp;

            tankGO.transform.position = new Vector3(tankInfo.x, tankInfo.y, tankInfo.z);
            tankGO.transform.eulerAngles = new Vector3(tankInfo.ex, tankInfo.ey, tankInfo.ez);

            if(tank.camp==1)
            {
                tank.Init("Tanks/Tank_Blue");
            }
            else
            {
                tank.Init("Tanks/Tank_Red");
            }

            AddTank(tank.id, tank);
        }
    }
}