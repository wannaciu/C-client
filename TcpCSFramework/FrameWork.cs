using System;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Diagnostics;
using System.Collections;

namespace TcpCSFramework
{
    /// <summary>
    /// ����ͨѶ�¼�ģ��ί��
    /// </summary>
    public delegate void NetEvent(object sender, NetEventArgs e);

    /// <summary>
    /// �ṩTCP���ӷ���ķ�������
    /// </summary>
    public class TcpSvr
    {
        #region �����ֶ�

        /// <summary>
        /// Ĭ�ϵķ�����������ӿͻ��˶�����
        /// </summary>
        public const int DefaultMaxClient = 1024;

        /// <summary>
        /// �������ݻ�������С64K
        /// </summary>
        public const int DefaultBufferSize = 64 * 1024;

        /// <summary>
        /// ������ݱ��Ĵ�С
        /// </summary>
        public const int MaxDatagramSize = 640 * 1024;

        /// <summary>
        /// ���Ľ�����
        /// </summary>
        private DatagramResolver _resolver;

        /// <summary>
        /// ͨѶ��ʽ���������
        /// </summary>
        private Coder _coder;

        /// <summary>
        /// ����������ʹ�õĶ˿�
        /// </summary>
        private ushort _port;

        /// <summary>
        /// ������������������ͻ���������
        /// </summary>
        private ushort _maxClient;

        /// <summary>
        /// ������������״̬
        /// </summary>
        private bool _isRun;

        /// <summary>
        /// �������ݻ�����
        /// </summary>
        private byte[] _recvDataBuffer;

        /// <summary>
        /// ������ʹ�õ��첽Socket��,
        /// </summary>
        private Socket _svrSock;

        /// <summary>
        /// �������пͻ��˻Ự�Ĺ�ϣ��
        /// </summary>
        private Hashtable _sessionTable;

        /// <summary>
        /// ��ǰ�����ӵĿͻ�����
        /// </summary>
        private ushort _clientCount;

        #endregion

        #region �¼�����

        /// <summary>
        /// �ͻ��˽��������¼�
        /// </summary>
        public event NetEvent ClientConn;

        /// <summary>
        /// �ͻ��˹ر��¼�
        /// </summary>
        public event NetEvent ClientClose;

        /// <summary>
        /// �������Ѿ����¼�
        /// </summary>
        public event NetEvent ServerFull;

        /// <summary>
        /// ���������յ������¼�
        /// </summary>
        public event NetEvent RecvData;

        #endregion

        #region ���캯��

        /// <summary>
        /// ���캯��
        /// </summary>
        /// <param name="port">�������˼����Ķ˿ں�</param>
        /// <param name="maxClient">�����������ɿͻ��˵��������</param>
        /// <param name="encodingMethod">ͨѶ�ı��뷽ʽ</param>
        public TcpSvr(ushort port, ushort maxClient, Coder coder)
        {
            _port = port;
            _maxClient = maxClient;
            _coder = coder;
        }


        /// <summary>
        /// ���캯��(Ĭ��ʹ��Default���뷽ʽ)
        /// </summary>
        /// <param name="port">�������˼����Ķ˿ں�</param>
        /// <param name="maxClient">�����������ɿͻ��˵��������</param>
        public TcpSvr(ushort port, ushort maxClient)
        {
            _port = port;
            _maxClient = maxClient;
            _coder = new Coder(Coder.EncodingMethod.Default);
        }


        /// <summary>
        /// ���캯��(Ĭ��ʹ��Default���뷽ʽ��DefaultMaxClient(100)���ͻ��˵�����)
        /// </summary>
        /// <param name="port">�������˼����Ķ˿ں�</param>
        public TcpSvr(ushort port)
            : this(port, DefaultMaxClient)
        {
        }

        #endregion

        #region ����

        /// <summary>
        /// ��������Socket����
        /// </summary>
        public Socket ServerSocket
        {
            get
            {
                return _svrSock;
            }
        }

        /// <summary>
        /// ���ݱ��ķ�����
        /// </summary>
        public DatagramResolver Resovlver
        {
            get
            {
                return _resolver;
            }
            set
            {
                _resolver = value;
            }
        }

        /// <summary>
        /// �ͻ��˻Ự����,�������еĿͻ���,������Ը���������ݽ����޸�
        /// </summary>
        public Hashtable SessionTable
        {
            get
            {
                return _sessionTable;
            }
        }

        /// <summary>
        /// �������������ɿͻ��˵��������
        /// </summary>
        public int Capacity
        {
            get
            {
                return _maxClient;
            }
        }

        /// <summary>
        /// ��ǰ�Ŀͻ���������
        /// </summary>
        public int SessionCount
        {
            get
            {
                return _clientCount;
            }
        }

        /// <summary>
        /// ����������״̬
        /// </summary>
        public bool IsRun
        {
            get
            {
                return _isRun;
            }

        }

        #endregion

        #region ���з���

        /// <summary>
        /// ��������������,��ʼ�����ͻ�������
        /// </summary>
        public virtual void Start()
        {
            if (_isRun)
            {
                throw (new ApplicationException("TcpSvr�Ѿ�������."));
            }
            _sessionTable = new Hashtable(1024);
            _recvDataBuffer = new byte[DefaultBufferSize];
            _svrSock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            IPEndPoint iep = new IPEndPoint(IPAddress.Any, _port);
            _svrSock.Bind(iep);
            _svrSock.Listen(1024);
            _svrSock.BeginAccept(new AsyncCallback(AcceptConn), _svrSock);

            _isRun = true;
        }

        /// <summary>
        /// ֹͣ����������,������ͻ��˵����ӽ��ر�
        /// </summary>
        public virtual void Stop()
        {
            if (!_isRun)
            {
                throw (new ApplicationException("TcpSvr�Ѿ�ֹͣ"));
            }
            _isRun = false;
            if (_svrSock.Connected)
            {
                _svrSock.Shutdown(SocketShutdown.Both);
            }

            CloseAllClient();
            _svrSock.Close();
            _sessionTable = null;
        }


        /// <summary>
        /// �ر����еĿͻ��˻Ự,�����еĿͻ������ӻ�Ͽ�
        /// </summary>
        public virtual void CloseAllClient()
        {
            foreach (Session client in _sessionTable.Values)
            {
                client.Close();
            }

            _clientCount = 0;
            _sessionTable.Clear();
        }

        /// <summary>
        /// �ر�һ����ͻ���֮��ĻỰ
        /// </summary>
        /// <param name="closeClient">��Ҫ�رյĿͻ��˻Ự����</param>
        public virtual void CloseSession(Session closeClient)
        {
            Debug.Assert(closeClient != null);

            if (closeClient != null)
            {

                closeClient.Datagram = null;
                _sessionTable.Remove(closeClient.ID);
                _clientCount--;

                //�ͻ���ǿ�ƹر�����
                if (ClientClose != null)
                {
                    ClientClose(this, new NetEventArgs(closeClient));
                }

                closeClient.Close();
            }
        }

        /// <summary>
        /// ��������
        /// </summary>
        /// <param name="recvDataClient">�������ݵĿͻ��˻Ự</param>
        /// <param name="datagram">���ݱ���</param>
        public virtual void Send(Session recvDataClient, string datagram)
        {
            //������ݱ���
            byte[] data = _coder.GetEncodingBytes(datagram);

            recvDataClient.ClientSocket.BeginSend(data, 0, data.Length, SocketFlags.None,
             new AsyncCallback(SendDataEnd), recvDataClient.ClientSocket);
        }
        /// <summary>
        /// ��������
        /// </summary>
        /// <param name="recvDataClient">�������ݵĿͻ��˻Ự</param>
        /// <param name="data"></param>
        public virtual void Send(Session recvDataClient, byte[] data)
        {
            //������ݱ���
            //byte[] data = _coder.GetEncodingBytes(datagram);

            recvDataClient.ClientSocket.BeginSend(data, 0, data.Length, SocketFlags.None,
             new AsyncCallback(SendDataEnd), recvDataClient.ClientSocket);
        }

        #endregion

        #region �ܱ�������
        /// <summary>
        /// �ر�һ���ͻ���Socket,������Ҫ�ر�Session
        /// </summary>
        /// <param name="client">Ŀ��Socket����</param>
        /// <param name="exitType">�ͻ����˳�������</param>
        protected virtual void CloseClient(Socket client, Session.ExitType exitType)
        {
            Debug.Assert(client != null);

            //���Ҹÿͻ����Ƿ����,���������,�׳��쳣
            Session closeClient = FindSession(client);

            closeClient.TypeOfExit = exitType;

            if (closeClient != null)
            {
                CloseSession(closeClient);
            }
            else
            {
                throw (new ApplicationException("��Ҫ�رյ�Socket���󲻴���"));
            }
        }

        /// <summary>
        /// �ͻ������Ӵ�����
        /// </summary>
        /// <param name="iar">���������������ӵ�Socket����</param>
        protected virtual void AcceptConn(IAsyncResult iar)
        {
            //���������ֹͣ�˷���,�Ͳ����ٽ����µĿͻ���
            if (!_isRun)
            {
                return;
            }

            //����һ���ͻ��˵���������
            Socket oldserver = (Socket)iar.AsyncState;
            Socket client = oldserver.EndAccept(iar);

            //����Ƿ�ﵽ��������Ŀͻ�����Ŀ
            if (_clientCount == _maxClient)
            {
                if (ServerFull != null)
                {
                    ServerFull(this, new NetEventArgs(new Session(client)));
                }
            }
            else
            {
                //�½�һ���ͻ�������
                Session newSession = new Session(client);
                _sessionTable.Add(newSession.ID, newSession);

                _clientCount++;

                newSession.RecvDataBuffer = new byte[16 * 1024];
                //��ʼ�������Ըÿͻ��˵�����
                client.BeginReceive(newSession.RecvDataBuffer, 0, newSession.RecvDataBuffer.Length, SocketFlags.None,
                 new AsyncCallback(ReceiveData), newSession);

                //�µĿͻ�������,����֪ͨ
                if (ClientConn != null)
                {
                    ClientConn(this, new NetEventArgs(newSession));
                }
            }

            //�������ܿͻ���
            _svrSock.BeginAccept(new AsyncCallback(AcceptConn), _svrSock);
        }

        /// <summary>
        /// ͨ��Socket�������Session����
        /// </summary>
        /// <param name="client"></param>
        /// <returns>�ҵ���Session����,���Ϊnull,˵���������ڸûỰ</returns>
        private Session FindSession(Socket client)
        {
            SessionId id = new SessionId((int)client.Handle);
            return (Session)_sessionTable[id];
        }

        /// <summary>
        /// ����������ɴ��������첽�����Ծ���������������У�
        /// �յ����ݺ󣬻��Զ�����Ϊ�ַ�������
        /// </summary>
        /// <param name="iar">Ŀ��ͻ���Socket</param>
        /// 
        //byte[] newBuffer = new byte[] { };//��С�ɱ�Ļ�����
        protected virtual void ReceiveData(IAsyncResult iar)
        {
            Session sendDataSession = (Session)iar.AsyncState;
            Socket client = sendDataSession.ClientSocket;

            try
            {
                //������ο�ʼ���첽�Ľ���,���Ե��ͻ����˳���ʱ��
                //������ִ��EndReceive
                int recv = client.EndReceive(iar);// +newBuffer.Length;

                if (recv == 0)
                {
                    CloseClient(client, Session.ExitType.NormalExit);
                    return;
                }

                string receivedData = _coder.GetEncodingString(sendDataSession.RecvDataBuffer, recv);
                string receivedData2 = _coder.GetEncodingString(_recvDataBuffer, recv);

                {
                    ICloneable copySession = (ICloneable)sendDataSession;
                    Session clientSession = (Session)copySession.Clone();
                    //clientSession.ClassName = this.GetClassFullName(ref receivedData);
                    clientSession.Datagram = receivedData;
                    clientSession.RecvDataBuffer = sendDataSession.RecvDataBuffer;
                    clientSession.Received = recv;
                    RecvData(this, new NetEventArgs(clientSession));
                }

                //���������������ͻ��˵�����
                client.BeginReceive(sendDataSession.RecvDataBuffer, 0, sendDataSession.RecvDataBuffer.Length, SocketFlags.None,
                 new AsyncCallback(ReceiveData), sendDataSession);
            }
            catch (SocketException ex)
            {
                if (10054 == ex.ErrorCode)
                {
                    //�ͻ���ǿ�ƹر�
                    CloseClient(client, Session.ExitType.ExceptionExit);
                }

            }
            catch (ObjectDisposedException ex)
            {
                if (ex != null)
                {
                    ex = null;
                    //DoNothing;
                }
            }
        }

        /// <summary>
        /// ����������ɴ�����
        /// </summary>
        /// <param name="iar">Ŀ��ͻ���Socket</param>
        protected virtual void SendDataEnd(IAsyncResult iar)
        {
            Socket client = (Socket)iar.AsyncState;

            int sent = client.EndSend(iar);
        }
        /// <summary>
        /// Э���������
        /// </summary>
        /// <param name="Term"></param>
        /// <returns></returns>
        private string GetClassFullName(ref string Term)
        {
            //{[object name][channel][request id][|param1|param2|param3|...|]}

            string ClassFullName = Term.Substring(2, Term.IndexOf(']') - 2);
            Term = "{" + Term.Substring(Term.IndexOf(']') + 1);

            return ClassFullName;
        }

        #endregion

    }


    /// <summary>
    /// �ṩTcp�������ӷ���Ŀͻ�����
    /// </summary>
    public class TcpCli
    {
        #region �ֶ�

        /// <summary>
        /// �ͻ����������֮��ĻỰ��
        /// </summary>
        private Session _session;

        /// <summary>
        /// �ͻ����Ƿ��Ѿ����ӷ�����
        /// </summary>
        private bool _isConnected = false;

        /// <summary>
        /// �������ݻ�������С64K
        /// </summary>
        public const int DefaultBufferSize = 1024;

        /// <summary>
        /// ���Ľ�����
        /// </summary>
        private DatagramResolver _resolver;

        /// <summary>
        /// ͨѶ��ʽ���������
        /// </summary>
        private Coder _coder;

        /// <summary>
        /// �������ݻ�����
        /// </summary>
        private byte[] _recvDataBuffer = new byte[DefaultBufferSize];

        #endregion

        #region �¼�����

        /// <summary>
        /// �Ѿ����ӷ������¼�
        /// </summary>
        public event NetEvent ConnectedServer;

        /// <summary>
        /// ���յ����ݱ����¼�
        /// </summary>
        public event NetEvent ReceivedDatagram;

        /// <summary>
        /// ���ӶϿ��¼�
        /// </summary>
        public event NetEvent DisConnectedServer;
        #endregion

        #region ����

        /// <summary>
        /// ���ؿͻ����������֮��ĻỰ����
        /// </summary>
        public Session ClientSession
        {
            get
            {
                return _session;
            }
        }

        /// <summary>
        /// ���ؿͻ����������֮�������״̬
        /// </summary>
        public bool IsConnected
        {
            get
            {
                return _isConnected;
            }
        }

        /// <summary>
        /// ���ݱ��ķ�����
        /// </summary>
        public DatagramResolver Resovlver
        {
            get
            {
                return _resolver;
            }
            set
            {
                _resolver = value;
            }
        }

        /// <summary>
        /// ���������
        /// </summary>
        public Coder ServerCoder
        {
            get
            {
                return _coder;
            }
        }

        #endregion

        #region ���з���

        /// <summary>
        /// Ĭ�Ϲ��캯��,ʹ��Ĭ�ϵı����ʽ
        /// </summary>
        public TcpCli()
        {
            _coder = new Coder(Coder.EncodingMethod.Default);
        }

        /// <summary>
        /// ���캯��,ʹ��һ���ض��ı���������ʼ��
        /// </summary>
        /// <param name="_coder">���ı�����</param>
        public TcpCli(Coder coder)
        {
            
            _coder = coder;
        }

        /// <summary>
        /// ���ӷ�����
        /// </summary>
        /// <param name="ip">������IP��ַ</param>
        /// <param name="port">�������˿�</param>
        public virtual void Connect(string ip, int port)
        {            
            if (IsConnected)
            {
                Debug.Assert(_session != null);
                Close();
            }
            
            Socket newsock = new Socket(AddressFamily.InterNetwork,
            SocketType.Stream, ProtocolType.Tcp);

            IPEndPoint iep = new IPEndPoint(IPAddress.Parse(ip), port);
            newsock.BeginConnect(iep, new AsyncCallback(Connected), newsock);

        }
        
        /// <summary>
        /// �������ݱ���
        /// </summary>
        /// <param name="datagram"></param>
        public virtual void Send(string datagram)
        {
            if (datagram.Length == 0)
            {
                return;
            }

            if (!_isConnected)
            {
                throw (new ApplicationException("û�����ӷ����������ܷ�������"));
            }

            //��ñ��ĵı����ֽ�
            byte[] data = _coder.GetEncodingBytes(datagram);

            //byte[] data = new byte[] { 68, 04, 07, 00, 00, 00 };

            _session.ClientSocket.BeginSend(data, 0, data.Length, SocketFlags.None,
             new AsyncCallback(SendDataEnd), _session.ClientSocket);
        }

        /// <summary>
        /// �������ݱ���
        /// </summary>
        /// <param name="data"></param>
        public virtual void Send(byte[] data)
        {


            if (!_isConnected)
            {
                throw (new ApplicationException("û�����ӷ����������ܷ�������"));
            }

           

            //byte[] data = new byte[] { 68, 04, 07, 00, 00, 00 };

            _session.ClientSocket.BeginSend(data, 0, data.Length, SocketFlags.None,
             new AsyncCallback(SendDataEnd), _session.ClientSocket);
        }

        /// <summary>
        /// �ر�����
        /// </summary>
        public virtual void Close()
        {
            if (!_isConnected)
            {
                return;
            }

            _session.Close();
            _session = null;
            _isConnected = false;
        }

        #endregion

        #region �ܱ�������

        /// <summary>
        /// ���ݷ�����ɴ�����
        /// </summary>
        /// <param name="iar"></param>
        protected virtual void SendDataEnd(IAsyncResult iar)
        {
            Socket remote = (Socket)iar.AsyncState;
            int sent = remote.EndSend(iar);
            Debug.Assert(sent != 0);
        }

        /// <summary>
        /// ����Tcp���Ӻ������
        /// </summary>
        /// <param name="iar">�첽Socket</param>
        protected virtual void Connected(IAsyncResult iar)
        {
            Socket socket = (Socket)iar.AsyncState;
            try
            {
                socket.EndConnect(iar);        
            }
            catch (SocketException exp)
            {
                string str = exp.Message;
                return;
            }
                

            //�����µĻỰ
            _session = new Session(socket);
            _isConnected = true;

            //�������ӽ����¼�
            if (ConnectedServer != null)
            {
                ConnectedServer(this, new NetEventArgs(_session));
            }

            _session.ClientSocket.BeginReceive(_recvDataBuffer, 0,
             DefaultBufferSize, SocketFlags.None,
             new AsyncCallback(RecvData), socket);            
        }

        /// <summary>
        /// ���ݽ��մ�����
        /// </summary>
        /// <param name="iar">�첽Socket</param>
        protected virtual void RecvData(IAsyncResult iar)
        {
            Socket remote = (Socket)iar.AsyncState;

            try
            {
                int recv = remote.EndReceive(iar);

                if (recv == 0)
                {
                    _session.TypeOfExit = Session.ExitType.NormalExit;

                    if (DisConnectedServer != null)
                    {
                        DisConnectedServer(this, new NetEventArgs(_session));
                    }

                    return;
                }

                string receivedData = _coder.GetEncodingString(_recvDataBuffer, recv);
                
                {
                    //omega ��� 2013��9��13��11:20:07
                    ICloneable copySession = (ICloneable)_session;
                    Session clientSession = (Session)copySession.Clone();
                    clientSession.Datagram = receivedData;
                    clientSession.RecvDataBuffer = _recvDataBuffer;
                    clientSession.Received = recv;
                    ReceivedDatagram(this, new NetEventArgs(clientSession));
                }


                //������������
                _session.ClientSocket.BeginReceive(_recvDataBuffer, 0, DefaultBufferSize, SocketFlags.None,
                 new AsyncCallback(RecvData), _session.ClientSocket);
            }
            catch (SocketException ex)
            {
                //�ͻ����˳�
                if (10054 == ex.ErrorCode)
                {
                    _session.TypeOfExit = Session.ExitType.ExceptionExit;

                    if (DisConnectedServer != null)
                    {
                        DisConnectedServer(this, new NetEventArgs(_session));
                    }
                }
                else
                {
                    throw (ex);
                }
            }
            catch (ObjectDisposedException ex)
            {
                if (ex != null)
                {
                    ex = null;
                    //DoNothing;
                }
            }
        }
        #endregion
    }
}



