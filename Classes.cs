using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace MeFastTextBox
{
    //  
    /// <summary>
    /// класс специального словаря/ слово-цвет /цвет-массив слов
    /// </summary>
    class SmartDictionary
    {
        /// <summary>
        /// ассоциация цветов и типов
        /// </summary>
        /// <param name=""></param>
        /// <param name=""></param>

        [Obsolete]
        public void Add(string[] words, Color type)
        {
            if (!SmaDictionary.ContainsKey(type)) SmaDictionary.Add(type, words);
            else SmaDictionary[type] = SmaDictionary[type].Concat(words).ToArray();


            foreach (var the_color in dictionary.Keys)
            {
                string[] wrds = dictionary[the_color];
                for (int i = 0; i < words.Length; i++)
                {
                    ColorDict.Add(wrds[i], the_color);
                }
            }
        }

        Dictionary<Color, string[]> dictionary;
        /// <summary>
        /// для компактного хранения. Хранится в настройках. Только для сохранения
        /// </summary>
        public Dictionary<Color, string[]> SmaDictionary
        {
            set
            {
                dictionary = value;
                dictionary.Serialize();

                foreach (var the_color in dictionary.Keys)
                {
                    string[] words = dictionary[the_color];
                    for (int i = 0; i < words.Length; i++)
                    {
                        ColorDict.Add(words[i], the_color);
                    }
                }
            }
            get
            {
                return dictionary;
            }
        }

        /// <summary>
        /// для быстрой работы
        /// </summary>            
        Dictionary<string, Color> ColorDict;

        public SmartDictionary()
        {
            //десериализуем dictionary
            dictionary = new Dictionary<Color, string[]>();
            dictionary.Deserialize();


            //преобразуем его в ColorDict
            ColorDict = new Dictionary<string, Color>();

            foreach (var the_color in dictionary.Keys)
            {
                string[] words = dictionary[the_color];
                for (int i = 0; i < words.Length; i++)
                {
                    ColorDict.Add(words[i], the_color);
                }
            }

        }

        public Color this[string word]
        {
            get
            {
                return ColorDict.ContainsKey(word) ? ColorDict[word] : Color.Black;
            }
        }



    }
}
