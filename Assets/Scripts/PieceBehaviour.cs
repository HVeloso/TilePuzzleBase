using UnityEngine;

public class PieceBehaviour : MonoBehaviour
{
    #region Parameters

    private Vector2 _positionOnTable;

    #endregion

    #region MonoBehaviour Functions

    #endregion

    #region

    public void SetPositionOnTable(Vector2 positionOnTable)
    {
        _positionOnTable = positionOnTable;
    }

    public Vector2 GetPositionOnTable()
    {
        return _positionOnTable;
    }

    #endregion
}
