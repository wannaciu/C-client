using System;
using System.Collections.Generic;
using System.Text;

namespace TcpCSFramework
{
    /// <summary>
    /// ������������¼�����,�����˼������¼��ĻỰ����
    /// </summary>
    public class NetEventArgs : EventArgs
    {
        #region �ֶ�

        /// <summary>
        /// �ͻ����������֮��ĻỰ
        /// </summary>
        private Session _client;

        #endregion

        #region ���캯��
        /// <summary>
        /// ���캯��
        /// </summary>
        /// <param name="client">�ͻ��˻Ự</param>
        public NetEventArgs(Session client)
        {
            if (null == client)
            {
                throw (new ArgumentNullException());
            }

            _client = client;
        }
        #endregion

        #region ����

        /// <summary>
        /// ��ü������¼��ĻỰ����
        /// </summary>
        public Session Client
        {
            get
            {
                return _client;
            }

        }

        #endregion
    }
}
