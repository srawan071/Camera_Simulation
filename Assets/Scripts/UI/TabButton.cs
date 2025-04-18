using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class TabButton : MonoBehaviour
{
    //[SerializeField] private TabGroup _tabGroup;
    [SerializeField] private Color _selectedColor ;
    [SerializeField] private Color _normalColor = Color.white;

    private Button _button;
    private Image _buttonImage;

    private void Awake()
    {
        _button = GetComponent<Button>();
        _buttonImage = GetComponent<Image>(); // Get the Image component for direct color control
    }

    private void Start()
    {/*
        Debug.Log("From Tab btn");
        if (_tabGroup != null)
        {
            _tabGroup.Register(this);
            _button.onClick.AddListener(() => _tabGroup.OnTabSelected(this));
        }
        else
        {
            Debug.LogWarning("TabGroup reference not set in TabButton", this);
        }*/
    }

    public void SetOnClickListiner(TabGroup tabGroup)
    {
        _button.onClick.AddListener(() => tabGroup.OnTabSelected(this));
    }

    public void SetSelected(bool isSelected)
    {
        if (_buttonImage != null)
        {
            // Directly set the image color instead of using button colors
            _buttonImage.color = isSelected ? _selectedColor : _normalColor;
        }

        // Optional: Also modify the button's color block if you want hover/pressed states
        var colors = _button.colors;
        colors.normalColor = isSelected ? _selectedColor : _normalColor;
        colors.highlightedColor = isSelected ? _selectedColor * 1.2f : _normalColor * 1.2f;
        _button.colors = colors;
    }
}