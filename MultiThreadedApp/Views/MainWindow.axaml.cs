using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace MultiThreadApp.Views
{
    public partial class MainWindow : Window
    {
        private Thread thread1, thread2, thread3;
        private volatile bool thread1Running, thread2Running, thread3Running;
        private readonly Random random = new Random();
        
        public MainWindow()
        {
            InitializeComponent();
            
            this.FindControl<Button>("StartThread1Btn").Click += (s, e) => StartThread1();
            this.FindControl<Button>("StopThread1Btn").Click += (s, e) => StopThread1();
            this.FindControl<Button>("StartThread2Btn").Click += (s, e) => StartThread2();
            this.FindControl<Button>("StopThread2Btn").Click += (s, e) => StopThread2();
            this.FindControl<Button>("StartThread3Btn").Click += (s, e) => StartThread3();
            this.FindControl<Button>("StopThread3Btn").Click += (s, e) => StopThread3();
            this.FindControl<Button>("StartAllBtn").Click += (s, e) => StartAllThreads();
            this.FindControl<Button>("StopAllBtn").Click += (s, e) => StopAllThreads();
        }
        
        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
        
        private void StartThread1() => StartThread(ref thread1, ref thread1Running, DerzhstandartAlgorithm, "ДЕРЖСТАНДАРТ");
        
        private void StartThread2() => StartThread(ref thread2, ref thread2Running, NHashAlgorithm, "N-Хеш");
        
        private void StartThread3() => StartThread(ref thread3, ref thread3Running, RC4Algorithm, "RC4");

        private void StopThread1()
        {
            thread1Running = false;
            thread1?.Join(500); // Очікуємо завершення потоку (максимум 500 мс)
            LogMessage("Потік 1 (ДЕРЖСТАНДАРТ) зупинено");
        }

        private void StopThread2() 
        {
            thread2Running = false;
            thread2?.Join(500);
            LogMessage("Потік 2 (N-Хеш) зупинено");
        }

        private void StopThread3() 
        {
            thread3Running = false;
            thread3?.Join(500);
            LogMessage("Потік 3 (RC4) зупинено");
        }
        
        private void StartAllThreads()
        {
            StartThread1();
            StartThread2();
            StartThread3();
        }
        
        private void StopAllThreads()
        {
            
            thread1Running = false;
            thread2Running = false;
            thread3Running = false;
            
            // Очікуємо завершення потоків
            thread1?.Join(500);
            thread2?.Join(500);
            thread3?.Join(500);
            
            LogMessage("Всі потоки зупинено");
        }
        
        private void StartThread(ref Thread thread, ref bool runningFlag, Action<string> algorithm, string algorithmName)
{
    if (runningFlag) return;
    
    runningFlag = true;
    bool localRunningFlag = runningFlag;
    thread = new Thread(() => 
    {
        while (localRunningFlag)
        {
            try 
            {
                var input = Guid.NewGuid().ToString();
                algorithm(input);
                Thread.Sleep(2000);
            }
            catch (ThreadAbortException)
            {
                localRunningFlag = false;
                Thread.ResetAbort();
            }
            catch (Exception ex)
            {
                LogMessage($"Помилка в потоці {algorithmName}: {ex.Message}");
                localRunningFlag = false;
            }
        }
    })
    { 
        IsBackground = true,
        Name = algorithmName
    };
    thread.Start();
    
    LogMessage($"{algorithmName}: потік запущено");
}
        
        private void DerzhstandartAlgorithm(string input)
        {
            // Імітація алгоритму ДЕРЖСТАНДАРТ
            var result = new StringBuilder();
            for (int i = 0; i < 8; i++)
            {
                result.Append((char)(0x0410 + random.Next(0, 32))); // Кириличні символи
            }
            
            Dispatcher.UIThread.Post(() => 
                LogMessage($"ДЕРЖСТАНДАРТ: {input} → {result}"));
        }
        
        private void NHashAlgorithm(string input)
        {
            // Алгоритм N-Хеш (спрощена версія)
            using var sha256 = SHA256.Create();
            byte[] hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
            var result = BitConverter.ToString(hash).Replace("-", "").Substring(0, 16);
            
            Dispatcher.UIThread.Post(() => 
                LogMessage($"N-Хеш: {input} → {result}"));
        }
        
        private void RC4Algorithm(string input)
        {
            // Алгоритм RC4
            byte[] key = Encoding.UTF8.GetBytes("secret-key");
            byte[] data = Encoding.UTF8.GetBytes(input);
            byte[] encrypted = RC4(data, key);
            var result = BitConverter.ToString(encrypted).Replace("-", "").Substring(0, 16);
            
            Dispatcher.UIThread.Post(() => 
                LogMessage($"RC4: {input} → {result}"));
        }
        
        private byte[] RC4(byte[] data, byte[] key)
        {
            int[] s = new int[256];
            for (int i = 0; i < 256; i++) s[i] = i;
            
            int j = 0;
            for (int i = 0; i < 256; i++)
            {
                j = (j + s[i] + key[i % key.Length]) % 256;
                (s[i], s[j]) = (s[j], s[i]);
            }
            
            byte[] result = new byte[data.Length];
            int x = 0, y = 0;
            for (int i = 0; i < data.Length; i++)
            {
                x = (x + 1) % 256;
                y = (y + s[x]) % 256;
                (s[x], s[y]) = (s[y], s[x]);
                result[i] = (byte)(data[i] ^ s[(s[x] + s[y]) % 256]);
            }
            
            return result;
        }
        
        private void LogMessage(string message)
        {
            var output = this.FindControl<TextBox>("OutputTextBox");
            output.Text = $"{DateTime.Now:HH:mm:ss} - {message}\n{output.Text}";
        }
    }
}