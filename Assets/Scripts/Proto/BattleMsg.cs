using Framework.Web;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Proto
{
    public class BattleMsg
    {
        [Serializable]
        public class TankInfo
        {
            public string id = "";
            public int camp = 0;
            public int hp = 0;

            public float x = 0;
            public float y = 0;
            public float z = 0;
            public float ex = 0;
            public float ey = 0;
            public float ez = 0;
        }

        // 有玩家进入, 向所有玩家发生
        public class MsgEnterBattle : MsgBase
        {
            public MsgEnterBattle()
            {
                protoName = "MsgEnterBattle";
            }
            public TankInfo[] tankInfos;
            public int mapId = 1;
        }
        // 游戏结束, 全房间玩家广播结果
        public class MsgBattleResult : MsgBase
        { 
            public MsgBattleResult()
            {
                protoName = "MsgBattleResult";
            }
            public int winCamp = 0;
        }
        // 有玩家主动离开/断线, 向其他玩家广播
        public class MsgLeaveBattle : MsgBase
        { 
            public MsgLeaveBattle()
            {
                protoName = "MsgLeaveBattle";
            }
            public string id = "";
        }
           
    }
}