using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace MeFastTextBox
{
    static class Extension
    {
        /// <summary>
        /// десериализация класса
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static T Deserialize<T>(this T obj) where T : class, new()
        {
            if (obj == null) obj = new T();

            string name = obj.GetType().Name + "cfg";

            if (!File.Exists(name)) return obj;

            using (FileStream fs = new FileStream(name, FileMode.Open))
            {
                obj = new BinaryFormatter().Deserialize(fs) as T;
            }

            return obj ?? new T();
        }


        public static void Serialize<T>(this T obj) where T : class, new()
        {
            string name = obj.GetType().Name + "cfg";

            using (FileStream fs = new FileStream(name, FileMode.OpenOrCreate))
            {
                new BinaryFormatter().Serialize(fs, obj);
            }
            
        }

        /// <summary>
        /// возвращает подстроку из строки в указанном диапазоне
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public static string GetRange(this string obj, int start, int end)
        {
            if (end >= obj.Length) end = obj.Length;// - 1;
            if (end < start) start = end;
            return obj.Substring(start, end - start);
        }
    }
}
