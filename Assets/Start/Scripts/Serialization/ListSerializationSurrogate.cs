using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Start.Scripts.Serialization
{
    public class ListSerializationSurrogate<T> : ISerializationSurrogate
    {
        public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
        {
            List<T> list = (List<T>)obj;
            info.AddValue("Count", list.Count);
            for (int i = 0; i < list.Count; i++)
            {
                info.AddValue(i.ToString(), list[i]);
            }
        }

        public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
        {
            List<T> list = new List<T>();
            int count = info.GetInt32("Count");
            for (int i = 0; i < count; i++)
            {
                T item = (T)info.GetValue(i.ToString(), typeof(T));
                list.Add(item);
            }
            return list;
        }
    }
}