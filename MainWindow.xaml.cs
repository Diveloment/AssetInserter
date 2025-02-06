using ImageMagick;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;
using static AssetInserter.FileItem;
using Path = System.IO.Path;

namespace AssetInserter
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        // vars
        public ObservableCollection<FileItem> Files { get; set; } = new ObservableCollection<FileItem>();
        private string _selectedProjectPath;
        private string _projectValidationStatus;
        private bool _isReadyToInsert;
        private readonly string[] requiredFiles = new string[]
        {
            "src/config/symbol.ts",
            "src/assets/keys.ts",
            "src/assets/manifest.ts",
            "src/assets/symbols/"
        };
        public static readonly string AssetFolderSubPath = "src/assets/symbols/";
        public static readonly string AssetsRootPath = "src/assets/";

        //getters setters
        public string SelectedProjectPath
        {
            get => _selectedProjectPath;
            set
            {
                _selectedProjectPath = value;
                ValidateProject();
                OnPropertyChanged(nameof(SelectedProjectPath));
            }
        }

        public string ProjectValidationStatus
        {
            get => _projectValidationStatus;
            set
            {
                _projectValidationStatus = value;
                OnPropertyChanged(nameof(ProjectValidationStatus));
            }
        }

        public bool IsReadyToInsert
        {
            get => _isReadyToInsert;
            set
            {
                _isReadyToInsert = value;
                OnPropertyChanged(nameof(IsReadyToInsert));
            }
        }

        // init
        public MainWindow()
        {
            InitializeComponent();
            lbFiles.ItemsSource = Files;
            DataContext = this;
            SelectedProjectPath = "Выберите папку или перетащите...";
            _isReadyToInsert = false;
        }

        private void lbFiles_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Copy;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
        }

        private void lbFiles_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                foreach (string file in files)
                {
                    if (File.Exists(file))
                    {
                        var fileItem = new FileItem(file);
                        fileItem.SelectedAssetTypeName = ResolveFileAssetType(file);
                        Files.Add(fileItem);
                    }
                }
            }
        }

        private void Directory_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                string target = files[0];
                if (Directory.Exists(target))
                {
                    SelectedProjectPath = target;
                }
            }
        }

        private void DeleteFile_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is FileItem file)
            {
                Files.Remove(file);
            }
        }

        private void SelectProjectFolder_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                dialog.Description = "Выберите рабочую директорию проекта";
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    SelectedProjectPath = dialog.SelectedPath;
                }
            }
        }

        private void GenerateAsset_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(assetNameBox.Text))
            {
                MessageBox.Show("Введите имя ассета!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (string.IsNullOrWhiteSpace(SelectedProjectPath) || !Directory.Exists(SelectedProjectPath))
            {
                MessageBox.Show("Выберите корректную папку проекта!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (Files.Count < 5)
            {
                MessageBox.Show("Неполный набор файлов ассета!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string assetName = assetNameBox.Text;
            string assetFolderPath = System.IO.Path.Combine(SelectedProjectPath, AssetFolderSubPath, assetName);
            string assetsSymbolRootPath = Path.Combine(SelectedProjectPath, AssetFolderSubPath);

            try
            {
                Directory.CreateDirectory(assetFolderPath);

                FileItem skeletonFile = null;

                List<FileItem> webpFiles = new List<FileItem>();

                foreach (var file in Files)
                {
                    string destPath = System.IO.Path.Combine(assetFolderPath, file.FileName);
                    File.Copy(file.FilePath, destPath, overwrite: true);
                    
                    if (file.SelectedAssetTypeName.Key.Equals(AssetType.Skeleton))
                    {
                        skeletonFile = file;
                    }

                    if (file.SelectedAssetTypeName.Key == AssetType.Static ||
                        file.SelectedAssetTypeName.Key == AssetType.Blur ||
                        file.SelectedAssetTypeName.Key == AssetType.Texture)
                    {
                        string webpFileName = System.IO.Path.GetFileNameWithoutExtension(file.FileName) + ".webp";
                        string webpFilePath = System.IO.Path.Combine(assetFolderPath, webpFileName);

                        ConvertToWebP(file.FilePath, webpFilePath);

                        AssetType newAssetType;

                        switch (file.SelectedAssetTypeName.Key)
                        {
                            case AssetType.Static:
                                newAssetType = AssetType.StWebp;
                                break;
                            case AssetType.Blur:
                                newAssetType = AssetType.BlWebp;
                                break;
                            case AssetType.Texture:
                                newAssetType = AssetType.TexWebp;
                                break;
                            default:
                                throw new InvalidOperationException("Неизвестный тип ассета");
                        }

                        webpFiles.Add(
                            new FileItem(webpFilePath, new KeyValuePair<AssetType, string>(newAssetType, newAssetType.ToString()))
                        );
                    }
                }

                foreach (var file in webpFiles)
                {
                    Files.Add(file);
                }

                if (skeletonFile == null)
                {
                    throw new FileNotFoundException("Skeleton file not found");
                }
                string json = File.ReadAllText(skeletonFile.FilePath);
                JObject obj = JObject.Parse(json);

                string armatureName = obj["armature"]?[0]?["name"]?.ToString();

                Generator.GenerateIndexTs(assetFolderPath, assetName);
                Generator.GenerateKeysTs(assetFolderPath, assetName, armatureName);
                Generator.GenerateManifestTs(assetFolderPath, Files);
                Generator.ModifySymbolsKeysTs(assetsSymbolRootPath, assetName);
                Generator.ModifySymbolsManifestTs(assetsSymbolRootPath, assetName);

                MessageBox.Show($"Ассет '{assetName}' успешно создан!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                Files.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при создании ассета: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                Files.Clear();
            }
        }

        private void ValidateProject()
        {
            if (string.IsNullOrEmpty(SelectedProjectPath) || !Directory.Exists(SelectedProjectPath))
            {
                ProjectValidationStatus = "Ошибка: Папка не выбрана или не существует.";
                return;
            }

            string report = "Проверка файлов проекта:\n";
            bool allFilesExist = true;

            foreach (var file in requiredFiles)
            {
                string fullPath = System.IO.Path.Combine(SelectedProjectPath, file);
                bool exists = file.EndsWith("/") ? Directory.Exists(fullPath) : File.Exists(fullPath);

                report += $"{(exists ? "✔️" : "❌")} {file}\n";

                if (!exists)
                    allFilesExist = false;
            }

            ProjectValidationStatus = allFilesExist ? "✅ Все необходимые файлы на месте!" : report;
            IsReadyToInsert = true;
        }

        static void ConvertToWebP(string inputPath, string outputPath)
        {
            using (MagickImage image = new MagickImage(inputPath))
            {
                image.Quality = 100;
                image.Write(outputPath, MagickFormat.WebP);
            }
        }

        static KeyValuePair<AssetType, string> ResolveFileAssetType(string filePath)
        {
            string fileName = Path.GetFileNameWithoutExtension(filePath);
            string extension = Path.GetExtension(filePath);

            if (extension == ".png")
            {
                if (fileName.Contains("bl_") || fileName.Contains("_bl") || fileName.Contains("_b") || fileName.Contains("b_"))
                {
                    return AssetNames[1];
                }
                if (fileName.Contains("_tex") || fileName.Contains("tex_"))
                {
                    return AssetNames[2];
                }
                return AssetNames[0];
            }
            if (extension == ".json")
            {
                if (fileName.Contains("_ske"))
                {
                    return AssetNames[4];
                }
                if (fileName.Contains("_tex"))
                {
                    return AssetNames[3];
                }
            }
            return AssetNames.FirstOrDefault();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
