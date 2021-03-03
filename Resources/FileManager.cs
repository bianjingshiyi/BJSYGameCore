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
            throw new NotImplementedException();
        }
        /// <summary>
        /// 将二进制数据保存为文件到相对路径。
        /// </summary>
        /// <param name="path">文件保存根目录下的相对路径</param>
        /// <param name="bytes">二进制数据</param>
        /// <returns>当文件写入完毕时返回</returns>
        public Task saveFile(string path, byte[] bytes)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// 是否存在指定文件？
        /// </summary>
        /// <param name="path">相对路径</param>
        /// <returns>是否存在？</returns>
        public bool isFileExist(string path)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// 删除相对路径上的文件。
        /// </summary>
        /// <param name="path">相对路径</param>
        /// <returns>是否存在文件被删除了？</returns>
        public bool deleteFile(string path)
        {
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }
        /// <summary>
        /// 读取某个文本文件的内容
        /// </summary>
        /// <param name="path">文件相对路径</param>
        /// <returns>当文件读取完毕时返回其文本内容</returns>
        /// <exception cref="FileLoadException">当目标文件不是文本文件的时候抛出该异常。</exception>
        public Task<string> readTextFromFile(string path)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// 读取某个二进制文件的数据
        /// </summary>
        /// <param name="path">文件相对路径</param>
        /// <returns>当文件读取完毕时返回其二进制数据</returns>
        /// <exception cref="FileLoadException">当目标文件不是二进制文件的时候抛出该异常。</exception>
        public Task<byte[]> readBytesFromFile(string path)
        {
            throw new NotImplementedException();
        }
    }
}
