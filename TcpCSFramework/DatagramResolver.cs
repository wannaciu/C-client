using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;

namespace TcpCSFramework
{
    /// <summary>
    /// ���ݱ��ķ�����
    /// </summary>
    public class DatagramResolver
    {
        /// <summary>
        /// ���Ľ������
        /// </summary>
        private string endTag;

        /// <summary>
        /// ���ؽ������
        /// </summary>
        string EndTag
        {
            get
            {
                return endTag;
            }
        }

        /// <summary>
        /// �ܱ�����Ĭ�Ϲ��캯��,�ṩ���̳���ʹ��
        /// </summary>
        protected DatagramResolver()
        {

        }

        /// <summary>
        /// ���캯��
        /// </summary>
        /// <param name="endTag">���Ľ������</param>
        public DatagramResolver(string endTag)
        {
            if (endTag == null)
            {
                throw (new ArgumentNullException("������ǲ���Ϊnull"));
            }

            if (endTag == "")
            {
                throw (new ArgumentException("������Ƿ��Ų���Ϊ���ַ���"));
            }

            this.endTag = endTag;
        }

        /// <summary>
        /// ��������
        /// </summary>
        /// <param name="rawDatagram">ԭʼ����,����δʹ�õı���Ƭ��,
        /// ��Ƭ�ϻᱣ����Session��Datagram������</param>
        /// <returns>��������,ԭʼ���ݿ��ܰ����������</returns>
        public virtual string[] Resolve(ref string rawDatagram)
        {
            ArrayList datagrams = new ArrayList();

            //ĩβ���λ������
            int tagIndex = -1;

            while (true)
            {
                tagIndex = rawDatagram.IndexOf(endTag, tagIndex + 1);

                if (tagIndex == -1)
                {
                    break;
                }
                else
                {
                    //����ĩβ��ǰ��ַ�����Ϊ������������
                    string newDatagram = rawDatagram.Substring(0, tagIndex + endTag.Length);

                    datagrams.Add(newDatagram);

                    if (tagIndex + endTag.Length >= rawDatagram.Length)
                    {
                        rawDatagram = "";

                        break;
                    }

                    rawDatagram = rawDatagram.Substring(tagIndex + endTag.Length,
                        rawDatagram.Length - newDatagram.Length);

                    //�ӿ�ʼλ�ÿ�ʼ����
                    tagIndex = 0;
                }
            }

            string[] results = new string[datagrams.Count];

            datagrams.CopyTo(results);

            return results;
        }
    }
}
