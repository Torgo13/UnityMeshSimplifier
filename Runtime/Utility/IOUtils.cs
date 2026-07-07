using System;
using System.IO;
using System.Linq;
using System.Text;

namespace UnityMeshSimplifier
{
    internal static class IOUtils
    {
        internal static string MakeSafeRelativePath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return string.Empty;

            path = path.Replace('\\', '/').Trim('/');

            if (Path.IsPathRooted(path))
                throw new ArgumentException("The path cannot be rooted.", "path");

#if OPTIMISATION
            var sb = new StringBuilder();

            // Make the path safe
            var pathParts = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < pathParts.Length; i++)
            {
                pathParts[i] = MakeSafeFileName(pathParts[i], sb);
            }

            return string.Join('/', pathParts);
#else
            // Make the path safe
            var pathParts = path.Split(new [] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < pathParts.Length; i++)
            {
                pathParts[i] = MakeSafeFileName(pathParts[i]);
            }
            return string.Join("/", pathParts);
#endif // OPTIMISATION
        }

#if OPTIMISATION
        private static readonly char[] invalidFileNameChars = Path.GetInvalidFileNameChars();
        internal static string MakeSafeFileName(string name,
		    StringBuilder sb)
        {
            _ = sb.Clear();
#else
        internal static string MakeSafeFileName(string name)
        {
            char[] invalidFileNameChars = Path.GetInvalidFileNameChars();

            var sb = new StringBuilder(name.Length);
#endif // OPTIMISATION
            bool lastWasInvalid = false;
            for (int i = 0; i < name.Length; i++)
            {
                char c = name[i];
                if (!invalidFileNameChars.Contains(c))
                {
                    sb.Append(c);
                }
                else if (!lastWasInvalid)
                {
                    lastWasInvalid = true;
                    sb.Append('_');
                }
            }
            return sb.ToString();
        }

#if UNITY_EDITOR
        internal static void CreateParentDirectory(string path)
        {
            int lastSlashIndex = path.LastIndexOf('/');
            if (lastSlashIndex != -1)
            {
                string parentPath = path.Substring(0, lastSlashIndex);
                if (!UnityEditor.AssetDatabase.IsValidFolder(parentPath))
                {
                    lastSlashIndex = parentPath.LastIndexOf('/');
                    if (lastSlashIndex != -1)
                    {
                        string folderName = parentPath.Substring(lastSlashIndex + 1);
                        string folderParentPath = parentPath.Substring(0, lastSlashIndex);
                        CreateParentDirectory(parentPath);
                        UnityEditor.AssetDatabase.CreateFolder(folderParentPath, folderName);
                    }
                    else
                    {
                        UnityEditor.AssetDatabase.CreateFolder(string.Empty, parentPath);
                    }
                }
            }
        }

        internal static bool DeleteEmptyDirectory(string path)
        {
            bool deletedAllSubFolders = true;
            var subFolders = UnityEditor.AssetDatabase.GetSubFolders(path);
            for (int i = 0; i < subFolders.Length; i++)
            {
                if (!DeleteEmptyDirectory(subFolders[i]))
                {
                    deletedAllSubFolders = false;
                }
            }

            if (!deletedAllSubFolders)
                return false;
            else if (!UnityEditor.AssetDatabase.IsValidFolder(path))
                return true;

            string[] assetGuids = UnityEditor.AssetDatabase.FindAssets(string.Empty, new string[] { path });
            if (assetGuids.Length > 0)
                return false;

            return UnityEditor.AssetDatabase.DeleteAsset(path);
        }
#endif
    }
}
