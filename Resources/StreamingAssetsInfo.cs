using UnityEngine;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;

namespace BJSYGameCore
{
    public class StreamingAssetsInfo : ScriptableObject
    {
        public void addFile(string path)
        {
            _fileList.Add(path);
        }
        public void clearFile()
        {
            _fileList.Clear();
        }
        public string[] getFiles(string dirName, string searchPattern)
        {
            searchPattern = Regex.Replace(searchPattern, "[.$^{\\[(|)*+?\\\\]", m =>
            {
                switch (m.Value)
                {
                    case "?":
                        return ".?";
                    case "*":
                        return ".*";
                    default:
                        return "\\" + m.Value;
                }
            }) + "$";
            return _fileList.Where(f => f.StartsWith(dirName) && Regex.IsMatch(f, searchPattern, RegexOptions.IgnoreCase)).Select(s => "sa:" + s).ToArray();
        }
        [SerializeField]
        List<string> _fileList = new List<string>();
    }
}
