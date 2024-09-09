﻿using System.Net;
using System.Net.Sockets;

namespace ServerCore
{
    /// <summary>
    /// 서버가 클라이언트의 접속을 받아들이는 클래스
    /// </summary>
    public class Listener
    {
        Socket listenSocket;
        Func<Session> sessionFactory;



        public void Init(IPEndPoint endPoint, Func<Session> sessionFactory)
        {
            this.sessionFactory += sessionFactory;

            listenSocket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            listenSocket.Bind(endPoint);
            listenSocket.Listen(10);

            SocketAsyncEventArgs args = new SocketAsyncEventArgs();   
            args.Completed += new EventHandler<SocketAsyncEventArgs>(OnAcceptCompleted); // .AcceptAsync() 호출 후 Accept 작업이 완료되면 호출될 콜백 메서드 지정
            RegisterAccept(args);
        }

        private void RegisterAccept(SocketAsyncEventArgs args)
        {
            args.AcceptSocket = null;   // 재사용을 위해 null로 초기화

            bool pending = listenSocket.AcceptAsync(args);  // 비동기 Accept 요청

            if (pending == false) // pending == false => 동기적(바로)으로 CompleteAccept() 호출
                OnAcceptCompleted(null, args);
        }

        private void OnAcceptCompleted(object sender, SocketAsyncEventArgs args)
        {
            if(args.SocketError == SocketError.Success) // Accept 완료
            {
                Session session = sessionFactory.Invoke();

                session.Start(args.AcceptSocket);
                session.OnConnected(args.AcceptSocket.RemoteEndPoint);
            }
            else
                Console.WriteLine(args.SocketError.ToString());

            RegisterAccept(args);   // 다음 Accept 요청 대기
        }
    }
}
