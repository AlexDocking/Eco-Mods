using System;

namespace ReplacementInteractions
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class ReplacementRPCAttribute : Attribute
    {
        public Type Type { get; }
        public string RPCName { get; }
        public Func<object, object[], object> Func { get; set; }
        public ReplacementRPCAttribute(Type type, string rpcName)
        {
            Type = type;
            RPCName = rpcName;
        }
    }
}
