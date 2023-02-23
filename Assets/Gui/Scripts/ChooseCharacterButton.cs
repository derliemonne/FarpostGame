using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ChooseCharacterButton : MonoBehaviour
{
    public CharacterType CharacterType => _characterType;

    public Button Button
    {
        get
        {
            if (_button == null) _button = GetComponent<Button>();
            return _button;
        }
    }
    
    [SerializeField] private CharacterType _characterType;

    private Button _button;
}
