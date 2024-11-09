using System.Collections.Generic;
using UnityEngine;

public class TableController : MonoBehaviour
{
	#region Parameters

	[Header("Pieces Parameters")]
	[SerializeField] private GameObject _piecePrefab;
	[SerializeField] private GameObject _pieceNumberPrefab;
	[SerializeField] private Transform _UITransform;
	private List<PieceBehaviour> _piecesOnTable;

	[Header("Table Parameters")]
	[SerializeField] [Min(0)] private float _cellSize;
	[SerializeField] [Min(0)] private float _distanceTolerance;
	[SerializeField] private bool _autoCalculateTolerance;
	private bool[,] _occupiedPositions;

	// Pieces parameters
	private bool _isDragging;
	private PieceBehaviour _currentPiece;

	// Pieces movement
	private Vector2 _pieceStarterPosition;
	private Vector2 _pieceTargetPosition;
	private Vector2 _dragDirection;
	private float _pointerDistance;

	#endregion

	#region MonoBehaviour Functions

	private void Awake()
	{
		InitializateVariables();
		InitializeTable();
		InitializePieces();
		RandomizePieces();
	}

	private void Update()
	{
		PointerDown();
		PointerUp();
	}

	private void FixedUpdate()
	{
		if (!_isDragging) return;

		Vector3 newPosition = _currentPiece.transform.position;
		Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

		if (_dragDirection.y != 0f)
		{
			newPosition.y = mousePos.y - _pointerDistance;

			float min = _pieceStarterPosition.y;
			float max = _pieceTargetPosition.y;

			if (min > max)
			{
				(min, max) = (max, min);
			}

			newPosition.y = Mathf.Clamp(newPosition.y, min, max);
		}
		else if (_dragDirection.x != 0f)
		{
			newPosition.x = mousePos.x - _pointerDistance;

			float min = _pieceStarterPosition.x;
			float max = _pieceTargetPosition.x;

			if (min > max)
			{
				(min, max) = (max, min);
			}

			newPosition.x = Mathf.Clamp(newPosition.x, min, max);
		}

		_currentPiece.transform.position = newPosition;
	}

	#endregion

	#region Initialize Functions

	private void InitializateVariables()
	{
		_isDragging = false;
		_occupiedPositions = new bool[3, 3];
		_piecesOnTable = new List<PieceBehaviour>();
	}

	private void InitializeTable()
	{
		int numberOfColumns = _occupiedPositions.GetLength(0);
		int numberOfRows = _occupiedPositions.GetLength(1);

		for (int column = 0; column < numberOfColumns; column++)
		{
			for (int row = 0; row < numberOfRows; row++)
			{
				_occupiedPositions[column, row] = true;
			}
		}

		_occupiedPositions[numberOfColumns - 1, numberOfRows - 1] = false;
	}

	private void InitializePieces()
	{
		transform.position = new Vector3(-_cellSize, _cellSize, 0f);

		GameObject newPiece;
		PieceBehaviour pieceBehaviour;

		for (int row = 0; row < _occupiedPositions.GetLength(1); row++)
		{
			for (int column = 0; column < _occupiedPositions.GetLength(0); column++)
			{
				if (_piecesOnTable.Count >= _occupiedPositions.Length - 1) break;

				newPiece = Instantiate(_piecePrefab);
				pieceBehaviour = newPiece.GetComponent<PieceBehaviour>();
				pieceBehaviour.SetPositionOnTable(new(column, row));
				newPiece.name = $"Piece [{column}|{row}]";
				newPiece.transform.parent = transform;
				newPiece.transform.localPosition = new Vector3(column * _cellSize, -row * _cellSize, 0f);

				GameObject newPieceNumber = Instantiate(_pieceNumberPrefab);
				newPieceNumber.name = newPiece.name;
				newPieceNumber.transform.SetParent(_UITransform);
				newPieceNumber.GetComponent<PieceNumber>().SetTextParameters(newPiece.transform, $"{_piecesOnTable.Count + 1}");


				_piecesOnTable.Add(pieceBehaviour);
			}
		}
	}

	public void RandomizePieces()
	{
		List<PieceBehaviour> _pieces = new(_piecesOnTable);

		for (int row = 0; row < _occupiedPositions.GetLength(1); row++)
		{
			for (int column = 0; column < _occupiedPositions.GetLength(0); column++)
			{
				if (_pieces.Count <= 0) break;

				int index = Random.Range(0, _pieces.Count);
				_pieces[index].SetPositionOnTable(new(column, row));
				_pieces[index].transform.localPosition = new Vector3(column * _cellSize, -row * _cellSize, 0f);

				_pieces.RemoveAt(index);
			}
		}
	}

	#endregion

	#region Pointer Functions

	private void PointerDown()
	{
		if (Input.GetMouseButtonDown(0))
		{
			Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

			RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

			if (hit.collider == null) return;

			if (hit.collider.TryGetComponent(out PieceBehaviour piece))
			{
				_dragDirection = GetDragDirection(piece);

				if (_dragDirection == Vector2.zero) return;

				_currentPiece = piece;
				_pieceStarterPosition = _currentPiece.transform.position;
				_pieceTargetPosition = _pieceStarterPosition + (_cellSize * _dragDirection);

				if (_dragDirection.x != 0f)
				{
					_pointerDistance = mousePos.x - _pieceStarterPosition.x;
				}
				else if (_dragDirection.y != 0f)
				{
					_pointerDistance = mousePos.y - _pieceStarterPosition.y;
				}

				_isDragging = true;
			}
		}
	}

	private void PointerUp()
	{
		if (!_isDragging) return;

		if (Input.GetMouseButtonUp(0))
		{
			_isDragging = false;

			if (Vector2.Distance(_currentPiece.transform.position, _pieceTargetPosition) <= _distanceTolerance)
			{
				_currentPiece.transform.position = _pieceTargetPosition;

				Vector2 pieceIndex = _currentPiece.GetPositionOnTable();
				_occupiedPositions[(int)pieceIndex.x, (int)pieceIndex.y] = false;

				if (_dragDirection.y == 0f) pieceIndex += _dragDirection;
				else pieceIndex -= _dragDirection;

				_occupiedPositions[(int)pieceIndex.x, (int)pieceIndex.y] = true;

				_currentPiece.SetPositionOnTable(pieceIndex);
			}
			else
			{
				_currentPiece.transform.position = _pieceStarterPosition;
			}
		}
	}

	private Vector2 GetDragDirection(PieceBehaviour piece)
	{
		Vector2 direction = Vector2.zero;
		Vector2 piecePosition = piece.GetPositionOnTable();

		if (piecePosition.x > 0f)
		{
			if (!_occupiedPositions[(int)piecePosition.x - 1, (int)piecePosition.y])
			{
				direction = Vector2.left;
				return direction;
			}
		}

		if (piecePosition.x < _occupiedPositions.GetLength(0) - 1)
		{
			if (!_occupiedPositions[(int)piecePosition.x + 1, (int)piecePosition.y])
			{
				direction = Vector2.right;
				return direction;
			}
		}

		if (piecePosition.y > 0f)
		{
			if (!_occupiedPositions[(int)piecePosition.x, (int)piecePosition.y - 1])
			{
				direction = Vector2.up;
				return direction;
			}
		}

		if (piecePosition.y < _occupiedPositions.GetLength(1) - 1)
		{
			if (!_occupiedPositions[(int)piecePosition.x, (int)piecePosition.y + 1])
			{
				direction = Vector2.down;
				return direction;
			}
		}

		return direction;
	}

	#endregion

	#region Debug

	private void OnValidate()
	{
		if (_autoCalculateTolerance)
		{
			_autoCalculateTolerance = false;

			_distanceTolerance = _cellSize * 0.55f;
		}
	}

	#endregion
}