using Javor.SipSerializer.Exceptions;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Javor.SipSerializer.HeaderFields
{
    /// <summary>
    ///     "Via" header field
    /// </summary>
    public class Via 
    {
        protected Dictionary<string, string> _values
            = new Dictionary<string, string>();

        /// <summary>
        ///     Initialize new Via header.
        /// </summary>
        /// <param name="ipAddress">Host ip address.</param>
        /// <param name="port">Host port.</param>
        /// <param name="transportProtocol">Communication protocol used by host.</param>
        /// <param name="branch">Branch parameter.</param>
        public Via(IPAddress ipAddress, TransportProtocol transportProtocol, int port = 0, string branch = null)
        {
   
            // TODO branch check + guardian clausses
            IpAddress = ipAddress;
            Port = port;
            TransportProtocol = transportProtocol;
            Branch = branch;
        }
        
        /// <summary>
        ///     Sip version.
        /// </summary>
        public string Version { get; private set; } 
            = Constants.SipVersion;

        /// <summary>
        ///     Branch parameter.
        /// </summary>
        public string Branch
        {
            get
            {
                return GetElementOrDefault("branch");
            }
            private set
            {
                _values["branch"] = value;
            }
        }

        /// <summary>
        ///     Transport protocol using by sip sip server/client which issued via header.
        /// </summary>
        public TransportProtocol TransportProtocol { get; private set; }

        /// <summary>
        ///     Ip address or hostname of SIP server/client.
        /// </summary>
        public IPAddress IpAddress { get; private set; }

        /// <summary>
        ///     Port which will be used by endpoint to communicate with localhost.
        /// </summary>
        public int Port { get; private set; }

        /// <summary>
        ///     Get element value
        /// </summary>
        /// <param name="elementName">Element name (eg branch, rport, ...)</param>
        /// <returns>Element value or null when element does not exist</returns>
        public string GetElementOrDefault(string elementName)
        {
            _values.TryGetValue(elementName, out string elementValue);

            return elementValue;
        }

        /// <summary>
        ///     Add element to the via. Element will be rewritten if exist.
        /// </summary>
        /// <param name="elementName">Element name</param>
        /// <param name="elementValue">Element value - should be null</param>
        public void AddElement(string elementName, string elementValue = null)
        {
            if (_values.ContainsKey(elementName))
            {
                _values[elementName] = elementValue;
            }
            else
            {
                _values.Add(elementName, elementValue);
            }
        }

        /// <summary>
        ///     Convert Via header to the ascii form.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(Version);
            sb.Append("/");
            sb.Append(TransportProtocol.ToString());
            sb.Append(" ");
            sb.Append(IpAddress.ToString());

            if (Port > 0) // ip port handling
            {
                sb.Append(":");
                sb.Append(Port);
            }

            if (_values.Count > 0)
            {
                sb.Append(';');

                foreach (string elementName in _values.Keys)
                {
                    sb.Append(elementName);
                    string val = _values[elementName];

                    if (val != null)
                    {
                        sb.Append("=");
                        sb.Append(val);
                    }
                    sb.Append(";");
                }
                sb.Length--; // remove last ';'
            }

            return sb.ToString();
        }

        /// <summary>
        ///     Create new via from a raw via line
        /// </summary>
        /// <param name="s">Raw via line</param>
        /// <returns>Via</returns>
        public static Via Parse(string s)
        {
            ReadOnlySpan<char> sSpan = s.AsSpan();
            ReadOnlySpan<char> vSpan = Constants.SipVersion.AsSpan();

            if (!sSpan.StartsWith(vSpan))
            {
                throw new SipException("Invalid sip version");
            }

            int elementsStartAt = sSpan.IndexOf(';');

            int connIpDelimIndex = sSpan.IndexOf(' ');
            string connectionProto = sSpan.Slice(vSpan.Length + 1, connIpDelimIndex - vSpan.Length).ToString();
            string[] ipAddr = sSpan.Slice(connIpDelimIndex + 1, elementsStartAt - connIpDelimIndex - 1)
                .ToString()
                .Split(':');

            int port = 0;
            if (ipAddr.Length > 1)
                int.TryParse(ipAddr[1], out port);

            Via via = new Via(
                IPAddress.Parse(ipAddr[0]),
                (TransportProtocol)Enum.Parse(typeof(TransportProtocol), connectionProto),
                port);

            string[] elements = sSpan.Slice(elementsStartAt + 1, sSpan.Length - elementsStartAt - 1)
                .ToString()
                .Split(';');

            foreach (string element in elements)
            {
                string[] nameValue = element.Split('=');
                if (nameValue.Length > 1)
                {
                    via.AddElement(nameValue[0], nameValue[1]);
                }
                else
                {
                    via.AddElement(nameValue[0]);
                }
            }

            return via;
        }

        /// <summary>
        ///     Parse multiple raw via headers
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static IEnumerable<Via> ParseMultiple(IEnumerable<string> s)
        {
            foreach (string item in s)
                yield return Parse(item); 
        }
    }
}