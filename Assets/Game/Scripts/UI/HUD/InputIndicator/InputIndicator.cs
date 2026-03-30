using Rewired;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InputIndicator : MonoBehaviour
{
    private Player player;

    [Header("PS5 controller button icons")]
    [SerializeField] private List<InputIconEntry> PS5IconList;

    [Header("XBOX controller button icons")]
    [SerializeField] private List<InputIconEntry> XBOXIconList;

    [Header("Mouse icons")]
    [SerializeField] private List<InputIconEntry> mouseIconList;

    [Header("Keyboard icons")]
    [SerializeField] private List<InputIconEntry> keyboardIconList;

    private Dictionary<string, Sprite> psButtonIconDictionary = new Dictionary<string, Sprite>();
    private Dictionary<string, Sprite> xboxButtonIconDictionary = new Dictionary<string, Sprite>();
    private Dictionary<string, Sprite> mouseButtonIconDictionary = new Dictionary<string, Sprite>();
    private Dictionary<string, Sprite> keyboardButtonIconDictionary = new Dictionary<string, Sprite>();



    private void Start()
    {
        player = ReInput.players.GetPlayer(0);

        ConvertIconListToDictonary(PS5IconList, psButtonIconDictionary);
        ConvertIconListToDictonary(XBOXIconList, xboxButtonIconDictionary);
        ConvertIconListToDictonary(mouseIconList, mouseButtonIconDictionary);
        ConvertIconListToDictonary(keyboardIconList, keyboardButtonIconDictionary);

        Debug.Log($"mouse icon count: {mouseButtonIconDictionary.Count}, keyboard icon count: {keyboardButtonIconDictionary.Count}, ps icon count: {psButtonIconDictionary.Count}, xbox icon count: {xboxButtonIconDictionary.Count}");
    }


    public Sprite GetSpriteAccordingToAction(string _actionName)
    {
        string elementName = FindElementName(_actionName);
        return FindSpriteAccordingToElementName(elementName);
    }

    private Sprite FindSpriteAccordingToElementName(string _elementName)
    {
        if (InputManager.instance.currentInputDevice == InputDevice.Controller)
        {
            if (psButtonIconDictionary.ContainsKey(_elementName))
            {
                return psButtonIconDictionary[_elementName];
            }
            else if (xboxButtonIconDictionary.ContainsKey(_elementName))
            {
                return xboxButtonIconDictionary[_elementName];
            }

        }
        else
        {
            if (mouseButtonIconDictionary.ContainsKey(_elementName))
            {
                return mouseButtonIconDictionary[_elementName];
            }
            else if (keyboardButtonIconDictionary.ContainsKey(_elementName))
            {
                return keyboardButtonIconDictionary[_elementName];
            }
        }

        return null;
    }

    private string FindElementName(string _actionName)
    {
        ActionElementMap map = null;
        map = FindActionElementMap(_actionName);

        if (map != null)
        {
            return map.elementIdentifierName;
        }

        return "Unbound";

        //List<ActionElementMap> results = null;
        //var map = player.controllers.maps.GetElementMapsWithAction(_actionName, true, results);

        //if (map != null)
        //{
        //    return map.elementIdentifierName;
        //}

        //return "Unbound";
    }

    private ActionElementMap FindActionElementMap(string _actionName)
    {
        ActionElementMap map;
        if (InputManager.instance.currentInputDevice == InputDevice.Controller)
        {
            map = player.controllers.maps.GetFirstElementMapWithAction(ControllerType.Joystick, _actionName, true);
        }
        else
        {
            map = player.controllers.maps.GetFirstElementMapWithAction(ControllerType.Mouse, _actionName, true);
            if (map == null)
            {
                map = player.controllers.maps.GetFirstElementMapWithAction(ControllerType.Keyboard, _actionName, true);
            }
        }

        return map;
    }



    private void ConvertIconListToDictonary(List<InputIconEntry> _iconList, Dictionary<string, Sprite> _iconDictionary)
    {
        foreach (var entry in _iconList)
        {
            if (!_iconDictionary.ContainsKey(entry.name))
            {
                _iconDictionary.Add(entry.name, entry.icon);
            }
        }
    }
}
