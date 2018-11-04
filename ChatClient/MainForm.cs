using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;



namespace ChatClient
{
    public partial class MainForm : Form
    {
        ClientSimpleTcp Network = new ClientSimpleTcp();
        PacketBufferManager PacketBuffer = new PacketBufferManager();

        Queue<PacketData> RecvPacketQueue = new Queue<PacketData>();
        Queue<byte[]> SendPacketQueue = new Queue<byte[]>();

        bool IsNetworkThreadRunning = false;
        System.Threading.Thread NetworkReadThread = null;
        System.Threading.Thread NetworkSendThread = null;
        
        System.Windows.Threading.DispatcherTimer dispatcherUITimer;
                
        const int PacketHeaderSize = 4;


        CLIENT_STATUS ClientStatus = new CLIENT_STATUS();
        

        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            PacketBuffer.Init((8096 * 10), PacketHeaderSize, 1024);

            IsNetworkThreadRunning = true;
            NetworkReadThread = new System.Threading.Thread(this.NetworkReadProcess);
            NetworkReadThread.Start();
            NetworkSendThread = new System.Threading.Thread(this.NetworkSendProcess);
            NetworkSendThread.Start();

            dispatcherUITimer = new System.Windows.Threading.DispatcherTimer();
            dispatcherUITimer.Tick += new EventHandler(ReadPacketQueueProcess);
            dispatcherUITimer.Interval = new TimeSpan(0, 0, 0, 0, 100);
            dispatcherUITimer.Start();

            btnDisconnect.Enabled = false;

            PacketHandler.Init(this);

            DevLog.Write("프로그램 시작 !!!", LOG_LEVEL.INFO);
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Network.Close();

            IsNetworkThreadRunning = false;
            NetworkReadThread.Join();
            NetworkSendThread.Join();
        }

        // 접속하기
        private void btnConnect_Click(object sender, EventArgs e)
        {
            string address = textBoxIP.Text;

            if (checkBoxLocalHostIP.Checked)
            {
                address = "127.0.0.1";
            }

            int port = Convert.ToInt32(textBoxPort.Text);

            if (Network.Connect(address, port))
            {
                labelStatus.Text = string.Format("{0}. 서버에 접속 중", DateTime.Now);
                btnConnect.Enabled = false;
                btnDisconnect.Enabled = true;
            }
            else
            {
                labelStatus.Text = string.Format("{0}. 서버에 접속 실패", DateTime.Now);
            }
        }

        // 접속 종료
        private void button3_Click(object sender, EventArgs e)
        {
            SetDisconnectd();
            Network.Close();
        }

        // 에코 요청
        private void button2_Click(object sender, EventArgs e)
        {
            if (Network.IsConnected() == false)
            {
                MessageBox.Show("먼저 서버에 접속해 주세요");
                return;
            }

            if (string.IsNullOrEmpty(textBox1.Text))
            {
                MessageBox.Show("내용이 없어요");
                return;
            }

            PostSendPacket(PACKET_ID.DEV_ECHO_REQ, textBox1.Text.ToByteArray());
        }

        // 로그인 하기
        private void button1_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(textBoxUserID.Text))
            {
                MessageBox.Show("사용할 ID를 입력해 주세요");
                return;
            }

            var request = new LoginReqPacket();
            request.UserID = textBoxUserID.Text.ToString();
            request.UserPW = textBoxPW.Text.ToString();
            var bodyData = request.ToBytes();
            PostSendPacket(PACKET_ID.LoginReq, bodyData);
        }

        // 방 만들기
        private void button3_Click_1(object sender, EventArgs e)
        {
            if (ClientStatus == CLIENT_STATUS.LOGIN)
            {
                DevLog.Write($"방 만들기 요청", LOG_LEVEL.INFO);

                PostSendPacket(PACKET_ID.NewRoomReq, null);
                return;
            }
            else
            {
                MessageBox.Show("로그인 하지 않았거나, 방에 입장한 상태입니다.");
            }
        }

        // 방 나가기
        private void button4_Click(object sender, EventArgs e)
        {
            if (ClientStatus == CLIENT_STATUS.ROOM)
            {
                DevLog.Write($"방 나가기 요청", LOG_LEVEL.INFO);

                PostSendPacket(PACKET_ID.LeaveRoomReq, null);
                return;
            }
            else
            {
                MessageBox.Show("로그인 하지 않았거나, 방에 입장한 상태가 아닙니다.");
            }
        }

        // 방 입장
        private void btnLobbyEnter_Click(object sender, EventArgs e)
        {
            if (ClientStatus == CLIENT_STATUS.LOGIN)
            {
                DevLog.Write($"방 입장 요청", LOG_LEVEL.INFO);

                var request = new RoomEnterReqPacket();
                request.RoomNumber = textBoxRoomID.Text.ToInt32();
                var bodyData = request.ToBytes();
                PostSendPacket(PACKET_ID.EnterRoomReq, bodyData);
                return;
            }           
            else
            {
                MessageBox.Show("로그인 하지 않았거나, 방에 입장한 상태입니다.");
            }
        }

        // 채팅 보내기
        private void button5_Click(object sender, EventArgs e)
        {
            if (ClientStatus == CLIENT_STATUS.ROOM)
            {
                if (string.IsNullOrEmpty(textBoxSendChat.Text))
                {
                    MessageBox.Show("채팅을 입력해 주세요");
                    return;
                }

                if(textBoxSendChat.Text.Length > PacketDataValue.MAX_CHAT_STRING_SIZE)
                {
                    MessageBox.Show("채팅이 너무 깁니다");
                    return;
                }

                DevLog.Write($"방 채팅 요청", LOG_LEVEL.INFO);

                var request = new RoomChatReqPacket();
                request.Message = textBoxSendChat.Text.ToString();
                var bodyData = request.ToBytes();
                PostSendPacket(PACKET_ID.ChatRoomReq, bodyData);
                return;
            }
            else
            {
                MessageBox.Show("로그인 하지 않았거나, 방에 입장한 상태가 아닙니다.");
            }
        }


        
        public void SetClientStatus(CLIENT_STATUS status)
        {
            ClientStatus = status;           
        }

        public void SetNewRoomNumber(int RoomNumber)
        {
            textBoxRoomID.Text = RoomNumber.ToString();
        }

        public void SetDisconnectd()
        {
            if (btnConnect.Enabled == false)
            {
                btnConnect.Enabled = true;
                btnDisconnect.Enabled = false;
            }

            ClientStatus = CLIENT_STATUS.NONE;

            RecvPacketQueue.Clear();
            SendPacketQueue.Clear();

            labelStatus.Text = "서버 접속이 끊어짐";
        }

        public void ChatToUI(string userID, string chatMsg)
        {
            listBoxChat.Items.Add(string.Format("[{0}]: {1}", userID, chatMsg));
        }

        void PostSendPacket(PACKET_ID packetID, byte[] bodyData)
        {
            if (Network.IsConnected() == false)
            {
                MessageBox.Show("서버에 접속하지 않았습니다");
                return;
            }

            List<byte> dataSource = new List<byte>();

            if (bodyData != null)
            {
                var packetSize = (Int16)(bodyData.Length + PacketHeaderSize);
                dataSource.AddRange(BitConverter.GetBytes(packetSize));
                dataSource.AddRange(BitConverter.GetBytes((Int16)packetID));
                dataSource.AddRange(bodyData);
            }
            else
            {
                var packetSize = (Int16)(PacketHeaderSize);
                dataSource.AddRange(BitConverter.GetBytes(packetSize));
                dataSource.AddRange(BitConverter.GetBytes((Int16)packetID));
            }                     

            SendPacketQueue.Enqueue(dataSource.ToArray());
        }

        void NetworkReadProcess()
        {
            while (IsNetworkThreadRunning)
            {
                System.Threading.Thread.Sleep(32);

                if (Network.IsConnected() == false)
                {
                    continue;
                }

                var recvData = Network.Receive();

                if (recvData.Count > 0)
                {
                    PacketBuffer.Write(recvData.Array, recvData.Offset, recvData.Count);

                    while (true)
                    {
                        var data = PacketBuffer.Read();
                        if (data.Count < 1)
                        {
                            break;
                        }

                        var packet = new PacketData();
                        packet.DataSize = (short)(data.Count - PacketHeaderSize);
                        packet.PacketID = BitConverter.ToInt16(data.Array, data.Offset+2);
                        packet.BodyData = new byte[packet.DataSize];
                        Buffer.BlockCopy(data.Array, (data.Offset + PacketHeaderSize), packet.BodyData, 0, (data.Count - PacketHeaderSize));

                        lock (((System.Collections.ICollection)RecvPacketQueue).SyncRoot)
                        {
                            RecvPacketQueue.Enqueue(packet);
                        }
                    }
                }
                else
                {
                    var packet = new PacketData();
                    packet.PacketID = (short)PACKET_ID.SYSTEM_CLIENT_DISCONNECTD;
                    packet.DataSize = 0;
                    
                    lock (((System.Collections.ICollection)RecvPacketQueue).SyncRoot)
                    {
                        RecvPacketQueue.Enqueue(packet);
                    }
                }
            }
        }

        void NetworkSendProcess()
        {
            while (IsNetworkThreadRunning)
            {
                System.Threading.Thread.Sleep(32);

                if (Network.IsConnected() == false)
                {
                    continue;
                }

                lock (((System.Collections.ICollection)RecvPacketQueue).SyncRoot)
                {
                    if (SendPacketQueue.Count > 0)
                    {
                        var packet = SendPacketQueue.Dequeue();
                        Network.Send(packet);
                    }
                }
            }
        }

        void ReadPacketQueueProcess(object sender, EventArgs e)
        {
            ProcessLog();

            try
            {
                PacketData packet = new PacketData();

                lock (((System.Collections.ICollection)RecvPacketQueue).SyncRoot)
                {
                    if (RecvPacketQueue.Count() > 0)
                    {
                        packet = RecvPacketQueue.Dequeue();
                    }
                }

                if (packet.PacketID != 0)
                {
                    PacketHandler.Process(packet);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("ReadPacketQueueProcess. error:{0}", ex.Message));
            }
        }

        private void ProcessLog()
        {
            // 너무 이 작업만 할 수 없으므로 일정 작업 이상을 하면 일단 패스한다.
            int logWorkCount = 0;

            while (true)
            {
                string msg;

                if (DevLog.GetLog(out msg))
                {
                    ++logWorkCount;

                    if (listBoxLog.Items.Count > 512)
                    {
                        listBoxLog.Items.Clear();
                    }

                    listBoxLog.Items.Add(msg);
                    listBoxLog.SelectedIndex = listBoxLog.Items.Count - 1;
                }
                else
                {
                    break;
                }

                if (logWorkCount > 8)
                {
                    break;
                }
            }
        }

        
    }


    public enum CLIENT_STATUS
    {
        NONE = 0,
        LOGIN = 1,
        ROOM = 2,
    }
    
}
