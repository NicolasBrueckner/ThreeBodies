#region

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

#endregion

[ RequireComponent( typeof( ScrollRect ) ) ]
public class ScrollRectAutoScroll : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
	private readonly List<Selectable> _selectables = new();
	private ScrollRect _scrollRect;
	private bool _mouseOver;

	private Vector2 _nextScrollPosition = Vector2.up;

	public void OnEnable()
	{
		if( _scrollRect )
			_scrollRect.content.GetComponentsInChildren( _selectables );
	}

	private void Awake()
	{
		_scrollRect = GetComponent<ScrollRect>();
	}

	private void Start()
	{
		if( _scrollRect )
			_scrollRect.content.GetComponentsInChildren( _selectables );
		ScrollToSelected( true );
	}

	private void ScrollToSelected( bool quickScroll )
	{
		int selectedIndex = -1;
		Selectable selectedElement = EventSystem.current.currentSelectedGameObject
			                             ? EventSystem.current.currentSelectedGameObject.GetComponent<Selectable>()
			                             : null;

		if( selectedElement )
			selectedIndex = _selectables.IndexOf( selectedElement );

		if( selectedIndex <= -1 )
			return;

		if( quickScroll )
		{
			_scrollRect.normalizedPosition =
				new Vector2( 0, 1 - selectedIndex / ( ( float )_selectables.Count - 1 ) );
			_nextScrollPosition = _scrollRect.normalizedPosition;
		}
		else
		{
			_nextScrollPosition = new Vector2( 0, 1 - selectedIndex / ( ( float )_selectables.Count - 1 ) );
		}
	}

	public void OnPointerEnter( PointerEventData eventData )
	{
		_mouseOver = true;
	}

	public void OnPointerExit( PointerEventData eventData )
	{
		_mouseOver = false;
		ScrollToSelected( false );
	}
}