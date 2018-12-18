using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using static MeFastTextBox.MainTestForm;

namespace MeFastTextBox
{
    /// <summary>
    /// блок кода
    /// </summary>
    class Block
    {
        /// <summary>
        /// одна на время работы программы
        /// </summary>
        internal Blocks parent;

        public ObservableCollection<Line> Lines;
        public string Name
        {
            get
            {
                if (Head.Content.Length == 0) return "";

                var ws = Head.Content.Split(' ');
                if (ws.Length == 1) return "";

                var r = ws[1];
                if (r.Contains("(") & r.Contains(")"))
                {
                    return r;
                }
                else
                {
                    return r.Split('(').First() + " (" + Head.Content.Split(',').Length + " args)";
                }
            }
        }

        public enum TypeBlock
        {

        }

        /// <summary>
        /// позиция char
        /// </summary>
        public int Position { get; set; }

        /// <summary>
        /// номер строки
        /// </summary>
        public int LineStart { get; set; }

        [Obsolete]
        public Block(string text)
        {
            Lines = new ObservableCollection<Line>(text.Split('\n').Select(s => new Line(s, this)));
        }

        /// <summary>
        /// внутреннее содержимое блока
        /// </summary>
        private string[] content;

        /// <summary>
        /// обзаглавие блока
        /// </summary>
        internal Line Head { get; set; }

        /// <summary>
        /// внутренние блоки/классы/функции
        /// </summary>
        public Blocks CodeBlocks;

        /// <summary>
        /// конструктор
        /// </summary>
        /// <param name="lines"></param>
        public Block(IEnumerable<string> lines, int posLine, int position, Blocks parent_dict)
        {
            LineStart = posLine;
            parent = parent_dict;
            Position = position;

            Lines = new ObservableCollection<Line>(lines.Select(s => new Line(s, this)));

            content = new string[lines.Count() - 1];
            Head = new Line(lines.First(), this);

            int i = 0;
            foreach (var line in lines)
            {
                if (string.IsNullOrEmpty(line)) continue;
                else if (line.StartsWith("    ")) content[i++] = line.Substring(4);
                else if (line.Length < 5) continue;             //для точного счета позиции здесь надо сократить все пустые строки в ртб
                else if (line.StartsWith('\t'.ToString())) content[i++] = line.Substring(1);
                else if (i == 0) continue;
                else throw new Exception("что за строка?");
            }

            CodeBlocks = new Blocks(content.ToList(), posLine);
        }


    }

    class Blocks : List<Block>
    {


        [Obsolete]
        public Blocks(string code)
        {
            InitializeDictByColored();

            var lines = code.Split('\n').ToList();
            int? startblock = null;
            int pos = 0;

            for (int i = 0; i < lines.Count; i++)
            {
                if (lines[i].StartsWith(" ") | lines[i].StartsWith("\t"))   //содержимое
                {
                    if (i == lines.Count - 1)
                    {                        

                        this.Add(new Block(
                            lines.GetRange(startblock.Value, i - startblock.Value + 1),
                            startblock.Value,
                            pos,
                            this));
                    }
                    continue;
                }

                //начало нового блока                        
                if (startblock.HasValue)
                {
                    this.Add(new Block(lines.GetRange(startblock.Value, i - startblock.Value), 
                        startblock.Value, 
                        pos,
                        this));
                }

                startblock = i;

            }
        }

        private void InitializeDictByColored()
        {
            SmaDict = new SmartDictionary();
            SmaDict.SmaDictionary = new Dictionary<System.Drawing.Color, string[]>();
            SmaDict.Add(new string[] { "def", "class" }, System.Drawing.Color.Blue);
        }

        public Blocks(List<string> lines, int pos_line = 0, int pos = 0)
        {
            InitializeDictByColored();

            int? startblock = null;
            var comsum = 0;

            for (int i = 0; i < lines.Count; i++)
            {
                if (lines[i] != null) comsum += lines[i].Length;
                comsum += 1 + 4;
                if (lines[i] == null)
                {
                    if (i == lines.Count - 1)
                    {
                        if (startblock.HasValue)
                            this.Add(new Block(lines.GetRange(startblock.Value, i - startblock.Value + 1), 
                                startblock.Value + pos_line, 
                                pos,
                                this));
                        else continue;
                    }
                    continue;
                }
                if (lines[i].StartsWith(" ") | lines[i].StartsWith("\t"))   //содержимое
                {
                    if (i == lines.Count - 1)
                    {
                        this.Add(new Block(lines.GetRange(startblock.Value, i - startblock.Value + 1),
                            startblock.Value + pos_line,
                            pos,
                            this));
                    }
                    continue;
                }
                else if (lines[i].StartsWith("class") | lines[i].StartsWith("def")) //начало нового блока
                {
                    if (!startblock.HasValue)
                    {
                        startblock = i;
                        //pos += comsum;// - lines[i].Length;
                    }
                    else
                    {                        

                        this.Add(new Block(
                            lines.GetRange(startblock.Value, i - startblock.Value),
                            startblock.Value + pos_line,
                            pos,
                            this));

                        startblock = i;
                    }
                }
                else
                {
                    pos_line++;    //комментарий
                }
            }            
        }


        internal SmartDictionary SmaDict { get; set; }
    }

}
