package main


func main() {
	server := NewServer("localhost:32452")

	server.OnNewClient(OnNewClient)
	server.OnClientConnectionClosed(OnClientConnectionClosed)
	server.OnNewMessage(OnNewMessage)

	go server.Listen()
}

func OnNewClient(c *Session) {
}

func OnClientConnectionClosed (c *Session, closeCase int) {
}

func OnNewMessage (c *Session, packetData []byte) {
}

