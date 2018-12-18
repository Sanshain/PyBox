using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using System.Runtime.InteropServices;
using System.Data;
using System.Text.RegularExpressions;

namespace MeFastTextBox
{


    partial class NeoRTB : RichTextBox
    {


        protected ListBox combobox;
        public List<string> KeyWords { get; set; }
        public List<string> GloBlocks { get; set; } = new List<string>();
        protected Stack<UndoMember> FatUndo = new Stack<UndoMember>();

        DOM orm = new DOM();

        public NeoRTB () 
        {
            this.SetStyle(
                System.Windows.Forms.ControlStyles.OptimizedDoubleBuffer,
                true);//*/

            combobox = new ListBox()
            {
                Visible = false
            };            
            this.Controls.Add(combobox);
            combobox.KeyDown += Combobox_KeyDown;
            combobox.KeyPress += Combobox_KeyPress;

            KeyWords = new List<string>() { "class", "def", "for", "while", "lambda", "self","pass", "super", "return" };
            SelectionTabs = new int[] { 12, 24, 36, 48 };//1 пробел = 3

        }



        private void Combobox_KeyPress(object sender, KeyPressEventArgs e)
        {
            //см OnKeyPress
            TransformList(sender,e);
        }

        private void TransformList(object sender, KeyPressEventArgs e)
        {
            int w = (int)(CreateGraphics().MeasureString(e.KeyChar.ToString(), Font).Width * 0.94);
            var val = GetWord().MakeWord(e.KeyChar, Text);

            if ((int)e.KeyChar > 8)
            {                
                var kw = KeyWords.Where(k => k.StartsWith(val)).ToArray();
                if (kw.Length > 0)
                {
                    var bs = new BindingSource(KeyWords.Where(k => k.StartsWith(val)), "");
                    //bs.Filter = GetWord().GetValue(this) + e.KeyChar.ToString() + "%";
                    combobox.DataSource = bs;
                    //(sender as ListBox).Left += w;
                    (sender as ListBox).Left = this.GetPositionFromCharIndex(SelectionStart).X + w;

                    InsertWord(e.KeyChar.ToString());//,1
                }
                else
                {
                    (sender as ListBox).DataSource = null;
                    combobox.Hide();
                    this.Focus();

                    if (e.KeyChar != '\r' && e.KeyChar != ' ')
                    {
                        SendKeys.Send(e.KeyChar.ToString());
                        SendKeys.Flush();
                    }
                }
                
            }
            else if ((int)e.KeyChar == 8)
            {
                //remove part of text:
                this.Focus();

                SendKeys.Send(e.KeyChar.ToString());
                SendKeys.Flush();

                if (val.Length > 2)
                {
                    (sender as ListBox).Focus();
                    (sender as ListBox).Left -= w;
                }
                else 
                {
                    (sender as ListBox).DataSource = null;
                    combobox.Hide();
                }

            }
        }

        private void Combobox_KeyDown(object sender, KeyEventArgs e)
        {
            var sen = sender as ListBox;
            switch (e.KeyCode)
            {
                case Keys.Space:
                case Keys.Enter:
                                        
                    InsertWord(sen.SelectedItem.ToString(), GetWord().Length);
                    sen.Hide();
                    this.Focus();
                    break;

                case Keys.Escape:

                    sen.Hide();
                    Focus();
                    break;
            }

            
        }

        /// <summary>
        /// убирает символ в позиции charposition
        /// </summary>
        /// <param name="charposition">позиция символа/слова</param>
        /// <param name="len"></param>
        private void DelWord(int charposition, int len = 1)
        {
            var sp = SelectionStart;
            var aptext = Text.Substring(++SelectionStart);    //сохранили текст после картетки            
            //SelectionStart -= len;                 //перешли в начало слова
            SelectionLength = TextLength - charposition+1;
            var rtf = SelectedRtf;
            SelectedText = "";                              //освободили место для вставки

            AppendText(aptext);
            SelectionStart = sp;
            SelectionLength = TextLength - sp;
            SelectedRtf = rtf;

            SelectionStart = sp;
        }

        /// <summary>
        /// вставляет слово в основной текст
        /// </summary>
        /// <param name="word">слво для вставки</param>
        /// <param name="charposition">позиция каретки в слове</param>
        private void InsertWord(string word, int charposition = 0)
        {

            int sp = SelectionStart;                        //сохранили каретку
            var aptext = Text.Substring(SelectionStart);    //сохранили текст после картетки
            SelectionStart -= charposition;                 //перешли в начало слова
            SelectionLength = aptext.Length + charposition;
            SelectedText = "";                              //освободили место для вставки

            if (charposition != 0)
            {
                string splistring = " ";
                if (word.Last() == ')')                                     //если заканчивается скобокой,
                {
                    sp -= 2;                                                //установим фокус внутрь скобки
                    splistring = "";
                }
                AppendText(string.Join(splistring, word, aptext));          //если слово, добавляем пробел в конце
            }
            else AppendText(string.Join("", word, aptext));                 //если символ, то только символ
            SelectionStart = sp + word.Length + 1 - charposition;

            //добавляем цвет, если это ключевое
            if (KeyWords.Any(w => w.Equals(word)))
            {
                SelectionStart -= word.Length + 1;
                SelectionLength = word.Length;
                
                SelectionColor = System.Drawing.Color.Blue;
                
                SelectionLength = 0;
                SelectionStart += word.Length + 1;
            }
            
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            switch (e.KeyCode)
            {
                case Keys.Enter:

                    orm.Source = Text;
                    var cls = orm.Classes;
                    GloBlocks.Clear();
                    foreach (var cl in cls) GloBlocks.Add(cl.Name+"()");
                        
                    break;
            }            

        }

        /// <summary>
        /// возвращает текущую строку
        /// </summary>
        /// <returns></returns>
        protected string CurrentLine
        {
            get
            {
                return this.Lines[this.GetLineFromCharIndex(SelectionStart)];
            }
        }

        [Obsolete]
        struct CodeLine
        {
            public string Name;
            public int Position;
        }

        /// <summary>
        /// добавляет в строку символ на место текущей каретки, без сдвига каретки
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        protected string MakeLine(char c)
        {
            var line = CurrentLine;
            var posInLine = SelectionStart - this.GetFirstCharIndexOfCurrentLine();
            return line.Insert(posInLine, c.ToString());
        }        

        protected override void OnPreviewKeyDown(PreviewKeyDownEventArgs e)
        {
            base.OnPreviewKeyDown(e);
            if (e.KeyCode == Keys.Back)
            {
                if (Text.Length == SelectionStart) return;
                if (Text[SelectionStart-1] == '(' && Text[SelectionStart] == ')') DelWord(SelectionStart);                
            }
        }

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            base.OnKeyPress(e);

            if (e.KeyChar == '\b')
            {
                return;
            }
            if (ModifierKeys == Keys.Control) return;

            var word = GetWord();
            var value = word.MakeWord(e.KeyChar, Text);
            

            //если длина баольше 1, то проверяем на intellisense
            if (value.Length > 1)
            {

                var list = KeyWords.Where(w =>
                {
                    if (w.StartsWith(value))
                    {
                        if (w == "class" | w == "def")
                        {
                            var posInLine = SelectionStart - this.GetFirstCharIndexOfCurrentLine();
                            if (CurrentLine.Substring(0, posInLine - word.Length).Trim().Length > 0) return false; //чтобы это слово было первое в списке
                        }
                        return true;
                    }
                    return false;
                }).ToList();
                list.AddRange(GloBlocks.Where(w => w.ToLower().StartsWith(value) || w.StartsWith(value)));
                list.OrderBy(w => w);
                combobox.DataSource = list;
                //combobox.Items.AddRange(list);

                ShowSmartList(list);
                //else if (combobox.Visible) combobox.Hide(); - без него работает прекрасно

            }
            else if (value.Length == 1)
            {
                if (Text.Length == 0) return;

                var line = Lines[this.GetLineFromCharIndex(SelectionStart)].Trim();
                //сделать большую букву
            }
            else if(e.KeyChar == '.')
            {

                var incMethods = new List<string>();

                //определяем слово, после которого нажата точка:
                string tone = word.GetValue();
                if (tone != null)
                {
                    var obj = orm.GetClass(tone);
                    string[] listFuncs = obj.ListFuncs();

                    //ищем свойства и методы объекта
                    //var cls = orm.GetClass(SelectionStart);//<-неверно. надо определить класс ли тип, к которому применяется эта операция            

                    if (listFuncs.Length > 0) incMethods.AddRange(listFuncs);

                    string[] varList = obj.ListVars();

                    if (varList.Length > 0) incMethods.AddRange(varList);

                    if (incMethods.Count > 0)
                    {
                        incMethods.Sort();
                        combobox.DataSource = incMethods;
                        ShowSmartList(incMethods);
                    }
                }


            }


            var s = e.KeyChar;

            switch (e.KeyChar)
            {                

                case '\'':
                case '"':
                    
                    var line = Lines[this.GetLineFromCharIndex(SelectionStart)];
                    var cnt = (line.Count(c => c == s));
                    if (cnt % 2 == 0)
                    {
                        //InsertWord(s.ToString()+ s.ToString());

                        //e.Handled = true;

                        /*
                        SelectionStart-=2;                        
                        SelectionLength = 2;

                        SelectionColor = System.Drawing.Color.Brown;
                        
                        SelectionLength = 0;//

                        SelectionStart++;//*/
                        
                    }
                    break;
                case '[':
                case '{':
                case '(':
                    e.Handled = true;
                    
                    var chs = new char[] {
                        s,
                        (char)(Convert.ToInt32(s) + (s != '(' ? 2 : 1))};
                    InsertWord(new string(chs));

                    SelectionStart--;
                    break;
                             
            }
        }

        private void ShowSmartList(List<string> list)
        {
            if (list.Count > 0)
            {
                combobox.Location = this.GetPositionFromCharIndex(SelectionStart);
                combobox.Location = new System.Drawing.Point(combobox.Location.X + 8, combobox.Location.Y + 12);
                combobox.Show();
                combobox.Focus();
                combobox.SelectedIndex = 0;
            }
        }



        private Word GetWord()
        {
            int pos = SelectionStart;
            string text = Text;
            int startChanges = pos;
            int closeChanges = startChanges;

            //3.1 идем сначала в направлении назад к началу, пока не найдем знак пунктуации или пробельный символ
            int locStart = 0;
            for (int i = startChanges - 1; i >= 0; i--)
            {
                if (Char.IsWhiteSpace(text[i]) | Char.IsPunctuation(text[i]) | Char.IsSymbol(text[i]) | i == 0)
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
            return new Word(Text) { Start = startChanges, End = closeChanges, Position = locStart };
        }

        

    }






    partial class NeoRTB
    {
        public class UndoMember
        {
            //Stack<>
            public UndoMember(char action, int pos, int volume = 1)
            {
                Action = action;
                Position = pos;
                Volume = volume;
            }
            public char Action { get; set; }
            public int Position { get; set; }
            public int Volume;
        }








        interface CodeBlock
        {
            string Name { get; set; }
            int Position { get; set; }
        }

        enum Types
        {
            Bool,
            Int,            
            Float,
            Text,
            List,
            Tuple,
            Set,
            Dict,
            Date,
            UserType,
            VoidError
        }

        class Variable : CodeBlock
        {
            /// <summary>
            /// 
            /// </summary>
            /// <param name="pos">номер строки в родительском блоке</param>
            /// <param name="value">строка с переменной</param>
            public Variable(int pos, string value)
            {
                Position = pos;

                var arr = value.Split('=');
                Name = arr.First().Trim();                
                DetectType(arr[1]);
                
            }



            public string Name { get; set; }
            /// <summary>
            /// номер строки в теле функции/класса
            /// </summary>
            public int Position { get; set; }

            /// <summary>
            /// по значению по умолчанию определяет тип переменной
            /// </summary>
            /// <param name="value"></param>
            private void DetectType(string value)
            {
                if (value.Length == 0) Tone = Types.VoidError;
                else if (bool.TryParse(value.ToLower(), out bool r1)) Tone = Types.Bool;
                else if (int.TryParse(value, out int r2)) Tone = Types.Int;
                else if (double.TryParse(value, out double r3)) Tone = Types.Float;
                else if (DateTime.TryParse(value, out DateTime date)) Tone = Types.Date;
                else
                {
                    switch (value.First())
                    {
                        case '"':
                        case '\'':
                            Tone = Types.Text;
                            break;
                        case '[':
                            Tone = Types.List;
                            break;
                        case '(':
                            Tone = Types.Tuple;
                            break;
                        case '{':
                            if (value.Contains(':')) Tone = Types.Dict;
                            else Tone = Types.Set;
                            break;
                        default:
                            Tone = Types.UserType;
                            break;
                    }

                }
            }
            public Types Tone { get; private set; }
        }

        class Function : CodeBlock
        {

            /// <summary>
            /// 
            /// </summary>
            /// <param name="sour">тело метода</param>
            /// <param name="pos">позиция относительно родителя</param>
            public Function(string sour, int pos)
            {
                Source = sour;                        
                Position = pos;
            }

            /// <summary>
            /// получает из первой строки метода его имя
            /// </summary>
            public string Name {
                get
                {
                    var r = new Regex(@"def (?<name>\w+) *\(");
                    var m = r.Match(Lines.FirstOrDefault());
                    return m.Groups["name"].Value;
                }
                set
                {
                    throw new NotImplementedException();
                }
            }            

            /// <summary>
            /// определяет, является ли текущий метод статическим, т.е. не принимает в качестве параметра экз класса
            /// </summary>
            /// <returns>да, если не принимает</returns>
            public bool IsStatic()
            {
                if (this.Lines.FirstOrDefault().Contains("self"))
                {
                    return false;
                }
                return true;
            }

            [Obsolete]
            public string Self
            {
                get
                {
                    return this.IsStatic() ? null : "self";
                }
            }

            public Variable[] GetVariables()
            {
                localVars = new List<Variable>();
                


                return localVars.ToArray();
            }

            List<string> Lines
            {
                get => Source.Split('\n').ToList();
                set
                {
                    throw new NotImplementedException("присвоение-то понятно, а вот изменение элемента? + событие");
                }
            }
            List<Variable> localVars = new List<Variable>();
            List<string> parentVars = new List<string>();

            public string Source { get; set; }
            public int Position { get; set; }
            public int Length { get => Source.Length; }
        }        

        abstract class Instance : CodeBlock
        {
            public string Name { get; set; }
            public int Position { get; set; }
            
            public string Source { get; set; }

            protected string[] lines;

            public virtual List<Function> Methods { get; set; }
            public string[] ListFuncs(bool isInstance = false)
            {
                if (isInstance)
                {
                    var mts = Methods.Where(m => !m.IsStatic()).ToArray();
                    //var r = mts.Select(m => m.Name + "(self)").ToArray();
                    string[] rs = new string[mts.Count()];
                    for (int i = 0; i < mts.Count(); i++)
                    {
                        rs[i] = mts[i].Name + "(self)";
                    }
                    return rs;
                }
                else
                {
                    var mts = Methods.Where(m => m.IsStatic()).ToArray();
                    //var r = mts.Select(m => m.Name + "(self)").ToArray();
                    string[] rs = new string[mts.Count()];
                    for (int i = 0; i < mts.Count(); i++)
                    {
                        rs[i] = mts[i].Name + "()";
                    }
                    return rs;
                    //return Methods.Where(m => m.IsStatic()).Select(m => m.Name + "()").ToArray();
                }
            }
            public string[] ListVars()
            {
                return Variables.Select(v => v.Name).ToArray();
            }
            public virtual List<Function> DetectMethods() { throw new Exception("см в дочерних классах"); }
            public virtual List<Variable> DetectVars() { throw new Exception("см в дочерних классах"); }

            public List<Variable> Variables = new List<Variable>();

            public string[] GetLines()
            {
                lines = Source.Split('\n');
                return lines;
            }

            public int Size
            {
                get => Source.Length;
            }
        }

        class InstanceObject : Instance
        {
            public override List<Function> DetectMethods()
            {
                return base.DetectMethods();
                
            }

        }

        class InstanceClass : Instance
        {            

            /// <summary>
            /// 
            /// </summary>
            /// <param name="source">исходники</param>
            /// <param name="pos">начало блока в основном контексте</param>
            public InstanceClass(string source, int pos=0)
            {
                Source = source;
                Position = pos;
                var pat = @"class (?<name>\w+)";
                Name = new Regex(pat).Match(this.GetLines().First()).Groups["name"].Value;
                DetectMethods();
                DetectVars();
            }

            /// <summary>
            /// Все методы
            /// </summary>
            /// <returns></returns>
            public override List<Variable> DetectVars()
            {
                Variables = new List<Variable>();

                var lines = Source.Split('\n').ToList();
                List<int> vars = new List<int>(); //символа ползция относит начала
                int pos = 0;
                //ищем все функции в блоке кода первого уровня
                for (int i = 0; i < lines.Count; i++)
                {
                    if (new Regex(@"\w+\=\w").IsMatch(lines[i]))
                    {
                        vars.Add(i);
                    }

                }

                Variables.Clear();

                //создаем объекты типа класс в ДОМ
                for (int i = 0; i < vars.Count; i++)
                {

                    Variables.Add(new Variable(vars[i], lines[vars[i]]));
                    
                }

                return Variables;
            }

            public override List<Function> DetectMethods()
            {
                Methods = new List<Function>();

                var lines = Source.Split('\n').ToList();
                List<int> funcs = new List<int>(); //символа ползция относит начала
                int pos = 0;
                //ищем все функции в блоке кода первого уровня
                for (int i = 0; i < lines.Count; i++)
                {
                    if (new Regex("^(\t| +)def ").IsMatch(lines[i]))
                    {
                        funcs.Add(pos);
                    }
                    pos += lines[i].Length + 1;
                }

                if (Methods != null) Methods.Clear();

                //создаем объекты типа класс в ДОМ
                for (int i = 0; i < funcs.Count; i++)
                {
                    var end = i == funcs.Count - 1 ? this.Source.Length : funcs[i + 1];

                    Methods.Add(new Function(Source.GetRange(funcs[i], end), funcs[i]));

                }

                return Methods;
            }

            /// <summary>
            /// уровень вложенности
            /// </summary>
            [Obsolete("Уровень вложенности. Пока не используем. Используем только верхний уровень")]
            public byte Level { get; set; } = 0;
        }

        class DOM
        {
            string sour;
            /// <summary>
            /// инициализирует объекты ДОМ на основании нового исходного кода
            /// </summary>
            public string Source {
                get => sour;
                set
                {
                    sour = value;
                    GetClasses(sour);                    
                }
            }

            public DOM(){}

            public DOM(string source)
            {
                Source = source;
            }

            /// <summary>
            /// все статические методы - без класса, в теле пространства имен
            /// </summary>
            /// <param name="codeblock">код блока класса</param>
            /// <returns></returns>
            public Function[] GetMethods(string codeblock = null)
            {
                return null;
            }

            /// <summary>
            /// получаем список классов и записываем его в свойство Classes
            /// </summary>
            /// <param name="obj"></param>
            /// <returns></returns>
            protected List<InstanceClass> GetClasses(string obj)
            {
                var lines = Source.Split('\n').ToList();
                List<int> classes = new List<int>(); //символа ползция относит начала
                int pos = 0;
                //ищем все вхождения class
                for (int i = 0; i < lines.Count; i++)
                {
                    if (lines[i].StartsWith("class "))
                    {
                        classes.Add(pos);
                    }
                    pos += lines[i].Length + 1;
                }

                Classes.Clear();

                //создаем объекты типа класс в ДОМ
                for (int i = 0; i < classes.Count; i++)
                {
                    var end = i == classes.Count - 1 ? this.Source.Length : classes[i + 1];

                    Classes.Add(new InstanceClass(obj.GetRange(classes[i], end), classes[i]));
                }

                return Classes;
            }

            /// <summary>
            /// определяет класс, внутри которого находится указанный символ (как правило текущая позиция в тексте)
            /// </summary>
            /// <param name="pos">позиция символа</param>
            /// <returns></returns>
            public InstanceClass GetClass(int pos)
            {
                for (int i = 0; i < Classes.Count; i++)
                {
                    if (Classes[i].Position < pos && pos < Classes[i].Position + Classes[i].Size)
                        return Classes[i];
                }
                return null;
            }

            public InstanceClass GetClass(string name)
            {
                for (int i = 0; i < Classes.Count; i++)
                {
                    if (Classes[i].Name == name) return Classes[i];
                }
                return null;
            }

            public List<InstanceClass> Classes { get; set; } = new List<InstanceClass>();


        }
    }
}
