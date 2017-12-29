using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Lab2;

namespace Lab2
{
    public partial class Form2 : Form
    {
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]  //подключение библиотеки kernel32.dll из папки Windows\system32
        //функция создания файла с перечислением стандартных для неё флагов:
        public static extern SafeFileHandle CreateFile(
      string lpFileName,
      [MarshalAs(UnmanagedType.U4)] FileAccess dwDesiredAccess,
      [MarshalAs(UnmanagedType.U4)] FileShare dwShareMode,
      IntPtr lpSecurityAttributes,
      [MarshalAs(UnmanagedType.U4)] FileMode dwCreationDisposition,
      [MarshalAs(UnmanagedType.U4)] FileAttributes dwFlagsAndAttributes,
      IntPtr hTemplateFile);

        //настройка функции из кернел.длл
        SafeFileHandle handle = CreateFile(
        lpFileName: @"\\.\" + Form1.drive[0] + ":",
        dwDesiredAccess: FileAccess.Read,
        dwShareMode: FileShare.ReadWrite,
        lpSecurityAttributes: IntPtr.Zero,
        dwCreationDisposition: System.IO.FileMode.OpenOrCreate,
        dwFlagsAndAttributes: FileAttributes.Normal,
        hTemplateFile: IntPtr.Zero);

       public int count_error;
        public Form2()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (String.IsNullOrWhiteSpace(textBox1.Text)) {
                MessageBox.Show("Старый пароль не введён!");
                return;
            }
            if (String.IsNullOrWhiteSpace(textBox2.Text)) {
                MessageBox.Show("Пароль не введён!");
                return;
            }
            if (String.IsNullOrWhiteSpace(textBox3.Text)) {
                MessageBox.Show("Введите ключ!");
                return;
            }

            //введённый старый пароль
            CaesarCipher AB = new CaesarCipher();
            string oldpassword = AB.Encryption(textBox1.Text, textBox3.Text);
            
            //защита от подмены флешки путем перечитывания нулевого сектора
            //читаем информацию из нулевого сектора для проверки есть ли пароль
            SafeFileHandle handle = CreateFile(
                    lpFileName: @"\\.\" + Form1.drive[0] + ":",
                    dwDesiredAccess: FileAccess.Read,
                    dwShareMode: FileShare.ReadWrite,
                    lpSecurityAttributes: IntPtr.Zero,
                    dwCreationDisposition: System.IO.FileMode.OpenOrCreate,
                    dwFlagsAndAttributes: FileAttributes.Normal,
                    hTemplateFile: IntPtr.Zero);

            using (FileStream disk = new FileStream(handle, FileAccess.Read))
            {
                Form1.buffer0sector = new byte[512];


                disk.Read(Form1.buffer0sector, 0, 512);
            }



            //сравниваем побитно хеш старого пароля и нового
            for (int j = 0; j < oldpassword.Length; j++)
            {
                if (Form1.buffer0sector[384 + j] != (byte)oldpassword[j])
                {
                    MessageBox.Show("Старый пароль введён неверно!");
                    textBox1.SelectAll();
                    textBox1.Focus();
                    count_error++;
                    if (count_error == 3)
                    {
                        textBox1.ReadOnly = true;
                        textBox2.ReadOnly = true;
                        textBox3.ReadOnly = true;
                        button1.Enabled = false;
                        MessageBox.Show("Отказано в доступе!");
                    }
                    return;
                }
            }

            
                //записываем пароль в нулевой сектор
                handle = CreateFile(
                lpFileName: @"\\.\" + Form1.drive[0] + ":",
                dwDesiredAccess: FileAccess.Write,
                dwShareMode: FileShare.ReadWrite,
                lpSecurityAttributes: IntPtr.Zero,
                dwCreationDisposition: System.IO.FileMode.OpenOrCreate,
                dwFlagsAndAttributes: FileAttributes.Normal,
                hTemplateFile: IntPtr.Zero);

                using (FileStream disk = new FileStream(handle, FileAccess.Write))
                {
                    string KeyWord;
                    int key;
                    const int LengthForKey = 3;
                    //  String hashpassword = GetMd5Hash(textBox1.Text);
                    CaesarCipher Cipher = new CaesarCipher();
                    int startSeed;
                    startSeed = Cipher.For_Shift();
                    KeyWord = Cipher.RandomKeyFor(LengthForKey, startSeed);
                    string hashpassword = Cipher.Encryption(textBox1.Text, KeyWord);

                    // Convert the input string to a byte array and compute the hash.
                    byte[] data = Encoding.Default.GetBytes(hashpassword);

                    // Create a new Stringbuilder to collect the bytes
                    // and create a string.
                    StringBuilder sBuilder = new StringBuilder();

                    // Loop through each byte of the hashed data 
                    // and format each one as a hexadecimal string.
                    for (int i = 0; i < data.Length; i++)
                    {
                        sBuilder.Append(data[i].ToString("x2"));
                    }
                    for (int i = 0; i < hashpassword.Length; i++)
                        Form1.buffer0sector[384 + i] = (byte)hashpassword[i];

                    disk.Write(Form1.buffer0sector, 0, 512);
                MessageBox.Show("Пароль успешно изменён!\nВаш ключ - " + KeyWord);
            }
              
            }
        

        public void Form2_Load(object sender, EventArgs e)
        {
          //  f1 = (Form1)this.Parent;
           int count_error = 0; //счётчик неправильных вводов пароля
        }
    }
}
