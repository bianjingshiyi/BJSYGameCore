namespace BJSYGameCore
{
    public interface ILanguageManager
    {
        /// <summary>
        /// 获取所有语言对应的名称
        /// </summary>
        /// <returns></returns>
        string[] getLanguages();
        /// <summary>
        /// 获取默认语言名称
        /// </summary>
        /// <returns></returns>
        string getDefaultLanguage();
        /// <summary>
        /// 获取当前语言名称
        /// </summary>
        /// <returns></returns>
        string getLanguage();
        /// <summary>
        /// 设置当前语言名称
        /// </summary>
        void setLanguage(string cultureName);
        /// <summary>
        /// 获取对应语言的字符串
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        string getString(string str);
    }
    public interface IAppManager
    {
        ILanguageManager langManager { get; }
        FileManager fileManager { get; }
    }
}
