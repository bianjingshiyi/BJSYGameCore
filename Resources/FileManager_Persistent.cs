using System;
using System.Threading.Tasks;
using System.IO;
using System.Text;
using UnityEngine;
using System.Linq;
using System.Threading;
using UnityEngine.Networking;
namespace BJSYGameCore
{
    /// <summary>
    /// 一个跨平台的可持续化文件管理系统。
    /// </summary>
    public partial class FileManager
    {
        /// <summary>
        /// 获取可持续化文件的路径
        /// </summary>
        /// <param name="path">相对路径</param>
        /// <returns>可持续化文件的绝对路径</returns>
        string getPersistentFilePath(string path)
        {
            return Path.Combine(Application.persistentDataPath, path);
        }
        /// <summary>
        /// 将文本保存为文件到相对路径。
        /// </summary>
        /// <param name="path">文件保存根目录下的相对路径</param>
        /// <param name="text">文本内容</param>
        /// <returns>当文件写入完毕时返回</returns>
        public Task savePersistentFile(string path, string text)
        {
            return savePersistentFile(path, new string[] { text });
        }
        /// <summary>
        /// 将文本保存为文件到相对路径
        /// </summary>
        /// <param name="path">相对路径</param>
        /// <param name="lines">文件内容</param>
        /// <returns></returns>
        public Task savePersistentFile(string path, string[] lines)
        {
            path = getPersistentFilePath(path);
            return saveFile(path, lines);
        }
        /// <summary>
        /// 将二进制数据保存为文件到相对路径。
        /// </summary>
        /// <param name="path">文件保存根目录下的相对路径</param>
        /// <param name="bytes">二进制数据</param>
        /// <returns>当文件写入完毕时返回</returns>
        public Task savePersistentFile(string path, byte[] bytes)
        {
            path = getPersistentFilePath(path);
            return saveFile(path, bytes);
        }
        /// <summary>
        /// 是否存在指定文件？
        /// </summary>
        /// <param name="path">相对路径</param>
        /// <returns>是否存在？</returns>
        public bool isPersistentFileExist(string path)
        {
            path = getPersistentFilePath(path);
            return isFileExist(path);
        }
        /// <summary>
        /// 删除相对路径上的文件。
        /// </summary>
        /// <param name="path">相对路径</param>
        /// <returns>是否存在文件被删除了？</returns>
        public bool deletePersistentFile(string path)
        {
            path = getPersistentFilePath(path);
            return deleteFile(path);
        }
        /// <summary>
        /// 获取某个目录下的所有符合通配符条件的文件路径。
        /// </summary>
        /// <param name="dir">文件存储根目录下的相对路径</param>
        /// <param name="filter">通配符过滤字符串</param>
        /// <param name="includeChildDir">是否包含子文件夹中的文件？</param>
        /// <returns>所有符合条件的文件路径</returns>
        public string[] getPersistentFiles(string dir, string filter, bool includeChildDir)
        {
            dir = getPersistentFilePath(dir);
            if (!Directory.Exists(dir))
                return new string[0];
            if (includeChildDir)
            {
                return Directory.GetFiles(dir, filter, SearchOption.AllDirectories);
            }
            else
            {
                return Directory.GetFiles(dir, filter, SearchOption.TopDirectoryOnly);
            }
        }
        /// <summary>
        /// 读取某个文本文件的内容
        /// </summary>
        /// <param name="path">文件相对路径</param>
        /// <returns>当文件读取完毕时返回其文本内容</returns>
        /// <exception cref="FileLoadException">当目标文件不是文本文件的时候抛出该异常。</exception>
        public Task<string> readPersistentTextFile(string path)
        {
            path = getPersistentFilePath(path);
            try
            {
                using (StreamReader sw = new StreamReader(path))
                {
                    return sw.ReadToEndAsync();
                }
            }
            catch (FileLoadException)
            {
                throw new FileLoadException("Invalid Text File!!");
            }
        }
        public Task<string[]> readPersistentTextFile(string path, int startLine = 0, int lineCount = 1, CancellationToken? cancelToken = null)
        {
            path = getPersistentFilePath(path);
            return readTextFile(path, startLine, lineCount, cancelToken);
        }
        /// <summary>
        /// 读取一组文件的指定数目行
        /// </summary>
        /// <param name="paths"></param>
        /// <param name="lineCount"></param>
        /// <param name="cancelToken"></param>
        /// <returns></returns>
        public Task<string[][]> readPersistentTextFiles(string[] paths, int startLine = 0, int lineCount = 1, CancellationToken? cancelToken = null)
        {
            Task<string[]>[] tasks = new Task<string[]>[paths.Length];
            for (int i = 0; i < paths.Length; i++)
            {
                if (cancelToken != null && cancelToken.Value.IsCancellationRequested)
                    return Task.WhenAll(tasks.Take(i));
                tasks[i] = readPersistentTextFile(paths[i], startLine, lineCount, cancelToken);
            }
            return Task.WhenAll(tasks);
        }
        /// <summary>
        /// 读取某个二进制文件的数据
        /// </summary>
        /// <param name="path">文件相对路径</param>
        /// <returns>当文件读取完毕时返回其二进制数据</returns>
        /// <exception cref="FileLoadException">当目标文件不是二进制文件的时候抛出该异常。</exception>
        public Task<byte[]> readPersistentBinaryFile(string path)
        {
            path = getPersistentFilePath(path);
            return readBinaryFile(path);
        }
    }
}
