using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;


namespace MeFastTextBox
{
    public partial class MainTestForm : Form
    {
        public MainTestForm()
        {
            InitializeComponent();
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);            
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {
            //при любом изменении текста, изменяем цвет текста 
            //HighLightAll();
            //нет, переделал на keydown

            //подсветка осуществляется в каждом блоке

            //сначала разделяем все на слова: 
            
        }

        private void HighLightAll()
        {
            var start = FatBox.SelectionStart;

            /*
            FatBox.SelectionStart = 0;
            FatBox.SelectionLength = FatBox.Text.Length;
            FatBox.SelectionColor = Color.Black;//*/

            //список зарезервированных языком слов сначала строки: 
            string[] keyword = { "class", "def", "for", "while", "if" };

            Highlights(keyword, Color.Blue, after: " ");


            //список зарезервированных языком слов в середине строки: 
            string[] middleKeywords = { "not", "is", "return" };   //голубой

            Highlights(middleKeywords, Color.Blue, " ", " ");

            FatBox.SelectionStart = start;

            //FatBox.SelectionBackColor = Color.win
            FatBox.SelectionColor = Color.Black;
        }

        readonly char[] seps = { '"', '\'' };

        private void FatBox_KeyDown(object sender, KeyEventArgs e)
        {
            switch(e.KeyCode)
            {
                //удаление символа:
                case Keys.Delete:

                    FatBox_KeyPress(sender, new KeyPressEventArgs((char)7));
                    break;

                //перенос строки:
                case Keys.Enter:

                    string[] enterwords = { "class", "def", "for", "if", "while"};
                    var idl = FatBox.GetLineFromCharIndex(FatBox.SelectionStart);
                    string line = FatBox.Lines[idl];

                    if (!LineIsComment(line))
                    {
                        List<int> o = new List<int>();
                        for (int i = 0; i < line.Length; i++)
                        {
                            if (line[i] == '(') o.Add(i);
                        }

                        if (line.Count(c => c == '(') == line.Count(c => c == ')'))
                        {
                            if (o.All(c => !IsInConon(c)))
                            {
                                FatBox.SelectionStart+=1;//2
                            }
                        }
                    }

                    //получаем отступ текущей строки:
                    int cIndent = GetIndent(line);

                    //если это первая строка, то просто добавляем
                    if (idl == 0)
                        { e.Handled = true; TabAppend(enterwords, line, cIndent); break; }

                    //если нет, то получаем отступ предыдущей строки:
                    int eIndent = GetIndent(FatBox.Lines[idl - 1]);

                    //проверяем, есть ли содержание в текущей строке
                    if (line.Trim().Length > 0)
                    {
                        e.Handled = true;
                        //значит применяем такой же отступ + таб в случае ключевого слова
                        TabAppend(enterwords, line, cIndent, eIndent);
                    }
                    //если нет, начинаем с новой строки
                    else {
                        e.Handled = true;
                        TabAppend(enterwords, line, cIndent-1, eIndent);
                        //TabAppend(e, enterwords, line, cIndent, eIndent);
                    }


                    //*/
                    break;
            }


        }

        bool LineIsComment(string line)
        {
            return line.IndexOf("\"\"\"") >= 0 | line.Contains('#');
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="enterwords">ключевые слова, которые добавляют отступ</param>
        /// <param name="line">текущая стрка</param>
        /// <param name="cIndent">отступ предыдущей строки</param>
        /// <param name="r">применяется при возврате к предыдущему отступу</param>
        private void TabAppend(string[] enterwords, string line, int cIndent, int r = 0)
        {
            var wrd = enterwords.SingleOrDefault(w => line.Trim().StartsWith(w));

            var tab = enterwords.Any(w => line.Trim().StartsWith(w));
            
            //если нет ключевых слов и нет отступа. то //здесь как раз таки r пригодится
            if (!tab & (r +cIndent) == 0)
            {
                return;
            }

            int sp = FatBox.SelectionStart;

            /*
            
            FatBox.SelectionLength = FatBox.TextLength - FatBox.SelectionStart; //aptext.Length; //
            var aptext = FatBox.SelectedRtf.Substring(FatBox.SelectionStart);           //text

            FatBox.Cut();//*/

            var aptext = FatBox.Text.Substring(FatBox.SelectionStart);           //text
            FatBox.SelectionLength = FatBox.TextLength - FatBox.SelectionStart; //aptext.Length; //
            var apRtf = FatBox.SelectedRtf;
            FatBox.SelectedText = "";//*/

            var colon = "";
            if (!line.TrimEnd().EndsWith(":") && tab)
            {
                colon = ":";
                sp ++;
                if(wrd == "def")
                {
                    if (!line.Contains('('))
                    {
                        colon = "()" + colon;
                        sp += 2;
                    }
                    /*//работает, но попробуем и без нее, т.к. в идеале скобки будут закрываться автоматически
                    else if (!line.Contains(')'))
                    {
                        colon = ")" + colon;
                        sp += 1;
                    }
                    //*/
                }

            }
            else if(line.IndexOf("\"\"\"") < 0 && !line.Contains('#'))
            {

                if (line.Count(c => c == '\"') % 2 == 1)
                {
                    colon = "\"";
                    sp++;
                }
                else if (line.Count(c => c == '\'') % 2 == 1)
                {
                    colon = "\'";
                    sp++;
                }
            }



            var tabcount = Convert.ToInt32(tab) + cIndent;
            
            if (FatBox.Text.Last() == '\n')
            {
                
                FatBox.AppendText(colon + new string('\t', tabcount) + Environment.NewLine + aptext);/*
                FatBox.Paste();
                Clipboard.Clear();
                FatBox.SelectedRtf = colon + new string('\t', tabcount) + Environment.NewLine + aptext;//*/

                FatBox.SelectionStart = sp + tabcount;

                FatBox.SelectionStart += 1;
                FatBox.SelectionLength = FatBox.Text.Length - FatBox.SelectionStart;
                FatBox.SelectedRtf = apRtf;

                FatBox.SelectionStart = sp + tabcount;
            }
            else//*/
            {
                /*
                FatBox.AppendText(colon + Environment.NewLine + new string('\t', tabcount));
                FatBox.Paste();
                Clipboard.Clear();//*/
                
                FatBox.AppendText(colon + Environment.NewLine + new string('\t', tabcount) + aptext);
                //*/
                FatBox.SelectionStart = sp + 1 + tabcount;//
            }

        }

        /// <summary>
        /// отступ в табах
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        private static int GetIndent(string line)
        {
            int cIndent = 0;

            for (int i = 0; i < line.Length; i++)
            {
                if (line[i] == '\t') cIndent++;
                else break;
            }

            return cIndent;
        }






        private void MainTestForm_Load(object sender, EventArgs e)
        {
            //richTextBox1_TextChanged(null, null);
            FatBox.SelectionTabs = new int[] { 12, 24, 36, 48 };//1 пробел = 3

            HighLightAll();
        }

        /// <summary>
        /// подсветки при запуске
        /// </summary>
        /// <param name="keyword"></param>
        /// <param name="cl"></param>
        /// <param name="before"></param>
        /// <param name="after"></param>
        private void Highlights(string[] keyword, Color cl, string before = "", string after = "")
        {              

            foreach (var word in keyword)
            {
                //ищем все вхождения слова в текст: 
                Regex rg = new Regex(before + word + after);
                var matches = rg.Matches(FatBox.Text);

                for (int i = 0; i < matches.Count; i++)
                {
                    FatBox.SelectionStart = matches[i].Index;
                    FatBox.SelectionLength = matches[i].Length;//-1 - убирать пробел в конце?                    
                    FatBox.SelectionColor = cl;                   
                    FatBox.SelectionLength = 0;
                }
            }
        }

        /// <summary>
        /// создает дерево объектов
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            var code = new Blocks(FatBox.Text);
            foreach (var item in code)
            {
                listBox1.Nodes.Add(item.Name);                

                foreach (var func in item.CodeBlocks)
                {
                    listBox1.Nodes[listBox1.Nodes.Count - 1].Nodes.Add(
                        new TreeNode(func.Name) { Tag = func.LineStart }
                    );
                    //listBox1.Items.Add("    " + func.Name);                    
                }

                listBox1.Nodes[listBox1.Nodes.Count - 1].Expand();
                listBox1.Nodes[listBox1.Nodes.Count - 1].ForeColor = Color.PaleVioletRed;
            }
        }





        private void listBox1_DoubleClick(object sender, EventArgs e)
        {
            if (listBox1.SelectedNode.Tag == null) return;

            var lines = FatBox.Text.Split('\n').ToList().GetRange(0, (int)listBox1.SelectedNode.Tag);

            var pos = 0;
            for (int i = 0; i < lines.Count; i++)
            {
                pos += lines[i].Length+1;
            }

            FatBox.Focus();

            FatBox.SelectionStart = pos;

            FatBox.Select();
        }
      
        /// <summary>
        /// получает введенный char-символ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FatBox_KeyPress(object sender, KeyPressEventArgs e)
        {

            KeyPresenter.FatBox = FatBox;

            if (KeyPresenter.Check(e.KeyChar) == false) return;
            

            var text = FatBox.Text;

            //при нажатии клавиши
            //1 определяем позицию нажатия: 
            var startChanges = FatBox.SelectionStart;
            //2 определяем конец выражения. изменения
            var closeChanges = startChanges;

            //3 находим смежные слова:

            var word = GetWord(text, FatBox.SelectionStart);
            int locStart = word.Position;
            startChanges = word.Start;
            closeChanges = word.End;

            //3.3 изменяем слово:
            string keyword = MakeWord(e.KeyChar, text, word);

            //показываем измененное слово:
            status.Text = keyword;
            //MessageBox.Show(keyword); //- убедились, что получили это слово

            //4.0 Задаем ключевые слова:
            string[] keywords = { "class", "def", "for", "while", "if" };   //blue


            //4.1 Если текущее слово совпадает с ключевым, то меняем цвет:                   // тестируем способ с кейвордс:
            if(IsInConon())
            {
                SetStyle(startChanges, keyword, Color.Brown);
            }
            else if (keywords.Contains(keyword))
            {
                SetStyle(startChanges, keyword, Color.Blue);
            }             
            else SetStyle(startChanges, keyword, Color.Black);

            //при нажатии любой клавиши, автокомплит

            //при нажатии Enter, выравнивание

            /*
            if (e.KeyChar == '\t')
            {
                var start = FatBox.SelectionStart;
                //FatBox.SelectionStart = 0;
                //FatBox.SelectionColor = Color.Black;

                FatBox.Text = FatBox.Text.Insert(FatBox.SelectionStart, "    ");
                e.Handled = true;

                FatBox.SelectionStart = start + 4;                
            }//*/
        }



        bool IsInConon(int locpos = 0, string line = "")
        {
            if (FatBox.Text.Length == 0) return false;

            line = FatBox.Lines[FatBox.GetLineFromCharIndex(FatBox.SelectionStart)];

            //определяем, есть ли кавычки в строке: 
            if (!line.Contains('"') && !line.Contains('\'')) return false;

            if (locpos == 0) locpos = FatBox.SelectionStart - (FatBox.TextLength - line.Length);
            List<int> conons = new List<int>();
            //определяем диапазоны кавычек:
            for (int i = 0; i < line.Length; i++)
            {
                if (line[i] == '\"')
                {
                    conons.Add(i);
                }
            }

            for (int i = 0; i < conons.Count; i++)
            {
                if (conons.Count == i + 1 & conons[i] < locpos) return true;     //открытые скобки
                if (conons[i] < locpos & conons[++i] >= locpos) return true;
                else continue;
            }

            return false;
        }

        private static Word GetWord(string text, int pos)
        {
            int startChanges = pos;
            int closeChanges = startChanges;

            //3.1 идем сначала в направлении назад к началу, пока не найдем знак пунктуации или пробельный символ
            int locStart = 0;
            for (int i = startChanges - 1; i >= 0; i--)
            {
                if (Char.IsWhiteSpace(text[i]) | Char.IsPunctuation(text[i]) | i == 0)
                {
                    locStart = startChanges - i - 1;        //позиция относительно текущего значения картетки -1
                    startChanges = i + 1;                   //позиция относительно начала документа           +1
                    break;
                }
            }
            //таким образом нашли номер символа начала слова - startChanges

            //3.2 идем в направлении к концу в поисках пробела или знака пунктуации
            for (int i = closeChanges; i < text.Length; i++)
            {
                if (Char.IsWhiteSpace(text[i]) | Char.IsPunctuation(text[i]) | text.Length == i + 1)
                {
                    closeChanges = i; break;
                }
            }
            //closeChanges - конец слова
            return new Word() { Start = startChanges, End = closeChanges, Position = locStart };
        }

        private void SetStyle(int startChanges, string keyword, Color cl)
        {
            if (keyword.Length == 0) return;

            var sts = FatBox.SelectionStart;

            FatBox.SelectionStart = startChanges;
            FatBox.SelectionLength = keyword.Length-1;
            FatBox.SelectionColor = cl;

            //возвращаем каретку выделения на исходное состояние
            FatBox.SelectionStart = sts;

            FatBox.SelectionLength = 0;
        }

        /// <summary>
        /// Добавляем к слову введенный символ
        /// либо удаляем текущий символ при нажатии del
        /// </summary>
        /// <param name="KeyChar">нажатая буква</param>
        /// <param name="text">сам текст</param>
        /// <param name="startChanges">номер первого символа слова</param>
        /// <param name="closeChanges">номер последнего символа слова</param>
        /// <param name="locStart">позиция изменения</param>
        /// <returns></returns>
        private string MakeWord(char KeyChar, string text, Word word)
        {
            string keyword = text.GetRange(word.Start, word.End);
            if (KeyChar > 8 /*!= '\b'*/ )         //если это не 7 и не 8
            {
                keyword = keyword.Insert(word.Position, KeyChar.ToString()); //мб еще 7 - это доп вирт флаг Delete
            }
            else if (KeyChar == (char)7)           //7 == delete
            {
                if(word.Position < keyword.Length) keyword = keyword.Remove(word.Position, 1);
            }
            else if(KeyChar == (char)8)             //'\b'
            {
                //nothing
            }

            return keyword;
        }
    }
}
