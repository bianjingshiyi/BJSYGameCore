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
    [AddComponentMenu("UI/Multi Select Dropdown", 36)]
    [RequireComponent(typeof(RectTransform))]
    /// <summary>
    /// 可以选中多个选项的下拉菜单
    /// </summary>
    public class MultiSelectDropdown : Selectable, IPointerClickHandler, ISubmitHandler, ICancelHandler
    {
        #region 公有方法
        /// <summary>
        /// Set index number of the current selection in the Dropdown without invoking onValueChanged callback.
        /// </summary>
        /// <param name="input"> The new index for the current selection. </param>
        public void SetValueWithoutNotify(bool[] input)
        {
            Set(input, false);
        }
        /// <summary>
        /// Refreshes the text and image (if available) of the currently selected option.
        /// </summary>
        /// <remarks>
        /// If you have modified the list of options, you should call this method afterwards to ensure that the visual state of the dropdown corresponds to the updated options.
        /// </remarks>
        public void RefreshShownValue()
        {
            OptionData data = _noOption;

            if (options.Count > 0)
            {
                for (int i = 0; i < _value.Length; i++)
                {
                    if (_value[i])
                    {
                        if (data == _noOption)
                        {
                            data = options[i];
                            continue;
                        }
                        else
                        {
                            _mixOption.text = data.text + "," + options[i].text;
                            data = _mixOption;
                        }
                    }
                }
            }

            if (_captionText)
            {
                if (data != null && data.text != null)
                    _captionText.text = data.text;
                else
                    _captionText.text = "";
            }

            if (_captionImage)
            {
                if (data != null)
                    _captionImage.sprite = data.image;
                else
                    _captionImage.sprite = null;
                _captionImage.enabled = (_captionImage.sprite != null);
            }
        }

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
            RefreshShownValue();
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
            RefreshShownValue();
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
            RefreshShownValue();
        }

        /// <summary>
        /// Clear the list of options in the Dropdown.
        /// </summary>
        public void ClearOptions()
        {
            options.Clear();
            _value = new bool[0];
            RefreshShownValue();
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
            MultiSelectDropdownItem itemTemplate = _dropdown.GetComponentInChildren<MultiSelectDropdownItem>();

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

            Toggle prev = null;
            for (int i = 0; i < options.Count; ++i)
            {
                OptionData data = options[i];
                bool isOn = i < value.Length ? value[i] : false;
                MultiSelectDropdownItem item = AddItem(data, isOn, itemTemplate, _itemList);
                if (item == null)
                    continue;

                // Automatically set up a toggle state change listener
                item.toggle.isOn = isOn;
                item.toggle.onValueChanged.AddListener(x => OnSelectItem(item.toggle));

                // Select current option
                if (item.toggle.isOn)
                    item.toggle.Select();

                // Automatically set up explicit navigation
                if (prev != null)
                {
                    Navigation prevNav = prev.navigation;
                    Navigation toggleNav = item.toggle.navigation;
                    prevNav.mode = Navigation.Mode.Explicit;
                    toggleNav.mode = Navigation.Mode.Explicit;

                    prevNav.selectOnDown = item.toggle;
                    prevNav.selectOnRight = item.toggle;
                    toggleNav.selectOnLeft = prev;
                    toggleNav.selectOnUp = prev;

                    prev.navigation = prevNav;
                    item.toggle.navigation = toggleNav;
                }
                prev = item.toggle;
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
        protected MultiSelectDropdown()
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

            if (_captionImage)
                _captionImage.enabled = (_captionImage.sprite != null);

            if (_template)
                _template.gameObject.SetActive(false);
        }

        protected override void Start()
        {
            m_AlphaTweenRunner = new TweenRunner<FloatTween>();
            m_AlphaTweenRunner.Init(this);
            base.Start();

            RefreshShownValue();
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();

            if (!IsActive())
                return;

            RefreshShownValue();
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
        void Set(bool[] value, bool sendCallback = true, bool forceUpdate = false)
        {
            if (Application.isPlaying && (CompareValue(value, _value) || options.Count == 0) && !forceUpdate)
                return;

            _value = new bool[options.Count];
            for (int i = 0; i < _value.Length; i++)
            {
                if (i < value.Length)
                    _value[i] = value[i];
                else
                    _value[i] = default;
            }
            RefreshShownValue();

            if (sendCallback)
            {
                // Notify all listeners
                UISystemProfilerApi.AddMarker("Dropdown.value", this);
                _onValueChanged.Invoke(_value);
            }
        }
        bool CompareValue(bool[] valueA, bool[] valueB)
        {
            if (valueA == null || valueB == null)
                return false;
            if (valueA.Length != valueB.Length)
                return false;
            for (int i = 0; i < valueA.Length; i++)
            {
                if (valueA[i] != valueB[i])
                    return false;
            }
            return true;
        }
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
            Toggle itemToggle = _template.GetComponentInChildren<Toggle>();

            validTemplate = true;
            if (!itemToggle || itemToggle.transform == template)
            {
                validTemplate = false;
                Debug.LogError("The dropdown template is not valid. The template must have a child GameObject with a Toggle component serving as the item.", template);
            }
            else if (!(itemToggle.transform.parent is RectTransform))
            {
                validTemplate = false;
                Debug.LogError("The dropdown template is not valid. The child GameObject with a Toggle component (the item) must have a RectTransform on its parent.", template);
            }
            else if (itemText != null && !itemText.transform.IsChildOf(itemToggle.transform))
            {
                validTemplate = false;
                Debug.LogError("The dropdown template is not valid. The Item Text must be on the item GameObject or children of it.", template);
            }
            else if (itemImage != null && !itemImage.transform.IsChildOf(itemToggle.transform))
            {
                validTemplate = false;
                Debug.LogError("The dropdown template is not valid. The Item Image must be on the item GameObject or children of it.", template);
            }

            if (!validTemplate)
            {
                templateGo.SetActive(false);
                return;
            }

            MultiSelectDropdownItem item = itemToggle.gameObject.AddComponent<MultiSelectDropdownItem>();
            item.text = _itemText;
            item.image = _itemImage;
            item.toggle = itemToggle;
            item.rectTransform = (RectTransform)itemToggle.transform;

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
        protected virtual MultiSelectDropdownItem CreateItem(MultiSelectDropdownItem itemTemplate)
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
        protected virtual void DestroyItem(MultiSelectDropdownItem item)
        { }

        // Add a new drop-down list item with the specified values.
        private MultiSelectDropdownItem AddItem(OptionData data, bool selected, MultiSelectDropdownItem itemTemplate, List<MultiSelectDropdownItem> items)
        {
            // Add a new item to the dropdown.
            MultiSelectDropdownItem item = CreateItem(itemTemplate);
            item.rectTransform.SetParent(itemTemplate.rectTransform.parent, false);

            item.gameObject.SetActive(true);
            item.gameObject.name = "Item " + items.Count + (data.text != null ? ": " + data.text : "");

            if (item.toggle != null)
            {
                item.toggle.isOn = false;
            }

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
        private void OnSelectItem(Toggle toggle)
        {
            int selectedIndex = -1;
            Transform tr = toggle.transform;
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

            if (selectedIndex >= value.Length)
            {
                bool[] newValue = new bool[selectedIndex + 1];
                value.CopyTo(newValue, 0);
                value = newValue;
            }
            value[selectedIndex] = !value[selectedIndex];
            Set(value, forceUpdate: true);
            toggle.SetIsOnWithoutNotify(value[selectedIndex]);
        }
        #endregion
        #region 属性字段
        /// <summary>
        /// The Rect Transform of the template for the dropdown list.
        /// </summary>
        public RectTransform template { get { return _template; } set { _template = value; RefreshShownValue(); } }
        /// <summary>
        /// The Text component to hold the text of the currently selected option.
        /// </summary>
        public Text captionText { get { return _captionText; } set { _captionText = value; RefreshShownValue(); } }
        /// <summary>
        /// The Image component to hold the image of the currently selected option.
        /// </summary>
        public Image captionImage { get { return _captionImage; } set { _captionImage = value; RefreshShownValue(); } }
        /// <summary>
        /// The Text component to hold the text of the item.
        /// </summary>
        public Text itemText { get { return _itemText; } set { _itemText = value; RefreshShownValue(); } }
        /// <summary>
        /// The Image component to hold the image of the item
        /// </summary>
        public Image itemImage { get { return _itemImage; } set { _itemImage = value; RefreshShownValue(); } }
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
            set { _optionList.options = value; RefreshShownValue(); }
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
        public MultiSelectDropdownEvent onValueChanged { get { return _onValueChanged; } set { _onValueChanged = value; } }
        /// <summary>
        /// The time interval at which a drop down will appear and disappear
        /// </summary>
        public float alphaFadeSpeed { get { return _alphaFadeSpeed; } set { _alphaFadeSpeed = value; } }
        /// <summary>
        /// The Value is the index number of the current selection in the Dropdown. 0 is the first option in the Dropdown, 1 is the second, and so on.
        /// </summary>
        /// <example>
        /// <code>
        /// //Create a new Dropdown GameObject by going to the Hierarchy and clicking __Create__>__UI__>__Dropdown__. Attach this script to the Dropdown GameObject.
        /// //Set your own Text in the Inspector window
        ///
        /// using UnityEngine;
        /// using UnityEngine.UI;
        ///
        /// public class Example : MonoBehaviour
        /// {
        ///     //Attach this script to a Dropdown GameObject
        ///     Dropdown m_Dropdown;
        ///     //This is the string that stores the current selection m_Text of the Dropdown
        ///     string m_Message;
        ///     //This Text outputs the current selection to the screen
        ///     public Text m_Text;
        ///     //This is the index value of the Dropdown
        ///     int m_DropdownValue;
        ///
        ///     void Start()
        ///     {
        ///         //Fetch the DropDown component from the GameObject
        ///         m_Dropdown = GetComponent<Dropdown>();
        ///         //Output the first Dropdown index value
        ///         Debug.Log("Starting Dropdown Value : " + m_Dropdown.value);
        ///     }
        ///
        ///     void Update()
        ///     {
        ///         //Keep the current index of the Dropdown in a variable
        ///         m_DropdownValue = m_Dropdown.value;
        ///         //Change the message to say the name of the current Dropdown selection using the value
        ///         m_Message = m_Dropdown.options[m_DropdownValue].text;
        ///         //Change the onscreen Text to reflect the current Dropdown selection
        ///         m_Text.text = m_Message;
        ///     }
        /// }
        /// </code>
        /// </example>
        public bool[] value
        {
            get { return _value; }
            set { Set(value); }
        }
        // Template used to create the dropdown.
        [SerializeField]
        private RectTransform _template;
        // Text to be used as a caption for the current value. It's not required, but it's kept here for convenience.
        [SerializeField]
        private Text _captionText;
        [SerializeField]
        private Image _captionImage;
        [Space]
        [SerializeField]
        private Text _itemText;
        [SerializeField]
        private Image _itemImage;
        [Space]
        [SerializeField]
        private bool[] _value;
        [Space]
        // Items that will be visible when the dropdown is shown.
        // We box this into its own class so we can use a Property Drawer for it.
        [SerializeField]
        private OptionDataList _optionList = new OptionDataList();
        [SerializeField]
        private OptionData _noOption = new OptionData() { text = string.Empty };
        [SerializeField]
        private OptionData _mixOption = new OptionData() { text = "..." };
        [Space]
        // Notification triggered when the dropdown changes.
        [SerializeField]
        private MultiSelectDropdownEvent _onValueChanged = new MultiSelectDropdownEvent();
        [SerializeField]
        private float _alphaFadeSpeed = 0.15f;
        private GameObject _dropdown;
        private GameObject _blocker;
        private List<MultiSelectDropdownItem> _itemList = new List<MultiSelectDropdownItem>();
        private TweenRunner<FloatTween> m_AlphaTweenRunner;
        private bool validTemplate = false;
        #endregion
        #region 嵌套类型
        protected internal class MultiSelectDropdownItem : MonoBehaviour, IPointerEnterHandler, ICancelHandler
        {
            public virtual void OnPointerEnter(PointerEventData eventData)
            {
                EventSystem.current.SetSelectedGameObject(gameObject);
            }
            public virtual void OnCancel(BaseEventData eventData)
            {
                MultiSelectDropdown dropdown = GetComponentInParent<MultiSelectDropdown>();
                if (dropdown)
                    dropdown.Hide();
            }
            public Text text { get { return _text; } set { _text = value; } }
            public Image image { get { return _image; } set { _image = value; } }
            public RectTransform rectTransform { get { return _rectTransform; } set { _rectTransform = value; } }
            public Toggle toggle { get { return _toggle; } set { _toggle = value; } }
            [SerializeField]
            private Text _text;
            [SerializeField]
            private Image _image;
            [SerializeField]
            private RectTransform _rectTransform;
            [SerializeField]
            private Toggle _toggle;
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
        public class MultiSelectDropdownEvent : UnityEvent<bool[]> { }
        // Base interface for tweeners,
        // using an interface instead of
        // an abstract class as we want the
        // tweens to be structs.
        internal interface ITweenValue
        {
            void TweenValue(float floatPercentage);
            bool ignoreTimeScale { get; }
            float duration { get; }
            bool ValidTarget();
        }
        // Tween runner, executes the given tween.
        // The coroutine will live within the given
        // behaviour container.
        internal class TweenRunner<T> where T : struct, ITweenValue
        {
            protected MonoBehaviour m_CoroutineContainer;
            protected IEnumerator m_Tween;

            // utility function for starting the tween
            private static IEnumerator Start(T tweenInfo)
            {
                if (!tweenInfo.ValidTarget())
                    yield break;

                var elapsedTime = 0.0f;
                while (elapsedTime < tweenInfo.duration)
                {
                    elapsedTime += tweenInfo.ignoreTimeScale ? Time.unscaledDeltaTime : Time.deltaTime;
                    var percentage = Mathf.Clamp01(elapsedTime / tweenInfo.duration);
                    tweenInfo.TweenValue(percentage);
                    yield return null;
                }
                tweenInfo.TweenValue(1.0f);
            }

            public void Init(MonoBehaviour coroutineContainer)
            {
                m_CoroutineContainer = coroutineContainer;
            }

            public void StartTween(T info)
            {
                if (m_CoroutineContainer == null)
                {
                    Debug.LogWarning("Coroutine container not configured... did you forget to call Init?");
                    return;
                }

                StopTween();

                if (!m_CoroutineContainer.gameObject.activeInHierarchy)
                {
                    info.TweenValue(1.0f);
                    return;
                }

                m_Tween = Start(info);
                m_CoroutineContainer.StartCoroutine(m_Tween);
            }

            public void StopTween()
            {
                if (m_Tween != null)
                {
                    m_CoroutineContainer.StopCoroutine(m_Tween);
                    m_Tween = null;
                }
            }
        }
        // Float tween class, receives the
        // TweenValue callback and then sets
        // the value on the target.
        internal struct FloatTween : ITweenValue
        {
            public class FloatTweenCallback : UnityEvent<float> { }

            private FloatTweenCallback m_Target;
            private float m_StartValue;
            private float m_TargetValue;

            private float m_Duration;
            private bool m_IgnoreTimeScale;

            public float startValue
            {
                get { return m_StartValue; }
                set { m_StartValue = value; }
            }

            public float targetValue
            {
                get { return m_TargetValue; }
                set { m_TargetValue = value; }
            }

            public float duration
            {
                get { return m_Duration; }
                set { m_Duration = value; }
            }

            public bool ignoreTimeScale
            {
                get { return m_IgnoreTimeScale; }
                set { m_IgnoreTimeScale = value; }
            }

            public void TweenValue(float floatPercentage)
            {
                if (!ValidTarget())
                    return;

                var newValue = Mathf.Lerp(m_StartValue, m_TargetValue, floatPercentage);
                m_Target.Invoke(newValue);
            }

            public void AddOnChangedCallback(UnityAction<float> callback)
            {
                if (m_Target == null)
                    m_Target = new FloatTweenCallback();

                m_Target.AddListener(callback);
            }

            public bool GetIgnoreTimescale()
            {
                return m_IgnoreTimeScale;
            }

            public float GetDuration()
            {
                return m_Duration;
            }

            public bool ValidTarget()
            {
                return m_Target != null;
            }
        }
        internal static class ListPool<T>
        {
            // Object pool to avoid allocations.
            private static readonly ObjectPool<List<T>> s_ListPool = new ObjectPool<List<T>>(null, Clear);
            static void Clear(List<T> l) { l.Clear(); }

            public static List<T> Get()
            {
                return s_ListPool.Get();
            }

            public static void Release(List<T> toRelease)
            {
                s_ListPool.Release(toRelease);
            }
        }
        internal class ObjectPool<T> where T : new()
        {
            private readonly Stack<T> m_Stack = new Stack<T>();
            private readonly UnityAction<T> m_ActionOnGet;
            private readonly UnityAction<T> m_ActionOnRelease;

            public int countAll { get; private set; }
            public int countActive { get { return countAll - countInactive; } }
            public int countInactive { get { return m_Stack.Count; } }

            public ObjectPool(UnityAction<T> actionOnGet, UnityAction<T> actionOnRelease)
            {
                m_ActionOnGet = actionOnGet;
                m_ActionOnRelease = actionOnRelease;
            }

            public T Get()
            {
                T element;
                if (m_Stack.Count == 0)
                {
                    element = new T();
                    countAll++;
                }
                else
                {
                    element = m_Stack.Pop();
                }
                if (m_ActionOnGet != null)
                    m_ActionOnGet(element);
                return element;
            }

            public void Release(T element)
            {
                if (m_Stack.Count > 0 && ReferenceEquals(m_Stack.Peek(), element))
                    Debug.LogError("Internal error. Trying to destroy object that is already released to pool.");
                if (m_ActionOnRelease != null)
                    m_ActionOnRelease(element);
                m_Stack.Push(element);
            }
        }
        #endregion
    }
}