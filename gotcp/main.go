package main

import (
	"fmt"
	"log"
	"net"
	"os"
	"os/signal"
	"syscall"
	"time"

	"../serverLib"
)

type Callback struct{}

func (this *Callback) OnConnect(c *serverLib.Conn) bool {
	addr := c.GetRawConn().RemoteAddr()
	c.PutExtraData(addr)
	fmt.Println("OnConnect:", addr)
	return true
}

func (this *Callback) OnMessage(c *serverLib.Conn, p serverLib.Packet) bool {
	echoPacket := p.(*EchoPacket)
	fmt.Printf("OnMessage:[%v] [%v]\n", echoPacket.GetLength(), string(echoPacket.GetBody()))
	c.AsyncWritePacket(NewEchoPacket(echoPacket.Serialize(), true), time.Second)
	return true
}

func (this *Callback) OnClose(c *serverLib.Conn) {
	fmt.Println("OnClose:", c.GetExtraData())
}

func main() {
	// creates a tcp listener
	tcpAddr, err := net.ResolveTCPAddr("tcp4", ":32452")
	checkError(err)
	listener, err := net.ListenTCP("tcp", tcpAddr)
	checkError(err)

	// creates a server
	config := &serverLib.Config{
		PacketSendChanLimit:    20,
		PacketReceiveChanLimit: 20,
	}
	srv := serverLib.NewServer(config, &Callback{}, &EchoProtocol{})

	// starts service
	go srv.Start(listener, time.Second)
	fmt.Println("listening:", listener.Addr())

	// catchs system signal
	chSig := make(chan os.Signal)
	signal.Notify(chSig, syscall.SIGINT, syscall.SIGTERM)
	fmt.Println("Signal: ", <-chSig)

	// stops service
	srv.Stop()
}

func checkError(err error) {
	if err != nil {
		log.Fatal(err)
	}
}