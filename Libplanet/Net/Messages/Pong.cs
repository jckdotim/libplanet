using System.Collections.Generic;
using NetMQ;

namespace Libplanet.Net.Messages
{
    internal class Pong : Message
    {
        public Pong(int appProtocolVersion)
        {
            this.AppProtocolVersion = appProtocolVersion;
        }

        public Pong(NetMQFrame[] body)
        {
            AppProtocolVersion = body[0].ConvertToInt32();
        }

        public int AppProtocolVersion { get; }

        protected override MessageType Type => MessageType.Pong;

        protected override IEnumerable<NetMQFrame> DataFrames
        {
            get
            {
                yield return new NetMQFrame(
                    NetworkOrderBitsConverter.GetBytes(AppProtocolVersion));
            }
        }
    }
}
