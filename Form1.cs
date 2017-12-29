using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
using System.IO;
using System.Security.Cryptography;
using Lab2;


namespace Lab2
{
    public partial class Form1 : Form
    {
        static public String drive;


        public Form1()
        {
            InitializeComponent();

        }
        //1. подключение кернел32.длл
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

        public static byte[] buffer0sector;

        void read0sector()
        {
            while (folderBrowserDialog1.ShowDialog() != DialogResult.OK)
            {
                MessageBox.Show("Выберите правильный путь!");  //      goto metka;
            }
            drive = folderBrowserDialog1.SelectedPath;
            //настройка функции из кернел.длл
            SafeFileHandle handle = CreateFile( 
                        lpFileName: @"\\.\" + folderBrowserDialog1.SelectedPath[0] + ":", 
                        dwDesiredAccess: FileAccess.Read,
                        dwShareMode: FileShare.ReadWrite,
                        lpSecurityAttributes: IntPtr.Zero,
                        dwCreationDisposition: System.IO.FileMode.OpenOrCreate,
                        dwFlagsAndAttributes: FileAttributes.Normal,
                        hTemplateFile: IntPtr.Zero);
                //чтение нулевого сектора
                     using (FileStream disk = new FileStream(handle, FileAccess.Read))
                     {
                           buffer0sector = new byte[512];
                           disk.Read(buffer0sector, 0, 512);
                           MessageBox.Show("Нулевой сектор прочитан");
                           button3.Enabled = true;
                           button2.Enabled = true;
                     }
        }


        private void button2_Click(object sender, EventArgs e)     //запись пароля
        {
            try
            {
                while (folderBrowserDialog1.ShowDialog() != DialogResult.OK)
                {
                    MessageBox.Show("Выберите правильный путь!");
                }

                drive = folderBrowserDialog1.SelectedPath;
                SafeFileHandle handle = CreateFile(
                lpFileName: @"\\.\" + folderBrowserDialog1.SelectedPath[0] + ":",
                dwDesiredAccess: FileAccess.Read,
                dwShareMode: FileShare.ReadWrite,
                lpSecurityAttributes: IntPtr.Zero,
                dwCreationDisposition: System.IO.FileMode.OpenOrCreate,
                dwFlagsAndAttributes: FileAttributes.Normal,
                hTemplateFile: IntPtr.Zero);

                //запись пароль в нулевой сектор
                handle = CreateFile(
                lpFileName: @"\\.\" + folderBrowserDialog1.SelectedPath[0] + ":",
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
                        buffer0sector[384 + i] = (byte)hashpassword[i];

                    disk.Write(buffer0sector, 0, 512);
                    MessageBox.Show("Пароль записан в нулевой сектор!\nВаш ключ - " + KeyWord);
                    button4.Enabled = true;
                }
            }
            catch (Exception exp)
            {
                MessageBox.Show(exp.Message);
            }
        }

        private void button3_Click(object sender, EventArgs e)      //удаление пароля из 0 сектора
        {
            try
            {
                while (folderBrowserDialog1.ShowDialog() != DialogResult.OK)
                {
                    MessageBox.Show("Выберите правильный путь!");
                }
                drive = folderBrowserDialog1.SelectedPath;
                //читаем информацию из нулевого сектора для проверки есть ли пароль
                SafeFileHandle handle = CreateFile(
                lpFileName: @"\\.\" + folderBrowserDialog1.SelectedPath[0] + ":",
                dwDesiredAccess: FileAccess.Read,
                dwShareMode: FileShare.ReadWrite,
                lpSecurityAttributes: IntPtr.Zero,
                dwCreationDisposition: System.IO.FileMode.OpenOrCreate,
                dwFlagsAndAttributes: FileAttributes.Normal,
                hTemplateFile: IntPtr.Zero);

                using (FileStream disk = new FileStream(handle, FileAccess.Read))
                {
                    buffer0sector = new byte[512];
                    disk.Read(buffer0sector, 0, 512);
                }
                //стирание пароля
                handle = CreateFile(
                lpFileName: @"\\.\" + folderBrowserDialog1.SelectedPath[0] + ":",
                dwDesiredAccess: FileAccess.Write,
                dwShareMode: FileShare.ReadWrite,
                lpSecurityAttributes: IntPtr.Zero,
                dwCreationDisposition: System.IO.FileMode.OpenOrCreate,
                dwFlagsAndAttributes: FileAttributes.Normal,
                hTemplateFile: IntPtr.Zero);

                using (FileStream disk = new FileStream(handle, FileAccess.Write))
                {
                    for (int i = 0; i < 32; i++)
                        buffer0sector[384 + i] = 0;

                    disk.Write(buffer0sector, 0, 512);
                    MessageBox.Show("Пароль удалён из нулевого сектора!");
                    button2.Enabled = true;
                    textBox1.ReadOnly = false;
                }
            }
            catch (Exception exp)
            {
                MessageBox.Show(exp.Message);
            }
        }

        public void button1_Click(object sender, EventArgs e)
        {
            try
            { 
                  read0sector();
                    //найдём сумму байт в секторах с 384 по 416 чтобы узнать есть ли там пароль
                  int summa = 0;
                  for (int i = 384; i < 416; i++)
                      summa = summa + buffer0sector[i];
                    //если сумма отличается от нуля, значит пароль в нулевом секторе есть
                  if (summa == 0)
                  {
                      MessageBox.Show("Пароль отсутствует!");
                    button2.Enabled = true;
                    textBox1.ReadOnly = false;
                  }
                  else
                {
                    button4.Enabled = true;
                    button2.Enabled = false;
                    textBox1.ReadOnly = false;
                }
                
            }
            catch (Exception exp)
            {
                MessageBox.Show("       Произошла ошибка\n" + exp.Message);
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Form2 NewForm2 = new Form2();
            NewForm2.ShowDialog();
        }
    }
}
