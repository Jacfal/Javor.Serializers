using Javor.SipSerializer.Schemes;
using System;
using System.Collections.Generic;
using System.Net;

namespace SipClient
{
    /// <summary>
    ///     SIp builder interface.
    /// </summary>
    public interface ISipBuilder
    {
        string User { get; set; }

        IEnumerable<string> AllowedMethods { get; set; }

        SipUri RegistrarUri { get; set; }
        SipUri ClientUri { get; set; }
    }

    /// <summary>
    ///     Default implementation of the Sip builder interface.
    /// </summary>
    public class SipBuilder : ISipBuilder
    {
        public string User { get; set; }

        public IEnumerable<string> AllowedMethods { get; set; }

        public SipUri RegistrarUri { get; set; }
        public SipUri ClientUri { get; set; }
    }
}