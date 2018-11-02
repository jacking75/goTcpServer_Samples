package main

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
