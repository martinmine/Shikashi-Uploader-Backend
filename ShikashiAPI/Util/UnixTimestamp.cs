using System;

namespace ShikashiAPI.Util
{
    public class UnixTimestamp
    {
        private static readonly DateTime epochStart = new DateTime(1970, 1, 1);

        public static long Timestamp()
        {
            return (long)(DateTime.UtcNow.Subtract(epochStart)).TotalSeconds;
        }
    }
}
