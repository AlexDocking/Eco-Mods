using Eco.Shared.Utils;
using System;
using System.Linq;
using System.Reflection;

namespace ReplacementInteractions
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class ReplacementRPCAttribute : Attribute
    {
        public Type Type { get; }
        public string RPCName { get; }
        public string ReplacementRPCName { get; set; }
        public Func<object, object[], object> Func { get; set; }
        public MethodInfo MethodInfo { get; set; }
        public ReplacementRPCAttribute(Type type, string rpcName)
        {
            Type = type;
            RPCName = rpcName;
        }
        public void Initialize(MethodInfo method)
        {
            MethodInfo = method;
            ReplacementRPCName = method.Name;
            Func = (object caller, object[] parameters) => method.Invoke(null, caller.SingleItemAsEnumerable().Concat(parameters).ToArray());
        }
    }
}
