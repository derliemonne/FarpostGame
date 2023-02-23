using System.Collections;
using System.Collections.Generic;
using System.Security.Authentication.ExtendedProtection;
using TMPro;
using UnityEngine;

public class GuiRecord : MonoBehaviour
{
    public Record Record
    {
        get => _record;
        set
        {
            _record = value;
            _name.text = _record.Name;
            _maxAltitude.text = _record.MaxAltitude.ToString();
            _place.text = _record.Place.ToString();
        }
    }

    [SerializeField] private TMP_Text _name;
    [SerializeField] private TMP_Text _maxAltitude;
    [SerializeField] private TMP_Text _place;

    private Record _record;
}
