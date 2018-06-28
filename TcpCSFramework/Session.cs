using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Diagnostics;

namespace TcpCSFramework
{
    /// <summary>
    /// �ͻ����������֮��ĻỰ��
    /// </summary>
    public class Session : ICloneable
    {
        #region �ֶ�

        /// <summary>
        /// �ỰID
        /// </summary>
        private SessionId _id;

        /// <summary>
        /// �������ݻ�����
        /// </summary>
        private byte[] _recvDataBuffer;
        private int _received;

        /// <summary>
        /// �ͻ��˷��͵��������ı���
        /// ע��:����Щ����±��Ŀ���ֻ�Ǳ��ĵ�Ƭ�϶�������
        /// </summary>
        private string _datagram;

        /// <summary>
        /// �ͻ��˵�Socket
        /// </summary>
        private Socket _cliSock;

        /// <summary>
        /// �ͻ��˵��˳�����
        /// </summary>
        private ExitType _exitType;

        /// <summary>
        /// �Ựͨ����
        /// </summary>
        private string _channel = "";

        /// <summary>
        /// �˳�����ö��
        /// </summary>
        public enum ExitType
        {
            NormalExit,
            ExceptionExit
        };

        #endregion

        #region ����

        /// <summary>
        /// ���ػỰ��ID
        /// </summary>
        public SessionId ID
        {
            get
            {
                return _id;
            }
            set
            {
                _id = value;
            }
        }

        /// <summary>
        /// �������ݻ����� 
        /// </summary>
        public byte[] RecvDataBuffer
        {
            get
            {
                return _recvDataBuffer;
            }
            set
            {
                _recvDataBuffer = value;
            }
        }
        public int Received
        {
            get { return _received; }
            set { _received = value; }
        }
        /// <summary>
        /// ��ȡ�Ự�ı���
        /// </summary>
        public string Datagram
        {
            get
            {
                return _datagram;
            }
            set
            {
                _datagram = value;
            }
        }

        /// <summary>
        /// �����ͻ��˻Ự������Socket����
        /// </summary>
        public Socket ClientSocket
        {
            get
            {
                return _cliSock;
                
            }
        }


        /// <summary>
        /// ��ȡ�ͻ��˵��˳���ʽ
        /// </summary>
        public ExitType TypeOfExit
        {
            get
            {
                return _exitType;
            }

            set
            {
                _exitType = value;
            }
        }

        /// <summary>
        /// �Ựִ�е�ͨ����
        /// </summary>
        public string Channel
        {
            get
            {
                return _channel;
            }

            set
            {
                _channel = value;
            }
        }
        
        #endregion

        #region ����

        /// <summary>
        /// ʹ��Socket�����Handleֵ��ΪHashCode,���������õ���������.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return (int)_cliSock.Handle;
        }

        /// <summary>
        /// ��������Session�Ƿ����ͬһ���ͻ���
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            Session rightObj = (Session)obj;

            return (int)_cliSock.Handle == (int)rightObj.ClientSocket.Handle;
        }

        /// <summary>
        /// ����ToString()����,����Session���������
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            string result = string.Format("Session:{0},IP:{1}",
             _id, _cliSock.RemoteEndPoint.ToString());

            //result.C
            return result;
        }

        /// <summary>
        /// ���캯��
        /// </summary>
        /// <param name="cliSock">�Ựʹ�õ�Socket����</param>
        public Session(Socket cliSock)
        {
            Debug.Assert(cliSock != null);

            _cliSock = cliSock;

            _id = new SessionId((int)cliSock.Handle);
        }

        /// <summary>
        /// �رջỰ
        /// </summary>
        public void Close()
        {
            Debug.Assert(_cliSock != null);

            //�ر����ݵĽ��ܺͷ���
            _cliSock.Shutdown(SocketShutdown.Both);

            //������Դ
            _cliSock.Close();
        }

        #endregion

        #region ICloneable ��Ա

        object System.ICloneable.Clone()
        {
            Session newSession = new Session(_cliSock);
            newSession.Datagram = _datagram;
            newSession.TypeOfExit = _exitType;

            return newSession;
        }

        #endregion
    }
}
