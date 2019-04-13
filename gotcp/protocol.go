package main

type Packet interface {
	Serialize() []byte
}

type Protocol interface {
	ReadPacket(recvData []byte) (Packet, int16)
}
