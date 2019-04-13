package main

import (
	"log"
	"os"
	"os/signal"
	"runtime"
	"syscall"
	"time"

	"github.com/davecgh/go-spew/spew"
)


//<<<--------------------------------------------------
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


//<<<--------------------------------------------------
var (
	// 호스트측에 종료를 통보
	OnDoneProcessExit = make(chan struct{})
)

// handle signals
func SignalsHandler_goroutine(onDone chan<- struct{}) {
	defer PrintPanicStack()

	log.Printf("signalsHandler Setting")

	ch := make(chan os.Signal, 1)
	signal.Notify(ch, syscall.SIGINT, syscall.SIGTERM)

	for {
		msg := <-ch

		switch msg {
		case syscall.SIGTERM: // os 명령어 kill로 종료 시켰음
			log.Printf("sigterm received: syscall.SIGTERM")

			_sighandlerProcessExit(onDone)
		case syscall.SIGINT: // ctrl + c 로 종료 시켰음
			log.Printf("sigterm received: syscall.SIGINT")

			_sighandlerProcessExit(onDone)
		}

		log.Printf("Exit sighandler")
		close(ch)
		return
	}

}

func _sighandlerProcessExit(onDone chan<- struct{}) {
	log.Printf("<<<<<<< -------------------")
	log.Printf("waiting for session closeSocket, please wait...")

	close(onDone)

	close(OnDoneProcessExit)

	time.Sleep(2 * time.Second)
}