using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Meou.Transport.DotNetty
{
    public class RequestBytes : BytesHolder
    {

        // 请求id自增器, 用于映射 <id, request, response> 三元组
        //
        // id在 <request, response> 生命周期内保证进程内唯一即可, 在Id对应的Response被处理完成后这个id就可以再次使用了,
        // 所以id可在 <Long.MIN_VALUE ~ Long.MAX_VALUE> 范围内从小到大循环利用, 即使溢出也是没关系的, 并且只是从理论上
        // 才有溢出的可能, 比如一个100万qps的系统把 <0 ~ Long.MAX_VALUE> 范围内的id都使用完大概需要29万年.
        //
        // 未来jupiter可能将invokeId限制在48位, 留出高地址的16位作为扩展字段.


        // 用于映射 <id, request, response> 三元组
        private long _invokeId;
        public long _timestamp;
        private static long sequence = 0L;


        public RequestBytes() :this(Interlocked.Increment(ref sequence))
        {
            
            
        }

        public RequestBytes(long invokeId)
        {
            this._invokeId = invokeId;
        }

        public long invokeId()
        {
            return this._invokeId;
        }

        public long timestamp()
        {
            return this._timestamp;
        }

        public void timestamp(long timestamp)
        {
            this._timestamp = timestamp;
        }
    }
}
