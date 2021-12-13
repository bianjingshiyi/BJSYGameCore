using UnityEngine;
using System.Threading.Tasks;
using UnityEngine.Networking;
using System.IO;
using System.Text;

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
            UnityWebRequest.Get(Application.streamingAssetsPath + "/" + path).SendWebRequest().completed += op =>
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
        public Task<string> readStreamingTextFile(string path)
        {
            TaskCompletionSource<string> tcs = new TaskCompletionSource<string>();
            UnityWebRequest.Get(Application.streamingAssetsPath + "/" + path).SendWebRequest().completed += op =>
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
                tcs.SetResult(text);
            };
            return tcs.Task;
        }
    }
}
