using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace AssetInserter
{
    public class FileItem : INotifyPropertyChanged
    {
        public string FilePath { get; set; }
        public string FileName => Path.GetFileName(FilePath);
        public BitmapImage Thumbnail { get; set; }

        public enum AssetType
        {
            Static,
            Blur,
            Texture,
            Skeleton,
            TexDesc,
            StWebp,
            BlWebp,
            TexWebp
        }

        public static ObservableCollection<KeyValuePair<AssetType, string>> AssetNames { get; } = new ObservableCollection<KeyValuePair<AssetType, string>>
        {
            new KeyValuePair<AssetType, string>(AssetType.Static, "static view"),
            new KeyValuePair<AssetType, string>(AssetType.Blur, "blur view"),
            new KeyValuePair<AssetType, string>(AssetType.Texture, "texture"),
            new KeyValuePair<AssetType, string>(AssetType.TexDesc, "tex json"),
            new KeyValuePair<AssetType, string>(AssetType.Skeleton, "ske json")
        };

        private KeyValuePair<AssetType, string> _selectedAssetType;
        public KeyValuePair<AssetType, string> SelectedAssetTypeName
        {
            get => _selectedAssetType;
            set
            {
                _selectedAssetType = value;
                OnPropertyChanged(nameof(SelectedAssetTypeName));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public FileItem(string filePath)
        {
            FilePath = filePath;
            Thumbnail = GenerateThumbnail(filePath);
        }

        public FileItem(string filePath, KeyValuePair<AssetType, string> selectedAssetTypeName)
        {
            FilePath = filePath;
            SelectedAssetTypeName = selectedAssetTypeName;
        }

        private BitmapImage GenerateThumbnail(string filePath)
        {
            try
            {
                if (IsImage(filePath))
                {
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(filePath);
                    bitmap.DecodePixelWidth = 50;
                    bitmap.DecodePixelHeight = 50;
                    bitmap.EndInit();
                    return bitmap;
                }
            }
            catch
            {
                // Можно добавить иконку по умолчанию
            }
            return new BitmapImage(new Uri("pack://application:,,,/Images/defaultIcon.png"));
        }

        private bool IsImage(string filePath)
        {
            string ext = Path.GetExtension(filePath).ToLower();
            return ext == ".jpg" || ext == ".jpeg" || ext == ".png" || ext == ".bmp" || ext == ".gif";
        }
    }
}
