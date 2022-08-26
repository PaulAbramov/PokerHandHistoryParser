using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace PokerHandHistoryDatabase
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private const string pokerstarsString = "Enter Path for Pokerstars Hands";
        private const string agouraHillsString = "Enter Path for Agoura Hills Hands";
        private const string pokerstarsWritingFile = "Pokerstars";
        private const string agouraHillsWritingFile = "AgouraHills";
        private Thread writingThreadOne;
        private Thread writingThreadTwo;
        private Thread executeCommandsThread;

        public event PropertyChangedEventHandler PropertyChanged;

        private string firstPath;
        public string FirstPath
        {
            get => firstPath;
            set
            {
                if (value != firstPath)
                {
                    firstPath = value;
                    OnPropertyChanged();
                }
            }
        }

        private string secondPath;
        public string SecondPath
        {
            get => secondPath;
            set
            {
                if (value != secondPath)
                {
                    secondPath = value;
                    OnPropertyChanged();
                }
            }
        }

        public ICommand StartWritingCommand { get; set; }
        public ICommand StopWritingCommand { get; set; }

        public MainViewModel()
        {
            StartWritingCommand = new RelayCommand(StartWriting);
            StopWritingCommand = new RelayCommand(StopWriting);

            FirstPath = pokerstarsString;
            SecondPath = agouraHillsString;
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void StartWriting(object _obj)
        {
            if (firstPath == string.Empty || firstPath.StartsWith(pokerstarsString))
            {
                MessageBox.Show("Path to Poker stars folder is empty, please fill in");
                return;
            }
            if (secondPath == string.Empty || secondPath.StartsWith(pokerstarsString))
            {
                MessageBox.Show("Path to Agoura Hills folder is empty, please fill in");
                return;
            }

            if (!Directory.Exists(firstPath))
            {
                MessageBox.Show("Folder for pokerstars does not exist");
                return;
            }
            if (!Directory.Exists(secondPath))
            {
                MessageBox.Show("Folder for agoura hills does not exist");
                return;
            }

            var assembly = Assembly.GetExecutingAssembly();


            DatabaseHandler.InitializeDatabase();
            executeCommandsThread = new Thread(DatabaseHandler.ExecuteCommands);
            executeCommandsThread.Start();

            writingThreadOne = CreateThread(assembly, firstPath);
            writingThreadTwo = CreateThread(assembly, secondPath);

            writingThreadOne.Start();
            writingThreadTwo.Start();
        }

        private Thread CreateThread(Assembly _assembly, string _pathToFolder)
        {
            return new Thread(() =>
            {
                try
                {
                    List<string> filesHandled = new List<string>();
                    while (true)
                    {
                        var files = Directory.GetFiles(_pathToFolder);

                        foreach (var file in files)
                        {
                            if (filesHandled.Contains(file))
                            {
                                continue;
                            }

                            using (var stream = File.OpenRead($@"{_pathToFolder}\{file}"))
                            using (var reader = new StreamReader(stream))
                            {
                                var wholeString = reader.ReadToEnd();

                                long id = 0;
                                if (file.Contains(pokerstarsWritingFile))
                                {
                                    id = Convert.ToInt64(wholeString.Substring(17, 12));
                                }
                                if (file.Contains(pokerstarsWritingFile))
                                {
                                    id = Convert.ToInt64(wholeString.Substring(6, 8));
                                }

                                DatabaseHandler.AddCommandToQueue(id, wholeString);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    if (e is ThreadAbortException)
                    {
                        return;
                    }
                    Console.WriteLine(e);
                    throw;
                }
            });
        }

        private void StopWriting(object _obj)
        {
            writingThreadOne.Abort();
            writingThreadTwo.Abort();
            executeCommandsThread.Abort();
        }
    }
}
