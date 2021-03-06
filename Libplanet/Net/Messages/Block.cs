using System.Collections.Generic;
using NetMQ;

namespace Libplanet.Net.Messages
{
    internal class Block : Message
    {
        public Block(byte[] payload)
        {
            Payload = payload;
        }

        public Block(NetMQFrame[] body)
        {
            Payload = body.ToByteArray();
        }

        public byte[] Payload { get; }

        protected override MessageType Type => MessageType.Block;

        protected override IEnumerable<NetMQFrame> DataFrames
        {
            get
            {
                yield return new NetMQFrame(Payload);
            }
        }
    }
}
