﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Cassandra.Native
{
    internal class SupportedResponse : IResponse
    {
        public const byte OpCode = 0x06;
        public OutputOptions Output;

        internal SupportedResponse(ResponseFrame frame)
        {
            var rd = new BEBinaryReader(frame);
            Output = new OutputOptions(rd);
        }
    }
}
