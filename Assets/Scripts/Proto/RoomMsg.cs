using Framework.Web;
using System;


namespace Proto
{
    [Serializable]
    public class RoomInfo
    {
        public int id=0;
        public int count=0;
        public int status=0;    // 0-×¼±¸ 1-Õ½¶·
    }

    [Serializable]
    public class PlayerInfo
    {
        public string id = "empty";
        public int camp = 0;
        public int win = 0;
        public int lose = 0;
        public int isOwner = 0;

    }

    public class MsgGetAchieve : MsgBase
    {
        public MsgGetAchieve(int win, int lose)
        {
            protoName = "MsgGetAchieve";
            this.win = win;
            this.lose = lose;
        }
        public int win;
        public int lose;
    }

    public class MsgGetRoomList : MsgBase
    {
        public MsgGetRoomList(RoomInfo[] rooms)
        {
            protoName = "MsgGetRoomList";
            this.rooms = rooms;
        }
        public RoomInfo[] rooms;
    }

    public class MsgCreateRoom : MsgBase
    {
        public MsgCreateRoom(int result)
        {
            protoName = "MsgCreateRoom";
            this.result = result;
        }
        public int result;
    }
    public class MsgEnterRoom : MsgBase
    {
        public MsgEnterRoom(int id, int result)
        {
            protoName = "MsgEnterRoom";
            this.id = id;
            this.result = result;
        }
        public int id;
        public int result;
    }
    
    public class MsgGetRoomInfo : MsgBase
    {
        public MsgGetRoomInfo(PlayerInfo[] players)
        {
            protoName = "MsgGetRoomInfo";
            this.players = players;
        }
        public PlayerInfo[] players;
    }

    public class MsgLeaveRoom : MsgBase
    {
        public MsgLeaveRoom(int result)
        {
            protoName = "MsgLeaveRoom";
            this.result = result;
        }
        public int result;
    }
    public class MsgStartBattle : MsgBase
    {
        public MsgStartBattle(int result)
        {
            protoName = "MsgStartBattle";
            this.result = result;
        }
        public int result;
    }
}