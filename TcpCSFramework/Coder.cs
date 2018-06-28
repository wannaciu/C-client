using System;
using System.Collections.Generic;
using System.Text;

namespace TcpCSFramework
{
    /// <summary>
    /// ͨѶ�����ṩ����ͽ������.
    /// </summary>
    public class Coder
    {
        /// <summary>
        /// ���뷽ʽ
        /// </summary>
        private EncodingMethod _encodingMethod;

        protected Coder()
        {

        }

        public Coder(EncodingMethod encodingMethod)
        {
            _encodingMethod = encodingMethod;
        }

        public enum EncodingMethod
        {
            Default = 0,
            Unicode,
            UTF8,
            ASCII,
        }

        /// <summary>
        /// ͨѶ���ݽ���
        /// </summary>
        /// <param name="dataBytes">��Ҫ���������</param>
        /// <returns>����������</returns>
        public virtual string GetEncodingString(byte[] dataBytes, int size)
        {
            switch (_encodingMethod)
            {
                case EncodingMethod.Default:
                    {
                        return Encoding.Default.GetString(dataBytes, 0, size);
                    }
                case EncodingMethod.Unicode:
                    {
                        return Encoding.Unicode.GetString(dataBytes, 0, size);
                    }
                case EncodingMethod.UTF8:
                    {
                        return Encoding.UTF8.GetString(dataBytes, 0, size);
                    }
                case EncodingMethod.ASCII:
                    {
                        return Encoding.ASCII.GetString(dataBytes, 0, size);
                    }
                default:
                    {
                        throw (new Exception("δ����ı����ʽ"));
                    }
            }

        }

        /// <summary>
        /// ���ݱ���
        /// </summary>
        /// <param name="datagram">��Ҫ����ı���</param>
        /// <returns>����������</returns>
        public virtual byte[] GetEncodingBytes(string datagram)
        {
            switch (_encodingMethod)
            {
                case EncodingMethod.Default:
                    {
                        return Encoding.Default.GetBytes(datagram);
                    }
                case EncodingMethod.Unicode:
                    {
                        return Encoding.Unicode.GetBytes(datagram);
                    }
                case EncodingMethod.UTF8:
                    {
                        return Encoding.UTF8.GetBytes(datagram);
                    }
                case EncodingMethod.ASCII:
                    {
                        return Encoding.ASCII.GetBytes(datagram);
                    }
                default:
                    {
                        throw (new Exception("δ����ı����ʽ"));
                    }
            }
        }
    }
}
