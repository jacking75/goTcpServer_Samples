using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatClient
{
    public class PacketDataValue
    {
        public const int USER_ID_LENGTH = 16 * 2;
        public const int USER_PW_LENGTH = 16 * 2;
        public const int MAX_CHAT_STRING_SIZE = 64;
        public const int MAX_CHAT_SIZE = MAX_CHAT_STRING_SIZE * 2;
    }

    struct PacketData
    {
        public Int16 DataSize;
        public Int16 PacketID;
        public byte[] BodyData;
    }

    public class LoginReqPacket
    {
        public string UserID;
        public string UserPW;

        public byte[] ToBytes()
        {
            var userID = new byte[PacketDataValue.USER_ID_LENGTH];
            var userPW = new byte[PacketDataValue.USER_PW_LENGTH];
            Encoding.Unicode.GetBytes(UserID).CopyTo(userID, 0);
            Encoding.Unicode.GetBytes(UserPW).CopyTo(userPW, 0);

            List<byte> dataSource = new List<byte>();
            dataSource.AddRange(userID);
            dataSource.AddRange(userPW);

            return dataSource.ToArray();
        }
    }

    public class LoginResPacket
    {
        public ERROR_CODE Result;
        
        public bool FromBytes(byte[] bodyData)
        {
            Result = (ERROR_CODE)BitConverter.ToInt16(bodyData, 0);
            return true;
        }
    }


    public class RoomNewReqPacket
    {       
    }

    public class RoomNewResPacket
    {        
        public ERROR_CODE Result;
        public int RoomNumber;

        public bool FromBytes(byte[] bodyData)
        {
            Result = (ERROR_CODE)BitConverter.ToInt16(bodyData, 0);
            RoomNumber = BitConverter.ToInt32(bodyData, 2);
            return true;
        }
    }


    public class RoomEnterReqPacket
    {
        public int RoomNumber;

        public byte[] ToBytes()
        {
            List<byte> dataSource = new List<byte>();
            dataSource.AddRange(BitConverter.GetBytes(RoomNumber));            
            return dataSource.ToArray();
        }
    }

    public class RoomEnterResPacket
    {
        public ERROR_CODE Result;
        
        public bool FromBytes(byte[] bodyData)
        {
            Result = (ERROR_CODE)BitConverter.ToInt16(bodyData, 0);
            return true;
        }
    }


    public class RoomLeaveReqPacket
    {
    }

    public class RoomLeaveResPacket
    {
        public ERROR_CODE Result;

        public bool FromBytes(byte[] bodyData)
        {
            Result = (ERROR_CODE)BitConverter.ToInt16(bodyData, 0);
            return true;
        }
    }


    public class RoomChatReqPacket
    {
        public string Message;

        public byte[] ToBytes()
        {
            var message = new byte[PacketDataValue.MAX_CHAT_SIZE];
            Encoding.Unicode.GetBytes(Message).CopyTo(message, 0);

            List<byte> dataSource = new List<byte>();
            dataSource.AddRange(message);

            return dataSource.ToArray();
        }
    }

    public class RoomChatResPacket
    {
        public ERROR_CODE Result;

        public bool FromBytes(byte[] bodyData)
        {
            Result = (ERROR_CODE)BitConverter.ToInt16(bodyData, 0);
            return true;
        }
    }

    public class RoomChatNotPacket
    {
        public string UserID;
        public string Message;

        public bool FromBytes(byte[] bodyData)
        {
            string[] userID = Encoding.Unicode.GetString(bodyData, 0, PacketDataValue.USER_ID_LENGTH).Split('\0');
            string[] message = Encoding.Unicode.GetString(bodyData, PacketDataValue.USER_ID_LENGTH, PacketDataValue.MAX_CHAT_SIZE).Split('\0');
            
            UserID = (userID.Length > 0) ? userID[0] : null ;
            Message = (message.Length > 0) ? message[0] : null;
            
            return true;
        }
    }
}
