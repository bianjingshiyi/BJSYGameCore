using System;
using System.Linq;

namespace BJSYGameCore.UI
{
    public abstract class UIPageGroup : UIObject
    {
        public abstract UIObject[] getPages();
        public UIObject getDisplayingPage()
        {
            return getPages().FirstOrDefault(p => p.isDisplaying);
        }
        /// <summary>
        /// 显示一个页面，会将其他页面隐藏。
        /// </summary>
        /// <param name="page">参数必须是该PageGroup的子页面</param>
        public void display(UIObject page)
        {
            UIObject[] pages = getPages();
            if (pages.Contains(page))
            {
                foreach (UIObject p in pages)
                {
                    if (p == page)
                        p.display();
                    else
                        p.hide();
                }
            }
        }
    }
}