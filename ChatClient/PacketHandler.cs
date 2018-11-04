using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace ChatClient
{
    class PacketHandler
    {
        static MainForm MainFormRef;


        public static void Init(MainForm form)
        {
            MainFormRef = form;
        }

        public static void Process(PacketData packet)
        {
            var packetType = (PACKET_ID)packet.PacketID;
            
            switch (packetType)
            {
                case PACKET_ID.SYSTEM_CLIENT_DISCONNECTD:
                    MainFormRef.SetDisconnectd();
                    break;

                case PACKET_ID.LoginRes:
                    {
                        var response = new LoginResPacket();
                        response.FromBytes(packet.BodyData);

                        if (response.Result == ERROR_CODE.NONE)
                        {
                            MainFormRef.SetClientStatus(CLIENT_STATUS.LOGIN);
                            MainFormRef.SetNewRoomNumber(-1);

                            DevLog.Write($"로그인 성공", LOG_LEVEL.INFO);
                        }
                        else
                        {
                            DevLog.Write(string.Format("로그인 실패:{0}", response.Result.ToString()), LOG_LEVEL.ERROR);
                        }
                    }
                    break;

                case PACKET_ID.NewRoomRes:
                    {
                        var response = new RoomNewResPacket();
                        response.FromBytes(packet.BodyData);

                        if (response.Result == ERROR_CODE.NONE)
                        {
                            MainFormRef.SetClientStatus(CLIENT_STATUS.ROOM);
                            MainFormRef.SetNewRoomNumber(response.RoomNumber);

                            DevLog.Write($"방 만들기 성공. 방 번호:{response.RoomNumber}", LOG_LEVEL.INFO);
                        }
                        else
                        {
                            DevLog.Write(string.Format("방 만들기 실패:{0}", response.Result.ToString()), LOG_LEVEL.ERROR);
                        }
                    }
                    break;

                case PACKET_ID.EnterRoomRes:
                    {
                        var response = new RoomEnterResPacket();
                        response.FromBytes(packet.BodyData);

                        if (response.Result == ERROR_CODE.NONE)
                        {
                            MainFormRef.SetClientStatus(CLIENT_STATUS.ROOM);
                            DevLog.Write($"방 들어가기 성공.", LOG_LEVEL.INFO);
                        }
                        else
                        {
                            DevLog.Write(string.Format("방 들어가기 실패:{0}", response.Result.ToString()), LOG_LEVEL.ERROR);
                        }
                    }
                    break;
                case PACKET_ID.LeaveRoomRes:
                    {
                        var response = new RoomLeaveResPacket();
                        response.FromBytes(packet.BodyData);

                        if (response.Result == ERROR_CODE.NONE)
                        {
                            MainFormRef.SetClientStatus(CLIENT_STATUS.LOGIN);
                            MainFormRef.SetNewRoomNumber(-1);

                            DevLog.Write($"방 나가기 성공.", LOG_LEVEL.INFO);
                        }
                        else
                        {
                            DevLog.Write(string.Format("방 나가기 실패:{0}", response.Result.ToString()), LOG_LEVEL.ERROR);
                        }
                    }
                    break;
                case PACKET_ID.ChatRoomRes:
                    {
                        var response = new RoomChatResPacket();
                        response.FromBytes(packet.BodyData);

                        if (response.Result == ERROR_CODE.NONE)
                        {
                            DevLog.Write($"채팅 보내기 성공", LOG_LEVEL.INFO);
                        }
                        else
                        {
                            DevLog.Write(string.Format("채팅 실패:{0}", response.Result.ToString()), LOG_LEVEL.ERROR);
                        }
                    }
                    break;
                case PACKET_ID.ChatRoomNtf:
                    {
                        var response = new RoomChatNotPacket();
                        response.FromBytes(packet.BodyData);
                        
                        MainFormRef.ChatToUI(response.UserID, response.Message);

                        DevLog.Write($"채팅 알림 받기 성공", LOG_LEVEL.INFO);
                    }
                    break;
                default:
                    break;
            }
        }
    }

    
}
