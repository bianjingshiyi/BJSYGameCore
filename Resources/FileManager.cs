using System;
using System.Threading.Tasks;
using System.IO;

namespace BJSYGameCore
{
    /// <summary>
    /// 一个跨平台的可持续化文件管理系统。
    /// </summary>
    public class FileManager
    {
        /// <summary>
        /// 将文本保存为文件到相对路径。
        /// </summary>
        /// <param name="path">文件保存根目录下的相对路径</param>
        /// <param name="text">文本内容</param>
        /// <returns>当文件写入完毕时返回</returns>
        public Task saveFile(string path, string text)
        {
            return Task.Run(() => File.WriteAllText(path, text, System.Text.Encoding.UTF8));
        }
        /// <summary>
        /// 将二进制数据保存为文件到相对路径。
        /// </summary>
        /// <param name="path">文件保存根目录下的相对路径</param>
        /// <param name="bytes">二进制数据</param>
        /// <returns>当文件写入完毕时返回</returns>
        public Task saveFile(string path, byte[] bytes)
        {
            return Task.Run(() => File.WriteAllBytes(path, bytes));
        }
        /// <summary>
        /// 是否存在指定文件？
        /// </summary>
        /// <param name="path">相对路径</param>
        /// <returns>是否存在？</returns>
        public bool isFileExist(string path)
        {
            if (Directory.Exists(path)) { return true; }
            else if (File.Exists(path)) { return true; }
            else { return false; }
        }
        /// <summary>
        /// 删除相对路径上的文件。
        /// </summary>
        /// <param name="path">相对路径</param>
        /// <returns>是否存在文件被删除了？</returns>
        public bool deleteFile(string path)
        {
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
            if (filter == "*" || filter == "?")
            {
                if (includeChildDir) { return Directory.GetFiles(dir, filter, SearchOption.AllDirectories); }
                else { return Directory.GetFiles(dir, filter, SearchOption.TopDirectoryOnly); }
            }
            throw new ArgumentException("Invalid Filter Character!!!");
        }
        /// <summary>
        /// 读取某个文本文件的内容
        /// </summary>
        /// <param name="path">文件相对路径</param>
        /// <returns>当文件读取完毕时返回其文本内容</returns>
        /// <exception cref="FileLoadException">当目标文件不是文本文件的时候抛出该异常。</exception>
        public Task<string> readTextFile(string path)
        {
            try
            {
                return Task.Run(() => File.ReadAllText(path));
            }
            catch (FileLoadException)
            {
                throw new FileLoadException("Invalid Text File!!");
            }
        }
        /// <summary>
        /// 读取某个二进制文件的数据
        /// </summary>
        /// <param name="path">文件相对路径</param>
        /// <returns>当文件读取完毕时返回其二进制数据</returns>
        /// <exception cref="FileLoadException">当目标文件不是二进制文件的时候抛出该异常。</exception>
        public Task<byte[]> readBinaryFile(string path)
        {
            try
            {
                return Task.Run(() => File.ReadAllBytes(path));
            }
            catch (FileLoadException)
            {
                throw new FileLoadException("Invalid Binary File!!");
            }
        }
        /// <summary>
        /// 读取某个文本文件的指定行
        /// </summary>
        /// <param name="path">文件相对路径</param>
        /// <param name="startLine">从第几行开始，0代表第一行</param>
        /// <param name="count">读多少行</param>
        /// <returns>当指定行读取完毕时返回</returns>
        public Task<string[]> readTextFile(string path, int startLine, int count)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// 读取某个二进制文件的指定区
        /// </summary>
        /// <param name="path">文件相对路径</param>
        /// <param name="offset">从第几个byte开始，0代表从头开始</param>
        /// <param name="length">读取的长度</param>
        /// <returns>当指定区域读取完毕时返回数组</returns>
        public Task<byte[]> readBinaryFile(string path, int offset, int length)
        {
            throw new NotImplementedException();
        }
    }
}
