using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetInserter
{
    internal class Generator
    {
        public static void GenerateIndexTs(string folderPath, string assetName)
        {

            string replaceValue = "<assetName>";
            string template = FileTemplates.AssetIndexTs;

            var result = template.Replace(replaceValue, assetName.ToLower());

            File.WriteAllText(Path.Combine(folderPath, "index.ts"), result);
        }

        public static void GenerateKeysTs(string folderPath, string assetName, string armDisplayName)
        {
            string replaceAssetValue = "<assetName>";
            string replaceArmValue = "<armName>";
            string template = FileTemplates.AssetKeysTs;

            template = template.Replace(replaceAssetValue, assetName.ToLower());
            var result = template.Replace(replaceArmValue, armDisplayName);

            File.WriteAllText(Path.Combine(folderPath, "keys.ts"), result);
        }

        public static void GenerateManifestTs(string folderPath, ICollection<FileItem> files)
        {
            var fileDictionary = files.ToDictionary(f => f.SelectedAssetTypeName.Key, f => f);

            string template = FileTemplates.AssetManifestTs;

            foreach (var item in fileDictionary)
            {
                template = template.Replace(item.Key.ToString(), item.Value.FileName);
            }

            File.WriteAllText(Path.Combine(folderPath, "manifest.ts"), template);
        }

        public static void ModifySymbolsKeysTs(string folderPath, string assetName)
        {
            string assetKeysFilePath = Path.Combine(folderPath, "keys.ts");
            string fileContent = File.ReadAllText(assetKeysFilePath);

            string newAssetName = assetName;
            string importStatement = $"import {{ {newAssetName}Keys, {newAssetName}ArmatureDisplayName }} from './{newAssetName}';\n";
            string newAssetKey = $"    {newAssetName}: {newAssetName}Keys,";
            string newArmatureKey = $"    {newAssetName}: {newAssetName}ArmatureDisplayName,";

            if (!fileContent.Contains(importStatement))
            {
                int firstExportIndex = fileContent.IndexOf("export ");
                if (firstExportIndex != -1)
                {
                    fileContent = fileContent.Insert(firstExportIndex, importStatement);
                }
            }

            if (!fileContent.Contains(newAssetKey))
            {
                fileContent = fileContent.Replace("export const assetKeys = {",
                    "export const assetKeys = {\n" + newAssetKey);
            }

            if (!fileContent.Contains(newArmatureKey))
            {
                fileContent = fileContent.Replace("export const armatureDisplayNames = {",
                    "export const armatureDisplayNames = {\n" + newArmatureKey);
            }

            File.WriteAllText(assetKeysFilePath, fileContent);
        }

        public static void ModifySymbolsManifestTs(string folderPath, string assetName)
        {
            string manifestFilePath = Path.Combine(folderPath, "manifest.ts");
            string fileContent = File.ReadAllText(manifestFilePath);

            string newAssetName = assetName;
            string importStatement = $"import {{ {newAssetName}AssetManifest, {newAssetName}AssetManifestWebp, {newAssetName}DragonbonesManifest, {newAssetName}DragonbonesManifestWebp }} from './{newAssetName}';\n";
            string newAssetManifest = $"    ...{newAssetName}AssetManifest,";
            string newAssetManifestWebp = $"    ...{newAssetName}AssetManifestWebp,";
            string newDragonbonesManifest = $"    ...{newAssetName}DragonbonesManifest,";
            string newDragonbonesManifestWebp = $"    ...{newAssetName}DragonbonesManifestWebp,";

            if (!fileContent.Contains(importStatement))
            {
                int firstExportIndex = fileContent.IndexOf("export ");
                if (firstExportIndex != -1)
                {
                    fileContent = fileContent.Insert(firstExportIndex, importStatement);
                }
            }

            if (!fileContent.Contains(newAssetManifest))
            {
                fileContent = fileContent.Replace("export const assetManifest = {",
                    "export const assetManifest = {\n" + newAssetManifest);
            }

            if (!fileContent.Contains(newAssetManifestWebp))
            {
                fileContent = fileContent.Replace("export const assetManifestWebp = {",
                    "export const assetManifestWebp = {\n" + newAssetManifestWebp);
            }

            if (!fileContent.Contains(newDragonbonesManifest))
            {
                fileContent = fileContent.Replace("export const dragonbonesManifest = {",
                    "export const dragonbonesManifest = {\n" + newDragonbonesManifest);
            }

            if (!fileContent.Contains(newDragonbonesManifestWebp))
            {
                fileContent = fileContent.Replace("export const dragonbonesManifestWebp = {",
                    "export const dragonbonesManifestWebp = {\n" + newDragonbonesManifestWebp);
            }

            File.WriteAllText(manifestFilePath, fileContent);

        }
    }
}
