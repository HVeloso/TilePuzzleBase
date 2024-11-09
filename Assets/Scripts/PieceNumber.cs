using UnityEngine;
using TMPro;

[RequireComponent(typeof(TextMeshProUGUI))]
public class PieceNumber : MonoBehaviour
{
    #region Parameters

    private TextMeshProUGUI _numberText;
    private Transform _pieceTransform;

    #endregion

    #region MonoBehaviour Functions

    private void Awake()
    {
        _numberText = GetComponent<TextMeshProUGUI>();
    }

    private void Update()
    {
        transform.position = _pieceTransform.position;
    }

    #endregion

    #region Function

    public void SetTextParameters(Transform pieceTransform, string pieceNumber)
    {
        _numberText.text = pieceNumber;
        _pieceTransform = pieceTransform;
    }

    #endregion
}
