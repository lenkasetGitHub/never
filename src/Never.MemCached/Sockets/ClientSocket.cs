﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;

namespace Never.Memcached.Sockets
{
    /// <summary>
    /// tcp socket 客户端
    /// </summary>
    public class ClientSocket : IWorkService
    {
        #region field and ctor
        private readonly List<ResultEventHandler<OnReceivedSocketEventArgs, byte[]>> eventHandlers = null;
        private readonly Socket socket = null;
        private readonly EndPoint serverEndPoint = null;
        private readonly EndPoint localEndPoint = null;
        private readonly System.Threading.ManualResetEvent manualResetEvent = null;
        private bool started = false;

        /// <summary>
        /// 
        /// </summary>
        public ClientSocket(SocketSetting setting, EndPoint serverEndPoint, EndPoint localEndPoint = null)
        {
            this.serverEndPoint = serverEndPoint;
            this.localEndPoint = localEndPoint;
            this.eventHandlers = new List<ResultEventHandler<OnReceivedSocketEventArgs, byte[]>>();
            this.socket = new System.Net.Sockets.Socket(System.Net.Sockets.AddressFamily.InterNetwork, System.Net.Sockets.SocketType.Stream, System.Net.Sockets.ProtocolType.Tcp)
            {
                ReceiveBufferSize = setting.ReceiveBufferSize,
                SendBufferSize = setting.SendBufferSize,
                //NoDelay = true,
                //Blocking = true,
            };

            //用来控制开始连接超时检查
            this.manualResetEvent = new System.Threading.ManualResetEvent(false);
        }

        #endregion

        /// <summary>
        /// connection
        /// </summary>
        public Connection Connection { get; private set; }

        /// <summary>
        /// 开始
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public ClientSocket Start(int millisecondsTimeout = 50000)
        {
            if (this.started)
                return this;

            this.started = true;
            var args = new SocketAsyncEventArgs()
            {
                AcceptSocket = this.socket,
                RemoteEndPoint = this.serverEndPoint,
            };
            args.Completed += this.ConnectionAsyncCompleted;

            if (this.localEndPoint != null)
            {
                this.socket.Bind(this.localEndPoint);
            }

            if (!this.socket.ConnectAsync(args))
            {
                this.ProcessConnection(this, args);
            }

            //不用的话创建不了session，这里应该要等待的，因为用的是异步，否则肯定是null对象的
            this.manualResetEvent.WaitOne(millisecondsTimeout);

            if (this.Connection != null)
            {
                this.manualResetEvent.Dispose();
                return this;
            }

            //不用的话创建不了session，这里应该要等待的，因为用的是异步，否则肯定是null对象的
            this.manualResetEvent.WaitOne(millisecondsTimeout);
            if (this.Connection == null)
                throw new ArgumentException(string.Format("连接不上远程服务器{0}", this.serverEndPoint.ToString()));

            return this;
        }

        /// <summary>
        /// 设置心跳包
        /// </summary>
        /// <param name="keepAlivePeriod">时间间隔</param>
        /// <returns></returns>
        public ClientSocket KeepAlive(TimeSpan keepAlivePeriod)
        {
            if (this.Connection != null)
                this.Connection.KeepAlive(keepAlivePeriod);

            return this;
        }

        /// <summary>
        /// 关闭
        /// </summary>
        /// <returns></returns>
        public ClientSocket Close()
        {
            this.Connection.Dispose();
            return this;
        }

        /// <summary>
        /// 接收到数据
        /// </summary>
        public event ResultEventHandler<OnReceivedSocketEventArgs, byte[]> OnMessageReceived
        {
            add
            {
                this.eventHandlers.Add(value);
            }
            remove
            {
                this.eventHandlers.Remove(value);
            }
        }

        /// <summary>
        /// 在连接关闭时刻
        /// </summary>
        public event EventHandler<SocketEventArgs> OnConnectionClosed;


        /// <summary>
        /// 在连接建立时刻
        /// </summary>
        public event EventHandler<SocketEventArgs> OnConnectionAccepted;

        private void ConnectionAsyncCompleted(object sender, SocketAsyncEventArgs e)
        {
            this.ProcessConnection(sender, e);
        }

        private void ProcessConnection(object sender, SocketAsyncEventArgs e)
        {
            var acceptSocket = e.AcceptSocket;
            //e应该没有用了，要不要删除?
            e.AcceptSocket = null;
            e.Completed -= this.ConnectionAsyncCompleted;
            e.Dispose();

            if (e.SocketError != SocketError.Success)
            {
                this.socket.Shutdown(SocketShutdown.Both);
                this.socket.Dispose();
            }

            var connection = new Connection(acceptSocket, (s) => new NetworkStream(s));
            connection.OnMessageReceived += this.Connection_OnMessageReceived;
            connection.OnConnectionClosed += this.Connection_OnConnectionClosed;
            connection.Start();
            this.manualResetEvent.Set();
            this.Connection = connection;
            this.OnConnectionAccepted?.Invoke(this, new SocketEventArgs(connection) { });
        }

        private void Connection_OnConnectionClosed(object sender, SocketEventArgs e)
        {
            try
            {
                ObjectExtension.Raise(e, sender, ref this.OnConnectionClosed);
            }
            finally
            {
                this.Connection = null;
            }
        }

        private IEnumerable<byte[]> Connection_OnMessageReceived(object sender, OnReceivedSocketEventArgs e)
        {
            foreach (var @delegate in this.eventHandlers)
            {
                try
                {
                    yield return @delegate(sender, e);
                }
                finally
                {
                }

            }
        }

        void IWorkService.Startup()
        {
            this.Start(30000);
        }

        void IWorkService.Shutdown()
        {
            this.Close();
        }

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="data">消息</param>
        public void Push(byte[] data)
        {
            if (this.Connection != null)
                this.Connection.Write(data);

            return;
        }

        /// <summary>
        /// 读消息
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        public byte[] ReadByte(int length)
        {
            if (this.Connection != null)
                return this.Connection.Read(length);

            return null;
        }
    }
}
