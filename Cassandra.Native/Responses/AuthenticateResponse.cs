﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Cassandra.Native
{
    internal class AuthenticateResponse : IResponse
    {
        public const byte OpCode = 0x03;

        public string Authenticator;
        internal AuthenticateResponse(ResponseFrame frame)
        {
            BEBinaryReader cb = new BEBinaryReader(frame);
            Authenticator = cb.ReadString();
        }
    }
}
