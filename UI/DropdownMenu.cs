using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using System.Collections;
using OptionDataList = UnityEngine.UI.Dropdown.OptionDataList;
using OptionData = UnityEngine.UI.Dropdown.OptionData;
namespace BJSYGameCore.UI
{
    [AddComponentMenu("UI/Dropdown Menu", 37)]
    [RequireComponent(typeof(RectTransform))]
    /// <summary>
    /// 可以选中多个选项的下拉菜单
    /// </summary>
    public class DropdownMenu : Selectable, IPointerClickHandler, ISubmitHandler, ICancelHandler
    {
        #region 公有方法
        /// <summary>
        /// Add multiple options to the options of the Dropdown based on a list of OptionData objects.
        /// </summary>
        /// <param name="options">The list of OptionData to add.</param>
        /// /// <remarks>
        /// See AddOptions(List<string> options) for code example of usages.
        /// </remarks>
        public void AddOptions(List<OptionData> options)
        {
            this.options.AddRange(options);
        }
        /// <summary>
        /// Add multiple text-only options to the options of the Dropdown based on a list of strings.
        /// </summary>
        /// <remarks>
        /// Add a List of string messages to the Dropdown. The Dropdown shows each member of the list as a separate option.
        /// </remarks>
        /// <param name="options">The list of text strings to add.</param>
        /// <example>
        /// <code>
        /// //Create a new Dropdown GameObject by going to the Hierarchy and clicking Create>UI>Dropdown. Attach this script to the Dropdown GameObject.
        ///
        /// using System.Collections.Generic;
        /// using UnityEngine;
        /// using UnityEngine.UI;
        ///
        /// public class Example : MonoBehaviour
        /// {
        ///     //Create a List of new Dropdown options
        ///     List<string> m_DropOptions = new List<string> { "Option 1", "Option 2"};
        ///     //This is the Dropdown
        ///     Dropdown m_Dropdown;
        ///
        ///     void Start()
        ///     {
        ///         //Fetch the Dropdown GameObject the script is attached to
        ///         m_Dropdown = GetComponent<Dropdown>();
        ///         //Clear the old options of the Dropdown menu
        ///         m_Dropdown.ClearOptions();
        ///         //Add the options created in the List above
        ///         m_Dropdown.AddOptions(m_DropOptions);
        ///     }
        /// }
        /// </code>
        /// </example>
        public void AddOptions(List<string> options)
        {
            for (int i = 0; i < options.Count; i++)
                this.options.Add(new OptionData(options[i]));
        }
        /// <summary>
        /// Add multiple image-only options to the options of the Dropdown based on a list of Sprites.
        /// </summary>
        /// <param name="options">The list of Sprites to add.</param>
        /// <remarks>
        /// See AddOptions(List<string> options) for code example of usages.
        /// </remarks>
        public void AddOptions(List<Sprite> options)
        {
            for (int i = 0; i < options.Count; i++)
                this.options.Add(new OptionData(options[i]));
        }
        /// <summary>
        /// Clear the list of options in the Dropdown.
        /// </summary>
        public void ClearOptions()
        {
            options.Clear();
        }
        /// <summary>
        /// Show the dropdown.
        ///
        /// Plan for dropdown scrolling to ensure dropdown is contained within screen.
        ///
        /// We assume the Canvas is the screen that the dropdown must be kept inside.
        /// This is always valid for screen space canvas modes.
        /// For world space canvases we don't know how it's used, but it could be e.g. for an in-game monitor.
        /// We consider it a fair constraint that the canvas must be big enough to contain dropdowns.
        /// </summary>
        public void Show(bool forceUpdate = false)
        {
            if (!forceUpdate && (!IsActive() || !IsInteractable() || _dropdown != null))
                return;

            // Get root Canvas.
            var list = ListPool<Canvas>.Get();
            gameObject.GetComponentsInParent(false, list);
            if (list.Count == 0)
                return;

            // case 1064466 rootCanvas should be last element returned by GetComponentsInParent()
            Canvas rootCanvas = list[list.Count - 1];
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].isRootCanvas)
                {
                    rootCanvas = list[i];
                    break;
                }
            }

            ListPool<Canvas>.Release(list);

            if (!validTemplate)
            {
                SetupTemplate();
                if (!validTemplate)
                    return;
            }

            _template.gameObject.SetActive(true);

            // popupCanvas used to assume the root canvas had the default sorting Layer, next line fixes (case 958281 - [UI] Dropdown list does not copy the parent canvas layer when the panel is opened)
            _template.GetComponent<Canvas>().sortingLayerID = rootCanvas.sortingLayerID;

            // Instantiate the drop-down template
            _dropdown = CreateDropdownList(_template.gameObject);
            _dropdown.name = "Dropdown List";
            _dropdown.SetActive(true);

            // Make drop-down RectTransform have same values as original.
            RectTransform dropdownRectTransform = _dropdown.transform as RectTransform;
            dropdownRectTransform.SetParent(_template.transform.parent, false);

            // Instantiate the drop-down list items

            // Find the dropdown item and disable it.
            DropdownMenuItem itemTemplate = _dropdown.GetComponentInChildren<DropdownMenuItem>();

            GameObject content = itemTemplate.rectTransform.parent.gameObject;
            RectTransform contentRectTransform = content.transform as RectTransform;
            itemTemplate.rectTransform.gameObject.SetActive(true);

            // Get the rects of the dropdown and item
            Rect dropdownContentRect = contentRectTransform.rect;
            Rect itemTemplateRect = itemTemplate.rectTransform.rect;

            // Calculate the visual offset between the item's edges and the background's edges
            Vector2 offsetMin = itemTemplateRect.min - dropdownContentRect.min + (Vector2)itemTemplate.rectTransform.localPosition;
            Vector2 offsetMax = itemTemplateRect.max - dropdownContentRect.max + (Vector2)itemTemplate.rectTransform.localPosition;
            Vector2 itemSize = itemTemplateRect.size;

            _itemList.Clear();

            Button prev = null;
            for (int i = 0; i < options.Count; ++i)
            {
                OptionData data = options[i];
                DropdownMenuItem item = AddItem(data, itemTemplate, _itemList);
                if (item == null)
                    continue;

                // Automatically set up a toggle state change listener
                item.button.onClick.AddListener(() => OnSelectItem(item.button));

                // Automatically set up explicit navigation
                if (prev != null)
                {
                    Navigation prevNav = prev.navigation;
                    Navigation toggleNav = item.button.navigation;
                    prevNav.mode = Navigation.Mode.Explicit;
                    toggleNav.mode = Navigation.Mode.Explicit;

                    prevNav.selectOnDown = item.button;
                    prevNav.selectOnRight = item.button;
                    toggleNav.selectOnLeft = prev;
                    toggleNav.selectOnUp = prev;

                    prev.navigation = prevNav;
                    item.button.navigation = toggleNav;
                }
                prev = item.button;
            }

            // Reposition all items now that all of them have been added
            Vector2 sizeDelta = contentRectTransform.sizeDelta;
            sizeDelta.y = itemSize.y * _itemList.Count + offsetMin.y - offsetMax.y;
            contentRectTransform.sizeDelta = sizeDelta;

            float extraSpace = dropdownRectTransform.rect.height - contentRectTransform.rect.height;
            if (extraSpace > 0)
                dropdownRectTransform.sizeDelta = new Vector2(dropdownRectTransform.sizeDelta.x, dropdownRectTransform.sizeDelta.y - extraSpace);

            // Invert anchoring and position if dropdown is partially or fully outside of canvas rect.
            // Typically this will have the effect of placing the dropdown above the button instead of below,
            // but it works as inversion regardless of initial setup.
            Vector3[] corners = new Vector3[4];
            dropdownRectTransform.GetWorldCorners(corners);

            RectTransform rootCanvasRectTransform = rootCanvas.transform as RectTransform;
            Rect rootCanvasRect = rootCanvasRectTransform.rect;
            for (int axis = 0; axis < 2; axis++)
            {
                bool outside = false;
                for (int i = 0; i < 4; i++)
                {
                    Vector3 corner = rootCanvasRectTransform.InverseTransformPoint(corners[i]);
                    if ((corner[axis] < rootCanvasRect.min[axis] && !Mathf.Approximately(corner[axis], rootCanvasRect.min[axis])) ||
                        (corner[axis] > rootCanvasRect.max[axis] && !Mathf.Approximately(corner[axis], rootCanvasRect.max[axis])))
                    {
                        outside = true;
                        break;
                    }
                }
                if (outside)
                    RectTransformUtility.FlipLayoutOnAxis(dropdownRectTransform, axis, false, false);
            }

            for (int i = 0; i < _itemList.Count; i++)
            {
                RectTransform itemRect = _itemList[i].rectTransform;
                itemRect.anchorMin = new Vector2(itemRect.anchorMin.x, 0);
                itemRect.anchorMax = new Vector2(itemRect.anchorMax.x, 0);
                itemRect.anchoredPosition = new Vector2(itemRect.anchoredPosition.x, offsetMin.y + itemSize.y * (_itemList.Count - 1 - i) + itemSize.y * itemRect.pivot.y);
                itemRect.sizeDelta = new Vector2(itemRect.sizeDelta.x, itemSize.y);
            }

            // Fade in the popup
            AlphaFadeList(_alphaFadeSpeed, 0f, 1f);

            // Make drop-down template and item template inactive
            _template.gameObject.SetActive(false);
            itemTemplate.gameObject.SetActive(false);

            _blocker = CreateBlocker(rootCanvas);
        }
        /// <summary>
        /// Hide the dropdown list. I.e. close it.
        /// </summary>
        public void Hide()
        {
            if (_dropdown != null)
            {
                AlphaFadeList(_alphaFadeSpeed, 0f);

                // User could have disabled the dropdown during the OnValueChanged call.
                if (IsActive())
                    StartCoroutine(DelayedDestroyDropdownList(_alphaFadeSpeed));
            }
            if (_blocker != null)
                DestroyBlocker(_blocker);
            _blocker = null;
            Select();
        }
        #region 事件回调
        /// <summary>
        /// Handling for when the dropdown is initially 'clicked'. Typically shows the dropdown
        /// </summary>
        /// <param name="eventData">The asocciated event data.</param>
        public virtual void OnPointerClick(PointerEventData eventData)
        {
            Show();
        }

        /// <summary>
        /// Handling for when the dropdown is selected and a submit event is processed. Typically shows the dropdown
        /// </summary>
        /// <param name="eventData">The asocciated event data.</param>
        public virtual void OnSubmit(BaseEventData eventData)
        {
            Show();
        }

        /// <summary>
        /// This will hide the dropdown list.
        /// </summary>
        /// <remarks>
        /// Called by a BaseInputModule when a Cancel event occurs.
        /// </remarks>
        /// <param name="eventData">The asocciated event data.</param>
        public virtual void OnCancel(BaseEventData eventData)
        {
            Hide();
        }
        #endregion
        #endregion
        #region 私有方法
        protected DropdownMenu()
        { }
        #region 生命周期
        protected override void Awake()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
                return;
#endif

            m_AlphaTweenRunner = new TweenRunner<FloatTween>();
            m_AlphaTweenRunner.Init(this);

            if (_template)
                _template.gameObject.SetActive(false);
        }

        protected override void Start()
        {
            m_AlphaTweenRunner = new TweenRunner<FloatTween>();
            m_AlphaTweenRunner.Init(this);
            base.Start();
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();

            if (!IsActive())
                return;
        }
#endif
        protected override void OnDisable()
        {
            //Destroy dropdown and blocker in case user deactivates the dropdown when they click an option (case 935649)
            ImmediateDestroyDropdownList();

            if (_blocker != null)
                DestroyBlocker(_blocker);
            _blocker = null;

            base.OnDisable();
        }
        #endregion
        private void SetupTemplate()
        {
            validTemplate = false;

            if (!_template)
            {
                Debug.LogError("The dropdown template is not assigned. The template needs to be assigned and must have a child GameObject with a Toggle component serving as the item.", this);
                return;
            }

            GameObject templateGo = _template.gameObject;
            templateGo.SetActive(true);
            Button itemButton = _template.GetComponentInChildren<Button>();

            validTemplate = true;
            if (!itemButton || itemButton.transform == template)
            {
                validTemplate = false;
                Debug.LogError("The dropdown template is not valid. The template must have a child GameObject with a Toggle component serving as the item.", template);
            }
            else if (!(itemButton.transform.parent is RectTransform))
            {
                validTemplate = false;
                Debug.LogError("The dropdown template is not valid. The child GameObject with a Toggle component (the item) must have a RectTransform on its parent.", template);
            }
            else if (itemText != null && !itemText.transform.IsChildOf(itemButton.transform))
            {
                validTemplate = false;
                Debug.LogError("The dropdown template is not valid. The Item Text must be on the item GameObject or children of it.", template);
            }
            else if (itemImage != null && !itemImage.transform.IsChildOf(itemButton.transform))
            {
                validTemplate = false;
                Debug.LogError("The dropdown template is not valid. The Item Image must be on the item GameObject or children of it.", template);
            }

            if (!validTemplate)
            {
                templateGo.SetActive(false);
                return;
            }

            DropdownMenuItem item = itemButton.gameObject.AddComponent<DropdownMenuItem>();
            item.text = _itemText;
            item.image = _itemImage;
            item.button = itemButton;
            item.rectTransform = (RectTransform)itemButton.transform;

            // Find the Canvas that this dropdown is a part of
            Canvas parentCanvas = null;
            Transform parentTransform = _template.parent;
            while (parentTransform != null)
            {
                parentCanvas = parentTransform.GetComponent<Canvas>();
                if (parentCanvas != null)
                    break;

                parentTransform = parentTransform.parent;
            }

            Canvas popupCanvas = GetOrAddComponent<Canvas>(templateGo);
            popupCanvas.overrideSorting = true;
            popupCanvas.sortingOrder = 30000;

            // If we have a parent canvas, apply the same raycasters as the parent for consistency.
            if (parentCanvas != null)
            {
                Component[] components = parentCanvas.GetComponents<BaseRaycaster>();
                for (int i = 0; i < components.Length; i++)
                {
                    Type raycasterType = components[i].GetType();
                    if (templateGo.GetComponent(raycasterType) == null)
                    {
                        templateGo.AddComponent(raycasterType);
                    }
                }
            }
            else
            {
                GetOrAddComponent<GraphicRaycaster>(templateGo);
            }

            GetOrAddComponent<CanvasGroup>(templateGo);
            templateGo.SetActive(false);

            validTemplate = true;
        }

        private static T GetOrAddComponent<T>(GameObject go) where T : Component
        {
            T comp = go.GetComponent<T>();
            if (!comp)
                comp = go.AddComponent<T>();
            return comp;
        }
        /// <summary>
        /// Create a blocker that blocks clicks to other controls while the dropdown list is open.
        /// </summary>
        /// <remarks>
        /// Override this method to implement a different way to obtain a blocker GameObject.
        /// </remarks>
        /// <param name="rootCanvas">The root canvas the dropdown is under.</param>
        /// <returns>The created blocker object</returns>
        protected virtual GameObject CreateBlocker(Canvas rootCanvas)
        {
            // Create blocker GameObject.
            GameObject blocker = new GameObject("Blocker");

            // Setup blocker RectTransform to cover entire root canvas area.
            RectTransform blockerRect = blocker.AddComponent<RectTransform>();
            blockerRect.SetParent(rootCanvas.transform, false);
            blockerRect.anchorMin = Vector3.zero;
            blockerRect.anchorMax = Vector3.one;
            blockerRect.sizeDelta = Vector2.zero;

            // Make blocker be in separate canvas in same layer as dropdown and in layer just below it.
            Canvas blockerCanvas = blocker.AddComponent<Canvas>();
            blockerCanvas.overrideSorting = true;
            Canvas dropdownCanvas = _dropdown.GetComponent<Canvas>();
            blockerCanvas.sortingLayerID = dropdownCanvas.sortingLayerID;
            blockerCanvas.sortingOrder = dropdownCanvas.sortingOrder - 1;

            // Find the Canvas that this dropdown is a part of
            Canvas parentCanvas = null;
            Transform parentTransform = _template.parent;
            while (parentTransform != null)
            {
                parentCanvas = parentTransform.GetComponent<Canvas>();
                if (parentCanvas != null)
                    break;

                parentTransform = parentTransform.parent;
            }

            // If we have a parent canvas, apply the same raycasters as the parent for consistency.
            if (parentCanvas != null)
            {
                Component[] components = parentCanvas.GetComponents<BaseRaycaster>();
                for (int i = 0; i < components.Length; i++)
                {
                    Type raycasterType = components[i].GetType();
                    if (blocker.GetComponent(raycasterType) == null)
                    {
                        blocker.AddComponent(raycasterType);
                    }
                }
            }
            else
            {
                // Add raycaster since it's needed to block.
                GetOrAddComponent<GraphicRaycaster>(blocker);
            }


            // Add image since it's needed to block, but make it clear.
            Image blockerImage = blocker.AddComponent<Image>();
            blockerImage.color = Color.clear;

            // Add button since it's needed to block, and to close the dropdown when blocking area is clicked.
            Button blockerButton = blocker.AddComponent<Button>();
            blockerButton.onClick.AddListener(Hide);

            return blocker;
        }

        /// <summary>
        /// Convenience method to explicitly destroy the previously generated blocker object
        /// </summary>
        /// <remarks>
        /// Override this method to implement a different way to dispose of a blocker GameObject that blocks clicks to other controls while the dropdown list is open.
        /// </remarks>
        /// <param name="blocker">The blocker object to destroy.</param>
        protected virtual void DestroyBlocker(GameObject blocker)
        {
            Destroy(blocker);
        }

        /// <summary>
        /// Create the dropdown list to be shown when the dropdown is clicked. The dropdown list should correspond to the provided template GameObject, equivalent to instantiating a copy of it.
        /// </summary>
        /// <remarks>
        /// Override this method to implement a different way to obtain a dropdown list GameObject.
        /// </remarks>
        /// <param name="template">The template to create the dropdown list from.</param>
        /// <returns>The created drop down list gameobject.</returns>
        protected virtual GameObject CreateDropdownList(GameObject template)
        {
            return Instantiate(template);
        }

        /// <summary>
        /// Convenience method to explicitly destroy the previously generated dropdown list
        /// </summary>
        /// <remarks>
        /// Override this method to implement a different way to dispose of a dropdown list GameObject.
        /// </remarks>
        /// <param name="dropdownList">The dropdown list GameObject to destroy</param>
        protected virtual void DestroyDropdownList(GameObject dropdownList)
        {
            Destroy(dropdownList);
        }

        /// <summary>
        /// Create a dropdown item based upon the item template.
        /// </summary>
        /// <remarks>
        /// Override this method to implement a different way to obtain an option item.
        /// The option item should correspond to the provided template DropdownItem and its GameObject, equivalent to instantiating a copy of it.
        /// </remarks>
        /// <param name="itemTemplate">e template to create the option item from.</param>
        /// <returns>The created dropdown item component</returns>
        protected virtual DropdownMenuItem CreateItem(DropdownMenuItem itemTemplate)
        {
            return Instantiate(itemTemplate);
        }

        /// <summary>
        ///  Convenience method to explicitly destroy the previously generated Items.
        /// </summary>
        /// <remarks>
        /// Override this method to implement a different way to dispose of an option item.
        /// Likely no action needed since destroying the dropdown list destroys all contained items as well.
        /// </remarks>
        /// <param name="item">The Item to destroy.</param>
        protected virtual void DestroyItem(DropdownMenuItem item)
        { }

        // Add a new drop-down list item with the specified values.
        private DropdownMenuItem AddItem(OptionData data, DropdownMenuItem itemTemplate, List<DropdownMenuItem> items)
        {
            // Add a new item to the dropdown.
            DropdownMenuItem item = CreateItem(itemTemplate);
            item.rectTransform.SetParent(itemTemplate.rectTransform.parent, false);

            item.gameObject.SetActive(true);
            item.gameObject.name = "Item " + items.Count + (data.text != null ? ": " + data.text : "");

            // Set the item's data
            if (item.text)
                item.text.text = data.text;
            if (item.image)
            {
                item.image.sprite = data.image;
                item.image.enabled = (item.image.sprite != null);
            }

            items.Add(item);
            return item;
        }

        private void AlphaFadeList(float duration, float alpha)
        {
            CanvasGroup group = _dropdown.GetComponent<CanvasGroup>();
            AlphaFadeList(duration, group.alpha, alpha);
        }

        private void AlphaFadeList(float duration, float start, float end)
        {
            if (end.Equals(start))
                return;

            FloatTween tween = new FloatTween { duration = duration, startValue = start, targetValue = end };
            tween.AddOnChangedCallback(SetAlpha);
            tween.ignoreTimeScale = true;
            m_AlphaTweenRunner.StartTween(tween);
        }

        private void SetAlpha(float alpha)
        {
            if (!_dropdown)
                return;
            CanvasGroup group = _dropdown.GetComponent<CanvasGroup>();
            group.alpha = alpha;
        }
        private IEnumerator DelayedDestroyDropdownList(float delay)
        {
            yield return new WaitForSecondsRealtime(delay);
            ImmediateDestroyDropdownList();
        }

        private void ImmediateDestroyDropdownList()
        {
            for (int i = 0; i < _itemList.Count; i++)
            {
                if (_itemList[i] != null)
                    DestroyItem(_itemList[i]);
            }
            _itemList.Clear();
            if (_dropdown != null)
                DestroyDropdownList(_dropdown);
            _dropdown = null;
        }

        // Change the value and hide the dropdown.
        private void OnSelectItem(Button button)
        {
            int selectedIndex = -1;
            Transform tr = button.transform;
            Transform parent = tr.parent;
            for (int i = 0; i < parent.childCount; i++)
            {
                if (parent.GetChild(i) == tr)
                {
                    // Subtract one to account for template child.
                    selectedIndex = i - 1;
                    break;
                }
            }
            if (selectedIndex < 0)
                return;
            onClickItem.Invoke(selectedIndex);
            Hide();
        }
        #endregion
        #region 属性字段
        /// <summary>
        /// The Rect Transform of the template for the dropdown list.
        /// </summary>
        public RectTransform template { get { return _template; } set { _template = value; } }
        /// <summary>
        /// The Text component to hold the text of the item.
        /// </summary>
        public Text itemText { get { return _itemText; } set { _itemText = value; } }
        /// <summary>
        /// The Image component to hold the image of the item
        /// </summary>
        public Image itemImage { get { return _itemImage; } set { _itemImage = value; } }
        /// <summary>
        /// The list of possible options. A text string and an image can be specified for each option.
        /// </summary>
        /// <remarks>
        /// This is the list of options within the Dropdown. Each option contains Text and/or image data that you can specify using UI.Dropdown.OptionData before adding to the Dropdown list.
        /// This also unlocks the ability to edit the Dropdown, including the insertion, removal, and finding of options, as well as other useful tools
        /// </remarks>
        /// /// <example>
        /// <code>
        /// //Create a new Dropdown GameObject by going to the Hierarchy and clicking __Create__>__UI__>__Dropdown__. Attach this script to the Dropdown GameObject.
        ///
        /// using UnityEngine;
        /// using UnityEngine.UI;
        /// using System.Collections.Generic;
        ///
        /// public class Example : MonoBehaviour
        /// {
        ///     //Use these for adding options to the Dropdown List
        ///     Dropdown.OptionData m_NewData, m_NewData2;
        ///     //The list of messages for the Dropdown
        ///     List<Dropdown.OptionData> m_Messages = new List<Dropdown.OptionData>();
        ///
        ///
        ///     //This is the Dropdown
        ///     Dropdown m_Dropdown;
        ///     string m_MyString;
        ///     int m_Index;
        ///
        ///     void Start()
        ///     {
        ///         //Fetch the Dropdown GameObject the script is attached to
        ///         m_Dropdown = GetComponent<Dropdown>();
        ///         //Clear the old options of the Dropdown menu
        ///         m_Dropdown.ClearOptions();
        ///
        ///         //Create a new option for the Dropdown menu which reads "Option 1" and add to messages List
        ///         m_NewData = new Dropdown.OptionData();
        ///         m_NewData.text = "Option 1";
        ///         m_Messages.Add(m_NewData);
        ///
        ///         //Create a new option for the Dropdown menu which reads "Option 2" and add to messages List
        ///         m_NewData2 = new Dropdown.OptionData();
        ///         m_NewData2.text = "Option 2";
        ///         m_Messages.Add(m_NewData2);
        ///
        ///         //Take each entry in the message List
        ///         foreach (Dropdown.OptionData message in m_Messages)
        ///         {
        ///             //Add each entry to the Dropdown
        ///             m_Dropdown.options.Add(message);
        ///             //Make the index equal to the total number of entries
        ///             m_Index = m_Messages.Count - 1;
        ///         }
        ///     }
        ///
        ///     //This OnGUI function is used here for a quick demonstration. See the [[wiki:UISystem|UI Section]] for more information about setting up your own UI.
        ///     void OnGUI()
        ///     {
        ///         //TextField for user to type new entry to add to Dropdown
        ///         m_MyString = GUI.TextField(new Rect(0, 40, 100, 40), m_MyString);
        ///
        ///         //Press the "Add" Button to add a new entry to the Dropdown
        ///         if (GUI.Button(new Rect(0, 0, 100, 40), "Add"))
        ///         {
        ///             //Make the index the last number of entries
        ///             m_Index = m_Messages.Count;
        ///             //Create a temporary option
        ///             Dropdown.OptionData temp = new Dropdown.OptionData();
        ///             //Make the option the data from the TextField
        ///             temp.text = m_MyString;
        ///
        ///             //Update the messages list with the TextField data
        ///             m_Messages.Add(temp);
        ///
        ///             //Add the Textfield data to the Dropdown
        ///             m_Dropdown.options.Insert(m_Index, temp);
        ///         }
        ///
        ///         //Press the "Remove" button to delete the selected option
        ///         if (GUI.Button(new Rect(110, 0, 100, 40), "Remove"))
        ///         {
        ///             //Remove the current selected item from the Dropdown from the messages List
        ///             m_Messages.RemoveAt(m_Dropdown.value);
        ///             //Remove the current selection from the Dropdown
        ///             m_Dropdown.options.RemoveAt(m_Dropdown.value);
        ///         }
        ///     }
        /// }
        /// </code>
        /// </example>
        public List<OptionData> options
        {
            get { return _optionList.options; }
            set { _optionList.options = value; }
        }
        /// <summary>
        /// A UnityEvent that is invoked when when a user has clicked one of the options in the dropdown list.
        /// </summary>
        /// <remarks>
        /// Use this to detect when a user selects one or more options in the Dropdown. Add a listener to perform an action when this UnityEvent detects a selection by the user. See https://unity3d.com/learn/tutorials/topics/scripting/delegates for more information on delegates.
        /// </remarks>
        /// <example>
        ///  <code>
        /// //Create a new Dropdown GameObject by going to the Hierarchy and clicking Create>UI>Dropdown. Attach this script to the Dropdown GameObject.
        /// //Set your own Text in the Inspector window
        ///
        /// using UnityEngine;
        /// using UnityEngine.UI;
        ///
        /// public class Example : MonoBehaviour
        /// {
        ///     Dropdown m_Dropdown;
        ///     public Text m_Text;
        ///
        ///     void Start()
        ///     {
        ///         //Fetch the Dropdown GameObject
        ///         m_Dropdown = GetComponent<Dropdown>();
        ///         //Add listener for when the value of the Dropdown changes, to take action
        ///         m_Dropdown.onValueChanged.AddListener(delegate {
        ///                 DropdownValueChanged(m_Dropdown);
        ///             });
        ///
        ///         //Initialise the Text to say the first value of the Dropdown
        ///         m_Text.text = "First Value : " + m_Dropdown.value;
        ///     }
        ///
        ///     //Ouput the new value of the Dropdown into Text
        ///     void DropdownValueChanged(Dropdown change)
        ///     {
        ///         m_Text.text =  "New Value : " + change.value;
        ///     }
        /// }
        /// </code>
        /// </example>
        public DropdownMenuEvent onClickItem { get { return _onClickItem; } set { _onClickItem = value; } }
        /// <summary>
        /// The time interval at which a drop down will appear and disappear
        /// </summary>
        public float alphaFadeSpeed { get { return _alphaFadeSpeed; } set { _alphaFadeSpeed = value; } }
        // Template used to create the dropdown.
        [SerializeField]
        private RectTransform _template;
        [Space]
        [SerializeField]
        private Text _itemText;
        [SerializeField]
        private Image _itemImage;
        [Space]
        // Items that will be visible when the dropdown is shown.
        // We box this into its own class so we can use a Property Drawer for it.
        [SerializeField]
        private OptionDataList _optionList = new OptionDataList();
        [Space]
        // Notification triggered when the dropdown changes.
        [SerializeField]
        private DropdownMenuEvent _onClickItem = new DropdownMenuEvent();
        [SerializeField]
        private float _alphaFadeSpeed = 0.15f;
        private GameObject _dropdown;
        private GameObject _blocker;
        private List<DropdownMenuItem> _itemList = new List<DropdownMenuItem>();
        private TweenRunner<FloatTween> m_AlphaTweenRunner;
        private bool validTemplate = false;
        #endregion
        #region 嵌套类型
        protected internal class DropdownMenuItem : MonoBehaviour, IPointerEnterHandler, ICancelHandler
        {
            public virtual void OnPointerEnter(PointerEventData eventData)
            {
                EventSystem.current.SetSelectedGameObject(gameObject);
            }
            public virtual void OnCancel(BaseEventData eventData)
            {
                DropdownMenu dropdown = GetComponentInParent<DropdownMenu>();
                if (dropdown)
                    dropdown.Hide();
            }
            public Text text { get { return _text; } set { _text = value; } }
            public Image image { get { return _image; } set { _image = value; } }
            public Button button { get { return _button; } set { _button = value; } }
            public RectTransform rectTransform { get { return _rectTransform; } set { _rectTransform = value; } }
            [SerializeField]
            private Text _text;
            [SerializeField]
            private Image _image;
            [SerializeField]
            private Button _button;
            [SerializeField]
            private RectTransform _rectTransform;
        }
        //[Serializable]
        ///// <summary>
        ///// 用来存储下拉菜单单个选项的文字或者图片的类型
        ///// </summary>
        //public class OptionData
        //{
        //    public OptionData()
        //    {
        //    }
        //    public OptionData(string text)
        //    {
        //        this.text = text;
        //    }
        //    public OptionData(Sprite image)
        //    {
        //        this.image = image;
        //    }
        //    public OptionData(string text, Sprite image)
        //    {
        //        this.text = text;
        //        this.image = image;
        //    }
        //    public string text { get { return _text; } set { _text = value; } }
        //    public Sprite image { get { return _image; } set { _image = value; } }
        //    [SerializeField]
        //    private string _text;
        //    [SerializeField]
        //    private Sprite _image;
        //}
        //[Serializable]
        ///// <summary>
        ///// Class used internally to store the list of options for the dropdown list.
        ///// </summary>
        ///// <remarks>
        ///// The usage of this class is not exposed in the runtime API. It's only relevant for the PropertyDrawer drawing the list of options.
        ///// </remarks>
        //public class OptionDataList
        //{
        //    public OptionDataList()
        //    {
        //        options = new List<OptionData>();
        //    }
        //    /// <summary>
        //    /// The list of options for the dropdown list.
        //    /// </summary>
        //    public List<OptionData> options { get { return _optionList; } set { _optionList = value; } }
        //    [SerializeField]
        //    private List<OptionData> _optionList;
        //}
        [Serializable]
        /// <summary>
        /// UnityEvent callback for when a dropdown current option is changed.
        /// </summary>
        public class DropdownMenuEvent : UnityEvent<int> { }
        #endregion
    }
}