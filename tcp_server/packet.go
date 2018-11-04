package main

// protocol Id
const (
	Pkt_ID_Echo_Req = 101
	Pkt_ID_Echo_Res = 102


	Pkt_ID_LoginReq = 201
	Pkt_ID_LoginRes = 202

	Pkt_ID_NewRoomReq = 203
	Pkt_ID_NewRoomRes = 204

	Pkt_ID_EnterRoomReq = 206
	Pkt_ID_EnterRoomRes = 207

	Pkt_ID_LeaveRoomReq = 209
	Pkt_ID_LeaveRoomRes = 210

	Pkt_ID_ChatRoomReq = 214
	Pkt_ID_ChatRoomRes = 215
	Pkt_ID_ChatRoomNtf = 216
)


type PacketHeader struct {
	TotalSize int16
	Id int16
}


func DecodingPacketHeader(header *PacketHeader, data []byte) {
	reader := MakeReader(data, true)
	header.TotalSize, _ = reader.ReadS16()
	header.Id, _ = reader.ReadS16()
}

func EncodingPacketHeader(writer *RawPacketData, totalSize int16, pktId int16, packetType int8) {
	writer.WriteS16(totalSize)
	writer.WriteS16(pktId)
}

///
/*
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
*/
///

/*
type RoomChatReqPacket struct {
	MsgLength int16
	Msgs      []byte
}

func (request RoomChatReqPacket) EncodingPacket(packetId int16) ([]byte, int16) {

	totalSize := HEADER_SIZE + 2 + int16(request.MsgLength)
	sendBuf := make([]byte, totalSize)
	writer := MakeWriter(sendBuf, true)
	EncodingPacketHeader(&writer, totalSize, packetId, 0)

	writer.WriteS16(request.MsgLength)
	writer.WriteBytes(request.Msgs)
	return sendBuf, totalSize
}

func (request *RoomChatReqPacket) DecodingPacketPre(Data []byte) bool {
	bodyLength := len(Data)
	if bodyLength < 2 {
		return false
	}

	reader := MakeReader(Data, true)
	request.MsgLength, _ = reader.ReadS16()

	if bodyLength != int((2 + request.MsgLength)) {
		return false
	}

	request.Msgs = Data[2:]
	return true
}
*/