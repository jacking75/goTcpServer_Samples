using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatClient
{
    public enum PACKET_ID : ushort
    {
        INVALID = 0,

        SYSTEM_CLIENT_CONNECT = 11,
        SYSTEM_CLIENT_DISCONNECTD = 12,

        #region DevPacket
        DEV_ECHO_REQ    = 101,
        DEV_ECHO_RES = 102,
        #endregion
               
        LoginReq = 201,
        LoginRes = 202,
        
        NewRoomReq = 203,
        NewRoomRes = 204,
        
        EnterRoomReq = 206,
        EnterRoomRes = 207,

        LeaveRoomReq = 209,
        LeaveRoomRes = 210,

        ChatRoomReq = 214,
        ChatRoomRes = 215,
        ChatRoomNtf = 216,
    }

    public enum ERROR_CODE : ushort
    {
        NONE = 0,
                
        USER_MGR_INVALID_USER_INDEX = 11,
        USER_MGR_INVALID_USER_UNIQUEID = 12,

        LOGIN_USER_ALREADY = 31,
        LOGIN_USER_USED_ALL_OBJ = 32,

        NEW_ROOM_USED_ALL_OBJ = 41,
        NEW_ROOM_FAIL_ENTER = 42,

        ENTER_ROOM_NOT_FINDE_USER = 51,
        ENTER_ROOM_INVALID_USER_STATUS = 52,
        ENTER_ROOM_NOT_USED_STATUS = 53,
        ENTER_ROOM_FULL_USER = 54,

        ROOM_INVALID_INDEX = 61,
        ROOM_NOT_USED = 62,
        ROOM_TOO_MANY_PACKET = 63,

        LEAVE_ROOM_INVALID_ROOM_INDEX = 71,

        CHAT_ROOM_INVALID_ROOM_INDEX = 81,
    }


#pragma warning disable 649                
#pragma warning restore 649
}
