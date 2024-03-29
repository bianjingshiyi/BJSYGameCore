﻿using System.Threading.Tasks;
using System.IO;
using System.Threading;
using System;
using System.Collections.Generic;

namespace BJSYGameCore
{
    partial class FileManager
    {
        public string[] getFiles(string dirName, string searchPattern)
        {
            return Directory.GetFiles(dirName, searchPattern);
        }
        /// <summary>
        /// 读取文本文件
        /// </summary>
        /// <param name="path"></param>
        /// <param name="startLine"></param>
        /// <param name="lineCount">要读取的行数。若设置为0则读到最后一行</param>
        /// <param name="cancelToken"></param>
        /// <returns></returns>
        /// <exception cref="FileLoadException"></exception>
        public Task<string[]> readTextFile(string path, int startLine = 0, int lineCount = 1, CancellationToken? cancelToken = null)
        {
            try
            {
                return Task.Run(() =>
                {
                    using (StreamReader reader = new StreamReader(path))
                    {
                        return readTextLines(reader, startLine, lineCount, cancelToken);
                    }
                });
            }
            catch (FileLoadException e)
            {
                throw new FileLoadException("Invalid Text File!!", e);
            }
        }
        public async Task<string> readTextFileToEnd(string path, int startLine = 0, CancellationToken? cancelToken = null)
        {
            try
            {
                string line = null;
                using (StreamReader reader = new StreamReader(path))
                {
                    for (int i = 0; i < startLine; i++)
                    {
                        if (cancelToken != null && cancelToken.Value.IsCancellationRequested)
                            return line;
                        await reader.ReadLineAsync();
                    }
                    line = await reader.ReadToEndAsync();
                }
                return line;
            }
            catch (FileLoadException e)
            {
                throw new FileLoadException("Invalid Text File!!", e);
            }
        }
        /// <summary>
        /// 读取某个二进制文件的数据
        /// </summary>
        /// <param name="path">文件相对路径</param>
        /// <returns>当文件读取完毕时返回其二进制数据</returns>
        /// <exception cref="FileLoadException">当目标文件不是二进制文件的时候抛出该异常。</exception>
        public async Task<byte[]> readBinaryFile(string path)
        {
            try
            {
                using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
                {
                    byte[] buffer = new byte[fs.Length];
                    await fs.ReadAsync(buffer, 0, (int)fs.Length);
                    return buffer;
                }
            }
            catch (FileLoadException)
            {
                throw new FileLoadException("Invalid Binary File!!");
            }
        }
        /// <summary>
        /// 将二进制数据保存为文件。
        /// </summary>
        /// <param name="path">文件保存根目录下的相对路径</param>
        /// <param name="bytes">二进制数据</param>
        /// <returns>当文件写入完毕时返回</returns>
        public async Task saveFile(string path, byte[] bytes)
        {
            string dir = Path.GetDirectoryName(path);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            using (FileStream fs = new FileStream(path, FileMode.Create))
            {
                await fs.WriteAsync(bytes, 0, bytes.Length);
            }
        }
        /// <summary>
        /// 将文本保存为文件
        /// </summary>
        /// <param name="path">相对路径</param>
        /// <param name="lines">文件内容</param>
        /// <returns></returns>
        public async Task saveFile(string path, string[] lines)
        {
            string dir = Path.GetDirectoryName(path);
            if (!Directory.Exists(dir))
            {
                try
                {
                    Directory.CreateDirectory(dir);
                }
                catch (UnauthorizedAccessException)
                {
                    throw new UnauthorizedAccessException("无法创建文件夹" + dir + "，因为没有访问权限");
                }
            }
            using (StreamWriter sw = new StreamWriter(path))
            {
                for (int i = 0; i < lines.Length; i++)
                {
                    await sw.WriteLineAsync(lines[i]);
                }
            }
        }
        /// <summary>
        /// 是否存在指定文件？
        /// </summary>
        /// <param name="path">相对路径</param>
        /// <returns>是否存在？</returns>
        public bool isFileExist(string path)
        {
            return File.Exists(path);
        }
        /// <summary>
        /// 删除文件。
        /// </summary>
        /// <param name="path">相对路径</param>
        /// <returns>是否存在文件被删除了？</returns>
        public bool deleteFile(string path)
        {
            if (isFileExist(path))
            {
                File.Delete(path);
                return true;
            }
            else
                return false;
        }

        private string[] readTextLines(TextReader reader, int startLine, int lineCount, CancellationToken? cancelToken)
        {
            string[] headLines = new string[lineCount];
            List<string> lines = lineCount == 0 ? new List<string>() : null;

            for (int i = 0; i < startLine; i++)
            {
                if (cancelToken != null && cancelToken.Value.IsCancellationRequested)
                    return headLines;
                reader.ReadLine();
            }
            for (int i = 0; i < lineCount; i++)
            {
                if (cancelToken != null && cancelToken.Value.IsCancellationRequested)
                    return headLines;
                headLines[i] = reader.ReadLine();
            }
            if (lineCount == 0)
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    lines.Add(line);
                    if (cancelToken != null && cancelToken.Value.IsCancellationRequested)
                        return lines.ToArray();
                }
            }
            return lineCount == 0 ? lines.ToArray() : headLines;
        }
    }
}
