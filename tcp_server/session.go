package main

import (
	"net"
)

// Session holds info about connection
type Session struct {
	conn   net.Conn
	Server *server
}

func (session *Session) handleTcpRead() {
	session.Server.onNewClientCallback(session)

	var startRecvPos int16
	var result int
	recviveBuff := make([]byte, MAX_RECEIVE_BUFFER_SIZE)

	for {
		recvBytes, err := session.conn.Read(recviveBuff[startRecvPos:])
		if err != nil {
			session.closeProcess(NET_CLOSE_REMOTE)
			return
		}

		if recvBytes < HEADER_SIZE {
			session.closeProcess(NET_CLOSE_RECV_TOO_SMALL_RECV_DATA)
			return
		}

		readAbleByte := int16(startRecvPos) + int16(recvBytes)
		startRecvPos, result = session.makePacket(readAbleByte, recviveBuff)
		if result != NET_ERROR_NONE {
			session.closeProcess(result)
			return
		}

	}
}

func (session *Session) closeProcess(reason int) {
	session.conn.Close()
	session.Server.onClientConnectionClosed(session, reason)
}

func (session *Session) makePacket(readAbleByte int16,	recviveBuff []byte) (int16, int) {
	var startRecvPos int16 = 0
	var readPos int16

	for {
		if readAbleByte < HEADER_SIZE {
			break
		}

		requireDataSize := packetTotalSize(recviveBuff[readPos:])

		if requireDataSize > readAbleByte {
			break
		}

		if requireDataSize > MAX_PACKET_SIZE {
			return startRecvPos, NET_ERROR_RECV_MAKE_PACKET_TOO_LARGE_PACKET_SIZE
		}

		ltvPacket := recviveBuff[readPos:(readPos + requireDataSize)]
		readPos += requireDataSize
		readAbleByte -= requireDataSize


		session.Server.onNewMessage(session, ltvPacket)
	}


	if readAbleByte > 0 {
		copy(recviveBuff, recviveBuff[readPos:(readPos+readAbleByte)])
	}

	startRecvPos = readAbleByte
	return startRecvPos, NET_ERROR_NONE
}

// Send text message to client
func (session *Session) Send(message string) error {
	_, err := session.conn.Write([]byte(message))
	return err
}

// Send bytes to client
func (session *Session) SendBytes(b []byte) error {
	_, err := session.conn.Write(b)
	return err
}

func (session *Session) Conn() net.Conn {
	return session.conn
}

func (session *Session) Close() error {
	return session.conn.Close()
}
