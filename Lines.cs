using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace MeFastTextBox
{
    /// <summary>
    /// слово в тексте
    /// </summary>
    class Word
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="text">контекст</param>
        public Word(string text = default(string))
        {
            Text = text;
        }

        public int Start { get; set; }
        public int End { get; set; }
        private string Text { get; }
        /// <summary>
        /// текущая позиция курсора в слове
        /// </summary>
        public int Position { get; set; }
        /// <summary>
        /// длина слова
        /// </summary>
        public int Length => End - Start;

        public string GetValue()
        {
            if (string.IsNullOrEmpty(Text)) throw new Exception("Текущий экземпляер не содержит контекста"); 
            else return Text.GetRange(Start, End);
        }

        public string GetValue(string context)
        {
            return context.GetRange(Start, End);
        }

        public string GetValue(NeoRTB context)
        {
            return context.Text.GetRange(Start, End);
        }

        public string MakeWord(char KeyChar, string text)
        {
            if (Char.IsPunctuation(KeyChar)) return "";

            string keyword = this.GetValue(text);
            if (KeyChar > 8)                       //если это не 7 и не 8
            {
                keyword = keyword.Insert(this.Position, KeyChar.ToString()); //мб еще 7 - это доп вирт флаг Delete
            }
            else if (KeyChar == (char)7)           //7 == delete
            {
                if (this.Position < keyword.Length) keyword = keyword.Remove(this.Position, 1);
            }

            return keyword;
        }


    }

    //каждая линия - класс

    //каждая строка без отступа - класс

    //класс, базовый для всех линий
    internal abstract class BaseClass
    {
        internal string Content { get; set; }

        protected SmartDictionary smd;

        /// <summary>
        /// возвращает список текстовых координат для каждого цвета
        /// </summary>
        /// <param name="smd"></param>
        /// <returns></returns>
        internal Dictionary<Color, List<KeyValuePair<int, int>>> HighLightList()
        {
            var matches = new Regex(@"\w+").Matches(Content);
            var vals = new Dictionary<Color, List<KeyValuePair<int, int>>>();

            for (int i = 0; i < matches.Count; i++)
            {
                Color cw = smd[matches[i].Value];
                if (cw != Color.Black)
                {
                    if (!vals.ContainsKey(cw))
                    {
                        vals.Add(cw,
                            new List<KeyValuePair<int, int>>
                            {
                                    new KeyValuePair<int, int>(
                                        matches[i].Index,
                                        matches[i].Length)
                            }
                        );
                    }
                    else
                    {
                        vals[cw].Add(new KeyValuePair<int, int>(
                                    matches[i].Index,
                                    matches[i].Length));
                    }


                }
            }

            return vals;
        }
    }

    /// <summary>
    /// класс строки
    /// </summary>
    internal class Line : BaseClass
    {
        //string Content;

        internal Line(string content, Block father)
        {
            base.smd = father.parent.SmaDict;

            Content = content;
            Parent = father;
        }



        int Indent { get; set; }

        int Num { get; set; }

        Block Parent { get; set; }
    }
}
