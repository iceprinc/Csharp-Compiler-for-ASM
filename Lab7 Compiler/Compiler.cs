//ЯЗЫК C#; VISUAL STUDIO 2019. Net Framework 4.7.2
//Приложение Windows Forms (.NET Framework)
//РЕАЛИЗОВАНЫ КОМАНДЫ dw db push pop mov (int 21h - выведет то что лежит в dl)
//ДЛЯ РАБОТЫ НА ФОРМУ НУЖНО ДОБАВИТЬ: 2 Button / и / 2 RichTextBox

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Windows.Forms;

namespace Lab7_Compiler
{
    public partial class Compiler : Form
    {
        //массивы для стека и для dw db
        public List<string> stack = new List<string>();
        public List<string> dwDannie = new List<string>();
        public List<string> dbDannie = new List<string>();


        public Compiler()
        {
            InitializeComponent();
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en");//для того что бы можно было складывать числа с плавающей точкой 10 + 0.5
        }

        private void button1_Click(object sender, EventArgs e)//обработка запуска кода
        {
            //перед началом очищаем массивы для данных и стека
            stack = new List<string>();
            dwDannie = new List<string>();
            dbDannie = new List<string>();

            //добавим стандартные переменные
            dw("ax dw 0");
            dw("bx dw 0");
            dw("cx dw 0");
            dw("dx dw 0");
            dw("al dw 0");
            dw("bl dw 0");
            dw("cl dw 0");
            dw("dl dw 0");
            push("push dl");

            //выведем дату запуска в окно вывода
            richTextBox2.Text = "Программа запущена: " + DateTime.Now + "\n\n";

            //текст в окне делим по новым строкам и заносим в массив
            string[] code = richTextBox1.Text.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);

            //цикл по всем строкам
            for (int i = 0; i < code.Length; i++)
            {
                //для избегания ошибок текст-> в нижний регистр
                string tempCode = code[i].ToLower();

                //проверяем действие в строке db dw push pop add

                //a dw 12
                if (tempCode.Contains(" dw "))
                {
                    dw(code[i]);
                }

                //a db 12
                if (tempCode.Contains(" db "))
                {
                    db(code[i]);
                }

                //push ax bx a b
                if (tempCode.Contains("push "))
                {
                    push(code[i]);
                }

                //pop b a bx ax
                if (tempCode.Contains("pop "))
                {
                    pop(code[i]);
                }

                //add ax,54 or add ax,bx
                if (tempCode.Contains("add "))
                {
                    add(code[i]);
                }

                //mov ax,54 or add ax,bx
                if (tempCode.Contains("mov "))
                {
                    mov(code[i]);
                }

                //int 21h
                if (tempCode == "int 21h")
                {
                    int21h();
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)//кнопка выхода
        {
            Close();//закрытие формы
        }

        public static bool IsDigitsOnly(string str)
        {
            foreach (char c in str)
            {
                if (c != '.' && c < '0' || c > '9')
                    return false;
            }

            return true;
        }

        public void dw(string code)
        {
            //каждрую строчку кода делим на ТРИ -  переменная . дейстиве . значение через ' 'пробел, пример (a dw 12)
            string[] param = code.Split(new char[] { ' ' }, 3, StringSplitOptions.RemoveEmptyEntries);

            //объявим флаг для ошибок
            bool error = false;

            //проверяем все строки в dw Данные на наличие там уже этой переменной
            for (int i = 0; i < dwDannie.Count; i++)
            {
                //для проверки делим каждый элемент на его название тип значение
                string[] temp = dwDannie[i].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                //если элемент с таким названием уже существует то выводим ошибку и этот элемент не создаем
                if (temp[0] == param[0])
                {
                    richTextBox2.Text += $"Ошибка при создании переменной с типом DW; Переменная с именем '{temp[0]}' уже объявленна. Невозможно повторно объявить '{param[0]}'\n";

                    error = true;
                }
            }

            //проверяем все строки в db Данные на наличие там уже этой переменной
            for (int i = 0; i < dbDannie.Count; i++)
            {
                //для проверки делим каждый элемент на его название тип значение
                string[] temp = dbDannie[i].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                //если элемент с таким названием уже существует то выводим ошибку и этот элемент не создаем
                if (temp[0] == param[0])
                {
                    richTextBox2.Text += $"Ошибка при создании переменной с типом DB; Переменная с именем '{temp[0]}' уже объявленна. Невозможно повторно объявить '{param[0]}'\n";

                    error = true;
                }
            }

            //если флаг для ошибок не сработал создаем переменную
            if (error == false)
            {
                //заносим в List предназначенный для dw Данные ( например. MyParam dw Hi)
                dwDannie.Add(param[0] + " dw " + param[2]);
            }

        }

        public void db(string code)
        {
            //каждрую строчку кода делим на ТРИ -  переменная . дейстиве . значение через ' ' - пробел, пример (a dw 12)
            string[] param = code.Split(new char[] { ' ' }, 3, StringSplitOptions.RemoveEmptyEntries);

            //объявим флаг для ошибок
            bool error = false;

            //проверяем все строки в dw Данные на наличие там уже этой переменной
            for (int i = 0; i < dwDannie.Count; i++)
            {
                //для проверки делим каждый элемент на его название и значение
                string[] temp = dwDannie[i].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                //если элемент с таким названием уже существует то выводим ошибку и этот элемент не создаем
                if (temp[0] == param[0])
                {
                    richTextBox2.Text += $"Ошибка при создании переменной с типом DW; Переменная с именем '{temp[0]}' уже объявленна. Невозможно повторно объявить '{param[0]}'\n";

                    error = true;
                }
            }

            //проверяем все строки в db Данные на наличие там уже этой переменной
            for (int i = 0; i < dbDannie.Count; i++)
            {
                //для проверки делим каждый элемент на его название и значение
                string[] temp = dbDannie[i].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                //если элемент с таким названием уже существует то выводим ошибку и этот элемент не создаем
                if (temp[0] == param[0])
                {
                    richTextBox2.Text += $"Ошибка при создании переменной с типом DB; Переменная с именем '{temp[0]}' уже объявленна. Невозможно повторно объявить '{param[0]}'\n";

                    error = true;
                }
            }

            //если флаг для ошибок не сработал создаем переменную
            if (error == false)
            {
                //заносим в List предназначенный для db Данные ( например. a db 12)
                dbDannie.Add(param[0] + " db " + param[2]);
            }

        }

        public void push(string code)
        {
            string[] param = code.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            //проверим все переменные на существование
            for (int j = 1/*начинаем с 1 т.к. в [0] лежит 'push'*/; j < param.Length; j++)
            {
                bool succesPush = false;

                //проверяем все строки в dw Данные на наличие там уже этой переменной
                for (int i = 0; i < dwDannie.Count; i++)
                {
                    //для проверки делим каждый элемент на его название и значение
                    string[] temp = dwDannie[i].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                    //если переменная существует то копируем её в стек
                    if (temp[0] == param[j])
                    {
                        stack.Add(dwDannie[i]);
                        succesPush = true;
                    }
                }

                //проверяем все строки в db Данные на наличие там уже этой переменной
                for (int i = 0; i < dbDannie.Count; i++)
                {
                    //для проверки делим каждый элемент на его название и значение
                    string[] temp = dbDannie[i].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                    //если переменная существует то копируем её в стек
                    if (temp[0] == param[j])
                    {
                        stack.Add(dbDannie[i]);
                        succesPush = true;
                    }
                }

                if (succesPush == false)
                {
                    richTextBox2.Text += $"Ошибка 'push'. Переменной '{param[j]}' не существует.\n";
                }
            }
        }

        public void pop(string code)
        {
            string[] param = code.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);



            //проверим все переменные на существование
            for (int j = 1/*начинаем с 1 т.к. в [0] лежит 'pop'*/; j < param.Length; j++)
            {
                string[] paramFromStak = stack[stack.Count - 1].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                //pop [j] переменная должна быть последней в стеке
                if (param[j] == paramFromStak[0])
                {
                    bool succesPop = false;

                    //проверяем все строки в dw Данные на наличие там уже этой переменной
                    for (int i = 0; i < dwDannie.Count; i++)
                    {
                        //для проверки делим каждый элемент на его название и значение
                        string[] temp = dwDannie[i].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                        //если переменная существует то копируем заменяем её значением из стека и убираем последнее значение из стека
                        if (temp[0] == param[j])
                        {
                            dwDannie[i] = stack[stack.Count - 1];
                            stack.Remove(stack[stack.Count - 1]);
                            succesPop = true;
                        }
                    }

                    //проверяем все строки в db Данные на наличие там уже этой переменной
                    for (int i = 0; i < dbDannie.Count; i++)
                    {
                        //для проверки делим каждый элемент на его название и значение
                        string[] temp = dbDannie[i].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                        //если переменная существует то копируем заменяем её значением из стека и убираем последнее значение из стека
                        if (temp[0] == param[j])
                        {
                            dbDannie[i] = stack[stack.Count - 1];
                            stack.Remove(stack[stack.Count - 1]);
                            succesPop = true;
                        }
                    }

                    if (succesPop == false)
                    {
                        richTextBox2.Text += $"Ошибка 'pop'. Переменная '{param[j]}'. Не объявлена.\n";
                    }
                }
                else
                {
                    richTextBox2.Text += $"Ошибка 'pop'. Последняя переменная в стеке '{stack[stack.Count - 1]}'. Вы пытаетесь достать '{param[j]}'.\n";
                }
            }
        }

        public void add(string code)
        {
            //делим на две части add / ax,2
            string[] secondCodePart = code.Split(new char[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries);
            //делем по запятой что складывать ax / 12
            string[] param = secondCodePart[1].Split(new char[] { ',' }, 2, StringSplitOptions.RemoveEmptyEntries);

            //проверим что вторая часть не переменная а цифры a,12

            if (IsDigitsOnly(param[1]) == true)
            {
                //проверим на существование переменную в param[0]
                bool succesAdd = false;

                //проверяем все строки в dw Данные на наличие там уже этой переменной
                for (int i = 0; i < dwDannie.Count; i++)
                {
                    //для проверки делим каждый элемент на его название и значение
                    string[] temp = dwDannie[i].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                    //если переменная существует
                    if (temp[0] == param[0])
                    {

                        string[] forSumma = dwDannie[i].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        forSumma[2] = Convert.ToString(Convert.ToDouble(forSumma[2]) + Convert.ToDouble(param[1]));//складываем и возвращаем
                        dwDannie[i] = $"{forSumma[0]} {forSumma[1]} {forSumma[2]}";
                        succesAdd = true;
                    }
                }

                //проверяем все строки в db Данные на наличие там уже этой переменной
                for (int i = 0; i < dbDannie.Count; i++)
                {
                    //для проверки делим каждый элемент на его название и значение
                    string[] temp = dbDannie[i].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                    //если переменная существует
                    if (temp[0] == param[0])
                    {
                        string[] forSumma = dbDannie[i].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        forSumma[2] = Convert.ToString(Convert.ToDouble(forSumma[2]) + Convert.ToDouble(param[1]));//складываем и возвращаем
                        dbDannie[i] = $"{forSumma[0]} {forSumma[1]} {forSumma[2]}";
                        succesAdd = true;
                    }
                }

                if (succesAdd == false)
                {
                    richTextBox2.Text += $"Ошибка 'add'. Переменной '{param[0]}' не существует.\n";
                }
            }
            //если вторая часть тоже переменная add a,b
            else
            {
                //проверим на существование переменную в param[0]
                bool succesAdd = false;

                //проверяем все строки в dw Данные на наличие первой переменной add A,b
                for (int i = 0; i < dwDannie.Count; i++)
                {
                    //для проверки делим каждый элемент на его название и значение
                    string[] temp = dwDannie[i].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                    //если переменная существует
                    if (temp[0] == param[0])
                    {

                        string[] forSumma = dwDannie[i].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        ///////// поиск существующей второй переменной add a,B <<<
                        for (int i2 = 0; i2 < dwDannie.Count; i2++)
                        {
                            //для проверки делим каждый элемент на его название и значение
                            string[] temp2 = dwDannie[i2].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                            //если переменная существует
                            if (temp2[0] == param[1])
                            {
                                string[] forSumma2 = dwDannie[i2].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                                forSumma[2] = Convert.ToString(Convert.ToDouble(forSumma[2]) + Convert.ToDouble(forSumma2[2]));//складываем и возвращаем
                                dwDannie[i] = $"{forSumma[0]} {forSumma[1]} {forSumma[2]}";
                                succesAdd = true;
                            }
                        }
                        for (int i2 = 0; i2 < dbDannie.Count; i2++)
                        {
                            //для проверки делим каждый элемент на его название и значение
                            string[] temp2 = dbDannie[i2].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                            //если переменная существует
                            if (temp2[0] == param[1])
                            {
                                string[] forSumma2 = dbDannie[i2].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                                forSumma[2] = Convert.ToString(Convert.ToDouble(forSumma[2]) + Convert.ToDouble(forSumma2[2]));//складываем и возвращаем
                                dwDannie[i] = $"{forSumma[0]} {forSumma[1]} {forSumma[2]}";
                                succesAdd = true;
                            }
                        }
                        /////////
                    }
                }

                //проверяем все строки в db Данные на наличие там уже этой переменной
                for (int i = 0; i < dbDannie.Count; i++)
                {
                    //для проверки делим каждый элемент на его название и значение
                    string[] temp = dbDannie[i].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                    //если переменная существует
                    if (temp[0] == param[0])
                    {

                        string[] forSumma = dbDannie[i].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        ///////// поиск существующей второй переменной add a,B <<<
                        for (int i2 = 0; i2 < dwDannie.Count; i2++)
                        {
                            //для проверки делим каждый элемент на его название и значение
                            string[] temp2 = dwDannie[i2].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                            //если переменная существует
                            if (temp2[0] == param[1])
                            {
                                string[] forSumma2 = dwDannie[i2].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                                forSumma[2] = Convert.ToString(Convert.ToDouble(forSumma[2]) + Convert.ToDouble(forSumma2[2]));//складываем и возвращаем
                                dbDannie[i] = $"{forSumma[0]} {forSumma[1]} {forSumma[2]}";
                                succesAdd = true;
                            }
                        }
                        for (int i2 = 0; i2 < dbDannie.Count; i2++)
                        {
                            //для проверки делим каждый элемент на его название и значение
                            string[] temp2 = dbDannie[i2].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                            //если переменная существует
                            if (temp2[0] == param[1])
                            {
                                string[] forSumma2 = dbDannie[i2].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                                forSumma[2] = Convert.ToString(Convert.ToDouble(forSumma[2]) + Convert.ToDouble(forSumma2[2]));//складываем и возвращаем
                                dbDannie[i] = $"{forSumma[0]} {forSumma[1]} {forSumma[2]}";
                                succesAdd = true;
                            }
                        }
                        /////////
                    }
                }

                if (succesAdd == false)
                {
                    richTextBox2.Text += $"Ошибка 'add'.\n";
                }
            }
        }

        public void mov(string code)
        {
            //делим на две части mov / ax,2
            string[] secondCodePart = code.Split(new char[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries);
            //делем по запятой что перемещать ax / 2
            string[] param = secondCodePart[1].Split(new char[] { ',' }, 2, StringSplitOptions.RemoveEmptyEntries);

            //проверим что вторая часть не переменная а цифры a,12

            if (IsDigitsOnly(param[1]) == true)
            {
                //проверим на существование переменную в param[0]
                bool succesAdd = false;

                //проверяем все строки в dw Данные на наличие там уже этой переменной
                for (int i = 0; i < dwDannie.Count; i++)
                {
                    //для проверки делим каждый элемент на его название и значение
                    string[] temp = dwDannie[i].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                    //если переменная существует
                    if (temp[0] == param[0])
                    {

                        string[] forSumma = dwDannie[i].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        forSumma[2] = param[1];//перемещаем и возвращаем
                        dwDannie[i] = $"{forSumma[0]} {forSumma[1]} {forSumma[2]}";
                        succesAdd = true;
                    }
                }

                //проверяем все строки в db Данные на наличие там уже этой переменной
                for (int i = 0; i < dbDannie.Count; i++)
                {
                    //для проверки делим каждый элемент на его название и значение
                    string[] temp = dbDannie[i].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                    //если переменная существует
                    if (temp[0] == param[0])
                    {
                        string[] forSumma = dbDannie[i].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        forSumma[2] = param[1];//перемещаем и возвращаем
                        dbDannie[i] = $"{forSumma[0]} {forSumma[1]} {forSumma[2]}";
                        succesAdd = true;
                    }
                }

                if (succesAdd == false)
                {
                    richTextBox2.Text += $"Ошибка 'mov'. Переменной '{param[0]}' не существует.\n";
                }
            }
            //если вторая часть тоже переменная add a,b
            else
            {
                //проверим на существование переменную в param[0]
                bool succesAdd = false;

                //проверяем все строки в dw Данные на наличие первой переменной add A,b
                for (int i = 0; i < dwDannie.Count; i++)
                {
                    //для проверки делим каждый элемент на его название и значение
                    string[] temp = dwDannie[i].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                    //если переменная существует
                    if (temp[0] == param[0])
                    {

                        string[] forSumma = dwDannie[i].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        ///////// поиск существующей второй переменной add a,B <<<
                        for (int i2 = 0; i2 < dwDannie.Count; i2++)
                        {
                            //для проверки делим каждый элемент на его название и значение
                            string[] temp2 = dwDannie[i2].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                            //если переменная существует
                            if (temp2[0] == param[1])
                            {
                                string[] forSumma2 = dwDannie[i2].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                                forSumma[2] = forSumma2[2];//перемещаем и возвращаем
                                dwDannie[i] = $"{forSumma[0]} {forSumma[1]} {forSumma[2]}";
                                succesAdd = true;
                            }
                        }
                        for (int i2 = 0; i2 < dbDannie.Count; i2++)
                        {
                            //для проверки делим каждый элемент на его название и значение
                            string[] temp2 = dbDannie[i2].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                            //если переменная существует
                            if (temp2[0] == param[1])
                            {
                                string[] forSumma2 = dbDannie[i2].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                                forSumma[2] = forSumma2[2];//перемещаем и возвращаем
                                dwDannie[i] = $"{forSumma[0]} {forSumma[1]} {forSumma[2]}";
                                succesAdd = true;
                            }
                        }
                        /////////
                    }
                }

                //проверяем все строки в db Данные на наличие там уже этой переменной
                for (int i = 0; i < dbDannie.Count; i++)
                {
                    //для проверки делим каждый элемент на его название и значение
                    string[] temp = dbDannie[i].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                    //если переменная существует
                    if (temp[0] == param[0])
                    {

                        string[] forSumma = dbDannie[i].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        ///////// поиск существующей второй переменной add a,B <<<
                        for (int i2 = 0; i2 < dwDannie.Count; i2++)
                        {
                            //для проверки делим каждый элемент на его название и значение
                            string[] temp2 = dwDannie[i2].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                            //если переменная существует
                            if (temp2[0] == param[1])
                            {
                                string[] forSumma2 = dwDannie[i2].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                                forSumma[2] = forSumma2[2];//перемещаем и возвращаем
                                dbDannie[i] = $"{forSumma[0]} {forSumma[1]} {forSumma[2]}";
                                succesAdd = true;
                            }
                        }
                        for (int i2 = 0; i2 < dbDannie.Count; i2++)
                        {
                            //для проверки делим каждый элемент на его название и значение
                            string[] temp2 = dbDannie[i2].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                            //если переменная существует
                            if (temp2[0] == param[1])
                            {
                                string[] forSumma2 = dbDannie[i2].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                                forSumma[2] = forSumma2[2];//перемещаем и возвращаем
                                dbDannie[i] = $"{forSumma[0]} {forSumma[1]} {forSumma[2]}";
                                succesAdd = true;
                            }
                        }
                        /////////
                    }
                }

                if (succesAdd == false)
                {
                    richTextBox2.Text += $"Ошибка 'mov'.\n";
                }
            }
        }

        public void int21h()
        {
            //dl прописана в коде в dwDannie[7] ячейке
            //делим её по пробелу  для извлечения значения
            string[] temp = dwDannie[7].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            richTextBox2.Text += temp[2] + "\n";
        }
    }
}