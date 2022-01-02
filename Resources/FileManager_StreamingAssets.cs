using UnityEngine;
using System.Threading.Tasks;
using UnityEngine.Networking;
using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Threading;
namespace BJSYGameCore
{
    partial class FileManager
    {
        /// <summary>
        /// 从StreamingAssets中读取二进制文件的数据
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public Task<byte[]> readStreamingBinaryFile(string path)
        {
            TaskCompletionSource<byte[]> tcs = new TaskCompletionSource<byte[]>();
            UnityWebRequest.Get(Path.Combine(Application.streamingAssetsPath, path)).SendWebRequest().completed += op =>
            {
                UnityWebRequestAsyncOperation operation = op as UnityWebRequestAsyncOperation;
                if (operation.webRequest.isHttpError)
                {
                    if (operation.webRequest.responseCode == 404)
                    {
                        //发生错误，没有找到文件
                        tcs.SetException(new FileNotFoundException($"Unable to load file {path}", path));
                    }
                    else
                    {
                        //发生错误
                        tcs.SetException(new IOException("读取文件" + path + "失败，错误信息：" + operation.webRequest.error));
                    }
                    return;
                }
                else if (operation.webRequest.isNetworkError)
                {
                    //发生错误
                    tcs.SetException(new IOException("读取文件" + path + "失败，错误信息：" + operation.webRequest.error));
                    return;
                }
                byte[] bytes = operation.webRequest.downloadHandler.data;
                tcs.SetResult(bytes);
            };
            return tcs.Task;
        }
        /// <summary>
        /// 从StreamingAssets中读取文本文件的文本，注意返回的格式为UTF8
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public Task<string[]> readStreamingTextFile(string path, int startLine = 0, int lineCount = 1, CancellationToken? cancelToken = null)
        {
            if (path.StartsWith("sa:"))
                path = path.Substring(3, path.Length - 3);
            TaskCompletionSource<string[]> tcs = new TaskCompletionSource<string[]>();
            UnityWebRequest.Get(Path.Combine(Application.streamingAssetsPath, path)).SendWebRequest().completed += op =>
            {
                UnityWebRequestAsyncOperation operation = op as UnityWebRequestAsyncOperation;
                if (operation.webRequest.isHttpError)
                {
                    if (operation.webRequest.responseCode == 404)
                    {
                        //发生错误，没有找到文件
                        tcs.SetException(new FileNotFoundException($"Unable to load file {path}", path));
                    }
                    else
                    {
                        //发生错误
                        tcs.SetException(new IOException("读取文件" + path + "失败，错误信息：" + operation.webRequest.error));
                    }
                    return;
                }
                else if (operation.webRequest.isNetworkError)
                {
                    //发生错误
                    tcs.SetException(new IOException("读取文件" + path + "失败，错误信息：" + operation.webRequest.error));
                    return;
                }
                //返回的是UTF8
                string text = operation.webRequest.downloadHandler.text;
                string[] headLines = new string[lineCount];
                using (StringReader reader = new StringReader(text))
                {
                    for (int i = 0; i < startLine; i++)
                    {
                        if (cancelToken != null && cancelToken.Value.IsCancellationRequested)
                        {
                            tcs.SetResult(headLines);
                            return;
                        }
                        reader.ReadLine();
                    }
                    for (int i = 0; i < lineCount; i++)
                    {
                        if (cancelToken != null && cancelToken.Value.IsCancellationRequested)
                        {
                            tcs.SetResult(headLines);
                            return;
                        }
                        headLines[i] = reader.ReadLine();
                    }
                    tcs.SetResult(headLines);
                }
            };
            return tcs.Task;
        }
        public string[] getStreamingFiles(string dirName, string searchPattern)
        {
            StreamingAssetsInfo streamingAssetsInfo = Resources.LoadAll<StreamingAssetsInfo>("").FirstOrDefault();
            if (streamingAssetsInfo == null)
                throw new NullReferenceException("Resources中无法找到StreamingAssetsInfo");
            return streamingAssetsInfo.getFiles(dirName, searchPattern);
        }
    }
}
