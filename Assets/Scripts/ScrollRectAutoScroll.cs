#region

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

#endregion

[ RequireComponent( typeof( ScrollRect ) ) ]
public class ScrollRectAutoScroll : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
	private readonly List<Selectable> m_Selectables = new();
	private ScrollRect m_ScrollRect;
	private bool mouseOver;

	private Vector2 m_NextScrollPosition = Vector2.up;

	public void OnEnable()
	{
		if( m_ScrollRect )
			m_ScrollRect.content.GetComponentsInChildren( m_Selectables );
	}

	private void Awake()
	{
		m_ScrollRect = GetComponent<ScrollRect>();
	}

	private void Start()
	{
		if( m_ScrollRect )
			m_ScrollRect.content.GetComponentsInChildren( m_Selectables );
		ScrollToSelected( true );
	}

	private void ScrollToSelected( bool quickScroll )
	{
		int selectedIndex = -1;
		Selectable selectedElement = EventSystem.current.currentSelectedGameObject
			                             ? EventSystem.current.currentSelectedGameObject.GetComponent<Selectable>()
			                             : null;

		if( selectedElement )
			selectedIndex = m_Selectables.IndexOf( selectedElement );

		if( selectedIndex <= -1 )
			return;

		if( quickScroll )
		{
			m_ScrollRect.normalizedPosition =
				new Vector2( 0, 1 - selectedIndex / ( ( float )m_Selectables.Count - 1 ) );
			m_NextScrollPosition = m_ScrollRect.normalizedPosition;
		}
		else
		{
			m_NextScrollPosition = new Vector2( 0, 1 - selectedIndex / ( ( float )m_Selectables.Count - 1 ) );
		}
	}

	public void OnPointerEnter( PointerEventData eventData )
	{
		mouseOver = true;
	}

	public void OnPointerExit( PointerEventData eventData )
	{
		mouseOver = false;
		ScrollToSelected( false );
	}
}