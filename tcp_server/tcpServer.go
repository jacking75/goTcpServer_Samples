package main

import (
	"github.com/davecgh/go-spew/spew"
	"log"
	"net"
	"runtime"
)


// TCP server
type server struct {
	address                  string // Address to open connection: localhost:9999
	onNewClientCallback      func(c *Session)
	onClientConnectionClosed func(c *Session, closeCase int)
	onNewMessage             func(c *Session, packetData []byte)
}

// Called right after server starts listening new client
func (s *server) OnNewClient(callback func(c *Session)) {
	s.onNewClientCallback = callback
}

// Called right after connection closed
func (s *server) OnClientConnectionClosed(callback func(c *Session, closeCase int)) {
	s.onClientConnectionClosed = callback
}

// Called when Session receives new message
func (s *server) OnNewMessage(callback func(c *Session, packetData []byte)) {
	s.onNewMessage = callback
}


// Creates new tcp server instance
func NewServer(address string) *server {
	log.Println("Creating server with address", address)
	server := &server{
		address: address,
	}

	server.OnNewClient(func(c *Session) {})
	server.OnNewMessage(func(c *Session, packetData []byte) {})
	server.OnClientConnectionClosed(func(c *Session, closeCase int) {})

	return server
}

// Start network server
func (s *server) Listen() {
	listener, err := net.Listen("tcp", s.address)
	if err != nil {
		log.Fatal("Error starting TCP server.")
	}
	defer listener.Close()

	for {
		conn, _ := listener.Accept()
		client := &Session{
			conn:   conn,
			Server: s,
		}

		go client.handleTcpRead()
	}
}


// 출처: https://github.com/gonet2/agent
func PrintPanicStack(extras ...interface{}) {
	if x := recover(); x != nil {
		log.Printf("%v", x)

		i := 0
		funcName, file, line, ok := runtime.Caller(i)

		for ok {
			log.Printf("PrintPanicStack. [func]: %s, [file]: %s, [line]: %d\n", runtime.FuncForPC(funcName).Name(), file, line)

			i++
			funcName, file, line, ok = runtime.Caller(i)
		}

		for k := range extras {
			log.Printf("EXRAS#%v DATA:%v\n", k, spew.Sdump(extras[k]))
		}
	}
}
