using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetInserter
{
    internal class FileTemplates
    {
        public static readonly string AssetIndexTs = "export { keys as <assetName>Keys, armatureDisplayName as <assetName>ArmatureDisplayName } from './keys'\r\nexport {\r\n    assetManifest as <assetName>AssetManifest,\r\n    assetManifestWebp as <assetName>AssetManifestWebp,\r\n    dragonbonesManifest as <assetName>DragonbonesManifest,\r\n    dragonbonesManifestWebp as <assetName>DragonbonesManifestWebp,\r\n} from './manifest'";

        public static readonly string AssetKeysTs = "import { generateKeysForSymbol } from '../lib'\r\n\r\nexport const keys = generateKeysForSymbol('<assetName>')\r\nexport const armatureDisplayName = '<armName>'";

        public static readonly string AssetManifestTs = "import { keys, armatureDisplayName } from './keys'\r\nimport staticView from './Static'\r\nimport blurView from './Blur'\r\nimport skeJson from './Skeleton'\r\nimport texJson from './TexDesc'\r\nimport texPng from './Texture'\r\n\r\nimport staticViewWebp from './StWebp'\r\nimport blurViewWebp from './BlWebp'\r\nimport texPngWebp from './TexWebp'\r\n\r\nexport const assetManifest = {\r\n    [keys.static]: staticView,\r\n    [keys.blur]: blurView,\r\n}\r\n\r\nexport const assetManifestWebp = {\r\n    [keys.static]: staticViewWebp,\r\n    [keys.blur]: blurViewWebp,\r\n}\r\n\r\nexport const dragonbonesManifest = {\r\n    [armatureDisplayName]: {\r\n        ske: skeJson,\r\n        tex: texJson,\r\n        png: texPng,\r\n    },\r\n}\r\n\r\nexport const dragonbonesManifestWebp = {\r\n    [armatureDisplayName]: {\r\n        ske: skeJson,\r\n        tex: texJson,\r\n        png: texPngWebp,\r\n    },\r\n}\r\n";
    }
}
