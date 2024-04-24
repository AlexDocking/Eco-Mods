﻿//XP Benefits
//Copyright (C) 2023 Alex Docking
//
//This program is free software: you can redistribute it and/or modify
//it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or
//(at your option) any later version.
//
//This program is distributed in the hope that it will be useful,
//but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//GNU General Public License for more details.
//
//You should have received a copy of the GNU General Public License
//along with this program.  If not, see <http://www.gnu.org/licenses/>.
using Eco.Core.Utils;
using Eco.Shared.Utils;
using System.Collections;
using System.Collections.Generic;

namespace CompatibleTools
{
    public class PriorityDynamicValueResolver : ICollection<IPriorityModifyInPlaceDynamicValueHandler>
    {
        public int Count => RequestHandlers.Count;
        public bool IsReadOnly => RequestHandlers.IsReadOnly;
        private ThreadSafeList<IPriorityModifyInPlaceDynamicValueHandler> RequestHandlers { get; } = new ThreadSafeList<IPriorityModifyInPlaceDynamicValueHandler>();
        private IComparer<IPriorityModifyInPlaceDynamicValueHandler> Comparer { get; } = new NumberComparer<IPriorityModifyInPlaceDynamicValueHandler>(handler => handler.Priority);
        public PriorityDynamicValueResolver(params IPriorityModifyInPlaceDynamicValueHandler[] requestHandlers)
        {
            foreach(var handler in requestHandlers) Add(handler);
        }
        public PriorityDynamicValueResolver() { }
        public void Add(IPriorityModifyInPlaceDynamicValueHandler item)
        {
            lock (RequestHandlers)
            {
                RequestHandlers.Add(item);
                RequestHandlers.Sort(Comparer);
            }
        }

        public void Clear()
        {
            RequestHandlers.Clear();
        }

        public bool Contains(IPriorityModifyInPlaceDynamicValueHandler item)
        {
            return RequestHandlers.Contains(item);
        }

        public void CopyTo(IPriorityModifyInPlaceDynamicValueHandler[] array, int arrayIndex)
        {
            RequestHandlers.CopyTo(array, arrayIndex);
        }

        public IEnumerator<IPriorityModifyInPlaceDynamicValueHandler> GetEnumerator()
        {
            return RequestHandlers.GetEnumerator();
        }

        public bool Remove(IPriorityModifyInPlaceDynamicValueHandler item)
        {
            return RequestHandlers.Remove(item);
        }

        public float Resolve(IModifyInPlaceDynamicValueContext context)
        {
            PassThroughHandlers(context);
            return context.FloatValue;
        }
        public int ResolveInt(IModifyInPlaceDynamicValueContext context)
        {
            PassThroughHandlers(context);
            return context.IntValue;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)RequestHandlers).GetEnumerator();
        }

        private void PassThroughHandlers(IModifyInPlaceDynamicValueContext context)
        {
            foreach (var handler in RequestHandlers.ToList())
            {
                Log.WriteLine(Eco.Shared.Localization.Localizer.Do($"Process {context.GetType()} with {handler.GetType()}:{context.FloatValue}f,{context.IntValue}"));

                handler.ModifyValue(context);
            }
            Log.WriteLine(Eco.Shared.Localization.Localizer.Do($"Evaluate {context.GetType()}:{context.FloatValue}f,{context.IntValue}"));
        }
    }
}
