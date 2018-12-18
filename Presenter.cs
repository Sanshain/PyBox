using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MeFastTextBox
{
    class Presenter
    {

    }

    class KeyPresenter : Presenter
    {
        public static NeoRTB FatBox { get; set; }

        /// <summary>
        /// обрабатывает KeyPress
        /// </summary>
        /// <param name="e">символ</param>
        /// <returns>false, если оставляем все как есть</returns>
        public static bool Check(char e)
        {
            
            if (Char.IsDigit(e)) return false;                                      //если число, возващаем false
                                                                                    
            if (Char.IsWhiteSpace(e) | Char.IsPunctuation(e)) return false;         //если пробел или знак пунктуации, возващаем false                           
            //Проверить: надо проверить оба слова, которые образовались в результате разделения

            //если длина слова больше
            if (FatBox.SelectionLength > 0) return false;

            return true;
        }

    }
}
