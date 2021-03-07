using System;
using UnityEngine;

namespace BJSYGameCore
{
    public class PlatformCompability
    {
        public static PlatformCompability Current => new PlatformCompability();

        PlatformCompability()
        {
            currentPlatform = Application.platform;
            is64Bit = IntPtr.Size == 8;
        }

        readonly RuntimePlatform currentPlatform;
        readonly bool is64Bit;
        
        /// <summary>
        /// 当前平台
        /// </summary>
        public RuntimePlatform Platform => currentPlatform;

        /// <summary>
        /// 是否是64位平台
        /// </summary>
        public bool Is64Bit => is64Bit;

        public PlatformCompability(RuntimePlatform rtp)
        {
            currentPlatform = rtp;
            is64Bit = IntPtr.Size == 8;
        }

        /// <summary>
        /// 读取文件是否需要WebRequest
        /// </summary>
        /// <returns></returns>
        public bool RequireWebRequest => currentPlatform == RuntimePlatform.Android;

        /// <summary>
        /// 是否支持直接读取Excel文件
        /// </summary>
        public bool SupportExcelReading => currentPlatform != RuntimePlatform.Android;
    }
}
