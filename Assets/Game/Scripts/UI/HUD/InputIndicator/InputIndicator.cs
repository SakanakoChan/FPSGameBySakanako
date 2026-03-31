using Rewired;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    private List<InputHint> inputHints;



    private void Awake()
    {
        player = ReInput.players.GetPlayer(0);

        inputHints = GetComponentsInChildren<InputHint>(true).ToList();

        ConvertIconListToDictonary(PS5IconList, psButtonIconDictionary);
        ConvertIconListToDictonary(XBOXIconList, xboxButtonIconDictionary);
        ConvertIconListToDictonary(mouseIconList, mouseButtonIconDictionary);
        ConvertIconListToDictonary(keyboardIconList, keyboardButtonIconDictionary);
    }

    private void Start()
    {
        InputManager.instance.OnInputDeviceChanged += UpdateAllInputHints;
        InputManager.instance.OnControllerLayoutChanged += UpdateAllInputHints;

        UpdateAllInputHints(InputManager.instance.currentInputDevice);
    }

    private void OnDestroy()
    {
        InputManager.instance.OnInputDeviceChanged -= UpdateAllInputHints;
        InputManager.instance.OnControllerLayoutChanged -= UpdateAllInputHints;
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
            //Debug.Log(map.elementIdentifierName);
            return map.elementIdentifierName;
        }

        return "Unbound";
    }

    private ActionElementMap FindActionElementMap(string _actionName)
    {
        ActionElementMap map;
        if (InputManager.instance.currentInputDevice == InputDevice.Controller)
        {
            map = player.controllers.maps.GetFirstElementMapWithAction(InputManager.instance.currentActiveJoystick, _actionName, false);
        }
        else
        {
            map = player.controllers.maps.GetFirstElementMapWithAction(ControllerType.Mouse, _actionName, false);
            if (map == null)
            {
                map = player.controllers.maps.GetFirstElementMapWithAction(ControllerType.Keyboard, _actionName, false);
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

    private void UpdateAllInputHints(InputDevice _inputDevice)
    {
        foreach (var inputHint in inputHints)
        {
            inputHint?.UpdateInputHint();
        }
    }

    private void UpdateAllInputHints(ControllerLayout _controllerLayout)
    {
        foreach (var inputHint in inputHints)
        {
            inputHint?.UpdateInputHint();
        }
    }
}
