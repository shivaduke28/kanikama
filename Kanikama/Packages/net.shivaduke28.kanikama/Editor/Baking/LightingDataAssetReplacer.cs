using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Kanikama.Utility;
using UnityEditor;
using UnityEngine;

namespace Kanikama.Editor.Baking
{
    // Dirty code to replace m_LightingDataAsset field in .unity file directly.
    public static class LightingDataAssetReplacer
    {
        public static async Task ReplaceAsync(SceneAsset sceneAsset, LightingDataAsset lightingDataAsset, CancellationToken cancellationToken)
        {
            var assetPath = AssetDatabase.GetAssetPath(sceneAsset);
            var objectId = GlobalObjectId.GetGlobalObjectIdSlow(lightingDataAsset);
            var replacedLine = lightingDataAsset == null
                ? "  m_LightingDataAsset: {fileID: 0}"
                : $"  m_LightingDataAsset: {{fileID: {objectId.targetObjectId.ToString()}, guid: {objectId.assetGUID.ToString()}, type: 2}}";

            Debug.LogFormat(KanikamaDebug.Format, $"replacing LightingDataAsset in {assetPath}");

            var stringBuilder = new StringBuilder();
            var encoding = new UTF8Encoding(false); // without BOM
            var dstPath = assetPath + ".txt";

            var newline = await DetectNewlineAsync(assetPath, cancellationToken);
            var t = newline == "\n" ? "LF" : newline == "\r\n" ? "CRLF" : "";
            Debug.LogFormat(KanikamaDebug.Format, $"detected newline: {t}");

            using (var fs = File.Open(assetPath, FileMode.Open, FileAccess.ReadWrite))
            using (var dst = File.Open(dstPath, FileMode.OpenOrCreate, FileAccess.Write))
            using (var reader = new StreamReader(fs, encoding))
            using (var writer = new StreamWriter(dst, encoding))
            {
                try
                {
                    while (!reader.EndOfStream)
                    {
                        var line = await reader.ReadLineAsync();
                        cancellationToken.ThrowIfCancellationRequested();

                        if (line.StartsWith("  m_LightingDataAsset"))
                        {
                            Debug.LogFormat(KanikamaDebug.Format, $"replace {line} -> {replacedLine}");
                            var before = stringBuilder.ToString();
                            await writer.WriteAsync(before);
                            await writer.WriteAsync(replacedLine + newline);
                            await writer.FlushAsync();

                            var beforeByteCount = encoding.GetByteCount(before);
                            var originalByteCount = beforeByteCount + encoding.GetByteCount(line + newline);
                            if (!reader.EndOfStream)
                            {
                                var nextLine = await reader.ReadLineAsync();
                                if (nextLine.StartsWith("    "))
                                {
                                    originalByteCount += encoding.GetByteCount(nextLine + newline);
                                    Debug.LogFormat(KanikamaDebug.Format, $"omit {nextLine}");
                                }
                            }
                            fs.Position = originalByteCount;
                            await fs.CopyToAsync(dst, 81920, cancellationToken);
                            await dst.FlushAsync(cancellationToken);
                            break;
                        }

                        stringBuilder.Append(line + newline);
                    }
                }
                catch (Exception)
                {
                    if (File.Exists(dstPath))
                    {
                        File.Delete(dstPath);
                    }
                    throw;
                }
            }

            var backup = assetPath + ".bac";
            File.Replace(dstPath, assetPath, backup);
            AssetDatabase.ImportAsset(assetPath);
            Debug.LogFormat(KanikamaDebug.Format, $"replaced LightingDataAsset successfully with backup {backup}");
        }

        const char CR = '\r';
        const char LF = '\n';

        static async Task<string> DetectNewlineAsync(string assetPath, CancellationToken cancellationToken)
        {
            using (var fs = File.Open(assetPath, FileMode.Open, FileAccess.Read))
            {
                var buffer = new byte[128];
                int byteRead;
                var returnSeen = false;
                while ((byteRead = await fs.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) > 0)
                {
                    for (var i = 0; i < byteRead; i++)
                    {
                        var ch = (char) buffer[i];
                        switch (ch)
                        {
                            case CR:
                                returnSeen = true;
                                break;
                            case LF:
                                return returnSeen ? "\r\n" : "\n";
                        }
                    }
                }
                throw new ArgumentException("Newline is not found.");
            }
        }
    }
}
