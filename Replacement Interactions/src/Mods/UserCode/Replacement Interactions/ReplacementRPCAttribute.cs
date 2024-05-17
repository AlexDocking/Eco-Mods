using Eco.Shared.Utils;
using System;
using System.Linq;
using System.Reflection;

namespace ReplacementInteractions
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class ReplacementRPCAttribute : Attribute
    {
        public Type Type { get; set; }
        public string RPCName { get; set; }
        public string ReplacementRPCName { get; set; }
        public Func<object, object[], object> Func { get; set; }
        public MethodInfo MethodInfo { get; set; }
        public ReplacementRPCAttribute(string rpcName)
        {
            RPCName = rpcName;
        }
        public ReplacementRPCAttribute(Type type, string rpcName)
        {
            Type = type;
            RPCName = rpcName;
        }
        public void Initialize(MethodInfo method)
        {
            MethodInfo = method;
            ReplacementRPCName = method.Name;
            if (Type == null)
            {
                Type = method.DeclaringType;
                Func = method.Invoke;
            }
            else
            {
                Func = (object caller, object[] parameters) => method.Invoke(null, caller.SingleItemAsEnumerable().Concat(parameters).ToArray());
            }
        }
    }
}
