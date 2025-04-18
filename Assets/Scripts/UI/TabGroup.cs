using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TabGroup : MonoBehaviour
{

    [SerializeField]
    private List<GameObject> _tabPannel = new();
    [SerializeField]
    private List<TabButton> _tabButtons = new();
    private TabButton _selectedTab;

    private IEnumerator Start()
    {
        yield return null;
        SetButtonClickListiner();
        if (_tabButtons.Count > 0)
            OnTabSelected(_tabButtons[0]);
    }

    private void SetButtonClickListiner()
    {
        foreach (TabButton button in _tabButtons)
        {
            button.SetOnClickListiner(this);
        }
    }
    public void Register(TabButton button)
    {
        if (!_tabButtons.Contains(button))
            _tabButtons.Add(button);
    }

    public void OnTabSelected(TabButton button)
    {
      
        _selectedTab = button;

        for (int i = 0; i < _tabButtons.Count; i++)
        {
            bool isActive = (_tabButtons[i] == _selectedTab);
            _tabPannel[i].SetActive(isActive);
            _tabButtons[i].SetSelected(isActive);
        }
    }
}
