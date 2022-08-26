using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;
using System.Windows.Input;

namespace PokerHandHistory
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private const string pokerstarsString = "Enter Path for Pokerstars Hands";
        private const string agouraHillsString = "Enter Path for Agoura Hills Hands";

        private Thread writingThreadOne;
        private Thread writingThreadTwo;

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
            var pokerstarsFile = "Pokerstars_Hand_161248135942.txt";
            var agouraHillsFile = "Agoura_Hills_Hand_53005191.txt";
            var pokerstarsWritingFile = "Pokerstars";
            var agouraHillsWritingFile = "AgouraHills";

            var pokerstarsAbsolutePath = $@"{firstPath}\{pokerstarsWritingFile}";
            var agouraHillsAbsolutePath = $@"{secondPath}\{agouraHillsWritingFile}";

            writingThreadOne = CreateThread(assembly, pokerstarsFile, pokerstarsAbsolutePath);
            writingThreadTwo = CreateThread(assembly, agouraHillsFile, agouraHillsAbsolutePath);

            writingThreadOne.Start();
            writingThreadTwo.Start();
        }

        private Thread CreateThread(Assembly _assembly, string _embeddedTextFile, string _absolutePathToFile)
        {
            return new Thread(() =>
            {
                try
                {
                    using (Stream stream = _assembly.GetManifestResourceStream(_assembly.GetManifestResourceNames().Single(str => str.EndsWith(_embeddedTextFile))))
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        string line;
                        bool recreateFile = true;
                        int counter = 0;

                        while ((line = reader.ReadLine()) != null)
                        {
                            if (recreateFile)
                            {
                                if (line == string.Empty)
                                {
                                    continue;
                                }

                                if (File.Exists($"{_absolutePathToFile + counter}.txt"))
                                {
                                    File.Delete($"{_absolutePathToFile + counter}.txt");
                                }

                                recreateFile = false;
                            }

                            using (var writer = File.AppendText($"{_absolutePathToFile + counter}.txt"))
                            {
                                if (line == string.Empty)
                                {
                                    Thread.Sleep(TimeSpan.FromSeconds(15));
                                    recreateFile = true;
                                    counter++;
                                    continue;
                                }

                                writer.WriteLine(line);
                                writer.Flush();
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
        }
    }
}
