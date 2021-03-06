﻿using System;
using System.Threading.Tasks;
using System.IO;
using System.Text;
using UnityEngine;
using System.Linq;
using System.Threading;

namespace BJSYGameCore
{
    /// <summary>
    /// 一个跨平台的可持续化文件管理系统。
    /// </summary>
    public class FileManager
    {
        /// <summary>
        /// 获取相对路径的绝对路径
        /// </summary>
        /// <param name="path">相对路径</param>
        /// <returns>绝对路径</returns>
        string getFullPath(string path)
        {
            return Path.Combine(Application.persistentDataPath, path);
        }

        /// <summary>
        /// 将文本保存为文件到相对路径。
        /// </summary>
        /// <param name="path">文件保存根目录下的相对路径</param>
        /// <param name="text">文本内容</param>
        /// <returns>当文件写入完毕时返回</returns>
        public Task saveFile(string path, string text)
        {
            return saveFile(path, new string[] { text });
        }
        public async Task saveFile(string path, string[] lines)
        {
            path = getFullPath(path);
            string dir = Path.GetDirectoryName(path);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            using (StreamWriter sw = new StreamWriter(path))
            {
                for (int i = 0; i < lines.Length; i++)
                {
                    await sw.WriteLineAsync(lines[i]);
                }
            }
        }
        /// <summary>
        /// 将二进制数据保存为文件到相对路径。
        /// </summary>
        /// <param name="path">文件保存根目录下的相对路径</param>
        /// <param name="bytes">二进制数据</param>
        /// <returns>当文件写入完毕时返回</returns>
        public Task saveFile(string path, byte[] bytes)
        {
            path = getFullPath(path);
            FileStream fs = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write);
            return fs.WriteAsync(bytes, 0, bytes.Length);
        }
        /// <summary>
        /// 是否存在指定文件？
        /// </summary>
        /// <param name="path">相对路径</param>
        /// <returns>是否存在？</returns>
        public bool isFileExist(string path)
        {
            path = getFullPath(path);
            if (File.Exists(path)) { return true; }
            else { return false; }
        }
        /// <summary>
        /// 删除相对路径上的文件。
        /// </summary>
        /// <param name="path">相对路径</param>
        /// <returns>是否存在文件被删除了？</returns>
        public bool deleteFile(string path)
        {
            path = getFullPath(path);
            bool res = isFileExist(path);
            if (res)
            {
                File.Delete(path);
                return res;
            }
            else { return !res; }
        }
        /// <summary>
        /// 获取某个目录下的所有符合通配符条件的文件路径。
        /// </summary>
        /// <param name="dir">文件存储根目录下的相对路径</param>
        /// <param name="filter">通配符过滤字符串</param>
        /// <param name="includeChildDir">是否包含子文件夹中的文件？</param>
        /// <returns>所有符合条件的文件路径</returns>
        public string[] getFiles(string dir, string filter, bool includeChildDir)
        {
            dir = getFullPath(dir);
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
        public Task<string> readTextFile(string path)
        {
            path = getFullPath(path);
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
        public async Task<string[]> readTextFile(string path, int startLine = 0, int lineCount = 1, CancellationToken? cancelToken = null)
        {
            path = getFullPath(path);
            try
            {
                string[] headLines = new string[lineCount];
                using (StreamReader reader = new StreamReader(path))
                {
                    for (int i = 0; i < startLine; i++)
                    {
                        if (cancelToken != null && cancelToken.Value.IsCancellationRequested)
                            return headLines;
                        await reader.ReadLineAsync();
                    }
                    for (int i = 0; i < lineCount; i++)
                    {
                        if (cancelToken != null && cancelToken.Value.IsCancellationRequested)
                            return headLines;
                        headLines[i] = await reader.ReadLineAsync();
                    }
                }
                return headLines;
            }
            catch (FileLoadException e)
            {
                throw new FileLoadException("Invalid Text File!!", e);
            }
        }
        /// <summary>
        /// 读取一组文件的指定数目行
        /// </summary>
        /// <param name="paths"></param>
        /// <param name="lineCount"></param>
        /// <param name="cancelToken"></param>
        /// <returns></returns>
        public Task<string[][]> readTextFiles(string[] paths, int startLine = 0, int lineCount = 1, CancellationToken? cancelToken = null)
        {
            Task<string[]>[] tasks = new Task<string[]>[paths.Length];
            for (int i = 0; i < paths.Length; i++)
            {
                if (cancelToken != null && cancelToken.Value.IsCancellationRequested)
                    return Task.WhenAll(tasks.Take(i));
                tasks[i] = readTextFile(paths[i], startLine, lineCount, cancelToken);
            }
            return Task.WhenAll(tasks);
        }
        /// <summary>
        /// 读取某个二进制文件的数据
        /// </summary>
        /// <param name="path">文件相对路径</param>
        /// <returns>当文件读取完毕时返回其二进制数据</returns>
        /// <exception cref="FileLoadException">当目标文件不是二进制文件的时候抛出该异常。</exception>
        public Task<byte[]> readBinaryFile(string path)
        {
            path = getFullPath(path);
            try
            {
                TaskCompletionSource<byte[]> tcs = new TaskCompletionSource<byte[]>();
                FileStream fs = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Read);
                byte[] buffer = new byte[fs.Length];
                fs.ReadAsync(buffer, 0, (int)fs.Length).GetAwaiter().OnCompleted(() => tcs.SetResult(buffer));
                return tcs.Task;
            }
            catch (FileLoadException)
            {
                throw new FileLoadException("Invalid Binary File!!");
            }
        }
    }
}
